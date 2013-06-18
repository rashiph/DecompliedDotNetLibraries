namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ServiceModel;

    public class ClaimTypeRequirement
    {
        private string claimType;
        internal const bool DefaultIsOptional = false;
        private bool isOptional;

        public ClaimTypeRequirement(string claimType) : this(claimType, false)
        {
        }

        public ClaimTypeRequirement(string claimType, bool isOptional)
        {
            if (claimType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimType");
            }
            if (claimType.Length <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("claimType", System.ServiceModel.SR.GetString("ClaimTypeCannotBeEmpty"));
            }
            this.claimType = claimType;
            this.isOptional = isOptional;
        }

        public string ClaimType
        {
            get
            {
                return this.claimType;
            }
        }

        public bool IsOptional
        {
            get
            {
                return this.isOptional;
            }
        }
    }
}

