namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel.Activation;

    internal class AllowHelper : MarshalByRefObject
    {
        private static Dictionary<string, RegistrationRefCount> processWideRefCount;
        private static AllowHelper singleton;
        private static object thisLock = new object();

        private static void EnsureInitialized()
        {
            if (singleton == null)
            {
                lock (ThisLock)
                {
                    if (singleton == null)
                    {
                        if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                        {
                            processWideRefCount = new Dictionary<string, RegistrationRefCount>();
                            singleton = new AllowHelper();
                        }
                        else
                        {
                            object obj2;
                            Guid clsid = new Guid("CB2F6723-AB3A-11D2-9C40-00C04FA30A3E");
                            Guid riid = new Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E");
                            ((ListenerUnsafeNativeMethods.ICorRuntimeHost) RuntimeEnvironment.GetRuntimeInterfaceAsObject(clsid, riid)).GetDefaultDomain(out obj2);
                            AppDomain domain = (AppDomain) obj2;
                            if (!domain.IsDefaultAppDomain())
                            {
                                throw Fx.AssertAndThrowFatal("AllowHelper..ctor() GetDefaultDomain did not return the default domain!");
                            }
                            singleton = domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AllowHelper).FullName) as AllowHelper;
                        }
                    }
                }
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public static IDisposable TryAllow(string newSid)
        {
            EnsureInitialized();
            singleton.TryAllowCore(newSid);
            return new RegistrationForAllow(singleton, newSid);
        }

        private void TryAllowCore(string newSid)
        {
            EnsureInitialized();
            lock (ThisLock)
            {
                RegistrationRefCount count;
                if (!processWideRefCount.TryGetValue(newSid, out count))
                {
                    count = new RegistrationRefCount(newSid);
                }
                count.AddRef();
            }
        }

        private void UndoAllow(string grantedSid)
        {
            lock (ThisLock)
            {
                processWideRefCount[grantedSid].RemoveRef();
            }
        }

        private static object ThisLock
        {
            get
            {
                return thisLock;
            }
        }

        private class RegistrationForAllow : IDisposable
        {
            private string grantedSid;
            private AllowHelper singleton;

            public RegistrationForAllow(AllowHelper singleton, string grantedSid)
            {
                this.singleton = singleton;
                this.grantedSid = grantedSid;
            }

            void IDisposable.Dispose()
            {
                this.singleton.UndoAllow(this.grantedSid);
            }
        }

        private class RegistrationRefCount
        {
            private string grantedSid;
            private int refCount;

            public RegistrationRefCount(string grantedSid)
            {
                this.grantedSid = grantedSid;
            }

            public void AddRef()
            {
                if (this.refCount == 0)
                {
                    Utility.AddRightGrantedToAccount(new SecurityIdentifier(this.grantedSid), 0x40);
                    AllowHelper.processWideRefCount.Add(this.grantedSid, this);
                }
                this.refCount++;
            }

            public void RemoveRef()
            {
                this.refCount--;
                if (this.refCount == 0)
                {
                    Utility.RemoveRightGrantedToAccount(new SecurityIdentifier(this.grantedSid), 0x40);
                    AllowHelper.processWideRefCount.Remove(this.grantedSid);
                }
            }
        }
    }
}

