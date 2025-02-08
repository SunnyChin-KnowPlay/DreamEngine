using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MysticIsle.DreamEngine.Core;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 面板开关动画类型
    /// </summary>
    [System.Flags] // 添加 Flags 特性
    public enum EPanelSwitchAnimationFunction
    {
        /// <summary>
        /// 空
        /// </summary>
        None = 0,
        /// <summary>
        /// 透明度
        /// </summary>
        Alpha = 1 << 0,
        /// <summary>
        /// 模糊
        /// </summary>
        Blur = 1 << 1,
    }

    [ExecuteAlways]
    [RequireComponent(typeof(Canvas))]
    public class WidgetPanel : Widget
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

        // Odin 面板按钮添加预设摄像机（直接使用 Canvas.worldCamera，无需额外变量）
        [TitleGroup("Panel", "Widget Camera Settings", Order = 2)]
        [Button("Add Preset Camera", ButtonSizes.Medium)]
        private void AddPresetCamera()
        {
            Canvas canvas = GetComponent<Canvas>();

            if (canvas.worldCamera != null)
            {
                Debug.LogWarning("预设摄像机已存在，使用 Canvas.worldCamera: " + canvas.worldCamera.name);
                return;
            }

            // 尝试在子物体中查找现有摄像机
            Camera cam = GetComponentInChildren<Camera>();
            if (cam == null)
            {
                // 自动创建预设摄像机
                GameObject camObj = new("PanelCamera");
                camObj.transform.SetParent(transform, false);
                // 将摄像机的 Transform 排到最上面
                camObj.transform.SetSiblingIndex(0);

                cam = camObj.AddComponent<Camera>();

                // 设置摄像机默认参数
                cam.clearFlags = CameraClearFlags.Depth;
                cam.cullingMask = LayerMask.GetMask("UI");

                // 设置为正交模式，并将 near clip plane 设为 0
                cam.orthographic = true;
                cam.nearClipPlane = 0f;

                Debug.Log("自动添加预设摄像机: " + cam.name);
            }
            else
            {
                Debug.Log("在子物体中已找到摄像机: " + cam.name);
            }
            // 直接将摄像机赋值给 Canvas 的 worldCamera 属性
            canvas.worldCamera = cam;
            canvas.vertexColorAlwaysGammaSpace = true;
        }
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
            if (null != this.Canvas.worldCamera)
                CameraManager.Instance.AddCamera(this.Canvas.worldCamera, this.Canvas.sortingLayerID);
            ApplyEnter().Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (null != this.Canvas.worldCamera)
                CameraManager.Instance.RemoveCamera(this.Canvas.worldCamera);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            // 当在编辑模式下挂上脚本且当前 Canvas 没有设置 worldCamera 时自动添加
            if (!Application.isPlaying)
            {
                Canvas canvas = GetComponent<Canvas>();
                if (canvas != null && canvas.worldCamera == null)
                {
                    AddPresetCamera();
                }
            }
        }
#endif
        #endregion

        #region Switch
        protected override void OnHide()
        {
            ApplyExit().Forget();
        }
        #endregion

        #region Logic
        internal void SetSortingLayer(int sortingLayerID)
        {
            if (this.Canvas.sortingLayerID != sortingLayerID)
            {
                this.Canvas.sortingLayerID = sortingLayerID;
                if (null != this.Canvas.worldCamera)
                {
                    CameraManager.Instance.RemoveCamera(this.Canvas.worldCamera);
                    CameraManager.Instance.AddCamera(this.Canvas.worldCamera, sortingLayerID);
                }
            }
        }

        /// <summary>
        /// 检查是否包含动画功能
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        private bool CheckHasAnimationFunction(EPanelSwitchAnimationFunction function)
        {
            return (animationFunction & function) == function;
        }

        /// <summary>
        /// 检查是否不存在任何动画功能
        /// </summary>
        /// <returns></returns>
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
                //hasFunctionBlur &= null != this.BlurredBackgroundImage;

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
                            //BlurredBackgroundImage.Strength = blurAnimationCurve.Evaluate(t);
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
                //hasFunctionBlur &= null != this.BlurredBackgroundImage;

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
                            //BlurredBackgroundImage.Strength = blurAnimationCurve.Evaluate(t);
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
