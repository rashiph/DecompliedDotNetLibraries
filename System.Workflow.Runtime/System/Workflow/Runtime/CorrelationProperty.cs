namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    [Serializable]
    public class CorrelationProperty
    {
        private string name;
        private object value;

        public CorrelationProperty(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.name = name;
            this.value = value;
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        public object Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
        }
    }
}

