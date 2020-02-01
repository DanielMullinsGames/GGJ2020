using System;

namespace Peppermint.DataBinding.Editor
{
    public abstract class BaseGenerator
    {
        public CodeWriter Writer { get; set; }
        public BaseGenerator Next { get; set; }

        public abstract void Generate();

        #region Wrapper

        public int IndentLevel
        {
            get { return Writer.IndentLevel; }
            set { Writer.IndentLevel = value; }
        }

        public void Reset()
        {
            Writer.Reset();
        }

        public void Write(string str)
        {
            Writer.Write(str);
        }

        public void Write(string format, params object[] args)
        {
            Writer.Write(format, args);
        }

        public void WriteLine()
        {
            Writer.WriteLine();
        }

        public void WriteLine(string str)
        {
            Writer.WriteLine(str);
        }

        public void WriteLine(string format, params object[] args)
        {
            Writer.WriteLine(format, args);
        }

        #endregion
    }

    public class ClassGenerator : BaseGenerator
    {
        private BaseGenerator internalGenerator;
        private Type type;

        public void Init(Type type, BaseGenerator internalGenerator)
        {
            this.type = type;
            this.internalGenerator = internalGenerator;
        }

        public override void Generate()
        {
            WriteLine("// Generated class");
            WriteLine("namespace {0}", type.Namespace);
            WriteLine("{");
            {
                IndentLevel++;

                Write("public partial class {0}", type.Name);
                WriteLine();
                WriteLine("{");
                {
                    IndentLevel++;

                    // call internal generator
                    CodeGeneratorUtility.GenerateCode(internalGenerator, Writer);

                    IndentLevel--;
                }
                WriteLine("}");

                IndentLevel--;
            }
            WriteLine("}");
        }
    }
}
