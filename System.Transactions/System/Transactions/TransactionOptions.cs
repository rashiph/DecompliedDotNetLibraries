namespace System.Transactions
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct TransactionOptions
    {
        private TimeSpan timeout;
        private System.Transactions.IsolationLevel isolationLevel;
        public TimeSpan Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = value;
            }
        }
        public System.Transactions.IsolationLevel IsolationLevel
        {
            get
            {
                return this.isolationLevel;
            }
            set
            {
                this.isolationLevel = value;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TransactionOptions))
            {
                return false;
            }
            TransactionOptions options = (TransactionOptions) obj;
            return ((options.timeout == this.timeout) && (options.isolationLevel == this.isolationLevel));
        }

        public static bool operator ==(TransactionOptions x, TransactionOptions y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(TransactionOptions x, TransactionOptions y)
        {
            return !x.Equals(y);
        }
    }
}

