using UnityEngine;


namespace AudioVisualizer
{
    public partial class AudioSpectrumEngine
    {
        private Transform[] _audioCubeArray;
        private Transform[] _bandBufferCubeArray;
        public VisualSettings visualSettings = new VisualSettings();
        
        void SetupCamera()
        {
            Vector3 center = _audioCubeArray[0].localPosition - _audioCubeArray[^1].localPosition;
            float center_offset = center.magnitude * 0.5f;
            visualSettings.rotatePivot.localPosition =
                _audioCubeArray[^1].localPosition + (center.normalized * center_offset);
            if (visualSettings.rotate)
            {
                visualSettings.cameraTransform.localPosition =
                    _audioCubeArray[^1].localPosition + (center.normalized * center_offset) +
                    new Vector3(0f, 0f, (-1.0f) * center_offset);
                visualSettings.cameraTransform.parent = visualSettings.rotatePivot.transform;
            }
        }
        
        void RegenerateSpectrum()
        {
            _audioCubeArray = new Transform[SampleCount - (visualSettings.cutoffLow + visualSettings.cutoffHigh)];
            for (var i = 0; i < _audioCubeArray.Length; i++)
            {
                _audioCubeArray[i] = Instantiate(visualSettings.target, new Vector3(0f, 0f, 0f), Quaternion.identity);
                _audioCubeArray[i].parent = transform;
            }

            for (int i = _audioCubeArray.Length - 1; i >= 0; i--)
            {
                //float scaleFactor = ( (1.0f / Mathf.Pow(10.0f, (i + 1.0f)))  + 1.0f) * widthMultiplier;
                float scaleFactor = (1.0f / (Mathf.Log10(i + 1.0f) + 1.0f)) * visualSettings.widthMultiplier;
                _audioCubeArray[i].localScale =
                    new Vector3(visualSettings.cubeSize.x * scaleFactor, visualSettings.cubeSize.y, visualSettings.cubeSize.z * scaleFactor);
                if (i == _audioCubeArray.Length - 1)
                    continue;
                _audioCubeArray[i].localPosition =
                    new Vector3(
                        _audioCubeArray[i + 1].localPosition.x + _audioCubeArray[i + 1].localScale.x / 2.0f +
                        _audioCubeArray[i].localScale.x / 2.0f + visualSettings.spacing, 0f, 0f);
            }

            // Center Camera
            if (!visualSettings.rotate)
            {
                Vector3 center = _audioCubeArray[0].localPosition - _audioCubeArray[^1].localPosition;
                float center_offset = center.magnitude * 0.5f;
                visualSettings.cameraTransform.localPosition =
                    _audioCubeArray[^1].localPosition + (center.normalized * center_offset) +
                    new Vector3(0f, 0f, (-1.0f) * center_offset);
            }

        }

        void RegenerateBand()
        {
            _bandBufferCubeArray = new Transform[8];
            for (int i = 0; i < _bandBufferCubeArray.Length; i++)
            {
                _bandBufferCubeArray[i] = Instantiate(visualSettings.target,
                    new Vector3(0f, 0f, 0f) + i * new Vector3(2.0f, 0.0f, 0.0f), Quaternion.identity);
                _bandBufferCubeArray[i].parent = transform;
            }
        }

        void RegenerateBeatCube()
        {
            visualSettings.beatCube = Instantiate(visualSettings.target, new Vector3(-3.0f, 0f, 0f), Quaternion.identity);
            visualSettings.beatCube.parent = this.transform;
        }

        void AnimateSpectrum()
        {
            if (visualSettings.useFullBuffer)
            {
                int index = 0;
                for (var i = visualSettings.cutoffLow; i < (_spectrumBuffer.Length - (visualSettings.cutoffHigh)); i++)
                {
                    _audioCubeArray[index].localScale = new Vector3(_audioCubeArray[index].localScale.x,
                        visualSettings.cubeSize.y + Mathf.Min(_fullSpectrumBuffer[i], visualSettings.spectrumAmplitudeCeiling) *
                        visualSettings.spectrumMultiplier, _audioCubeArray[index].localScale.z);
                }
            }
            else
            {
                int index = 0;
                for (var i = visualSettings.cutoffLow; i < (_spectrumBuffer.Length - (visualSettings.cutoffHigh)); i++)
                {
                    _audioCubeArray[index].localScale = new Vector3(_audioCubeArray[index].localScale.x,
                        visualSettings.cubeSize.y + Mathf.Min(_spectrumBuffer[i], visualSettings.spectrumAmplitudeCeiling) *
                        visualSettings.spectrumMultiplier, _audioCubeArray[index].localScale.z);
                    index++;
                }
            }
        }

        void AnimateBand()
        {
            for (int i = 0; i < _bandBuffer.Length; i++)
            {
                _bandBufferCubeArray[i].localScale = new Vector3(_bandBufferCubeArray[i].localScale.x,
                    visualSettings.cubeSize.y + Mathf.Min(_bandBuffer[i], visualSettings.bandAmplitudeCeiling) * visualSettings.bandAmplitudeMultiplier, _bandBufferCubeArray[i].localScale.z);
            }
        }

        void AnimateBeat()
        {
            if (_beatTimeout)
            {
                visualSettings.beatCube.localScale = new Vector3(1f, visualSettings.beatCube.localScale.y / 2f, 1f);
            }

            if (!_beatTimeout && _beat)
            {
                visualSettings.beatCube.localScale = new Vector3(1f, 15f, 1f);
            }
        }
        
    }
    
    [System.Serializable]
    public class VisualSettings
    {
        [Header("Toggles")]
        public bool regenerateSpectrum = false;
        public bool useFullBuffer = false;
        public bool rotate = false;
        
        [Header("Spectrum Settings")]
        public int cutoffLow = 0;
        public int cutoffHigh = 0;
        public float spectrumAmplitudeCeiling = 1.0f;
        public float spectrumMultiplier = 1.0f;
        public float bandBufferDecayRate = 0.005f;
        public float bandBandBufferDecayMultiplier = 1.2f;
        public float spacing = 1.0f;
        public float widthMultiplier = 1.0f;
        public Vector3 cubeSize = Vector3.one;
        
        [Header("Band Settings")]
        public float bandAmplitudeCeiling = 1.0f;
        public float bandAmplitudeMultiplier = 1.0f;
        
        [Header("Components")]
        public Transform target;
        public Transform bandCube;
        public Transform beatCube;
        public Transform cameraTransform;
        public Transform rotatePivot;
    }
}