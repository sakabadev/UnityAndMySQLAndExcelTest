using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Sakaba.Domain;
using Sakaba.Domain.Helper;
using Sakaba.Infra;
using UnityEditor;
using UnityEngine;

namespace Sakaba.Editor
{
    public class ExcelFileCreator : EditorWindow
    {
        private Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>> memoryTableTypes;
        private Type current;
        
        [MenuItem("Sakaba/Open ExcelFileCreatorWindow", false, 1)]
        private static void Init()
        {
            var window = GetWindow(typeof(ExcelFileCreator));
            window.titleContent = new GUIContent("Excel File Creator");
            window.minSize = new Vector2(400, 200);
        }
        
        private void OnGUI()
        {
            if (GUILayout.Button("update MemoryTable list"))
                memoryTableTypes = EditorHelper.GetMemoryTableTypes();
            GUILayout.Space(10);
            
            if (memoryTableTypes == null || memoryTableTypes.Count <= 0)
            {
                GUILayout.Label("有効なMemoryTableクラスがありません");
                return;
            }

            List<Type> typeKeys = memoryTableTypes.Keys.ToList();
            string[] typeNames = memoryTableTypes.Select(x => x.Key.Name).ToArray();

            int typeIdx = typeKeys.IndexOf(current);
            if (typeIdx == -1) typeIdx = 0;
            current = typeKeys[EditorGUILayout.Popup(typeIdx, typeNames)];
            GUILayout.Space(20);
            
            if (GUILayout.Button ("create xlsx", GUILayout.Height(40)))
            {
                Debug.Log($"{nameof(CreateExcelFile)} : start");
                fileName = current.Name;
                filePath = DIR + fileName + ".xlsx";
                Directory.CreateDirectory(DIR);
                if (File.Exists(filePath))
                {
                    if (!EditorUtility.DisplayDialog("ファイルがすでに存在します。", "すでにあるファイルは削除して作成します。", "おーけー", "きゃんせる"))
                        return;

                    File.Delete(filePath);
                }
                
                CreateExcelFile(memoryTableTypes[current]);

                // AssetDatabase.ImportAsset (filePath);
                // AssetDatabase.Refresh (ImportAssetOptions.ForceUpdate);
                // Close ();
                Debug.Log($"{nameof(CreateExcelFile)} : end");
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button ("create table to MySql", GUILayout.Height(40))
            && EditorUtility.DisplayDialog("CREATEの前にDROP TABLEします", "フィールドを追加したい場合は各シートをコミットで行われます。", "おーけー", "きゃんせる"))
            {
                Debug.Log($"{nameof(CreateTableForMySql)} : start");
                CreateTableForMySql(memoryTableTypes[current]);
                Debug.Log($"{nameof(CreateTableForMySql)} : end");
            }
        }

        void CreateTableForMySql(Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>> sheetTypes)
        {
            foreach (var sheetType in sheetTypes)
            {
                string tableName = sheetType.Key.Name.ToLowercaseNamingConvention(true);
                
                string sql = GenerateCreateTableSql(tableName, sheetType.Value);
                Debug.Log("CreateTable: "+sql);
                MySQLHelper.Execute(sql, null);

                sql = GenerateCreateConfigSql(tableName, sheetType.Value);
                Debug.Log("CreateConfigTable: "+sql);
                MySQLHelper.Execute(sql, null);
                
                sql = GenerateInsertConfigSql(tableName, sheetType.Value);
                Debug.Log("CreateConfigTable: " + sql);
                MySQLHelper.Execute(sql, null);
            }
        }

