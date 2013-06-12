namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Util;

    internal class PageDependencyParser : TemplateControlDependencyParser
    {
        protected override void PrepareParse()
        {
            if (((base.PagesConfig != null) && (base.PagesConfig.MasterPageFileInternal != null)) && (base.PagesConfig.MasterPageFileInternal.Length != 0))
            {
                base.AddDependency(VirtualPath.Create(base.PagesConfig.MasterPageFileInternal));
            }
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive)
        {
            base.ProcessDirective(directiveName, directive);
            if (StringUtil.EqualsIgnoreCase(directiveName, "previousPageType") || StringUtil.EqualsIgnoreCase(directiveName, "masterType"))
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
                return "page";
            }
        }
    }
}

