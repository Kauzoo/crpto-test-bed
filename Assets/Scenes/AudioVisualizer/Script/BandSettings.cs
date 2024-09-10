using UnityEngine;

namespace AudioVisualizer
{
    [System.Serializable]
    public class BandSettings
    {
        [Header("BandWidth")] public uint bandWidth1 = 1;
        public uint bandWidth2 = 2;
        public uint bandWidth3 = 4;
        public uint bandWidth4 = 8;
        public uint bandWidth5 = 16;
        public uint bandWidth6 = 32;
        public uint bandWidth7 = 64;
        public uint bandWidth8 = 128;
    }
}