using System.Collections.Generic;
using System.Linq;
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

        // 内部记录的当前激活的虚拟相机（用于切换时设置优先级）
        private CinemachineVirtualCameraBase activeCamera;

        #endregion

        #region Properties

        /// <summary>
        /// 获取只读的虚拟相机字典。
        /// </summary>
        public IReadOnlyDictionary<string, CinemachineVirtualCameraBase> VirtualCameras => virtualCameras;

        /// <summary>
        /// 通过返回 Priority 最高的虚拟相机来获取当前激活的虚拟相机。
        /// </summary>
        public CinemachineVirtualCameraBase ActiveCamera
        {
            get
            {
                if (virtualCameras.Count == 0)
                {
                    return null;
                }
                // 返回 Priority 最高的虚拟相机
                return virtualCameras.Values.OrderByDescending(cam => cam.Priority).First();
            }
        }

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
                // 默认将字典中第一个虚拟相机设为激活状态
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
        /// 通过调整虚拟相机的 Priority 属性来控制其激活状态，Priority 高的将优先激活，Priority 低的将失去激活效果。
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
            // 通过调整 Priority 来控制激活状态：将当前摄像机设为高优先级，其它摄像机设为低优先级
            foreach (var kvp in virtualCameras)
            {
                if (kvp.Value == vcam)
                {
                    kvp.Value.Priority = 10; // 高优先级
                }
                else
                {
                    kvp.Value.Priority = 0; // 低优先级
                }
            }
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