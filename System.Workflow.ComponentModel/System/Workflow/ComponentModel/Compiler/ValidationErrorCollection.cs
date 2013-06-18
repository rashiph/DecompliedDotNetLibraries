namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;

    [Serializable]
    public sealed class ValidationErrorCollection : Collection<ValidationError>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ValidationErrorCollection()
        {
        }

        public ValidationErrorCollection(IEnumerable<ValidationError> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.AddRange(value);
        }

        public ValidationErrorCollection(ValidationErrorCollection value)
        {
            this.AddRange(value);
        }

        public void AddRange(IEnumerable<ValidationError> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            foreach (ValidationError error in value)
            {
                base.Add(error);
            }
        }

        protected override void InsertItem(int index, ValidationError item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ValidationError item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        public ValidationError[] ToArray()
        {
            ValidationError[] array = new ValidationError[base.Count];
            base.CopyTo(array, 0);
            return array;
        }

        public bool HasErrors
        {
            get
            {
                if (base.Count > 0)
                {
                    foreach (ValidationError error in this)
                    {
                        if ((error != null) && !error.IsWarning)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool HasWarnings
        {
            get
            {
                if (base.Count > 0)
                {
                    foreach (ValidationError error in this)
                    {
                        if ((error != null) && error.IsWarning)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}

