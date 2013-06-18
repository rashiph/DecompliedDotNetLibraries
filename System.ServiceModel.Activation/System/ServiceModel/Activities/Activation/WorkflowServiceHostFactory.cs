namespace System.ServiceModel.Activities.Activation
{
    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Xaml;
    using System.Xml;
    using System.Xml.Linq;

    public class WorkflowServiceHostFactory : ServiceHostFactoryBase
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            WorkflowServiceHost host = null;
            Stream stream;
            string str2;
            if (string.IsNullOrEmpty(constructorString))
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.WorkflowServiceHostFactoryConstructorStringNotProvided));
            }
            if (baseAddresses == null)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.ArgumentNull("baseAddresses");
            }
            if (baseAddresses.Length == 0)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.BaseAddressesNotProvided));
            }
            if (!HostingEnvironment.IsHosted)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_ProcessNotExecutingUnderHostedContext("WorkflowServiceHostFactory.CreateServiceHost")));
            }
            string virtualPath = VirtualPathUtility.Combine(AspNetEnvironment.Current.XamlFileBaseLocation, constructorString);
            if (this.GetServiceFileStreamOrCompiledCustomString(virtualPath, baseAddresses, out stream, out str2))
            {
                object obj2;
                using (stream)
                {
                    BuildManager.GetReferencedAssemblies();
                    XamlXmlReaderSettings settings = new XamlXmlReaderSettings {
                        ProvideLineInfo = true
                    };
                    XamlReader xamlReader = ActivityXamlServices.CreateReader(new XamlXmlReader(XmlReader.Create(stream), settings));
                    if (System.ServiceModel.Activation.TD.XamlServicesLoadStartIsEnabled())
                    {
                        System.ServiceModel.Activation.TD.XamlServicesLoadStart();
                    }
                    obj2 = XamlServices.Load(xamlReader);
                    if (System.ServiceModel.Activation.TD.XamlServicesLoadStopIsEnabled())
                    {
                        System.ServiceModel.Activation.TD.XamlServicesLoadStop();
                    }
                }
                WorkflowService service = null;
                if (obj2 is Activity)
                {
                    service = new WorkflowService {
                        Body = (Activity) obj2
                    };
                }
                else if (obj2 is WorkflowService)
                {
                    service = (WorkflowService) obj2;
                }
                if (service != null)
                {
                    if (service.Name == null)
                    {
                        string fileName = VirtualPathUtility.GetFileName(virtualPath);
                        string namespaceName = string.Format(CultureInfo.InvariantCulture, "/{0}{1}", new object[] { ServiceHostingEnvironment.SiteName, VirtualPathUtility.GetDirectory(ServiceHostingEnvironment.FullVirtualPath) });
                        service.Name = XName.Get(XmlConvert.EncodeLocalName(fileName), namespaceName);
                        if ((service.ConfigurationName == null) && (service.Body != null))
                        {
                            service.ConfigurationName = XmlConvert.EncodeLocalName(service.Body.DisplayName);
                        }
                    }
                    host = this.CreateWorkflowServiceHost(service, baseAddresses);
                }
            }
            else
            {
                Type typeFromAssembliesInCurrentDomain = this.GetTypeFromAssembliesInCurrentDomain(constructorString);
                if (null == typeFromAssembliesInCurrentDomain)
                {
                    typeFromAssembliesInCurrentDomain = this.GetTypeFromCompileCustomString(str2, constructorString);
                }
                if (null == typeFromAssembliesInCurrentDomain)
                {
                    BuildManager.GetReferencedAssemblies();
                    typeFromAssembliesInCurrentDomain = this.GetTypeFromAssembliesInCurrentDomain(constructorString);
                }
                if (null != typeFromAssembliesInCurrentDomain)
                {
                    if (!TypeHelper.AreTypesCompatible(typeFromAssembliesInCurrentDomain, typeof(Activity)))
                    {
                        throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.TypeNotActivity(typeFromAssembliesInCurrentDomain.FullName)));
                    }
                    Activity activity = (Activity) Activator.CreateInstance(typeFromAssembliesInCurrentDomain);
                    host = this.CreateWorkflowServiceHost(activity, baseAddresses);
                }
            }
            if (host == null)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.CannotResolveConstructorStringToWorkflowType(constructorString)));
            }
            ((IDurableInstancingOptions) host.DurableInstancingOptions).SetScopeName(XName.Get(XmlConvert.EncodeLocalName(VirtualPathUtility.GetFileName(ServiceHostingEnvironment.FullVirtualPath)), string.Format(CultureInfo.InvariantCulture, "/{0}{1}", new object[] { ServiceHostingEnvironment.SiteName, VirtualPathUtility.GetDirectory(ServiceHostingEnvironment.FullVirtualPath) })));
            return host;
        }

        protected virtual WorkflowServiceHost CreateWorkflowServiceHost(Activity activity, Uri[] baseAddresses)
        {
            return new WorkflowServiceHost(activity, baseAddresses);
        }

        protected virtual WorkflowServiceHost CreateWorkflowServiceHost(WorkflowService service, Uri[] baseAddresses)
        {
            return new WorkflowServiceHost(service, baseAddresses);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        private bool GetServiceFileStreamOrCompiledCustomString(string virtualPath, Uri[] baseAddresses, out Stream serviceFileStream, out string compiledCustomString)
        {
            IDisposable disposable = null;
            bool flag;
            compiledCustomString = null;
            serviceFileStream = null;
            try
            {
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        disposable = HostingEnvironmentWrapper.UnsafeImpersonate();
                    }
                    if (HostingEnvironment.VirtualPathProvider.FileExists(virtualPath))
                    {
                        serviceFileStream = HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).Open();
                        return true;
                    }
                    if (!AspNetEnvironment.Current.IsConfigurationBased)
                    {
                        compiledCustomString = BuildManager.GetCompiledCustomString(baseAddresses[0].AbsolutePath);
                    }
                    flag = false;
                }
                finally
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }
            return flag;
        }

        private Type GetTypeFromAssembliesInCurrentDomain(string typeString)
        {
            Type type = Type.GetType(typeString, false);
            if (null == type)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeString, false);
                    if (null != type)
                    {
                        return type;
                    }
                }
            }
            return type;
        }

        private Type GetTypeFromCompileCustomString(string compileCustomString, string typeString)
        {
            if (string.IsNullOrEmpty(compileCustomString))
            {
                return null;
            }
            string[] strArray = compileCustomString.Split(new char[] { '|' });
            if (strArray.Length < 3)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.InvalidCompiledString(compileCustomString)));
            }
            Type type = null;
            for (int i = 3; i < strArray.Length; i++)
            {
                type = Assembly.Load(strArray[i]).GetType(typeString, false);
                if (type != null)
                {
                    return type;
                }
            }
            return type;
        }
    }
}

