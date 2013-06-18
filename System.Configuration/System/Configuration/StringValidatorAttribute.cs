namespace System.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class StringValidatorAttribute : ConfigurationValidatorAttribute
    {
        private string _invalidChars;
        private int _maxLength = 0x7fffffff;
        private int _minLength;

        public string InvalidCharacters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._invalidChars;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._invalidChars = value;
            }
        }

        public int MaxLength
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._maxLength;
            }
            set
            {
                if (this._minLength > value)
                {
                    throw new ArgumentOutOfRangeException("value", System.Configuration.SR.GetString("Validator_min_greater_than_max"));
                }
                this._maxLength = value;
            }
        }

        public int MinLength
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._minLength;
            }
            set
            {
                if (this._maxLength < value)
                {
                    throw new ArgumentOutOfRangeException("value", System.Configuration.SR.GetString("Validator_min_greater_than_max"));
                }
                this._minLength = value;
            }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new StringValidator(this._minLength, this._maxLength, this._invalidChars);
            }
        }
    }
}

