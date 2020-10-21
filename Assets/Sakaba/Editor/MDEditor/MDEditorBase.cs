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
        public static MDEditorUseCase UseCase;
        
        void OnEnable() {
            if (TempMD == null)
                TempMD = GameDatabase.DB.ToImmutableBuilder().Build();
            
            if(UseCase == null)
                UseCase = new MDEditorUseCase();
            
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
            GameDatabase.Reload();
            isDirty = false;
        }
    }
}