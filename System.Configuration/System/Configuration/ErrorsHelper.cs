namespace System.Configuration
{
    using System;
    using System.Collections.Generic;

    internal static class ErrorsHelper
    {
        internal static void AddError(ref List<ConfigurationException> errors, ConfigurationException e)
        {
            if (errors == null)
            {
                errors = new List<ConfigurationException>();
            }
            ConfigurationErrorsException exception = e as ConfigurationErrorsException;
            if (exception == null)
            {
                errors.Add(e);
            }
            else
            {
                ICollection<ConfigurationException> errorsGeneric = exception.ErrorsGeneric;
                if (errorsGeneric.Count == 1)
                {
                    errors.Add(e);
                }
                else
                {
                    errors.AddRange(errorsGeneric);
                }
            }
        }

        internal static void AddErrors(ref List<ConfigurationException> errors, ICollection<ConfigurationException> coll)
        {
            if ((coll != null) && (coll.Count != 0))
            {
                foreach (ConfigurationException exception in coll)
                {
                    AddError(ref errors, exception);
                }
            }
        }

        internal static int GetErrorCount(List<ConfigurationException> errors)
        {
            if (errors == null)
            {
                return 0;
            }
            return errors.Count;
        }

        internal static ConfigurationErrorsException GetErrorsException(List<ConfigurationException> errors)
        {
            if (errors == null)
            {
                return null;
            }
            return new ConfigurationErrorsException(errors);
        }

        internal static bool GetHasErrors(List<ConfigurationException> errors)
        {
            return (GetErrorCount(errors) > 0);
        }

        internal static void ThrowOnErrors(List<ConfigurationException> errors)
        {
            ConfigurationErrorsException errorsException = GetErrorsException(errors);
            if (errorsException != null)
            {
                throw errorsException;
            }
        }
    }
}

