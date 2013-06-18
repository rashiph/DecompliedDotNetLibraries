namespace System.Data
{
    using System;

    internal interface IFilter
    {
        bool Invoke(DataRow row, DataRowVersion version);
    }
}

