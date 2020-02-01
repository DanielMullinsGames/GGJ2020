using System;
using System.IO;
using System.Text;
using UnityEditor;

namespace Peppermint.DataBinding.Editor
{
    public static class CodeGeneratorUtility
    {
        public static void WriteCode(string templatePath, string outputPath, CodeWriter writer)
        {
            // load template
            string template = File.ReadAllText(templatePath);

            // convert EOL
            template = template.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);

            // replace
            string content = writer.GetText();
            string result = template.Replace("@", content);

            // write code
            EditorHelper.PrepareOutputPath(outputPath);
            EditorHelper.CheckoutFile(outputPath);

            File.WriteAllText(outputPath, result, Encoding.UTF8);

            AssetDatabase.Refresh();
        }

        public static void GenerateCode(BaseGenerator start, CodeWriter writer)
        {
            BaseGenerator current = start;

            while (current != null)
            {
                // set writer
                current.Writer = writer;

                current.Generate();

                // advance to next
                current = current.Next;
            }
        }
    }
}
