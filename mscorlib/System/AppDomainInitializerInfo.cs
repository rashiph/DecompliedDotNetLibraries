namespace System
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    internal class AppDomainInitializerInfo
    {
        internal ItemInfo[] Info = null;

        internal AppDomainInitializerInfo(AppDomainInitializer init)
        {
            if (init != null)
            {
                List<ItemInfo> list = new List<ItemInfo>();
                List<AppDomainInitializer> list2 = new List<AppDomainInitializer> {
                    init
                };
                int num = 0;
                while (list2.Count > num)
                {
                    Delegate[] invocationList = list2[num++].GetInvocationList();
                    for (int i = 0; i < invocationList.Length; i++)
                    {
                        if (!invocationList[i].Method.IsStatic)
                        {
                            if (invocationList[i].Target != null)
                            {
                                AppDomainInitializer target = invocationList[i].Target as AppDomainInitializer;
                                if (target == null)
                                {
                                    throw new ArgumentException(Environment.GetResourceString("Arg_MustBeStatic"), invocationList[i].Method.ReflectedType.FullName + "::" + invocationList[i].Method.Name);
                                }
                                list2.Add(target);
                            }
                        }
                        else
                        {
                            ItemInfo item = new ItemInfo {
                                TargetTypeAssembly = invocationList[i].Method.ReflectedType.Module.Assembly.FullName,
                                TargetTypeName = invocationList[i].Method.ReflectedType.FullName,
                                MethodName = invocationList[i].Method.Name
                            };
                            list.Add(item);
                        }
                    }
                }
                this.Info = list.ToArray();
            }
        }

        [SecuritySafeCritical]
        internal AppDomainInitializer Unwrap()
        {
            if (this.Info == null)
            {
                return null;
            }
            AppDomainInitializer a = null;
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            for (int i = 0; i < this.Info.Length; i++)
            {
                Assembly assembly = Assembly.Load(this.Info[i].TargetTypeAssembly);
                AppDomainInitializer b = (AppDomainInitializer) Delegate.CreateDelegate(typeof(AppDomainInitializer), assembly.GetType(this.Info[i].TargetTypeName), this.Info[i].MethodName);
                if (a == null)
                {
                    a = b;
                }
                else
                {
                    a = (AppDomainInitializer) Delegate.Combine(a, b);
                }
            }
            return a;
        }

        internal class ItemInfo
        {
            public string MethodName;
            public string TargetTypeAssembly;
            public string TargetTypeName;
        }
    }
}

