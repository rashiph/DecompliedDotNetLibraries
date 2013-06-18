namespace System.Configuration
{
    using System;
    using System.Collections.Generic;

    internal class ConfigurationSchemaErrors
    {
        private List<ConfigurationException> _errorsAll;
        private List<ConfigurationException> _errorsGlobal;
        private List<ConfigurationException> _errorsLocal;

        internal ConfigurationSchemaErrors()
        {
        }

        internal void AddError(ConfigurationException ce, ExceptionAction action)
        {
            switch (action)
            {
                case ExceptionAction.NonSpecific:
                    ErrorsHelper.AddError(ref this._errorsAll, ce);
                    return;

                case ExceptionAction.Local:
                    ErrorsHelper.AddError(ref this._errorsLocal, ce);
                    return;

                case ExceptionAction.Global:
                    ErrorsHelper.AddError(ref this._errorsAll, ce);
                    ErrorsHelper.AddError(ref this._errorsGlobal, ce);
                    return;
            }
        }

        internal void AddSavedLocalErrors(ICollection<ConfigurationException> coll)
        {
            ErrorsHelper.AddErrors(ref this._errorsAll, coll);
        }

        internal bool HasErrors(bool ignoreLocal)
        {
            if (ignoreLocal)
            {
                return this.HasGlobalErrors;
            }
            return this.HasAllErrors;
        }

        internal void ResetLocalErrors()
        {
            this.RetrieveAndResetLocalErrors(false);
        }

        internal List<ConfigurationException> RetrieveAndResetLocalErrors(bool keepLocalErrors)
        {
            List<ConfigurationException> coll = this._errorsLocal;
            this._errorsLocal = null;
            if (keepLocalErrors)
            {
                ErrorsHelper.AddErrors(ref this._errorsAll, coll);
            }
            return coll;
        }

        internal void SetSingleGlobalError(ConfigurationException ce)
        {
            this._errorsAll = null;
            this._errorsLocal = null;
            this._errorsGlobal = null;
            this.AddError(ce, ExceptionAction.Global);
        }

        internal void ThrowIfErrors(bool ignoreLocal)
        {
            if (this.HasErrors(ignoreLocal))
            {
                if (this.HasGlobalErrors)
                {
                    throw new ConfigurationErrorsException(this._errorsGlobal);
                }
                throw new ConfigurationErrorsException(this._errorsAll);
            }
        }

        internal int GlobalErrorCount
        {
            get
            {
                return ErrorsHelper.GetErrorCount(this._errorsGlobal);
            }
        }

        private bool HasAllErrors
        {
            get
            {
                return ErrorsHelper.GetHasErrors(this._errorsAll);
            }
        }

        internal bool HasGlobalErrors
        {
            get
            {
                return ErrorsHelper.GetHasErrors(this._errorsGlobal);
            }
        }

        internal bool HasLocalErrors
        {
            get
            {
                return ErrorsHelper.GetHasErrors(this._errorsLocal);
            }
        }
    }
}

