using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 面板开关动画类型
    /// </summary>
    [System.Flags]
    public enum EPanelSwitchAnimationFunction
    {
        None = 0,
        Alpha = 1 << 0,
        Blur = 1 << 1,
    }

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public partial class WidgetPanel : Widget
    {
        #region Params
        public RectTransform Root => root;
        [TitleGroup("Panel", "Widget", Order = 1), ShowInInspector, OdinSerialize, PropertyOrder(0)]
        private RectTransform root;

        [TitleGroup("Panel", "Widget"), ShowInInspector, ReadOnly, PropertyOrder(2)]
        public WidgetButton ExitButton => this.GetWidget<WidgetButton>("Root/ExitButton");
        [TitleGroup("Panel", "Widget"), ShowInInspector, ReadOnly, PropertyOrder(3)]
        public Widget TitleText => this.GetWidget("Root/TitleText");

        [FoldoutGroup("Panel/Animation Settings"), PropertyOrder(0)]
        public AnimationCurve alphaAnimationCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
        [FoldoutGroup("Panel/Animation Settings"), PropertyOrder(0)]
        public AnimationCurve blurAnimationCurve = new(new Keyframe(0, 0), new Keyframe(1, 10));

        [FoldoutGroup("Panel/Animation Settings"), LabelText("Show Animation Duration"), PropertyOrder(0)]
        public float showAnimationDuration = 0.3f;
        [FoldoutGroup("Panel/Animation Settings"), LabelText("Hide Animation Duration"), PropertyOrder(0)]
        public float hideAnimationDuration = 0.3f;

        [FoldoutGroup("Panel/Animation Settings"), EnumToggleButtons, PropertyOrder(0)]
        public EPanelSwitchAnimationFunction animationFunction;
        #endregion

        #region Mono
        protected override void Awake()
        {
            base.Awake();
            root = this.GetTransform(nameof(Root)) as RectTransform;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyEnter().Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }
        #endregion

        #region Switch
        protected override void OnHide()
        {
            ApplyExit().Forget();
        }
        #endregion

        #region Logic
        private bool CheckHasAnimationFunction(EPanelSwitchAnimationFunction function)
        {
            return (animationFunction & function) == function;
        }

        private bool CheckAnimationFunctionsIsEmpty()
        {
            return animationFunction == EPanelSwitchAnimationFunction.None;
        }
        #endregion

        #region Animation
        protected virtual async UniTaskVoid ApplyEnter()
        {
            if (!CheckAnimationFunctionsIsEmpty())
            {
                bool hasFunctionAlpha = CheckHasAnimationFunction(EPanelSwitchAnimationFunction.Alpha);
                hasFunctionAlpha &= root.TryGetComponent<CanvasGroup>(out CanvasGroup cg);

                bool hasFunctionBlur = CheckHasAnimationFunction(EPanelSwitchAnimationFunction.Blur);

                if (hasFunctionAlpha || hasFunctionBlur)
                {
                    float t = 0;
                    float speed = 1 / showAnimationDuration;
                    while (t < 1)
                    {
                        t += Time.unscaledDeltaTime * speed;
                        if (hasFunctionAlpha)
                        {
                            cg.alpha = Mathf.Min(alphaAnimationCurve.Evaluate(t), 1);
                        }
                        if (hasFunctionBlur)
                        {
                            // 示例：处理模糊动画
                        }
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }
                }
            }
        }

        protected virtual async UniTaskVoid ApplyExit()
        {
            if (!CheckAnimationFunctionsIsEmpty())
            {
                bool hasFunctionAlpha = CheckHasAnimationFunction(EPanelSwitchAnimationFunction.Alpha);
                hasFunctionAlpha &= root.TryGetComponent<CanvasGroup>(out CanvasGroup cg);

                bool hasFunctionBlur = CheckHasAnimationFunction(EPanelSwitchAnimationFunction.Blur);

                if (hasFunctionAlpha || hasFunctionBlur)
                {
                    float t = 1;
                    float speed = 1 / hideAnimationDuration;
                    while (t > 0)
                    {
                        t -= Time.unscaledDeltaTime * speed;
                        if (hasFunctionAlpha)
                        {
                            cg.alpha = Mathf.Min(alphaAnimationCurve.Evaluate(t), 1);
                        }
                        if (hasFunctionBlur)
                        {
                            // 示例：处理模糊动画
                        }
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }
                }
            }
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}