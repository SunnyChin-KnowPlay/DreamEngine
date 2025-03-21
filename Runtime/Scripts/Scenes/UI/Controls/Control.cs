using UnityEngine;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 界面控制器
    /// </summary>
    [RequireComponent(typeof(Widget))]
    public partial class Control : MonoBehaviour
    {
        #region Event
        public delegate void OnControlHandle(Control control);

        public OnControlHandle OnEnableEvent;
        public OnControlHandle OnDisableEvent;
        #endregion

        #region Params
        /// <summary>
        /// 首要的小部件
        /// </summary>
        private Widget firstWidget;

        /// <summary>
        /// 是否为激活状态
        /// </summary>
        public bool IsActive => null != this.gameObject && this.gameObject.activeSelf;
        #endregion

        #region Mono
        protected virtual void Awake()
        {
            _ = FirstWidget;
        }

        protected virtual void OnEnable()
        {
            OnEnableEvent?.Invoke(this);
        }

        protected virtual void OnDisable()
        {
            OnDisableEvent?.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 在每帧检查是否处于脏状态，如果是则调用刷新函数。
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
        public Widget FirstWidget
        {
            get
            {
                if (null == firstWidget)
                {
                    firstWidget = GetComponent<Widget>();
                }
                return firstWidget;
            }
        }

        public UIManager UIManager => FirstWidget.UIManager;

        /// <summary>
        /// 获取部件
        /// </summary>
        /// <typeparam name="TWidget"></typeparam>
        /// <returns></returns>
        public TWidget GetWidget<TWidget>() where TWidget : Widget
        {
            return GetComponent<TWidget>();
        }

        protected virtual T GetComponent<T>(string path) where T : Component
        {
            return this.FirstWidget.GetComponent<T>(path);
        }

        protected virtual bool TryGetComponent<T>(string path, out T component) where T : Component
        {
            return this.FirstWidget.TryGetComponent<T>(path, out component);
        }

        protected virtual T GetOrAddComponent<T>(string path) where T : Component
        {
            return this.FirstWidget.GetOrAddComponent<T>(path);
        }
        #endregion

        #region Switch
        public bool IsShowing => gameObject.activeSelf;

        public virtual void Close()
        {
            this.FirstWidget.Hide();
        }
        #endregion

        #region Refresh
        // 内部标记表示控件数据已修改，需要刷新
        private bool isDirty = false;

        /// <summary>
        /// 设置控件为脏状态，下一帧将触发刷新操作。
        /// </summary>
        public void SetDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// 当控件刷新时调用此函数进行更新显示。
        /// 子类可重写该方法以实现自定义刷新逻辑。
        /// </summary>
        protected virtual void OnRefresh()
        {
            // 默认实现为空
        }
        #endregion
    }
}
