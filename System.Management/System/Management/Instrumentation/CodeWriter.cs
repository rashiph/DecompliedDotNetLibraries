namespace System.Management.Instrumentation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;

    internal class CodeWriter
    {
        private ArrayList children = new ArrayList();
        private int depth;

        public CodeWriter AddChild(string name)
        {
            this.Line(name);
            this.Line("{");
            CodeWriter writer = new CodeWriter {
                depth = this.depth + 1
            };
            this.children.Add(writer);
            this.Line("}");
            return writer;
        }

        public CodeWriter AddChild(params string[] parts)
        {
            return this.AddChild(string.Concat(parts));
        }

        public CodeWriter AddChild(CodeWriter snippet)
        {
            snippet.depth = this.depth;
            this.children.Add(snippet);
            return snippet;
        }

        public CodeWriter AddChildNoIndent(string name)
        {
            this.Line(name);
            CodeWriter writer = new CodeWriter {
                depth = this.depth + 1
            };
            this.children.Add(writer);
            return writer;
        }

        public void Line()
        {
            this.children.Add(null);
        }

        public void Line(string line)
        {
            this.children.Add(line);
        }

        public void Line(params string[] parts)
        {
            this.Line(string.Concat(parts));
        }

        public static explicit operator string(CodeWriter writer)
        {
            return writer.ToString();
        }

        public override string ToString()
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            this.WriteCode(writer);
            string str = writer.ToString();
            writer.Close();
            return str;
        }

        private void WriteCode(TextWriter writer)
        {
            string str = new string(' ', this.depth * 4);
            foreach (object obj2 in this.children)
            {
                if (obj2 == null)
                {
                    writer.WriteLine();
                }
                else if (obj2 is string)
                {
                    writer.Write(str);
                    writer.WriteLine(obj2);
                }
                else
                {
                    ((CodeWriter) obj2).WriteCode(writer);
                }
            }
        }
    }
}

