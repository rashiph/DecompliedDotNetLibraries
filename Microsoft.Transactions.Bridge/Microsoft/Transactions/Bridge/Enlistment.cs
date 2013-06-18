namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Runtime;

    internal sealed class Enlistment
    {
        private Guid enlistmentId;
        private Microsoft.Transactions.Bridge.EnlistmentOptions enlistmentOptions;
        private Guid localTransactionId;
        private Notifications notificationMask;
        private object protocolProviderContext;
        private byte[] recoveryData;
        private string remoteTransactionId;
        private object transactionManagerContext;

        public Enlistment()
        {
            this.localTransactionId = Guid.Empty;
            this.enlistmentId = Guid.NewGuid();
            this.remoteTransactionId = null;
            this.recoveryData = new byte[0];
            this.transactionManagerContext = null;
            this.protocolProviderContext = null;
            this.notificationMask = Notifications.AllProtocols;
        }

        public Enlistment(Guid enlistmentId)
        {
            this.localTransactionId = Guid.Empty;
            this.enlistmentId = enlistmentId;
            this.remoteTransactionId = null;
            this.recoveryData = new byte[0];
            this.transactionManagerContext = null;
            this.protocolProviderContext = null;
            this.notificationMask = Notifications.AllProtocols;
        }

        public byte[] GetRecoveryData()
        {
            return (byte[]) this.recoveryData.Clone();
        }

        public void SetRecoveryData(byte[] data)
        {
            if (data != null)
            {
                this.recoveryData = (byte[]) data.Clone();
            }
            else
            {
                this.recoveryData = new byte[0];
            }
        }

        public override string ToString()
        {
            return (base.GetType().ToString() + " enlistment ID = " + this.enlistmentId.ToString("B", null) + " transaction ID = " + this.localTransactionId.ToString("B", null));
        }

        public Guid EnlistmentId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enlistmentId;
            }
        }

        public Microsoft.Transactions.Bridge.EnlistmentOptions EnlistmentOptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enlistmentOptions;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.enlistmentOptions = value;
            }
        }

        public Guid LocalTransactionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localTransactionId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.localTransactionId = value;
            }
        }

        public Notifications NotificationMask
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.notificationMask;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.notificationMask = value;
            }
        }

        public object ProtocolProviderContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolProviderContext;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.protocolProviderContext = value;
            }
        }

        public string RemoteTransactionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.remoteTransactionId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.remoteTransactionId = value;
            }
        }

        public object TransactionManagerContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionManagerContext;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.transactionManagerContext = value;
            }
        }
    }
}

