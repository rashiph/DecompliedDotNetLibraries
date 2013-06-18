namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [TypeForwardedFrom("System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class ServiceHostFactory : ServiceHostFactoryBase
    {
        private Collection<string> referencedAssemblies = new Collection<string>();

        internal void AddAssemblyReference(string assemblyName)
        {
            this.referencedAssemblies.Add(assemblyName);
        }

        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (!AspNetEnvironment.Enabled)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_ProcessNotExecutingUnderHostedContext("ServiceHostFactory.CreateServiceHost")));
            }
            if (string.IsNullOrEmpty(constructorString))
            {
                throw FxTrace.Exception.Argument("constructorString", System.ServiceModel.Activation.SR.Hosting_ServiceTypeNotProvided);
            }
            Type serviceType = Type.GetType(constructorString, false);
            if (serviceType == null)
            {
                if (this.referencedAssemblies.Count == 0)
                {
                    AspNetEnvironment.Current.EnsureAllReferencedAssemblyLoaded();
                }
                foreach (string str in this.referencedAssemblies)
                {
                    serviceType = Assembly.Load(str).GetType(constructorString, false);
                    if (serviceType != null)
                    {
                        break;
                    }
                }
            }
            if (serviceType == null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    serviceType = assemblies[i].GetType(constructorString, false);
                    if (serviceType != null)
                    {
                        break;
                    }
                }
            }
            if (serviceType == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_ServiceTypeNotResolved(constructorString)));
            }
            return this.CreateServiceHost(serviceType, baseAddresses);
        }

        protected virtual ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new ServiceHost(serviceType, baseAddresses);
        }
    }
}

