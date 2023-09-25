using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T pInstance
    {
        get
        {
            if (instance != null)
                return instance;
            
            instance = FindObjectOfType<T>();

            if (instance != null)
                return instance;

            GameObject obj = new GameObject();
            obj.name = typeof(T).Name;
            instance = obj.AddComponent<T>();

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}