using UnityEngine;

/// <summary>
///     Be aware this will not prevent a non singleton constructor
///     such as `T myT = new T();`
///     To prevent that, add `protected T () {}` to your singleton class.
///     As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _Instance;

    private static readonly object _lock = new object();

    protected static bool _applicationIsQuitting;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting) return null;

            lock (_lock)
            {
                if (_Instance == null)
                {
                    _Instance = (T) FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                        return _Instance;

                    if (_Instance == null)
                    {
                        var singleton = new GameObject();
                        _Instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T);
                    }
                }

                return _Instance;
            }
        }
    }

    /// <summary>
    ///     When Unity quits, it destroys objects in a random order.
    ///     In principle, a Singleton is only destroyed when application quits.
    ///     If any script calls Instance after it have been destroyed,
    ///     it will create a buggy ghost object that will stay on the Editor scene
    ///     even after stopping playing the Application. Really bad!
    ///     So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public virtual void OnDestroy()
    {
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}