namespace System.Web.Services.Description
{
    using System;
    using System.Web.Services.Protocols;

    internal class MimeFormReflector : MimeReflector
    {
        internal override bool ReflectParameters()
        {
            if (!ValueCollectionParameterReader.IsSupported(base.ReflectionContext.Method))
            {
                return false;
            }
            base.ReflectionContext.ReflectStringParametersMessage();
            MimeContentBinding extension = new MimeContentBinding {
                Type = "application/x-www-form-urlencoded"
            };
            base.ReflectionContext.OperationBinding.Input.Extensions.Add(extension);
            return true;
        }

        internal override bool ReflectReturn()
        {
            return false;
        }
    }
}

