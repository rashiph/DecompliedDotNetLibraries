namespace System.Xml.Serialization
{
    using System;
    using System.IO;

    internal class IndentedWriter
    {
        private bool compact;
        private int indentLevel;
        private bool needIndent;
        private TextWriter writer;

        internal IndentedWriter(TextWriter writer, bool compact)
        {
            this.writer = writer;
            this.compact = compact;
        }

        internal void Write(char c)
        {
            if (this.needIndent)
            {
                this.WriteIndent();
            }
            this.writer.Write(c);
        }

        internal void Write(string s)
        {
            if (this.needIndent)
            {
                this.WriteIndent();
            }
            this.writer.Write(s);
        }

        internal void WriteIndent()
        {
            this.needIndent = false;
            if (!this.compact)
            {
                for (int i = 0; i < this.indentLevel; i++)
                {
                    this.writer.Write("    ");
                }
            }
        }

        internal void WriteLine()
        {
            this.writer.WriteLine();
            this.needIndent = true;
        }

        internal void WriteLine(string s)
        {
            if (this.needIndent)
            {
                this.WriteIndent();
            }
            this.writer.WriteLine(s);
            this.needIndent = true;
        }

        internal int Indent
        {
            get
            {
                return this.indentLevel;
            }
            set
            {
                this.indentLevel = value;
            }
        }
    }
}

