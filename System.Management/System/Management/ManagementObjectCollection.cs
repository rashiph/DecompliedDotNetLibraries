namespace System.Management
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class ManagementObjectCollection : ICollection, IEnumerable, IDisposable
    {
        private IEnumWbemClassObject enumWbem;
        private bool isDisposed;
        private static readonly string name = typeof(ManagementObjectCollection).FullName;
        internal EnumerationOptions options;
        internal ManagementScope scope;

        internal ManagementObjectCollection(ManagementScope scope, EnumerationOptions options, IEnumWbemClassObject enumWbem)
        {
            if (options != null)
            {
                this.options = (EnumerationOptions) options.Clone();
            }
            else
            {
                this.options = new EnumerationOptions();
            }
            if (scope != null)
            {
                this.scope = scope.Clone();
            }
            else
            {
                this.scope = ManagementScope._Clone(null);
            }
            this.enumWbem = enumWbem;
        }

        public void CopyTo(Array array, int index)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(name);
            }
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((index < array.GetLowerBound(0)) || (index > array.GetUpperBound(0)))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int num = array.Length - index;
            int num2 = 0;
            ArrayList list = new ArrayList();
            ManagementObjectEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ManagementBaseObject current = enumerator.Current;
                list.Add(current);
                num2++;
                if (num2 > num)
                {
                    throw new ArgumentException(null, "index");
                }
            }
            list.CopyTo(array, index);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void CopyTo(ManagementBaseObject[] objectCollection, int index)
        {
            this.CopyTo((Array) objectCollection, index);
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.Dispose(true);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
                this.isDisposed = true;
            }
            Marshal.ReleaseComObject(this.enumWbem);
        }

        ~ManagementObjectCollection()
        {
            this.Dispose(false);
        }

        public ManagementObjectEnumerator GetEnumerator()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(name);
            }
            if (!this.options.Rewindable)
            {
                return new ManagementObjectEnumerator(this, this.enumWbem);
            }
            IEnumWbemClassObject ppEnum = null;
            int errorCode = 0;
            try
            {
                errorCode = this.scope.GetSecuredIEnumWbemClassObjectHandler(this.enumWbem).Clone_(ref ppEnum);
                if ((errorCode & 0x80000000L) == 0L)
                {
                    errorCode = this.scope.GetSecuredIEnumWbemClassObjectHandler(ppEnum).Reset_();
                }
            }
            catch (COMException exception)
            {
                ManagementException.ThrowWithExtendedInfo(exception);
            }
            if ((errorCode & 0xfffff000L) == 0x80041000L)
            {
                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
            }
            else if ((errorCode & 0x80000000L) != 0L)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return new ManagementObjectEnumerator(this, ppEnum);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(name);
                }
                int num = 0;
                IEnumerator enumerator = this.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    num++;
                }
                return num;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(name);
                }
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(name);
                }
                return this;
            }
        }

        public class ManagementObjectEnumerator : IEnumerator, IDisposable
        {
            private bool atEndOfCollection;
            private uint cachedCount;
            private IWbemClassObjectFreeThreaded[] cachedObjects;
            private int cacheIndex;
            private ManagementObjectCollection collectionObject;
            private IEnumWbemClassObject enumWbem;
            private bool isDisposed;
            private static readonly string name = typeof(ManagementObjectCollection.ManagementObjectEnumerator).FullName;

            internal ManagementObjectEnumerator(ManagementObjectCollection collectionObject, IEnumWbemClassObject enumWbem)
            {
                this.enumWbem = enumWbem;
                this.collectionObject = collectionObject;
                this.cachedObjects = new IWbemClassObjectFreeThreaded[collectionObject.options.BlockSize];
                this.cachedCount = 0;
                this.cacheIndex = -1;
                this.atEndOfCollection = false;
            }

            public void Dispose()
            {
                if (!this.isDisposed)
                {
                    if (this.enumWbem != null)
                    {
                        Marshal.ReleaseComObject(this.enumWbem);
                        this.enumWbem = null;
                    }
                    this.cachedObjects = null;
                    this.collectionObject = null;
                    this.isDisposed = true;
                    GC.SuppressFinalize(this);
                }
            }

            ~ManagementObjectEnumerator()
            {
                this.Dispose();
            }

            public bool MoveNext()
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(name);
                }
                if (this.atEndOfCollection)
                {
                    return false;
                }
                this.cacheIndex++;
                if ((this.cachedCount - this.cacheIndex) == 0L)
                {
                    int lTimeout = (this.collectionObject.options.Timeout.Ticks == 0x7fffffffffffffffL) ? -1 : ((int) this.collectionObject.options.Timeout.TotalMilliseconds);
                    SecurityHandler securityHandler = this.collectionObject.scope.GetSecurityHandler();
                    IWbemClassObject_DoNotMarshal[] ppOutParams = new IWbemClassObject_DoNotMarshal[this.collectionObject.options.BlockSize];
                    int errorCode = this.collectionObject.scope.GetSecuredIEnumWbemClassObjectHandler(this.enumWbem).Next_(lTimeout, (uint) this.collectionObject.options.BlockSize, ppOutParams, ref this.cachedCount);
                    securityHandler.Reset();
                    if (errorCode >= 0)
                    {
                        for (int i = 0; i < this.cachedCount; i++)
                        {
                            this.cachedObjects[i] = new IWbemClassObjectFreeThreaded(Marshal.GetIUnknownForObject(ppOutParams[i]));
                        }
                    }
                    if (errorCode < 0)
                    {
                        if ((errorCode & 0xfffff000L) == 0x80041000L)
                        {
                            ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        }
                        else
                        {
                            Marshal.ThrowExceptionForHR(errorCode);
                        }
                    }
                    else
                    {
                        if ((errorCode == 0x40004) && (this.cachedCount == 0))
                        {
                            ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        }
                        if ((errorCode == 1) && (this.cachedCount == 0))
                        {
                            this.atEndOfCollection = true;
                            this.cacheIndex--;
                            return false;
                        }
                    }
                    this.cacheIndex = 0;
                }
                return true;
            }

            public void Reset()
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(name);
                }
                if (!this.collectionObject.options.Rewindable)
                {
                    throw new InvalidOperationException();
                }
                SecurityHandler securityHandler = this.collectionObject.scope.GetSecurityHandler();
                int errorCode = 0;
                try
                {
                    errorCode = this.collectionObject.scope.GetSecuredIEnumWbemClassObjectHandler(this.enumWbem).Reset_();
                }
                catch (COMException exception)
                {
                    ManagementException.ThrowWithExtendedInfo(exception);
                }
                finally
                {
                    securityHandler.Reset();
                }
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else if ((errorCode & 0x80000000L) != 0L)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                for (int i = (this.cacheIndex >= 0) ? this.cacheIndex : 0; i < this.cachedCount; i++)
                {
                    Marshal.ReleaseComObject((IWbemClassObject_DoNotMarshal) Marshal.GetObjectForIUnknown((IntPtr) this.cachedObjects[i]));
                }
                this.cachedCount = 0;
                this.cacheIndex = -1;
                this.atEndOfCollection = false;
            }

            public ManagementBaseObject Current
            {
                get
                {
                    if (this.isDisposed)
                    {
                        throw new ObjectDisposedException(name);
                    }
                    if (this.cacheIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return ManagementBaseObject.GetBaseObject(this.cachedObjects[this.cacheIndex], this.collectionObject.scope);
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
}

