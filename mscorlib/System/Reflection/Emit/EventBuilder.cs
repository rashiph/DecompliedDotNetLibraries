namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ClassInterface(ClassInterfaceType.None), ComVisible(true), ComDefaultInterface(typeof(_EventBuilder)), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventBuilder : _EventBuilder
    {
        private EventAttributes m_attributes;
        private EventToken m_evToken;
        private ModuleBuilder m_module;
        private string m_name;
        private TypeBuilder m_type;

        private EventBuilder()
        {
        }

        internal EventBuilder(ModuleBuilder mod, string name, EventAttributes attr, TypeBuilder type, EventToken evToken)
        {
            this.m_name = name;
            this.m_module = mod;
            this.m_attributes = attr;
            this.m_evToken = evToken;
            this.m_type = type;
        }

        [SecuritySafeCritical]
        public void AddOtherMethod(MethodBuilder mdBuilder)
        {
            this.SetMethodSemantics(mdBuilder, MethodSemanticsAttributes.Other);
        }

        public EventToken GetEventToken()
        {
            return this.m_evToken;
        }

        [SecuritySafeCritical]
        public void SetAddOnMethod(MethodBuilder mdBuilder)
        {
            this.SetMethodSemantics(mdBuilder, MethodSemanticsAttributes.AddOn);
        }

        [SecuritySafeCritical]
        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            if (customBuilder == null)
            {
                throw new ArgumentNullException("customBuilder");
            }
            this.m_type.ThrowIfCreated();
            customBuilder.CreateCustomAttribute(this.m_module, this.m_evToken.Token);
        }

        [SecuritySafeCritical, ComVisible(true)]
        public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
        {
            if (con == null)
            {
                throw new ArgumentNullException("con");
            }
            if (binaryAttribute == null)
            {
                throw new ArgumentNullException("binaryAttribute");
            }
            this.m_type.ThrowIfCreated();
            TypeBuilder.DefineCustomAttribute(this.m_module, this.m_evToken.Token, this.m_module.GetConstructorToken(con).Token, binaryAttribute, false, false);
        }

        [SecurityCritical]
        private void SetMethodSemantics(MethodBuilder mdBuilder, MethodSemanticsAttributes semantics)
        {
            if (mdBuilder == null)
            {
                throw new ArgumentNullException("mdBuilder");
            }
            this.m_type.ThrowIfCreated();
            TypeBuilder.DefineMethodSemantics(this.m_module.GetNativeHandle(), this.m_evToken.Token, semantics, mdBuilder.GetToken().Token);
        }

        [SecuritySafeCritical]
        public void SetRaiseMethod(MethodBuilder mdBuilder)
        {
            this.SetMethodSemantics(mdBuilder, MethodSemanticsAttributes.Fire);
        }

        [SecuritySafeCritical]
        public void SetRemoveOnMethod(MethodBuilder mdBuilder)
        {
            this.SetMethodSemantics(mdBuilder, MethodSemanticsAttributes.RemoveOn);
        }

        void _EventBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _EventBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _EventBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _EventBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }
    }
}

