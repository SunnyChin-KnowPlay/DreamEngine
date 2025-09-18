using MysticIsle.DreamEngine.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// 面板展示模式
        /// </summary>
        private enum PanelShowMode { Push, Replace }

        /// <summary>
        /// 栈项：记录面板与展示模式
        /// </summary>
        private struct PanelEntry
        {
            public WidgetPanel Panel;
            public PanelShowMode Mode;
        }

        /// <summary>
        /// 面板栈：管理当前显示的面板顺序（包含展示模式）
        /// </summary>
        [ShowInInspector]
        private readonly Stack<PanelEntry> panelStack = new();

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
                if (top.Panel == null || !top.Panel.gameObject.activeSelf)
                {
                    panelStack.Pop();
                    if (panelStack.Count > 0)
                    {
                        var newTop = panelStack.Peek();
                        newTop.Panel?.Show();
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
        /// 设置面板归属当前UIManager，并确保显示在最前面
        /// </summary>
        /// <param name="go">面板对象</param>
        private void SetupPanel(WidgetPanel go)
        {
            go.transform.SetParent(this.transform, false);
            Control control = go.GetComponent<Control>();
            if (null != control)
                control.SetDirty();

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

        /// <summary>
        /// 推入面板：同步加载并显示，管理面板栈逻辑（记录为 Push）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public TControl PushPanel<TControl>() where TControl : Control
        {
            string path = Control.GetPath<TControl>();
            return PushPanelInternal<TControl>(path, PanelShowMode.Push);
        }

        /// <summary>
        /// 推入面板：带自定义面板路径（记录为 Push）
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public TControl PushPanel<TControl>(string path) where TControl : Control
        {
            return PushPanelInternal<TControl>(path, PanelShowMode.Push);
        }

        /// <summary>
        /// 统一的推入/替换逻辑：根据 mode 记录展示方式
        /// 规则：当有新面板进来（Push/Replace 都一样），若栈顶模式为 Replace，则先弹出栈顶，再压入新面板。
        /// </summary>
        private TControl PushPanelInternal<TControl>(string path, PanelShowMode mode) where TControl : Control
        {
            // 1. 加载面板
            WidgetPanel panelWidget = LoadPanel(path);
            if (panelWidget == null)
                return null;

            // 2. 确保面板持有正确的Control
            TControl control;
            if (!panelWidget.TryGetComponent(out Control existingPanel))
            {
                control = panelWidget.gameObject.AddComponent<TControl>();
            }
            else
            {
                if (existingPanel is not TControl)
                {
                    GameObject.Destroy(existingPanel);
                    control = panelWidget.gameObject.AddComponent<TControl>();
                }
                else
                {
                    control = existingPanel as TControl;
                }
            }

            // 3. 设置面板归属与排序
            SetupPanel(panelWidget);

            // 4. 面板栈逻辑
            // 4.1 新面板进来前，若栈顶是 Replace，则先弹出栈顶
            if (panelStack.Count > 0 && panelStack.Peek().Mode == PanelShowMode.Replace)
            {
                var popped = panelStack.Pop();
                popped.Panel?.Hide();
            }

            // 4.2 如果该面板已在栈中：弹出至该面板，并更新其展示模式
            bool existsInStack = false;
            foreach (var entry in panelStack)
            {
                if (entry.Panel == panelWidget)
                {
                    existsInStack = true;
                    break;
                }
            }

            if (existsInStack)
            {
                // 弹出直至目标面板在栈顶
                while (panelStack.Count > 0 && panelStack.Peek().Panel != panelWidget)
                {
                    var top = panelStack.Pop();
                    top.Panel?.Hide();
                }

                if (panelStack.Count > 0 && panelStack.Peek().Panel == panelWidget)
                {
                    // 更新该面板的展示模式为本次操作的 mode
                    var target = panelStack.Pop();
                    target.Mode = mode;
                    panelStack.Push(target);
                    panelWidget.Show();
                }
            }
            else
            {
                // 若是新面板，先隐藏当前栈顶（若有），再压入
                if (panelStack.Count > 0)
                {
                    panelStack.Peek().Panel?.Hide();
                }
                panelStack.Push(new PanelEntry { Panel = panelWidget, Mode = mode });
                panelWidget.Show();
            }

            return control;
        }

        /// <summary>
        /// 将面板栈弹出到只剩栈顶一个面板
        /// </summary>
        public void PopUntilTop()
        {
            if (panelStack.Count == 0)
            {
                Debug.LogWarning("无面板可回退至根面板！");
                return;
            }
            while (panelStack.Count > 1)
            {
                var entry = panelStack.Pop();
                entry.Panel?.Hide();
            }
            var top = panelStack.Peek();
            top.Panel?.Show();
        }

        /// <summary>
        /// 替换面板：移除当前栈顶面板后推入新面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public TControl ReplacePanel<TControl>() where TControl : Control
        {
            return ReplacePanel<TControl>(Control.GetPath<TControl>());
        }

        /// <summary>
        /// 替换面板：带自定义面板路径
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public TControl ReplacePanel<TControl>(string path) where TControl : Control
        {
            // 新逻辑：Replace 仅标记新面板为 Replace，不强制移除当前栈顶（除非栈顶本身就是 Replace）
            return PushPanelInternal<TControl>(path, PanelShowMode.Replace);
        }

        /// <summary>
        /// 清除所有面板（隐藏所有，并清空栈）
        /// </summary>
        public void ClearAllPanels()
        {
            while (panelStack.Count > 0)
            {
                var top = panelStack.Pop();
                top.Panel?.Hide();
            }
        }

        #endregion

        #region 面板管理 - 异步操作

        /// <summary>
        /// 异步加载面板
        /// </summary>
        /// <param name="path">面板路径</param>
        /// <returns>面板对象</returns>
        public async Task<WidgetPanel> LoadPanelAsync(string path)
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
        public async Task<TControl> ShowPanelAsync<TControl>() where TControl : Control
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
        public async Task<TControl> ShowPanelAsync<TControl>(string path) where TControl : Control
        {
            WidgetPanel widgetPanel = await LoadPanelAsync(path);
            if (widgetPanel == null)
                return null;

            if (!widgetPanel.TryGetComponent(out Control panel))
            {
                widgetPanel.gameObject.AddComponent<TControl>();
            }
            else if (panel.GetType() != typeof(TControl))
            {
                GameObject.Destroy(panel);
                widgetPanel.gameObject.AddComponent<TControl>();
            }

            widgetPanel.Show();
            SetupPanel(widgetPanel);
            widgetPanel.TryGetComponent(out TControl control);
            return control;
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
            TControl control = null;
            WidgetPanel panelWidget = LoadPanel(path);
            if (panelWidget == null)
                return control;

            if (!panelWidget.TryGetComponent<Control>(out Control panel))
            {
                panelWidget.gameObject.AddComponent<TControl>();
            }
            else if (panel.GetType() != typeof(TControl))
            {
                GameObject.Destroy(panel);
                panelWidget.gameObject.AddComponent<TControl>();
            }

            panelWidget.Show();
            SetupPanel(panelWidget);
            panelWidget.TryGetComponent<TControl>(out control);
            return control;
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