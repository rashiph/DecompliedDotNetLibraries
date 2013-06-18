namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    internal class Syntax
    {
        public string attributeSyntax;
        public OMObjectClass oMObjectClass;
        public int oMSyntax;

        public Syntax(string attributeSyntax, int oMSyntax, OMObjectClass oMObjectClass)
        {
            this.attributeSyntax = attributeSyntax;
            this.oMSyntax = oMSyntax;
            this.oMObjectClass = oMObjectClass;
        }

        public bool Equals(Syntax syntax)
        {
            bool flag = true;
            if (!syntax.attributeSyntax.Equals(this.attributeSyntax) || (syntax.oMSyntax != this.oMSyntax))
            {
                return false;
            }
            return (((((this.oMObjectClass == null) || (syntax.oMObjectClass != null)) && ((this.oMObjectClass != null) || (syntax.oMObjectClass == null))) && (((this.oMObjectClass == null) || (syntax.oMObjectClass == null)) || this.oMObjectClass.Equals(syntax.oMObjectClass))) && flag);
        }
    }
}

