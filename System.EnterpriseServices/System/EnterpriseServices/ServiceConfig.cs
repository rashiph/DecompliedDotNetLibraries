namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;
    using System.Transactions;

    [ComVisible(false)]
    public sealed class ServiceConfig
    {
        private bool m_bComTIIntrinsics;
        private bool m_bIISIntrinsics;
        private BindingOption m_binding;
        private bool m_bTracker;
        private Guid m_guidPart;
        private InheritanceOption m_inheritance;
        private System.EnterpriseServices.PartitionOption m_part;
        private ServiceConfigThunk m_sct;
        private string m_strSxsDirectory;
        private string m_strSxsName;
        private string m_strTipUrl;
        private string m_strTrackerAppName;
        private string m_strTrackerCompName;
        private string m_strTxDesc;
        private System.EnterpriseServices.SxsOption m_sxs;
        private SynchronizationOption m_sync;
        private ThreadPoolOption m_thrpool;
        private int m_timeout;
        private TransactionOption m_txn;
        private System.EnterpriseServices.ITransaction m_txnByot;
        private TransactionIsolationLevel m_txniso;
        private TransactionProxy m_txnProxyByot;

        public ServiceConfig()
        {
            this.Init();
        }

        private void Init()
        {
            this.m_sct = new ServiceConfigThunk();
        }

        public BindingOption Binding
        {
            get
            {
                return this.m_binding;
            }
            set
            {
                this.m_sct.Binding = (int) value;
                this.m_binding = value;
            }
        }

        public System.Transactions.Transaction BringYourOwnSystemTransaction
        {
            get
            {
                if (this.m_txnByot != null)
                {
                    return TransactionInterop.GetTransactionFromDtcTransaction(this.m_txnByot as IDtcTransaction);
                }
                if (this.m_txnProxyByot != null)
                {
                    return this.m_txnProxyByot.SystemTransaction;
                }
                return null;
            }
            set
            {
                if (!this.m_sct.SupportsSysTxn)
                {
                    this.m_txnByot = (System.EnterpriseServices.ITransaction) TransactionInterop.GetDtcTransaction(value);
                    this.m_sct.Byot = this.m_txnByot;
                    this.m_txnProxyByot = null;
                }
                else
                {
                    System.Transactions.Transaction systemTx = value;
                    if (systemTx == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                    this.m_txnByot = null;
                    this.m_txnProxyByot = new TransactionProxy(systemTx);
                    this.m_sct.ByotSysTxn = this.m_txnProxyByot;
                }
            }
        }

        public System.EnterpriseServices.ITransaction BringYourOwnTransaction
        {
            get
            {
                return this.m_txnByot;
            }
            set
            {
                this.m_sct.Byot = value;
                this.m_txnByot = value;
            }
        }

        public bool COMTIIntrinsicsEnabled
        {
            get
            {
                return this.m_bComTIIntrinsics;
            }
            set
            {
                this.m_sct.COMTIIntrinsics = value;
                this.m_bComTIIntrinsics = value;
            }
        }

        public bool IISIntrinsicsEnabled
        {
            get
            {
                return this.m_bIISIntrinsics;
            }
            set
            {
                this.m_sct.IISIntrinsics = value;
                this.m_bIISIntrinsics = value;
            }
        }

        public InheritanceOption Inheritance
        {
            get
            {
                return this.m_inheritance;
            }
            set
            {
                this.m_sct.Inheritance = (int) value;
                this.m_inheritance = value;
                switch (value)
                {
                    case InheritanceOption.Inherit:
                        this.m_thrpool = ThreadPoolOption.Inherit;
                        this.m_txn = TransactionOption.Supported;
                        this.m_sync = SynchronizationOption.Supported;
                        this.m_bIISIntrinsics = true;
                        this.m_bComTIIntrinsics = true;
                        this.m_sxs = System.EnterpriseServices.SxsOption.Inherit;
                        this.m_part = System.EnterpriseServices.PartitionOption.Inherit;
                        return;

                    case InheritanceOption.Ignore:
                        this.m_thrpool = ThreadPoolOption.None;
                        this.m_txn = TransactionOption.Disabled;
                        this.m_sync = SynchronizationOption.Disabled;
                        this.m_bIISIntrinsics = false;
                        this.m_bComTIIntrinsics = false;
                        this.m_sxs = System.EnterpriseServices.SxsOption.Ignore;
                        this.m_part = System.EnterpriseServices.PartitionOption.Ignore;
                        return;
                }
                throw new ArgumentException(Resource.FormatString("Err_value"));
            }
        }

        public TransactionIsolationLevel IsolationLevel
        {
            get
            {
                return this.m_txniso;
            }
            set
            {
                this.m_sct.TxIsolationLevel = (int) value;
                this.m_txniso = value;
            }
        }

        public Guid PartitionId
        {
            get
            {
                return this.m_guidPart;
            }
            set
            {
                this.m_sct.PartitionId = value;
                this.m_guidPart = value;
            }
        }

        public System.EnterpriseServices.PartitionOption PartitionOption
        {
            get
            {
                return this.m_part;
            }
            set
            {
                this.m_sct.Partition = (int) value;
                this.m_part = value;
            }
        }

        internal ServiceConfigThunk SCT
        {
            get
            {
                return this.m_sct;
            }
        }

        public string SxsDirectory
        {
            get
            {
                return this.m_strSxsDirectory;
            }
            set
            {
                this.m_sct.SxsDirectory = value;
                this.m_strSxsDirectory = value;
            }
        }

        public string SxsName
        {
            get
            {
                return this.m_strSxsName;
            }
            set
            {
                this.m_sct.SxsName = value;
                this.m_strSxsName = value;
            }
        }

        public System.EnterpriseServices.SxsOption SxsOption
        {
            get
            {
                return this.m_sxs;
            }
            set
            {
                this.m_sct.Sxs = (int) value;
                this.m_sxs = value;
            }
        }

        public SynchronizationOption Synchronization
        {
            get
            {
                return this.m_sync;
            }
            set
            {
                this.m_sct.Synchronization = (int) value;
                this.m_sync = value;
            }
        }

        public ThreadPoolOption ThreadPool
        {
            get
            {
                return this.m_thrpool;
            }
            set
            {
                this.m_sct.ThreadPool = (int) value;
                this.m_thrpool = value;
            }
        }

        public string TipUrl
        {
            get
            {
                return this.m_strTipUrl;
            }
            set
            {
                this.m_sct.TipUrl = value;
                this.m_strTipUrl = value;
            }
        }

        public string TrackingAppName
        {
            get
            {
                return this.m_strTrackerAppName;
            }
            set
            {
                this.m_sct.TrackerAppName = value;
                this.m_strTrackerAppName = value;
            }
        }

        public string TrackingComponentName
        {
            get
            {
                return this.m_strTrackerCompName;
            }
            set
            {
                this.m_sct.TrackerCtxName = value;
                this.m_strTrackerCompName = value;
            }
        }

        public bool TrackingEnabled
        {
            get
            {
                return this.m_bTracker;
            }
            set
            {
                this.m_sct.Tracker = value;
                this.m_bTracker = value;
            }
        }

        public TransactionOption Transaction
        {
            get
            {
                return this.m_txn;
            }
            set
            {
                this.m_sct.Transaction = (int) value;
                this.m_txn = value;
            }
        }

        public string TransactionDescription
        {
            get
            {
                return this.m_strTxDesc;
            }
            set
            {
                this.m_sct.TxDesc = value;
                this.m_strTxDesc = value;
            }
        }

        public int TransactionTimeout
        {
            get
            {
                return this.m_timeout;
            }
            set
            {
                this.m_sct.TxTimeout = value;
                this.m_timeout = value;
            }
        }
    }
}

