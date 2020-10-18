using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace MD2DBFromExcel.Infrastructure {
	public static class MySQLHelper
	{
        const string MYSERVER = "server=127.0.0.1;"; // localhostは指定できない
        const string MYPORT = "port=3306;";
        const string MYDATABASE = "database=sakaba-game-1;";
        const string MYUSER = "userid=root;";
        const string MYPASS = "password=root;";
        const string MYCHARSET = "charset='utf8';";

        internal static string ConnectionCommand => MYSERVER + MYPORT + MYDATABASE + MYUSER + MYPASS + MYCHARSET;
        
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
    }
}