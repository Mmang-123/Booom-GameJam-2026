using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Mmang.Editors
{

    public class SelectionContainer<T> : SelectionContainer
    {
        public class TElement : Element
        {
            public T BindingObject;
        }

        public TElement TCurrentSelected => CurrentSelected as TElement;
        public T CurrentSelectedObject => TCurrentSelected == null ? default : TCurrentSelected.BindingObject;

        private Dictionary<T, TElement> m_TElementMap = new();

        public TElement AddSelection(T obj, VisualElement visualElement, System.Action onSelected = null, System.Action onUnselected = null)
        {
            if (visualElement == null || obj == null)
            {
                return null;
            }

            TElement element = new() { VisualElement = visualElement, BindingObject = obj };
            AddSelection(element, onSelected, onUnselected);
            return element;
        }

        protected override void OnAddSelection(Element element, Action onSelected = null, Action onUnselected = null)
        {
            base.OnAddSelection(element, onSelected, onUnselected);
            if (element is TElement tElement && !m_TElementMap.ContainsKey(tElement.BindingObject))
            {
                m_TElementMap.Add(tElement.BindingObject, tElement);
            }
        }

        protected override void OnRemoveElement(Element element)
        {
            base.OnRemoveElement(element);
            if (element is TElement tElement
            && m_TElementMap.ContainsKey(tElement.BindingObject)
            && m_TElementMap[tElement.BindingObject] == element)
            {
                m_TElementMap.Remove(tElement.BindingObject);
            }
        }

        public TElement GetSelectionElement(T obj)
        {
            if (m_TElementMap.TryGetValue(obj, out var result))
            {
                return result;
            }
            return null;
        }

        public void SelectObject(T obj)
        {
            if (obj == null)
            {
                SelectElement(null); // 这里相当于Unselect
                return;
            }

            var element = GetSelectionElement(obj);
            if (element != null)
            {
                SelectElement(element);
            }
        }

        // 跟SelectObject区别在于传入null时会直接返回false
        public bool TrySelectObject(T obj)
        {
            var element = GetSelectionElement(obj);
            if (element == null)
            {
                return false;
            }

            SelectElement(element);
            return true;
        }
    }

    public static class SelectionContainerExtension
    {
        public static bool IsValid(this SelectionContainer.Element element)
        {
            return element != null && element.VisualElement != null;
        }
    }

    public class SelectionContainer : VisualElement
    {
        public class Element
        {
            public VisualElement VisualElement;
        }

        private List<Element> m_Selections = new();
        private Dictionary<Element, System.Action> m_OnSelectedMap = new();
        private Dictionary<Element, System.Action> m_OnUnselectedMap = new();

        public int CurrentSelectedIndex { get; private set; } = -1;
        public Element CurrentSelected { get; private set; } = null;

        public string OnSelectedStyleName { get; private set; }

        public event System.Action OnSelectedNull;
        
        public SelectionContainer(string onSelectedStyleName = "selected")
        {
            OnSelectedStyleName = onSelectedStyleName;
        }

        public void ClearSelections()
        {
            SelectElement(null);
            Clear();
            m_Selections.Clear();
            m_OnSelectedMap.Clear();
            m_OnUnselectedMap.Clear();

            CurrentSelected = null;
            CurrentSelectedIndex = -1;
        }

        protected bool ContainsSelection(Element element)
        {
            if (element == null || !m_OnSelectedMap.ContainsKey(element)) // 两个Map都会储存，只判断一个即可
            {
                return false;
            }
            return true;
        }

        public Element AddVisualSelection(VisualElement visualElement, System.Action onSelected = null, System.Action onUnselected = null)
        {
            if (visualElement == null)
            {
                return null;
            }

            Element element = new() { VisualElement = visualElement };
            AddSelection(element, onSelected, onUnselected);
            return element;
        }

        // 希望泛型类能约束Element类型，所以这里不开放公共方法
        protected virtual void OnAddSelection(Element element, System.Action onSelected = null, System.Action onUnselected = null) { }
        protected void AddSelection(Element element, System.Action onSelected = null, System.Action onUnselected = null)
        {
            if (!element.IsValid() || ContainsSelection(element))
            {
                return;
            }

            OnAddSelection(element, onSelected, onUnselected);

            m_Selections.Add(element);
            m_OnSelectedMap.Add(element, onSelected);
            m_OnUnselectedMap.Add(element, onUnselected);
            Add(element.VisualElement);

            // 刷新
            if (CurrentSelected != null)
            {
                CurrentSelectedIndex = m_Selections.IndexOf(element);
            }
        }

        protected virtual void OnRemoveElement(Element element) { }
        public void RemoveSelection(Element element)
        {
            if (!element.IsValid() || !ContainsSelection(element))
            {
                return;
            }

            OnRemoveElement(element);

            m_Selections.Remove(element);
            m_OnSelectedMap.Remove(element);
            m_OnUnselectedMap.Remove(element);
            Remove(element.VisualElement);

            // 
            if (CurrentSelected == element)
            {
                int oldIndex = CurrentSelectedIndex;

                if (oldIndex < m_Selections.Count)
                {
                    SelectElement(m_Selections[oldIndex]);
                }
                else if (m_Selections.Count > 0)
                {
                    SelectElement(m_Selections[0]);
                }
                else
                {
                    SelectElement(null);
                }
            }
        }

        public void SelectElement(Element element)
        {
            if (element == null)
            {
                Unselect();
                return;
            }

            if (element == CurrentSelected || !ContainsSelection(element))
            {
                return;
            }

            if (CurrentSelected != null && ContainsSelection(CurrentSelected))
            {
                InternalOnUnselectElement();
            }

            InternalOnSelectElement(element);
        }

        public void Unselect()
        {
            if (CurrentSelected != null)
            {
                // 删除元素时候可能触发Unselect
                if (ContainsSelection(CurrentSelected))
                {
                    InternalOnUnselectElement();
                }

                OnSelectedNull?.Invoke();
            }
        }

        /// <summary>
        /// 选择元素，元素需要存在
        /// </summary>
        /// <param name="element"></param>
        private void InternalOnSelectElement(Element element)
        {
            CurrentSelected = element;
            m_OnSelectedMap[element]?.Invoke();
            CurrentSelectedIndex = m_Selections.IndexOf(element);

            element.VisualElement?.AddToClassList(OnSelectedStyleName);
        }

        /// <summary>
        /// 取消当前选择，需要当前选择存在
        /// </summary>
        private void InternalOnUnselectElement()
        {
            CurrentSelected.VisualElement?.RemoveFromClassList(OnSelectedStyleName);
            m_OnUnselectedMap[CurrentSelected]?.Invoke();
            CurrentSelected = null;
            CurrentSelectedIndex = -1;
        }
    }
}