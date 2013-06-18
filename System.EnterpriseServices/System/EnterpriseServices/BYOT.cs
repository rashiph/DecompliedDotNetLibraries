namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Transactions;

    public sealed class BYOT
    {
        private BYOT()
        {
        }

        public static object CreateWithTipTransaction(string url, Type t)
        {
            Guid rclsid = Marshal.GenerateGuidForType(t);
            return ((ICreateWithTipTransactionEx) GetByotServer()).CreateInstance(url, rclsid, Util.IID_IUnknown);
        }

        public static object CreateWithTransaction(object transaction, Type t)
        {
            Guid rclsid = Marshal.GenerateGuidForType(t);
            ITransaction pTransaction = null;
            Transaction systemTx = transaction as Transaction;
            if (systemTx != null)
            {
                ICreateWithLocalTransaction byotServer = GetByotServer() as ICreateWithLocalTransaction;
                if (byotServer != null)
                {
                    return byotServer.CreateInstanceWithSysTx(new TransactionProxy(systemTx), rclsid, Util.IID_IUnknown);
                }
                pTransaction = (ITransaction) TransactionInterop.GetDtcTransaction(systemTx);
            }
            else
            {
                pTransaction = (ITransaction) transaction;
            }
            return ((ICreateWithTransactionEx) GetByotServer()).CreateInstance(pTransaction, rclsid, Util.IID_IUnknown);
        }

        private static object GetByotServer()
        {
            return new xByotServer();
        }
    }
}

