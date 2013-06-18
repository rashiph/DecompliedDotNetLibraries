namespace System.Messaging
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Messaging.Interop;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class MessageQueueEnumerator : MarshalByRefObject, IEnumerator, IDisposable
    {
        private bool checkSecurity;
        private MessageQueueCriteria criteria;
        private MessageQueue currentMessageQueue;
        private bool disposed;
        private System.Messaging.Interop.LocatorHandle locatorHandle;

        internal MessageQueueEnumerator(MessageQueueCriteria criteria)
        {
            this.locatorHandle = System.Messaging.Interop.LocatorHandle.InvalidHandle;
            this.criteria = criteria;
            this.checkSecurity = true;
        }

        internal MessageQueueEnumerator(MessageQueueCriteria criteria, bool checkSecurity)
        {
            this.locatorHandle = System.Messaging.Interop.LocatorHandle.InvalidHandle;
            this.criteria = criteria;
            this.checkSecurity = checkSecurity;
        }

        public void Close()
        {
            if (!this.locatorHandle.IsInvalid)
            {
                this.locatorHandle.Close();
                this.currentMessageQueue = null;
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

        ~MessageQueueEnumerator()
        {
            this.Dispose(false);
        }

        public bool MoveNext()
        {
            string str;
            MQPROPVARIANTS[] variantArray = new MQPROPVARIANTS[2];
            byte[] destination = new byte[0x10];
            string strA = null;
            if ((this.criteria != null) && this.criteria.FilterMachine)
            {
                if (this.criteria.MachineName.CompareTo(".") == 0)
                {
                    strA = MessageQueue.ComputerName + @"\";
                }
                else
                {
                    strA = this.criteria.MachineName + @"\";
                }
            }
            do
            {
                int propertyCount = 2;
                int num2 = SafeNativeMethods.MQLocateNext(this.Handle, ref propertyCount, variantArray);
                if (MessageQueue.IsFatalError(num2))
                {
                    throw new MessageQueueException(num2);
                }
                if (propertyCount != 2)
                {
                    this.currentMessageQueue = null;
                    return false;
                }
                str = Marshal.PtrToStringUni(variantArray[0].ptr);
                Marshal.Copy(variantArray[1].ptr, destination, 0, 0x10);
                SafeNativeMethods.MQFreeMemory(variantArray[0].ptr);
                SafeNativeMethods.MQFreeMemory(variantArray[1].ptr);
            }
            while ((strA != null) && ((strA.Length >= str.Length) || (string.Compare(strA, 0, str, 0, strA.Length, true, CultureInfo.InvariantCulture) != 0)));
            this.currentMessageQueue = new MessageQueue(str, new Guid(destination));
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Reset()
        {
            this.Close();
        }

        public MessageQueue Current
        {
            get
            {
                if (this.currentMessageQueue == null)
                {
                    throw new InvalidOperationException(Res.GetString("NoCurrentMessageQueue"));
                }
                return this.currentMessageQueue;
            }
        }

        private System.Messaging.Interop.LocatorHandle Handle
        {
            get
            {
                if (this.locatorHandle.IsInvalid)
                {
                    System.Messaging.Interop.LocatorHandle handle;
                    int num;
                    if (this.disposed)
                    {
                        throw new ObjectDisposedException(base.GetType().Name);
                    }
                    if (this.checkSecurity)
                    {
                        new MessageQueuePermission(MessageQueuePermissionAccess.Browse, "*").Demand();
                    }
                    Columns columns = new Columns(2);
                    columns.AddColumnId(0x67);
                    columns.AddColumnId(0x65);
                    if (this.criteria != null)
                    {
                        num = UnsafeNativeMethods.MQLocateBegin(null, this.criteria.Reference, columns.GetColumnsRef(), out handle);
                    }
                    else
                    {
                        num = UnsafeNativeMethods.MQLocateBegin(null, null, columns.GetColumnsRef(), out handle);
                    }
                    if (MessageQueue.IsFatalError(num))
                    {
                        throw new MessageQueueException(num);
                    }
                    this.locatorHandle = handle;
                }
                return this.locatorHandle;
            }
        }

        public IntPtr LocatorHandle
        {
            get
            {
                return this.Handle.DangerousGetHandle();
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

