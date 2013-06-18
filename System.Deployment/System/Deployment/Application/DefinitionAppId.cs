namespace System.Deployment.Application
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;

    internal class DefinitionAppId
    {
        private System.Deployment.Internal.Isolation.IDefinitionAppId _idComPtr;

        public DefinitionAppId()
        {
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.CreateDefinition();
        }

        public DefinitionAppId(params System.Deployment.Application.DefinitionIdentity[] idPath) : this(null, idPath)
        {
        }

        public DefinitionAppId(System.Deployment.Internal.Isolation.IDefinitionAppId idComPtr)
        {
            this._idComPtr = idComPtr;
        }

        public DefinitionAppId(string text)
        {
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.TextToDefinition(0, text);
        }

        public DefinitionAppId(string codebase, params System.Deployment.Application.DefinitionIdentity[] idPath)
        {
            uint length = (uint) idPath.Length;
            System.Deployment.Internal.Isolation.IDefinitionIdentity[] definitionIdentity = new System.Deployment.Internal.Isolation.IDefinitionIdentity[length];
            for (uint i = 0; i < length; i++)
            {
                definitionIdentity[i] = idPath[i].ComPointer;
            }
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.CreateDefinition();
            this._idComPtr.put_Codebase(codebase);
            this._idComPtr.SetAppPath(length, definitionIdentity);
        }

        public override bool Equals(object obj)
        {
            return ((obj is System.Deployment.Application.DefinitionAppId) && System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.AreDefinitionsEqual(0, this.ComPointer, ((System.Deployment.Application.DefinitionAppId) obj).ComPointer));
        }

        public override int GetHashCode()
        {
            return (int) this.Hash;
        }

        private System.Deployment.Application.DefinitionIdentity PathComponent(uint index)
        {
            System.Deployment.Internal.Isolation.IEnumDefinitionIdentity o = null;
            System.Deployment.Application.DefinitionIdentity identity2;
            try
            {
                o = this._idComPtr.EnumAppPath();
                if (index > 0)
                {
                    o.Skip(index);
                }
                System.Deployment.Internal.Isolation.IDefinitionIdentity[] definitionIdentity = new System.Deployment.Internal.Isolation.IDefinitionIdentity[1];
                identity2 = (o.Next(1, definitionIdentity) == 1) ? new System.Deployment.Application.DefinitionIdentity(definitionIdentity[0]) : null;
            }
            finally
            {
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return identity2;
        }

        public System.ApplicationIdentity ToApplicationIdentity()
        {
            return new System.ApplicationIdentity(System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.DefinitionToText(0, this._idComPtr));
        }

        public System.Deployment.Application.DefinitionAppId ToDeploymentAppId()
        {
            return new System.Deployment.Application.DefinitionAppId(this.Codebase, new System.Deployment.Application.DefinitionIdentity[] { this.DeploymentIdentity });
        }

        public override string ToString()
        {
            return System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.DefinitionToText(0, this._idComPtr);
        }

        public System.Deployment.Application.DefinitionIdentity ApplicationIdentity
        {
            get
            {
                return this.PathComponent(1);
            }
        }

        public string Codebase
        {
            get
            {
                return this._idComPtr.get_Codebase();
            }
        }

        public System.Deployment.Internal.Isolation.IDefinitionAppId ComPointer
        {
            get
            {
                return this._idComPtr;
            }
        }

        public System.Deployment.Application.DefinitionIdentity DeploymentIdentity
        {
            get
            {
                return this.PathComponent(0);
            }
        }

        public ulong Hash
        {
            get
            {
                return System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.HashDefinition(0, this._idComPtr);
            }
        }
    }
}

