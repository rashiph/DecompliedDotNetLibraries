namespace System.Web.Compilation
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;

    internal class BuildResultMainCodeAssembly : BuildResultCompiledAssembly
    {
        private MethodInfo _appInitializeMethod;
        private const string appInitializeMethodName = "AppInitialize";

        internal BuildResultMainCodeAssembly()
        {
        }

        internal BuildResultMainCodeAssembly(Assembly a) : base(a)
        {
            this.FindAppInitializeMethod();
        }

        internal void CallAppInitializeMethod()
        {
            if (this._appInitializeMethod != null)
            {
                using (new ApplicationImpersonationContext())
                {
                    using (HostingEnvironment.SetCultures())
                    {
                        this._appInitializeMethod.Invoke(null, null);
                    }
                }
            }
        }

        private void FindAppInitializeMethod()
        {
            foreach (Type type in this.ResultAssembly.GetExportedTypes())
            {
                MethodInfo info = this.FindAppInitializeMethod(type);
                if (info != null)
                {
                    if (this._appInitializeMethod != null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Duplicate_appinitialize", new object[] { this._appInitializeMethod.ReflectedType.FullName, type.FullName }));
                    }
                    this._appInitializeMethod = info;
                }
            }
        }

        private MethodInfo FindAppInitializeMethod(Type t)
        {
            return t.GetMethod("AppInitialize", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase, null, new Type[0], null);
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.BuildResultMainCodeAssembly;
        }

        internal override void GetPreservedAttributes(PreservationFileReader pfr)
        {
            base.GetPreservedAttributes(pfr);
            string attribute = pfr.GetAttribute("appInitializeClass");
            if (attribute != null)
            {
                Type t = this.ResultAssembly.GetType(attribute);
                this._appInitializeMethod = this.FindAppInitializeMethod(t);
            }
        }

        internal override void SetPreservedAttributes(PreservationFileWriter pfw)
        {
            base.SetPreservedAttributes(pfw);
            if (this._appInitializeMethod != null)
            {
                pfw.SetAttribute("appInitializeClass", this._appInitializeMethod.ReflectedType.FullName);
            }
        }
    }
}

