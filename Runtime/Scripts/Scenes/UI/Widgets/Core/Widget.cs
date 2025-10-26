using Sirenix.OdinInspector; // Import Odin Inspector namespace
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace MysticIsle.DreamEngine.UI
{
    public partial class Widget : SerializedMonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        // Cache to avoid GC allocs when walking CanvasGroup hierarchy
        private static readonly List<CanvasGroup> s_CanvasGroupCache = new();
        // 封装一个 WidgetEvent，继承自 UnityEvent<Widget>
        [System.Serializable]
        public class WidgetEvent : UnityEvent<Widget> { }

        // 封装一个 WidgetDropEvent，继承自 UnityEvent<Widget, Widget>
        [System.Serializable]
        public class WidgetDropEvent : UnityEvent<Widget, Widget> { }

        #region Event
        [HideInInspector]
        public WidgetEvent OnClick = new(); // Called on single click

        [HideInInspector]
        public WidgetEvent OnDoubleClick = new(); // Called on double click

        [HideInInspector]
        public WidgetEvent OnFocusIn = new(); // Called when focus is gained

        [HideInInspector]
        public WidgetEvent OnFocusOut = new(); // Called when focus is lost

        [HideInInspector]
        public WidgetEvent OnSelected = new(); // Called when selection is gained

        [HideInInspector]
        public WidgetEvent OnDeselected = new(); // Called when selection is lost

        [HideInInspector]
        public WidgetDropEvent OnDropped = new(); // Called when widget is dropped

        [HideInInspector]
        public WidgetEvent OnPointerDownEvent = new(); // Called on pointer down

        [HideInInspector]
        public WidgetEvent OnPointerUpEvent = new(); // Called on pointer up
        #endregion

        #region Params
        /// <summary>
        /// Indicates if the widget is active.
        /// </summary>
        public bool IsActive => null != this.gameObject && this.gameObject.activeSelf;

        [TitleGroup("Core", nameof(Widget), Order = 0), ShowInInspector]
        public RectTransform RectTransform => rectTransform; // RectTransform of the widget
        protected RectTransform rectTransform;

        [TitleGroup("Core", nameof(Widget)), ShowInInspector, ReadOnly]
        public bool IsFocused
        {
            get => isFocused;
            protected set
            {
                if (isFocused != value)
                {
                    isFocused = value;
                    if (isFocused)
                    {
                        this.OnFocusIn?.Invoke(this);
                        if (null != UIManager)
                        {
                            UIManager.OnWidgetFocusIn(this);
                        }
                    }
                    else
                    {
                        this.OnFocusOut?.Invoke(this);
                        if (null != UIManager)
                        {
                            UIManager.OnWidgetFocusOut(this);
                        }
                    }
                }
            }
        }
        private bool isFocused = false; // Indicates if the widget is focused

        [TitleGroup("Core", nameof(Widget)), ShowInInspector]
        public virtual bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    if (isSelected)
                    {
                        OnSelected?.Invoke(this);
                    }
                    else
                    {
                        OnDeselected?.Invoke(this);
                    }
                }
            }
        }
        protected bool isSelected = false; // Indicates if the widget is selected

        /// <summary>
        /// Tooltip key for the widget.
        /// </summary>
        public string TooltipKey;

        public bool Draggable { get; set; } = false; // Indicates if the widget is draggable

        [Tooltip("为 true 时将指针事件转发给下一个命中对象；为 false 时不再向下转发")]
        public bool forwardPointerToNextTarget = false;

        public Canvas Canvas => GetComponentInParent<Canvas>();
        public CanvasGroup CanvasGroup => canvasGroup; // CanvasGroup for managing UI interactions
        private CanvasGroup canvasGroup;

        /// <summary>
        /// UI管理器
        /// </summary>
        public virtual UIManager UIManager => GetComponentInParent<UIManager>();
        #endregion

        #region References
        [TitleGroup("Core/References", nameof(Widget)), ShowInInspector]
        [OdinSerialize]
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine, KeyLabel = "Key", ValueLabel = "Reference")]
        public readonly Dictionary<string, UnityEngine.Object> references = new();

        [TitleGroup("Core/References", nameof(Widget)), ShowInInspector]
        [CustomValueDrawer(nameof(OnDrawReferenceDragArea))]
        public string dragArea; // 拖拽引用的区域

        [HorizontalGroup("Core/References/Buttons")]
        [GUIColor(1f, 0.4f, 0.4f)] // 红色调按钮
        [Button("Clear All References")]
        public void ClearReferences()
        {
            references.Clear();
        }

        [HorizontalGroup("Core/References/Buttons")]
        [GUIColor(0.4f, 0.7f, 1f)] // 淡蓝色按钮
        [Button("Refresh References")]
        public void RefreshReferences()
        {
            // 移除所有值为 null 的键
            var keysToRemove = new List<string>();
            foreach (var kvp in references)
            {
                if (kvp.Value == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                references.Remove(key);
            }

            // 添加所有子节点引用
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                // 排除自己
                if (child == this.transform)
                    continue;

                // 如果对象在编辑器中隐藏（例如通过 HideInHierarchy 隐藏），则跳过
                if ((child.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                    continue;

                // 使用 child.gameObject 作为引用并生成唯一键
                string key = GenerateKey(child.gameObject);
                if (!references.ContainsKey(key))
                {
                    references.Add(key, child.gameObject);
                }
            }
        }
        #endregion

        #region Mono
        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (!TryGetComponent<CanvasGroup>(out canvasGroup))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        protected virtual void OnEnable()
        {
        }

        /// <summary>
        /// Called when the behaviour becomes disabled or inactive.
        /// </summary>
        protected virtual void OnDisable()
        {
            IsFocused = false;
        }

        /// <summary>
        /// Called to validate the object when changes are made in the editor.
        /// </summary>
        protected virtual void OnValidate()
        {
        }

        /// <summary>
        /// Adds a reference to the widget.
        /// </summary>
        /// <param name="obj">The object to add as a reference.</param>
        public void AddReference(UnityEngine.Object obj)
        {
            if (transform != null)
            {
                string key = GenerateKey(obj);
                if (!references.ContainsKey(key))
                {
                    references.Add(key, obj);
                }
            }
        }

        /// <summary>
        /// Binds a node from the references dictionary to a variable.
        /// 通过指定的 reference key，尝试从 references 中获取对应的对象，并获取该对象上指定类型的 Component 进行赋值。
        /// 如果绑定失败，则会输出警告日志。
        /// </summary>
        /// <typeparam name="T">要绑定组件的类型。</typeparam>
        /// <param name="referenceKey">references 字典中的键。</param>
        /// <param name="component">绑定成功后输出的组件引用。</param>
        protected virtual void BindReference<T>(string referenceKey, out T component) where T : Component
        {
            component = null;
            if (references.TryGetValue(referenceKey, out UnityEngine.Object obj))
            {
                if (obj is GameObject go)
                {
                    component = go.GetComponent<T>();
                }
                else if (obj is T t)
                {
                    component = t;
                }
                if (component == null)
                {
                    Debug.LogWarning($"绑定失败：从 key '{referenceKey}' 获取的对象上未找到类型 {typeof(T)} 的组件。");
                }
            }
            else
            {
                Debug.LogWarning($"绑定失败:references 中不存在 key '{referenceKey}'。");
            }
        }

        /// <summary>
        /// Generates a unique key for the given object.
        /// 如果生成的 key 已存在，则尝试将父节点的名称加在前面（格式为 parentName_objectName），
        /// 如果仍然重复，则再附加数字后缀，直至生成唯一的 key。
        /// </summary>
        /// <param name="obj">The object to generate a key for.</param>
        /// <returns>A unique key for the object.</returns>
        public string GenerateKey(Object obj)
        {
            string baseKey = obj.name;

            // 如果初始key不存在，直接返回
            if (!references.ContainsKey(baseKey))
            {
                return baseKey;
            }

            // 尝试获取父节点名称
            string parentName = string.Empty;
            if (obj is GameObject go)
            {
                if (go.transform.parent != null)
                {
                    parentName = go.transform.parent.gameObject.name;
                }
            }
            else if (obj is Component comp)
            {
                if (comp.transform.parent != null)
                {
                    parentName = comp.transform.parent.gameObject.name;
                }
            }

            if (!string.IsNullOrEmpty(parentName))
            {
                string newKey = $"{parentName}_{baseKey}";
                if (!references.ContainsKey(newKey))
                {
                    return newKey;
                }
                // 如果依然重复，则追加数字后缀
                int index = 1;
                string finalKey = newKey;
                while (references.ContainsKey(finalKey))
                {
                    finalKey = $"{newKey}_{index}";
                    index++;
                }
                return finalKey;
            }
            else
            {
                // 如果没有父节点或父节点名称为空，则采用数字后缀
                int index = 1;
                string newKey = baseKey;
                while (references.ContainsKey(newKey))
                {
                    newKey = $"{baseKey}_{index}";
                    index++;
                }
                return newKey;
            }
        }
        #endregion

        #region Getter
        /// <summary>
        /// Retrieves a Transform associated with the given path.
        /// </summary>
        /// <param name="path">The path or key of the Transform to find.</param>
        /// <returns>The found Transform, or null if not found.</returns>
        public virtual Transform GetTransform(string path)
        {
            if (null != references && references.TryGetValue(path, out UnityEngine.Object reference))
            {
                if (reference is UnityEngine.GameObject)
                {
                    GameObject go = (GameObject)reference;
                    return go.transform;
                }
            }

            if (null != rectTransform)
            {
                Transform foundTransform = rectTransform.Find(path);
                if (foundTransform == null)
                {
                    Debug.LogWarning($"Transform not found for path: {path}");
                }
                return foundTransform;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a Widget associated with the given path.
        /// </summary>
        /// <param name="path">The path of the Widget to find.</param>
        /// <returns>The found Widget, or null if not found.</returns>
        public virtual Widget GetWidget(string path)
        {
            Transform t = this.GetTransform(path);
            if (null == t)
            {
                Debug.LogWarning($"Widget not found for path: {path}");
                return null;
            }
            return t.GetComponent<Widget>();
        }

        /// <summary>
        /// Retrieves a Widget of type T associated with the given path.
        /// </summary>
        /// <typeparam name="T">The type of Widget to find.</typeparam>
        /// <param name="path">The path of the Widget to find.</param>
        /// <returns>The found Widget of type T, or null if not found.</returns>
        public virtual T GetWidget<T>(string path) where T : Widget
        {
            Transform t = this.GetTransform(path);
            if (null == t)
            {
                Debug.LogWarning($"Widget not found for path: {path}");
                return null;
            }
            return t.GetComponent<T>();
        }

        /// <summary>
        /// Retrieves a GameObject associated with the given path.
        /// </summary>
        /// <param name="path">The path of the GameObject to find.</param>
        /// <returns>The found GameObject, or null if not found.</returns>
        public virtual GameObject GetObject(string path)
        {
            Transform t = this.GetTransform(path);
            if (null == t)
            {
                Debug.LogWarning($"GameObject not found for path: {path}");
                return null;
            }
            return t.gameObject;
        }

        /// <summary>
        /// Retrieves a RectTransform associated with the given path.
        /// </summary>
        /// <param name="path">The path of the RectTransform to find.</param>
        /// <returns>The found RectTransform, or null if not found or type mismatch.</returns>
        public virtual RectTransform GetRectTransform(string path)
        {
            Transform transform = GetTransform(path);
            if (transform is RectTransform rt)
            {
                return rt;
            }
            Debug.LogWarning($"Transform at path: {path} is not a RectTransform");
            return null;
        }
        #endregion

        #region Switch
        public virtual void Close()
        {
            this.Hide();
        }

        /// <summary>
        /// Shows the widget by setting it active.
        /// </summary>
        public void Show()
        {
            this.OnShow();
        }

        /// <summary>
        /// Hides the widget by setting it inactive.
        /// </summary>
        public void Hide()
        {
            this.OnHide();
        }

        /// <summary>
        /// Called when the widget is shown.
        /// </summary>
        protected virtual void OnShow()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when the widget is hidden.
        /// </summary>
        protected virtual void OnHide()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hides the widget after a delay.
        /// </summary>
        /// <param name="duration">The delay duration in seconds.</param>
        public void DelayHide(float duration)
        {
            ApplyDelayHide(duration).Forget();
        }

        /// <summary>
        /// Applies a delay before hiding the widget.
        /// </summary>
        /// <param name="duration">The delay duration in seconds.</param>
        protected virtual async UniTask ApplyDelayHide(float duration)
        {
            if (duration > 0)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
            }

            this.Hide();
        }
        #endregion

        #region Logic
        /// <summary>
        /// Destroys the widget's GameObject.
        /// </summary>
        public void DestroyGameObject()
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(this.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(this.gameObject);
            }
        }

        /// <summary>
        /// Invokes the OnClick event.
        /// </summary>
        /// <param name="widget">The widget that was clicked.</param>
        protected virtual void InvokeOnClick(Widget widget)
        {
            OnClick?.Invoke(widget);
        }

        /// <summary>
        /// Invokes the OnDoubleClick event.
        /// </summary>
        /// <param name="widget">The widget that was double-clicked.</param>
        protected virtual void InvokeOnDoubleClick(Widget widget)
        {
            OnDoubleClick?.Invoke(widget);
        }

        /// <summary>
        /// Retrieves a component of type T from the given path.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <param name="path">The path to the component.</param>
        /// <returns>The component of type T, or default if not found.</returns>
        public virtual T GetComponent<T>(string path) where T : Component
        {
            Transform t = this.GetTransform(path);
            if (null == t)
                return default;

            return t.GetComponent<T>();
        }

        /// <summary>
        /// Tries to retrieve a component of type T from the given path.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <param name="path">The path to the component.</param>
        /// <param name="component">The retrieved component, or null if not found.</param>
        /// <returns>True if the component was found, false otherwise.</returns>
        public virtual bool TryGetComponent<T>(string path, out T component) where T : Component
        {
            Transform t = this.GetTransform(path);
            if (null == t)
            {
                component = null;
                return false;
            }

            return t.TryGetComponent<T>(out component);
        }

        /// <summary>
        /// Retrieves or adds a component of type T to the given path.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve or add.</typeparam>
        /// <param name="path">The path to the component.</param>
        /// <returns>The component of type T.</returns>
        public virtual T GetOrAddComponent<T>(string path) where T : Component
        {
            Transform t = this.GetTransform(path);
            if (null == t)
                return default;

            if (!t.TryGetComponent<T>(out var c))
                c = t.gameObject.AddComponent<T>();
            return c;
        }
        #endregion

        #region UI Event
        /// <summary>
        /// Called when the pointer enters the widget.
        /// </summary>
        /// <param name="eventData">The event data associated with the pointer enter event.</param>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            this.IsFocused = true;
        }

        /// <summary>
        /// Called when the pointer exits the widget.
        /// </summary>
        /// <param name="eventData">The event data associated with the pointer exit event.</param>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            this.IsFocused = false;
        }

        /// <summary>
        /// Called when the widget is clicked.
        /// </summary>
        /// <param name="eventData">The event data associated with the pointer click event。</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // Only handle left clicks and when interactable/raycastable (Button-like)
            if (!IsEligibleForPointer(true, eventData)) return;

            // Prefer Unity's built-in clickCount when available
            int count = eventData != null ? eventData.clickCount : 1;
            if (count >= 2)
            {
                InvokeOnDoubleClick(this);
            }
            else
            {
                InvokeOnClick(this);
            }

            // --- 转发事件给下层 ---
            Propagate(eventData, ExecuteEvents.pointerClickHandler);
        }

        /// <summary>
        /// Called when the pointer is pressed down on the widget.
        /// </summary>
        /// <param name="eventData">The event data associated with the pointer down event.</param>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            // Only handle left button and when interactable/raycastable
            if (!IsEligibleForPointer(true, eventData)) return;
            OnPointerDownEvent?.Invoke(this);

            // --- 转发事件给下层 ---
            Propagate(eventData, ExecuteEvents.pointerDownHandler);
        }

        /// <summary>
        /// Called when the pointer is released from the widget.
        /// </summary>
        /// <param name="eventData">The event data associated with the pointer up event.</param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            // Only handle left button and when interactable/raycastable
            if (!IsEligibleForPointer(true, eventData)) return;
            OnPointerUpEvent?.Invoke(this);

            // --- 转发事件给下层 ---
            Propagate(eventData, ExecuteEvents.pointerUpHandler);
        }

        // --------- 辅助：转发实现 ----------
        private void Propagate<T>(PointerEventData originalEventData, ExecuteEvents.EventFunction<T> eventFunc, bool onlyForwardFirst = true)
        where T : IEventSystemHandler
        {
            if (!forwardPointerToNextTarget) return;
            if (EventSystem.current == null || originalEventData == null) return;

            // Raycast 用的临时数据（不要修改原始 originalEventData）
            var raycastData = new PointerEventData(EventSystem.current)
            {
                position = originalEventData.position,
                pointerId = originalEventData.pointerId,
                // 可以根据需要复制更多字段用于更准确的 raycast（比如 pressPosition 不必要）
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(raycastData, results);
            if (results.Count == 0) return;

            // 先定位自己在命中列表中的索引 —— 更稳妥
            int selfIndex = results.FindIndex(r => r.gameObject != null && IsSelfOrChild(r.gameObject));
            if (selfIndex < 0)
            {
                // 自己不在命中列表，按需求可:
                // - 不转发（保持原逻辑），或
                // - 从第一个开始转发（但通常不希望这样）
                return;
            }

            // 从自己后面的命中项开始转发
            for (int i = selfIndex + 1; i < results.Count; i++)
            {
                var hit = results[i];
                var target = hit.gameObject;
                if (target == null) continue;

                // 创建转发用的 PointerEventData（复制必要字段）
                var forwarded = new PointerEventData(EventSystem.current)
                {
                    position = originalEventData.position,
                    delta = originalEventData.delta,
                    pressPosition = originalEventData.pressPosition,
                    pointerId = originalEventData.pointerId,
                    button = originalEventData.button,
                    clickCount = originalEventData.clickCount,
                    // 复制更多你觉得“必要”的字段（可选）
                };

                // **关键**：把 RaycastResult 回填，便于 handler 使用 e.g. eventData.pointerCurrentRaycast.gameObject
                forwarded.pointerCurrentRaycast = hit;
                forwarded.pointerPressRaycast = hit;
                forwarded.pointerEnter = target;

                // 推荐使用 ExecuteHierarchy，这样父对象上的 handler 也能被调用
                ExecuteEvents.ExecuteHierarchy(target, forwarded, eventFunc);

                // 如果你想“把一个真实点击”转发成目标也经历 down->up->click，可以按下面注释的方式模拟：
                // SimulateClickSequence(target, forwarded);

                if (onlyForwardFirst) break; // 如果只希望转发到第一个“在自己之后”的目标，打开这个
            }
        }

        // 判断命中对象是否是自己或子对象（用于跳过自己）
        private bool IsSelfOrChild(GameObject go)
        {
            if (go == null) return false;
            if (go == this.gameObject) return true;
            return go.transform.IsChildOf(this.transform);
        }

        protected bool IsEligibleForPointer(bool requireLeftButton, PointerEventData eventData)
        {
            // ① 左键（或你自己的输入过滤）
            if (requireLeftButton && (eventData == null || eventData.button != PointerEventData.InputButton.Left))
                return false;

            // ② 像 Button 一样尊重父链 CanvasGroup 的 interactable
            if (!CanvasGroupsAllowInteraction(this.transform))
                return false;

            return true;
        }

        static bool CanvasGroupsAllowInteraction(Transform t)
        {
            // 与 UnityEngine.UI.Selectable 的逻辑一致：任一父 CanvasGroup
            // 在 ignoreParentGroups=false 且 interactable=false 时，视为不允许交互
            var groups = ListPool<CanvasGroup>.Get();   // 或直接 GetComponentsInParent<CanvasGroup>(true, groups)
            t.GetComponentsInParent(true, groups);
            bool allow = true;
            foreach (var g in groups)
            {
                if (!g.interactable && !g.ignoreParentGroups) { allow = false; break; }
            }
            ListPool<CanvasGroup>.Release(groups);
            return allow;
        }


        #endregion

        #region Drag and Drop
        /// <summary>
        /// Called when a drag operation begins.
        /// </summary>
        /// <param name="eventData">The event data associated with the drag event.</param>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {

            if (!Draggable) return;

            canvasGroup.alpha = 0.6f; // Make the widget semi-transparent
            canvasGroup.blocksRaycasts = false; // Allow the widget to pass through other UI elements
        }

        /// <summary>
        /// Called during a drag operation.
        /// </summary>
        /// <param name="eventData">The event data associated with the drag event.</param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!Draggable) return;

            if (Canvas != null)
            {
                RectTransform rectTransform = GetComponent<RectTransform>();
                rectTransform.anchoredPosition += eventData.delta / Canvas.scaleFactor;
            }
        }

        /// <summary>
        /// Called when a drag operation ends.
        /// </summary>
        /// <param name="eventData">The event data associated with the drag event.</param>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!Draggable) return;

            canvasGroup.alpha = 1.0f; // Restore the widget's opacity
            canvasGroup.blocksRaycasts = true; // Restore the widget's ability to block raycasts
        }

        /// <summary>
        /// Called when a drop operation occurs.
        /// </summary>
        /// <param name="eventData">The event data associated with the drop event.</param>
        public virtual void OnDrop(PointerEventData eventData)
        {
            if (!Draggable) return;

            if (eventData.pointerDrag.TryGetComponent<Widget>(out var target))
            {
                // Trigger the OnDropped event
                OnDropped?.Invoke(this, target);
            }
        }
        #endregion

        #region Conversion

        /// <summary>
        /// Converts a screen position to a world position using the Canvas's RectTransform.
        /// </summary>
        /// <param name="screenPosition">The screen position.</param>
        /// <returns>
        /// The converted world position, or Vector3.zero if conversion fails.
        /// </returns>
        public Vector3 ConvertScreenPointToWorldPoint(Vector3 screenPosition)
        {
            Canvas canvas = this.Canvas;
            if (canvas == null)
            {
                Debug.LogWarning("Canvas is null. Cannot convert screen point to world point.");
                return Vector3.zero;
            }

            // 获取 Canvas 的 RectTransform
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            // 如果 Canvas 处于 ScreenSpaceOverlay 模式，则 worldCamera 传 null
            Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPosition, cam, out Vector3 worldPosition))
            {
                return worldPosition;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Converts a world position to a screen position.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <returns>
        /// The converted screen position.
        /// </returns>
        public Vector2 ConvertWorldPointToScreenPoint(Vector3 worldPosition)
        {
            Canvas canvas = this.Canvas;
            if (canvas == null)
            {
                Debug.LogWarning("Canvas is null. Cannot convert world point to screen point.");
                return Vector2.zero;
            }

            // 如果 Canvas 处于 ScreenSpaceOverlay 模式，则不需要摄像机，否则使用 Canvas 的 worldCamera
            Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

            // 利用 RectTransformUtility 将世界点转换为屏幕点
            return RectTransformUtility.WorldToScreenPoint(cam, worldPosition);
        }

        /// <summary>
        /// Converts the current Widget's RectTransform position (in world coordinates) to a screen position.
        /// This method uses the Widget's own RectTransform.position as the conversion basis.
        /// </summary>
        /// <returns>
        /// The converted screen position.
        /// </returns>
        public Vector2 ConvertWorldPointToScreenPoint()
        {
            // 调用重载方法，将 Widget 的 RectTransform 位置转换为屏幕坐标
            return ConvertWorldPointToScreenPoint(this.RectTransform.position);
        }

        /// <summary>
        /// Converts a screen position to a local position relative to the Canvas.
        /// </summary>
        /// <param name="screenPosition">The screen position.</param>
        /// <returns>
        /// The converted local position, or Vector2.zero if conversion fails.
        /// </returns>
        public Vector2 ConvertScreenPointToLocalPosition(Vector3 screenPosition)
        {
            Canvas canvas = this.Canvas;
            if (canvas == null)
            {
                Debug.LogWarning("Canvas is null. Cannot convert screen point to local position.");
                return Vector2.zero;
            }

            // 获取 Canvas 的 RectTransform
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            // 如果 Canvas 处于 ScreenSpaceOverlay 模式，则 worldCamera 传 null
            Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, cam, out Vector2 localPosition))
            {
                return localPosition;
            }
            return Vector2.zero;
        }

        #endregion
    }

}