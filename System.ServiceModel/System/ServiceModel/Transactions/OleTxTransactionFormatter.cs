namespace System.ServiceModel.Transactions
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.Transactions;

    internal class OleTxTransactionFormatter : TransactionFormatter
    {
        private static OleTxTransactionHeader emptyTransactionHeader = new OleTxTransactionHeader(null, null);

        public static uint GetTimeoutFromTransaction(Transaction transaction)
        {
            XACTOPT xactopt;
            ((ITransactionOptions) TransactionInterop.GetDtcTransaction(transaction)).GetOptions(out xactopt);
            return xactopt.ulTimeout;
        }

        public static void GetTransactionAttributes(Transaction transaction, out uint timeout, out IsolationFlags isoFlags, out string description)
        {
            XACTOPT xactopt;
            XACTTRANSINFO xacttransinfo;
            IDtcTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(transaction);
            ITransactionOptions options = (ITransactionOptions) dtcTransaction;
            ISaneDtcTransaction transaction3 = (ISaneDtcTransaction) dtcTransaction;
            options.GetOptions(out xactopt);
            timeout = xactopt.ulTimeout;
            description = xactopt.szDescription;
            transaction3.GetTransactionInfo(out xacttransinfo);
            isoFlags = xacttransinfo.isoFlags;
        }

        public override TransactionInfo ReadTransaction(Message message)
        {
            OleTxTransactionHeader header = OleTxTransactionHeader.ReadFrom(message);
            if (header == null)
            {
                return null;
            }
            return new OleTxTransactionInfo(header);
        }

        public override void WriteTransaction(Transaction transaction, Message message)
        {
            WsatExtendedInformation information;
            byte[] transmitterPropagationToken = TransactionInterop.GetTransmitterPropagationToken(transaction);
            if (!TransactionCache<Transaction, WsatExtendedInformation>.Find(transaction, out information))
            {
                uint timeoutFromTransaction = GetTimeoutFromTransaction(transaction);
                information = (timeoutFromTransaction != 0) ? new WsatExtendedInformation(null, timeoutFromTransaction) : null;
            }
            OleTxTransactionHeader header = new OleTxTransactionHeader(transmitterPropagationToken, information);
            message.Headers.Add(header);
        }

        public override MessageHeader EmptyTransactionHeader
        {
            get
            {
                return emptyTransactionHeader;
            }
        }

        [ComImport, Guid("0fb15084-af41-11ce-bd2b-204c4f4f5020"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISaneDtcTransaction
        {
            void Abort(IntPtr reason, int retaining, int async);
            void Commit(int retaining, int commitType, int reserved);
            void GetTransactionInfo(out OleTxTransactionFormatter.XACTTRANSINFO transactionInformation);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3A6AD9E0-23B9-11cf-AD60-00AA00A74CCD")]
        private interface ITransactionOptions
        {
            void SetOptions([In] ref OleTxTransactionFormatter.XACTOPT pOptions);
            void GetOptions(out OleTxTransactionFormatter.XACTOPT pOptions);
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        private struct XACTOPT
        {
            public uint ulTimeout;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=40)]
            public string szDescription;
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        private struct XACTTRANSINFO
        {
            public Guid uow;
            public IsolationLevel isoLevel;
            public IsolationFlags isoFlags;
            public uint grfTCSupported;
            public uint grfRMSupported;
            public uint grfTCSupportedRetaining;
            public uint grfRMSupportedRetaining;
        }
    }
}

