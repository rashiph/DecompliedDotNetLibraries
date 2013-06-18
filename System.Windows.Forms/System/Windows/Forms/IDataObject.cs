namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IDataObject
    {
        object GetData(string format);
        object GetData(System.Type format);
        object GetData(string format, bool autoConvert);
        bool GetDataPresent(string format);
        bool GetDataPresent(System.Type format);
        bool GetDataPresent(string format, bool autoConvert);
        string[] GetFormats();
        string[] GetFormats(bool autoConvert);
        void SetData(object data);
        void SetData(string format, object data);
        void SetData(System.Type format, object data);
        void SetData(string format, bool autoConvert, object data);
    }
}

