namespace System.Activities.Validation
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public class ValidationError
    {
        public ValidationError(string message) : this(message, false, string.Empty)
        {
        }

        internal ValidationError(string message, Activity activity) : this(message, false, string.Empty, activity)
        {
        }

        public ValidationError(string message, bool isWarning) : this(message, isWarning, string.Empty)
        {
        }

        internal ValidationError(string message, bool isWarning, Activity activity) : this(message, isWarning, string.Empty, activity)
        {
        }

        public ValidationError(string message, bool isWarning, string propertyName) : this(message, isWarning, propertyName, null)
        {
        }

        internal ValidationError(string message, bool isWarning, string propertyName, Activity activity)
        {
            this.Message = message;
            this.IsWarning = isWarning;
            this.PropertyName = propertyName;
            if (activity != null)
            {
                this.Source = activity;
                this.Id = activity.Id;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "ValidationError {{ Message = {0}, Source = {1}, PropertyName = {2}, IsWarning = {3} }}", new object[] { this.Message, this.Source, this.PropertyName, this.IsWarning });
        }

        public string Id { get; internal set; }

        public bool IsWarning { get; private set; }

        public string Message { get; internal set; }

        public string PropertyName { get; private set; }

        public Activity Source { get; internal set; }
    }
}

