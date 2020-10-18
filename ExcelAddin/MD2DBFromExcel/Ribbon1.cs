using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MD2DBFromExcel.UseCase;
using Microsoft.Office.Tools.Ribbon;

namespace MD2DBFromExcel {
    public partial class Ribbon1 {
        ExcelUseCase excelUseCase = new ExcelUseCase();

        private void Ribbon1_Load(object sender, RibbonUIEventArgs e) {
        }

        private void Export2DBButton_Click(object sender, RibbonControlEventArgs e) {
            var workbook = Globals.ThisAddIn.Application.ActiveWorkbook;
            excelUseCase.UpdateDBFromWorkbook(workbook);
        }

        private void ImportSelectedSheetButton_Click(object sender, RibbonControlEventArgs e) {
            var sheet = Globals.ThisAddIn.Application.ActiveSheet;
            excelUseCase.ImportSheetFromDB(sheet);
        }

        private void ExportSelectedSheetButton_Click(object sender, RibbonControlEventArgs e) {
            var worksheet = Globals.ThisAddIn.Application.ActiveSheet;
            excelUseCase.ExportSheetToDB(worksheet);
        }
    }
}
