using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    public class PropertyCodeBuilder
    {
        public class BuildArgs
        {
            public class Item
            {
                public Type type;
                public string name;
                public string propertyName;
                public MemberTypes memberType;
                public bool enabled = true;
            }

            public Type type;
            public List<Item> memberList = new List<Item>();
        }

        #region Bindable Property Generator

        public class CodeGenerator : BaseGenerator
        {
            private BuildArgs args;

            public void Init(BuildArgs args)
            {
                this.args = args;
            }

            public override void Generate()
            {
                WriteLine("#region Bindable Properties");
                WriteLine();

                foreach (var item in args.memberList)
                {
                    if (!item.enabled)
                    {
                        // skip disable member
                        continue;
                    }

                    var fieldType = item.type;
                    var propertyName = item.propertyName;

                    string typeName = fieldType.GetFriendlyTypeName();

                    WriteLine("public {0} {1}", typeName, propertyName);
                    WriteLine("{");

                    WriteLine("\tget {{ return {0}; }}", item.name);

                    WriteSetProperty(item);

                    WriteLine("}");
                    WriteLine();
                }

                WriteLine("#endregion");
            }

            private void WriteSetProperty(BuildArgs.Item item)
            {
                string setPropertyStatement = null;

                if (item.memberType == MemberTypes.Field)
                {
#if NET_4_6
                    setPropertyStatement = string.Format("SetProperty(ref {0}, value);", item.name);
#else
                    setPropertyStatement = string.Format("SetProperty(ref {0}, value, \"{1}\");", item.name, item.propertyName);
#endif
                }
                else if (item.memberType == MemberTypes.Property)
                {
#if NET_4_6
                    setPropertyStatement = string.Format("SetProperty({0}, value, x => {0} = x);", item.name);
#else
                    setPropertyStatement = string.Format("SetProperty({0}, value, x => {0} = x, \"{1}\");", item.name, item.propertyName);
#endif
                }
                else
                {
                    throw new InvalidOperationException("Invalid member type");
                }

                // single line style
                WriteLine("\tset {{ {0} }}", setPropertyStatement);
            }
        }

        #endregion

        public string typeName;
        public string classInfo;
        public string lastOutputPath;
        public bool includePublic;
        public bool includeProperty;
        public BuildArgs buildArgs;
        public PropertyCodeSettings settings;

        private string rootDirectory;

        public PropertyCodeBuilder(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public void LoadSettings()
        {
            var path = string.Format("{0}/Settings/{1}.asset", rootDirectory, typeof(PropertyCodeSettings).Name);

            // load settings
            settings = AssetDatabase.LoadAssetAtPath<PropertyCodeSettings>(path);

            if (settings == null)
            {
                // create new settings
                settings = ScriptableObject.CreateInstance<PropertyCodeSettings>();
                settings.outputDirectory = "Code";
                settings.templatePath = string.Format("{0}/Editor/CodeTemplates/BindableClass.Code.txt", rootDirectory);

                // save it
                EditorHelper.PrepareOutputPath(path);
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }
        }

        public void LoadClass()
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogError("Type name is null");
                return;
            }

            // reset config
            buildArgs = null;

            var typeList = BindingEditorUtility.GetClassByName(typeName);
            if (typeList.Count == 0)
            {
                Debug.LogErrorFormat("Failed to get type {0}", typeName);
                return;
            }
            else if (typeList.Count > 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < typeList.Count; i++)
                {
                    sb.Append(typeList[i].FullName);
                    sb.Append(",\n");
                }

                Debug.LogErrorFormat("{0} types are found for name {1}, type list:\n{2}You need input more specific name.", typeList.Count, typeName, sb.ToString());
                return;
            }

            // get the only one matched
            var type = typeList.First();

            if (type == null)
            {
                Debug.LogErrorFormat("Failed to get type {0}", typeName);
                return;
            }

            // check type
            if (!IsBindableType(type))
            {
                Debug.LogErrorFormat("Class {0} is not sub class of BindableClass", type.Name);
                return;
            }

            // create new config
            buildArgs = new BuildArgs();
            buildArgs.type = type;

            // get class info
            classInfo = GetClassInfo(type);

            UpdateMemberList();
        }

        public void UpdateMemberList()
        {
            if (buildArgs == null)
            {
                return;
            }

            // clear member list
            buildArgs.memberList.Clear();

            var type = buildArgs.type;

            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            if (includePublic)
            {
                // add public
                flags |= BindingFlags.Public;
            }

            // get all fields
            var fields = type.GetFields(flags).Where(x => !x.Name.EndsWith("k__BackingField"));
            foreach (var item in fields)
            {
                // create member
                var member = new BuildArgs.Item()
                {
                    type = item.FieldType,
                    name = item.Name,
                    memberType = MemberTypes.Field,
                };

                member.propertyName = BindingEditorUtility.GetPropertyName(item.Name);

                // add it
                buildArgs.memberList.Add(member);
            }

            if (!includeProperty)
            {
                return;
            }

            // get all properties
            var properties = type.GetProperties(flags);
            foreach (var item in properties)
            {
                // create member
                var member = new BuildArgs.Item()
                {
                    type = item.PropertyType,
                    name = item.Name,
                    memberType = MemberTypes.Property,
                };

                member.propertyName = BindingEditorUtility.GetPropertyName(item.Name);

                // add it
                buildArgs.memberList.Add(member);
            }
        }

        public void GenerateCodeSnippet()
        {
            if (buildArgs == null)
            {
                Debug.LogWarning("Class is not loaded");
                return;
            }

            // create writer
            var writer = new CodeWriter();

            // create generator
            var propertyGenerator = new CodeGenerator();
            propertyGenerator.Init(buildArgs);

            // generate code
            CodeGeneratorUtility.GenerateCode(propertyGenerator, writer);

            // copy to system pasteboard
            GUIUtility.systemCopyBuffer = writer.GetText();

            Debug.LogFormat("Copy code snippet to system pasteboard");
        }

        public void GenerateCode()
        {
            if (buildArgs == null)
            {
                Debug.LogWarning("Class is not loaded");
                return;
            }

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

        private string GetClassInfo(Type type)
        {
            // get private fields
            const BindingFlags PrivateFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            const BindingFlags PublicFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

            int privateFieldCount = type.GetFields(PrivateFlags).Where(x => !x.Name.EndsWith("k__BackingField")).Count();
            int publicFieldCount = type.GetFields(PublicFlags).Where(x => !x.Name.EndsWith("k__BackingField")).Count();

            int privatePropertyCount = type.GetProperties(PrivateFlags).Count();
            int publicPropertyCount = type.GetProperties(PublicFlags).Count();

            var info = string.Format("{0}, private fields: {1}, public fields: {2}, private property: {3}, public property: {4}",
                type.FullName, privateFieldCount, publicFieldCount, privatePropertyCount, publicPropertyCount);

            return info;
        }

        private bool IsBindableType(Type type)
        {
            // check built-in bindable type
            if (type.IsSubclassOf(typeof(BindableObject)) || type.IsSubclassOf(typeof(BindableMonoBehaviour)) || type.IsSubclassOf(typeof(BindableScriptableObject)))
            {
                return true;
            }

            // check if type implements INotifyPropertyChanged 
            var interfaceType = typeof(INotifyPropertyChanged);
            if (!interfaceType.IsAssignableFrom(type))
            {
                return false;
            }

            // check if method void NotifyPropertyChanged(string propertyName) exists
            var notifyMethod = type.GetMethod("NotifyPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
            if (notifyMethod == null || notifyMethod.ReturnType != typeof(void))
            {
                return false;
            }

            // check method "SetProperty"
            bool hasSetPropertyA = false;
            bool hasSetPropertyB = false;

            // get all method with name "SetProperty"
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name == "SetProperty");
            foreach (var m in methods)
            {
                if (m.ReturnType != typeof(bool))
                {
                    continue;
                }

                if (!m.IsGenericMethodDefinition)
                {
                    continue;
                }

                var genericType = m.GetGenericArguments().FirstOrDefault();
                if (genericType == null)
                {
                    continue;
                }

                // get method signature
                var methodSignature = m.ToString();
                var setPropertySianatureA = string.Format("Boolean SetProperty[{0}]({0} ByRef, {0}, System.String)", genericType);
                var setPropertySianatureB = string.Format("Boolean SetProperty[{0}]({0}, {0}, System.Action`1[{0}], System.String)", genericType);

                // verify signature
                if (methodSignature == setPropertySianatureA)
                {
                    hasSetPropertyA = true;
                }
                else if (methodSignature == setPropertySianatureB)
                {
                    hasSetPropertyB = true;
                }
            }

            // must have both methods
            if (hasSetPropertyA && hasSetPropertyB)
            {
                return true;
            }

            return false;
        }
    }
}