        string GenerateCreateTableSql(string tableName, IEnumerable<IGrouping<Type, FieldInfo>> group) {
            StringBuilder sb = new StringBuilder();
            
            // 値テーブルの作成
            sb.Append($"DROP TABLE IF EXISTS `{tableName}`;");
            sb.Append($@"
CREATE TABLE IF NOT EXISTS `{tableName}` (
`idx` int(11) NOT NULL AUTO_INCREMENT,
`id` varchar(50) NOT NULL,
");

            var columnConfigList = new Dictionary<string, ColumnConfigAttribute>();
            var reverseGroup = group.Reverse();
            foreach (var infos in reverseGroup)
            foreach (var info in infos)
            {
                if(info.Name == "id" || info.Name == "idx")
                    continue;
                
                AddColumnTypeAttribute attr = (AddColumnTypeAttribute)Attribute.GetCustomAttribute(info, typeof(AddColumnTypeAttribute));
                if (attr == null){
                    Debug.LogError($"[AddColumnType]を設定して下さい。 {info.Name}");
                    return string.Empty;
                }

                ColumnConfigAttribute attr2 = (ColumnConfigAttribute)Attribute.GetCustomAttribute(info, typeof(ColumnConfigAttribute));
                if (attr2 == null){
                    Debug.LogError($"[ColumnConfig]を設定して下さい。 {info.Name}");
                    return string.Empty;
                }

                string columnName = info.Name.ToLowercaseNamingConvention(true);
                sb.Append($"`{columnName}` {string.Join(" ", attr.GetMembers())},");
                columnConfigList.Add(columnName, attr2); 
            }

            sb.Append($@"
`disable` int(1) NULL,
PRIMARY KEY (`idx`) USING BTREE,
UNIQUE KEY (`id`)
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;
");
            return sb.ToString();
        }
        
        string GenerateCreateConfigSql(string tableName, IEnumerable<IGrouping<Type, FieldInfo>> group) {
            StringBuilder sb = new StringBuilder();
            
            // configテーブルの作成
            sb.Append($"DROP TABLE IF EXISTS `{tableName}_config`;");
            sb.Append($@"
CREATE TABLE IF NOT EXISTS `{tableName}_config` (
`idx` int(11) NOT NULL AUTO_INCREMENT,
");
            
            List<string> configColNames = new List<string>();
            foreach (var info in typeof(ColumnConfigAttribute).GetFields())
            {
                var attr = (AddColumnTypeAttribute)Attribute.GetCustomAttribute(info, typeof(AddColumnTypeAttribute));
                if (attr == null){
                    Debug.LogError($"[AddColumnType]を設定して下さい。 {info.Name}");
                    return string.Empty;
                }

                if (info.Name == "sortLabel")
                {
                    // column_nameフィールドはsort_labelフィールドの手前に入れたい
                    configColNames.Add("column_name");
                    sb.Append($"`column_name` varchar(50) NOT NULL,");
                }
                
                sb.Append($"`{info.Name.ToLowercaseNamingConvention(true)}` {string.Join(" ", attr.GetMembers())},");
                configColNames.Add(info.Name.ToLowercaseNamingConvention(true));
            }
            
            sb.Append($@"
PRIMARY KEY (`idx`) USING BTREE,
UNIQUE KEY (`column_name`)
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;
");
            Debug.Log(sb.ToString());
            return sb.ToString();
        }
        
        string GenerateInsertConfigSql(string tableName, IEnumerable<IGrouping<Type, FieldInfo>> group) {
            StringBuilder sb = new StringBuilder();
            
            List<string> configColNames = new List<string>{"column_name"};
            foreach (var info in typeof(ColumnConfigAttribute).GetFields())
                configColNames.Add(info.Name.ToLowercaseNamingConvention(true));

            // configテーブルにInsert
            sb.Append($"INSERT INTO {tableName}_config (");
            sb.Append($"{string.Join(",", configColNames)}");
            sb.Append($") VALUES ");

            var reverseGroup = group.Reverse();
            foreach (var infos in reverseGroup)
            foreach (var info in infos)
            {
                if(info.Name == "idx")
                    continue;
                
                ColumnConfigAttribute attr2 = (ColumnConfigAttribute)Attribute.GetCustomAttribute(info, typeof(ColumnConfigAttribute));
                if (attr2 == null){
                    Debug.LogError($"[ColumnConfig]を設定して下さい。 {info.Name}");
                    return String.Empty;
                }

                string columnName = info.Name.ToLowercaseNamingConvention(true);
                sb.Append($"('{columnName}',");
                foreach (var field in typeof(ColumnConfigAttribute).GetFields())
                    sb.Append($"'{field.GetValue(attr2)}',");
                sb.Remove(sb.Length-1, 1);
                sb.Append($"),");
            }
            sb.Remove(sb.Length-1, 1);
            sb.Append($";");

            return sb.ToString();
        }

        string DIR => Application.dataPath + "/../Excels/";
        private string filePath = string.Empty;
        private string fileName = string.Empty;

        async void CreateExcelFile(Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>> sheetTypes)
        {
            IWorkbook workbook;
            // ファイルがなかったらファイル作成
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                workbook = new XSSFWorkbook();
                workbook.Write(fs);
            }
            await Task.Delay(1);


            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                workbook = new XSSFWorkbook(fs);

            IRow row;
            ICell cell;
            foreach (var sheetType in sheetTypes)
            {
                string sheetName = sheetType.Key.Name.ToLowercaseNamingConvention(true);
                ISheet sheet = workbook.GetSheet(sheetName) ?? workbook.CreateSheet(sheetName);
                
                row = sheet.GetRow(0) ?? sheet.CreateRow(0);
                cell = row.GetCell(1) ?? row.CreateCell(1);
                cell.SetCellType(CellType.String);
                cell.SetCellValue("アドインから同期して下さい。");
                
                // // 一番左列の設定作成
                // row = sheet.GetRow(1) ?? sheet.CreateRow(1);
                // cell = row.GetCell(0) ?? row.CreateCell(0);
                // cell.SetCellType(CellType.String);
                // cell.SetCellValue("key");
                //
                // row = sheet.GetRow(3) ?? sheet.CreateRow(3);
                // cell = row.GetCell(0) ?? row.CreateCell(0);
                // cell.SetCellType(CellType.String);
                // cell.SetCellValue("value");
                //
                // // 列作成
                // var list = sheetType.Value.Reverse();
                // int colCount = 1;
                // foreach (var list2 in list)
                // foreach (var field in list2)
                // {
                //     int rowCount = 0;
                //     
                //     ColumnConfigAttribute attr2 = (ColumnConfigAttribute)Attribute.GetCustomAttribute(field, typeof(ColumnConfigAttribute));
                //     // フィールド設定リストの取得
                //     foreach (var confField in typeof(ColumnConfigAttribute).GetFields())
                //     {
                //         if(confField.Name == "sortLabel")
                //             continue;
                //         
                //         row = sheet.GetRow(rowCount) ?? sheet.CreateRow(rowCount);
                //         cell = row.GetCell(colCount) ?? row.CreateCell(colCount);
                //         cell.SetCellType(CellType.String);
                //         cell.SetCellValue(confField.GetValue(attr2).ToString());
                //         rowCount++;
                //     }
                //
                //     // MySQLのカラム名となるフィールド名をAdd
                //     row = sheet.GetRow(rowCount) ?? sheet.CreateRow(rowCount);
                //     cell = row.GetCell(colCount) ?? row.CreateCell(colCount);
                //     cell.SetCellType(CellType.String);
                //     cell.SetCellValue(field.Name.ToLowercaseNamingConvention(true));
                //     rowCount++;
                //     
                //     colCount++;
                // }
            }
            
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                workbook.Write(stream);
        }
        
    }
}