namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class PolicyChain : IDisposable
    {
        private InternalPolicyElement[] m_chain;
        private HGlobalSafeHandle m_nativeChain;

        public PolicyChain(CardSpacePolicyElement[] elements)
        {
            int length = elements.Length;
            this.m_chain = new InternalPolicyElement[length];
            for (int i = 0; i < length; i++)
            {
                this.m_chain[i] = new InternalPolicyElement(elements[i]);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            if (this.m_chain != null)
            {
                foreach (InternalPolicyElement element in this.m_chain)
                {
                    if (element != null)
                    {
                        element.Dispose();
                    }
                }
                this.m_chain = null;
            }
            if (this.m_nativeChain != null)
            {
                this.m_nativeChain.Dispose();
            }
        }

        public SafeHandle DoMarshal()
        {
            if (this.m_nativeChain == null)
            {
                int size = InternalPolicyElement.Size;
                int length = this.m_chain.Length;
                this.m_nativeChain = HGlobalSafeHandle.Construct((int) (length * size));
                IntPtr handle = this.m_nativeChain.DangerousGetHandle();
                foreach (InternalPolicyElement element in this.m_chain)
                {
                    element.DoMarshal(handle);
                    handle = new IntPtr(((long) ((ulong) handle.ToPointer())) + size);
                }
            }
            return this.m_nativeChain;
        }

        ~PolicyChain()
        {
            this.Dispose(false);
        }

        public int Length
        {
            get
            {
                return this.m_chain.Length;
            }
        }
    }
}

