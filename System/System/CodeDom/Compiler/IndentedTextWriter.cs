namespace System.CodeDom.Compiler
{
    using System;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class IndentedTextWriter : TextWriter
    {
        public const string DefaultTabString = "    ";
        private int indentLevel;
        private bool tabsPending;
        private string tabString;
        private TextWriter writer;

        public IndentedTextWriter(TextWriter writer) : this(writer, "    ")
        {
        }

        public IndentedTextWriter(TextWriter writer, string tabString) : base(CultureInfo.InvariantCulture)
        {
            this.writer = writer;
            this.tabString = tabString;
            this.indentLevel = 0;
            this.tabsPending = false;
        }

        public override void Close()
        {
            this.writer.Close();
        }

        public override void Flush()
        {
            this.writer.Flush();
        }

        internal void InternalOutputTabs()
        {
            for (int i = 0; i < this.indentLevel; i++)
            {
                this.writer.Write(this.tabString);
            }
        }

        protected virtual void OutputTabs()
        {
            if (this.tabsPending)
            {
                for (int i = 0; i < this.indentLevel; i++)
                {
                    this.writer.Write(this.tabString);
                }
                this.tabsPending = false;
            }
        }

        public override void Write(bool value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(char value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(char[] buffer)
        {
            this.OutputTabs();
            this.writer.Write(buffer);
        }

        public override void Write(double value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(int value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(long value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(object value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(float value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(string s)
        {
            this.OutputTabs();
            this.writer.Write(s);
        }

        public override void Write(string format, object arg0)
        {
            this.OutputTabs();
            this.writer.Write(format, arg0);
        }

        public override void Write(string format, params object[] arg)
        {
            this.OutputTabs();
            this.writer.Write(format, arg);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            this.OutputTabs();
            this.writer.Write(buffer, index, count);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            this.OutputTabs();
            this.writer.Write(format, arg0, arg1);
        }

        public override void WriteLine()
        {
            this.OutputTabs();
            this.writer.WriteLine();
            this.tabsPending = true;
        }

        public override void WriteLine(bool value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(char value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(char[] buffer)
        {
            this.OutputTabs();
            this.writer.WriteLine(buffer);
            this.tabsPending = true;
        }

        public override void WriteLine(double value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(int value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(long value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(object value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(float value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(string s)
        {
            this.OutputTabs();
            this.writer.WriteLine(s);
            this.tabsPending = true;
        }

        [CLSCompliant(false)]
        public override void WriteLine(uint value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(string format, object arg0)
        {
            this.OutputTabs();
            this.writer.WriteLine(format, arg0);
            this.tabsPending = true;
        }

        public override void WriteLine(string format, params object[] arg)
        {
            this.OutputTabs();
            this.writer.WriteLine(format, arg);
            this.tabsPending = true;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            this.OutputTabs();
            this.writer.WriteLine(buffer, index, count);
            this.tabsPending = true;
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            this.OutputTabs();
            this.writer.WriteLine(format, arg0, arg1);
            this.tabsPending = true;
        }

        public void WriteLineNoTabs(string s)
        {
            this.writer.WriteLine(s);
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return this.writer.Encoding;
            }
        }

        public int Indent
        {
            get
            {
                return this.indentLevel;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                this.indentLevel = value;
            }
        }

        public TextWriter InnerWriter
        {
            get
            {
                return this.writer;
            }
        }

        public override string NewLine
        {
            get
            {
                return this.writer.NewLine;
            }
            set
            {
                this.writer.NewLine = value;
            }
        }

        internal string TabString
        {
            get
            {
                return this.tabString;
            }
        }
    }
}

