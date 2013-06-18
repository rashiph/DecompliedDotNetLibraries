namespace System.Deployment.Application
{
    using System;
    using System.Collections;
    using System.Deployment.Internal.Isolation;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class DefinitionIdentity : ICloneable
    {
        private System.Deployment.Internal.Isolation.IDefinitionIdentity _idComPtr;

        public DefinitionIdentity()
        {
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.CreateDefinition();
        }

        public DefinitionIdentity(System.Deployment.Application.ReferenceIdentity refId)
        {
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.CreateDefinition();
            foreach (System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE identity_attribute in refId.Attributes)
            {
                this[identity_attribute.Namespace, identity_attribute.Name] = identity_attribute.Value;
            }
        }

        public DefinitionIdentity(System.Deployment.Internal.Isolation.IDefinitionIdentity idComPtr)
        {
            this._idComPtr = idComPtr;
        }

        public DefinitionIdentity(AssemblyName asmName)
        {
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.CreateDefinition();
            this["name"] = asmName.Name;
            this["version"] = asmName.Version.ToString();
            if (asmName.CultureInfo != null)
            {
                this["culture"] = asmName.CultureInfo.Name;
            }
            byte[] publicKeyToken = asmName.GetPublicKeyToken();
            if ((publicKeyToken != null) && (publicKeyToken.Length > 0))
            {
                this["publicKeyToken"] = HexString.FromBytes(publicKeyToken);
            }
        }

        public DefinitionIdentity(string text)
        {
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.TextToDefinition(0, text);
        }

        public object Clone()
        {
            return new System.Deployment.Application.DefinitionIdentity(this._idComPtr.Clone(IntPtr.Zero, null));
        }

        public override bool Equals(object obj)
        {
            return ((obj is System.Deployment.Application.DefinitionIdentity) && System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.AreDefinitionsEqual(0, this.ComPointer, ((System.Deployment.Application.DefinitionIdentity) obj).ComPointer));
        }

        public override int GetHashCode()
        {
            return (int) this.Hash;
        }

        public bool Matches(System.Deployment.Application.ReferenceIdentity refId, bool exact)
        {
            return (System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.DoesDefinitionMatchReference(exact ? 1 : 0, this._idComPtr, refId.ComPointer) && (this.Version == refId.Version));
        }

        public System.Deployment.Application.DefinitionIdentity ToPKTGroupId()
        {
            System.Deployment.Application.DefinitionIdentity identity = (System.Deployment.Application.DefinitionIdentity) this.Clone();
            identity["version"] = null;
            identity["publicKeyToken"] = null;
            return identity;
        }

        public override string ToString()
        {
            return System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.DefinitionToText(0, this._idComPtr);
        }

        public System.Deployment.Application.DefinitionIdentity ToSubscriptionId()
        {
            System.Deployment.Application.DefinitionIdentity identity = (System.Deployment.Application.DefinitionIdentity) this.Clone();
            identity["version"] = null;
            return identity;
        }

        public System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE[] Attributes
        {
            get
            {
                System.Deployment.Internal.Isolation.IEnumIDENTITY_ATTRIBUTE o = null;
                System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE[] identity_attributeArray2;
                try
                {
                    ArrayList list = new ArrayList();
                    o = this._idComPtr.EnumAttributes();
                    System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE[] rgAttributes = new System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE[1];
                    while (o.Next(1, rgAttributes) == 1)
                    {
                        list.Add(rgAttributes[0]);
                    }
                    identity_attributeArray2 = (System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE[]) list.ToArray(typeof(System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE));
                }
                finally
                {
                    if (o != null)
                    {
                        Marshal.ReleaseComObject(o);
                    }
                }
                return identity_attributeArray2;
            }
        }

        public System.Deployment.Internal.Isolation.IDefinitionIdentity ComPointer
        {
            get
            {
                return this._idComPtr;
            }
        }

        public ulong Hash
        {
            get
            {
                return System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.HashDefinition(0, this._idComPtr);
            }
        }

        public bool IsEmpty
        {
            get
            {
                foreach (System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE identity_attribute in this.Attributes)
                {
                    if (!string.IsNullOrEmpty(identity_attribute.Value))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public string this[string name]
        {
            get
            {
                return this._idComPtr.GetAttribute(null, name);
            }
            set
            {
                this._idComPtr.SetAttribute(null, name, value);
            }
        }

        public string this[string ns, string name]
        {
            set
            {
                this._idComPtr.SetAttribute(ns, name, value);
            }
        }

        public string KeyForm
        {
            get
            {
                return System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.GenerateDefinitionKey(0, this._idComPtr);
            }
        }

        public string Name
        {
            get
            {
                return this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        public string ProcessorArchitecture
        {
            get
            {
                return this["processorArchitecture"];
            }
        }

        public string PublicKeyToken
        {
            get
            {
                return this["publicKeyToken"];
            }
        }

        public System.Version Version
        {
            get
            {
                string version = this["version"];
                if (version == null)
                {
                    return null;
                }
                return new System.Version(version);
            }
        }
    }
}

