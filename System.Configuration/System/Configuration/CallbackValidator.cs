namespace System.Configuration
{
    using System;

    public sealed class CallbackValidator : ConfigurationValidatorBase
    {
        private ValidatorCallback _callback;
        private Type _type;

        internal CallbackValidator(ValidatorCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            this._type = null;
            this._callback = callback;
        }

        public CallbackValidator(Type type, ValidatorCallback callback) : this(callback)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this._type = type;
        }

        public override bool CanValidate(Type type)
        {
            if (!(type == this._type))
            {
                return (this._type == null);
            }
            return true;
        }

        public override void Validate(object value)
        {
            this._callback(value);
        }
    }
}

