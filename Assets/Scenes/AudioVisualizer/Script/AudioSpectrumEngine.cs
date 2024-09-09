using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;
using UnityEngine.XR;
using Unity.VisualScripting;
using UnityEngine.Audio;


public class AudioSpectrumEngine : MonoBehaviour
{
    // Hardcoded only
    private const uint SampleCount = 256;
    private const long BufferSizeBytes = 1024;
    private Transform[] _audioCubeArray;
    private FileStream _fileStream;
    private BinaryReader _reader;
    private float[] _spectrumBuffer;
    private float[] _spectrumBands;
    private float[] _bandBuffer;
    private float[] _bandBufferDecay;
    private Transform[] _bandBufferCubeArray;
    private float[] _fullSpectrumBuffer;
    private float[] _fullSpectrumBufferDecay;

    
    [Header("Components")]
    public Transform target;
    public Transform bandCube;
    public Transform cameraTransform;
    public Transform rotatePivot;
    public AudioSource audioSource;
    public string microphone;
    public AudioMixerGroup standard;

    [Header("SignalProcessing")] public bool useGNURadio = false;
    public EqualizerSettings equalizerSettings = new EqualizerSettings();
    public BandSettings bandSettings = new BandSettings();
    
    [Header("VisualSettings")]
    public bool regenerateSpectrum = false;
    public bool useFullBuffer = false;
    [Range(0, SampleCount / 2)]public int cutoffLow = 0;
    [Range(0, SampleCount / 2)]public int cutoffHigh = 0;
    public float amplitudeCeiling = 1.0f;
    public float flatAmplitudeMultiplier = 1.0f;
    public float bandBufferDecayRate = 0.005f;
    public float bandBandBufferDecayMultiplier = 1.2f;
    public float spacing = 1.0f;
    public float widthMultiplier = 1.0f;
    public Vector3 cubeSize = Vector3.one;

    [Header("BeatSettings")]
    private Queue<float> _history = new Queue<float>();
    public int historyLength;
    public int useBandsLow;
    public int useBandsHigh;
    public float beatTimeoutDuration;
    private bool _beatTimeout;
    private float _currentBeatTimeout;
    public int historyBeatTimeout;
    public float beatThreshold;
    public float estimatedBPM;
    public float bps;
    public long beat_count;
    public float duration;
    public bool beat;
    public bool flush;
    private Transform _beatCube;

    [Header("Other")]
    private float[] _amplitude_peaks;
    public float clampMultiplier = 1.0f;
    
    [Header("CameraSettings")]
    public bool rotate;
    public float speed;
    

