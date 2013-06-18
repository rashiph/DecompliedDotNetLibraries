namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class ResourcePool : IObjPool
    {
        private TransactionEndDelegate _cb;
        private static readonly Guid GUID_TransactionProperty = new Guid("ecabaeb1-7f19-11d2-978e-0000f8757e2a");

        public ResourcePool(TransactionEndDelegate cb)
        {
            this._cb = cb;
        }

        public object GetResource()
        {
            object obj2 = null;
            ITransactionResourcePool o = null;
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = this.GetToken();
                o = GetResourcePool();
                if ((o != null) && (o.GetResource(zero, out obj2) >= 0))
                {
                    Marshal.Release(zero);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                }
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return obj2;
        }

        private static ITransactionResourcePool GetResourcePool()
        {
            ITransactionResourcePool pool = null;
            object pUnk = null;
            int flags = 0;
            ((IContext) ContextUtil.ObjectContext).GetProperty(GUID_TransactionProperty, out flags, out pUnk);
            if (((ITransactionProperty) pUnk).GetTransactionResourcePool(out pool) >= 0)
            {
                return pool;
            }
            return null;
        }

        private IntPtr GetToken()
        {
            return Marshal.GetComInterfaceForObject(this, typeof(IObjPool));
        }

        public bool PutResource(object resource)
        {
            ITransactionResourcePool o = null;
            IntPtr zero = IntPtr.Zero;
            bool flag = false;
            try
            {
                o = GetResourcePool();
                if (o == null)
                {
                    return flag;
                }
                zero = this.GetToken();
                if (o.PutResource(zero, resource) < 0)
                {
                    return false;
                }
                flag = true;
            }
            finally
            {
                if (!flag && (zero != IntPtr.Zero))
                {
                    Marshal.Release(zero);
                }
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return flag;
        }

        private void ReleaseToken()
        {
            IntPtr comInterfaceForObject = Marshal.GetComInterfaceForObject(this, typeof(IObjPool));
            Marshal.Release(comInterfaceForObject);
            Marshal.Release(comInterfaceForObject);
        }

        object IObjPool.Get()
        {
            throw new NotSupportedException();
        }

        void IObjPool.Init(object p)
        {
            throw new NotSupportedException();
        }

        void IObjPool.PutDeactivated(object p)
        {
            throw new NotSupportedException();
        }

        void IObjPool.PutEndTx(object p)
        {
            this._cb(p);
            this.ReleaseToken();
        }

        void IObjPool.PutNew(object o)
        {
            throw new NotSupportedException();
        }

        void IObjPool.SetOption(int o, int dw)
        {
            throw new NotSupportedException();
        }

        void IObjPool.Shutdown()
        {
            throw new NotSupportedException();
        }

        public delegate void TransactionEndDelegate(object resource);
    }
}

