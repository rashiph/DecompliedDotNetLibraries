namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Util;

    internal class MasterPageDependencyParser : UserControlDependencyParser
    {
        internal override void ProcessDirective(string directiveName, IDictionary directive)
        {
            base.ProcessDirective(directiveName, directive);
            if (StringUtil.EqualsIgnoreCase(directiveName, "masterType"))
            {
                VirtualPath andRemoveVirtualPathAttribute = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualPath");
                if (andRemoveVirtualPathAttribute != null)
                {
                    base.AddDependency(andRemoveVirtualPathAttribute);
                }
            }
        }

        internal override string DefaultDirectiveName
        {
            get
            {
                return "master";
            }
        }
    }
}

