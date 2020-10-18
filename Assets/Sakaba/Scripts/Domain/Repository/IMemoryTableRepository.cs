using System.Collections.Generic;

namespace Sakaba.Domain
{
    public interface IMemoryTableRepository
    {
        void SaveAll<T>(List<T> items);
        IReadOnlyList<T> FindAll<T>();
    }
}