namespace System.Data.Design
{
    internal class SourceNameService : SimpleNameService
    {
        private static SourceNameService defaultInstance;

        internal static SourceNameService DefaultInstance
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new SourceNameService();
                }
                return defaultInstance;
            }
        }
    }
}

