namespace System.Data.Design
{
    using System;

    internal class ManagedProviderNames
    {
        private ManagedProviderNames()
        {
        }

        public static string SqlClient
        {
            get
            {
                return "System.Data.SqlClient";
            }
        }
    }
}

