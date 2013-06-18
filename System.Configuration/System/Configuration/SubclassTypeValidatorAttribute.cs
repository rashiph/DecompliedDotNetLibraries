namespace System.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SubclassTypeValidatorAttribute : ConfigurationValidatorAttribute
    {
        private Type _baseClass;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SubclassTypeValidatorAttribute(Type baseClass)
        {
            this._baseClass = baseClass;
        }

        public Type BaseClass
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._baseClass;
            }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new SubclassTypeValidator(this._baseClass);
            }
        }
    }
}

