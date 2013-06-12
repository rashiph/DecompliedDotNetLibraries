namespace System.ComponentModel.Design
{
    using System;

    public interface IDictionaryService
    {
        object GetKey(object value);
        object GetValue(object key);
        void SetValue(object key, object value);
    }
}

