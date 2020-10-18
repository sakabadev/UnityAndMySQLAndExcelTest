using System;
using MD;
using Sakaba.Infra;
using UnityEngine;

namespace Sakaba.Domain {
	public sealed class GameDatabase
    {
        static bool isDirty = true;
        private static IMDRepository repo;
        
        static MemoryDatabase cache = null;
        public static MemoryDatabase DB {
            get {
                if (cache == null || isDirty) {
                    if(repo == null)
                        repo = new FileMDRepository();

                    try
                    {
                        cache = repo.Load();
                        // nullならわざとエラーを出す
                        Debug.Log($"DB is {cache.ItemTable.Count}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    finally
                    {
                        isDirty = false;
                    }
                }
                return cache;
            }
        }

        public static void SetDirty()
        {
            isDirty = true;
        }
    }
}