namespace System.Configuration.Internal
{
    using System;
    using System.Configuration;

    internal class ConfigSystem : IConfigSystem
    {
        private IInternalConfigHost _configHost;
        private IInternalConfigRoot _configRoot;

        void IConfigSystem.Init(Type typeConfigHost, params object[] hostInitParams)
        {
            this._configRoot = new InternalConfigRoot();
            this._configHost = (IInternalConfigHost) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(typeConfigHost);
            this._configRoot.Init(this._configHost, false);
            this._configHost.Init(this._configRoot, hostInitParams);
        }

        IInternalConfigHost IConfigSystem.Host
        {
            get
            {
                return this._configHost;
            }
        }

        IInternalConfigRoot IConfigSystem.Root
        {
            get
            {
                return this._configRoot;
            }
        }
    }
}

