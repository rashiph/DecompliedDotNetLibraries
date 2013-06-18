namespace Microsoft.VisualBasic.ApplicationServices
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class AssemblyInfo
    {
        private Assembly m_Assembly;
        private string m_CompanyName = null;
        private string m_Copyright = null;
        private string m_Description = null;
        private string m_ProductName = null;
        private string m_Title = null;
        private string m_Trademark = null;

        public AssemblyInfo(Assembly currentAssembly)
        {
            if (currentAssembly == null)
            {
                throw ExceptionUtils.GetArgumentNullException("CurrentAssembly");
            }
            this.m_Assembly = currentAssembly;
        }

        private object GetAttribute(Type AttributeType)
        {
            object[] customAttributes = this.m_Assembly.GetCustomAttributes(AttributeType, true);
            if (customAttributes.Length == 0)
            {
                return null;
            }
            return customAttributes[0];
        }

        public string AssemblyName
        {
            get
            {
                return this.m_Assembly.GetName().Name;
            }
        }

        public string CompanyName
        {
            get
            {
                if (this.m_CompanyName == null)
                {
                    AssemblyCompanyAttribute attribute = (AssemblyCompanyAttribute) this.GetAttribute(typeof(AssemblyCompanyAttribute));
                    if (attribute == null)
                    {
                        this.m_CompanyName = "";
                    }
                    else
                    {
                        this.m_CompanyName = attribute.Company;
                    }
                }
                return this.m_CompanyName;
            }
        }

        public string Copyright
        {
            get
            {
                if (this.m_Copyright == null)
                {
                    AssemblyCopyrightAttribute attribute = (AssemblyCopyrightAttribute) this.GetAttribute(typeof(AssemblyCopyrightAttribute));
                    if (attribute == null)
                    {
                        this.m_Copyright = "";
                    }
                    else
                    {
                        this.m_Copyright = attribute.Copyright;
                    }
                }
                return this.m_Copyright;
            }
        }

        public string Description
        {
            get
            {
                if (this.m_Description == null)
                {
                    AssemblyDescriptionAttribute attribute = (AssemblyDescriptionAttribute) this.GetAttribute(typeof(AssemblyDescriptionAttribute));
                    if (attribute == null)
                    {
                        this.m_Description = "";
                    }
                    else
                    {
                        this.m_Description = attribute.Description;
                    }
                }
                return this.m_Description;
            }
        }

        public string DirectoryPath
        {
            get
            {
                return Path.GetDirectoryName(this.m_Assembly.Location);
            }
        }

        public ReadOnlyCollection<Assembly> LoadedAssemblies
        {
            get
            {
                Collection<Assembly> list = new Collection<Assembly>();
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    list.Add(assembly);
                }
                return new ReadOnlyCollection<Assembly>(list);
            }
        }

        public string ProductName
        {
            get
            {
                if (this.m_ProductName == null)
                {
                    AssemblyProductAttribute attribute = (AssemblyProductAttribute) this.GetAttribute(typeof(AssemblyProductAttribute));
                    if (attribute == null)
                    {
                        this.m_ProductName = "";
                    }
                    else
                    {
                        this.m_ProductName = attribute.Product;
                    }
                }
                return this.m_ProductName;
            }
        }

        public string StackTrace
        {
            get
            {
                return Environment.StackTrace;
            }
        }

        public string Title
        {
            get
            {
                if (this.m_Title == null)
                {
                    AssemblyTitleAttribute attribute = (AssemblyTitleAttribute) this.GetAttribute(typeof(AssemblyTitleAttribute));
                    if (attribute == null)
                    {
                        this.m_Title = "";
                    }
                    else
                    {
                        this.m_Title = attribute.Title;
                    }
                }
                return this.m_Title;
            }
        }

        public string Trademark
        {
            get
            {
                if (this.m_Trademark == null)
                {
                    AssemblyTrademarkAttribute attribute = (AssemblyTrademarkAttribute) this.GetAttribute(typeof(AssemblyTrademarkAttribute));
                    if (attribute == null)
                    {
                        this.m_Trademark = "";
                    }
                    else
                    {
                        this.m_Trademark = attribute.Trademark;
                    }
                }
                return this.m_Trademark;
            }
        }

        public System.Version Version
        {
            get
            {
                return this.m_Assembly.GetName().Version;
            }
        }

        public long WorkingSet
        {
            get
            {
                return Environment.WorkingSet;
            }
        }
    }
}

