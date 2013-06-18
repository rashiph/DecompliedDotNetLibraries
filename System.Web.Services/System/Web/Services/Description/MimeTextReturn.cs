namespace System.Web.Services.Description
{
    using System;

    internal class MimeTextReturn : MimeReturn
    {
        private MimeTextBinding textBinding;

        internal MimeTextBinding TextBinding
        {
            get
            {
                return this.textBinding;
            }
            set
            {
                this.textBinding = value;
            }
        }
    }
}

