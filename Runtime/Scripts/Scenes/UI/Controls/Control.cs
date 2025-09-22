using System;
using System.Reflection;
using UnityEngine;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 界面控制器
    /// </summary>
    [RequireComponent(typeof(Widget))]
    public partial class Control : MonoBehaviour
    {
        /// <summary>
        /// 获取控件路径
        /// </summary>
        /// <typeparam name="T">控件类型</typeparam>
        /// <returns>控件路径</returns>
        public static string GetPath<T>() where T : Control
        {
            return GetPath(typeof(T));
        }

        /// <summary>
        /// 获取控件路径
        /// </summary>
        /// <param name="type">控件类型</param>
        /// <returns>控件路径</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetPath(Type type)
        {
            // 获取路径
            var pathAttribute = type.GetCustomAttribute<PanelPathAttribute>();
            if (pathAttribute == null)
            {
                throw new InvalidOperationException($"Panel path not defined for {type.Name}");
            }
            return pathAttribute.Path;
        }

        #region Event
        // 控件事件定义
        public delegate void OnControlHandle(Control control);
        public OnControlHandle OnEnableEvent;  // 控件启用时触发
        public OnControlHandle OnDisableEvent; // 控件禁用时触发
        #endregion

        #region Params
        /// <summary>
        /// 首要的小部件（当前控制器关联的 Widget）
        /// </summary>
        private Widget firstWidget;

        /// <summary>
        /// 是否为激活状态
        /// </summary>
        public bool IsActive => this.gameObject != null && this.gameObject.activeSelf;
        #endregion

        #region Mono
        /// <summary>
        /// Awake 在脚本实例加载时调用。用于初始化 FirstWidget 并绑定引用。
        /// </summary>
        protected virtual void Awake()
        {
            // 初始化 firstWidget
            _ = FirstWidget;
            // 绑定引用（子类可在 OnBindReferences 中实现具体逻辑）
            OnBindReferences();
        }

        /// <summary>
        /// OnEnable 控件启用时调用，此时通知外部 OnEnableEvent。
        /// </summary>
        protected virtual void OnEnable()
        {
            OnEnableEvent?.Invoke(this);
        }

        /// <summary>
        /// OnDisable 控件禁用时调用，此时通知外部 OnDisableEvent，并取消焦点状态。
        /// </summary>
        protected virtual void OnDisable()
        {
            OnDisableEvent?.Invoke(this);
        }

        /// <summary>
        /// OnDestroy 在控件销毁时调用，可用于清理资源（默认实现为空）。
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 预留：销毁前的清理逻辑
        }

        /// <summary>
        /// Update 每帧调用，如果控件为脏状态，则执行刷新操作。
        /// </summary>
        protected virtual void Update()
        {
            if (isDirty)
            {
                OnRefresh();
                isDirty = false;
            }
        }
        #endregion

        #region Getter
        /// <summary>
        /// 获取或初始化本控件关联的 Widget（首要小部件）。
        /// </summary>
        public Widget FirstWidget
        {
            get
            {
                if (firstWidget == null)
                {
                    firstWidget = GetComponent<Widget>();
                }
                return firstWidget;
            }
        }

        /// <summary>
        /// 获取界面管理器（UIManager），通过关联的 Widget 获取。
        /// </summary>
        public UIManager UIManager => FirstWidget.UIManager;

        /// <summary>
        /// 用于根据路径获取指定类型的组件，封装自 Widget 的绑定方法。
        /// </summary>
        protected virtual T GetComponent<T>(string path) where T : Component
        {
            return FirstWidget.GetComponent<T>(path);
        }

        /// <summary>
        /// 尝试根据路径获取指定类型的组件，封装自 Widget 的绑定方法；返回获取是否成功。
        /// </summary>
        protected virtual bool TryGetComponent<T>(string path, out T component) where T : Component
        {
            return FirstWidget.TryGetComponent<T>(path, out component);
        }

        /// <summary>
        /// 获取或添加组件，当组件不存在时自动添加；封装自 Widget 的逻辑。
        /// </summary>
        protected virtual T GetOrAddComponent<T>(string path) where T : Component
        {
            return FirstWidget.GetOrAddComponent<T>(path);
        }
        #endregion

        #region Switch
        /// <summary>
        /// 当前控件显示状态（是否显示）。
        /// </summary>
        public bool IsShowing => gameObject.activeSelf;

        /// <summary>
        /// 关闭控件，默认实现调用关联 Widget 的隐藏方法。
        /// </summary>
        public virtual void Close()
        {
            FirstWidget.Hide();
        }
        #endregion

        #region Refresh
        // 内部标记，表示控件数据已修改，需要刷新显示
        private bool isDirty = false;

        /// <summary>
        /// 将控件设置为脏状态，下一帧会触发刷新操作。
        /// </summary>
        public void SetDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// 当控件刷新时调用的方法。
        /// 子类可以重写此方法以实现自定义刷新逻辑更新显示。
        /// </summary>
        protected virtual void OnRefresh()
        {
            // 默认实现为空
        }
        #endregion

        #region Binding
        /// <summary>
        /// 绑定引用。子类可以在此方法中实现具体的引用绑定逻辑。
        /// </summary>
        protected virtual void OnBindReferences()
        {
            // 默认实现为空
        }
        #endregion
    }
}