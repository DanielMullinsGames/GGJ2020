using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Peppermint.DataBinding.Editor
{
    public static class BindingEditorUtility
    {
        public static readonly string[] UniversalTypeNames = { "System.Boolean", "System.Byte", "System.SByte", "System.Char", "System.Decimal", "System.Double", "System.Single", "System.Int32", "System.UInt32", "System.Int64", "System.UInt64", "System.Object", "System.Int16", "System.UInt16", "System.String" };
        public static readonly string[] CSharpTypeNames = { "bool", "byte", "sbyte", "char", "decimal", "double", "float", "int", "uint", "long", "ulong", "object", "short", "ushort", "string" };

        public const string ScriptAssembliesDirectory = "Library/ScriptAssemblies";

        public static string GetPropertyName(string input)
        {
            var result = input.Substring(0, 1).ToUpper() + input.Substring(1);
            return result;
        }

        public static string GetGenericArgumentString(Type type, bool fullName = false)
        {
            var sb = new StringBuilder();

            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                sb.Append("<");

                int index = 0;
                foreach (var item in genericArgs)
                {
                    if (index != 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(item.GetFriendlyTypeName(fullName));
                    index++;
                }

                sb.Append(">");
            }

            return sb.ToString();
        }

        public static string GetCSharpTypeName(Type type)
        {
            int index = Array.FindIndex(UniversalTypeNames, x => x == type.FullName);

            if (index == -1)
            {
                // no match
                return null;
            }

            return CSharpTypeNames[index];
        }

        public static string GetFriendlyTypeName(this Type type, bool fullName = false)
        {
            string csName = GetCSharpTypeName(type);
            if (csName != null)
            {
                return csName;
            }

            if (type.IsGenericType)
            {
                var name = fullName ? type.FullName : type.Name;
                var typeName = name.Remove(name.IndexOf('`'));

                string arguments = GetGenericArgumentString(type, fullName);

                // combine
                string result = string.Format("{0}{1}", typeName, arguments);
                result = result.Replace('+', '.');
                return result;
            }
            else
            {
                var result = fullName ? type.FullName : type.Name;
                result = result.Replace('+', '.');
                return result;
            }
        }

        public static bool IsBinder(this Type type)
        {
            return type.IsDefined(typeof(BinderAttribute), false);
        }

        public static List<Type> GetClassByName(string typeName)
        {
            var typeList = GetScriptTypeList();

            var result = typeList.Where(x =>
            {
                if (!x.IsClass)
                {
                    return false;
                }

                if (x.Name == typeName || x.FullName == typeName)
                {
                    // full match
                    return true;
                }

                // check partial match
                var nameString = x.FullName;

                if (nameString.EndsWith(typeName))
                {
                    return true;
                }

                return false;
            }).ToList();

            return result;
        }


        public static List<Type> GetImplicitOperatorTypes(bool twoWay)
        {
            var dictionary = new Dictionary<Tuple<Type, Type>, MethodInfo>();

            // search all types
            var types = BindingEditorUtility.GetAllTypeList();

            foreach (var type in types)
            {
                if (!type.IsPublic)
                {
                    continue;
                }

                if (type.IsGenericTypeDefinition)
                {
                    continue;
                }

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

                foreach (var method in methods)
                {
                    if (method.Name != "op_Implicit")
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        // with 1 parameter
                        continue;
                    }

                    var parameter = parameters[0];

                    // add operator
                    var key = Tuple.Create(parameter.ParameterType, method.ReturnType);
                    dictionary.Add(key, method);
                }
            }

            var resultList = new List<Type>();

            foreach (var item in dictionary)
            {
                var key = item.Key;

                if (twoWay)
                {
                    var pairedKey = Tuple.Create(key.Item2, key.Item1);

                    // check convert back
                    MethodInfo convertBackMethod = null;
                    dictionary.TryGetValue(pairedKey, out convertBackMethod);

                    if (convertBackMethod == null)
                    {
                        // skip one-way
                        continue;
                    }
                }

                var type = item.Value.DeclaringType;

                if (!resultList.Contains(type))
                {
                    // add current type
                    resultList.Add(type);
                }
            }

            return resultList;
        }

        public static List<string> GetScriptAssemblyNameList()
        {
            var list = new List<string>();

            // get dll files
            var files = Directory.GetFiles(ScriptAssembliesDirectory);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (!fileName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    // skip non dll
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(fileName);
                list.Add(name);
            }

            return list;
        }

        public static List<Assembly> GetUnityEngineAssemblyList()
        {
            var list = new List<Assembly>();

            // check loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            {
                var name = item.GetName().Name;

                if (name.StartsWith("UnityEngine"))
                {
                    // add it
                    list.Add(item);
                }
            }

            return list;
        }

        public static List<Assembly> GetScriptAssemblyList()
        {
            var names = GetScriptAssemblyNameList();

            var assemblyList = new List<Assembly>();

            // check all assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            {
                var name = item.GetName().Name;

                if (!names.Contains(name))
                {
                    // skip not matched
                    continue;
                }

                // add it
                assemblyList.Add(item);
            }

            return assemblyList;
        }

        public static List<Type> GetScriptTypeList()
        {
            var typeList = new List<Type>();

            var assemblyList = GetScriptAssemblyList();
            foreach (var item in assemblyList)
            {
                try
                {
                    var types = item.GetTypes();
                    typeList.AddRange(types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // ignore types that cannot be loaded
                }
            }

            return typeList;
        }

        public static List<Type> GetUnityEngineTypeList()
        {
            var typeList = new List<Type>();

            var assemblyList = GetUnityEngineAssemblyList();
            foreach (var item in assemblyList)
            {
                try
                {
                    var types = item.GetTypes();
                    typeList.AddRange(types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // ignore types that cannot be loaded
                }
            }

            return typeList;
        }

        public static List<Type> GetAllTypeList()
        {
            var typeList = new List<Type>();

            var types = GetScriptTypeList();
            typeList.AddRange(types);

            types = GetUnityEngineTypeList();
            typeList.AddRange(types);

            return typeList;
        }
    }
}
