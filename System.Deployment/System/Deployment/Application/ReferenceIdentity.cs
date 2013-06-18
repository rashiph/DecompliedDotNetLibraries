namespace System.Deployment.Application
{
    using System;
    using System.Collections;
    using System.Deployment.Internal.Isolation;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class ReferenceIdentity : ICloneable
    {
        private System.Deployment.Internal.Isolation.IReferenceIdentity _idComPtr;

        public ReferenceIdentity()
        {
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.CreateReference();
        }

        public ReferenceIdentity(System.Deployment.Internal.Isolation.IReferenceIdentity idComPtr)
        {
            this._idComPtr = idComPtr;
        }

        public ReferenceIdentity(string text)
        {
            this._idComPtr = System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.TextToReference(0, text);
        }

        public object Clone()
        {
            return new System.Deployment.Application.ReferenceIdentity(this._idComPtr.Clone(IntPtr.Zero, null));
        }

        public override bool Equals(object obj)
        {
            return ((obj is System.Deployment.Application.ReferenceIdentity) && System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.AreReferencesEqual(0, this.ComPointer, ((System.Deployment.Application.ReferenceIdentity) obj).ComPointer));
        }

        public override int GetHashCode()
        {
            return (int) this.Hash;
        }

        public override string ToString()
        {
            return System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.ReferenceToText(0, this._idComPtr);
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

        public System.Deployment.Internal.Isolation.IReferenceIdentity ComPointer
        {
            get
            {
                return this._idComPtr;
            }
        }

        public string Culture
        {
            get
            {
                return this["culture"];
            }
        }

        public ulong Hash
        {
            get
            {
                return System.Deployment.Internal.Isolation.IsolationInterop.IdentityAuthority.HashReference(0, this._idComPtr);
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

        public string Name
        {
            get
            {
                return this["name"];
            }
        }

        public string ProcessorArchitecture
        {
            get
            {
                return this["processorArchitecture"];
            }
            set
            {
                this["processorArchitecture"] = value;
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

