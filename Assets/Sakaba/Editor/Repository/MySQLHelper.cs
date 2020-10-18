
using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using MySql.Data.MySqlClient;
using Sakaba.Domain;
using Sakaba.Domain.Helper;
using Sakaba.Editor;
using UnityEngine;

namespace Sakaba.Infra {
	public static class MySQLHelper
	{
        const string MYSERVER = "server=127.0.0.1;"; // localhostは指定できない
        const string MYPORT = "port=3306;";
        const string MYDATABASE = "database=sakaba-game-1;";
        const string MYUSER = "userid=root;";
        const string MYPASS = "password=root;";
        const string MYCHARSET = "charset='utf8';";
        static string ConnectionCommand => MYSERVER + MYPORT + MYDATABASE + MYUSER + MYPASS + MYCHARSET;
        
        internal static IReadOnlyList<T> Query<T>(
            string sql
            , MySqlParameter[] parameters
            , Func<MySqlDataReader, T> createEntity)
        {
            var result = new List<T>();
            using (var connection = new MySqlConnection(ConnectionCommand))
            using (var command = new MySqlCommand(sql, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(createEntity(reader));
                    }
                }
            }
            return result.AsReadOnly();
        }
        
        internal static void Execute(string sql, MySqlParameter[] parameters)
        {
            using (var connection = new MySqlConnection(ConnectionCommand))
            using (var command = new MySqlCommand(sql, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                command.ExecuteNonQuery();
            }
        }

        internal static void Execute(string insert, string update, MySqlParameter[] parameters)
        {
            using (var connection = new MySqlConnection(ConnectionCommand))
            using (var command = new MySqlCommand(update, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                // 対象があったらupdate
                if (command.ExecuteNonQuery() < 1)
                {
                    // 対象がなかったらinsert
                    command.CommandText = insert;
                    command.ExecuteNonQuery();
                }
            }
        }
        
        internal static void AddColumnIfNotExistQuery<T>()
        {
            var dic = EditorHelper.GetFieldNamesAndTypesFrom<T>();
            using (var connection = new MySqlConnection(ConnectionCommand))
            {
                connection.Open();
                MySqlScript script = new MySqlScript(connection);

                StringBuilder sb = new StringBuilder();
                sb.Append("DROP PROCEDURE IF EXISTS alter_table_procedure??");
                sb.Append("CREATE PROCEDURE alter_table_procedure() ");
                sb.Append("BEGIN ");
                
                foreach (var data in dic)
                {
                    foreach (var v in data.Value)
                    {
                        sb.Append($"IF NOT EXISTS (select * from information_schema.COLUMNS where table_name = '{data.Key}' and column_name = '{v.Item1}') THEN ");
                        sb.Append($"ALTER TABLE {data.Key} ADD COLUMN {v.Item1} {string.Join(" ", v.Item2)}; ");
                        sb.Append($"END IF; ");
                    }
                }
                
                sb.Append("END ?? ");
                sb.Append("CALL alter_table_procedure();");
                
                script.Query = sb.ToString();
                script.Delimiter = "??";
                script.Execute();
                Debug.Log($"Query: {script.Query}");

                script.Delimiter = ";";
                script.Query = "DROP PROCEDURE alter_table_procedure;";
                script.Execute();
                Debug.Log($"Query: {script.Query}");
            }
        }
    }
}