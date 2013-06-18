namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class GlobalDataBindingHandler
    {
        private static Hashtable dataBindingHandlerTable;
        public static readonly EventHandler Handler = new EventHandler(GlobalDataBindingHandler.OnDataBind);

        private GlobalDataBindingHandler()
        {
        }

        public static void OnDataBind(object sender, EventArgs e)
        {
            Control component = (Control) sender;
            IDataBindingsAccessor accessor = (IDataBindingsAccessor) sender;
            if (accessor.HasDataBindings)
            {
                DataBindingHandlerAttribute attribute = (DataBindingHandlerAttribute) TypeDescriptor.GetAttributes(sender)[typeof(DataBindingHandlerAttribute)];
                if ((attribute != null) && (attribute.HandlerTypeName.Length != 0))
                {
                    ISite site = component.Site;
                    IDesignerHost designerHost = null;
                    if (site == null)
                    {
                        Page page = component.Page;
                        if (page != null)
                        {
                            site = page.Site;
                        }
                        else
                        {
                            for (Control control2 = component.Parent; (site == null) && (control2 != null); control2 = control2.Parent)
                            {
                                if (control2.Site != null)
                                {
                                    site = control2.Site;
                                }
                            }
                        }
                    }
                    if (site != null)
                    {
                        designerHost = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                    }
                    if ((designerHost != null) && (designerHost.GetDesigner(component) == null))
                    {
                        DataBindingHandler handler = null;
                        try
                        {
                            string handlerTypeName = attribute.HandlerTypeName;
                            handler = (DataBindingHandler) DataBindingHandlerTable[handlerTypeName];
                            if (handler == null)
                            {
                                Type type = Type.GetType(handlerTypeName);
                                if (type != null)
                                {
                                    handler = (DataBindingHandler) Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, null);
                                    DataBindingHandlerTable[handlerTypeName] = handler;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }
                        if (handler != null)
                        {
                            handler.DataBindControl(designerHost, component);
                        }
                    }
                }
            }
        }

        private static Hashtable DataBindingHandlerTable
        {
            get
            {
                if (dataBindingHandlerTable == null)
                {
                    dataBindingHandlerTable = new Hashtable();
                }
                return dataBindingHandlerTable;
            }
        }
    }
}

