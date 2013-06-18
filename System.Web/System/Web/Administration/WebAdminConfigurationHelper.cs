namespace System.Web.Administration
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;

    [Serializable]
    internal sealed class WebAdminConfigurationHelper : MarshalByRefObject, IRegisteredObject
    {
        public WebAdminConfigurationHelper()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public object CallMembershipProviderMethod(string methodName, object[] parameters, Type[] paramTypes)
        {
            Type type = typeof(HttpContext).Assembly.GetType("System.Web.Security.Membership");
            object obj2 = null;
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            MethodInfo method = null;
            if (paramTypes != null)
            {
                method = type.GetMethod(methodName, bindingAttr, null, paramTypes, null);
            }
            else
            {
                method = type.GetMethod(methodName, bindingAttr);
            }
            if (method != null)
            {
                if (HttpRuntime.NamedPermissionSet != null)
                {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
                obj2 = method.Invoke(null, parameters);
            }
            object[] objArray = new object[parameters.Length + 1];
            objArray[0] = obj2;
            int num = 1;
            for (int i = 0; i < parameters.Length; i++)
            {
                objArray[num++] = parameters[i];
            }
            return objArray;
        }

        public object CallRoleProviderMethod(string methodName, object[] parameters, Type[] paramTypes)
        {
            Type type = typeof(HttpContext).Assembly.GetType("System.Web.Security.Roles");
            object obj2 = null;
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            MethodInfo method = null;
            if (paramTypes != null)
            {
                method = type.GetMethod(methodName, bindingAttr, null, paramTypes, null);
            }
            else
            {
                method = type.GetMethod(methodName, bindingAttr);
            }
            if (method != null)
            {
                if (HttpRuntime.NamedPermissionSet != null)
                {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
                obj2 = method.Invoke(null, parameters);
            }
            object[] objArray = new object[parameters.Length + 1];
            objArray[0] = obj2;
            int num = 1;
            for (int i = 0; i < parameters.Length; i++)
            {
                objArray[num++] = parameters[i];
            }
            return objArray;
        }

        public object GetMembershipProviderProperty(string propertyName)
        {
            Type type = typeof(HttpContext).Assembly.GetType("System.Web.Security.Membership");
            BindingFlags invokeAttr = BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            if (HttpRuntime.NamedPermissionSet != null)
            {
                HttpRuntime.NamedPermissionSet.PermitOnly();
            }
            return type.InvokeMember(propertyName, invokeAttr, null, null, null, CultureInfo.InvariantCulture);
        }

        public VirtualDirectory GetVirtualDirectory(string path)
        {
            if (HttpRuntime.NamedPermissionSet != null)
            {
                HttpRuntime.NamedPermissionSet.PermitOnly();
            }
            return HostingEnvironment.VirtualPathProvider.GetDirectory(path);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
        }
    }
}

