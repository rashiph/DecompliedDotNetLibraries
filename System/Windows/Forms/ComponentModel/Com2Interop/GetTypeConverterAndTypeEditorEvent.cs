namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;

    internal class GetTypeConverterAndTypeEditorEvent : EventArgs
    {
        private System.ComponentModel.TypeConverter typeConverter;
        private object typeEditor;

        public GetTypeConverterAndTypeEditorEvent(System.ComponentModel.TypeConverter typeConverter, object typeEditor)
        {
            this.typeEditor = typeEditor;
            this.typeConverter = typeConverter;
        }

        public System.ComponentModel.TypeConverter TypeConverter
        {
            get
            {
                return this.typeConverter;
            }
            set
            {
                this.typeConverter = value;
            }
        }

        public object TypeEditor
        {
            get
            {
                return this.typeEditor;
            }
            set
            {
                this.typeEditor = value;
            }
        }
    }
}

