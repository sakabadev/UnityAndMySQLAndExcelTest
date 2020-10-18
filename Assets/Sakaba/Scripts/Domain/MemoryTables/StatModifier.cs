using MessagePack;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sakaba.Domain
{
    [MessagePackObject(true)]
    public sealed class StatModifier
    {
        public float value;
        public StatModType type;
        public ModTargetType targetType;
        
#if UNITY_EDITOR
        public void Draw()
        {
            value = EditorGUILayout.FloatField("変化量", value);
            type = (StatModType)EditorGUILayout.EnumPopup("値変更の計算方法", type);
            targetType = (ModTargetType)EditorGUILayout.EnumPopup("ターゲット値", targetType);
        }
#endif
    }
}