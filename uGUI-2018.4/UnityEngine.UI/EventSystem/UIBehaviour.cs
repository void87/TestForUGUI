namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Base behaviour that has protected implementations of Unity lifecycle functions.
    /// </summary>
    public abstract class UIBehaviour : MonoBehaviour
    {
        // Dropdown, Selectable
        protected virtual void Awake()
        {}

        // AspectRatioFitter, BaseInputModule, BaseMeshEffect, BaseRaycaster
        // CanvasScaler, ContentSizeFitter, EventSystem, Graphic, Image, InputField
        // LayoutElement, LayoutGroup, Mask, MaskableGraphic, RectMask2D,
        // Scrollbar, ScrollRect, Selectable, Slider, Text, Toggle, 
        protected virtual void OnEnable()
        {}

        // Dropdown, Toggle
        protected virtual void Start()
        {}

        // AspectRatioFitter, BaseInputModule, BaseMeshEffect, BaseRaycaster
        // CanvasScaler, ContentSizeFitter, EventSystem, Graphic, Image, InputField,
        // LayoutElement, LayoutGroup, Mask, MaskableGraphic, RectMask2D, Scrollbar
        // ScrollRect, Selectable, Slider, Text, Toggle, 
        protected virtual void OnDisable()
        {}

        protected virtual void OnDestroy()
        {}

        /// <summary>
        /// Returns true if the GameObject and the Component are active.
        /// </summary>
        ///
        /// ScrollRect
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {}

        protected virtual void Reset()
        {}
#endif
        /// <summary>
        /// This callback is called if an associated RectTransform has its dimensions changed. The call is also made to all child rect transforms, even if the child transform itself doesn't change - as it could have, depending on its anchoring.
        /// </summary>
        ///
        /// AspectRatioFitter, ContentSizeFitter, Graphic, LayoutGroup, Scrollbar
        /// ScrollRect, Slider, 
        protected virtual void OnRectTransformDimensionsChange()
        {}

        // Graphic, LayoutElement, 
        protected virtual void OnBeforeTransformParentChanged()
        {}

        // Graphic, LayoutElement, MaskableGraphic, RectMask2D, 
        protected virtual void OnTransformParentChanged()
        {}

        // BaseMeshEffect, Graphic, LayoutElement, LayoutGroup, Selectable, Slider, Toggle
        protected virtual void OnDidApplyAnimationProperties()
        {}

        // Selectable
        protected virtual void OnCanvasGroupChanged()
        {}

        /// <summary>
        /// Called when the state of the parent Canvas is changed.
        /// </summary>
        ///
        /// Graphic, RectMask2D, MaskableGraphic
        protected virtual void OnCanvasHierarchyChanged()
        {}

        /// <summary>
        /// Returns true if the native representation of the behaviour has been destroyed.
        /// </summary>
        /// <remarks>
        /// When a parent canvas is either enabled, disabled or a nested canvas's OverrideSorting is changed this function is called. You can for example use this to modify objects below a canvas that may depend on a parent canvas - for example, if a canvas is disabled you may want to halt some processing of a UI element.
        /// </remarks>
        public bool IsDestroyed()
        {
            // Workaround for Unity native side of the object
            // having been destroyed but accessing via interface
            // won't call the overloaded ==
            return this == null;
        }
    }
}
