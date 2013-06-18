namespace System.Activities.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class ValidationResults
    {
        private ReadOnlyCollection<ValidationError> allValidationErrors;
        private ReadOnlyCollection<ValidationError> errors;
        private bool processedAllValidationErrors;
        private ReadOnlyCollection<ValidationError> warnings;

        internal ValidationResults(IList<ValidationError> allValidationErrors)
        {
            if (allValidationErrors == null)
            {
                this.allValidationErrors = ActivityValidationServices.EmptyValidationErrors;
            }
            else
            {
                this.allValidationErrors = new ReadOnlyCollection<ValidationError>(allValidationErrors);
            }
        }

        private void ProcessAllValidationErrors()
        {
            if (this.allValidationErrors.Count == 0)
            {
                this.errors = ActivityValidationServices.EmptyValidationErrors;
                this.warnings = ActivityValidationServices.EmptyValidationErrors;
            }
            else
            {
                IList<ValidationError> list = null;
                IList<ValidationError> list2 = null;
                for (int i = 0; i < this.allValidationErrors.Count; i++)
                {
                    ValidationError item = this.allValidationErrors[i];
                    if (item.IsWarning)
                    {
                        if (list == null)
                        {
                            list = new Collection<ValidationError>();
                        }
                        list.Add(item);
                    }
                    else
                    {
                        if (list2 == null)
                        {
                            list2 = new Collection<ValidationError>();
                        }
                        list2.Add(item);
                    }
                }
                if (list == null)
                {
                    this.warnings = ActivityValidationServices.EmptyValidationErrors;
                }
                else
                {
                    this.warnings = new ReadOnlyCollection<ValidationError>(list);
                }
                if (list2 == null)
                {
                    this.errors = ActivityValidationServices.EmptyValidationErrors;
                }
                else
                {
                    this.errors = new ReadOnlyCollection<ValidationError>(list2);
                }
            }
            this.processedAllValidationErrors = true;
        }

        public ReadOnlyCollection<ValidationError> Errors
        {
            get
            {
                if (!this.processedAllValidationErrors)
                {
                    this.ProcessAllValidationErrors();
                }
                return this.errors;
            }
        }

        public ReadOnlyCollection<ValidationError> Warnings
        {
            get
            {
                if (!this.processedAllValidationErrors)
                {
                    this.ProcessAllValidationErrors();
                }
                return this.warnings;
            }
        }
    }
}

