using MD;
using MD.Tables;
using Sakaba.Domain;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
#endif

namespace Sakaba.Infra
{
    public sealed class FileMDRepository : IMDRepository
    {
        static string DIR_PATH => Application.dataPath + "/Sakaba/Resources/";
        const string DATABASE_NAME = "MD";
        static string DATABASE_FILENAME => $"{DATABASE_NAME}.bytes";
        
#if UNITY_EDITOR
        public void Save(DatabaseBuilder builder)
        {
            if (builder == null)
            {
                var md = new MemoryDatabase(
                    new ItemTable(new []{ new UnitItem{ id = "100", name = "デフォルトユニット" }}),
                    new ItemTierTable(new [] {new ItemTier()}));
                builder = md.ToDatabaseBuilder();
            }
            
            Debug.Log($"[Save DB] to {DIR_PATH + DATABASE_FILENAME}");
            using (var fs = new FileStream(
                DIR_PATH + DATABASE_FILENAME,
                System.IO.FileMode.OpenOrCreate,
                System.IO.FileAccess.Write)) 
            {
                builder.WriteToStream(fs);
            }
        }
#endif

        public MemoryDatabase Load()
        {
            Debug.Log($"[Load DB] from {DIR_PATH + DATABASE_FILENAME}");
            var bs = Resources.Load<TextAsset>(DATABASE_NAME);
            if (bs == null)
            {
                Debug.Log($"{DATABASE_NAME} == null");
                var db = new MemoryDatabase(
                    new ItemTable(new[] {new UnitItem {id = "100", name = "デフォルトユニット"}}),
                    new ItemTierTable(new [] {new ItemTier()}));
                // Save(db.ToDatabaseBuilder());
                return db;
            }
            return new MemoryDatabase(bs.bytes);
        }
    }
}