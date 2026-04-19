using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Util
{
    public class AlignLabelManipulator : Manipulator
    {
        readonly Label m_Label;
        public Label Label => m_Label ?? target.Q<Label>();
        
        VisualElement m_CachedInspectorElement;
        VisualElement m_CachedContextWidthElement;

        float m_LabelWidthRatio;
        float m_LabelExtraPadding;
        float m_LabelBaseMinWidth;
        float m_LabelExtraContextWidth;

        static readonly string ussClassName = "unity-base-field";
        static readonly string inspectorFieldUssClassName = ussClassName + "__inspector-field";
        static readonly string labelFieldUssClassName = ussClassName + "__label";

        public AlignLabelManipulator(Label label = null)
        {
            m_Label = label;
        }

        void OnAttachToPanel(AttachToPanelEvent e)
        {
            if (e.destinationPanel == null || e.destinationPanel.contextType == ContextType.Player)
            {
                return;
            }

            for (VisualElement visualElement = target.parent; visualElement != null; visualElement = visualElement.parent)
            {
                if (visualElement.ClassListContains("unity-inspector-element"))
                {
                    m_CachedInspectorElement = visualElement;
                }

                if (visualElement.ClassListContains("unity-inspector-main-container"))
                {
                    m_CachedContextWidthElement = visualElement;
                    break;
                }
            }

            if (m_CachedInspectorElement != null)
            {
                m_LabelWidthRatio = 0.45f;
                m_LabelExtraPadding = /*target.style.flexGrow == 0 ? 37f :40f*/37f;
                m_LabelBaseMinWidth = 123;
                m_LabelExtraContextWidth = 1f;
                target.AddToClassList(inspectorFieldUssClassName);
                target.RegisterCallback<GeometryChangedEvent>(OnInspectorFieldGeometryChanged);
            }

            if (Label != null)
            {
                //Label.AddToClassList(ussClassName);
                Label.AddToClassList(labelFieldUssClassName);
            }
        }
        void OnInspectorFieldGeometryChanged(GeometryChangedEvent e)
        {
            AlignLabel();
        }
        void AlignLabel()
        {
            if (Label == null)
                return;

            float labelExtraPadding = m_LabelExtraPadding;
            float num = target.worldBound.x - m_CachedInspectorElement.worldBound.x - m_CachedInspectorElement.resolvedStyle.paddingLeft;
            labelExtraPadding += num;
            labelExtraPadding += target.resolvedStyle.paddingLeft;
            float a = m_LabelBaseMinWidth - num - target.resolvedStyle.paddingLeft;
            VisualElement visualElement = m_CachedContextWidthElement ?? m_CachedInspectorElement;
            Label.style.minWidth = Mathf.Max(a, 0f);
            float num2 = (visualElement.resolvedStyle.width + m_LabelExtraContextWidth) * m_LabelWidthRatio - labelExtraPadding;
            if (Mathf.Abs(Label.resolvedStyle.width - num2) > 1E-30f)
            {
                Label.style.width = Mathf.Max(0f, num2);
            }
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }
    }
}