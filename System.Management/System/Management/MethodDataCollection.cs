namespace System.Management
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class MethodDataCollection : ICollection, IEnumerable
    {
        private ManagementObject parent;

        internal MethodDataCollection(ManagementObject parent)
        {
            this.parent = parent;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void Add(string methodName)
        {
            this.Add(methodName, null, null);
        }

        public virtual void Add(string methodName, ManagementBaseObject inParameters, ManagementBaseObject outParameters)
        {
            IWbemClassObjectFreeThreaded pInSignature = null;
            IWbemClassObjectFreeThreaded pOutSignature = null;
            if (this.parent.GetType() == typeof(ManagementObject))
            {
                throw new InvalidOperationException();
            }
            if (inParameters != null)
            {
                pInSignature = inParameters.wbemObject;
            }
            if (outParameters != null)
            {
                pOutSignature = outParameters.wbemObject;
            }
            int errorCode = -2147217407;
            try
            {
                errorCode = this.parent.wbemObject.PutMethod_(methodName, 0, pInSignature, pOutSignature);
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
        }

        public void CopyTo(Array array, int index)
        {
            foreach (MethodData data in this)
            {
                array.SetValue(data, index++);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void CopyTo(MethodData[] methodArray, int index)
        {
            this.CopyTo((Array) methodArray, index);
        }

        public MethodDataEnumerator GetEnumerator()
        {
            return new MethodDataEnumerator(this.parent);
        }

        public virtual void Remove(string methodName)
        {
            if (this.parent.GetType() == typeof(ManagementObject))
            {
                throw new InvalidOperationException();
            }
            int errorCode = -2147217407;
            try
            {
                errorCode = this.parent.wbemObject.DeleteMethod_(methodName);
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
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MethodDataEnumerator(this.parent);
        }

        public int Count
        {
            get
            {
                int num = 0;
                IWbemClassObjectFreeThreaded ppInSignature = null;
                IWbemClassObjectFreeThreaded ppOutSignature = null;
                int errorCode = -2147217407;
                lock (typeof(enumLock))
                {
                    try
                    {
                        errorCode = this.parent.wbemObject.BeginMethodEnumeration_(0);
                        if (errorCode >= 0)
                        {
                            string pstrName = "";
                            while (((pstrName != null) && (errorCode >= 0)) && (errorCode != 0x40005))
                            {
                                pstrName = null;
                                ppInSignature = null;
                                ppOutSignature = null;
                                errorCode = this.parent.wbemObject.NextMethod_(0, out pstrName, out ppInSignature, out ppOutSignature);
                                if ((errorCode >= 0) && (errorCode != 0x40005))
                                {
                                    num++;
                                }
                            }
                            this.parent.wbemObject.EndMethodEnumeration_();
                        }
                    }
                    catch (COMException exception)
                    {
                        ManagementException.ThrowWithExtendedInfo(exception);
                    }
                }
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    return num;
                }
                if ((errorCode & 0x80000000L) != 0L)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return num;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual MethodData this[string methodName]
        {
            get
            {
                if (methodName == null)
                {
                    throw new ArgumentNullException("methodName");
                }
                return new MethodData(this.parent, methodName);
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        private class enumLock
        {
        }

        public class MethodDataEnumerator : IEnumerator
        {
            private IEnumerator en;
            private ArrayList methodNames;
            private ManagementObject parent;

            internal MethodDataEnumerator(ManagementObject parent)
            {
                this.parent = parent;
                this.methodNames = new ArrayList();
                IWbemClassObjectFreeThreaded ppInSignature = null;
                IWbemClassObjectFreeThreaded ppOutSignature = null;
                int errorCode = -2147217407;
                lock (typeof(MethodDataCollection.enumLock))
                {
                    try
                    {
                        errorCode = parent.wbemObject.BeginMethodEnumeration_(0);
                        if (errorCode >= 0)
                        {
                            string pstrName = "";
                            while (((pstrName != null) && (errorCode >= 0)) && (errorCode != 0x40005))
                            {
                                pstrName = null;
                                errorCode = parent.wbemObject.NextMethod_(0, out pstrName, out ppInSignature, out ppOutSignature);
                                if ((errorCode >= 0) && (errorCode != 0x40005))
                                {
                                    this.methodNames.Add(pstrName);
                                }
                            }
                            parent.wbemObject.EndMethodEnumeration_();
                        }
                    }
                    catch (COMException exception)
                    {
                        ManagementException.ThrowWithExtendedInfo(exception);
                    }
                    this.en = this.methodNames.GetEnumerator();
                }
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else if ((errorCode & 0x80000000L) != 0L)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }

            public bool MoveNext()
            {
                return this.en.MoveNext();
            }

            public void Reset()
            {
                this.en.Reset();
            }

            public MethodData Current
            {
                get
                {
                    return new MethodData(this.parent, (string) this.en.Current);
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

