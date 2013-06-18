namespace System.Web.Services.Description
{
    using System;

    internal class HttpPostProtocolReflector : HttpProtocolReflector
    {
        protected override void BeginClass()
        {
            if (!base.IsEmptyBinding)
            {
                HttpBinding extension = new HttpBinding {
                    Verb = "POST"
                };
                base.Binding.Extensions.Add(extension);
                HttpAddressBinding binding2 = new HttpAddressBinding {
                    Location = base.ServiceUrl
                };
                base.Port.Extensions.Add(binding2);
            }
        }

        protected override bool ReflectMethod()
        {
            if (!base.ReflectMimeParameters())
            {
                return false;
            }
            if (!base.ReflectMimeReturn())
            {
                return false;
            }
            HttpOperationBinding extension = new HttpOperationBinding {
                Location = base.MethodUrl
            };
            base.OperationBinding.Extensions.Add(extension);
            return true;
        }

        public override string ProtocolName
        {
            get
            {
                return "HttpPost";
            }
        }
    }
}

