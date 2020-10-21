using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Sakaba.Domain;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sakaba.MDEditor
{
    public class ItemTab : MdItemEditorTab
    {
        List<Item> items = null;
        Item current = null;
        ReorderableList itemList;

        Vector2 cardLeftScrollPos = Vector2.zero;
        Vector2 cardCenterScrollPos = Vector2.zero;

        public ItemTab(MdItemEditor editor) : base(editor)
        {
        }
        
        public override void OnTabSelected()
        {
            CreateItemList();
        }
        
        void CreateItemList()
        {
            if (MDEditorBase.TempMD.ItemTable == null) {
                var builder = MDEditorBase.TempMD.ToDatabaseBuilder();
                builder.Append(new [] {new UnitItem{ id = "100", name = "デフォルトユニット" }});
                MDEditorBase.TempMD = new MD.MemoryDatabase(builder.Build());
            }
            
            items = new List<Item>(MDEditorBase.TempMD.ItemTable.All);

            itemList = new ReorderableList(items, typeof(Item), false, true, false, false);
            // ヘッダーの描画設定
            itemList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Item");
            };
            // エレメントの描画設定
            itemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), items[index].name);
            };
            // 要素を選択した時
            itemList.onSelectCallback = (ReorderableList l) =>
            {
                current = items[itemList.index];
            };
        }

        public override void Draw()
        {
            
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Import", GUILayout.Width(140), GUILayout.Height(28)))
                    MDEditorBase.UseCase.ImportTable<Item>(  // 1
                        (builder, list) =>
                        {
                            var excepts = GameDatabase.DB.ItemTable.All.Select(x => x.id).ToArray();  // 2
                            excepts = excepts.Except(items.Select(x => x.id).ToArray()).ToArray();
                            // 無いIdのものを削除
                            builder.RemoveItem(excepts); // 3
                            // データ差し替え
                            builder.ReplaceAll(items.ToArray());
                        },
                        () =>
                        {
                            current = null;
                            CreateItemList();
                        });

                if (GUILayout.Button("Export", GUILayout.Width(140), GUILayout.Height(28)))
                    MDEditorBase.UseCase.ExportTable( GameDatabase.DB.ItemTable.All.ToList());  // 4
            }
            
            using (new GUILayout.HorizontalScope())
            {
                // 左側
                using (new GUILayout.VerticalScope(GUILayout.Width(160)))
                {
                    cardLeftScrollPos = EditorGUILayout.BeginScrollView(cardLeftScrollPos, GUI.skin.box);

                    if (itemList == null)
                    {
                        CreateItemList();
                    }
                    itemList?.DoLayoutList();

                    EditorGUILayout.EndScrollView();
                }

                if (current != null)
                {
                    // 真ん中
                    using (new GUILayout.VerticalScope())
                    {
                        cardCenterScrollPos = EditorGUILayout.BeginScrollView(cardCenterScrollPos, GUI.skin.box);
                        using (new GUILayout.VerticalScope()) {
                            current?.Draw();
                        }
                        GUILayout.Space(10);
                        EditorGUILayout.EndScrollView();
                    }
                }
            }
        }
    }
}