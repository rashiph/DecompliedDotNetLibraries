namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;

    internal class ApplicationContext
    {
        private System.Deployment.Internal.Isolation.IActContext _appcontext;

        public ApplicationContext(System.Deployment.Internal.Isolation.DefinitionAppId appid)
        {
            if (appid == null)
            {
                throw new ArgumentNullException();
            }
            this._appcontext = System.Deployment.Internal.Isolation.IsolationInterop.CreateActContext(appid._id);
        }

        internal ApplicationContext(System.Deployment.Internal.Isolation.IActContext a)
        {
            if (a == null)
            {
                throw new ArgumentNullException();
            }
            this._appcontext = a;
        }

        public ApplicationContext(System.Deployment.Internal.Isolation.ReferenceAppId appid)
        {
            if (appid == null)
            {
                throw new ArgumentNullException();
            }
            this._appcontext = System.Deployment.Internal.Isolation.IsolationInterop.CreateActContext(appid._id);
        }

        internal System.Deployment.Internal.Isolation.Manifest.ICMS GetComponentManifest(System.Deployment.Internal.Isolation.DefinitionIdentity component)
        {
            object obj2;
            this._appcontext.GetComponentManifest(0, component._id, ref System.Deployment.Internal.Isolation.IsolationInterop.IID_ICMS, out obj2);
            return (obj2 as System.Deployment.Internal.Isolation.Manifest.ICMS);
        }

        internal string GetComponentManifestPath(System.Deployment.Internal.Isolation.DefinitionIdentity component)
        {
            object obj2;
            string str;
            this._appcontext.GetComponentManifest(0, component._id, ref System.Deployment.Internal.Isolation.IsolationInterop.IID_IManifestInformation, out obj2);
            ((System.Deployment.Internal.Isolation.IManifestInformation) obj2).get_FullPath(out str);
            return str;
        }

        public string GetComponentPath(System.Deployment.Internal.Isolation.DefinitionIdentity component)
        {
            string str;
            this._appcontext.GetComponentPayloadPath(0, component._id, out str);
            return str;
        }

        public System.Deployment.Internal.Isolation.DefinitionIdentity MatchReference(System.Deployment.Internal.Isolation.ReferenceIdentity TheRef)
        {
            object obj2;
            this._appcontext.FindReferenceInContext(0, TheRef._id, out obj2);
            return new System.Deployment.Internal.Isolation.DefinitionIdentity(obj2 as System.Deployment.Internal.Isolation.IDefinitionIdentity);
        }

        public void PrepareForExecution()
        {
            this._appcontext.PrepareForExecution(IntPtr.Zero, IntPtr.Zero);
        }

        public string ReplaceStrings(string culture, string toreplace)
        {
            string str;
            this._appcontext.ReplaceStringMacros(0, culture, toreplace, out str);
            return str;
        }

        public ApplicationStateDisposition SetApplicationState(ApplicationState s)
        {
            uint num;
            this._appcontext.SetApplicationRunningState(0, (uint) s, out num);
            return (ApplicationStateDisposition) num;
        }

        public string BasePath
        {
            get
            {
                string str;
                this._appcontext.ApplicationBasePath(0, out str);
                return str;
            }
        }

        public System.Deployment.Internal.Isolation.EnumDefinitionIdentity Components
        {
            get
            {
                object obj2;
                this._appcontext.EnumComponents(0, out obj2);
                return new System.Deployment.Internal.Isolation.EnumDefinitionIdentity(obj2 as System.Deployment.Internal.Isolation.IEnumDefinitionIdentity);
            }
        }

        public System.Deployment.Internal.Isolation.DefinitionAppId Identity
        {
            get
            {
                object obj2;
                this._appcontext.GetAppId(out obj2);
                return new System.Deployment.Internal.Isolation.DefinitionAppId(obj2 as System.Deployment.Internal.Isolation.IDefinitionAppId);
            }
        }

        public string StateLocation
        {
            get
            {
                string str;
                this._appcontext.GetApplicationStateFilesystemLocation(0, UIntPtr.Zero, IntPtr.Zero, out str);
                return str;
            }
        }

        public enum ApplicationState
        {
            Undefined,
            Starting,
            Running
        }

        public enum ApplicationStateDisposition
        {
            Running = 2,
            Running_FirstTime = 0x20002,
            Starting = 1,
            Starting_Migrated = 0x10001,
            Undefined = 0
        }
    }
}

