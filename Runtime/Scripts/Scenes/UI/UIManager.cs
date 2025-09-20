using Cysharp.Threading.Tasks;
using MysticIsle.DreamEngine.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 面板打开模式
    /// </summary>
    public enum OpenMode
    {
        /// <summary>
        /// 叠加：不关闭当前栈顶，直接在其上方叠加显示新面板
        /// </summary>
        Overlay = 0,
        /// <summary>
        /// 压栈：先关闭栈顶，再打开新面板，旧面板留在栈中（已关闭）
        /// </summary>
        Push,
        /// <summary>
        /// 替换：关闭并弹出栈顶，再打开新面板
        /// </summary>
        Replace,
    }



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
        /// 每个 sortingLayer 独享一个面板列表（尾部视为栈顶）
        /// </summary>
        [ShowInInspector]
        private readonly Dictionary<int, List<WidgetPanel>> layerPanelStacks = new();

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
        protected virtual void Update()
        {
            foreach (var layer in this.layerPanelStacks.Keys)
            {
                var list = this.layerPanelStacks[layer];
                if (list != null && list.Count > 0)
                {
                    var top = list[^1];
                    if (top == null || !top.IsActive)
                    {
                        list.RemoveAt(list.Count - 1);
                        if (list.Count > 0)
                        {
                            top = list[^1];
                            top.Show();
                        }
                    }
                }
            }
        }

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
        /// <summary>
        /// 打开面板（泛型，无需手动提供路径），按 sortingLayerId 分组管理栈。
        /// </summary>
        public virtual TControl Open<TControl>(OpenMode mode = OpenMode.Overlay) where TControl : Control
        {
            string path = Control.GetPath<TControl>();
            return Open<TControl>(path, mode);
        }

        /// <summary>
        /// 打开面板（带自定义路径），按 sortingLayerId 分组管理栈。
        /// </summary>
        public virtual TControl Open<TControl>(string path, OpenMode mode = OpenMode.Overlay) where TControl : Control
        {
            // 1) 加载并获取目标层的列表
            WidgetPanel panel = LoadPanel(path);
            if (panel == null)
                return null;

            int layerId = panel.Canvas ? panel.Canvas.sortingLayerID : this.Canvas.sortingLayerID;
            if (!layerPanelStacks.TryGetValue(layerId, out var list))
            {
                list = new List<WidgetPanel>();
                layerPanelStacks[layerId] = list;
            }

            WidgetPanel top = list.Count > 0 ? list[^1] : null;

            // 2) 如果已存在于列表中，分情况处理
            int idx = list.IndexOf(panel);
            if (idx >= 0)
            {
                if (idx == list.Count - 1)
                {
                    // 已是栈顶：重开（Hide->Show），并返回控制器
                    var ctrlTop = SetupPanel<TControl>(panel);
                    panel.Hide();
                    panel.Show();
                    return ctrlTop;
                }

                list.RemoveAt(idx);
            }

            switch (mode)
            {
                case OpenMode.Push:
                    if (top != null)
                        top.Hide();
                    break;
                case OpenMode.Replace:
                    if (top != null)
                    {
                        top.Hide();
                        if (list != null && list.Count > 0)
                            list.RemoveAt(list.Count - 1);
                    }
                    break;
                case OpenMode.Overlay:
                default:
                    break;
            }

            var ctrl = SetupPanel<TControl>(panel);
            AttachPanelToRoot(panel);
            panel.Show();
            list.Add(panel);
            return ctrl;

        }

        /// <summary>
        /// 关闭指定类型的面板：仅关闭并移除目标，不影响其他面板的显示状态。
        /// </summary>
        public virtual void Close<TControl>() where TControl : Control
        {
            string path = Control.GetPath<TControl>();
            Close(path);
        }

        /// <summary>
        /// 关闭指定路径的面板：仅关闭并移除目标，不影响其他面板的显示状态。
        /// </summary>
        public virtual void Close(string path)
        {
            WidgetPanel panel = GetPanel(path);
            if (panel == null)
                return;

            int layerId = panel.Canvas ? panel.Canvas.sortingLayerID : this.Canvas.sortingLayerID;
            if (!layerPanelStacks.TryGetValue(layerId, out var list) || list.Count == 0)
                return;

            int index = list.IndexOf(panel);
            if (index < 0)
                return;

            panel.Hide();
            list.RemoveAt(index);
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