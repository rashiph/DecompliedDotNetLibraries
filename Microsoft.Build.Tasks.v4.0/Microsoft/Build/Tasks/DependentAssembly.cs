namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    internal sealed class DependentAssembly
    {
        private BindingRedirect[] bindingRedirects;
        private AssemblyName partialAssemblyName;

        internal void Read(XmlTextReader reader)
        {
            ArrayList list = new ArrayList();
            if (this.bindingRedirects != null)
            {
                list.AddRange(this.bindingRedirects);
            }
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.EndElement) && AppConfig.StringEquals(reader.Name, "dependentassembly"))
                {
                    break;
                }
                if ((reader.NodeType == XmlNodeType.Element) && AppConfig.StringEquals(reader.Name, "assemblyIdentity"))
                {
                    string str = null;
                    string str2 = "null";
                    string str3 = "neutral";
                    while (reader.MoveToNextAttribute())
                    {
                        if (AppConfig.StringEquals(reader.Name, "name"))
                        {
                            str = reader.Value;
                        }
                        else
                        {
                            if (AppConfig.StringEquals(reader.Name, "publicKeyToken"))
                            {
                                str2 = reader.Value;
                                continue;
                            }
                            if (AppConfig.StringEquals(reader.Name, "culture"))
                            {
                                str3 = reader.Value;
                            }
                        }
                    }
                    string assemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, Version=0.0.0.0, Culture={1}, PublicKeyToken={2}", new object[] { str, str3, str2 });
                    try
                    {
                        this.partialAssemblyName = new AssemblyNameExtension(assemblyName).AssemblyName;
                    }
                    catch (FileLoadException exception)
                    {
                        Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(false, exception, "AppConfig.InvalidAssemblyIdentityFields");
                    }
                }
                if ((reader.NodeType == XmlNodeType.Element) && AppConfig.StringEquals(reader.Name, "bindingRedirect"))
                {
                    BindingRedirect redirect = new BindingRedirect();
                    redirect.Read(reader);
                    list.Add(redirect);
                }
            }
            this.bindingRedirects = (BindingRedirect[]) list.ToArray(typeof(BindingRedirect));
        }

        internal BindingRedirect[] BindingRedirects
        {
            get
            {
                return this.bindingRedirects;
            }
            set
            {
                this.bindingRedirects = value;
            }
        }

        internal AssemblyName PartialAssemblyName
        {
            get
            {
                if (this.partialAssemblyName == null)
                {
                    return null;
                }
                return (AssemblyName) this.partialAssemblyName.Clone();
            }
            set
            {
                this.partialAssemblyName = (AssemblyName) value.Clone();
                this.partialAssemblyName.Version = null;
            }
        }
    }
}

