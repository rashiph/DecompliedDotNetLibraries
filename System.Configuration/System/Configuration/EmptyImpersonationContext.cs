namespace System.Configuration
{
    using System;

    internal class EmptyImpersonationContext : IDisposable
    {
        private static IDisposable s_emptyImpersonationContext;

        public void Dispose()
        {
        }

        internal static IDisposable GetStaticInstance()
        {
            if (s_emptyImpersonationContext == null)
            {
                s_emptyImpersonationContext = new EmptyImpersonationContext();
            }
            return s_emptyImpersonationContext;
        }
    }
}

