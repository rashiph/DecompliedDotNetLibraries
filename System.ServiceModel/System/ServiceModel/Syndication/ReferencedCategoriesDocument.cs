namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ReferencedCategoriesDocument : CategoriesDocument
    {
        private Uri link;

        public ReferencedCategoriesDocument()
        {
        }

        public ReferencedCategoriesDocument(Uri link)
        {
            if (link == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("link");
            }
            this.link = link;
        }

        internal override bool IsInline
        {
            get
            {
                return false;
            }
        }

        public Uri Link
        {
            get
            {
                return this.link;
            }
            set
            {
                this.link = value;
            }
        }
    }
}

