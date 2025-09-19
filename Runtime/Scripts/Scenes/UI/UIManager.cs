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
        /// 面板栈：管理当前显示的面板顺序（包含展示模式）
        /// </summary>
        [ShowInInspector]
        private readonly Stack<WidgetPanel> panelStack = new();

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

        /// <summary>
        /// Unity的Update方法：检查栈顶面板状态，并做相应处理
        /// </summary>
        protected virtual void Update()
        {
            if (panelStack.Count > 0)
            {
                var top = panelStack.Peek();
                // 如果栈顶面板已关闭，则弹出；若栈不为空则显示新的栈顶
                if (top == null || !top.gameObject.activeSelf)
                {
                    panelStack.Pop();
                    if (panelStack.Count > 0)
                    {
                        var newTop = panelStack.Peek();
                        newTop?.Show();
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

        #region 面板管理 - 基础操作

        /// <summary>
        /// 将面板挂载到本 UIManager 根节点并置顶排序（父子关系 + 排序）
        /// </summary>
        /// <param name="go">面板对象</param>
        private void AttachPanelToRoot(WidgetPanel go)
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
        private TControl SetupPanel<TControl>(WidgetPanel go) where TControl : Control
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

            var type = typeof(TControl);
            var attr = (PanelStackModeAttribute)System.Attribute.GetCustomAttribute(type, typeof(PanelStackModeAttribute));
            var mode = attr != null ? attr.Mode : PanelStackMode.Push;
            go.StackMode = mode;

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
        public void UnloadAllPanels()
        {
            panelStack.Clear();
            foreach (var kvp in panels)
            {
                Helper.DestroyGameObject(kvp.Value.gameObject);
            }
            panels.Clear();
        }

        #endregion

        #region 面板管理 - 栈操作

        // 统一的显示逻辑：根据面板的 StackMode 自动处理（Standalone/Push/Replace）
        private TControl ShowPanelCore<TControl>(WidgetPanel panelWidget) where TControl : Control
        {
            if (panelWidget == null)
                return null;

            // Push/Replace：参与栈
            // 先确保归属与 Control
            TControl control = SetupPanel<TControl>(panelWidget);

            // Standalone：独立显示，不参与栈
            if (panelWidget.StackMode == PanelStackMode.Standalone)
            {
                panelWidget.Show();
                return control;
            }

            // 入栈前：若栈顶是 Replace，则先弹出
            if (panelStack.Count > 0 && panelStack.Peek() != null && panelStack.Peek().StackMode == PanelStackMode.Replace)
            {
                var popped = panelStack.Pop();
                popped?.Hide();
            }

            // 已存在则回退至该面板并更新模式
            bool existsInStack = false;
            foreach (var entry in panelStack)
            {
                if (entry == panelWidget)
                {
                    existsInStack = true;
                    break;
                }
            }

            if (existsInStack)
            {
                while (panelStack.Count > 0 && panelStack.Peek() != panelWidget)
                {
                    var top = panelStack.Pop();
                    top?.Hide();
                }

                if (panelStack.Count > 0 && panelStack.Peek() == panelWidget)
                {
                    panelWidget.Show();
                }
            }
            else
            {
                if (panelStack.Count > 0)
                {
                    panelStack.Peek()?.Hide();
                }
                panelStack.Push(panelWidget);
                panelWidget.Show();
            }

            return control;
        }

        /// <summary>
        /// 兼容旧版：推入面板（已废弃，改用 ShowPanel）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        [System.Obsolete("Use ShowPanel instead; behavior now auto-determined by WidgetPanel.StackMode")]
        public TControl PushPanel<TControl>() where TControl : Control
        {
            return ShowPanel<TControl>(Control.GetPath<TControl>());
        }

        /// <summary>
        /// 替换面板：移除当前栈顶面板后推入新面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        [System.Obsolete("Use ShowPanel instead; behavior now auto-determined by WidgetPanel.StackMode")]
        public TControl ReplacePanel<TControl>() where TControl : Control
        {
            return ShowPanel<TControl>(Control.GetPath<TControl>());
        }

        /// <summary>
        /// 替换面板：带自定义面板路径
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        [System.Obsolete("Use ShowPanel instead; behavior now auto-determined by WidgetPanel.StackMode")]
        public TControl ReplacePanel<TControl>(string path) where TControl : Control
        {
            return ShowPanel<TControl>(path);
        }

        /// <summary>
        /// 清除所有面板（隐藏所有，并清空栈）
        /// </summary>
        public void ClearAllPanels()
        {
            while (panelStack.Count > 0)
            {
                var top = panelStack.Pop();
                top?.Hide();
            }
        }

        #endregion

        #region 面板管理 - 异步操作

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
        /// 异步展示面板（泛型）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public async UniTask<TControl> ShowPanelAsync<TControl>() where TControl : Control
        {
            string path = Control.GetPath<TControl>();
            return await ShowPanelAsync<TControl>(path);
        }

        /// <summary>
        /// 异步展示面板（带自定义路径）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public async UniTask<TControl> ShowPanelAsync<TControl>(string path) where TControl : Control
        {
            WidgetPanel widgetPanel = await LoadPanelAsync(path);
            if (widgetPanel == null)
                return null;

            return ShowPanelCore<TControl>(widgetPanel);
        }

        #endregion

        #region 面板管理

        /// <summary>
        /// 同步显示面板（泛型）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public TControl ShowPanel<TControl>() where TControl : Control
        {
            return ShowPanel<TControl>(Control.GetPath<TControl>());
        }

        /// <summary>
        /// 同步显示面板（带自定义路径）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public TControl ShowPanel<TControl>(string path) where TControl : Control
        {
            WidgetPanel widgetPanel = LoadPanel(path);
            if (widgetPanel == null)
                return null;

            return ShowPanelCore<TControl>(widgetPanel);
        }

        /// <summary>
        /// 隐藏指定面板
        /// </summary>
        /// <param name="path">面板路径</param>
        public void HidePanel(string path)
        {
            WidgetPanel panel = GetPanel(path);
            panel?.Hide();
        }

        /// <summary>
        /// 隐藏指定面板（泛型）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        public void HidePanel<TControl>() where TControl : Control
        {
            var panelControl = GetPanel<TControl>();
            if (panelControl != null)
            {
                var panel = panelControl.GetComponent<WidgetPanel>();
                panel.Hide();
            }
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