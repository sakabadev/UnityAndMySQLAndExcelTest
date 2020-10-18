using System;
using System.Collections.Generic;
using Sakaba.Domain;
using Sakaba.Infra;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sakaba.MDEditor
{
    public class MdItemEditorTab
    {
        protected MdItemEditor ParentEditor;
        protected IMDRepository MdRepository;
        protected IMemoryTableRepository TableRepository;

        public MdItemEditorTab(MdItemEditor editor)
        {
            ParentEditor = editor;
            MdRepository = new FileMDRepository();
            TableRepository = new MySQLMemoryTableRepository();
        }

        public virtual void OnTabSelected()
        {
        }

        public virtual void Draw()
        {
        }

        public static ReorderableList SetupReorderableList<T>(
            string headerText,
            List<T> elements,
            Action<Rect, T> drawElement,
            Action<T> selectElement,
            Action createElement,
            Action<T> removeElement)
        {
            var list = new ReorderableList(elements, typeof(T), true, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, headerText); },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = elements[index];
                    drawElement(rect, element);
                }
            };

            list.onSelectCallback = l =>
            {
                var selectedElement = elements[list.index];
                selectElement(selectedElement);
            };

            if (createElement != null)
            {
                list.onAddDropdownCallback = (buttonRect, l) =>
                {
                    createElement();
                };
            }

            list.onRemoveCallback = l =>
            {
                if (!EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this item?", "Yes", "No")
                )
                {
                    return;
                }
                var element = elements[l.index];
                removeElement(element);
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            };

            return list;
        }
    }
}