namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal class DiagnosticTraceSource : PiiTraceSource
    {
        private const string PropagateActivityValue = "propagateActivity";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DiagnosticTraceSource(string name, string eventSourceName) : base(name, eventSourceName)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DiagnosticTraceSource(string name, string eventSourceName, SourceLevels level) : base(name, eventSourceName, level)
        {
        }

        protected override string[] GetSupportedAttributes()
        {
            string[] supportedAttributes = base.GetSupportedAttributes();
            string[] strArray2 = new string[supportedAttributes.Length + 1];
            for (int i = 0; i < supportedAttributes.Length; i++)
            {
                strArray2[i] = supportedAttributes[i];
            }
            strArray2[supportedAttributes.Length] = "propagateActivity";
            return strArray2;
        }

        internal bool PropagateActivity
        {
            get
            {
                bool result = false;
                string str = base.Attributes["propagateActivity"];
                if (!string.IsNullOrEmpty(str) && !bool.TryParse(str, out result))
                {
                    result = false;
                }
                return result;
            }
            set
            {
                base.Attributes["propagateActivity"] = value.ToString();
            }
        }
    }
}

