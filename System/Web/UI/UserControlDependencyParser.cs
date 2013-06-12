namespace System.Web.UI
{
    using System;

    internal class UserControlDependencyParser : TemplateControlDependencyParser
    {
        internal override string DefaultDirectiveName
        {
            get
            {
                return "control";
            }
        }
    }
}

