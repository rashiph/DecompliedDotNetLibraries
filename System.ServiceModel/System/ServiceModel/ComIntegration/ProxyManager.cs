namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class ProxyManager : IProxyManager
    {
        private Dictionary<Guid, ComProxy> InterfaceIDToComProxy;
        private IProxyCreator proxyCreator;

        internal ProxyManager(IProxyCreator proxyCreator)
        {
            this.proxyCreator = proxyCreator;
            this.InterfaceIDToComProxy = new Dictionary<Guid, ComProxy>();
        }

        private ComProxy CreateServiceChannel(IntPtr outerProxy, ref Guid riid)
        {
            return this.proxyCreator.CreateProxy(outerProxy, ref riid);
        }

        private void FindOrCreateProxyInternal(IntPtr outerProxy, ref Guid riid, out ComProxy comProxy)
        {
            comProxy = null;
            lock (this)
            {
                this.InterfaceIDToComProxy.TryGetValue(riid, out comProxy);
                if (comProxy == null)
                {
                    if (this.IsIntrinsic(ref riid))
                    {
                        comProxy = this.GenerateIntrinsic(outerProxy, ref riid);
                    }
                    else
                    {
                        comProxy = this.CreateServiceChannel(outerProxy, ref riid);
                    }
                    this.InterfaceIDToComProxy[riid] = comProxy;
                }
            }
            if (comProxy == null)
            {
                throw Fx.AssertAndThrow("comProxy should not be null at this point");
            }
        }

        private ComProxy GenerateIntrinsic(IntPtr outerProxy, ref Guid riid)
        {
            if (!this.proxyCreator.SupportsIntrinsics())
            {
                throw Fx.AssertAndThrow("proxyCreator does not support intrinsic");
            }
            if (riid == typeof(IChannelOptions).GUID)
            {
                return ChannelOptions.Create(outerProxy, this.proxyCreator as IProvideChannelBuilderSettings);
            }
            if (riid != typeof(IChannelCredentials).GUID)
            {
                throw Fx.AssertAndThrow("Given IID is not an intrinsic");
            }
            return ChannelCredentials.Create(outerProxy, this.proxyCreator as IProvideChannelBuilderSettings);
        }

        private bool IsIntrinsic(ref Guid riid)
        {
            if ((riid != typeof(IChannelOptions).GUID) && (riid != typeof(IChannelCredentials).GUID))
            {
                return false;
            }
            return true;
        }

        int IProxyManager.FindOrCreateProxy(IntPtr outerProxy, ref Guid riid, out IntPtr tearOff)
        {
            tearOff = IntPtr.Zero;
            try
            {
                ComProxy comProxy = null;
                this.FindOrCreateProxyInternal(outerProxy, ref riid, out comProxy);
                comProxy.QueryInterface(ref riid, out tearOff);
                return HR.S_OK;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                return Marshal.GetHRForException(exception.GetBaseException());
            }
        }

        void IProxyManager.GetIDsOfNames([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr pDispID)
        {
            int val = -1;
            string str = name;
            if (str != null)
            {
                if (!(str == "ChannelOptions"))
                {
                    if (str == "ChannelCredentials")
                    {
                        val = 2;
                    }
                }
                else
                {
                    val = 1;
                }
            }
            Marshal.WriteInt32(pDispID, val);
        }

        int IProxyManager.InterfaceSupportsErrorInfo(ref Guid riid)
        {
            if (!this.IsIntrinsic(ref riid) && !this.proxyCreator.SupportsErrorInfo(ref riid))
            {
                return HR.S_FALSE;
            }
            return HR.S_OK;
        }

        int IProxyManager.Invoke(uint dispIdMember, IntPtr outerProxy, IntPtr pVarResult, IntPtr pExcepInfo)
        {
            try
            {
                Guid gUID;
                ComProxy comProxy = null;
                if (dispIdMember == 1)
                {
                    gUID = typeof(IChannelOptions).GUID;
                }
                else if (dispIdMember == 2)
                {
                    gUID = typeof(IChannelCredentials).GUID;
                }
                else
                {
                    return HR.DISP_E_MEMBERNOTFOUND;
                }
                this.FindOrCreateProxyInternal(outerProxy, ref gUID, out comProxy);
                TagVariant structure = new TagVariant {
                    vt = 9
                };
                IntPtr zero = IntPtr.Zero;
                comProxy.QueryInterface(ref gUID, out zero);
                structure.ptr = zero;
                Marshal.StructureToPtr(structure, pVarResult, true);
                return HR.S_OK;
            }
            catch (Exception baseException)
            {
                if (Fx.IsFatal(baseException))
                {
                    throw;
                }
                if (pExcepInfo != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.ComTypes.EXCEPINFO excepinfo = new System.Runtime.InteropServices.ComTypes.EXCEPINFO();
                    baseException = baseException.GetBaseException();
                    excepinfo.bstrDescription = baseException.Message;
                    excepinfo.bstrSource = baseException.Source;
                    excepinfo.scode = Marshal.GetHRForException(baseException);
                    Marshal.StructureToPtr(excepinfo, pExcepInfo, false);
                }
                return HR.DISP_E_EXCEPTION;
            }
        }

        int IProxyManager.SupportsDispatch()
        {
            if (this.proxyCreator.SupportsDispatch())
            {
                return HR.S_OK;
            }
            return HR.E_FAIL;
        }

        void IProxyManager.TearDownChannels()
        {
            lock (this)
            {
                IEnumerator<KeyValuePair<Guid, ComProxy>> enumerator = this.InterfaceIDToComProxy.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<Guid, ComProxy> current = enumerator.Current;
                    IDisposable disposable = current.Value;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                this.InterfaceIDToComProxy.Clear();
                this.proxyCreator.Dispose();
                enumerator.Dispose();
                this.proxyCreator = null;
            }
        }
    }
}

