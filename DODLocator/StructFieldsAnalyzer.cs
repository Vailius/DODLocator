using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DODLocator
{
    /// <summary>
    /// Analyzer for struct fields
    /// </summary>
    /// <typeparam name="T">Type of target struct</typeparam>
    public static class StructFieldsAnalyzer<T>
        where T : unmanaged
    {
        public static readonly bool IsValid = false;
        /// <summary>
        /// Field size of name
        /// </summary>
        public static readonly IReadOnlyDictionary<string, int> Size;
        /// <summary>
        /// Field type of name
        /// </summary>
        public static readonly IReadOnlyDictionary<string, Type> FieldType;
        /// <summary>
        /// Field identifier for indexing
        /// </summary>
        public static readonly IReadOnlyDictionary<string, int> Identifier;

        static StructFieldsAnalyzer()
        {
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var size = new Dictionary<string, int>();
            var ftype = new Dictionary<string, Type>();
            var id = new Dictionary<string, int>();

            Size = size;
            FieldType = ftype;
            Identifier = id;

            if (fields.Length == 0)
                return;

            foreach (var field in fields)
            {
                if (field.IsStatic)
                    continue;
                string name = field.Name;
                size.Add(name, Marshal.SizeOf(field.FieldType));
                ftype.Add(name, field.FieldType);
                id.Add(name, id.Count);
            }

            IsValid = id.Count > 0;
        }
    }
}