namespace System.Net.Security
{
    using System;
    using System.Net;
    using System.Threading;

    internal static class SSPIHandleCache
    {
        private static SafeCredentialReference[] _CacheSlots = new SafeCredentialReference[0x20];
        private static int _Current = -1;
        private const int c_MaxCacheSize = 0x1f;

        internal static void CacheCredential(SafeFreeCredentials newHandle)
        {
            try
            {
                SafeCredentialReference reference = SafeCredentialReference.CreateReference(newHandle);
                if (reference != null)
                {
                    int index = Interlocked.Increment(ref _Current) & 0x1f;
                    reference = Interlocked.Exchange<SafeCredentialReference>(ref _CacheSlots[index], reference);
                    if (reference != null)
                    {
                        reference.Close();
                    }
                }
            }
            catch (Exception exception)
            {
                NclUtilities.IsFatal(exception);
            }
        }
    }
}

