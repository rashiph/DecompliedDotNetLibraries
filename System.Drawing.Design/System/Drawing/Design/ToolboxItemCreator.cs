namespace System.Drawing.Design
{
    using System;
    using System.Runtime;
    using System.Windows.Forms;

    public sealed class ToolboxItemCreator
    {
        private ToolboxItemCreatorCallback _callback;
        private string _format;

        internal ToolboxItemCreator(ToolboxItemCreatorCallback callback, string format)
        {
            this._callback = callback;
            this._format = format;
        }

        public ToolboxItem Create(IDataObject data)
        {
            return this._callback(data, this._format);
        }

        public string Format
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._format;
            }
        }
    }
}

