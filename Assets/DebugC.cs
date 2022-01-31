
    using UnityEngine;

    public class DebugC
    {
        private const string PREFIX = "LOG::: "; 
        
        public static void Log(string msg)
        {
            Debug.Log(PREFIX + msg);
        }
        
        public static void Warn(string msg)
        {
            Debug.LogWarning(PREFIX + msg);
        }
        
        public static void Error(string msg)
        {
            Debug.LogError(PREFIX + msg);
        }
    }
