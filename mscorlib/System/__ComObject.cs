namespace System
{
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal class __ComObject : MarshalByRefObject
    {
        private Hashtable m_ObjectToDataMap;

        private __ComObject()
        {
        }

        [SecurityCritical, ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        private object CreateEventProvider(RuntimeType t)
        {
            object data = Activator.CreateInstance(t, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { this }, null);
            if (this.SetData(t, data))
            {
                return data;
            }
            IDisposable disposable = data as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            return this.GetData(t);
        }

        [SecurityCritical]
        internal void FinalReleaseSelf()
        {
            Marshal.InternalFinalReleaseComObject(this);
        }

        internal object GetData(object key)
        {
            object obj2 = null;
            lock (this)
            {
                if (this.m_ObjectToDataMap != null)
                {
                    obj2 = this.m_ObjectToDataMap[key];
                }
            }
            return obj2;
        }

        [SecurityCritical]
        internal object GetEventProvider(RuntimeType t)
        {
            object data = this.GetData(t);
            if (data == null)
            {
                data = this.CreateEventProvider(t);
            }
            return data;
        }

        [SecurityCritical]
        internal IntPtr GetIUnknown(out bool fIsURTAggregated)
        {
            fIsURTAggregated = !base.GetType().IsDefined(typeof(ComImportAttribute), false);
            return Marshal.GetIUnknownForObject(this);
        }

        [SecurityCritical]
        internal void ReleaseAllData()
        {
            lock (this)
            {
                if (this.m_ObjectToDataMap != null)
                {
                    foreach (object obj2 in this.m_ObjectToDataMap.Values)
                    {
                        IDisposable disposable = obj2 as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                        __ComObject o = obj2 as __ComObject;
                        if (o != null)
                        {
                            Marshal.ReleaseComObject(o);
                        }
                    }
                    this.m_ObjectToDataMap = null;
                }
            }
        }

        [SecurityCritical]
        internal int ReleaseSelf()
        {
            return Marshal.InternalReleaseComObject(this);
        }

        internal bool SetData(object key, object data)
        {
            bool flag = false;
            lock (this)
            {
                if (this.m_ObjectToDataMap == null)
                {
                    this.m_ObjectToDataMap = new Hashtable();
                }
                if (this.m_ObjectToDataMap[key] == null)
                {
                    this.m_ObjectToDataMap[key] = data;
                    flag = true;
                }
            }
            return flag;
        }
    }
}

