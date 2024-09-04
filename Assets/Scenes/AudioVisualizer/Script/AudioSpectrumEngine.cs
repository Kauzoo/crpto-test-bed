using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;

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
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!useGNURadio)
        {
            ReadMicrophone();
        }
        RegenerateSpectrum();
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
       //MakeFrequencyBand();
       //BandBuffer();
       if (regenerateSpectrum)
       {
           RegenerateSpectrum();
           regenerateSpectrum = false;
       }
       AnimateSpectrum();
       if (rotate)
       {
           RotateCam();
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
        for (int i = 0; i < _bandBufferCubeArray.Length; i++)
        {
            _bandBufferCubeArray[i] = Instantiate(target, new Vector3(0f, 0f, 0f), Quaternion.identity);
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
    }

    private void ReadMicrophone()
    {
        audioSource.clip = Microphone.Start(microphone, true, 10, AudioSettings.outputSampleRate);
        audioSource.Play();
    }
    
    private void RotateCam()
    {
        rotatePivot.transform.Rotate(speed * Time.deltaTime * 0.5f, speed * Time.deltaTime, 0);
    }
}
