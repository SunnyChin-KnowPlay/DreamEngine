using UnityEngine;

#if UNITY_EDITOR
#endif

namespace MysticIsle.DreamEngine
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
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
                    if (isQuitting)
                        return null;
                    // Only create or find, do not assign or persist
                    T found = FindAnyObjectByType<T>();
                    if (found != null)
                        return found;
                    GameObject go = new(typeof(T).Name);
                    return go.AddComponent<T>();
                }
                else
                {
                    if (editorInstance == null)
                    {
                        GameObject go = new(typeof(T).Name + "_EditorInstance")
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        editorInstance = go.AddComponent<T>();
                    }
                    return editorInstance;
                }
            }
        }

        // Remove CreateOrFindInstance, logic now only in Awake

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
                    // First instance: adopt and persist
                    instance = this as T;
                    // Standardize naming
                    string expectedName = $"Singleton of {typeof(T).Name}";
                    if (gameObject.name != expectedName)
                        gameObject.name = expectedName;
                    // Parent under Singleton root
                    GameObject rootObj = GetOrCreateRoot();
                    if (rootObj != null && transform.parent != rootObj.transform)
                        transform.SetParent(rootObj.transform);
                    // Always persist
                    if (gameObject.scene.IsValid())
                        DontDestroyOnLoad(gameObject);
                }
                else if (instance != this)
                {
                    // Destroy duplicate
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
