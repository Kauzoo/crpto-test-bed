using UnityEngine;

namespace AudioVisualizer
{
    [System.Serializable]
    public class EqualizerSettings
    {
        [Header("BandWidth")] public uint bandWidth1 = 1;
        public uint bandWidth2 = 2;
        public uint bandWidth3 = 4;
        public uint bandWidth4 = 8;
        public uint bandWidth5 = 16;
        public uint bandWidth6 = 32;
        public uint bandWidth7 = 64;
        public uint bandWidth8 = 128;

        [Header("Multipliers")] [Range(0.0f, 2f)]
        public float band1 = 1.0f;

        [Range(0.0f, 2f)] public float band2 = 1.0f;
        [Range(0.0f, 2f)] public float band3 = 1.0f;
        [Range(0.0f, 2f)] public float band4 = 1.0f;
        [Range(0.0f, 2f)] public float band5 = 1.0f;
        [Range(0.0f, 2f)] public float band6 = 1.0f;
        [Range(0.0f, 2f)] public float band7 = 1.0f;
        [Range(0.0f, 2f)] public float band8 = 1.0f;
    }
}