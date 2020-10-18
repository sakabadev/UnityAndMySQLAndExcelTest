using System.Collections;
using MD;
using UnityEngine;

namespace Sakaba.Domain {
	public interface IMDRepository
    {
#if UNITY_EDITOR
        void Save(DatabaseBuilder builder);
#endif
        MemoryDatabase Load();
    }
}