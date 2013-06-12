namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class ComponentCollection : ReadOnlyCollectionBase
    {
        public ComponentCollection(IComponent[] components)
        {
            base.InnerList.AddRange(components);
        }

        public void CopyTo(IComponent[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public virtual IComponent this[string name]
        {
            get
            {
                if (name != null)
                {
                    foreach (IComponent component in base.InnerList)
                    {
                        if (((component != null) && (component.Site != null)) && ((component.Site.Name != null) && string.Equals(component.Site.Name, name, StringComparison.OrdinalIgnoreCase)))
                        {
                            return component;
                        }
                    }
                }
                return null;
            }
        }

        public virtual IComponent this[int index]
        {
            get
            {
                return (IComponent) base.InnerList[index];
            }
        }
    }
}

