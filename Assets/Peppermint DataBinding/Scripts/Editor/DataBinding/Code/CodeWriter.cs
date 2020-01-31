using System.Text;

namespace Peppermint.DataBinding.Editor
{
    public class CodeWriter
    {
        private StringBuilder buffer;
        private int indentLevel;
        private bool lineStart;

        public int IndentLevel
        {
            get { return indentLevel; }
            set { indentLevel = value; }
        }

        public CodeWriter()
        {
            buffer = new StringBuilder();
            Reset();
        }

        public void Reset()
        {
            buffer = new StringBuilder();
            indentLevel = 0;
            lineStart = true;
        }

        public string GetText()
        {
            return buffer.ToString();
        }

        public void Write(string str)
        {
            WriteIndent();
            buffer.Append(str);
        }

        public void Write(string format, params object[] args)
        {
            WriteIndent();
            buffer.AppendFormat(format, args);
        }

        public void WriteLine()
        {
            WriteIndent();
            buffer.AppendLine();
            lineStart = true;
        }

        public void WriteLine(string str)
        {
            WriteIndent();
            buffer.AppendLine(str);
            lineStart = true;
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteIndent();
            buffer.AppendFormat(format, args);
            buffer.AppendLine();
            lineStart = true;
        }

        private void WriteIndent()
        {
            if (lineStart)
            {
                for (int i = 0; i < indentLevel; i++)
                {
                    buffer.Append('\t');
                }

                lineStart = false;
            }
        }
    }
}
