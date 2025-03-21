using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace MysticIsle.DreamEngine.Scenes
{
    /// <summary>
    /// Manages the virtual cameras in the game.
    /// 支持添加、移除、切换和获取当前激活的虚拟相机。
    /// 采用 Dictionary 存储虚拟相机，要求名称唯一。
    /// </summary>
    public class VirtualCameraManager : MonoBehaviour, IManager
    {
        #region Fields

        // 存储所有管理的虚拟相机（基于 Cinemachine），键为虚拟相机的名称
        private readonly Dictionary<string, CinemachineVirtualCameraBase> virtualCameras = new();

        // 当前激活的虚拟相机
        private CinemachineVirtualCameraBase activeCamera;

        #endregion

        #region Properties

        /// <summary>
        /// 获取只读的虚拟相机字典。
        /// </summary>
        public IReadOnlyDictionary<string, CinemachineVirtualCameraBase> VirtualCameras => virtualCameras;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 自动获取所有子物体上的虚拟相机（包括非激活状态）
            var cams = GetComponentsInChildren<CinemachineVirtualCameraBase>(true);
            if (cams != null && cams.Length > 0)
            {
                foreach (var cam in cams)
                {
                    string key = cam.gameObject.name;
                    if (!virtualCameras.ContainsKey(key))
                    {
                        virtualCameras.Add(key, cam);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate virtual camera name detected: {key}. Skipping camera.");
                    }
                }
                // 默认激活字典中的第一个虚拟相机
                foreach (var kvp in virtualCameras)
                {
                    SetActiveCamera(kvp.Value);
                    break;
                }
            }
        }

        #endregion

        #region Virtual Camera Management

        /// <summary>
        /// 添加虚拟相机到管理系统中，要求虚拟相机的名称唯一。
        /// </summary>
        /// <param name="vcam">要添加的 Cinemachine 虚拟相机实例。</param>
        public void AddVirtualCamera(CinemachineVirtualCameraBase vcam)
        {
            if (vcam == null)
            {
                Debug.LogError("Attempted to add a null virtual camera.");
                return;
            }

            string key = vcam.gameObject.name;
            if (virtualCameras.ContainsKey(key))
            {
                Debug.LogWarning($"Virtual camera with name '{key}' already exists in the manager.");
                return;
            }

            virtualCameras.Add(key, vcam);
            // 如果当前没有激活的虚拟相机，则设置添加的为激活状态
            if (activeCamera == null)
            {
                SetActiveCamera(vcam);
            }
        }

        /// <summary>
        /// 从管理系统中移除指定的虚拟相机。
        /// </summary>
        /// <param name="vcam">要移除的 Cinemachine 虚拟相机实例。</param>
        public void RemoveVirtualCamera(CinemachineVirtualCameraBase vcam)
        {
            if (vcam == null)
            {
                Debug.LogError("Attempted to remove a null virtual camera.");
                return;
            }

            string key = vcam.gameObject.name;
            if (virtualCameras.ContainsKey(key))
            {
                // 如果被移除的虚拟相机当前处于激活状态，则清空激活引用
                if (activeCamera == vcam)
                {
                    activeCamera = null;
                }
                virtualCameras.Remove(key);
            }
            else
            {
                Debug.LogWarning($"Virtual camera '{key}' not found in the manager.");
            }
        }

        /// <summary>
        /// 设置当前激活的虚拟相机。
        /// 只有激活的虚拟相机会被启用，其他的将被禁用。
        /// </summary>
        /// <param name="vcam">要激活的虚拟相机实例。</param>
        public void SetActiveCamera(CinemachineVirtualCameraBase vcam)
        {
            if (vcam == null)
            {
                Debug.LogError("Attempted to activate a null virtual camera.");
                return;
            }

            if (!virtualCameras.ContainsKey(vcam.gameObject.name))
            {
                Debug.LogWarning("Virtual camera is not managed by this manager.");
                return;
            }

            activeCamera = vcam;
            // 只激活当前选中的虚拟相机，禁用其他虚拟相机
            foreach (var kvp in virtualCameras)
            {
                kvp.Value.gameObject.SetActive(kvp.Value == vcam);
            }
        }

        /// <summary>
        /// 获取当前激活的虚拟相机。
        /// </summary>
        /// <returns>当前激活的 Cinemachine 虚拟相机实例。</returns>
        public CinemachineVirtualCameraBase GetActiveCamera()
        {
            return activeCamera;
        }

        /// <summary>
        /// 根据虚拟相机名称切换激活的虚拟相机。
        /// </summary>
        /// <param name="cameraKey">虚拟相机名称（必须唯一）。</param>
        public void SwitchCamera(string cameraKey)
        {
            if (string.IsNullOrEmpty(cameraKey))
            {
                Debug.LogWarning("Camera key is null or empty. Unable to switch camera.");
                return;
            }

            if (!virtualCameras.ContainsKey(cameraKey))
            {
                Debug.LogWarning($"Virtual camera with name '{cameraKey}' not found.");
                return;
            }

            SetActiveCamera(virtualCameras[cameraKey]);
        }

        #endregion
    }
}