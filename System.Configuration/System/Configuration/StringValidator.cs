namespace System.Configuration
{
    using System;
    using System.Runtime;

    public class StringValidator : ConfigurationValidatorBase
    {
        private string _invalidChars;
        private int _maxLength;
        private int _minLength;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StringValidator(int minLength) : this(minLength, 0x7fffffff, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StringValidator(int minLength, int maxLength) : this(minLength, maxLength, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StringValidator(int minLength, int maxLength, string invalidCharacters)
        {
            this._minLength = minLength;
            this._maxLength = maxLength;
            this._invalidChars = invalidCharacters;
        }

        public override bool CanValidate(Type type)
        {
            return (type == typeof(string));
        }

        public override void Validate(object value)
        {
            ValidatorUtils.HelperParamValidation(value, typeof(string));
            string str = value as string;
            int num = (str == null) ? 0 : str.Length;
            if (num < this._minLength)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Validator_string_min_length", new object[] { this._minLength }));
            }
            if (num > this._maxLength)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Validator_string_max_length", new object[] { this._maxLength }));
            }
            if (((num > 0) && (this._invalidChars != null)) && (this._invalidChars.Length > 0))
            {
                char[] destination = new char[this._invalidChars.Length];
                this._invalidChars.CopyTo(0, destination, 0, this._invalidChars.Length);
                if (str.IndexOfAny(destination) != -1)
                {
                    throw new ArgumentException(System.Configuration.SR.GetString("Validator_string_invalid_chars", new object[] { this._invalidChars }));
                }
            }
        }
    }
}

