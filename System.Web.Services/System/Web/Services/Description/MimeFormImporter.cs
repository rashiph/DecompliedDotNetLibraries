namespace System.Web.Services.Description
{
    using System;
    using System.Web.Services.Protocols;

    internal class MimeFormImporter : MimeImporter
    {
        internal override MimeParameterCollection ImportParameters()
        {
            MimeContentBinding binding = (MimeContentBinding) base.ImportContext.OperationBinding.Input.Extensions.Find(typeof(MimeContentBinding));
            if (binding == null)
            {
                return null;
            }
            if (string.Compare(binding.Type, "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }
            MimeParameterCollection parameters = base.ImportContext.ImportStringParametersMessage();
            if (parameters == null)
            {
                return null;
            }
            parameters.WriterType = typeof(HtmlFormParameterWriter);
            return parameters;
        }

        internal override MimeReturn ImportReturn()
        {
            return null;
        }
    }
}

