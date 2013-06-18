namespace System.ServiceModel.Description
{
    using System;

    public sealed class MetadataImporterQuotas
    {
        private const int DefaultMaxPolicyAssertions = 0x400;
        private const int DefaultMaxPolicyConversionContexts = 0x20;
        private const int DefaultMaxPolicyNodes = 0x1000;
        private const int DefaultMaxYields = 0x400;
        private int maxPolicyAssertions;
        private int maxPolicyConversionContexts;
        private int maxPolicyNodes;
        private int maxYields = 0x400;

        private static MetadataImporterQuotas CreateDefaultSettings()
        {
            return new MetadataImporterQuotas { maxPolicyConversionContexts = 0x20, maxPolicyNodes = 0x1000, maxPolicyAssertions = 0x400 };
        }

        private static MetadataImporterQuotas CreateMaxSettings()
        {
            return new MetadataImporterQuotas { maxPolicyConversionContexts = 0x20, maxPolicyNodes = 0x7fffffff, maxPolicyAssertions = 0x7fffffff };
        }

        public static MetadataImporterQuotas Defaults
        {
            get
            {
                return CreateDefaultSettings();
            }
        }

        public static MetadataImporterQuotas Max
        {
            get
            {
                return CreateMaxSettings();
            }
        }

        internal int MaxPolicyAssertions
        {
            get
            {
                return this.maxPolicyAssertions;
            }
            set
            {
                this.maxPolicyAssertions = value;
            }
        }

        internal int MaxPolicyConversionContexts
        {
            get
            {
                return this.maxPolicyConversionContexts;
            }
            set
            {
                this.maxPolicyConversionContexts = value;
            }
        }

        internal int MaxPolicyNodes
        {
            get
            {
                return this.maxPolicyNodes;
            }
            set
            {
                this.maxPolicyNodes = value;
            }
        }

        internal int MaxYields
        {
            get
            {
                return this.maxYields;
            }
            set
            {
                this.maxYields = value;
            }
        }
    }
}

