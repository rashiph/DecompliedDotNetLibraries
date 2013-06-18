namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;

    [Serializable]
    public sealed class ValidationError
    {
        private int errorNumber;
        private string errorText;
        private bool isWarning;
        private string propertyName;
        private Hashtable userData;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ValidationError(string errorText, int errorNumber) : this(errorText, errorNumber, false, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ValidationError(string errorText, int errorNumber, bool isWarning) : this(errorText, errorNumber, isWarning, null)
        {
        }

        public ValidationError(string errorText, int errorNumber, bool isWarning, string propertyName)
        {
            this.errorText = string.Empty;
            this.errorText = errorText;
            this.errorNumber = errorNumber;
            this.isWarning = isWarning;
            this.propertyName = propertyName;
        }

        public static ValidationError GetNotSetValidationError(string propertyName)
        {
            return new ValidationError(SR.GetString("Error_PropertyNotSet", new object[] { propertyName }), 0x116) { PropertyName = propertyName };
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}: {2}", new object[] { this.isWarning ? "warning" : "error", this.errorNumber, this.errorText });
        }

        public int ErrorNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errorNumber;
            }
        }

        public string ErrorText
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errorText;
            }
        }

        public bool IsWarning
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isWarning;
            }
        }

        public string PropertyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.propertyName = value;
            }
        }

        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                {
                    this.userData = new Hashtable();
                }
                return this.userData;
            }
        }
    }
}

