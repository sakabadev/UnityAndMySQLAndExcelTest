using MD2DBFromExcel.Domain;
using MD2DBFromExcel.Infrastructure;
using Excel = Microsoft.Office.Interop.Excel;

namespace MD2DBFromExcel.UseCase {
    public class ExcelUseCase {
        IWorksheetRepository repo;

        public ExcelUseCase() : this(new MySQLWorksheetRepository()) { }

        public ExcelUseCase(IWorksheetRepository repo) {
            this.repo = repo;
        }

        internal void UpdateDBFromWorkbook(Excel.Workbook workbook) {
            foreach (Excel.Worksheet sheet in workbook.Worksheets) {
                ExportSheetToDB(sheet);
            }
        }

        internal void ExportSheetToDB(Excel.Worksheet sheet) {
            repo.Save(sheet);
        }

        internal void ImportSheetFromDB(Excel.Worksheet sheet) {
            repo.Find(sheet);
        }
    }
}
