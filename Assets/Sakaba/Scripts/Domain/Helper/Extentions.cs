using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Sakaba.Domain.Helper
{
    public static class Extentions
    {
        /// <summary>
        /// Typeを継承しているクラスを探索する
        /// </summary>
        /// <param name="aAppDomain"></param>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
        {
            var result = new List<System.Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                // assemblyがMySql.Dataの時などにGetTypesに失敗したので、一部は無視する
                // 多分、MySql Connectorをインポートした時にエラーが出なかったDllはプロジェクトに入れなかったからかな？
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsSubclassOf(aType))
                            result.Add(type);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"{assembly.FullName} failed GetTypes()");
                }
            }
            return result.ToArray();
        }
        
        public static string ToLowercaseNamingConvention(this string s, bool toLowercase)
        {
            if (toLowercase)
            {
                var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
                return r.Replace(s, "_").ToLower();
            }
            else
                return s;
        }
        
        public static string Camelize(this string str) {
            string result = "";
        
            string[] strArray = str.Split('_');
            for (int i = 0; i < strArray.Length; i++)
            {
                if(i == 0)
                    result += strArray[i].Substring(0, 1).ToLower() + strArray[i].Substring(1);
                else
                    result += strArray[i].Substring(0, 1).ToUpper() + strArray[i].Substring(1);
            }
            return result;
        }

    }
}