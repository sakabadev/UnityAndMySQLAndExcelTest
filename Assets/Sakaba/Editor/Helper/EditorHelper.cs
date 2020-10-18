using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MasterMemory;
using MessagePack;
using Sakaba.Domain;
using Sakaba.Domain.Helper;
using UnityEngine;

namespace Sakaba.Editor
{
    public class EditorHelper
    {
        static Dictionary<Type, List<Type>> derivedTypeCache = new Dictionary<Type, List<Type>>();
        internal static Dictionary<Type, List<string>> GetFieldNamesFrom<T>(bool isLowercaseNamingConvention = true)
        {
            Dictionary<Type, List<string>> keyDic = new Dictionary<Type, List<string>>();
            var t = typeof(T);
            if (t.IsAbstract)
            {
                if (!derivedTypeCache.ContainsKey(t))
                    CacheDerivedTypes<T>();
                // Typeのフィールド名を特定
                foreach (var type in derivedTypeCache[t])
                    keyDic.Add(type, GetFieldNameList(type, isLowercaseNamingConvention));
            }
            else
            {
                keyDic.Add(t, GetFieldNameList(t, isLowercaseNamingConvention));
            }
            return keyDic;
        }

        static List<string> GetFieldNameList(Type t, bool isLowercaseNamingConvention = true)
        {
            List<string> result = new List<string>();
            foreach (var field in t.GetFields())
            {
                IgnoreMemberAttribute ignoreAttr = (IgnoreMemberAttribute)Attribute.GetCustomAttribute(field, typeof(IgnoreMemberAttribute));
                // [IgnoreMember]じゃなく、publicなフィールドのみDBに入れる
                if (ignoreAttr == null
                    && field.IsPublic
                    && !field.IsStatic
                )
                    result.Add(
                        isLowercaseNamingConvention ? field.Name.ToLowercaseNamingConvention(true) : field.Name);
            }
            return result;
        }

        /// <summary>
        /// 全てのAssemblyからTの実装を検索。重いので一度検索したTypeはキャッシュする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        static void CacheDerivedTypes<T>()
        {
            if (derivedTypeCache == null)
                derivedTypeCache = new Dictionary<Type, List<Type>>();
            
            derivedTypeCache.Add(typeof(T), new List<Type>());
            var types = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(T));
            foreach (var type in types)
            {
                // 実装されているもののみ有効
                if (!type.IsAbstract)
                    derivedTypeCache[typeof(T)].Add(type);
            }
        }

        internal static IReadOnlyDictionary<string, List<(string, string[])>> GetFieldNamesAndTypesFrom<T>()
        {
            var res = new Dictionary<string, List<(string, string[])>>();
            var t = typeof(T);
            if (t.IsAbstract)
            {
                if (!derivedTypeCache.ContainsKey(typeof(T)))
                    CacheDerivedTypes<T>();

                // Typeのフィールド名を特定
                foreach (var type in derivedTypeCache[typeof(T)])
                {
                    string tableName = GetTableNameFrom(type);
                    res.Add(tableName, GetFieldNamesAndTypesFrom(type));
                }
            }
            else
            {
                res.Add(GetTableNameFrom(t)
                    , GetFieldNamesAndTypesFrom(t));
            }
            return res;
        }

        static List<(string, string[])> GetFieldNamesAndTypesFrom(Type t)
        {
            List<(string, string[])> res = new List<(string, string[])>();
            foreach (var field in t.GetFields())
            {
                AddColumnTypeAttribute attr =
                    (AddColumnTypeAttribute) Attribute.GetCustomAttribute(field,
                        typeof(AddColumnTypeAttribute));
                // [IgnoreMember]じゃなく、publicなフィールドのみDBに入れる
                if (attr != null
                    && field.IsPublic
                    && !field.IsStatic
                )
                    res.Add((field.Name.ToLowercaseNamingConvention(true), attr.GetMembers()));
            }
            return res;
        }
        
        internal static string GetTableNameFrom(Type type)
        {
            string[] arr = type.Name.Split('.');
            return arr[arr.Length - 1].ToLowercaseNamingConvention(true);
        }
        
        
        /// <summary>
        /// Dic keyのType = Book名
        /// IGroupingのType = Sheet名
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>> GetMemoryTableTypes()
        {
            Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>> res = new Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var t in types)
                    {
                        var attr = (MemoryTableAttribute)Attribute.GetCustomAttribute(t, typeof(MemoryTableAttribute));
                        if (attr != null)
                        {
                            // memo: 基底クラスより先にサブクラスが読まれた場合の想定はしてない
                            
                            var group = t
                                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                                .GroupBy(x => x.DeclaringType);

                            if (group.Count() == 1)
                            {
                                res.Add(t, new Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>());
                                if(!t.IsAbstract)
                                    res[t].Add(t, group);
                            }
                            else
                            {
                                foreach (var res2 in res)
                                {
                                    // どれかの派生クラスだったらそこにAdd
                                    if (t.IsSubclassOf(res2.Key))
                                    {
                                        res2.Value.Add(t, group);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"{assembly.FullName} failed GetTypes()");
                }
            }
            return res;
        }
        
        /// <summary>
        /// 特定のMemoryTableクラスからMySQLへCreate Tableするための情報を収集します
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>> GetMemoryTableTypes<T>()
        {
            if (derivedTypeCache == null)
                derivedTypeCache = new Dictionary<Type, List<Type>>();
            
            if (!derivedTypeCache.ContainsKey(typeof(T)))
                CacheDerivedTypes<T>();

            Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>> res =
                new Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>();
            
            // memo: 基底クラスより先にサブクラスが読まれた場合の想定はしてない
            var t = typeof(T);
            var group = t
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .GroupBy(x => x.DeclaringType)
                .ToList();

            if (group.Count() == 1)
            {
                if(!t.IsAbstract)
                    res.Add(t, group);
            }
            else
            {
                if(t.IsSubclassOf(typeof(T)))
                    res.Add(t, group);
            }
            return res;
        }
    }
}