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
        private readonly List<IGameService> services = new();
        private readonly List<IGameService> runningServices = new();
        #endregion

        #region Properties

        /// <summary>
        /// 运行时间
        /// </summary>
        public float RunningTime => runningTime;
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

            services.Clear();
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

            runningServices.Clear();
            runningServices.AddRange(services);
            foreach (var service in runningServices)
            {
                service.OnUpdate();
            }
        }

        /// <summary>
        /// Unity的FixedUpdate方法
        /// </summary>
        protected virtual void FixedUpdate()
        {
        }

        #endregion

        #region Initialization



        /// <summary>
        /// 将已有的游戏服务（Game Service）添加到管理器
        /// </summary>
        /// <typeparam name="TGameService">游戏服务类型</typeparam>
        /// <param name="gameService">游戏服务实例</param>
        /// <returns>添加后的系统实例</returns>
        protected TGameService AddGameService<TGameService>(TGameService gameService) where TGameService : class, IGameService
        {
            if (gameService == null)
            {
                return null;
            }

            if (!services.Contains(gameService))
            {
                services.Add(gameService);
            }

            return gameService;
        }


        #endregion
    }
}