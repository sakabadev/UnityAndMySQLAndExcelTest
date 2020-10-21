using System.Collections.Generic;
using System.Threading.Tasks;
using MD;
using MessagePack;
using Sakaba.Domain;
using Sakaba.Infra;
using UnityEngine;

namespace Sakaba.MDEditor
{
    public class MDEditorUseCase
    {
        private IMDRepository MdRepository;
        private IMemoryTableRepository TableRepository;

        public MDEditorUseCase() : this(new FileMDRepository(), new MySQLMemoryTableRepository()) {}
        public MDEditorUseCase(
            IMDRepository mdRepository,
            IMemoryTableRepository tableRepository)
        {
            this.MdRepository = mdRepository;
            this.TableRepository = tableRepository;
        }
        
        public async void ImportTable<T>(
            System.Action<ImmutableBuilder, IReadOnlyList<T>> replaceItemsFromMD,
            System.Action onComplete)
        {
            Debug.Log($"[{nameof(ImportTable)}] Start");
            
            var items = TableRepository.FindAll<T>();
            Debug.Log(MessagePackSerializer.SerializeToJson(items));
            
            if(GameDatabase.DB == null)
                MdRepository.Save(null);

            var builder = GameDatabase.DB.ToImmutableBuilder();
            replaceItemsFromMD(builder, items);

            // Editorの一時保存してるDBを更新
            MDEditorBase.TempMD = builder.Build();
            // 本番用DBも更新
            MdRepository.Save(MDEditorBase.TempMD.ToDatabaseBuilder());
            
            // Fileにセーブ直後にReloadでFileを読み込もうとすると動作不安定感があったためDelay
            await Task.Delay(1);
            GameDatabase.Reload();

            onComplete.Invoke();
            Debug.Log($"[{nameof(ImportTable)}] End");
        }
        
        public void ExportTable<T>(List<T> items)
        {
            Debug.Log($"[{nameof(ExportTable)}] Start");
            if (GameDatabase.DB == null)
            {
                MdRepository.Save(null);
                return;
            }
            if(GameDatabase.DB.ItemTierTable == null)
                return;
            
            TableRepository.SaveAll(items);
            Debug.Log($"[{nameof(ExportTable)}] End");
        }
    }
}