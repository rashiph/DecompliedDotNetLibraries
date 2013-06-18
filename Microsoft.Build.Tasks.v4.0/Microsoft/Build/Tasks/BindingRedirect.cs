namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Xml;

    internal sealed class BindingRedirect
    {
        private Version newVersion;
        private Version oldVersionHigh;
        private Version oldVersionLow;

        internal void Read(XmlTextReader reader)
        {
            string attribute = reader.GetAttribute("oldVersion");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(!string.IsNullOrEmpty(attribute), "AppConfig.BindingRedirectMissingOldVersion");
            int index = attribute.IndexOf('-');
            try
            {
                if (index != -1)
                {
                    this.oldVersionLow = new Version(attribute.Substring(0, index));
                    this.oldVersionHigh = new Version(attribute.Substring(index + 1));
                }
                else
                {
                    this.oldVersionLow = new Version(attribute);
                    this.oldVersionHigh = new Version(attribute);
                }
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception))
                {
                    throw;
                }
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(false, exception, "AppConfig.InvalidOldVersionAttribute", exception.Message);
            }
            string str2 = reader.GetAttribute("newVersion");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(!string.IsNullOrEmpty(str2), "AppConfig.BindingRedirectMissingNewVersion");
            try
            {
                this.newVersion = new Version(str2);
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception2))
                {
                    throw;
                }
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(false, exception2, "AppConfig.InvalidNewVersionAttribute", exception2.Message);
            }
        }

        internal Version NewVersion
        {
            get
            {
                return this.newVersion;
            }
            set
            {
                this.newVersion = value;
            }
        }

        internal Version OldVersionHigh
        {
            get
            {
                return this.oldVersionHigh;
            }
            set
            {
                this.oldVersionHigh = value;
            }
        }

        internal Version OldVersionLow
        {
            get
            {
                return this.oldVersionLow;
            }
            set
            {
                this.oldVersionLow = value;
            }
        }
    }
}

