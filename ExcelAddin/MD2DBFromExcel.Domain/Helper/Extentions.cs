using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Sakaba.Domain.Helper
{
    public static class Extentions
    {
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
                    Debug.WriteLine($"{assembly.FullName} failed GetTypes()");
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
    }
}