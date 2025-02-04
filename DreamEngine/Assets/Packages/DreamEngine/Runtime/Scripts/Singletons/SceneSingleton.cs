using UnityEngine;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 场景实例
    /// </summary>
    /// <typeparam name="T">子类</typeparam>
    public abstract class SceneSingleton<T> : MonoBehaviour where T : SceneSingleton<T>
    {
        #region Instance
        private const string instanceRootName = "SceneInstances";

        /// <summary>
        /// 实例
        /// </summary>
        public static T SceneInstance => sceneInstance;
        private static T sceneInstance;

        public static T CreateInstance()
        {
            System.Type type = typeof(T);
            string name = string.Format("Singleton of {0}", type.Name);
            GameObject go = new(name);
            T component = go.AddComponent<T>();
            return component;
        }

        private static GameObject RootObject
        {
            get
            {
                GameObject rootObj = GameObject.Find(instanceRootName);
                if (null == rootObj)
                {
                    rootObj = new GameObject(instanceRootName);
                }
                return rootObj;
            }
        }
        #endregion

        #region Mono
        protected virtual void Awake()
        {
            if (null != sceneInstance && sceneInstance != this)
            {
                GameObject.Destroy(this);
                return;
            }

            sceneInstance = this as T;
            GameObject rootObj = RootObject;
            if (rootObj != null)
            {
                sceneInstance.transform.SetParent(rootObj.transform);
            }
        }

        protected virtual void OnDestroy()
        {
            if (null != sceneInstance && sceneInstance == this)
            {
                sceneInstance = null;
            }
        }
        #endregion
    }
}
