namespace System.Runtime.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal class DiagnosticTraceSource : TraceSource
    {
        private const string PropagateActivityValue = "propagateActivity";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DiagnosticTraceSource(string name) : base(name)
        {
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "propagateActivity" };
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

