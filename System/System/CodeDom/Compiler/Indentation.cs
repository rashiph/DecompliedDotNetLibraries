namespace System.CodeDom.Compiler
{
    using System;
    using System.Text;

    internal class Indentation
    {
        private int indent;
        private string s;
        private IndentedTextWriter writer;

        internal Indentation(IndentedTextWriter writer, int indent)
        {
            this.writer = writer;
            this.indent = indent;
            this.s = null;
        }

        internal string IndentationString
        {
            get
            {
                if (this.s == null)
                {
                    string tabString = this.writer.TabString;
                    StringBuilder builder = new StringBuilder(this.indent * tabString.Length);
                    for (int i = 0; i < this.indent; i++)
                    {
                        builder.Append(tabString);
                    }
                    this.s = builder.ToString();
                }
                return this.s;
            }
        }
    }
}

