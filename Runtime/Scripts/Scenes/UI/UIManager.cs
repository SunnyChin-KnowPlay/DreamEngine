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
        /// 面板栈：管理当前显示的面板顺序
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
                var topPanel = panelStack.Peek();
                // 如果栈顶面板已关闭，则弹出；若栈不为空则显示新的栈顶
                if (!topPanel.gameObject.activeSelf)
                {
                    panelStack.Pop();
                    if (panelStack.Count > 0)
                    {
                        panelStack.Peek().Show();
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
        /// 设置面板归属当前UIManager
        /// </summary>
        /// <param name="go">面板对象</param>
        private void SetupPanel(WidgetPanel go)
        {
            go.transform.SetParent(this.transform, false);
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
        /// 推入面板：同步加载并显示，管理面板栈逻辑
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public TControl PushPanel<TControl>() where TControl : Control
        {
            string path = Control.GetPath<TControl>();
            return PushPanel<TControl>(path);
        }

        /// <summary>
        /// 推入面板：带自定义面板路径
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public TControl PushPanel<TControl>(string path) where TControl : Control
        {
            // 1. 加载面板
            WidgetPanel panelWidget = LoadPanel(path);
            if (panelWidget == null)
                return null;

            TControl control;
            // 2. 确保面板持有正确的Control
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

            // 3. 设置面板归属
            SetupPanel(panelWidget);

            // 4. 面板栈逻辑：如果面板已存在栈中则回退至该面板，否则隐藏当前并新推入
            if (panelStack.Contains(panelWidget))
            {
                while (panelStack.Count > 0 && panelStack.Peek() != panelWidget)
                {
                    var topPanel = panelStack.Pop();
                    topPanel.Hide();
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
                    panelStack.Peek().Hide();
                }
                panelStack.Push(panelWidget);
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
                var panel = panelStack.Pop();
                panel?.Hide();
            }
            panelStack.Peek()?.Show();
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
            if (panelStack.Count > 0)
            {
                WidgetPanel topPanel = panelStack.Pop();
                GameObject.Destroy(topPanel.gameObject);
            }
            return PushPanel<TControl>(path);
        }

        /// <summary>
        /// 清除所有面板（隐藏所有，并清空栈）
        /// </summary>
        public void ClearAllPanels()
        {
            while (panelStack.Count > 0)
            {
                WidgetPanel topPanel = panelStack.Pop();
                topPanel?.Hide();
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

        #region 面板管理 - 同步显示操作

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

        #endregion
    }
}