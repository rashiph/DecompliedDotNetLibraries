namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;

    [Guid("532BF563-A85D-4088-8048-41F51AC5239F"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public class Product : IProduct
    {
        private CopyAllFilesType copyAllPackageFiles;
        private Hashtable cultures;
        private List<List<Product>> dependencies;
        private ProductCollection includes;
        private ArrayList missingDependencies;
        private XmlNode node;
        private PackageCollection packages;
        private string productCode;
        private ProductValidationResults validationResults;

        public Product()
        {
            throw new InvalidOperationException();
        }

        internal Product(XmlNode node, string code, ProductValidationResults validationResults, string copyAll)
        {
            this.node = node;
            this.packages = new PackageCollection();
            this.includes = new ProductCollection();
            this.dependencies = new List<List<Product>>();
            this.missingDependencies = new ArrayList();
            this.productCode = code;
            this.validationResults = validationResults;
            this.cultures = new Hashtable();
            if (copyAll == "IfNotHomeSite")
            {
                this.copyAllPackageFiles = CopyAllFilesType.CopyAllFilesIfNotHomeSite;
            }
            else if (copyAll == "false")
            {
                this.copyAllPackageFiles = CopyAllFilesType.CopyAllFilesFalse;
            }
            else
            {
                this.copyAllPackageFiles = CopyAllFilesType.CopyAllFilesTrue;
            }
        }

        internal void AddDependentProduct(Product product)
        {
            List<Product> list;
            list = new List<Product> {
                product,
                list
            };
        }

        internal void AddIncludedProduct(Product product)
        {
            this.includes.Add(product);
        }

        internal void AddMissingDependency(ArrayList productCodes)
        {
            bool flag = false;
            foreach (ArrayList list in this.missingDependencies)
            {
                bool flag2 = true;
                foreach (string str in list)
                {
                    if (!productCodes.Contains(str))
                    {
                        flag2 = false;
                        break;
                    }
                }
                if (flag2)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                this.missingDependencies.Add(productCodes);
            }
        }

        internal void AddPackage(Package package)
        {
            if ((package == null) || string.IsNullOrEmpty(package.Culture))
            {
                throw new ArgumentNullException("package");
            }
            if (!this.cultures.Contains(package.Culture.ToLowerInvariant()))
            {
                this.packages.Add(package);
                this.cultures.Add(package.Culture.ToLowerInvariant(), package);
            }
        }

        internal bool ContainsCulture(string culture)
        {
            return this.cultures.Contains(culture.ToLowerInvariant());
        }

        internal bool ContainsDependencies(List<Product> dependenciesToCheck)
        {
            foreach (List<Product> list in this.dependencies)
            {
                bool flag = true;
                foreach (Product product in list)
                {
                    bool flag2 = false;
                    foreach (Product product2 in dependenciesToCheck)
                    {
                        if (product.productCode == product2.productCode)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (!flag2)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }

        internal XmlValidationResults GetPackageValidationResults(string culture)
        {
            if (this.validationResults == null)
            {
                return null;
            }
            return this.validationResults.PackageResults(culture);
        }

        internal CopyAllFilesType CopyAllPackageFiles
        {
            get
            {
                return this.copyAllPackageFiles;
            }
        }

        internal List<List<Product>> Dependencies
        {
            get
            {
                return this.dependencies;
            }
        }

        public ProductCollection Includes
        {
            get
            {
                return this.includes;
            }
        }

        internal ArrayList MissingDependencies
        {
            get
            {
                return this.missingDependencies;
            }
        }

        public string Name
        {
            get
            {
                CultureInfo defaultCultureInfo = Util.DefaultCultureInfo;
                Package package = this.packages.Package(defaultCultureInfo.Name);
                if (package == null)
                {
                    while ((defaultCultureInfo != null) && (defaultCultureInfo != CultureInfo.InvariantCulture))
                    {
                        package = this.packages.Package(defaultCultureInfo.Parent.Name);
                        if (package != null)
                        {
                            return package.Name;
                        }
                        defaultCultureInfo = defaultCultureInfo.Parent;
                    }
                    if (this.packages.Count > 0)
                    {
                        return this.packages.Item(0).Name;
                    }
                    return this.productCode.ToString();
                }
                return package.Name;
            }
        }

        internal XmlNode Node
        {
            get
            {
                return this.node;
            }
        }

        internal PackageCollection Packages
        {
            get
            {
                return this.packages;
            }
        }

        public Microsoft.Build.Tasks.Deployment.Bootstrapper.ProductBuilder ProductBuilder
        {
            get
            {
                return new Microsoft.Build.Tasks.Deployment.Bootstrapper.ProductBuilder(this);
            }
        }

        public string ProductCode
        {
            get
            {
                return this.productCode;
            }
        }

        internal bool ValidationPassed
        {
            get
            {
                return ((this.validationResults == null) || this.validationResults.ValidationPassed);
            }
        }

        internal ProductValidationResults ValidationResults
        {
            get
            {
                return this.validationResults;
            }
        }
    }
}

