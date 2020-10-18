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
    }
}