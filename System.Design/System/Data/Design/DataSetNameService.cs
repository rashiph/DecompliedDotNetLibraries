namespace System.Data.Design
{
    using System;

    internal class DataSetNameService : SimpleNameService
    {
        private static DataSetNameService defaultInstance;

        public override void ValidateName(string name)
        {
        }

        internal static DataSetNameService DefaultInstance
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new DataSetNameService();
                }
                return defaultInstance;
            }
        }
    }
}

