namespace System.Web.UI
{
    using System;

    internal class ScriptBlockData : SourceLineInfo
    {
        private int _column;
        protected string _script;

        internal ScriptBlockData(int line, int column, string virtualPath)
        {
            base.Line = line;
            this.Column = column;
            base.VirtualPath = virtualPath;
        }

        internal int Column
        {
            get
            {
                return this._column;
            }
            set
            {
                this._column = value;
            }
        }

        internal string Script
        {
            get
            {
                return this._script;
            }
            set
            {
                this._script = value;
            }
        }
    }
}

