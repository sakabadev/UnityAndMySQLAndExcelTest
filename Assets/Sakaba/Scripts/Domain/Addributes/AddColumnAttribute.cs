#if UNITY_EDITOR
using System.Collections.Generic;

namespace Sakaba.Domain {
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class AddColumnTypeAttribute : System.Attribute
    {
        // MySQLのカラムの型
        public string type;
        // MySQLのカラムのオプション
        public string[] options;
        
        public AddColumnTypeAttribute(string type, params string[] options)
        {
            this.type = type;
            this.options = options;
        }

        public string[] GetMembers()
        {
            List<string> l = new List<string>();
            l.Add(type);
            l.AddRange(options);
            return l.ToArray();
        }
    }
}
#endif