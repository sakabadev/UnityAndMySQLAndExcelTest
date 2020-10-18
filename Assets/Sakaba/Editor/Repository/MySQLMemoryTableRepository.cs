using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessagePack;
using MySql.Data.MySqlClient;
using Sakaba.Domain;
using Sakaba.Editor;
using UnityEngine;

namespace Sakaba.Infra
{
    public sealed class MySQLMemoryTableRepository : IMemoryTableRepository
    {
        public void SaveAll<T>(List<T> items)
        {
            // CreateTableはExcelから行うものとし、ここではUpdate系のみ実装します。
            MySQLHelper.AddColumnIfNotExistQuery<T>();

            foreach(T item in items)
                Save(item);
        }
        
        void Save<T>(T item)
        {
            var (insert, update, sqlParams) = GetInsertAndUpdateQuery(item);
            MySQLHelper.Execute(insert, update, sqlParams.ToArray()); 
        }

        (string insert, string update, List<MySqlParameter> sqlParams) GetInsertAndUpdateQuery<T>(T item)
        {
            Type thisType = item.GetType();
            
            List<MySqlParameter> sqlParams = new List<MySqlParameter>();
            var keyDic = EditorHelper.GetFieldNamesFrom<Item>();

            string[] names = keyDic[thisType].ToArray();

            StringBuilder valuesBuilder = new StringBuilder();
            StringBuilder updateValuesBuilder = new StringBuilder();
            foreach (var n in names)
            {
                var json = MessagePackSerializer.SerializeToJson(thisType.GetField(n).GetValue(item));
                json = json.TrimStart('"').TrimEnd('"');
                
                valuesBuilder.Append($"@{n}");
                
                if(n != "idx" || n != "id")
                    updateValuesBuilder.Append($"{n} = @{n}");
                
                sqlParams.Add(new MySqlParameter($"@{n}", $"{json}"));

                if (n != names[names.Length - 1])
                {
                    valuesBuilder.Append(", ");
                    if(n != "idx" || n != "id") 
                        updateValuesBuilder.Append(", ");
                }
            }

            string tableName = EditorHelper.GetTableNameFrom(thisType);

            string insert = $@"
INSERT INTO {tableName}
({string.Join(",", names).TrimEnd(',')})
VALUES
({valuesBuilder.ToString()});
";
            
            string update = $@"
UPDATE {tableName}
SET {updateValuesBuilder.ToString()}
WHERE id = @id;
";
            return (insert, update, sqlParams);
        }

        public IReadOnlyList<T> FindAll<T>()
        {
            List<T> items = new List<T>();
            if (typeof(T).IsAbstract)
            {
                var keyDic = EditorHelper.GetFieldNamesFrom<T>();
                foreach (var key in keyDic.Keys)
                {
                    items.AddRange(FindByTypeIsAbstract<T>(key));
                }
            }
            else
            {
                items.AddRange(FindByType<T>());
            }
            return items.AsReadOnly();
        }
        
        IReadOnlyList<T> FindByType<T>()
        {
            var type = typeof(T);
            var fieldNameDic = EditorHelper.GetFieldNamesFrom<T>(false);
            var tableNameDic = EditorHelper.GetFieldNamesFrom<T>();
            
            string sql = $@"SELECT * FROM {EditorHelper.GetTableNameFrom(type)} WHERE disable IS NULL;";
            Debug.Log(sql);

            return MySQLHelper.Query(sql, null,
                reader =>
                {
                    // jsonへ加工開始
                    StringBuilder jsonBuilder = new StringBuilder();
                    jsonBuilder.Append("{");
                    for (int i = 0; i < fieldNameDic[type].Count; i++)
                    {
                        try
                        {
                            var value = reader[tableNameDic[type][i]];
                            
                            if (value is string str)
                            {
                                if (!(str.Trim().StartsWith("[") || str.Trim().StartsWith("{")) 
                                    && !str.Trim().StartsWith("\""))
                                    value = $"\"{str}\"";
                                if (string.IsNullOrEmpty(str) || str == "\"null\"" || str == "null")
                                    value = "null";
                            }
                            else if(value == DBNull.Value)
                            {
                                value = "null";
                            }
                            
                            jsonBuilder.Append($"\"{fieldNameDic[type][i]}\" : {value},");
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"{fieldNameDic[type][i]} is Not Found");
                        }
                    }
                    jsonBuilder.Remove(jsonBuilder.Length-1, 1);
                    jsonBuilder.Append("}");
                    // jsonへ加工おわり

                    Debug.Log(jsonBuilder.ToString());
                    var temp2 = MessagePackSerializer.ConvertFromJson(jsonBuilder.ToString());
                    return MessagePackSerializer.Deserialize<T>(temp2);
                });
        }
        
        IReadOnlyList<T> FindByTypeIsAbstract<T>(Type type)
        {
            if (!type.IsSubclassOf(typeof(T)))
            {
                Debug.LogWarning($"誤ったタイプです。 {type.FullName}");
                return new List<T>().AsReadOnly();
            }
            
            // Union番号を特定
            var unions = (UnionAttribute[])Attribute.GetCustomAttributes(typeof(T), typeof (UnionAttribute));
            var unionPair = unions.ToDictionary(x => x.SubType.Name, x => x.Key);
            int union = unionPair[type.Name];
            
            var fieldNameDic = EditorHelper.GetFieldNamesFrom<T>(false);
            var tableNameDic = EditorHelper.GetFieldNamesFrom<T>();
            
            string sql = $@"SELECT * FROM {EditorHelper.GetTableNameFrom(type)} WHERE disable IS NULL";
            Debug.Log(sql);

            return MySQLHelper.Query(sql, null,
                reader =>
                {
                    // jsonへ加工開始
                    StringBuilder jsonBuilder = new StringBuilder();
                    jsonBuilder.Append("[" + union + ",{");
                    for (int i = 0; i < fieldNameDic[type].Count; i++)
                    {
                        try
                        {
                            var value = reader[tableNameDic[type][i]];
                            
                            if (value is string str)
                            {
                                if (!(str.Trim().StartsWith("[") || str.Trim().StartsWith("{")) 
                                    && !str.Trim().StartsWith("\""))
                                    value = $"\"{str}\"";
                                if (string.IsNullOrEmpty(str) || str == "\"null\"" || str == "null")
                                    value = "null";
                            }
                            else if(value == DBNull.Value)
                            {
                                value = "null";
                            }
                            
                            jsonBuilder.Append($"\"{fieldNameDic[type][i]}\" : {value},");
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"{fieldNameDic[type][i]} is Not Found");
                        }
                    }
                    jsonBuilder.Remove(jsonBuilder.Length-1, 1);
                    jsonBuilder.Append("}]");
                    // jsonへ加工おわり

                    Debug.Log(jsonBuilder.ToString());
                    var temp2 = MessagePackSerializer.ConvertFromJson(jsonBuilder.ToString());
                    return MessagePackSerializer.Deserialize<T>(temp2);
                });
        }
    }
}