using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace AudioVisualizer
{
    public partial class AudioSpectrumEngine : MonoBehaviour
    {
        // Hardcoded only
        protected const uint SampleCount = 256;
        protected const long BufferSizeBytes = 1024;
        protected FileStream _fileStream;
        protected BinaryReader _reader;
        protected float[] _spectrumBuffer;
        protected float[] _spectrumBands;
        protected float[] _bandBuffer;
        protected float[] _bandBufferDecay;
        protected float[] _fullSpectrumBuffer;
        protected float[] _fullSpectrumBufferDecay;
        // Beat Detection
        protected bool _beatTimeout;
        protected float _currentBeatTimeout;
        protected Queue<float> _history = new Queue<float>();
        protected bool _beat;
        private float[] _amplitude_peaks;
        
        [Header("General Settings")]
        public string audioFilePath = "R:" + @"\" + "sink.txt";
        [SerializeField] protected bool displaySpectrum = true;
        [SerializeField] protected bool displayBands = true;
        [SerializeField] protected bool displayBeat = true;

        [Header("SignalProcessing")] 
        public EqualizerSettings equalizerSettings = new();
        public BandSettings bandSettings = new();
        public BeatSettings beatSettings = new();
        
        

        [Header("Other")] 
        public float clampMultiplier = 1.0f;

        private void Awake()
        {
            Debug.LogWarning("FFT buffer reads ocasionally produce exceptions which are silently ignored");
            try
            {
                _fileStream = new FileStream(audioFilePath, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite);
                _reader = new BinaryReader(_fileStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.LogError(e);
                Debug.LogError("Missing GNU Radio Sink File. RAMDsik is probably missconfigured");
                throw;
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
            StartRoutine();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateLoop();
        }

        protected virtual void StartRoutine()
        {
            RegenerateSpectrum();
            RegenerateBand();
            RegenerateBeatCube();
        }
        
        protected virtual void UpdateLoop()
        {
            ReadFFTBuffer();
            Equalizer();
            if (visualSettings.useFullBuffer)
            {
                FullSpectrumBuffer();
            }

            MakeFrequencyBand();
            BeatDetection(_bandBuffer);
            BandBuffer();
            ClampAmplitudes(_bandBuffer);
            if (visualSettings.regenerateSpectrum)
            {
                RegenerateSpectrum();
                visualSettings.regenerateSpectrum = false;
            }

            if (displaySpectrum)
            {
                AnimateSpectrum();
            }

            if (displayBands)
            {
                AnimateBand();
            }

            if (displayBeat)
            {
                AnimateBeat();
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
                samples[i] *= 1000f; // TODO This is really hacky
                _amplitude_peaks[i] = Mathf.Max(_amplitude_peaks[i], samples[i]);
                _bandBuffer[i] = (_bandBuffer[i] / _amplitude_peaks[i]) * clampMultiplier;
            }
        }

        void AverageAmplitude(float[] samples)
        {
            // TODO
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
            _beat = false;
            if (beatSettings.flush)
            {
                beatSettings.duration = 0f;
                beatSettings.beat_count = 0;
            }

            beatSettings.duration += Time.deltaTime;
            if (_beatTimeout)
            {
                _currentBeatTimeout -= Time.deltaTime;
                if (_currentBeatTimeout <= 0)
                {
                    _beatTimeout = false;
                    _currentBeatTimeout = beatSettings.beatTimeoutDuration;
                    //_beatCube.localScale = Vector3.zero;
                }
            }

            var frame_amplitude = 0f;
            for (var i = beatSettings.useBandsLow; i <= beatSettings.useBandsHigh; i++)
            {
                frame_amplitude += samples[i];
            }

            if (!_beatTimeout && frame_amplitude >= (_history.Sum() / (float)_history.Count) * beatSettings.beatThreshold)
            {
                _beatTimeout = true;
                _beat = true;
                beatSettings.beat_count++;
                //Debug.Log("Beat");
                _currentBeatTimeout = beatSettings.beatTimeoutDuration;
            }

            if (_beatTimeout && _currentBeatTimeout >= beatSettings.beatTimeoutDuration / 2f)
            {
                beatSettings.bps = beatSettings.beat_count / beatSettings.duration;
                beatSettings.estimatedBPM = beatSettings.bps * 60f;
                return;
            }

            if (_history.Count > beatSettings.historyLength)
            {
                _history.Dequeue();
            }

            _history.Enqueue(frame_amplitude);
            beatSettings.bps = beatSettings.beat_count / beatSettings.duration;
            beatSettings.estimatedBPM = beatSettings.bps * 60f;
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
                    _fullSpectrumBufferDecay[i] *= visualSettings.bandBandBufferDecayMultiplier;
                }

                if (_spectrumBuffer[i] > _fullSpectrumBuffer[i])
                {
                    _fullSpectrumBuffer[i] = _spectrumBuffer[i];
                    _fullSpectrumBufferDecay[i] = visualSettings.bandBufferDecayRate;
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
                    _bandBufferDecay[i] *= visualSettings.bandBandBufferDecayMultiplier;
                }

                if (_spectrumBands[i] > _bandBuffer[i])
                {
                    _bandBuffer[i] = _spectrumBands[i];
                    _bandBufferDecay[i] = visualSettings.bandBufferDecayRate;
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

        #region Interface
        public bool GetBeat()
        {
            return _beat;
        }

        public float[] GetSpectrum()
        {
            return _spectrumBuffer;
        }

        public float[] GetSpectrumBuffered()
        {
            throw new NotImplementedException();
        }

        public float[] GetBands()
        {
            return _bandBuffer;
        }

        public float[] GetBandsBuffered()
        {
            throw new NotImplementedException();
        }

        #endregion
        
    }

    [System.Serializable]
    public class BeatSettings
    {
        [Header("Info")]
        public float estimatedBPM;
        public float bps;
        public long beat_count;
        
        [Header("Toggles")]
        public bool flush;
        
        [Header("Settings")]
        public int historyLength;
        public int useBandsLow;
        public int useBandsHigh;
        public float beatTimeoutDuration;
        public int historyBeatTimeout;
        public float beatThreshold;
        public float duration;
    }
}