namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, ExternalProcessMgmt=true)]
    public sealed class LicenseManager
    {
        private static LicenseContext context = null;
        private static object contextLockHolder = null;
        private static object internalSyncObject = new object();
        private static Hashtable providerInstances;
        private static Hashtable providers;
        private static readonly object selfLock = new object();

        private LicenseManager()
        {
        }

        private static void CacheProvider(Type type, LicenseProvider provider)
        {
            if (providers == null)
            {
                providers = new Hashtable();
            }
            providers[type] = provider;
            if (provider != null)
            {
                if (providerInstances == null)
                {
                    providerInstances = new Hashtable();
                }
                providerInstances[provider.GetType()] = provider;
            }
        }

        public static object CreateWithContext(Type type, LicenseContext creationContext)
        {
            return CreateWithContext(type, creationContext, new object[0]);
        }

        public static object CreateWithContext(Type type, LicenseContext creationContext, object[] args)
        {
            object obj2 = null;
            lock (internalSyncObject)
            {
                LicenseContext currentContext = CurrentContext;
                try
                {
                    CurrentContext = creationContext;
                    LockContext(selfLock);
                    try
                    {
                        obj2 = SecurityUtils.SecureCreateInstance(type, args);
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                    return obj2;
                }
                finally
                {
                    UnlockContext(selfLock);
                    CurrentContext = currentContext;
                }
            }
            return obj2;
        }

        private static bool GetCachedNoLicenseProvider(Type type)
        {
            return ((providers != null) && providers.ContainsKey(type));
        }

        private static LicenseProvider GetCachedProvider(Type type)
        {
            if (providers != null)
            {
                return (LicenseProvider) providers[type];
            }
            return null;
        }

        private static LicenseProvider GetCachedProviderInstance(Type providerType)
        {
            if (providerInstances != null)
            {
                return (LicenseProvider) providerInstances[providerType];
            }
            return null;
        }

        private static IntPtr GetLicenseInteropHelperType()
        {
            return typeof(LicenseInteropHelper).TypeHandle.Value;
        }

        public static bool IsLicensed(Type type)
        {
            License license;
            bool flag = ValidateInternal(type, null, false, out license);
            if (license != null)
            {
                license.Dispose();
                license = null;
            }
            return flag;
        }

        public static bool IsValid(Type type)
        {
            License license;
            bool flag = ValidateInternal(type, null, false, out license);
            if (license != null)
            {
                license.Dispose();
                license = null;
            }
            return flag;
        }

        public static bool IsValid(Type type, object instance, out License license)
        {
            return ValidateInternal(type, instance, false, out license);
        }

        public static void LockContext(object contextUser)
        {
            lock (internalSyncObject)
            {
                if (contextLockHolder != null)
                {
                    throw new InvalidOperationException(SR.GetString("LicMgrAlreadyLocked"));
                }
                contextLockHolder = contextUser;
            }
        }

        public static void UnlockContext(object contextUser)
        {
            lock (internalSyncObject)
            {
                if (contextLockHolder != contextUser)
                {
                    throw new ArgumentException(SR.GetString("LicMgrDifferentUser"));
                }
                contextLockHolder = null;
            }
        }

        public static void Validate(Type type)
        {
            License license;
            if (!ValidateInternal(type, null, true, out license))
            {
                throw new LicenseException(type);
            }
            if (license != null)
            {
                license.Dispose();
                license = null;
            }
        }

        public static License Validate(Type type, object instance)
        {
            License license;
            if (!ValidateInternal(type, instance, true, out license))
            {
                throw new LicenseException(type, instance);
            }
            return license;
        }

        private static bool ValidateInternal(Type type, object instance, bool allowExceptions, out License license)
        {
            string str;
            return ValidateInternalRecursive(CurrentContext, type, instance, allowExceptions, out license, out str);
        }

        private static bool ValidateInternalRecursive(LicenseContext context, Type type, object instance, bool allowExceptions, out License license, out string licenseKey)
        {
            LicenseProvider cachedProvider = GetCachedProvider(type);
            if ((cachedProvider == null) && !GetCachedNoLicenseProvider(type))
            {
                LicenseProviderAttribute attribute = (LicenseProviderAttribute) Attribute.GetCustomAttribute(type, typeof(LicenseProviderAttribute), false);
                if (attribute != null)
                {
                    Type licenseProvider = attribute.LicenseProvider;
                    cachedProvider = GetCachedProviderInstance(licenseProvider);
                    if (cachedProvider == null)
                    {
                        cachedProvider = (LicenseProvider) SecurityUtils.SecureCreateInstance(licenseProvider);
                    }
                }
                CacheProvider(type, cachedProvider);
            }
            license = null;
            bool flag = true;
            licenseKey = null;
            if (cachedProvider != null)
            {
                license = cachedProvider.GetLicense(context, type, instance, allowExceptions);
                if (license == null)
                {
                    flag = false;
                }
                else
                {
                    licenseKey = license.LicenseKey;
                }
            }
            if (flag && (instance == null))
            {
                string str;
                Type baseType = type.BaseType;
                if (!(baseType != typeof(object)) || (baseType == null))
                {
                    return flag;
                }
                if (license != null)
                {
                    license.Dispose();
                    license = null;
                }
                flag = ValidateInternalRecursive(context, baseType, null, allowExceptions, out license, out str);
                if (license != null)
                {
                    license.Dispose();
                    license = null;
                }
            }
            return flag;
        }

        public static LicenseContext CurrentContext
        {
            get
            {
                if (context == null)
                {
                    lock (internalSyncObject)
                    {
                        if (context == null)
                        {
                            context = new RuntimeLicenseContext();
                        }
                    }
                }
                return context;
            }
            set
            {
                lock (internalSyncObject)
                {
                    if (contextLockHolder != null)
                    {
                        throw new InvalidOperationException(SR.GetString("LicMgrContextCannotBeChanged"));
                    }
                    context = value;
                }
            }
        }

        public static LicenseUsageMode UsageMode
        {
            get
            {
                if (context != null)
                {
                    return context.UsageMode;
                }
                return LicenseUsageMode.Runtime;
            }
        }

        private class LicenseInteropHelper
        {
            private const int CLASS_E_NOTLICENSED = -2147221230;
            private const int E_FAIL = -2147483640;
            private const int E_NOTIMPL = -2147467263;
            private DesigntimeLicenseContext helperContext;
            private const int S_OK = 0;
            private LicenseContext savedLicenseContext;
            private Type savedType;

            private static object AllocateAndValidateLicense(RuntimeTypeHandle rth, IntPtr bstrKey, int fDesignTime)
            {
                object obj2;
                Type typeFromHandle = Type.GetTypeFromHandle(rth);
                CLRLicenseContext creationContext = new CLRLicenseContext((fDesignTime != 0) ? LicenseUsageMode.Designtime : LicenseUsageMode.Runtime, typeFromHandle);
                if ((fDesignTime == 0) && (bstrKey != IntPtr.Zero))
                {
                    creationContext.SetSavedLicenseKey(typeFromHandle, Marshal.PtrToStringBSTR(bstrKey));
                }
                try
                {
                    obj2 = LicenseManager.CreateWithContext(typeFromHandle, creationContext);
                }
                catch (LicenseException exception)
                {
                    throw new COMException(exception.Message, -2147221230);
                }
                return obj2;
            }

            private void GetCurrentContextInfo(ref int fDesignTime, ref IntPtr bstrKey, RuntimeTypeHandle rth)
            {
                this.savedLicenseContext = LicenseManager.CurrentContext;
                this.savedType = Type.GetTypeFromHandle(rth);
                if (this.savedLicenseContext.UsageMode == LicenseUsageMode.Designtime)
                {
                    fDesignTime = 1;
                    bstrKey = IntPtr.Zero;
                }
                else
                {
                    fDesignTime = 0;
                    string savedLicenseKey = this.savedLicenseContext.GetSavedLicenseKey(this.savedType, null);
                    bstrKey = Marshal.StringToBSTR(savedLicenseKey);
                }
            }

            private void GetLicInfo(RuntimeTypeHandle rth, ref int pRuntimeKeyAvail, ref int pLicVerified)
            {
                License license;
                string str;
                pRuntimeKeyAvail = 0;
                pLicVerified = 0;
                Type typeFromHandle = Type.GetTypeFromHandle(rth);
                if (this.helperContext == null)
                {
                    this.helperContext = new DesigntimeLicenseContext();
                }
                else
                {
                    this.helperContext.savedLicenseKeys.Clear();
                }
                if (LicenseManager.ValidateInternalRecursive(this.helperContext, typeFromHandle, null, false, out license, out str))
                {
                    if (this.helperContext.savedLicenseKeys.Contains(typeFromHandle.AssemblyQualifiedName))
                    {
                        pRuntimeKeyAvail = 1;
                    }
                    if (license != null)
                    {
                        license.Dispose();
                        license = null;
                        pLicVerified = 1;
                    }
                }
            }

            private static int RequestLicKey(RuntimeTypeHandle rth, ref IntPtr pbstrKey)
            {
                License license;
                string str;
                Type typeFromHandle = Type.GetTypeFromHandle(rth);
                if (!LicenseManager.ValidateInternalRecursive(LicenseManager.CurrentContext, typeFromHandle, null, false, out license, out str))
                {
                    return -2147483640;
                }
                if (str == null)
                {
                    return -2147483640;
                }
                pbstrKey = Marshal.StringToBSTR(str);
                if (license != null)
                {
                    license.Dispose();
                    license = null;
                }
                return 0;
            }

            private void SaveKeyInCurrentContext(IntPtr bstrKey)
            {
                if (bstrKey != IntPtr.Zero)
                {
                    this.savedLicenseContext.SetSavedLicenseKey(this.savedType, Marshal.PtrToStringBSTR(bstrKey));
                }
            }

            internal class CLRLicenseContext : LicenseContext
            {
                private string key;
                private Type type;
                private LicenseUsageMode usageMode;

                public CLRLicenseContext(LicenseUsageMode usageMode, Type type)
                {
                    this.usageMode = usageMode;
                    this.type = type;
                }

                public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly)
                {
                    if (!(type == this.type))
                    {
                        return null;
                    }
                    return this.key;
                }

                public override void SetSavedLicenseKey(Type type, string key)
                {
                    if (type == this.type)
                    {
                        this.key = key;
                    }
                }

                public override LicenseUsageMode UsageMode
                {
                    get
                    {
                        return this.usageMode;
                    }
                }
            }
        }
    }
}

