using Cysharp.Threading.Tasks;
using MysticIsle.DreamEngine.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// UI管理器
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public abstract class UIManager : MonoBehaviour, IManager
    {
        #region 字段与属性

        /// <summary>
        /// 界面词典：每个面板路径对应唯一的面板
        /// </summary>
        [ShowInInspector]
        private readonly Dictionary<string, WidgetPanel> panels = new();

        /// <summary>
        /// 当前Canvas组件
        /// </summary>
        public Canvas Canvas => this.GetComponent<Canvas>();

        private int sortingLayerId = 0;
        /// <summary>
        /// 相机排序层级ID，设置后会更新当前Canvas的相机
        /// </summary>
        public int SortingLayerId
        {
            get => sortingLayerId;
            set
            {
                if (sortingLayerId != value)
                {
                    sortingLayerId = value;
                    RefreshCameraSorting();
                }
            }
        }

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// Unity的Awake方法
        /// </summary>
        protected virtual void Awake()
        {

        }

        /// <summary>
        /// Unity的OnEnable方法
        /// </summary>
        protected virtual void OnEnable()
        {
            Canvas canvas = this.Canvas;
            if (canvas != null && canvas.worldCamera != null)
                CameraManager.Instance.AddCamera(canvas.worldCamera, SortingLayerId);
        }

        /// <summary>
        /// Unity的OnDisable方法
        /// </summary>
        protected virtual void OnDisable()
        {
            Canvas canvas = this.Canvas;
            if (canvas != null && canvas.worldCamera != null)
                CameraManager.Instance?.RemoveCamera(canvas.worldCamera);
        }

        /// <summary>
        /// Unity的OnDestroy方法，销毁前卸载所有面板
        /// </summary>
        protected virtual void OnDestroy()
        {
            this.UnloadAllPanels();
        }

        // 基类不处理每帧栈检查；派生类可按需覆盖 Update。

        /// <summary>
        /// 刷新相机排序：先移除当前Canvas的相机，再用新的排序层级添加
        /// </summary>
        private void RefreshCameraSorting()
        {
            Canvas canvas = this.Canvas;
            if (canvas != null && canvas.worldCamera != null)
            {
                CameraManager.Instance.RemoveCamera(canvas.worldCamera);
                CameraManager.Instance.AddCamera(canvas.worldCamera, sortingLayerId);
            }
        }

        #endregion

        #region 面板管理

        /// <summary>
        /// 将面板挂载到本 UIManager 根节点并置顶排序（父子关系 + 排序）
        /// </summary>
        /// <param name="go">面板对象</param>
        protected void AttachPanelToRoot(WidgetPanel go)
        {
            go.transform.SetParent(this.transform, false);

            // 安全地处理面板层级：使用Canvas的sortingOrder而不是简单的transform层级
            if (go.TryGetComponent<Canvas>(out var panelCanvas))
            {
                // 确保面板Canvas启用Override Sorting
                panelCanvas.overrideSorting = true;

                // 找到当前最高的sortingOrder
                int maxSortingOrder = 0;
                foreach (var panel in panels.Values)
                {
                    if (panel != null && panel != go)
                    {
                        Canvas canvas = panel.GetComponent<Canvas>();
                        if (canvas != null && canvas.overrideSorting)
                        {
                            maxSortingOrder = Mathf.Max(maxSortingOrder, canvas.sortingOrder);
                        }
                    }
                }

                // 设置当前面板为最高层级
                panelCanvas.sortingOrder = maxSortingOrder + 1;
            }
            else
            {
                // 如果没有Canvas组件，回退到transform层级
                go.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 设置面板并确保其持有正确的 Control 组件（泛型版本）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="go">面板对象</param>
        /// <returns>确保存在并返回的控制脚本</returns>
        protected TControl SetupPanel<TControl>(WidgetPanel go) where TControl : Control
        {
            // 先执行通用的父子关系与排序设置
            AttachPanelToRoot(go);

            // 再确保面板持有正确的 Control 组件
            TControl ctrl;
            if (!go.TryGetComponent(out Control existingPanel))
            {
                ctrl = go.gameObject.AddComponent<TControl>();
            }
            else if (existingPanel is not TControl)
            {
                GameObject.Destroy(existingPanel);
                ctrl = go.gameObject.AddComponent<TControl>();
            }
            else
            {
                ctrl = existingPanel as TControl;
            }

            // 确保正确的 Control 后再标记脏
            ctrl?.SetDirty();
            return ctrl;
        }

        /// <summary>
        /// 获取已有面板
        /// </summary>
        /// <param name="path">面板路径</param>
        /// <returns>面板对象</returns>
        public WidgetPanel GetPanel(string path)
        {
            panels.TryGetValue(path, out WidgetPanel panel);
            return panel;
        }

        /// <summary>
        /// 泛型获取面板
        /// </summary>
        /// <typeparam name="TPanel">面板类型</typeparam>
        /// <returns>面板对象</returns>
        public TPanel GetPanel<TPanel>() where TPanel : Control
        {
            string path = Control.GetPath<TPanel>();
            WidgetPanel panel = GetPanel(path);
            if (panel != null)
            {
                return panel.GetComponent<TPanel>();
            }
            return null;
        }

        /// <summary>
        /// 加载面板（同步）
        /// </summary>
        /// <param name="path">面板路径</param>
        /// <returns>面板对象</returns>
        public WidgetPanel LoadPanel(string path)
        {
            WidgetPanel panel = GetPanel(path);
            if (panel == null)
            {
                GameObject go = Helper.InitializeGameObject(path);
                if (go != null)
                {
                    panel = go.GetComponent<WidgetPanel>();
                    panels.Add(path, panel);
                }
            }
            return panel;
        }

        /// <summary>
        /// 卸载指定面板
        /// </summary>
        /// <param name="path">面板路径</param>
        public void UnloadPanel(string path)
        {
            WidgetPanel panel = GetPanel(path);
            if (panel != null)
            {
                panels.Remove(path);
                Helper.DestroyGameObject(panel.gameObject);
            }
        }

        /// <summary>
        /// 卸载所有面板并清空面板栈
        /// </summary>
        private void UnloadAllPanels()
        {
            foreach (var kvp in panels)
            {
                Helper.DestroyGameObject(kvp.Value.gameObject);
            }
            panels.Clear();
        }

        /// <summary>
        /// 异步加载面板
        /// </summary>
        /// <param name="path">面板路径</param>
        /// <returns>面板对象</returns>
        public async UniTask<WidgetPanel> LoadPanelAsync(string path)
        {
            WidgetPanel panel = GetPanel(path);
            if (panel == null)
            {
                var go = await Helper.InitializeGameObjectAsync(path);
                if (go != null)
                {
                    panel = go.GetComponent<WidgetPanel>();
                    panels.Add(path, panel);
                }
            }
            return panel;
        }

        /// <summary>
        /// 同步显示面板（泛型，无需手动提供路径）。
        /// 默认实现：加载 + 挂载/排序 + 确保 Control + 调用面板 Show。
        /// 导航/栈/层逻辑建议在派生类中通过 Open 等接口实现。
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象（可能为 null）</returns>
        public virtual TControl ShowPanel<TControl>() where TControl : Control
        {
            string path = Control.GetPath<TControl>();
            return ShowPanel<TControl>(path);
        }

        /// <summary>
        /// 同步显示面板（带自定义路径）。
        /// 默认实现：加载 + 挂载/排序 + 确保 Control + 调用面板 Show。
        /// 导航/栈/层逻辑建议在派生类中通过 Open 等接口实现。
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象（可能为 null）</returns>
        public virtual TControl ShowPanel<TControl>(string path) where TControl : Control
        {
            WidgetPanel widgetPanel = LoadPanel(path);
            if (widgetPanel == null)
                return null;

            var ctrl = SetupPanel<TControl>(widgetPanel);
            widgetPanel.Show();
            return ctrl;
        }

        #endregion

        #region Widget管理
        public virtual void OnWidgetFocusIn(Widget widget)
        {
            // 处理Widget获得焦点的逻辑
        }

        public virtual void OnWidgetFocusOut(Widget widget)
        {
            // 处理Widget失去焦点的逻辑
        }
        #endregion
    }
}