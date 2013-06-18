namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Configuration.Internal;

    internal class ErrorRuntimeConfig : RuntimeConfig
    {
        internal ErrorRuntimeConfig() : base(new ErrorConfigRecord(), false)
        {
        }

        protected override object GetSectionObject(string sectionName)
        {
            throw new ConfigurationErrorsException();
        }

        private class ErrorConfigRecord : IInternalConfigRecord
        {
            internal ErrorConfigRecord()
            {
            }

            object IInternalConfigRecord.GetLkgSection(string configKey)
            {
                throw new ConfigurationErrorsException();
            }

            object IInternalConfigRecord.GetSection(string configKey)
            {
                throw new ConfigurationErrorsException();
            }

            void IInternalConfigRecord.RefreshSection(string configKey)
            {
                throw new ConfigurationErrorsException();
            }

            void IInternalConfigRecord.Remove()
            {
                throw new ConfigurationErrorsException();
            }

            void IInternalConfigRecord.ThrowIfInitErrors()
            {
                throw new ConfigurationErrorsException();
            }

            string IInternalConfigRecord.ConfigPath
            {
                get
                {
                    throw new ConfigurationErrorsException();
                }
            }

            bool IInternalConfigRecord.HasInitErrors
            {
                get
                {
                    return true;
                }
            }

            string IInternalConfigRecord.StreamName
            {
                get
                {
                    throw new ConfigurationErrorsException();
                }
            }
        }
    }
}

