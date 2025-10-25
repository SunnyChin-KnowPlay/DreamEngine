using UnityEngine;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 通用单例基类，提供运行时与编辑器下的单例访问与生命周期管理。
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        #region Params
        /// <summary>
        /// 单例根节点在层级中的名称。
        /// </summary>
        private const string singletonRootName = "Singletons";
        #endregion

        #region Instance
        /// <summary>
        /// 标记是否正在退出应用，用于阻止退出流程中再次创建实例。
        /// </summary>
        private static bool isQuitting = false;
        /// <summary>
        /// 运行时单例实例。
        /// </summary>
        private static T instance;
        /// <summary>
        /// 编辑器模式下的临时实例（仅在非运行时使用）。
        /// </summary>
        private static T editorInstance; // 编辑器模式专用实例

        /// <summary>
        /// 获取单例实例；在运行时确保唯一实例，在编辑器模式下提供隐藏的临时实例。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (instance != null)
                        return instance;

                    if (isQuitting)
                        return null;

                    // 只查找或创建，不在此阶段持久化引用
                    T found = FindAnyObjectByType<T>();
                    if (found != null)
                    {
                        instance = found;
                        return instance;
                    }
                    GameObject go = new(typeof(T).Name);
                    instance = go.AddComponent<T>();
                    return instance;
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
        // 已移除 CreateOrFindInstance，初始化逻辑集中在 Awake 中

        /// <summary>
        /// 查找或创建单例根对象，确保其在场景切换时持久存在。
        /// </summary>
        /// <returns>单例根对象。</returns>
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

        /// <summary>
        /// 销毁当前单例实例；运行时销毁对象，编辑器模式仅清理缓存引用。
        /// </summary>
        public static void DestroyInstance()
        {
            if (Application.isPlaying)
            {
                if (instance != null)
                {
                    // 运行时销毁单例对象
                    Destroy(instance.gameObject);
                }
            }
            else
            {
                // 编辑器模式下仅清理缓存的实例
                editorInstance = null;
            }
        }
        #endregion

        #region Mono
        /// <summary>
        /// Awake 生命周期回调，保证仅存在一个实例并配置根节点及持久化。
        /// </summary>
        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                isQuitting = false;
                if (instance == null)
                {
                    // 首个实例：接管并持久化
                    instance = this as T;
                    // 统一命名
                    string expectedName = $"Singleton of {typeof(T).Name}";
                    if (gameObject.name != expectedName)
                        gameObject.name = expectedName;
                    // 挂载到单例根节点之下
                    GameObject rootObj = GetOrCreateRoot();
                    if (rootObj != null && transform.parent != rootObj.transform)
                        transform.SetParent(rootObj.transform);
                    // 确保切换场景时不被销毁
                    if (gameObject.scene.IsValid())
                        DontDestroyOnLoad(gameObject);
                }
                else if (instance != this)
                {
                    // 销毁重复实例
                    DestroyImmediate(gameObject);
                }
            }
        }

        /// <summary>
        /// OnDestroy 生命周期回调，在运行时与编辑器模式下分别清理引用。
        /// </summary>
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
                // 确保退出播放模式时清理编辑器缓存的实例
                editorInstance = null;
            }
        }

        /// <summary>
        /// 应用退出回调，标记退出状态以防止在退出流程中重新创建实例。
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }
        #endregion
    }
}
