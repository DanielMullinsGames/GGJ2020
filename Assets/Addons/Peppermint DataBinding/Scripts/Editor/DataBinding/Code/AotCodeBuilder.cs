using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    public class AotCodeBuilder
    {
        public class BuildArgs
        {
            public class PropertyItem
            {
                public Type propertyType;
            }

            public class ImplicitOperatorItem
            {
                public Type sourceType;
                public Type targetType;
            }

            public Type type;
            public List<PropertyItem> propertyList = new List<PropertyItem>();
            public List<ImplicitOperatorItem> implicitOperatorList = new List<ImplicitOperatorItem>();
        }

        #region AOT Code Generator

        public class CodeGenerator : BaseGenerator
        {
            private BuildArgs args;

            public void Init(BuildArgs args)
            {
                this.args = args;
            }

            public override void Generate()
            {
                WriteLine("#region AOT Type Registration");
                WriteLine();

                var objectType = typeof(object);
                WriteLine("private void RegisterProperties()");
                WriteLine("{");
                {
                    IndentLevel++;

                    // register property type
                    foreach (var item in args.propertyList)
                    {
                        WriteLine("AotUtility.RegisterProperty<{0}, {1}>();", objectType.GetFriendlyTypeName(true), item.propertyType.GetFriendlyTypeName(true));
                    }

                    IndentLevel--;
                }
                WriteLine("}");
                WriteLine();

                WriteLine("private void RegisterImplicitOperators()");
                WriteLine("{");
                {
                    IndentLevel++;

                    // register implicit operator type
                    foreach (var item in args.implicitOperatorList)
                    {
                        WriteLine("AotUtility.RegisterImplicitOperator<{0}, {1}>();", item.sourceType.GetFriendlyTypeName(true), item.targetType.GetFriendlyTypeName(true));
                    }

                    IndentLevel--;
                }
                WriteLine("}");
                WriteLine();

                WriteLine("#endregion");
            }
        }

        #endregion

        public string lastOutputPath;
        public AotCodeSettings settings;

        private string rootDirectory;
        private BuildArgs buildArgs;

        public AotCodeBuilder(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public void LoadSettings()
        {
            var path = string.Format("{0}/Settings/{1}.asset", rootDirectory, typeof(AotCodeSettings).Name);

            // load settings
            settings = AssetDatabase.LoadAssetAtPath<AotCodeSettings>(path);

            if (settings == null)
            {
                // create new settings
                settings = ScriptableObject.CreateInstance<AotCodeSettings>();
                settings.outputDirectory = "Code";
                settings.className = "AotGeneratedClass";
                settings.templatePath = string.Format("{0}/Editor/CodeTemplates/AotGeneratedClass.Code.txt", rootDirectory);

                // save it
                EditorHelper.PrepareOutputPath(path);
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }
        }

        public void GenerateCode()
        {
            buildArgs = new BuildArgs();

            // load target type
            var classType = BindingEditorUtility.GetClassByName(settings.className).FirstOrDefault();
            if (classType == null)
            {
                Debug.LogErrorFormat("Invalid class name {0}", settings.className);
                return;
            }

            // set target class
            buildArgs.type = classType;

            // get bindable properties
            var outputList = CodeTool.GetBindableProperties();
            if (outputList == null)
            {
                Debug.LogError("Get bindable properties failed.");
                return;
            }

            SetupBuildArgs(outputList);

            WriteCode();
        }


        private void SetupBuildArgs(List<KeyValuePair<string, string>> nameList)
        {
            // add common types
            var commonTypeList = GetCommonTypes();
            var propertyTypeSet = new HashSet<Type>(commonTypeList);

            // load type
            foreach (var item in nameList)
            {
                // convert to .net type
                var typeName = item.Key.Replace('/', '+');

                // get type
                var type = BindingEditorUtility.GetClassByName(typeName).FirstOrDefault();
                if (type == null)
                {
                    Debug.LogWarningFormat("Invalid type name {0}", typeName);
                    continue;
                }

                // get property
                var property = type.GetProperty(item.Value);
                if (property == null)
                {
                    Debug.LogWarningFormat("Invalid property name {0}", item.Value);
                    continue;
                }

                var aotType = GetAotType(property.PropertyType);
                if (!propertyTypeSet.Contains(aotType))
                {
                    // add it
                    propertyTypeSet.Add(aotType);
                }
            }

            // combine with custom properties
            Combine(propertyTypeSet, GetCustomProperties());

            // sort type set
            var sortedList = GetSortedTypeList(propertyTypeSet);

            // set property list
            foreach (var item in sortedList)
            {
                var newItem = new BuildArgs.PropertyItem()
                {
                    propertyType = item,
                };

                buildArgs.propertyList.Add(newItem);
            }

            // get implicit operators
            var types = ImplicitConverter.GetTypes();
            var operatorDictionary = BindingUtility.GetImplicitOperators(types);

            // set implicit operator list
            foreach (var item in operatorDictionary)
            {
                var newItem = new BuildArgs.ImplicitOperatorItem()
                {
                    sourceType = item.Key.Item1,
                    targetType = item.Key.Item2,
                };

                buildArgs.implicitOperatorList.Add(newItem);
            }
        }

        private void WriteCode()
        {
            // create writer
            var writer = new CodeWriter();

            // create generator
            var propertyGenerator = new CodeGenerator();
            var classGenerator = new ClassGenerator();

            classGenerator.Init(buildArgs.type, propertyGenerator);
            propertyGenerator.Init(buildArgs);

            // generate code
            CodeGeneratorUtility.GenerateCode(classGenerator, writer);

            // generate output path
            string fileName = buildArgs.type.FullName;
            if (!string.IsNullOrEmpty(buildArgs.type.Namespace))
            {
                fileName = buildArgs.type.FullName.Substring(buildArgs.type.Namespace.Length + 1);
            }

            string outputPath = string.Format("{0}/{1}.Code.cs", settings.outputDirectory, fileName);

            Debug.LogFormat("Write code to \"{0}\"", outputPath);
            CodeGeneratorUtility.WriteCode(settings.templatePath, outputPath, writer);

            // save it
            lastOutputPath = outputPath;
        }

        private void Combine(HashSet<Type> hashSet, IEnumerable<Type> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (!hashSet.Contains(item))
                {
                    hashSet.Add(item);
                }
            }
        }

        private List<Type> GetSortedTypeList(HashSet<Type> hashSet)
        {
            var list = new List<Type>();

            // object first
            list.Add(typeof(object));

            // primitive type
            list.AddRange(hashSet.Where(x => x.IsPrimitive).OrderBy(x => x.GetFriendlyTypeName()));

            // enum
            list.AddRange(hashSet.Where(x => x.IsEnum).OrderBy(x => x.FullName));

            // struct
            list.AddRange(hashSet.Where(x => x.IsValueType && !x.IsEnum && !x.IsPrimitive).OrderBy(x => x.GetFriendlyTypeName(true)));

            if (list.Count != hashSet.Count)
            {
                // this should never happen
                Debug.LogError("Count not match");
            }

            return list;
        }

        private Type GetAotType(Type type)
        {
            if (type.IsValueType)
            {
                return type;
            }
            else
            {
                return typeof(object);
            }
        }

        private List<Type> GetCommonTypes()
        {
            var list = new List<Type>()
            {
                typeof(object),
                typeof(bool),
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Color),
            };

            return list;
        }

        private HashSet<Type> GetCustomProperties()
        {
            var list = new HashSet<Type>();

            // get all types
            var types = BindingEditorUtility.GetScriptTypeList();
            foreach (var type in types)
            {
                if (!type.IsClass)
                {
                    continue;
                }

                // check all properties
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    // check if the BindablePropertyAttribute is defined
                    if (!property.IsDefined(typeof(BindablePropertyAttribute), false))
                    {
                        continue;
                    }

                    var aotType = GetAotType(property.PropertyType);
                    if (!list.Contains(aotType))
                    {
                        // add it
                        list.Add(aotType);
                    }
                }
            }

            return list;
        }

    }
}
