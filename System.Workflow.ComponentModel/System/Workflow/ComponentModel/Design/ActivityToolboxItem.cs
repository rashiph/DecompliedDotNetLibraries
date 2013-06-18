namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class ActivityToolboxItem : ToolboxItem
    {
        private const string ActivitySuffix = "Activity";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityToolboxItem()
        {
        }

        public ActivityToolboxItem(Type type) : base(type)
        {
            if (type != null)
            {
                if (type.Name != null)
                {
                    string name = type.Name;
                    if (((type.Assembly == Assembly.GetExecutingAssembly()) || (((type.Assembly != null) && (type.Assembly.FullName != null)) && type.Assembly.FullName.Equals("System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.OrdinalIgnoreCase))) && (type.Name.EndsWith("Activity", StringComparison.Ordinal) && !type.Name.Equals("Activity", StringComparison.Ordinal)))
                    {
                        name = type.Name.Substring(0, type.Name.Length - "Activity".Length);
                    }
                    base.DisplayName = name;
                }
                base.Description = ActivityDesigner.GetActivityDescription(type);
            }
        }

        protected ActivityToolboxItem(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost host)
        {
            Type c = this.GetType(host, base.AssemblyName, base.TypeName, true);
            if ((c == null) && (host != null))
            {
                c = host.GetType(base.TypeName);
            }
            if (c == null)
            {
                ITypeProviderCreator service = null;
                if (host != null)
                {
                    service = (ITypeProviderCreator) host.GetService(typeof(ITypeProviderCreator));
                }
                if (service != null)
                {
                    Assembly transientAssembly = service.GetTransientAssembly(base.AssemblyName);
                    if (transientAssembly != null)
                    {
                        c = transientAssembly.GetType(base.TypeName);
                    }
                }
                if (c == null)
                {
                    c = this.GetType(host, base.AssemblyName, base.TypeName, true);
                }
            }
            ArrayList list = new ArrayList();
            if ((c != null) && typeof(IComponent).IsAssignableFrom(c))
            {
                list.Add(TypeDescriptor.CreateInstance(null, c, null, null));
            }
            IComponent[] array = new IComponent[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public virtual IComponent[] CreateComponentsWithUI(IDesignerHost host)
        {
            return this.CreateComponentsCore(host);
        }

        public static string GetToolboxDisplayName(Type activityType)
        {
            if (activityType == null)
            {
                throw new ArgumentNullException("activityType");
            }
            string name = activityType.Name;
            object[] customAttributes = activityType.GetCustomAttributes(typeof(ToolboxItemAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                ToolboxItemAttribute attribute = customAttributes[0] as ToolboxItemAttribute;
                if ((attribute != null) && (attribute.ToolboxItemType != null))
                {
                    try
                    {
                        ToolboxItem item = Activator.CreateInstance(attribute.ToolboxItemType, new object[] { activityType }) as ToolboxItem;
                        if (item != null)
                        {
                            name = item.DisplayName;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            if ((((activityType.Assembly != null) && (activityType.Assembly.FullName != null)) && (activityType.Assembly.FullName.Equals("System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.OrdinalIgnoreCase) || activityType.Assembly.FullName.Equals(Assembly.GetExecutingAssembly().FullName, StringComparison.OrdinalIgnoreCase))) && (name.EndsWith("Activity", StringComparison.Ordinal) && !name.Equals("Activity", StringComparison.Ordinal)))
            {
                name = name.Substring(0, name.Length - "Activity".Length);
            }
            return name;
        }

        public static Image GetToolboxImage(Type activityType)
        {
            if (activityType == null)
            {
                throw new ArgumentNullException("activityType");
            }
            Image image = null;
            if (activityType != null)
            {
                object[] customAttributes = activityType.GetCustomAttributes(typeof(ToolboxBitmapAttribute), false);
                if ((customAttributes != null) && (customAttributes.GetLength(0) == 0))
                {
                    customAttributes = activityType.GetCustomAttributes(typeof(ToolboxBitmapAttribute), true);
                }
                ToolboxBitmapAttribute attribute = ((customAttributes != null) && (customAttributes.GetLength(0) > 0)) ? (customAttributes[0] as ToolboxBitmapAttribute) : null;
                if (attribute != null)
                {
                    image = attribute.GetImage(activityType);
                }
            }
            return image;
        }
    }
}

