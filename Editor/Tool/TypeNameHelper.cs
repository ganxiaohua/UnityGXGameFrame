using System;
using System.Linq;

namespace GameFrame.Editor
{
    public static class TypeNameHelper
    {
        public static string GetFullNameWithoutAssemblyDetails(Type type)
        {
            if (type == null) return "null";


            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return $"{GetFullNameWithoutAssemblyDetails(elementType)}[]";
            }


            if (type.IsPointer)
            {
                var elementType = type.GetElementType();
                return $"{GetFullNameWithoutAssemblyDetails(elementType)}*";
            }


            if (type.IsByRef)
            {
                var elementType = type.GetElementType();
                return $"{GetFullNameWithoutAssemblyDetails(elementType)}&";
            }

            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();

                string baseName = GetNamespaceQualifiedName(genericDef);


                var argNames = type.GetGenericArguments()
                        .Select(GetFullNameWithoutAssemblyDetails);

                return $"{baseName}<{string.Join(", ", argNames)}>";
            }


            return GetNamespaceQualifiedName(type);
        }


        private static string GetNamespaceQualifiedName(Type type)
        {
            var builtInNames = new System.Collections.Generic.Dictionary<Type, string>
            {
                    [typeof(void)] = "void",
                    [typeof(object)] = "object",
                    [typeof(bool)] = "bool",
                    [typeof(char)] = "char",
                    [typeof(string)] = "string",
                    [typeof(sbyte)] = "sbyte",
                    [typeof(byte)] = "byte",
                    [typeof(short)] = "short",
                    [typeof(ushort)] = "ushort",
                    [typeof(int)] = "int",
                    [typeof(uint)] = "uint",
                    [typeof(long)] = "long",
                    [typeof(ulong)] = "ulong",
                    [typeof(float)] = "float",
                    [typeof(double)] = "double",
                    [typeof(decimal)] = "decimal",
            };

            if (builtInNames.TryGetValue(type, out var keyword))
            {
                return type.FullName ?? keyword;
            }

            if (type.IsNested)
            {
                var declaringType = type.DeclaringType;
                var parentName = GetNamespaceQualifiedName(declaringType);
                return $"{parentName}.{type.Name.Split('`')[0]}";
            }

            string name = type.Name.Split('`')[0];
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                return $"{type.Namespace}.{name}";
            }

            return name;
        }
    }
}