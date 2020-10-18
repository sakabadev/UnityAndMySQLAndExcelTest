using MD;
using Sakaba.Domain;
using Sakaba.Infra;
using UnityEditor;

namespace Sakaba.MDEditor
{
    public class MDEditorBase : EditorWindow
    {
        public static MemoryDatabase TempMD = null;
        public static bool isDirty;
        
        void OnEnable() {
            if (TempMD == null) {
                TempMD = GameDatabase.DB.ToImmutableBuilder().Build();
            }
            OnEnableAfter();
        }

        protected virtual void OnEnableAfter()
        {
        }
        
        public void SetMDDirty(string reason) {
            if (TempMD == null) return;

            var repo = new FileMDRepository();
            var builder = TempMD.ToDatabaseBuilder();
            repo.Save(builder);
            GameDatabase.SetDirty();
            var db = GameDatabase.DB; // DBの更新

            isDirty = false;
        }
    }
}