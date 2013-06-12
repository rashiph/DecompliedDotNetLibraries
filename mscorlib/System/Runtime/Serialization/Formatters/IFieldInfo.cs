namespace System.Runtime.Serialization.Formatters
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IFieldInfo
    {
        string[] FieldNames { [SecurityCritical] get; [SecurityCritical] set; }

        Type[] FieldTypes { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

