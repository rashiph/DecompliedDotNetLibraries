namespace System.Workflow.Runtime.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Runtime;

    public class WorkflowRuntimeServiceElement : ConfigurationElement
    {
        private NameValueCollection _parameters = new NameValueCollection();
        private const string _type = "type";

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            this._parameters.Add(name, value);
            return true;
        }

        public NameValueCollection Parameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._parameters;
            }
        }

        [ConfigurationProperty("type", DefaultValue=null)]
        public string Type
        {
            get
            {
                return (string) base["type"];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base["type"] = value;
            }
        }
    }
}

