namespace System.Reflection
{
    using Microsoft.Runtime.Hosting;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public class StrongNameKeyPair : IDeserializationCallback, ISerializable
    {
        private byte[] _keyPairArray;
        private string _keyPairContainer;
        private bool _keyPairExported;
        private byte[] _publicKey;

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public StrongNameKeyPair(FileStream keyPairFile)
        {
            if (keyPairFile == null)
            {
                throw new ArgumentNullException("keyPairFile");
            }
            int length = (int) keyPairFile.Length;
            this._keyPairArray = new byte[length];
            keyPairFile.Read(this._keyPairArray, 0, length);
            this._keyPairExported = true;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public StrongNameKeyPair(byte[] keyPairArray)
        {
            if (keyPairArray == null)
            {
                throw new ArgumentNullException("keyPairArray");
            }
            this._keyPairArray = new byte[keyPairArray.Length];
            Array.Copy(keyPairArray, this._keyPairArray, keyPairArray.Length);
            this._keyPairExported = true;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public StrongNameKeyPair(string keyPairContainer)
        {
            if (keyPairContainer == null)
            {
                throw new ArgumentNullException("keyPairContainer");
            }
            this._keyPairContainer = keyPairContainer;
            this._keyPairExported = false;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected StrongNameKeyPair(SerializationInfo info, StreamingContext context)
        {
            this._keyPairExported = (bool) info.GetValue("_keyPairExported", typeof(bool));
            this._keyPairArray = (byte[]) info.GetValue("_keyPairArray", typeof(byte[]));
            this._keyPairContainer = (string) info.GetValue("_keyPairContainer", typeof(string));
            this._publicKey = (byte[]) info.GetValue("_publicKey", typeof(byte[]));
        }

        [SecurityCritical]
        private unsafe byte[] ComputePublicKey()
        {
            byte[] dest = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                IntPtr zero = IntPtr.Zero;
                int pcbPublicKeyBlob = 0;
                try
                {
                    bool flag;
                    if (this._keyPairExported)
                    {
                        flag = StrongNameHelpers.StrongNameGetPublicKey(null, this._keyPairArray, this._keyPairArray.Length, out zero, out pcbPublicKeyBlob);
                    }
                    else
                    {
                        flag = StrongNameHelpers.StrongNameGetPublicKey(this._keyPairContainer, (byte[]) null, 0, out zero, out pcbPublicKeyBlob);
                    }
                    if (!flag)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_StrongNameGetPublicKey"));
                    }
                    dest = new byte[pcbPublicKeyBlob];
                    Buffer.memcpy((byte*) zero.ToPointer(), 0, dest, 0, pcbPublicKeyBlob);
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        StrongNameHelpers.StrongNameFreeBuffer(zero);
                    }
                }
            }
            return dest;
        }

        private bool GetKeyPair(out object arrayOrContainer)
        {
            arrayOrContainer = this._keyPairExported ? ((object) this._keyPairArray) : ((object) this._keyPairContainer);
            return this._keyPairExported;
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_keyPairExported", this._keyPairExported);
            info.AddValue("_keyPairArray", this._keyPairArray);
            info.AddValue("_keyPairContainer", this._keyPairContainer);
            info.AddValue("_publicKey", this._publicKey);
        }

        public byte[] PublicKey
        {
            [SecuritySafeCritical]
            get
            {
                if (this._publicKey == null)
                {
                    this._publicKey = this.ComputePublicKey();
                }
                byte[] destinationArray = new byte[this._publicKey.Length];
                Array.Copy(this._publicKey, destinationArray, this._publicKey.Length);
                return destinationArray;
            }
        }
    }
}

