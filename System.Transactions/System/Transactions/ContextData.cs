namespace System.Transactions
{
    using System;

    internal class ContextData
    {
        internal TransactionScope CurrentScope;
        internal Transaction CurrentTransaction;
        internal System.Transactions.DefaultComContextState DefaultComContextState;
        [ThreadStatic]
        private static ContextData staticData;
        internal WeakReference WeakDefaultComContext;

        internal static ContextData CurrentData
        {
            get
            {
                ContextData staticData = ContextData.staticData;
                if (staticData == null)
                {
                    staticData = new ContextData();
                    ContextData.staticData = staticData;
                }
                return staticData;
            }
        }
    }
}

