using MysticIsle.DreamEngine.Phases;
using MysticIsle.DreamEngine.Scenes;
using MysticIsle.DreamEngine.UI;
using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 基础管理器抽象类
    /// </summary>
    public abstract class BaseManager<TBaseManager> : MonoBehaviour
    where TBaseManager : BaseManager<TBaseManager>
    {
        #region Instance

        private static bool isQuitting = false;
        private static TBaseManager instance;

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns>游戏管理器实例</returns>
        protected static TBaseManager GetInstance()
        {
            if (Application.isPlaying)
            {
                if (!isQuitting && instance == null)
                {
                    CreateOrFindInstance();
                }
                return instance;
            }

            return null;
        }

        /// <summary>
        /// 创建或查找实例
        /// </summary>
        private static void CreateOrFindInstance()
        {
            if (instance == null)
            {
                // Use FindAnyObjectByType to find an existing instance if any
                instance = FindAnyObjectByType<TBaseManager>();

                if (instance == null)
                {
                    GameObject go = new(typeof(TBaseManager).Name);
                    instance = go.AddComponent<TBaseManager>();
                    DontDestroyOnLoad(go);
                }
            }
        }

        /// <summary>
        /// 销毁实例
        /// </summary>
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
        }

        /// <summary>
        /// 是否为全局
        /// </summary>
        protected abstract bool IsGlobal { get; }

        #endregion

        #region Fields

        private float runningTime;
        private readonly List<IManager> managers = new();
        #endregion

        #region Properties

        /// <summary>
        /// 运行时间
        /// </summary>
        public float RunningTime => runningTime;

        /// <summary>
        /// UI管理器
        /// </summary>
        public UIManager UIManager { get; protected set; }

        /// <summary>
        /// 阶段管理器
        /// </summary>
        public PhaseManager PhaseManager { get; protected set; }

        /// <summary>
        /// 虚拟相机管理器
        /// </summary>
        public VirtualCameraManager VirtualCameraManager { get; protected set; }

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Unity的Awake方法
        /// </summary>
        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                isQuitting = false;
                if (instance == null)
                {
                    instance = this as TBaseManager;
                    if (IsGlobal)
                    {
                        DontDestroyOnLoad(gameObject);
                    }
                }
                else if (instance != this)
                {
                    DestroyImmediate(gameObject);
                }
            }

            OnCreateManagers();
        }

        /// <summary>
        /// Unity的OnDestroy方法
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

            managers.Clear();
        }

        /// <summary>
        /// Unity的OnApplicationQuit方法
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }

        /// <summary>
        /// Unity的OnEnable方法
        /// </summary>
        protected virtual void OnEnable()
        {
            runningTime = 0;
        }

        /// <summary>
        /// Unity的OnDisable方法
        /// </summary>
        protected virtual void OnDisable()
        {
        }

        /// <summary>
        /// Unity的Start方法
        /// </summary>
        protected virtual void Start()
        {
        }

        /// <summary>
        /// Unity的Update方法
        /// </summary>
        protected virtual void Update()
        {
            float delta = Time.deltaTime;
            runningTime += delta;
        }

        /// <summary>
        /// Unity的FixedUpdate方法
        /// </summary>
        protected virtual void FixedUpdate()
        {
        }

        #endregion

        #region Logic

        /// <summary>
        /// 在层级中查找对象
        /// </summary>
        /// <param name="parent">父对象</param>
        /// <param name="objectName">对象名称</param>
        /// <returns>找到的对象</returns>
        protected static GameObject FindObjectInHierarchy(GameObject parent, string objectName)
        {
            if (parent.name == objectName)
            {
                return parent;
            }

            foreach (Transform child in parent.transform)
            {
                GameObject foundObj = FindObjectInHierarchy(child.gameObject, objectName);
                if (foundObj != null)
                {
                    return foundObj;
                }
            }

            return null;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 创建管理器
        /// </summary>
        protected abstract void OnCreateManagers();

        /// <summary>
        /// 创建指定类型的管理器
        /// </summary>
        /// <typeparam name="TManager">管理器类型</typeparam>
        /// <returns>创建的管理器</returns>
        protected TManager CreateManager<TManager>() where TManager : MonoBehaviour, IManager
        {
            string managerName = string.Format("{0}", typeof(TManager).Name);
            GameObject managerObject = new(managerName);
            managerObject.transform.SetParent(this.transform, false);
            TManager manager = managerObject.AddComponent<TManager>();
            managers.Add(manager);

            return manager;
        }

        /// <summary>
        /// 通过路径读取预制件创建指定类型的管理器
        /// </summary>
        /// <typeparam name="TManager">管理器类型</typeparam>
        /// <param name="path">预制件路径</param>
        /// <returns>创建的管理器</returns>
        protected TManager CreateManager<TManager>(string path) where TManager : MonoBehaviour, IManager
        {
            Object obj = AssetManager.LoadAsset<Object>(path);
            if (obj == null)
            {
                return null;
            }

            GameObject managerObject = Instantiate(obj) as GameObject;
            string managerName = string.Format("{0}", typeof(TManager).Name);
            managerObject.name = managerName;
            managerObject.transform.SetParent(this.transform, false);
            if (!managerObject.TryGetComponent<TManager>(out var manager))
            {
                manager = managerObject.AddComponent<TManager>();
            }

            managers.Add(manager);

            return manager;
        }

        #endregion
    }
}