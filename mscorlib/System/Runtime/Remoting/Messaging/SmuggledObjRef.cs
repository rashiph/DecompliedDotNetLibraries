namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Remoting;
    using System.Security;

    internal class SmuggledObjRef
    {
        [SecurityCritical]
        private System.Runtime.Remoting.ObjRef _objRef;

        [SecurityCritical]
        public SmuggledObjRef(System.Runtime.Remoting.ObjRef objRef)
        {
            this._objRef = objRef;
        }

        public System.Runtime.Remoting.ObjRef ObjRef
        {
            [SecurityCritical]
            get
            {
                return this._objRef;
            }
        }
    }
}

