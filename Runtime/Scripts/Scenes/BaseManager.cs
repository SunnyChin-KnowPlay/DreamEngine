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

        /// <summary>
        /// 是否为全局
        /// </summary>
        protected abstract bool IsGlobal { get; }

        #endregion

        #region Fields

        private float runningTime;
        private readonly List<IGameSystem> gameSystems = new();
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

        #region Initialization



        /// <summary>
        /// 将已有的游戏系统（Game System）添加到管理器
        /// </summary>
        /// <typeparam name="TGameSystem">游戏系统类型</typeparam>
        /// <param name="gameSystem">游戏系统实例</param>
        /// <returns>添加后的系统实例</returns>
        protected TGameSystem AddGameSystem<TGameSystem>(TGameSystem gameSystem) where TGameSystem : class, IGameSystem
        {
            if (gameSystem == null)
            {
                return null;
            }

            if (!gameSystems.Contains(gameSystem))
            {
                gameSystems.Add(gameSystem);
            }

            return gameSystem;
        }


        #endregion
    }
}