namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class InternalPolicyElement : IDisposable
    {
        private CardSpacePolicyElement m_element;
        private NativePolicyElement m_nativeElement;
        private IntPtr m_nativePtr = IntPtr.Zero;

        public InternalPolicyElement(CardSpacePolicyElement element)
        {
            if (element.Target == null)
            {
                throw InfoCardTrace.ThrowHelperArgumentNull("PolicyElement.Target");
            }
            this.m_element = element;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IntPtr.Zero != this.m_nativePtr)
            {
                Marshal.DestroyStructure(this.m_nativePtr, typeof(NativePolicyElement));
                this.m_nativePtr = IntPtr.Zero;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        public void DoMarshal(IntPtr ptr)
        {
            string outerXml = this.m_element.Target.OuterXml;
            string str2 = "";
            this.m_nativePtr = ptr;
            if (this.m_element.Issuer != null)
            {
                str2 = this.m_element.Issuer.OuterXml;
            }
            string str3 = string.Empty;
            if (this.m_element.Parameters != null)
            {
                str3 = CardSpaceSelector.XmlToString(this.m_element.Parameters);
            }
            this.m_nativeElement.targetEndpointAddress = outerXml;
            this.m_nativeElement.issuerEndpointAddress = str2;
            this.m_nativeElement.issuedTokenParameters = str3;
            this.m_nativeElement.policyNoticeLink = (null != this.m_element.PolicyNoticeLink) ? this.m_element.PolicyNoticeLink.ToString() : null;
            this.m_nativeElement.policyNoticeVersion = this.m_element.PolicyNoticeVersion;
            this.m_nativeElement.isManagedCardProvider = this.m_element.IsManagedIssuer;
            Marshal.StructureToPtr(this.m_nativeElement, ptr, false);
        }

        ~InternalPolicyElement()
        {
            this.Dispose(false);
        }

        public static int Size
        {
            get
            {
                return Marshal.SizeOf(typeof(NativePolicyElement));
            }
        }
    }
}

