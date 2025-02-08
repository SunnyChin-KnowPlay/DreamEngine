using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public abstract class UIManager : MonoBehaviour, IManager
    {
        #region Fields

        /// <summary>
        /// panel栈
        /// </summary>
        [ShowInInspector]
        private readonly Stack<WidgetPanel> panelStack = new();

        /// <summary>
        /// 界面词典 界面有唯一性
        /// key:界面路径
        /// </summary>
        [ShowInInspector]
        private readonly Dictionary<string, WidgetPanel> panels = new();

        #endregion

        #region Properties

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Unity的Awake方法
        /// </summary>
        protected virtual void Awake()
        {

        }

        /// <summary>
        /// Unity的OnDestroy方法
        /// </summary>
        protected virtual void OnDestroy()
        {
            this.UnloadAllPanels();
        }

        /// <summary>
        /// 更新方法
        /// </summary>
        protected virtual void Update()
        {
            if (panelStack.Count > 0)
            {
                var topPanel = panelStack.Peek();
                // 如果栈顶面板被关闭了，就将其弹出
                if (!topPanel.gameObject.activeSelf)
                {
                    panelStack.Pop();
                    // 可选：如果弹出后栈还有面板，则显示新的栈顶
                    if (panelStack.Count > 0)
                    {
                        panelStack.Peek().Show();
                    }
                }
            }
        }

        #endregion

        #region Panel Management

        /// <summary>
        /// 设置面板
        /// </summary>
        /// <param name="go">面板对象</param>
        private void SetupPanel(WidgetPanel go)
        {
            go.transform.SetParent(this.transform, false);
        }

        /// <summary>
        /// 获取面板
        /// </summary>
        /// <param name="path">面板路径</param>
        /// <returns>面板对象</returns>
        public WidgetPanel GetPanel(string path)
        {
            this.panels.TryGetValue(path, out WidgetPanel panel);
            return panel;
        }

        /// <summary>
        /// 获取面板
        /// </summary>
        /// <typeparam name="TPanel">面板类型</typeparam>
        /// <returns>面板对象</returns>
        public TPanel GetPanel<TPanel>() where TPanel : PanelControl
        {
            string path = PanelControl.GetPath<TPanel>();
            WidgetPanel panel = GetPanel(path);
            if (panel != null)
            {
                return panel.GetComponent<TPanel>();
            }
            return null;
        }

        /// <summary>
        /// 加载面板
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
                    this.panels.Add(path, panel);
                }
            }

            return panel;
        }

        /// <summary>
        /// 卸载面板
        /// </summary>
        /// <param name="path">面板路径</param>
        public void UnloadPanel(string path)
        {
            WidgetPanel panel = GetPanel(path);

            if (panel != null)
            {
                this.panels.Remove(path);
                Helper.DestroyGameObject(panel.gameObject);
            }
        }

        /// <summary>
        /// 卸载所有面板
        /// </summary>
        public void UnloadAllPanels()
        {
            this.panelStack.Clear();
            foreach (var kvp in panels)
            {
                Helper.DestroyGameObject(kvp.Value.gameObject);
            }
            this.panels.Clear();
        }

        /// <summary>
        /// 推入面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public TControl PushPanel<TControl>() where TControl : PanelControl
        {
            string path = PanelControl.GetPath<TControl>();
            return PushPanel<TControl>(path);
        }

        /// <summary>
        /// 推入面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public TControl PushPanel<TControl>(string path) where TControl : PanelControl
        {
            // 1. 先尝试加载面板
            WidgetPanel panelWidget = this.LoadPanel(path);
            if (panelWidget == null)
                return null;

            TControl control;
            // 2. 处理面板控件类型：若没有PanelControl则添加，若类型不匹配则销毁后重新添加
            if (!panelWidget.TryGetComponent<PanelControl>(out PanelControl existingPanel))
            {
                // 如果没有任何PanelControl，就直接添加TControl
                control = panelWidget.gameObject.AddComponent<TControl>();
            }
            else
            {
                // 已存在PanelControl，但类型不是TControl
                if (existingPanel is not TControl)
                {
                    GameObject.Destroy(existingPanel);
                    control = panelWidget.gameObject.AddComponent<TControl>();
                }
                else
                {
                    // 已经是正确类型
                    control = existingPanel as TControl;
                }
            }
            // 3. 初始化面板
            SetupPanel(panelWidget);

            // 4. 处理面板在堆栈中的逻辑
            if (panelStack.Contains(panelWidget))
            {
                // 如果该面板在堆栈里，就弹出至该面板为止，并显示它
                while (panelStack.Count > 0 && panelStack.Peek() != panelWidget)
                {
                    var topPanel = panelStack.Pop();
                    topPanel.Hide();
                }

                // 将目标面板显示
                if (panelStack.Count > 0 && panelStack.Peek() == panelWidget)
                {
                    panelWidget.Show();
                }
            }
            else
            {
                // 如果堆栈顶部有别的面板，则先Hide它，再Push新面板并Show
                if (panelStack.Count > 0)
                {
                    var topPanel = panelStack.Peek();
                    topPanel.Hide();
                }

                panelStack.Push(panelWidget);
                panelWidget.Show();
            }

            return control;
        }

        /// <summary>
        /// Pops all panels off the stack except the top one.
        /// </summary>
        public void PopUntilTop()
        {
            if (panelStack.Count == 0)
            {
                Debug.LogWarning("No panels to pop to root!");
                return;
            }

            // 只要栈中超过1个面板，就不断Pop并Hide
            while (panelStack.Count > 1)
            {
                var panel = panelStack.Pop();
                if (panel != null)
                {
                    panel.Hide();
                }
            }

            // 栈顶面板始终保持并Show
            var topPanel = panelStack.Peek();
            if (topPanel != null)
            {
                topPanel.Show();
            }
        }

        /// <summary>
        /// 替换面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public TControl ReplacePanel<TControl>() where TControl : PanelControl
        {
            return ReplacePanel<TControl>(PanelControl.GetPath<TControl>());
        }

        /// <summary>
        /// 替换面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public TControl ReplacePanel<TControl>(string path) where TControl : PanelControl
        {
            if (panelStack.Count > 0)
            {
                WidgetPanel topPanel = panelStack.Pop();
                GameObject.Destroy(topPanel.gameObject);
            }

            return PushPanel<TControl>(path);
        }

        /// <summary>
        /// 弹出所有面板
        /// </summary>
        public void ClearAllPanels()
        {
            while (panelStack.Count > 0)
            {
                WidgetPanel topPanel = panelStack.Pop();
                topPanel?.Hide();
            }
        }

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
                    this.panels.Add(path, panel);
                }
            }

            return panel;
        }

        /// <summary>
        /// 异步显示面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public async Task<TControl> ShowPanelAsync<TControl>() where TControl : PanelControl
        {
            string path = PanelControl.GetPath<TControl>();
            return await this.ShowPanelAsync<TControl>(path);
        }

        /// <summary>
        /// 异步显示面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public async Task<TControl> ShowPanelAsync<TControl>(string path) where TControl : PanelControl
        {
            WidgetPanel widgetPanel = await this.LoadPanelAsync(path);
            if (widgetPanel == null)
                return null;

            if (!widgetPanel.TryGetComponent<PanelControl>(out PanelControl panel))
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

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <returns>面板控制对象</returns>
        public TControl ShowPanel<TControl>() where TControl : PanelControl
        {
            return ShowPanel<TControl>(PanelControl.GetPath<TControl>());
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <typeparam name="TControl">面板控制类型</typeparam>
        /// <param name="path">面板路径</param>
        /// <returns>面板控制对象</returns>
        public TControl ShowPanel<TControl>(string path) where TControl : PanelControl
        {
            TControl control = null;
            WidgetPanel panelWidget = this.LoadPanel(path);
            if (panelWidget == null)
                return control;

            if (!panelWidget.TryGetComponent<PanelControl>(out PanelControl panel))
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
        /// 隐藏面板
        /// </summary>
        /// <param name="path">面板路径</param>
        public void HidePanel(string path)
        {
            WidgetPanel panel = this.GetPanel(path);
            if (null != panel)
            {
                panel.Hide();
            }
        }

        /// <summary>
        /// 检查面板栈是否为空
        /// </summary>
        /// <returns>是否为空</returns>
        protected virtual bool IsPanelStackEmpty()
        {
            return panelStack.Count == 0;
        }

        #endregion
    }
}