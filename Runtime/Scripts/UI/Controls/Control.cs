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
            firstWidget = GetComponent<Widget>();
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
        #endregion

        #region Getter
        public Widget FirstWidget => firstWidget;

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
    }
}
