namespace System.Configuration
{
    using System;
    using System.Runtime;

    public abstract class ConfigurationValidatorBase
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ConfigurationValidatorBase()
        {
        }

        public virtual bool CanValidate(Type type)
        {
            return false;
        }

        public abstract void Validate(object value);
    }
}

