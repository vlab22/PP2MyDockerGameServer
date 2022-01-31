using System;
using UnityEngine.Events;

[Serializable]
public class StringIntUnityEvent : UnityEvent<string,int>
{
        
}

[Serializable]
public class StringUnityEvent : UnityEvent<string>
{
        
}


[Serializable]
public class IntUnityEvent : UnityEvent<int>
{
        
}