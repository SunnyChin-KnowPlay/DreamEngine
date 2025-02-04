using UnityEngine;

#if UNITY_EDITOR
#endif

namespace MysticIsle.DreamEngine
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>, new()
    {
        #region Params
        private const string singletonRootName = "Singletons";


        #endregion

        #region Instance
        private static bool isQuitting = false;
        private static T instance;
        private static T editorInstance; // For Editor use

        public static T Instance
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (!isQuitting && instance == null)
                    {
                        CreateOrFindInstance();
                    }
                    return instance;
                }
                else
                {
                    if (editorInstance == null)
                    {
                        editorInstance = new T(); // Use a regular instance in Editor mode
                    }
                    return editorInstance;
                }
            }
        }

        private static void CreateOrFindInstance()
        {
            if (instance == null)
            {
                // Use FindAnyObjectByType to find an existing instance if any
                instance = FindAnyObjectByType<T>();

                if (instance == null)
                {
                    GameObject go = new(typeof(T).Name);
                    instance = go.AddComponent<T>();
                    // Ensure that the singleton root exists and set parent to it
                    GameObject rootObj = GetOrCreateRoot();
                    if (rootObj != null)
                    {
                        instance.transform.SetParent(rootObj.transform);
                    }
                    DontDestroyOnLoad(go);
                }
            }
        }

        private static GameObject GetOrCreateRoot()
        {
            GameObject rootObj = GameObject.Find(singletonRootName);
            if (rootObj == null)
            {
                rootObj = new GameObject(singletonRootName);
                DontDestroyOnLoad(rootObj);
            }
            return rootObj;
        }

        public static void DestroyInstance()
        {
            if (Application.isPlaying)
            {
                if (instance != null)
                {
                    // If it's in play mode, destroy the instance's game object
                    Destroy(instance.gameObject);
                    instance = null;
                }
            }
            else
            {
                // If it's in editor mode, just clear the editorInstance
                editorInstance = null;
            }
        }
        #endregion

        #region Mono
        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                isQuitting = false;
                if (instance == null)
                {
                    instance = this as T;
                    DontDestroyOnLoad(gameObject);

                    string singletonName = $"Singleton of {typeof(T).Name}";
                    gameObject.name = singletonName;

                    // Set parent to Singleton root (if applicable)
                    GameObject rootObj = GetOrCreateRoot();
                    if (rootObj != null)
                    {
                        instance.transform.SetParent(rootObj.transform);
                    }
                }
                else if (instance != this)
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        // Clean up in both play and editor mode
        protected virtual void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (instance == this)
                {
                    instance = null;
                }
            }
            else
            {
                // Ensure editorInstance is properly cleared when exiting play mode in the editor
                editorInstance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }
        #endregion
    }
}
