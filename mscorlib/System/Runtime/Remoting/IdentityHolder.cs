namespace System.Runtime.Remoting
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Proxies;
    using System.Security;
    using System.Threading;

    internal sealed class IdentityHolder
    {
        private static Context _cachedDefaultContext = null;
        private static Hashtable _URITable = new Hashtable();
        private const int CleanUpCountInterval = 0x40;
        private const int INFINITE = 0x7fffffff;
        private static int SetIDCount = 0;

        private IdentityHolder()
        {
        }

        [SecurityCritical]
        internal static bool AddDynamicProperty(MarshalByRefObject obj, IDynamicProperty prop)
        {
            if (RemotingServices.IsObjectOutOfContext(obj))
            {
                return RemotingServices.GetRealProxy(obj).IdentityObject.AddProxySideDynamicProperty(prop);
            }
            MarshalByRefObject obj2 = (MarshalByRefObject) RemotingServices.AlwaysUnwrap((ContextBoundObject) obj);
            ServerIdentity identity = (ServerIdentity) MarshalByRefObject.GetIdentity(obj2);
            if (identity == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_NoIdentityEntry"));
            }
            return identity.AddServerSideDynamicProperty(prop);
        }

        [SecurityCritical]
        internal static Identity CasualResolveIdentity(string uri)
        {
            if (uri == null)
            {
                return null;
            }
            Identity identity = CasualResolveReference(URITable[MakeURIKeyNoLower(uri)]);
            if (identity == null)
            {
                identity = CasualResolveReference(URITable[MakeURIKey(uri)]);
                if (identity == null)
                {
                    identity = RemotingConfigHandler.CreateWellKnownObject(uri);
                }
            }
            return identity;
        }

        private static Identity CasualResolveReference(object o)
        {
            WeakReference reference = o as WeakReference;
            if (reference != null)
            {
                return (Identity) reference.Target;
            }
            return (Identity) o;
        }

        private static void CleanupIdentities(object state)
        {
            IDictionaryEnumerator enumerator = URITable.GetEnumerator();
            ArrayList list = new ArrayList();
            while (enumerator.MoveNext())
            {
                WeakReference reference = enumerator.Value as WeakReference;
                if ((reference != null) && (reference.Target == null))
                {
                    list.Add(enumerator.Key);
                }
            }
            foreach (string str in list)
            {
                URITable.Remove(str);
            }
        }

        [SecurityCritical]
        internal static Identity FindOrCreateIdentity(string objURI, string URL, ObjRef objectRef)
        {
            Identity idObj = null;
            bool flag = URL != null;
            idObj = ResolveIdentity(flag ? URL : objURI);
            if ((flag && (idObj != null)) && (idObj is ServerIdentity))
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_WellKnown_CantDirectlyConnect"), new object[] { URL }));
            }
            if (idObj == null)
            {
                idObj = new Identity(objURI, URL);
                ReaderWriterLock tableLock = TableLock;
                bool flag2 = !tableLock.IsWriterLockHeld;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    if (flag2)
                    {
                        tableLock.AcquireWriterLock(0x7fffffff);
                    }
                    idObj = SetIdentity(idObj, null, DuplicateIdentityOption.UseExisting);
                    idObj.RaceSetObjRef(objectRef);
                }
                finally
                {
                    if (flag2 && tableLock.IsWriterLockHeld)
                    {
                        tableLock.ReleaseWriterLock();
                    }
                }
            }
            return idObj;
        }

        [SecurityCritical]
        internal static ServerIdentity FindOrCreateServerIdentity(MarshalByRefObject obj, string objURI, int flags)
        {
            ServerIdentity idObj = null;
            bool flag;
            idObj = (ServerIdentity) MarshalByRefObject.GetIdentity(obj, out flag);
            if (idObj == null)
            {
                Context serverCtx = null;
                if (obj is ContextBoundObject)
                {
                    serverCtx = Thread.CurrentContext;
                }
                else
                {
                    serverCtx = DefaultContext;
                }
                ServerIdentity id = new ServerIdentity(obj, serverCtx);
                if (flag)
                {
                    idObj = obj.__RaceSetServerIdentity(id);
                }
                else
                {
                    RealProxy realProxy = null;
                    realProxy = RemotingServices.GetRealProxy(obj);
                    realProxy.IdentityObject = id;
                    idObj = (ServerIdentity) realProxy.IdentityObject;
                }
            }
            if (IdOps.bStrongIdentity(flags))
            {
                ReaderWriterLock tableLock = TableLock;
                bool flag2 = !tableLock.IsWriterLockHeld;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    if (flag2)
                    {
                        tableLock.AcquireWriterLock(0x7fffffff);
                    }
                    if ((idObj.ObjURI == null) || !idObj.IsInIDTable())
                    {
                        SetIdentity(idObj, objURI, DuplicateIdentityOption.Unique);
                    }
                    if (idObj.IsDisconnected())
                    {
                        idObj.SetFullyConnected();
                    }
                }
                finally
                {
                    if (flag2 && tableLock.IsWriterLockHeld)
                    {
                        tableLock.ReleaseWriterLock();
                    }
                }
            }
            return idObj;
        }

        [SecurityCritical]
        internal static void FlushIdentityTable()
        {
            ReaderWriterLock tableLock = TableLock;
            bool flag = !tableLock.IsWriterLockHeld;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (flag)
                {
                    tableLock.AcquireWriterLock(0x7fffffff);
                }
                CleanupIdentities(null);
            }
            finally
            {
                if (flag && tableLock.IsWriterLockHeld)
                {
                    tableLock.ReleaseWriterLock();
                }
            }
        }

        private static string MakeURIKey(string uri)
        {
            return Identity.RemoveAppNameOrAppGuidIfNecessary(uri.ToLower(CultureInfo.InvariantCulture));
        }

        private static string MakeURIKeyNoLower(string uri)
        {
            return Identity.RemoveAppNameOrAppGuidIfNecessary(uri);
        }

        [SecurityCritical]
        internal static bool RemoveDynamicProperty(MarshalByRefObject obj, string name)
        {
            if (RemotingServices.IsObjectOutOfContext(obj))
            {
                return RemotingServices.GetRealProxy(obj).IdentityObject.RemoveProxySideDynamicProperty(name);
            }
            MarshalByRefObject obj2 = (MarshalByRefObject) RemotingServices.AlwaysUnwrap((ContextBoundObject) obj);
            ServerIdentity identity = (ServerIdentity) MarshalByRefObject.GetIdentity(obj2);
            if (identity == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_NoIdentityEntry"));
            }
            return identity.RemoveServerSideDynamicProperty(name);
        }

        [SecurityCritical]
        internal static void RemoveIdentity(string uri)
        {
            RemoveIdentity(uri, true);
        }

        [SecurityCritical]
        internal static void RemoveIdentity(string uri, bool bResetURI)
        {
            string key = MakeURIKey(uri);
            ReaderWriterLock tableLock = TableLock;
            bool flag = !tableLock.IsWriterLockHeld;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Identity target;
                if (flag)
                {
                    tableLock.AcquireWriterLock(0x7fffffff);
                }
                object obj2 = URITable[key];
                WeakReference reference = obj2 as WeakReference;
                if (reference != null)
                {
                    target = (Identity) reference.Target;
                    reference.Target = null;
                }
                else
                {
                    target = (Identity) obj2;
                    if (target != null)
                    {
                        ((ServerIdentity) target).ResetHandle();
                    }
                }
                if (target != null)
                {
                    URITable.Remove(key);
                    target.ResetInIDTable(bResetURI);
                }
            }
            finally
            {
                if (flag && tableLock.IsWriterLockHeld)
                {
                    tableLock.ReleaseWriterLock();
                }
            }
        }

        [SecurityCritical]
        internal static Identity ResolveIdentity(string URI)
        {
            Identity identity;
            if (URI == null)
            {
                throw new ArgumentNullException("URI");
            }
            ReaderWriterLock tableLock = TableLock;
            bool flag = !tableLock.IsReaderLockHeld;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (flag)
                {
                    tableLock.AcquireReaderLock(0x7fffffff);
                }
                identity = ResolveReference(URITable[MakeURIKey(URI)]);
            }
            finally
            {
                if (flag && tableLock.IsReaderLockHeld)
                {
                    tableLock.ReleaseReaderLock();
                }
            }
            return identity;
        }

        private static Identity ResolveReference(object o)
        {
            WeakReference reference = o as WeakReference;
            if (reference != null)
            {
                return (Identity) reference.Target;
            }
            return (Identity) o;
        }

        [SecurityCritical]
        private static Identity SetIdentity(Identity idObj, string URI, DuplicateIdentityOption duplicateOption)
        {
            bool flag = idObj is ServerIdentity;
            if (idObj.URI == null)
            {
                idObj.SetOrCreateURI(URI);
                if (idObj.ObjectRef != null)
                {
                    idObj.ObjectRef.URI = idObj.URI;
                }
            }
            string key = MakeURIKey(idObj.URI);
            object obj2 = URITable[key];
            if (obj2 != null)
            {
                bool flag2;
                WeakReference reference = obj2 as WeakReference;
                Identity target = null;
                if (reference != null)
                {
                    target = (Identity) reference.Target;
                    flag2 = target is ServerIdentity;
                }
                else
                {
                    target = (Identity) obj2;
                    flag2 = target is ServerIdentity;
                }
                if ((target != null) && (target != idObj))
                {
                    switch (duplicateOption)
                    {
                        case DuplicateIdentityOption.Unique:
                        {
                            string uRI = idObj.URI;
                            throw new RemotingException(Environment.GetResourceString("Remoting_URIClash", new object[] { uRI }));
                        }
                        case DuplicateIdentityOption.UseExisting:
                            idObj = target;
                            return idObj;
                    }
                    return idObj;
                }
                if (reference != null)
                {
                    if (flag2)
                    {
                        URITable[key] = idObj;
                        return idObj;
                    }
                    reference.Target = idObj;
                }
                return idObj;
            }
            object obj3 = null;
            if (flag)
            {
                obj3 = idObj;
                ((ServerIdentity) idObj).SetHandle();
            }
            else
            {
                obj3 = new WeakReference(idObj);
            }
            URITable.Add(key, obj3);
            idObj.SetInIDTable();
            SetIDCount++;
            if ((SetIDCount % 0x40) == 0)
            {
                CleanupIdentities(null);
            }
            return idObj;
        }

        internal static Context DefaultContext
        {
            [SecurityCritical]
            get
            {
                if (_cachedDefaultContext == null)
                {
                    _cachedDefaultContext = Thread.GetDomain().GetDefaultContext();
                }
                return _cachedDefaultContext;
            }
        }

        internal static ReaderWriterLock TableLock
        {
            get
            {
                return Thread.GetDomain().RemotingData.IDTableLock;
            }
        }

        internal static Hashtable URITable
        {
            get
            {
                return _URITable;
            }
        }
    }
}

