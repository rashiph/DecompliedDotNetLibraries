namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;

    public class ProductBuilder : IProductBuilder
    {
        private string culture;
        private Microsoft.Build.Tasks.Deployment.Bootstrapper.Product product;

        internal ProductBuilder(Microsoft.Build.Tasks.Deployment.Bootstrapper.Product product)
        {
            this.product = product;
            this.culture = string.Empty;
        }

        internal ProductBuilder(Microsoft.Build.Tasks.Deployment.Bootstrapper.Product product, string culture)
        {
            this.product = product;
            this.culture = culture;
        }

        internal string Name
        {
            get
            {
                return this.product.Name;
            }
        }

        public Microsoft.Build.Tasks.Deployment.Bootstrapper.Product Product
        {
            get
            {
                return this.product;
            }
        }

        internal string ProductCode
        {
            get
            {
                return this.product.ProductCode;
            }
        }
    }
}

