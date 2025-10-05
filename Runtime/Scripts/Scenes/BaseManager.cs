using MysticIsle.DreamEngine.Phases;
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
        /// 实例
        /// </summary>
        public static TBaseManager Instance => GetInstance();

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
        #endregion

        #region Fields

        private float runningTime;
        private readonly List<IGameController> gameSystems = new();
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
        public PhaseController PhaseController { get; protected set; }

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
                }
                else if (instance != this)
                {
                    DestroyImmediate(gameObject);
                }
            }

            OnCreateGameSystems();
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

            gameSystems.Clear();
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
        /// 创建并初始化游戏系统（在 Awake 中调用）
        /// </summary>
        protected abstract void OnCreateGameSystems();

        /// <summary>
        /// 创建指定类型的游戏控制器（Game Controller）
        /// </summary>
        /// <typeparam name="TGameController">游戏系统类型</typeparam>
        /// <returns>创建的系统实例</returns>
        protected TGameController CreateGameController<TGameController>() where TGameController : MonoBehaviour, IGameController
        {
            string managerName = string.Format("{0}", typeof(TGameController).Name);
            GameObject managerObject = new(managerName);
            managerObject.transform.SetParent(this.transform, false);
            TGameController manager = managerObject.AddComponent<TGameController>();
            gameSystems.Add(manager);

            return manager;
        }

        /// <summary>
        /// 通过路径读取预制件创建指定类型的游戏控制器（Game Controller）
        /// </summary>
        /// <typeparam name="TGameController">游戏系统类型</typeparam>
        /// <param name="path">预制件路径</param>
        /// <returns>创建的系统实例</returns>
        protected TGameController CreateGameController<TGameController>(string path) where TGameController : MonoBehaviour, IGameController
        {
            Object obj = AssetManager.LoadAsset<Object>(path);
            if (obj == null)
            {
                return null;
            }

            GameObject managerObject = Instantiate(obj) as GameObject;
            string managerName = string.Format("{0}", typeof(TGameController).Name);
            managerObject.name = managerName;
            managerObject.transform.SetParent(this.transform, false);
            if (!managerObject.TryGetComponent<TGameController>(out var manager))
            {
                manager = managerObject.AddComponent<TGameController>();
            }

            gameSystems.Add(manager);

            return manager;
        }

        #endregion
    }
}