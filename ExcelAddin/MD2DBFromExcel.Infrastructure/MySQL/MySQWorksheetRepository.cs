using System;
using System.Collections.Generic;
using System.Text;
using MD2DBFromExcel.Domain;
using Microsoft.Office.Interop.Excel;
using Constants = MD2DBFromExcel.Domain.Constants.Constants;
using Excel = Microsoft.Office.Interop.Excel;

namespace MD2DBFromExcel.Infrastructure {
    public sealed class MySQLWorksheetRepository : IWorksheetRepository {
        // Create、Add ColumnはUnityEditorで行うため、ここではUpdate、Insert
        public void Save(Worksheet sheet) {
            int keyRowPos = 1;
            int valueStartRowPos = 1;

            string sql = string.Empty;
            // keyの行を取得
            while (keyRowPos < sheet.Rows.Count) {
                if (sheet.Cells[++keyRowPos, 1]?.Value?.ToString() == "key")
                    break;

                // idのテーブル名が見つからない場合、テーブルの構造がおかしいのでSqlは実行しない
                if (keyRowPos > 10) {
                    System.Diagnostics.Debug.WriteLine("keyのセルが見つかるまでが長すぎます。");
                    return;
                }
            }

            // valueの開始行を取得
            valueStartRowPos = keyRowPos;
            while (valueStartRowPos < sheet.Rows.Count) {
                if (sheet.Cells[++valueStartRowPos, 1]?.Value?.ToString() == "value")
                    break;

                // idのテーブル名が見つからない場合、テーブルの構造がおかしいのでSqlは実行しない
                if (valueStartRowPos > 10) {
                    System.Diagnostics.Debug.WriteLine("valueのセルが見つかるまでが長すぎます。");
                    return;
                }
            }


            // Insert or Update
            try {
                sql = GenerateUpdateSql(keyRowPos, valueStartRowPos, sheet);
                System.Diagnostics.Debug.WriteLine(sql);

                MySQLHelper.Execute(sql, null);

            } catch (System.Exception) {
                throw;
            }
        }

        string GenerateUpdateSql(int keyRow, int valueStartRow, Worksheet sheet) {
            StringBuilder sb = new StringBuilder();
            // ループカウントの都合上1減らす
            int colResetValue = 2 - 1;
            int col = colResetValue;

            List<string> cells = new List<string>();

            // nameを取得
            while (sheet.Cells[keyRow, ++col]?.Value != null) {
                cells.Add(sheet.Cells[keyRow, col]?.Value.ToString());
            }
            string[] names = cells.ToArray();

            sb.Append($@"INSERT INTO `{sheet.Name}` ({string.Join(",", cells).TrimEnd(',')}) VALUES ");
            int colCount = colResetValue + cells.Count;
            cells.Clear();

            col = colResetValue;
            int row = valueStartRow;
            while (sheet.Cells[row, col + 1]?.Value != null) {
                while (col < colCount) {
                    cells.Add(CheckIsString(sheet.Cells[row, ++col]?.Value));
                }
                sb.Append($@"({string.Join(",", cells).TrimEnd(',')})");

                cells.Clear();
                col = colResetValue;
                if (sheet.Cells[++row, col + 1]?.Value != null) {
                    sb.Append(",");
                }
            }

            sb.Append($@" ON DUPLICATE KEY UPDATE ");
            for (int i = 0; i < names.Length; i++) {
                if (names[i] == "idx" || names[i] == "id")
                    continue;

                sb.Append($"{names[i]} = VALUES({names[i]})");
                if (i < names.Length - 1)
                    sb.Append(",");
            }
            sb.Append(";");
            return sb.ToString();
        }

        string CheckIsString(dynamic value) {
            if (value == null) return "NULL";
            if (value.GetType() == typeof(double)) return value.ToString();
            return $"'{value.ToString()}'";
        }

        /// <summary>
        /// 暫定で副作用で対応してます
        /// </summary>
        /// <param name="sheet"></param>
        public void Find(Worksheet sheet) {
            string[] configLabels = Constants.ConfigLables;

            int sortLabelRow = 1;
            for (int i = 1; i < configLabels.Length; i++) {
                if(configLabels[i] == "column_name") {
                    sheet.Cells[i + 1, 1].Value = "key";
                }
                if (configLabels[i] == "sort_label") {
                    sheet.Cells[i + 2, 1].Value = "value";
                    sortLabelRow = i + 1;
                }
            }

            int row = 1;
            int col = 2;
            List<string> columnNames = new List<string>();
            List<string> sortLabels = new List<string>();
            List<int> ignoreColumns = new List<int>();
            string sql = $@"SELECT * FROM {sheet.Name}_config";
            // configを取得
            MySQLHelper.Query(sql, null,
                reader => {
                    row = 1;
                    foreach (string label in configLabels) {
                        sheet.Cells[row++, col].Value = reader[label];
                        if (label == "column_name")
                            columnNames.Add((string)reader[label]);
                        if (label == "column_width")
                            sheet.Columns[col].ColumnWidth = 18 * ((int)reader[label] / 100.0);
                        if (label == "prefer_excel" && (int)reader[label] == 1)
                            ignoreColumns.Add(col);

                    }
                    col++;
                    return configLabels;
                });

            sql = $@"SELECT * FROM {sheet.Name}";
            // configを取得
            MySQLHelper.Query(sql, null,
                reader => {
                    col = 2;
                    foreach (string name in columnNames) {
                        if (sheet.Cells[row, col].Value == null || !ignoreColumns.Contains(col))
                            sheet.Cells[row, col].Value = reader[name];
                        col++;
                    }
                    row++;
                    return configLabels;
                });

            // sheetを取得
            if (sheet.ListObjects.Count <= 0 || sheet.ListObjects["TestTable"] == null) {
                sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, sheet.Range[sheet.Cells[sortLabelRow, 2], sheet.Cells[row - 1, col - 1]],
                Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing).Name = "TestTable";
                sheet.ListObjects["TestTable"].TableStyle = "TableStyleMedium3";
            }
        }
    }
}
