namespace System
{
    using System.Collections;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(false)]
    public sealed class ActivationContext : IDisposable, ISerializable
    {
        private IActContext _actContext;
        private ApplicationIdentity _applicationIdentity;
        private ApplicationStateDisposition _appRunState;
        private ArrayList _definitionIdentities;
        private ContextForm _form;
        private string[] _manifestPaths;
        private ArrayList _manifests;
        private const int DefaultComponentCount = 2;

        private ActivationContext()
        {
        }

        internal ActivationContext(ApplicationIdentity applicationIdentity)
        {
            this.CreateFromName(applicationIdentity);
        }

        internal ActivationContext(ApplicationIdentity applicationIdentity, string[] manifestPaths)
        {
            this.CreateFromNameAndManifests(applicationIdentity, manifestPaths);
        }

        [SecurityCritical]
        private ActivationContext(SerializationInfo info, StreamingContext context)
        {
            string applicationIdentityFullName = (string) info.GetValue("FullName", typeof(string));
            string[] manifestPaths = (string[]) info.GetValue("ManifestPaths", typeof(string[]));
            if (manifestPaths == null)
            {
                this.CreateFromName(new ApplicationIdentity(applicationIdentityFullName));
            }
            else
            {
                this.CreateFromNameAndManifests(new ApplicationIdentity(applicationIdentityFullName), manifestPaths);
            }
        }

        [SecuritySafeCritical]
        private void CreateFromName(ApplicationIdentity applicationIdentity)
        {
            if (applicationIdentity == null)
            {
                throw new ArgumentNullException("applicationIdentity");
            }
            this._applicationIdentity = applicationIdentity;
            IEnumDefinitionIdentity identity = this._applicationIdentity.Identity.EnumAppPath();
            this._definitionIdentities = new ArrayList(2);
            IDefinitionIdentity[] definitionIdentity = new IDefinitionIdentity[1];
            while (identity.Next(1, definitionIdentity) == 1)
            {
                this._definitionIdentities.Add(definitionIdentity[0]);
            }
            this._definitionIdentities.TrimToSize();
            if (this._definitionIdentities.Count <= 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAppId"));
            }
            this._manifestPaths = null;
            this._manifests = null;
            this._actContext = IsolationInterop.CreateActContext(this._applicationIdentity.Identity);
            this._form = ContextForm.StoreBounded;
            this._appRunState = ApplicationStateDisposition.Undefined;
        }

        [SecuritySafeCritical]
        private void CreateFromNameAndManifests(ApplicationIdentity applicationIdentity, string[] manifestPaths)
        {
            if (applicationIdentity == null)
            {
                throw new ArgumentNullException("applicationIdentity");
            }
            if (manifestPaths == null)
            {
                throw new ArgumentNullException("manifestPaths");
            }
            this._applicationIdentity = applicationIdentity;
            IEnumDefinitionIdentity identity = this._applicationIdentity.Identity.EnumAppPath();
            this._manifests = new ArrayList(2);
            this._manifestPaths = new string[manifestPaths.Length];
            IDefinitionIdentity[] definitionIdentity = new IDefinitionIdentity[1];
            int index = 0;
            while (identity.Next(1, definitionIdentity) == 1)
            {
                ICMS icms = (ICMS) IsolationInterop.ParseManifest(manifestPaths[index], null, ref IsolationInterop.IID_ICMS);
                if (!IsolationInterop.IdentityAuthority.AreDefinitionsEqual(0, icms.Identity, definitionIdentity[0]))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_IllegalAppIdMismatch"));
                }
                this._manifests.Add(icms);
                this._manifestPaths[index] = manifestPaths[index];
                index++;
            }
            if (index != manifestPaths.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalAppId"));
            }
            this._manifests.TrimToSize();
            if (this._manifests.Count <= 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAppId"));
            }
            this._definitionIdentities = null;
            this._actContext = null;
            this._form = ContextForm.Loose;
            this._appRunState = ApplicationStateDisposition.Undefined;
        }

        public static ActivationContext CreatePartialActivationContext(ApplicationIdentity identity)
        {
            return new ActivationContext(identity);
        }

        public static ActivationContext CreatePartialActivationContext(ApplicationIdentity identity, string[] manifestPaths)
        {
            return new ActivationContext(identity, manifestPaths);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        private void Dispose(bool fDisposing)
        {
            this._applicationIdentity = null;
            this._definitionIdentities = null;
            this._manifests = null;
            this._manifestPaths = null;
            if (this._actContext != null)
            {
                Marshal.ReleaseComObject(this._actContext);
            }
        }

        ~ActivationContext()
        {
            this.Dispose(false);
        }

        [SecuritySafeCritical]
        internal byte[] GetApplicationManifestBytes()
        {
            string str;
            if (this._form == ContextForm.Loose)
            {
                str = this._manifestPaths[this._manifests.Count - 1];
            }
            else
            {
                object obj2;
                this._actContext.GetComponentManifest(0, (IDefinitionIdentity) this._definitionIdentities[1], ref IsolationInterop.IID_IManifestInformation, out obj2);
                ((IManifestInformation) obj2).get_FullPath(out str);
                Marshal.ReleaseComObject(obj2);
            }
            return ReadBytesFromFile(str);
        }

        [SecurityCritical]
        internal ICMS GetComponentManifest(IDefinitionIdentity component)
        {
            object obj2;
            this._actContext.GetComponentManifest(0, component, ref IsolationInterop.IID_ICMS, out obj2);
            return (obj2 as ICMS);
        }

        [SecuritySafeCritical]
        internal byte[] GetDeploymentManifestBytes()
        {
            string str;
            if (this._form == ContextForm.Loose)
            {
                str = this._manifestPaths[0];
            }
            else
            {
                object obj2;
                this._actContext.GetComponentManifest(0, (IDefinitionIdentity) this._definitionIdentities[0], ref IsolationInterop.IID_IManifestInformation, out obj2);
                ((IManifestInformation) obj2).get_FullPath(out str);
                Marshal.ReleaseComObject(obj2);
            }
            return ReadBytesFromFile(str);
        }

        [SecuritySafeCritical]
        internal void PrepareForExecution()
        {
            if (this._form != ContextForm.Loose)
            {
                this._actContext.PrepareForExecution(IntPtr.Zero, IntPtr.Zero);
            }
        }

        private static byte[] ReadBytesFromFile(string manifestPath)
        {
            byte[] buffer = null;
            using (FileStream stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Read))
            {
                int length = (int) stream.Length;
                buffer = new byte[length];
                if (stream.CanSeek)
                {
                    stream.Seek(0L, SeekOrigin.Begin);
                }
                stream.Read(buffer, 0, length);
            }
            return buffer;
        }

        [SecuritySafeCritical]
        internal ApplicationStateDisposition SetApplicationState(ApplicationState s)
        {
            uint num;
            if (this._form == ContextForm.Loose)
            {
                return ApplicationStateDisposition.Undefined;
            }
            this._actContext.SetApplicationRunningState(0, (uint) s, out num);
            this._appRunState = (ApplicationStateDisposition) num;
            return this._appRunState;
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this._applicationIdentity != null)
            {
                info.AddValue("FullName", this._applicationIdentity.FullName, typeof(string));
            }
            if (this._manifestPaths != null)
            {
                info.AddValue("ManifestPaths", this._manifestPaths, typeof(string[]));
            }
        }

        internal ICMS ActivationContextData
        {
            [SecurityCritical]
            get
            {
                return this.ApplicationComponentManifest;
            }
        }

        internal ICMS ApplicationComponentManifest
        {
            [SecurityCritical]
            get
            {
                if (this._form == ContextForm.Loose)
                {
                    return (ICMS) this._manifests[this._manifests.Count - 1];
                }
                return this.GetComponentManifest((IDefinitionIdentity) this._definitionIdentities[this._definitionIdentities.Count - 1]);
            }
        }

        internal string ApplicationDirectory
        {
            [SecurityCritical]
            get
            {
                string str;
                if (this._form == ContextForm.Loose)
                {
                    return Path.GetDirectoryName(this._manifestPaths[this._manifestPaths.Length - 1]);
                }
                this._actContext.ApplicationBasePath(0, out str);
                return str;
            }
        }

        public byte[] ApplicationManifestBytes
        {
            [SecuritySafeCritical]
            get
            {
                return this.GetApplicationManifestBytes();
            }
        }

        internal string DataDirectory
        {
            [SecurityCritical]
            get
            {
                string str;
                if (this._form == ContextForm.Loose)
                {
                    return null;
                }
                this._actContext.GetApplicationStateFilesystemLocation(1, UIntPtr.Zero, IntPtr.Zero, out str);
                return str;
            }
        }

        internal ICMS DeploymentComponentManifest
        {
            [SecurityCritical]
            get
            {
                if (this._form == ContextForm.Loose)
                {
                    return (ICMS) this._manifests[0];
                }
                return this.GetComponentManifest((IDefinitionIdentity) this._definitionIdentities[0]);
            }
        }

        public byte[] DeploymentManifestBytes
        {
            [SecuritySafeCritical]
            get
            {
                return this.GetDeploymentManifestBytes();
            }
        }

        public ContextForm Form
        {
            get
            {
                return this._form;
            }
        }

        public ApplicationIdentity Identity
        {
            get
            {
                return this._applicationIdentity;
            }
        }

        internal ApplicationStateDisposition LastApplicationStateResult
        {
            get
            {
                return this._appRunState;
            }
        }

        internal string[] ManifestPaths
        {
            get
            {
                return this._manifestPaths;
            }
        }

        internal enum ApplicationState
        {
            Undefined,
            Starting,
            Running
        }

        internal enum ApplicationStateDisposition
        {
            Running = 2,
            RunningFirstTime = 0x20002,
            Starting = 1,
            StartingMigrated = 0x10001,
            Undefined = 0
        }

        public enum ContextForm
        {
            Loose,
            StoreBounded
        }
    }
}

