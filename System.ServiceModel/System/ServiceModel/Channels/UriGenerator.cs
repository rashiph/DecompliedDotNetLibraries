namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.Threading;

    internal class UriGenerator
    {
        private long id;
        private string prefix;

        public UriGenerator() : this("uuid")
        {
        }

        public UriGenerator(string scheme) : this(scheme, ";")
        {
        }

        public UriGenerator(string scheme, string delimiter)
        {
            if (scheme == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("scheme"));
            }
            if (scheme.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("UriGeneratorSchemeMustNotBeEmpty"), "scheme"));
            }
            this.prefix = scheme + ":" + Guid.NewGuid().ToString() + delimiter + "id=";
        }

        public string Next()
        {
            long num = Interlocked.Increment(ref this.id);
            return (this.prefix + num.ToString(CultureInfo.InvariantCulture));
        }
    }
}