    private void Awake()
    {
        Debug.LogWarning("FFT buffer reads ocasionally produce exceptions which are silently ignored");
        if (useGNURadio)
        {
            try
            {
                _fileStream = new FileStream(@"R:" + @"\" + "sink.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                _reader = new BinaryReader(_fileStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.LogError(e);
                Debug.LogError("Missing GNU Radio Sink File. RAMDsik is probably missconfigured");
                throw;
            }
        }
        
        _spectrumBuffer = new float[SampleCount];
        _spectrumBands = new float[8];
        _bandBuffer = new float[8];
        _bandBufferDecay = new float[8];
        _fullSpectrumBuffer = new float[SampleCount];
        _fullSpectrumBufferDecay = new float[SampleCount];
        _amplitude_peaks = new float[8];
        
    }

    // TODO Add better amplitude equlization (dealing with low volumes)

    // Start is called before the first frame update
    void Start()
    {
        if (!useGNURadio)
        {
            ReadMicrophone();
        }
        //RegenerateSpectrum();
        RegenerateBand();
        _beatCube = Instantiate(target, new Vector3(-3.0f, 0f, 0f), Quaternion.identity);
        _beatCube.parent = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (useGNURadio)
        {
            ReadFFTBuffer();
        }
        else
        {
            GenerateFFTBuffer();
        } 
        Equalizer();
       if (useFullBuffer)
       {
           FullSpectrumBuffer();
       }
       MakeFrequencyBand();
       //BandBuffer();
       BeatDetection(_bandBuffer);
       ClampAmplitudes(_bandBuffer);
       //BeatDetection(_bandBuffer);
       if (regenerateSpectrum)
       {
           RegenerateSpectrum();
           regenerateSpectrum = false;
       }
       //AnimateSpectrum();
       AnimateBand();
       if (rotate)
       {
           RotateCam();
       }
    }

    void SetupCamera()
    {
        Vector3 center = _audioCubeArray[0].localPosition - _audioCubeArray[^1].localPosition;
        float center_offset = center.magnitude * 0.5f;
        rotatePivot.localPosition =
            _audioCubeArray[^1].localPosition + (center.normalized * center_offset);
        if (rotate)
        {
            cameraTransform.localPosition =
                _audioCubeArray[^1].localPosition + (center.normalized * center_offset) + new Vector3(0f, 0f, (-1.0f) * center_offset);
            cameraTransform.parent = rotatePivot.transform;
        }
    }

    void RegenerateSpectrum()
    {
        _audioCubeArray = new Transform[SampleCount - (cutoffLow + cutoffHigh)];
        for (var i = 0; i < _audioCubeArray.Length; i++)
        {
            _audioCubeArray[i] = Instantiate(target, new Vector3(0f, 0f, 0f), Quaternion.identity);
            _audioCubeArray[i].parent = transform;
        }
        for (int i = _audioCubeArray.Length-1; i >= 0; i--)
        {
            //float scaleFactor = ( (1.0f / Mathf.Pow(10.0f, (i + 1.0f)))  + 1.0f) * widthMultiplier;
            float scaleFactor = (1.0f / (Mathf.Log10(i + 1.0f) + 1.0f)) * widthMultiplier;
            _audioCubeArray[i].localScale = new Vector3(cubeSize.x * scaleFactor, cubeSize.y, cubeSize.z * scaleFactor);
            if (i == _audioCubeArray.Length - 1)
                continue;
            _audioCubeArray[i].localPosition = new Vector3(_audioCubeArray[i+1].localPosition.x + _audioCubeArray[i+1].localScale.x / 2.0f + _audioCubeArray[i].localScale.x / 2.0f + spacing, 0f, 0f);
        }
        
        // Center Camera
        if (!rotate)
        {
            Vector3 center = _audioCubeArray[0].localPosition - _audioCubeArray[^1].localPosition;
            float center_offset = center.magnitude * 0.5f;
            cameraTransform.localPosition =
                _audioCubeArray[^1].localPosition + (center.normalized * center_offset) + new Vector3(0f, 0f, (-1.0f) * center_offset);
        }
        
    }

    void RegenerateBand()
    {
        _bandBufferCubeArray = new Transform[8];
        for (int i = 0; i < _bandBufferCubeArray.Length; i++)
        {
            _bandBufferCubeArray[i] = Instantiate(target, new Vector3(0f, 0f, 0f) + i * new Vector3(2.0f, 0.0f, 0.0f), Quaternion.identity);
            _bandBufferCubeArray[i].parent = transform;
        }
    }

    void Equalizer()
    {
        uint[] bandWidths = new uint[8];
        bandWidths[0] = equalizerSettings.bandWidth1;
        bandWidths[1] = equalizerSettings.bandWidth2;
        bandWidths[2] = equalizerSettings.bandWidth3;
        bandWidths[3] = equalizerSettings.bandWidth4;
        bandWidths[4] = equalizerSettings.bandWidth5;
        bandWidths[5] = equalizerSettings.bandWidth6;
        bandWidths[6] = equalizerSettings.bandWidth7;
        bandWidths[7] = equalizerSettings.bandWidth8;
        float[] equalizer = new float[8];
        equalizer[0] = equalizerSettings.band1;
        equalizer[1] = equalizerSettings.band2;
        equalizer[2] = equalizerSettings.band3;
        equalizer[3] = equalizerSettings.band4;
        equalizer[4] = equalizerSettings.band5;
        equalizer[5] = equalizerSettings.band6;
        equalizer[6] = equalizerSettings.band7;
        equalizer[7] = equalizerSettings.band8;
        
        uint position = 0;
        float eq_value = 1.0f;
        for (var j = 0; j < equalizer.Length; j++)
        {
            eq_value = equalizer[j];
            for (var i = position; i < position + bandWidths[j]; i++)
            {
                _spectrumBuffer[i] = eq_value * _spectrumBuffer[i];
            }
            position += bandWidths[j];
        }
    }

    /// <summary>
    /// Clamp values to range [0, 1] * clampMultiplier
    /// </summary>
    /// <param name="samples"></param>
    void ClampAmplitudes(float[] samples)
    {
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] *= 1000f;    // TODO This is really hacky
            _amplitude_peaks[i] = Mathf.Max(_amplitude_peaks[i], samples[i]);
            _bandBuffer[i] = (_bandBuffer[i] / _amplitude_peaks[i]) * clampMultiplier;
        }
    }

