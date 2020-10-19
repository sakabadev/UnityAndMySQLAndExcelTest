using System.Collections.Generic;
using System.Linq;
using MasterMemory;
using MessagePack;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sakaba.Domain {
    [Union(0, typeof(UnitItem))]
    [Union(1, typeof(EquipItem))]
    [MemoryTable(nameof(Item)), MessagePackObject(true)]
	public abstract class Item
    {
#if UNITY_EDITOR
        [ColumnConfig(sortLabel = "キー", columnWidth = 20, preferExcel = 1)]
        [AddColumnType("varchar(50)","NOT NULL")]
#endif
        public string id;
        
#if UNITY_EDITOR
        [ColumnConfig(sortLabel="名前", preferExcel = 1)]
        [AddColumnType("varchar(50)","NOT NULL")]
#endif
        public string name;
        
#if UNITY_EDITOR
        [ColumnConfig(sortLabel = "説明", columnWidth = 200, preferExcel = 1)]
        [AddColumnType("varchar(255)","NULL")]
#endif
        public string text;
        
#if UNITY_EDITOR
        [ColumnConfig(sortLabel = "アイコンパス")]
        [AddColumnType("varchar(255)","NULL")]
#endif
        public string icon;
        
        [PrimaryKey, IgnoreMember]
        public string Id => id;

#if UNITY_EDITOR
        public virtual void Draw()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box, GUILayout.MaxWidth(500)))
            {
                EditorGUILayout.LabelField("id", id);
                name = EditorGUILayout.TextField("名前", name);
                text = EditorGUILayout.TextField("説明", text);
                icon = EditorGUILayout.TextField("アイコンパス", icon);
            }
        }
#endif
    }

    [MessagePackObject(true)]
    public class UnitItem : Item
    {
#if UNITY_EDITOR
        [ColumnConfig(sortLabel ="HP", columnWidth = 20, preferExcel = 1)]
        [AddColumnType("int(11)","NOT NULL")]
#endif
        public int hp;
        
#if UNITY_EDITOR
        [ColumnConfig(sortLabel ="攻撃力", columnWidth = 20, preferExcel = 1)]
        [AddColumnType("int(11)","NOT NULL")]
#endif
        public int attack;
        
#if UNITY_EDITOR
        public override void Draw()
        {
            base.Draw();
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("ユニット設定");
            GUILayout.Space(5);
            hp = EditorGUILayout.IntField("基礎HP", hp, GUILayout.Width(200));
            attack = EditorGUILayout.IntField("基礎攻撃力", attack, GUILayout.Width(200));
        }
#endif
    }
    
    [MessagePackObject(true)]
    public class EquipItem : Item
    {
#if UNITY_EDITOR
        [ColumnConfig(sortLabel ="Stat変更Objリスト", columnWidth = 200)]
        [AddColumnType("varchar(255)", "NULL")]
#endif
        public StatModifier[] modifiers;
        
#if UNITY_EDITOR
        public override void Draw()
        {
            base.Draw();
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("装備設定");
            GUILayout.Space(5);
            
            var mods = modifiers?.ToList();
            if(mods == null)
                mods = new List<StatModifier>();

            using (new GUILayout.VerticalScope())
            {
                if (GUILayout.Button("Add"))
                    mods.Add(new StatModifier());
                GUILayout.Space(8);

                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("X"))
                        {
                            mods.RemoveAt(i);
                            continue;
                        }

                        GUILayout.Space(8);

                        mods[i].Draw();
                        GUILayout.Space(8);
                    }
                }
            }

            modifiers = mods.ToArray();
        }
#endif
    }
}