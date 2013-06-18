namespace System.EnterpriseServices
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;

    internal class MethodCallMessageWrapperEx : MethodCallMessageWrapper
    {
        private System.Reflection.MethodBase _mb;

        public MethodCallMessageWrapperEx(IMethodCallMessage imcmsg, System.Reflection.MethodBase mb) : base(imcmsg)
        {
            this._mb = mb;
        }

        public override System.Reflection.MethodBase MethodBase
        {
            get
            {
                return this._mb;
            }
        }
    }
}

