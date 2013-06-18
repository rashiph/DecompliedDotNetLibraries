namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.ServiceModel;

    internal sealed class ServiceMonikerInternal : ContextBoundObject, System.ServiceModel.ComIntegration.IMoniker, IParseDisplayName, IDisposable
    {
        private Dictionary<MonikerHelper.MonikerAttribute, string> PropertyTable = new Dictionary<MonikerHelper.MonikerAttribute, string>();

        void IDisposable.Dispose()
        {
        }

        void System.ServiceModel.ComIntegration.IMoniker.BindToObject(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, ref Guid riidResult, IntPtr ppvResult)
        {
            ProxyBuilder.Build(this.PropertyTable, ref riidResult, ppvResult);
        }

        void System.ServiceModel.ComIntegration.IMoniker.BindToStorage(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, ref Guid riid, out object ppvObj)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.CommonPrefixWith(System.ServiceModel.ComIntegration.IMoniker pmkOther, out System.ServiceModel.ComIntegration.IMoniker ppmkPrefix)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.ComposeWith(System.ServiceModel.ComIntegration.IMoniker pmkRight, bool fOnlyIfNotGeneric, out System.ServiceModel.ComIntegration.IMoniker ppmkComposite)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.Enum(bool fForward, out IEnumMoniker ppenumMoniker)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.GetClassID(out Guid clsid)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.GetDisplayName(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, out string ppszDisplayName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.GetSizeMax(out long size)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.GetTimeOfLastChange(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, out System.Runtime.InteropServices.ComTypes.FILETIME pFileTime)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.Hash(IntPtr pdwHash)
        {
            if (IntPtr.Zero == pdwHash)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pdwHash");
            }
            Marshal.WriteInt32(pdwHash, 0);
        }

        void System.ServiceModel.ComIntegration.IMoniker.Inverse(out System.ServiceModel.ComIntegration.IMoniker ppmk)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        int System.ServiceModel.ComIntegration.IMoniker.IsDirty()
        {
            return HR.S_FALSE;
        }

        int System.ServiceModel.ComIntegration.IMoniker.IsEqual(System.ServiceModel.ComIntegration.IMoniker pmkOtherMoniker)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        int System.ServiceModel.ComIntegration.IMoniker.IsRunning(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, System.ServiceModel.ComIntegration.IMoniker pmkNewlyRunning)
        {
            return HR.S_FALSE;
        }

        int System.ServiceModel.ComIntegration.IMoniker.IsSystemMoniker(IntPtr pdwMksys)
        {
            if (IntPtr.Zero == pdwMksys)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pdwMksys");
            }
            Marshal.WriteInt32(pdwMksys, 0);
            return HR.S_FALSE;
        }

        void System.ServiceModel.ComIntegration.IMoniker.Load(IStream stream)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.ParseDisplayName(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, string pszDisplayName, out int pchEaten, out System.ServiceModel.ComIntegration.IMoniker ppmkOut)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.Reduce(IBindCtx pbc, int dwReduceHowFar, ref System.ServiceModel.ComIntegration.IMoniker ppmkToLeft, out System.ServiceModel.ComIntegration.IMoniker ppmkReduced)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.RelativePathTo(System.ServiceModel.ComIntegration.IMoniker pmkOther, out System.ServiceModel.ComIntegration.IMoniker ppmkRelPath)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void System.ServiceModel.ComIntegration.IMoniker.Save(IStream stream, bool isDirty)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IParseDisplayName.ParseDisplayName(IBindCtx pbc, string pszDisplayName, IntPtr pchEaten, IntPtr ppmkOut)
        {
            if (IntPtr.Zero == ppmkOut)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ppmkOut");
            }
            Marshal.WriteIntPtr(ppmkOut, IntPtr.Zero);
            if (IntPtr.Zero == pchEaten)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pchEaten");
            }
            if (string.IsNullOrEmpty(pszDisplayName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pszDisplayName");
            }
            MonikerUtility.Parse(pszDisplayName, ref this.PropertyTable);
            ComPlusServiceMonikerTrace.Trace(TraceEventType.Verbose, 0x5001c, "TraceCodeComIntegrationServiceMonikerParsed", this.PropertyTable);
            Marshal.WriteInt32(pchEaten, pszDisplayName.Length);
            IntPtr interfacePtrForObject = InterfaceHelper.GetInterfacePtrForObject(typeof(System.ServiceModel.ComIntegration.IMoniker).GUID, this);
            Marshal.WriteIntPtr(ppmkOut, interfacePtrForObject);
        }
    }
}

