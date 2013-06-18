namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    internal static class RequestCodes
    {
        internal const int DisableCollection = 7;
        internal const int DisableEvents = 5;
        internal const int EnableCollection = 6;
        internal const int EnableEvents = 4;
        internal const int ExecuteMethod = 9;
        internal const int GetAllData = 0;
        internal const int GetSingleInstance = 1;
        internal const int RegInfo = 8;
        internal const int SetSingleInstance = 2;
        internal const int SetSingleItem = 3;
    }
}

