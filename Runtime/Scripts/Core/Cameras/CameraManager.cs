using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MysticIsle.DreamEngine.Core
{
    /// <summary>
    /// 相机管理器，负责管理主相机的相机堆栈和顶部相机堆栈。
    /// 这使得能够管理多个相机渲染层次，为复杂的渲染策略提供支持。
    /// </summary>
    public sealed class CameraManager : Singleton<CameraManager>
    {
        #region Fields
        // 使用词典来管理摄像机堆栈，key为堆栈类型，value为队列
        private readonly Dictionary<int, Queue<Camera>> cameraQueues = new();
        private CinemachineBrain _cinemachineBrain;
        #endregion

        #region Properties
        /// <summary>
        /// 获取主相机。
        /// </summary>
        public Camera MainCamera => Camera.main;

        /// <summary>
        /// 获取主相机的 CinemachineBrain。
        /// </summary>
        public CinemachineBrain CinemachineBrain
        {
            get
            {
                if (_cinemachineBrain == null)
                {
                    _cinemachineBrain = MainCamera.GetComponent<CinemachineBrain>();
                    if (_cinemachineBrain == null)
                    {
                        Debug.LogError("CinemachineBrain not found in the scene.");
                    }
                }
                return _cinemachineBrain;
            }
        }
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// 初始化时加载和设置主相机。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        }
        #endregion

        #region Camera Stack Management
        /// <summary>
        /// 将相机添加到指定的摄像机堆栈中。
        /// </summary>
        /// <param name="camera">要添加的相机。</param>
        /// <param name="stackType">摄像机堆栈类型，比如 0 代表普通，1 代表顶部等等。</param>
        public void AddCamera(Camera camera, int stackType)
        {
            if (camera == null)
            {
                Debug.LogError("Attempted to add a null camera to the stack.");
                return;
            }

            if (!cameraQueues.ContainsKey(stackType))
            {
                cameraQueues[stackType] = new Queue<Camera>();
            }

            if (!cameraQueues[stackType].Contains(camera))
            {
                SetupCamera(camera);
                cameraQueues[stackType].Enqueue(camera);
                UpdateCameraStack();
            }
        }

        /// <summary>
        /// 从所有的摄像机堆栈中移除相机。
        /// </summary>
        /// <param name="camera">要移除的相机。</param>
        public void RemoveCamera(Camera camera)
        {
            if (camera == null)
            {
                Debug.LogError("Attempted to remove a null camera from the stack.");
                return;
            }

            // 遍历所有堆栈，过滤掉要移除的摄像机
            foreach (var key in cameraQueues.Keys.ToList())
            {
                Queue<Camera> originalQueue = cameraQueues[key];
                Queue<Camera> newQueue = new();
                foreach (var cam in originalQueue)
                {
                    if (cam != camera)
                    {
                        newQueue.Enqueue(cam);
                    }
                }
                cameraQueues[key] = newQueue;
            }
            UpdateCameraStack();
        }

        /// <summary>
        /// 更新主相机的摄像机堆栈，按照 key 升序排列，保证顶部堆栈的相机渲染在其他相机之上。
        /// </summary>
        private void UpdateCameraStack()
        {
            if (MainCamera != null)
            {
                UniversalAdditionalCameraData mainCameraData = MainCamera.GetUniversalAdditionalCameraData();
                mainCameraData.cameraStack.Clear();
                foreach (var kvp in cameraQueues.OrderBy(kvp => kvp.Key))
                {
                    foreach (var cam in kvp.Value)
                    {
                        mainCameraData.cameraStack.Add(cam);
                    }
                }
            }
            else
            {
                Debug.LogError("Main camera is not set, cannot update camera stack.");
            }
        }

        /// <summary>
        /// 设置新添加的相机属性，使其作为覆盖层正确渲染。
        /// </summary>
        /// <param name="camera">要设置的相机。</param>
        private void SetupCamera(Camera camera)
        {
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            if (cameraData != null)
            {
                cameraData.renderType = CameraRenderType.Overlay;
            }
            else
            {
                Debug.LogError("Added camera does not have UniversalAdditionalCameraData component.");
            }
        }
        #endregion
    }
}