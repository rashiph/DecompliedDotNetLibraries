namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;

    internal sealed class ProductValidationResults : XmlValidationResults
    {
        private Hashtable packageValidationResults;

        public ProductValidationResults(string filePath) : base(filePath)
        {
            this.packageValidationResults = new Hashtable();
        }

        public void AddPackageResults(string culture, XmlValidationResults results)
        {
            if (!this.packageValidationResults.Contains(culture))
            {
                this.packageValidationResults.Add(culture, results);
            }
        }

        public XmlValidationResults PackageResults(string culture)
        {
            return (XmlValidationResults) this.packageValidationResults[culture];
        }
    }
}

