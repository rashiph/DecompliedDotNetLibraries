namespace System.Deployment.Application
{
    using System;
    using System.Globalization;

    internal class DownloadOptions
    {
        public bool Background;
        public bool EnforceSizeLimit;
        public ulong Size;
        public ulong SizeLimit;

        public override string ToString()
        {
            return ((((" Background = " + this.Background.ToString()) + " EnforceSizeLimit = " + this.EnforceSizeLimit.ToString()) + " SizeLimit =" + this.SizeLimit.ToString(CultureInfo.InvariantCulture)) + " Size =" + this.Size.ToString(CultureInfo.InvariantCulture));
        }
    }
}

