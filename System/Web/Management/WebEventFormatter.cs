namespace System.Web.Management
{
    using System;
    using System.Text;

    public class WebEventFormatter
    {
        private int _level = 0;
        private StringBuilder _sb = new StringBuilder();
        private int _tabSize = 4;

        internal WebEventFormatter()
        {
        }

        private void AddTab()
        {
            for (int i = this._level; i > 0; i--)
            {
                this._sb.Append(' ', this._tabSize);
            }
        }

        public void AppendLine(string s)
        {
            this.AddTab();
            this._sb.Append(s);
            this._sb.Append('\n');
        }

        public string ToString()
        {
            return this._sb.ToString();
        }

        public int IndentationLevel
        {
            get
            {
                return this._level;
            }
            set
            {
                this._level = Math.Max(value, 0);
            }
        }

        public int TabSize
        {
            get
            {
                return this._tabSize;
            }
            set
            {
                this._tabSize = Math.Max(value, 0);
            }
        }
    }
}

