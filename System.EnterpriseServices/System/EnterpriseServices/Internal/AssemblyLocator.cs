namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;

    [Guid("458aa3b5-265a-4b75-bc05-9bea4630cf18")]
    public class AssemblyLocator : MarshalByRefObject, IAssemblyLocator
    {
        string[] IAssemblyLocator.GetModules(string appdir, string appName, string name)
        {
            string[] strArray2;
            if ((appdir != null) && (appdir.Length > 0))
            {
                AssemblyLocator locator = null;
                try
                {
                    AppDomainSetup info = new AppDomainSetup {
                        ApplicationBase = appdir
                    };
                    AppDomain domain = AppDomain.CreateDomain(appName, null, info);
                    if (domain != null)
                    {
                        ObjectHandle handle = domain.CreateInstance(typeof(AssemblyLocator).Assembly.FullName, typeof(AssemblyLocator).FullName);
                        if (handle != null)
                        {
                            locator = (AssemblyLocator) handle.Unwrap();
                        }
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    return null;
                }
                return ((IAssemblyLocator) locator).GetModules(null, null, name);
            }
            try
            {
                Module[] modules = Assembly.Load(name).GetModules();
                string[] strArray = new string[modules.Length];
                for (int i = 0; i < modules.Length; i++)
                {
                    strArray[i] = modules[i].FullyQualifiedName;
                }
                strArray2 = strArray;
            }
            catch (Exception exception2)
            {
                if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                {
                    throw;
                }
                throw exception2;
            }
            return strArray2;
        }
    }
}

