namespace System.Net
{
    using System;
    using System.Configuration;

    internal sealed class TimeoutValidator : ConfigurationValidatorBase
    {
        private bool _zeroValid;

        internal TimeoutValidator(bool zeroValid)
        {
            this._zeroValid = zeroValid;
        }

        public override bool CanValidate(Type type)
        {
            if (!(type == typeof(int)))
            {
                return (type == typeof(long));
            }
            return true;
        }

        public override void Validate(object value)
        {
            if (value != null)
            {
                int num = (int) value;
                if ((!this._zeroValid || (num != 0)) && ((num <= 0) && (num != -1)))
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("net_io_timeout_use_gt_zero"));
                }
            }
        }
    }
}

