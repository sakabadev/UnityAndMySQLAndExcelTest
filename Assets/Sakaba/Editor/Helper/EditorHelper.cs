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
            // <Book, <Sheet, Group<Class, Fields>>>
            Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>> res
                = new Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>>();
            
            List<Type> notAbstractTypes = new List<Type>();

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
                            Debug.Log($"<color=yellow>Match: {t.Name}</color>");
                            // Abstractのクラスを前もって抽出
                            if (!t.IsAbstract)
                                notAbstractTypes.Add(t);
                            else
                                res.Add(t, new Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>());
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"{assembly.FullName} failed GetTypes()");
                }
            }
            
            foreach (var t in notAbstractTypes)
            {
                // 親子関係にあるクラス毎にfieldをまとめる。
                var group = t
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .GroupBy(x => x.DeclaringType);

                bool isSubclass = false;
                foreach (var res2 in res)
                {
                    // 既にリストに載ってるどれかの実装クラスだったらそこにAdd
                    if (t.IsSubclassOf(res2.Key))
                    {
                        // Debug.Log($"<color=green>{t.Name} is SubClass {res2.Key.Name} and Add</color>");
                        res2.Value.Add(t, group);
                        isSubclass = true;
                        break;
                    }
                }
                
                if(isSubclass) continue;

                // 新参ものだったらKeyから作る
                res.Add(t, new Dictionary<Type, IEnumerable<IGrouping<Type, FieldInfo>>>());
                res[t].Add(t, group);
                // Debug.Log($"<color=blue>Add: {t.Name}</color>");
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