    void AverageAmplitude(float[] samples)
    {
        

    }

    void BeatDetection(float[] samples)
    {
        // TODO This stuff should be moved into fixed update to be framerate independent
        /*
        -Assume large Amplitude peaks as beats (prefer in lower frequencys)
        -Use AverageAmplitude as reference
        -For sake of sanity beats are counted as fractions of a bar instead of completed bars
        */

        // TODO Everything relating to time here is still wacky
        beat = false;
        if (flush)
        {
            duration = 0f;
            beat_count = 0;
        }
        duration += Time.deltaTime;
        if (_beatTimeout)
        {
            _currentBeatTimeout -= Time.deltaTime;
            _beatCube.localScale = new Vector3(1f, _beatCube.localScale.y / 2f, 1f);
            if (_currentBeatTimeout <= 0)
            {
                _beatTimeout = false;
                _currentBeatTimeout = beatTimeoutDuration;
                _beatCube.localScale = Vector3.zero;
            }
        }

        var frame_amplitude = 0f;
        for (var i = useBandsLow; i <= useBandsHigh; i++)
        {
            frame_amplitude += samples[i];
        }

        if (!_beatTimeout && frame_amplitude >= (_history.Sum() / (float) _history.Count) * beatThreshold)
        {
            _beatTimeout = true;
            beat = true;
            beat_count++;
            Debug.Log("Beat");
            _currentBeatTimeout = beatTimeoutDuration;
            _beatCube.localScale = new Vector3(1f, 15f, 1f);
        }

        if (_beatTimeout && _currentBeatTimeout >= beatTimeoutDuration / 2f)
        {
            bps = beat_count / duration;
            estimatedBPM = bps * 60f;
            return;
        }
        if (_history.Count > historyLength)
        {
            _history.Dequeue();
        }
        _history.Enqueue(frame_amplitude);
        bps = beat_count / duration;
        estimatedBPM = bps * 60f;
    }

    void AnimateSpectrum()
    {
        if (useFullBuffer)
        {
            int index = 0;
            for (var i = cutoffLow; i < (_spectrumBuffer.Length - (cutoffHigh)); i++)
            {
                _audioCubeArray[index].localScale = new Vector3(_audioCubeArray[index].localScale.x, cubeSize.y + Mathf.Min(_fullSpectrumBuffer[i], amplitudeCeiling)  * flatAmplitudeMultiplier, _audioCubeArray[index].localScale.z);
            }
        }
        else
        {
            int index = 0;
            for (var i = cutoffLow; i < (_spectrumBuffer.Length - (cutoffHigh)); i++)
            {
                _audioCubeArray[index].localScale = new Vector3(_audioCubeArray[index].localScale.x, cubeSize.y + Mathf.Min(_spectrumBuffer[i], amplitudeCeiling)  * flatAmplitudeMultiplier, _audioCubeArray[index].localScale.z);
                index++;
            }
        }
    }

    void AnimateBand()
    {
        for (int i = 0; i < _bandBuffer.Length; i++)
        {
            _bandBufferCubeArray[i].localScale = new Vector3(_bandBufferCubeArray[i].localScale.x, cubeSize.y + _bandBuffer[i], _bandBufferCubeArray[i].localScale.z);
        }
    }

    void MakeFrequencyBand()
    {
        uint[] bandWidths = new uint[8];
        bandWidths[0] = bandSettings.bandWidth1;
        bandWidths[1] = bandSettings.bandWidth2;
        bandWidths[2] = bandSettings.bandWidth3;
        bandWidths[3] = bandSettings.bandWidth4;
        bandWidths[4] = bandSettings.bandWidth5;
        bandWidths[5] = bandSettings.bandWidth6;
        bandWidths[6] = bandSettings.bandWidth7;
        bandWidths[7] = bandSettings.bandWidth8;

        uint position = 0;
        for (var j = 0; j < bandWidths.Length; j++)
        {
            float average = 0;
            for (var i = position; i < position + bandWidths[j]; i++)
            {
                average += _spectrumBuffer[i];
            }
            average /= bandWidths[j];
            _spectrumBands[j] = average;
            _bandBuffer[j] = average;
            position += bandWidths[j];
        }
    }

