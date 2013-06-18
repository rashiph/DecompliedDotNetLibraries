namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web.UI;

    [Serializable, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class WebControlToolboxItem : ToolboxItem
    {
        private int persistChildren;
        private string toolData;

        public WebControlToolboxItem()
        {
            this.persistChildren = -1;
        }

        public WebControlToolboxItem(Type type) : base(type)
        {
            this.persistChildren = -1;
            this.BuildMetadataCache(type);
        }

        protected WebControlToolboxItem(SerializationInfo info, StreamingContext context)
        {
            this.persistChildren = -1;
            this.Deserialize(info, context);
        }

        private void BuildMetadataCache(Type type)
        {
            this.toolData = ExtractToolboxData(type);
            this.persistChildren = ExtractPersistChildrenAttribute(type);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost host)
        {
            throw new Exception(System.Design.SR.GetString("Toolbox_OnWebformsPage"));
        }

        protected override void Deserialize(SerializationInfo info, StreamingContext context)
        {
            base.Deserialize(info, context);
            this.toolData = info.GetString("ToolData");
            this.persistChildren = info.GetInt32("PersistChildren");
        }

        private static int ExtractPersistChildrenAttribute(Type type)
        {
            if (type != null)
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(PersistChildrenAttribute), true);
                if ((customAttributes != null) && (customAttributes.Length == 1))
                {
                    PersistChildrenAttribute attribute = (PersistChildrenAttribute) customAttributes[0];
                    if (!attribute.Persist)
                    {
                        return 0;
                    }
                    return 1;
                }
            }
            if (!PersistChildrenAttribute.Default.Persist)
            {
                return 0;
            }
            return 1;
        }

        private static string ExtractToolboxData(Type type)
        {
            string str = string.Empty;
            if (type == null)
            {
                return str;
            }
            object[] customAttributes = type.GetCustomAttributes(typeof(ToolboxDataAttribute), false);
            if ((customAttributes != null) && (customAttributes.Length == 1))
            {
                ToolboxDataAttribute attribute = (ToolboxDataAttribute) customAttributes[0];
                return attribute.Data;
            }
            string name = type.Name;
            return ("<{0}:" + name + " runat=\"server\"></{0}:" + name + ">");
        }

        public object GetToolAttributeValue(IDesignerHost host, Type attributeType)
        {
            if (!(attributeType == typeof(PersistChildrenAttribute)))
            {
                throw new ArgumentException(System.Design.SR.GetString("Toolbox_BadAttributeType"));
            }
            if (this.persistChildren == -1)
            {
                Type toolType = this.GetToolType(host);
                this.persistChildren = ExtractPersistChildrenAttribute(toolType);
            }
            return (this.persistChildren == 1);
        }

        public string GetToolHtml(IDesignerHost host)
        {
            if (this.toolData == null)
            {
                Type toolType = this.GetToolType(host);
                this.toolData = ExtractToolboxData(toolType);
            }
            return this.toolData;
        }

        public Type GetToolType(IDesignerHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            return this.GetType(host, base.AssemblyName, base.TypeName, true);
        }

        public override void Initialize(Type type)
        {
            base.Initialize(type);
            this.BuildMetadataCache(type);
        }

        protected override void Serialize(SerializationInfo info, StreamingContext context)
        {
            base.Serialize(info, context);
            info.AddValue("ToolData", this.toolData);
            info.AddValue("PersistChildren", this.persistChildren);
        }
    }
}

