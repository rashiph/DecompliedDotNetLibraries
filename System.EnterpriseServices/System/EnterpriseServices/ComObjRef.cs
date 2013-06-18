namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class ComObjRef : IObjectReference, ISerializable
    {
        private object _realobj;

        public ComObjRef(SerializationInfo info, StreamingContext ctx)
        {
            byte[] b = null;
            IntPtr zero = IntPtr.Zero;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name.Equals("buffer"))
                {
                    b = (byte[]) enumerator.Value;
                }
            }
            try
            {
                zero = Proxy.UnmarshalObject(b);
                this._realobj = Marshal.GetObjectForIUnknown(zero);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                }
            }
            if (this._realobj == null)
            {
                throw new NotSupportedException();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            throw new NotSupportedException();
        }

        public object GetRealObject(StreamingContext ctx)
        {
            return this._realobj;
        }
    }
}

