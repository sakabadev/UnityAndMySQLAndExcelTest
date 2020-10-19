using MasterMemory;
using MessagePack;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sakaba.Domain
{
    [MemoryTable(nameof(ItemTier)), MessagePackObject(true)]
    public class ItemTier
    {
#if UNITY_EDITOR
        [ColumnConfig(sortLabel="キー", columnWidth = 20, preferExcel = 1)]
        [AddColumnType("varchar(50)","NOT NULL")]
#endif
        public string id;
        
#if UNITY_EDITOR
        [ColumnConfig(sortLabel="名前", preferExcel = 1)]
        [AddColumnType("varchar(255)","NOT NULL")]
#endif
        public string name;
        
#if UNITY_EDITOR
        [ColumnConfig(sortLabel="基礎値段", preferExcel = 1, columnWidth = 20)]
        [AddColumnType("int(11)","NOT NULL", "DEFAULT 10")]
#endif
        public int price;

        [PrimaryKey, IgnoreMember]
        public string Id => id;
        
        
#if UNITY_EDITOR
        public void Draw()
        {
            EditorGUILayout.LabelField(id);
            EditorGUILayout.LabelField("名前", name);
            EditorGUILayout.LabelField("基礎価格", price.ToString());
        }
#endif
    }
}