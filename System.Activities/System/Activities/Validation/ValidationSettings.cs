namespace System.Activities.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ValidationSettings
    {
        private IDictionary<Type, IList<Constraint>> additionalConstraints;

        public IDictionary<Type, IList<Constraint>> AdditionalConstraints
        {
            get
            {
                if (this.additionalConstraints == null)
                {
                    this.additionalConstraints = new Dictionary<Type, IList<Constraint>>();
                }
                return this.additionalConstraints;
            }
        }

        internal bool HasAdditionalConstraints
        {
            get
            {
                return ((this.additionalConstraints != null) && (this.additionalConstraints.Count > 0));
            }
        }

        public bool OnlyUseAdditionalConstraints { get; set; }

        public bool SingleLevel { get; set; }
    }
}

