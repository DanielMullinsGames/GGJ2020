using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    public class ImplicitConverterCodeBuilder
    {
        public class BuildArgs
        {
            public class Item
            {
                public Type type;
            }

            public List<Item> itemList = new List<Item>();
        }

        #region ImplicitConverter Code Generator

        public class CodeGenerator : BaseGenerator
        {
            private BuildArgs args;

            public void Init(BuildArgs args)
            {
                this.args = args;
            }

            public override void Generate()
            {
                WriteLine("// Generated method");
                WriteLine("public static string[] GetTypeNames()");
                WriteLine("{");
                {
                    IndentLevel++;

                    WriteLine("var names = new string[]");
                    WriteLine("{");
                    {
                        IndentLevel++;

                        foreach (var item in args.itemList)
                        {
                            var typeName = item.type.GetFriendlyTypeName(true);
                            WriteLine("\"{0}\",", typeName);
                        }

                        IndentLevel--;
                    }
                    WriteLine("};");

                    WriteLine();
                    WriteLine("return names;");

                    IndentLevel--;
                }
                WriteLine("}");
                WriteLine();
            }
        }

        #endregion

        public string lastOutputPath;
        public ImplicitConverterCodeSettings settings;
        public BuildArgs buildArgs;

        private string rootDirectory;

        public ImplicitConverterCodeBuilder(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public void LoadSettings()
        {
            var path = string.Format("{0}/Settings/{1}.asset", rootDirectory, typeof(ImplicitConverterCodeSettings).Name);

            // load settings
            settings = AssetDatabase.LoadAssetAtPath<ImplicitConverterCodeSettings>(path);

            if (settings == null)
            {
                // create new settings
                settings = ScriptableObject.CreateInstance<ImplicitConverterCodeSettings>();
                settings.outputDirectory = string.Format("{0}/Scripts/DataBinding/Converter", rootDirectory);
                settings.templatePath = string.Format("{0}/Editor/CodeTemplates/ImplicitConverter.Code.txt", rootDirectory);

                // save it
                EditorHelper.PrepareOutputPath(path);
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }
        }

        public void LoadTypeList()
        {
            // create build args
            buildArgs = new BuildArgs();

            // get types
            var types = BindingEditorUtility.GetImplicitOperatorTypes(true);

            foreach (var item in types)
            {
                var newItem = new BuildArgs.Item()
                {
                    type = item,
                };

                // add it
                buildArgs.itemList.Add(newItem);
            }
        }

        public void GenerateCode()
        {
            if (buildArgs == null)
            {
                Debug.LogWarning("Type is not loaded");
                return;
            }

            // create writer
            var writer = new CodeWriter();

            // create generator
            var propertyGenerator = new CodeGenerator();
            var classGenerator = new ClassGenerator();

            var outputType = typeof(ImplicitConverter);
            classGenerator.Init(outputType, propertyGenerator);
            propertyGenerator.Init(buildArgs);

            // generate code
            CodeGeneratorUtility.GenerateCode(classGenerator, writer);

            // generate output path
            string fileName = outputType.FullName;
            if (!string.IsNullOrEmpty(outputType.Namespace))
            {
                fileName = outputType.FullName.Substring(outputType.Namespace.Length + 1);
            }

            string outputPath = string.Format("{0}/{1}.Code.cs", settings.outputDirectory, fileName);

            Debug.LogFormat("Write code to \"{0}\"", outputPath);
            CodeGeneratorUtility.WriteCode(settings.templatePath, outputPath, writer);

            // save it
            lastOutputPath = outputPath;
        }
    }
}
