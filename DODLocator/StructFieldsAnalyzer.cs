using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DODLocator
{
    internal static class StructFieldsAnalyzer<T>
        where T : unmanaged
    {
        public static readonly bool IsValid = false;
        public static readonly IReadOnlyDictionary<string, int> Size;
        public static readonly IReadOnlyDictionary<string, Type> FieldType;
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