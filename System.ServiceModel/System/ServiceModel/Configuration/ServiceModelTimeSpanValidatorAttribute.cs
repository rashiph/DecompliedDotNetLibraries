namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class ServiceModelTimeSpanValidatorAttribute : ConfigurationValidatorAttribute
    {
        private TimeSpanValidatorAttribute innerValidatorAttribute = new TimeSpanValidatorAttribute();

        public ServiceModelTimeSpanValidatorAttribute()
        {
            this.innerValidatorAttribute.MaxValueString = TimeoutHelper.MaxWait.ToString();
        }

        public TimeSpan MaxValue
        {
            get
            {
                return this.innerValidatorAttribute.MaxValue;
            }
        }

        public string MaxValueString
        {
            get
            {
                return this.innerValidatorAttribute.MaxValueString;
            }
            set
            {
                this.innerValidatorAttribute.MaxValueString = value;
            }
        }

        public TimeSpan MinValue
        {
            get
            {
                return this.innerValidatorAttribute.MinValue;
            }
        }

        public string MinValueString
        {
            get
            {
                return this.innerValidatorAttribute.MinValueString;
            }
            set
            {
                this.innerValidatorAttribute.MinValueString = value;
            }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new TimeSpanOrInfiniteValidator(this.MinValue, this.MaxValue);
            }
        }
    }
}

