using Sirenix.OdinInspector; // 引入 Odin Inspector 命名空间
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 按钮信息结构体
    /// </summary>
    public struct ButtonInfo
    {
        /// <summary>
        /// 按钮文本
        /// </summary>
        public string text;

        /// <summary>
        /// 按钮点击事件
        /// </summary>
        public Widget.OnWidgetHandle onClick;

        /// <summary>
        /// 按钮图标名称
        /// </summary>
        public string spriteName;

        /// <summary>
        /// 构造函数，初始化按钮信息
        /// </summary>
        /// <param name="text">按钮文本</param>
        /// <param name="onClick">按钮点击事件</param>
        /// <param name="spriteName">按钮图标名称</param>
        public ButtonInfo(string text, Widget.OnWidgetHandle onClick, string spriteName)
        {
            this.text = text;
            this.onClick = onClick;
            this.spriteName = spriteName;
        }
    }


    [RequireComponent(typeof(Button))]
    public class WidgetButton : Widget
    {
        #region Params
        [TitleGroup("Button", "Widget", Order = 1), ShowInInspector, PropertyOrder(-1)]
        public bool Interactable
        {
            get
            {
                if (null == button)
                    return false;
                return button.interactable;
            }

            set
            {
                if (null != button)
                {
                    button.interactable = value;
                    UpdateButtonShader();
                }
            }
        }
        [TitleGroup("Button", "Widget")]
        public string titleText;


        [TitleGroup("Button", "Widget"), ReadOnly, ShowInInspector]
        public Button Button => button;
        private Button button;
        [TitleGroup("Button", "Widget"), ShowInInspector]
        private Image image;
        public TMP_Text Text => text;
        [TitleGroup("Button", "Widget"), ShowInInspector, OdinSerialize]
        private TMP_Text text;

        [TitleGroup("Button", "Widget")]
        public ButtonClickedEvent onClick;

        private float targetAmount;
        private float grayAmount;
        private const float transitionSpeed = 2f; // Adjust the speed of transition
        #endregion

        #region Mono
        protected override void Awake()
        {
            base.Awake();

            button = GetComponent<Button>();
            button.onClick.AddListener(OnClicked);

            image = GetComponent<Image>();

            if (null == text)
            {
                text = this.GetComponent<TMP_Text>(nameof(Text));
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Refresh();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            this.Refresh();
        }

        private void Update()
        {
            grayAmount = Mathf.Lerp(grayAmount, targetAmount, Time.deltaTime * transitionSpeed);
            image.material.SetFloat("_GrayAmount", grayAmount);
        }
        #endregion

        #region Logic
        public void Refresh()
        {
            if (null != text)
            {
                this.text.text = titleText;
            }
        }

        public void SetTitle(string title)
        {
            this.titleText = title;
            this.Refresh();
        }

        public void SetBackground(Sprite sprite)
        {
            this.button.image.sprite = sprite;
        }

        private void OnClicked()
        {
            onClick?.Invoke();
            InvokeOnClick(this);
        }

        private void UpdateButtonShader()
        {
            targetAmount = button.interactable ? 0f : 1f;

            if (!Application.isPlaying)
            {
                grayAmount = targetAmount;
                image.material.SetFloat("_GrayAmount", grayAmount);
            }
        }

        [TitleGroup("Button", "Widget")]
        [Button("Click"), GUIColor(0, 0.7f, 0.7f)]
        public void Click()
        {
            if (null != button)
            {
                button.onClick.Invoke();
            }
        }
        #endregion

        #region UI Event
        public override void OnPointerClick(PointerEventData eventData)
        {

        }
        #endregion
    }
}
