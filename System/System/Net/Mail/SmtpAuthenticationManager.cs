namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.Net;

    internal static class SmtpAuthenticationManager
    {
        private static ArrayList modules = new ArrayList();

        static SmtpAuthenticationManager()
        {
            if (ComNetOS.IsWin2K)
            {
                Register(new SmtpNegotiateAuthenticationModule());
            }
            Register(new SmtpNtlmAuthenticationModule());
            Register(new SmtpDigestAuthenticationModule());
            Register(new SmtpLoginAuthenticationModule());
        }

        internal static ISmtpAuthenticationModule[] GetModules()
        {
            lock (modules)
            {
                ISmtpAuthenticationModule[] array = new ISmtpAuthenticationModule[modules.Count];
                modules.CopyTo(0, array, 0, modules.Count);
                return array;
            }
        }

        internal static void Register(ISmtpAuthenticationModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }
            lock (modules)
            {
                modules.Add(module);
            }
        }
    }
}

