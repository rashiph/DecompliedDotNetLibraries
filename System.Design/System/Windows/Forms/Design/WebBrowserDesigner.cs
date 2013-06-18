namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class WebBrowserDesigner : AxDesigner
    {
        public override void Initialize(IComponent c)
        {
            WebBrowser browser = c as WebBrowser;
            this.Url = browser.Url;
            browser.Url = new Uri("about:blank");
            base.Initialize(c);
            browser.Url = null;
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            WebBrowser component = (WebBrowser) base.Component;
            if (component != null)
            {
                component.MinimumSize = new Size(20, 20);
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Url" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(WebBrowserDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        protected override System.ComponentModel.InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if (base.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.Inherited)
                {
                    return System.ComponentModel.InheritanceAttribute.InheritedReadOnly;
                }
                return base.InheritanceAttribute;
            }
        }

        public Uri Url
        {
            get
            {
                return (Uri) base.ShadowProperties["Url"];
            }
            set
            {
                base.ShadowProperties["Url"] = value;
            }
        }
    }
}

