using Microsoft.Office.Interop.Excel;

namespace MD2DBFromExcel.Domain {
    public interface IWorksheetRepository {
        void Save(Worksheet sheet);
        void Find(Worksheet sheet);
    }
}
