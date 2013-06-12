namespace System.Web.Util
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Configuration;

    public class RequestValidator
    {
        private static RequestValidator _customValidator;
        private static readonly Lazy<RequestValidator> _customValidatorResolver = new Lazy<RequestValidator>(new Func<RequestValidator>(RequestValidator.GetCustomValidatorFromConfig));

        private static RequestValidator GetCustomValidatorFromConfig()
        {
            HttpRuntimeSection httpRuntime = RuntimeConfig.GetAppConfig().HttpRuntime;
            Type userBaseType = ConfigUtil.GetType(httpRuntime.RequestValidationType, "requestValidationType", httpRuntime);
            ConfigUtil.CheckBaseType(typeof(RequestValidator), userBaseType, "requestValidationType", httpRuntime);
            return (RequestValidator) HttpRuntime.CreatePublicInstance(userBaseType);
        }

        internal static void InitializeOnFirstRequest()
        {
            RequestValidator local1 = _customValidatorResolver.Value;
        }

        private static bool IsAtoZ(char c)
        {
            return (((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')));
        }

        protected internal virtual bool IsValidRequestString(HttpContext context, string value, RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex)
        {
            if (requestValidationSource == RequestValidationSource.Headers)
            {
                validationFailureIndex = 0;
                return true;
            }
            return !CrossSiteScriptingValidation.IsDangerousString(value, out validationFailureIndex);
        }

        public static RequestValidator Current
        {
            get
            {
                if (_customValidator == null)
                {
                    _customValidator = _customValidatorResolver.Value;
                }
                return _customValidator;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _customValidator = value;
            }
        }
    }
}

