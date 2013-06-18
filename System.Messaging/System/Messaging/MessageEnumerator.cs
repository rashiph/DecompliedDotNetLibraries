namespace System.Messaging
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Messaging.Interop;
    using System.Runtime;

    public class MessageEnumerator : MarshalByRefObject, IEnumerator, IDisposable
    {
        private bool disposed;
        private System.Messaging.Interop.CursorHandle handle = System.Messaging.Interop.CursorHandle.NullHandle;
        private int index;
        private MessageQueue owner;
        private bool useCorrectRemoveCurrent;

        internal MessageEnumerator(MessageQueue owner, bool useCorrectRemoveCurrent)
        {
            this.owner = owner;
            this.useCorrectRemoveCurrent = useCorrectRemoveCurrent;
        }

        public void Close()
        {
            this.index = 0;
            if (!this.handle.IsInvalid)
            {
                this.handle.Close();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Close();
            this.disposed = true;
        }

        ~MessageEnumerator()
        {
            this.Dispose(false);
        }

        public bool MoveNext()
        {
            return this.MoveNext(TimeSpan.Zero);
        }

        public bool MoveNext(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 0xffffffffL))
            {
                throw new ArgumentException(Res.GetString("InvalidParameter", new object[] { "timeout", timeout.ToString() }));
            }
            int num2 = 0;
            int action = -2147483647;
            if (this.index == 0)
            {
                action = -2147483648;
            }
            num2 = this.owner.StaleSafeReceiveMessage((uint) totalMilliseconds, action, null, null, null, this.Handle, IntPtr.Zero);
            switch (num2)
            {
                case -1072824293:
                    this.Close();
                    return false;

                case -1072824292:
                    this.index = 0;
                    this.Close();
                    return false;
            }
            if (MessageQueue.IsFatalError(num2))
            {
                throw new MessageQueueException(num2);
            }
            this.index++;
            return true;
        }

        public Message RemoveCurrent()
        {
            return this.RemoveCurrent(TimeSpan.Zero, null, MessageQueueTransactionType.None);
        }

        public Message RemoveCurrent(MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            return this.RemoveCurrent(TimeSpan.Zero, transaction, MessageQueueTransactionType.None);
        }

        public Message RemoveCurrent(MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            return this.RemoveCurrent(TimeSpan.Zero, null, transactionType);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message RemoveCurrent(TimeSpan timeout)
        {
            return this.RemoveCurrent(timeout, null, MessageQueueTransactionType.None);
        }

        public Message RemoveCurrent(TimeSpan timeout, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            return this.RemoveCurrent(timeout, transaction, MessageQueueTransactionType.None);
        }

        public Message RemoveCurrent(TimeSpan timeout, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            return this.RemoveCurrent(timeout, null, transactionType);
        }

        private Message RemoveCurrent(TimeSpan timeout, MessageQueueTransaction transaction, MessageQueueTransactionType transactionType)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 0xffffffffL))
            {
                throw new ArgumentException(Res.GetString("InvalidParameter", new object[] { "timeout", timeout.ToString() }));
            }
            if (this.index == 0)
            {
                return null;
            }
            Message message = this.owner.ReceiveCurrent(timeout, 0, this.Handle, this.owner.MessageReadPropertyFilter, transaction, transactionType);
            if (!this.useCorrectRemoveCurrent)
            {
                this.index--;
            }
            return message;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Reset()
        {
            this.Close();
        }

        public Message Current
        {
            get
            {
                if (this.index == 0)
                {
                    throw new InvalidOperationException(Res.GetString("NoCurrentMessage"));
                }
                return this.owner.ReceiveCurrent(TimeSpan.Zero, -2147483648, this.Handle, this.owner.MessageReadPropertyFilter, null, MessageQueueTransactionType.None);
            }
        }

        public IntPtr CursorHandle
        {
            get
            {
                return this.Handle.DangerousGetHandle();
            }
        }

        internal System.Messaging.Interop.CursorHandle Handle
        {
            get
            {
                if (this.handle.IsInvalid)
                {
                    System.Messaging.Interop.CursorHandle handle;
                    if (this.disposed)
                    {
                        throw new ObjectDisposedException(base.GetType().Name);
                    }
                    int num = SafeNativeMethods.MQCreateCursor(this.owner.MQInfo.ReadHandle, out handle);
                    if (MessageQueue.IsFatalError(num))
                    {
                        throw new MessageQueueException(num);
                    }
                    this.handle = handle;
                }
                return this.handle;
            }
        }

        object IEnumerator.Current
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.Current;
            }
        }
    }
}

