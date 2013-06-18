namespace System.Configuration
{
    using System;
    using System.Reflection;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CallbackValidatorAttribute : ConfigurationValidatorAttribute
    {
        private ValidatorCallback _callbackMethod;
        private string _callbackMethodName = string.Empty;
        private System.Type _type;

        public string CallbackMethodName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._callbackMethodName;
            }
            set
            {
                this._callbackMethodName = value;
                this._callbackMethod = null;
            }
        }

        public System.Type Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
                this._callbackMethod = null;
            }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                if (this._callbackMethod == null)
                {
                    if (this._type == null)
                    {
                        throw new ArgumentNullException("Type");
                    }
                    if (!string.IsNullOrEmpty(this._callbackMethodName))
                    {
                        MethodInfo method = this._type.GetMethod(this._callbackMethodName, BindingFlags.Public | BindingFlags.Static);
                        if (method != null)
                        {
                            ParameterInfo[] parameters = method.GetParameters();
                            if ((parameters.Length == 1) && (parameters[0].ParameterType == typeof(object)))
                            {
                                this._callbackMethod = (ValidatorCallback) Delegate.CreateDelegate(typeof(ValidatorCallback), method);
                            }
                        }
                    }
                }
                if (this._callbackMethod == null)
                {
                    throw new ArgumentException(System.Configuration.SR.GetString("Validator_method_not_found", new object[] { this._callbackMethodName }));
                }
                return new CallbackValidator(this._callbackMethod);
            }
        }
    }
}

