using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Scroll Rect", 37)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// A component for making a child RectTransform scroll.
    /// </summary>
    /// <remarks>
    /// ScrollRect will not do any clipping on its own. Combined with a Mask component, it can be turned into a scroll view.
    /// </remarks>
    public class ScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        /// <summary>
        /// A setting for which behavior to use when content moves beyond the confines of its container.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     //Called when a button is pressed
        ///     public void Example(int option)
        ///     {
        ///         if (option == 0)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Clamped;
        ///         }
        ///         else if (option == 1)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Elastic;
        ///         }
        ///         else if (option == 2)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public enum MovementType
        {
            /// <summary>
            /// Unrestricted movement. The content can move forever.
            /// </summary>
            ///
            /// 移动没有限制
            Unrestricted,

            /// <summary>
            /// Elastic movement. The content is allowed to temporarily move beyond the container, but is pulled back elastically.
            /// </summary>
            ///
            /// 移动有弹性效果
            Elastic,

            /// <summary>
            /// Clamped movement. The content can not be moved beyond its container.
            /// </summary>
            ///
            /// 移动不能超出父容器
            Clamped,
        }

        /// <summary>
        /// Enum for which behavior to use for scrollbar visibility.
        /// </summary>
        /// 滚动条可见性
        public enum ScrollbarVisibility
        {
            /// <summary>
            /// Always show the scrollbar.
            /// </summary>
            /// 总是可见
            Permanent,

            /// <summary>
            /// Automatically hide the scrollbar when no scrolling is needed on this axis. The viewport rect will not be changed.
            /// </summary>
            /// 自动隐藏
            AutoHide,

            /// <summary>
            /// Automatically hide the scrollbar when no scrolling is needed on this axis, and expand the viewport rect accordingly.
            /// </summary>
            /// <remarks>
            /// When this setting is used, the scrollbar and the viewport rect become driven, meaning that values in the RectTransform are calculated automatically and can't be manually edited.
            /// </remarks>
            /// 自动隐藏和扩展Viewport
            AutoHideAndExpandViewport,
        }

        [Serializable]
        /// <summary>
        /// Event type used by the ScrollRect.
        /// </summary>
        public class ScrollRectEvent : UnityEvent<Vector2> {}

        [SerializeField]
        private RectTransform m_Content;

        /// <summary>
        /// The content that can be scrolled. It should be a child of the GameObject with ScrollRect on it.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public RectTransform scrollableContent;
        ///
        ///     //Do this when the Save button is selected.
        ///     public void Start()
        ///     {
        ///         // assigns the contect that can be scrolled using the ScrollRect.
        ///         myScrollRect.content = scrollableContent;
        ///     }
        /// }
        /// </code>
        /// </example>
        ///
        /// ScrollView->Viewport->Content
        public RectTransform content { get { return m_Content; } set { m_Content = value; } }

        // 是否可以水平滚动
        [SerializeField]
        private bool m_Horizontal = true;

        /// <summary>
        /// Should horizontal scrolling be enabled?
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // Is horizontal scrolling enabled?
        ///         if (myScrollRect.horizontal == true)
        ///         {
        ///             Debug.Log("Horizontal Scrolling is Enabled!");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool horizontal { get { return m_Horizontal; } set { m_Horizontal = value; } }

        // 是否可以垂直滚动
        [SerializeField]
        private bool m_Vertical = true;

        /// <summary>
        /// Should vertical scrolling be enabled?
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // Is Vertical scrolling enabled?
        ///         if (myScrollRect.vertical == true)
        ///         {
        ///             Debug.Log("Vertical Scrolling is Enabled!");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool vertical { get { return m_Vertical; } set { m_Vertical = value; } }

        [SerializeField]
        private MovementType m_MovementType = MovementType.Elastic;

        /// <summary>
        /// The behavior to use when the content moves beyond the scroll rect.
        /// </summary>
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        [SerializeField]
        private float m_Elasticity = 0.1f;

        /// <summary>
        /// The amount of elasticity to use when the content moves beyond the scroll rect.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // assigns a new value to the elasticity of the scroll rect.
        ///         // The higher the number the longer it takes to snap back.
        ///         myScrollRect.elasticity = 3.0f;
        ///     }
        /// }
        /// </code>
        /// </example>
        ///
        /// 内容移动回弹系数
        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }

        [SerializeField]
        private bool m_Inertia = true;

        /// <summary>
        /// Should movement inertia be enabled?
        /// </summary>
        /// <remarks>
        /// Inertia means that the scrollrect content will keep scrolling for a while after being dragged. It gradually slows down according to the decelerationRate.
        /// </remarks>
        ///
        /// 惯性，Content 在拖动后会保持滚动一段时间
        /// 通过 decelerationRate 减速
        public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }

        [SerializeField]
        private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled

        /// <summary>
        /// The rate at which movement slows down.
        /// </summary>
        /// <remarks>
        /// The deceleration rate is the speed reduction per second. A value of 0.5 halves the speed each second. The default is 0.135. The deceleration rate is only used when inertia is enabled.
        /// </remarks>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // assigns a new value to the decelerationRate of the scroll rect.
        ///         // The higher the number the longer it takes to decelerate.
        ///         myScrollRect.decelerationRate = 5.0f;
        ///     }
        /// }
        /// </code>
        /// </example>
        ///
        /// 惯性滚动减速速率
        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }

        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;

        /// <summary>
        /// The sensitivity to scroll wheel and track pad scroll events.
        /// </summary>
        /// <remarks>
        /// Higher values indicate higher sensitivity.
        /// </remarks>
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        [SerializeField]
        private RectTransform m_Viewport;

        /// <summary>
        /// Reference to the viewport RectTransform that is the parent of the content RectTransform.
        /// </summary>
        ///
        /// 默认 ScrollView->Viewport.RectTransform
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; SetDirtyCaching(); } }

        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;

        /// <summary>
        /// Optional Scrollbar object linked to the horizontal scrolling of the ScrollRect.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         // Assigns a scroll bar element to the ScrollRect, allowing you to scroll in the horizontal axis.
        ///         myScrollRect.horizontalScrollbar = newScrollBar;
        ///     }
        /// }
        /// </code>
        /// </example>
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                m_HorizontalScrollbar = value;
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private Scrollbar m_VerticalScrollbar;

        /// <summary>
        /// Optional Scrollbar object linked to the vertical scrolling of the ScrollRect.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         // Assigns a scroll bar element to the ScrollRect, allowing you to scroll in the vertical axis.
        ///         myScrollRect.verticalScrollbar = newScrollBar;
        ///     }
        /// }
        /// </code>
        /// </example>
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                m_VerticalScrollbar = value;
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private ScrollbarVisibility m_HorizontalScrollbarVisibility;

        /// <summary>
        /// The mode of visibility for the horizontal scrollbar.
        /// </summary>
        public ScrollbarVisibility horizontalScrollbarVisibility { get { return m_HorizontalScrollbarVisibility; } set { m_HorizontalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        private ScrollbarVisibility m_VerticalScrollbarVisibility;

        /// <summary>
        /// The mode of visibility for the vertical scrollbar.
        /// </summary>
        public ScrollbarVisibility verticalScrollbarVisibility { get { return m_VerticalScrollbarVisibility; } set { m_VerticalScrollbarVisibility = value; SetDirtyCaching(); } }

        // 
        [SerializeField]
        private float m_HorizontalScrollbarSpacing;

        /// <summary>
        /// The space between the scrollbar and the viewport.
        /// </summary>
        public float horizontalScrollbarSpacing { get { return m_HorizontalScrollbarSpacing; } set { m_HorizontalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        private float m_VerticalScrollbarSpacing;

        /// <summary>
        /// The space between the scrollbar and the viewport.
        /// </summary>
        public float verticalScrollbarSpacing { get { return m_VerticalScrollbarSpacing; } set { m_VerticalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();

        /// <summary>
        /// Callback executed when the position of the child changes.
        /// </summary>
        /// <remarks>
        /// onValueChanged is used to watch for changes in the ScrollRect object.
        /// The onValueChanged call will use the UnityEvent.AddListener API to watch for
        /// changes.  When changes happen script code provided by the user will be called.
        /// The UnityEvent.AddListener API for UI.ScrollRect._onValueChanged takes a Vector2.
        ///
        /// Note: The editor allows the onValueChanged value to be set up manually.For example the
        /// value can be set to run only a runtime.  The object and script function to call are also
        /// provided here.
        ///
        /// The onValueChanged variable can be alternatively set-up at runtime.The script example below
        /// shows how this can be done.The script is attached to the ScrollRect object.
        /// </remarks>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class ExampleScript : MonoBehaviour
        /// {
        ///     static ScrollRect scrollRect;
        ///
        ///     void Start()
        ///     {
        ///         scrollRect = GetComponent<ScrollRect>();
        ///         scrollRect.onValueChanged.AddListener(ListenerMethod);
        ///     }
        ///
        ///     public void ListenerMethod(Vector2 value)
        ///     {
        ///         Debug.Log("ListenerMethod: " + value);
        ///     }
        /// }
        /// </code>
        /// </example>
        public ScrollRectEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        // The offset from handle position to mouse down position
        // 记录鼠标点击时，鼠标在Viewport局部空间的坐标
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        // 记录拖动开始时， Content 的起始 anchoredPosition
        protected Vector2 m_ContentStartPosition = Vector2.zero;

        private RectTransform m_ViewRect;

        // ScrollView->Viewport.RectTransform || ScrollView->RectTransform
        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                    m_ViewRect = m_Viewport;
                if (m_ViewRect == null)
                    m_ViewRect = (RectTransform)transform;
                return m_ViewRect;
            }
        }

        // Content的Bounds
        protected Bounds m_ContentBounds;
        // Viewport的Bounds
        private Bounds m_ViewBounds;

        // Content 的 anchoredPosition 的位置改变/每秒
        private Vector2 m_Velocity;

        /// <summary>
        /// The current velocity of the content.
        /// </summary>
        /// <remarks>
        /// The velocity is defined in units per second.
        /// </remarks>
        ///
        /// Content 的 anchoredPosition 的位置改变/每秒
        public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

        // 是否拖动
        private bool m_Dragging;
        // 是否滚动
        private bool m_Scrolling;

        // 上一个 Content.anchoredPosition 数据
        private Vector2 m_PrevPosition = Vector2.zero;
        // 上一个 Content.Bounds 数据
        private Bounds m_PrevContentBounds;
        // 上一个 Viewport.Bounds 数据
        private Bounds m_PrevViewBounds;

        // 是否已经 LayoutRebuild 过了
        [NonSerialized]
        private bool m_HasRebuiltLayout = false;

        // HorizontalScroll是否扩展 Viewport
        private bool m_HSliderExpand;
        // VerticalScroll是否扩展 Viewport
        private bool m_VSliderExpand;
        // HorizontalScroll的高度
        private float m_HSliderHeight;
        // VerticalScroll的宽度
        private float m_VSliderWidth;

        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private RectTransform m_HorizontalScrollbarRect;
        private RectTransform m_VerticalScrollbarRect;

        private DrivenRectTransformTracker m_Tracker;

        protected ScrollRect()
        {}

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                // 标记已经LayoutRebuild
                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        {}

        public virtual void GraphicUpdateComplete()
        {}

        // 缓存一些相关数据
        void UpdateCachedData()
        {
            Transform transform = this.transform;
            // 设置 HorizontalScrollRect
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            // 设置 VerticalScrollRect
            m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            // ScrollRect 是否有 Viewport 子物体
            bool viewIsChild = (viewRect.parent == transform);
            // ScrollRect 是否有 HorizontalScroll 子物体
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            // ScrollRect 是否有 VerticalScroll 子物体
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            // ScrollRect 是否同时有 Viewport, HorizontalScroll, VerticalScroll 3个子物体
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
            m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            SetDirty();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            m_Scrolling = false;
            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        /// <summary>
        /// See member in base class.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         //Checks if the ScrollRect called "myScrollRect" is active.
        ///         if (myScrollRect.IsActive())
        ///         {
        ///             Debug.Log("The Scroll Rect is active!");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        // 确保已经 LayoutRebuild
        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                // 强制执行CanvasUpdateRegistry.PerformUpdate
                Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Sets the velocity to zero on both axes so the content stops moving.
        /// </summary>
        public virtual void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        // Mouse 触发
        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (vertical && !horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }
            if (horizontal && !vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            if (data.IsScrolling())
                m_Scrolling = true;

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;
            if (m_MovementType == MovementType.Clamped)
                position += CalculateOffset(position - m_Content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Velocity = Vector2.zero;
        }

        /// <summary>
        /// Handling for when the content is beging being dragged.
        /// </summary>
        ///<example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IBeginDragHandler // required interface when using the OnBeginDrag method.
        /// {
        ///     //Do this when the user starts dragging the element this script is attached to..
        ///     public void OnBeginDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("They started dragging " + this.name);
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            // 记录鼠标点击时，鼠标在Viewport局部空间的坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
            // 记录拖动开始时， Content 的起始 anchoredPosition
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_Dragging = true;
        }

        /// <summary>
        /// Handling for when the content has finished being dragged.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IEndDragHandler // required interface when using the OnEndDrag method.
        /// {
        ///     //Do this when the user stops dragging this UI Element.
        ///     public void OnEndDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("Stopped dragging " + this.name + "!");
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Dragging = false;
        }

        /// <summary>
        /// Handling for when the content is dragged.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IDragHandler // required interface when using the OnDrag method.
        /// {
        ///     //Do this while the user is dragging this UI Element.
        ///     public void OnDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("Currently dragging " + this.name);
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            // 记录鼠标拖动时，鼠标在Viewport局部空间的坐标
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            // 获取 鼠标的位置的 delta
            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            // 赋值新的 Content.anchoredPosition
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            // 计算 Content 相对于 Viewport 的偏移
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;

            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
            }

            // 设置新的 Content 的 anchoredPosition
            SetContentAnchoredPosition(position);
        }

        /// <summary>
        /// Sets the anchored position of the content.
        /// </summary>
        /// 设置 Content.anchoredPosition
        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            // 不能水平移动
            if (!m_Horizontal)
                position.x = m_Content.anchoredPosition.x;
            // 不能垂直移动
            if (!m_Vertical)
                position.y = m_Content.anchoredPosition.y;

            if (position != m_Content.anchoredPosition)
            {
                // 设置 Content.anchoredPosition
                m_Content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        protected virtual void LateUpdate()
        {
            if (!m_Content)
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;
            // 计算 Content 相对于 Viewport 的偏移
            Vector2 offset = CalculateOffset(Vector2.zero);

            // 没有拖动, 但是有 offset || m_Velocity
            if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
            {
                // 获取 Content.anchoredPosition, 用于 SetContentAnchoredPosition
                Vector2 position = m_Content.anchoredPosition;

                // axis == 0, x轴
                // axis == 1, y轴
                for (int axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if (m_MovementType == MovementType.Elastic && offset[axis] != 0)
                    {
                        // 获取单轴的速度
                        float speed = m_Velocity[axis];
                        float smoothTime = m_Elasticity;

                        if (m_Scrolling)
                            smoothTime *= 3.0f;

                        // Damp
                        position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, smoothTime, Mathf.Infinity, deltaTime);

                        if (Mathf.Abs(speed) < 1)
                            speed = 0;

                        m_Velocity[axis] = speed;
                    }
                    // Else move content according to velocity with deceleration applied.
                    // 计算惯性
                    else if (m_Inertia)
                    {
                        m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);

                        if (Mathf.Abs(m_Velocity[axis]) < 1)
                            m_Velocity[axis] = 0;

                        position[axis] += m_Velocity[axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        m_Velocity[axis] = 0;
                    }
                }

                if (m_MovementType == MovementType.Clamped)
                {
                    offset = CalculateOffset(position - m_Content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }

            // 拖动且有惯性
            if (m_Dragging && m_Inertia)
            {
                // Content的 这一帧的 anchoredPosition 减上一帧的 anchoredPosition 得到新速度
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                // 插值一下就得到 m_Velocity, 也就是 anchoredPosition的改变
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }

            if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                // 更新滚动条的value
                UpdateScrollbars(offset);

                UISystemProfilerApi.AddMarker("ScrollRect.value", this);
                // 调用滚动条事件
                m_OnValueChanged.Invoke(normalizedPosition);

                UpdatePrevData();
            }

            // 设置滚动条的可见性
            UpdateScrollbarVisibility();

            // 滚动结束
            m_Scrolling = false;
        }

        /// <summary>
        /// Helper function to update the previous data fields on a ScrollRect. Call this before you change data in the ScrollRect.
        /// </summary>
        /// 记录 这一帧数据以便下一帧使用
        protected void UpdatePrevData()
        {
            // 记录Content.anchoredPosition
            if (m_Content == null)
                m_PrevPosition = Vector2.zero;
            else
                m_PrevPosition = m_Content.anchoredPosition;
            // 记录 ViewportBounds
            m_PrevViewBounds = m_ViewBounds;
            // 记录 ContentBounds
            m_PrevContentBounds = m_ContentBounds;
        }

        // 更新 滚动条的 value
        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                if (m_ContentBounds.size.x > 0)
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
                else
                    m_HorizontalScrollbar.size = 1;

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                if (m_ContentBounds.size.y > 0)
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
                else
                    m_VerticalScrollbar.size = 1;

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        /// <summary>
        /// The scroll position as a Vector2 between (0,0) and (1,1) with (0,0) being the lower left corner.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Vector2 myPosition = new Vector2(0.5f, 0.5f);
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current scroll position.
        ///         myScrollRect.normalizedPosition = myPosition;
        ///     }
        /// }
        /// </code>
        /// </example>
        ///
        /// x,y 两个轴的 归一化值
        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        /// <summary>
        /// The horizontal scroll position as a value between 0 and 1, with 0 being at the left.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current horizontal scroll position.
        ///         myScrollRect.horizontalNormalizedPosition = 0.5f;
        ///     }
        /// }
        /// </code>
        /// </example>
        ///
        /// 水平滑动条的 归一化值 (0-1)
        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if ((m_ContentBounds.size.x <= m_ViewBounds.size.x) || Mathf.Approximately(m_ContentBounds.size.x, m_ViewBounds.size.x))
                    return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
                return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        /// <summary>
        /// The vertical scroll position as a value between 0 and 1, with 0 being at the bottom.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current vertical scroll position.
        ///         myScrollRect.verticalNormalizedPosition = 0.5f;
        ///     }
        /// }
        /// </code>
        /// </example>
        // 垂直滚动条归一化值 (1-0)
        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if ((m_ContentBounds.size.y <= m_ViewBounds.size.y) || Mathf.Approximately(m_ContentBounds.size.y, m_ViewBounds.size.y))
                    return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;

                return (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        /// <summary>
        /// >Set the horizontal or vertical scroll position as a value between 0 and 1, with 0 being at the left or at the bottom.
        /// </summary>
        /// <param name="value">The position to set, between 0 and 1.</param>
        /// <param name="axis">The axis to set: 0 for horizontal, 1 for vertical.</param>
        ///
        /// 设置 x || y 轴的归一化值, 0 = x轴, 1 = y轴
        protected virtual void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            // How much the content is larger than the view.
            // 计算 Content 比 Viewport 大多少(x||y轴)
            float hiddenLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
            // Where the position of the lower left corner of the content bounds should be, in the space of the view.
            // Content 左下角在 Viewport 空间下的位置
            float contentBoundsMinPosition = m_ViewBounds.min[axis] - value * hiddenLength;
            // The new content localPosition, in the space of the view.
            // 计算 Content 的 LocalPosition
            float newLocalPosition = m_Content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

            Vector3 localPosition = m_Content.localPosition;

            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                // 重新设置 Content.localPosition
                m_Content.localPosition = localPosition;
                m_Velocity[axis] = 0;

                UpdateBounds();
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        // 是否需要HorizontalScroll
        private bool hScrollingNeeded
        {
            get
            {
                // 只要 Content.x 大于 Viewport.x 就需要
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                return true;
            }
        }

        // 是否需要VerticalScroll
        private bool vScrollingNeeded
        {
            get
            {
                // 只要 Content.y 大于 Viewport.y 就需要
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                return true;
            }
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal() {}

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputVertical() {}

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minWidth { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredWidth { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleWidth { get { return -1; } }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minHeight { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredHeight { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleHeight { get { return -1; } }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual int layoutPriority { get { return -1; } }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();

            if (m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (m_HSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        // 设置滚动条的可见性
        void UpdateScrollbarVisibility()
        {
            // 设置垂直滚动条的可见性
            UpdateOneScrollbarVisibility(vScrollingNeeded, m_Vertical, m_VerticalScrollbarVisibility, m_VerticalScrollbar);
            // 设置水平滚动条的可见性
            UpdateOneScrollbarVisibility(hScrollingNeeded, m_Horizontal, m_HorizontalScrollbarVisibility, m_HorizontalScrollbar);
        }

        // 设置 单个滚动条的可见性
        private static void UpdateOneScrollbarVisibility(bool xScrollingNeeded, bool xAxisEnabled, ScrollbarVisibility scrollbarVisibility, Scrollbar scrollbar)
        {
            if (scrollbar)
            {
                if (scrollbarVisibility == ScrollbarVisibility.Permanent)
                {
                    if (scrollbar.gameObject.activeSelf != xAxisEnabled)
                        scrollbar.gameObject.SetActive(xAxisEnabled);
                }
                else
                {
                    if (scrollbar.gameObject.activeSelf != xScrollingNeeded)
                        scrollbar.gameObject.SetActive(xScrollingNeeded);
                }
            }
        }

        void UpdateScrollbarLayout()
        {
            if (m_VSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinY |
                    DrivenTransformProperties.AnchorMaxY |
                    DrivenTransformProperties.SizeDeltaY |
                    DrivenTransformProperties.AnchoredPositionY);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        /// <summary>
        /// Calculate the bounds the ScrollRect should be using.
        /// </summary>
        ///
        /// 计算 Viewport.Bounds, Content.Bounds
        protected void UpdateBounds()
        {
            // center 按 pivot 计算
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            // 获取 Content 的 Bounds
            m_ContentBounds = GetBounds();

            if (m_Content == null)
                return;

            // Content的宽高
            Vector3 contentSize = m_ContentBounds.size;
            // Content相对于Pivot的Center
            Vector3 contentPos = m_ContentBounds.center;
            // Content的pivot
            var contentPivot = m_Content.pivot;

            AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);

            // 重新赋值 Content的size
            m_ContentBounds.size = contentSize;
            // 重新赋值 Content的center
            m_ContentBounds.center = contentPos;

            // MovementType.Clamped 条件下， 让Content的移动不能超出View
            if (movementType == MovementType.Clamped)
            {
                // Adjust content so that content bounds bottom (right side) is never higher (to the left) than the view bounds bottom (right side).
                // top (left side) is never lower (to the right) than the view bounds top (left side).
                // All this can happen if content has shrunk.
                // This works because content size is at least as big as view size (because of the call to InternalUpdateBounds above).
                Vector2 delta = Vector2.zero;
                if (m_ViewBounds.max.x > m_ContentBounds.max.x)
                {
                    delta.x = Math.Min(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
                }
                else if (m_ViewBounds.min.x < m_ContentBounds.min.x)
                {
                    delta.x = Math.Max(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
                }

                if (m_ViewBounds.min.y < m_ContentBounds.min.y)
                {
                    delta.y = Math.Max(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
                }
                else if (m_ViewBounds.max.y > m_ContentBounds.max.y)
                {
                    delta.y = Math.Min(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
                }
                if (delta.sqrMagnitude > float.Epsilon)
                {
                    contentPos = m_Content.anchoredPosition + delta;
                    if (!m_Horizontal)
                        contentPos.x = m_Content.anchoredPosition.x;
                    if (!m_Vertical)
                        contentPos.y = m_Content.anchoredPosition.y;
                    AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
                }
            }
        }

        // 根据 View.Bounds 调整 Content.Bounds
        internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
        {
            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            // 获取 Viewport.Bounds.size 和 Content.Bounds.size
            Vector3 excess = viewBounds.size - contentSize;
            // x轴Viewport大于Content
            if (excess.x > 0)
            {
                // 调整 Content pos.x
                contentPos.x -= excess.x * (contentPivot.x - 0.5f);
                // 调整 Content size.x
                contentSize.x = viewBounds.size.x;
            }
            // x轴Viewport大于Content
            if (excess.y > 0)
            {
                // 调整 Content pos.y
                contentPos.y -= excess.y * (contentPivot.y - 0.5f);
                // 调整 Content size.y
                contentSize.y = viewBounds.size.y;
            }
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        // 获取 Content的Bounds
        private Bounds GetBounds()
        {
            if (m_Content == null)
                return new Bounds();
            // 获取 Content 四个角的世界坐标
            m_Content.GetWorldCorners(m_Corners);
            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
            return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
        }

        // 只用于获取 Content的Bounds
        internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // 获取4个角的局部坐标
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            // Grows the Bounds to include the point
            bounds.Encapsulate(vMax);
            return bounds;
        }

        // 计算 Content 相对于 Viewport 的偏移
        private Vector2 CalculateOffset(Vector2 delta)
        {
            return InternalCalculateOffset(ref m_ViewBounds, ref m_ContentBounds, m_Horizontal, m_Vertical, m_MovementType, ref delta);
        }

        internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal, bool vertical, MovementType movementType, ref Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (movementType == MovementType.Unrestricted)
                return offset;

            // Content 的左下角
            Vector2 min = contentBounds.min;
            // Content 的右上角
            Vector2 max = contentBounds.max;

            // 计算x轴 Content 相对于 Viewport 的偏移
            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
                // Content 在 Viewport 右边，返回负
                if (min.x > viewBounds.min.x)
                    offset.x = viewBounds.min.x - min.x;
                // Content 在 Viewport 左边, 返回正
                else if (max.x < viewBounds.max.x)
                    offset.x = viewBounds.max.x - max.x;
            }

            // 计算y轴 Content 相对于 Viewport 的偏移
            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
                // Content 在 Viewport 下面，返回正
                if (max.y < viewBounds.max.y)
                    offset.y = viewBounds.max.y - max.y;
                // Content 在 Viewport 上面，返回负
                else if (min.y > viewBounds.min.y)
                    offset.y = viewBounds.min.y - min.y;
            }

            return offset;
        }

        /// <summary>
        /// Override to alter or add to the code that keeps the appearance of the scroll rect synced with its data.
        /// </summary>
        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        /// <summary>
        /// Override to alter or add to the code that caches data to avoid repeated heavy operations.
        /// </summary>
        ///
        /// For Layout
        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;


            // 下面两个方法都会尝试添加到 m_LayoutRebuildQueue
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }

        #endif
    }
}
