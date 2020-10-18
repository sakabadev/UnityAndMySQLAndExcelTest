using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sakaba.MDEditor
{
    public class MdItemEditor : MDEditorBase
    {
        private readonly List<MdItemEditorTab> tabs = new List<MdItemEditorTab>();

        private int selectedTabIndex = -1;
        private int prevSelectedTabIndex = -1;
        
        [MenuItem("Sakaba/DBEditor/Item", false, 1)]
        private static void Init()
        {
            var window = GetWindow(typeof(MdItemEditor));
            window.titleContent = new GUIContent("Item Editor");
            window.minSize = new Vector2(800, 600);
        }

        protected override void OnEnableAfter()
        {
            tabs.Add(new ItemTab(this));
            tabs.Add(new ItemTierTab(this));
            selectedTabIndex = 0;
        }

        private void OnGUI()
        {
            if (isDirty && GUILayout.Button("Save", GUILayout.Width(140), GUILayout.Height(28)))
            {
                SetMDDirty("Standard Editor Change");
            }
            
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
                new[] { "Item", "ItemTier" });
            if (selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
            {
                var selectedEditor = tabs[selectedTabIndex];
                if (selectedTabIndex != prevSelectedTabIndex)
                {
                    selectedEditor.OnTabSelected();
                    GUI.FocusControl(null);
                }
                
                EditorGUI.BeginChangeCheck();

                selectedEditor.Draw();
                
                if (EditorGUI.EndChangeCheck())
                {
                    isDirty = true;
                }
                
                prevSelectedTabIndex = selectedTabIndex;
            }
        }
    }
}