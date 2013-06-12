namespace System.Web.UI
{
    using System;
    using System.Web;

    internal abstract class TemplateControlDependencyParser : DependencyParser
    {
        protected TemplateControlDependencyParser()
        {
        }

        internal override void ProcessMainDirectiveAttribute(string deviceName, string name, string value)
        {
            string str;
            if (((str = name) != null) && (str == "masterpagefile"))
            {
                value = value.Trim();
                if (value.Length > 0)
                {
                    base.AddDependency(VirtualPath.Create(value));
                }
            }
            else
            {
                base.ProcessMainDirectiveAttribute(deviceName, name, value);
            }
        }
    }
}

