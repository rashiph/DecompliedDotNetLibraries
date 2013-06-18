namespace System.Data.OleDb
{
    using System;
    using System.Data.ProviderBase;

    internal sealed class OleDbConnectionPoolGroupProviderInfo : DbConnectionPoolGroupProviderInfo
    {
        private bool _hasQuoteFix;
        private string _quotePrefix;
        private string _quoteSuffix;

        internal OleDbConnectionPoolGroupProviderInfo()
        {
        }

        internal void SetQuoteFix(string prefix, string suffix)
        {
            this._quotePrefix = prefix;
            this._quoteSuffix = suffix;
            this._hasQuoteFix = true;
        }

        internal bool HasQuoteFix
        {
            get
            {
                return this._hasQuoteFix;
            }
        }

        internal string QuotePrefix
        {
            get
            {
                return this._quotePrefix;
            }
        }

        internal string QuoteSuffix
        {
            get
            {
                return this._quoteSuffix;
            }
        }
    }
}

