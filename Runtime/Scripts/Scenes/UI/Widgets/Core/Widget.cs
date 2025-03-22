using Sirenix.OdinInspector; // Import Odin Inspector namespace
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

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
        #region Event
        [FoldoutGroup("Events"), ReadOnly, ShowInInspector]
        public delegate void OnWidgetHandle(Widget widget); // Delegate for widget events

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetHandle OnClick; // Event triggered on single click

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetHandle OnDoubleClick; // Event triggered on double click

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetHandle OnFocusIn; // Event triggered when focus is gained

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetHandle OnFocusOut; // Event triggered when focus is lost

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetHandle OnSelected; // Event triggered when selection is gained

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetHandle OnDeselected; // Event triggered when selection is lost

        public delegate void OnWidgetDroppedHandle(Widget widget, Widget targetWidget); // Delegate for drop events

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetDroppedHandle OnDropped; // Event triggered when widget is dropped

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetHandle OnPointerDownEvent; // Event triggered on pointer down

        [FoldoutGroup("Events"), ShowInInspector]
        public event OnWidgetHandle OnPointerUpEvent; // Event triggered on pointer up
        #endregion

        #region Params
        /// <summary>
        /// Indicates if the widget is active.
        /// </summary>
        public bool IsActive => null != this.gameObject && this.gameObject.activeSelf;

        [TitleGroup("Core", "Widget", Order = 0), ShowInInspector]
        public RectTransform RectTransform => rectTransform; // RectTransform of the widget
        protected RectTransform rectTransform;

        [TitleGroup("Core", "Widget"), ShowInInspector, ReadOnly]
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
                    }
                    else
                    {
                        this.OnFocusOut?.Invoke(this);
                    }
                }
            }
        }
        private bool isFocused = false; // Indicates if the widget is focused

        [TitleGroup("Core", "Widget"), ShowInInspector]
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

        protected float lastClickTime = 0f; // Time of the last click
        protected const float doubleClickThreshold = 0.5f; // Time interval threshold for double click

        public bool Draggable { get; set; } = false; // Indicates if the widget is draggable

        public Canvas Canvas => GetComponentInParent<Canvas>();
        public CanvasGroup CanvasGroup => canvasGroup; // CanvasGroup for managing UI interactions
        private CanvasGroup canvasGroup;

        /// <summary>
        /// UI管理器
        /// </summary>
        public virtual UIManager UIManager => GetComponentInParent<UIManager>();
        #endregion

        #region References
        /// <summary>
        /// Dictionary of references associated with the widget.
        /// </summary>
        [TitleGroup("Core/References", nameof(Widget)), ShowInInspector]
        [OdinSerialize]
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine, KeyLabel = "Key", ValueLabel = "Reference")]
        public readonly Dictionary<string, UnityEngine.Object> references = new();

        [TitleGroup("Core/References", nameof(Widget)), ShowInInspector]
        [CustomValueDrawer(nameof(OnDrawReferenceDragArea))]
        public string dragArea; // Area for dragging references

        // 将按钮横向排列
        [HorizontalGroup("Core/References/Buttons")]
        [Button("Clear References")]
        public void ClearReferences()
        {
            references.Clear();
        }

        [HorizontalGroup("Core/References/Buttons")]
        [Button("Add All Children References")]
        public void AddAllChildrenReferences()
        {
            // 获取当前物体所有的子 Transform（包括隐藏和孙子节点）
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                // 排除自己
                if (child == this.transform)
                    continue;

                // 如果对象在编辑器中隐藏（例如 HideInHierarchy），则跳过
                if ((child.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                    continue;

                // 使用 child.gameObject 作为引用
                string key = GenerateKey(child.gameObject);
                if (!references.ContainsKey(key))
                {
                    references.Add(key, child.gameObject);
                }
            }
        }

        [HorizontalGroup("Core/References/Buttons")]
        [Button("Clear Null References")]
        public void ClearNullReferences()
        {
            // 收集所有值为 null 的键
            var keysToRemove = new List<string>();
            foreach (var kvp in references)
            {
                if (kvp.Value == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            // 从字典中移除收集到的键
            foreach (var key in keysToRemove)
            {
                references.Remove(key);
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
        /// <param name="eventData">The event data associated with the pointer click event.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            float currentTime = Time.time;
            if (currentTime - lastClickTime < doubleClickThreshold)
            {
                InvokeOnDoubleClick(this);
            }
            else
            {
                InvokeOnClick(this);
            }
            lastClickTime = currentTime;
        }

        /// <summary>
        /// Called when the pointer is pressed down on the widget.
        /// </summary>
        /// <param name="eventData">The event data associated with the pointer down event.</param>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownEvent?.Invoke(this);
        }

        /// <summary>
        /// Called when the pointer is released from the widget.
        /// </summary>
        /// <param name="eventData">The event data associated with the pointer up event.</param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpEvent?.Invoke(this);
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