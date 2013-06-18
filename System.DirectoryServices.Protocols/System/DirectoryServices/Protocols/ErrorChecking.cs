namespace System.DirectoryServices.Protocols
{
    using System;

    internal class ErrorChecking
    {
        public static void CheckAndSetLdapError(int error)
        {
            if (error != 0)
            {
                string str;
                if (Utility.IsResultCode((ResultCode) error))
                {
                    str = OperationErrorMappings.MapResultCode(error);
                    throw new DirectoryOperationException(null, str);
                }
                if (Utility.IsLdapError((LdapError) error))
                {
                    str = LdapErrorMappings.MapResultCode(error);
                    throw new LdapException(error, str);
                }
                throw new LdapException(error);
            }
        }
    }
}