    void FullSpectrumBuffer()
    {
        for (int i = 0; i < _fullSpectrumBuffer.Length; i++)
        {
            if (_spectrumBuffer[i] < _fullSpectrumBuffer[i])
            {
                _fullSpectrumBuffer[i] -= _fullSpectrumBufferDecay[i];
                _fullSpectrumBufferDecay[i] *= bandBandBufferDecayMultiplier;
            }
            if (_spectrumBuffer[i] > _fullSpectrumBuffer[i])
            {
                _fullSpectrumBuffer[i] = _spectrumBuffer[i];
                _fullSpectrumBufferDecay[i] = bandBufferDecayRate;
            }
        }
    }
    
    void BandBuffer()
    {
        for (int i = 0; i < _spectrumBands.Length; i++)
        {
            if (_spectrumBands[i] < _bandBuffer[i])
            {
                _bandBuffer[i] -= _bandBufferDecay[i];
                _bandBufferDecay[i] *= bandBandBufferDecayMultiplier;
            }

            if (_spectrumBands[i] > _bandBuffer[i])
            {
                _bandBuffer[i] = _spectrumBands[i];
                _bandBufferDecay[i] = bandBufferDecayRate;
            }
        }
    }
    
    private void ReadFFTBuffer()
    {
        long updated = Math.Min(_fileStream.Length, BufferSizeBytes);
        try
        {
            _fileStream.Seek(-updated, SeekOrigin.End);
        }
        catch (IOException e)
        {
            // TODO Idk why this ocasionally attempts to seek beyond the start of the file
            //Debug.LogError(e.Message);
            //Debug.Log($"updated: {updated} | {_fileStream.Length}");
        }
        long length = updated / 4;
        for (int i = 0; i < length; i++)
        {
            try
            {
                _spectrumBuffer[i] = _reader.ReadSingle();
            }
            catch (EndOfStreamException e)
            {
                // FIXME
            }
        }
    }

    private void GenerateFFTBuffer()
    {
        AudioListener.GetSpectrumData(_spectrumBuffer, 0, FFTWindow.BlackmanHarris);
        foreach (var samp in _spectrumBuffer)
        {
            Debug.Log(samp);
        }
    }

    private void ReadMicrophone()
    {
        microphone = Microphone.devices[0].ToString();
        audioSource.outputAudioMixerGroup = standard;
        audioSource.clip = Microphone.Start(microphone, true, 10, AudioSettings.outputSampleRate);
        audioSource.Play();
    }
    
    private void RotateCam()
    {
        rotatePivot.transform.Rotate(speed * Time.deltaTime * 0.5f, speed * Time.deltaTime, 0);
    }
}

[System.Serializable]
public class EqualizerSettings
{
    [Header("BandWidth")]
    public uint bandWidth1 = 1;
    public uint bandWidth2 = 2;
    public uint bandWidth3 = 4;
    public uint bandWidth4 = 8;
    public uint bandWidth5 = 16; 
    public uint bandWidth6 = 32;
    public uint bandWidth7 = 64;
    public uint bandWidth8 = 128;

    [Header("Multipliers")] 
    [Range(0.0f, 2f)]public float band1 = 1.0f;
    [Range(0.0f, 2f)]public float band2 = 1.0f;
    [Range(0.0f, 2f)]public float band3 = 1.0f;
    [Range(0.0f, 2f)]public float band4 = 1.0f;
    [Range(0.0f, 2f)]public float band5 = 1.0f;
    [Range(0.0f, 2f)]public float band6 = 1.0f;
    [Range(0.0f, 2f)]public float band7 = 1.0f;
    [Range(0.0f, 2f)]public float band8 = 1.0f;
}

[System.Serializable]
public class BandSettings
{
    [Header("BandWidth")]
    public uint bandWidth1 = 1;
    public uint bandWidth2 = 2;
    public uint bandWidth3 = 4;
    public uint bandWidth4 = 8;
    public uint bandWidth5 = 16; 
    public uint bandWidth6 = 32;
    public uint bandWidth7 = 64;
    public uint bandWidth8 = 128;
}