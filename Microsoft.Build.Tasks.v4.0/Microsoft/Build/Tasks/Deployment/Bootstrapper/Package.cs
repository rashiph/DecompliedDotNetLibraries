namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    [ComVisible(false)]
    internal class Package
    {
        private string culture;
        private string name;
        private XmlNode node;
        private Microsoft.Build.Tasks.Deployment.Bootstrapper.Product product;
        private XmlValidationResults validationResults;

        public Package(Microsoft.Build.Tasks.Deployment.Bootstrapper.Product product, XmlNode node, XmlValidationResults validationResults, string name, string culture)
        {
            this.product = product;
            this.node = node;
            this.name = name;
            this.culture = culture;
            this.validationResults = validationResults;
        }

        public string Culture
        {
            get
            {
                return this.culture;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal XmlNode Node
        {
            get
            {
                return this.node;
            }
        }

        public Microsoft.Build.Tasks.Deployment.Bootstrapper.Product Product
        {
            get
            {
                return this.product;
            }
        }

        internal bool ValidationPassed
        {
            get
            {
                return ((this.validationResults == null) || this.validationResults.ValidationPassed);
            }
        }

        internal XmlValidationResults ValidationResults
        {
            get
            {
                return this.validationResults;
            }
        }
    }
}

