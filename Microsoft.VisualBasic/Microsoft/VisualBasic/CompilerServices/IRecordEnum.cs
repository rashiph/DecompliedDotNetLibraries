namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Reflection;

    internal interface IRecordEnum
    {
        bool Callback(FieldInfo FieldInfo, ref object Value);
    }
}

