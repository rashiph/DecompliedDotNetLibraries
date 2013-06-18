namespace System.IdentityModel
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Security.Authentication.ExtendedProtection;

    internal class ExtendedProtectionPolicyHelper
    {
        private System.Security.Authentication.ExtendedProtection.ChannelBinding _channelBinding;
        private bool _checkServiceBinding;
        private System.Security.Authentication.ExtendedProtection.PolicyEnforcement _policyEnforcement = DefaultPolicy.PolicyEnforcement;
        private System.Security.Authentication.ExtendedProtection.ProtectionScenario _protectionScenario = DefaultPolicy.ProtectionScenario;
        private System.Security.Authentication.ExtendedProtection.ServiceNameCollection _serviceNameCollection;
        private static ExtendedProtectionPolicy disabledPolicy = new ExtendedProtectionPolicy(System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never);

        public ExtendedProtectionPolicyHelper(System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding, ExtendedProtectionPolicy extendedProtectionPolicy)
        {
            this._channelBinding = channelBinding;
            this._serviceNameCollection = null;
            this._checkServiceBinding = true;
            if (extendedProtectionPolicy != null)
            {
                this._policyEnforcement = extendedProtectionPolicy.PolicyEnforcement;
                this._protectionScenario = extendedProtectionPolicy.ProtectionScenario;
                this._serviceNameCollection = extendedProtectionPolicy.CustomServiceNames;
            }
            if (this._policyEnforcement == System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never)
            {
                this._checkServiceBinding = false;
            }
        }

        public void CheckServiceBinding(SafeDeleteContext securityContext, string defaultServiceBinding)
        {
            if (this._policyEnforcement != System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never)
            {
                string specifiedTarget = null;
                int num = SspiWrapper.QuerySpecifiedTarget(securityContext, out specifiedTarget);
                if (num == 0)
                {
                    switch (this._policyEnforcement)
                    {
                        case System.Security.Authentication.ExtendedProtection.PolicyEnforcement.WhenSupported:
                            if (specifiedTarget != null)
                            {
                                break;
                            }
                            return;

                        case System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Always:
                            if (string.IsNullOrEmpty(specifiedTarget))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched", new object[] { string.Empty })));
                            }
                            break;
                    }
                    if ((this._serviceNameCollection == null) || (this._serviceNameCollection.Count < 1))
                    {
                        if (defaultServiceBinding == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched", new object[] { string.Empty })));
                        }
                        if (string.Compare(defaultServiceBinding, specifiedTarget, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(specifiedTarget))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched", new object[] { string.Empty })));
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched", new object[] { specifiedTarget })));
                    }
                    foreach (string str2 in this._serviceNameCollection)
                    {
                        if ((str2 != null) && (string.Compare(str2, specifiedTarget, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            return;
                        }
                    }
                    if (string.IsNullOrEmpty(specifiedTarget))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched", new object[] { string.Empty })));
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched", new object[] { specifiedTarget })));
                }
                if ((num != -2146893053) && (num != -2146893054))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationNoServiceBinding")));
                }
                if (this._policyEnforcement == System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Always)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationNoServiceBinding")));
                }
                if (this._policyEnforcement != System.Security.Authentication.ExtendedProtection.PolicyEnforcement.WhenSupported)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("InvalidServiceBindingInSspiNegotiationNoServiceBinding")));
                }
            }
        }

        public bool ShouldAddChannelBindingToASC()
        {
            return (((this._channelBinding != null) && (this._policyEnforcement != System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never)) && (this._protectionScenario != System.Security.Authentication.ExtendedProtection.ProtectionScenario.TrustedProxy));
        }

        public System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                return this._channelBinding;
            }
        }

        public static ExtendedProtectionPolicy DefaultPolicy
        {
            get
            {
                return disabledPolicy;
            }
        }

        public System.Security.Authentication.ExtendedProtection.PolicyEnforcement PolicyEnforcement
        {
            get
            {
                return this._policyEnforcement;
            }
        }

        public System.Security.Authentication.ExtendedProtection.ProtectionScenario ProtectionScenario
        {
            get
            {
                return this._protectionScenario;
            }
        }

        public System.Security.Authentication.ExtendedProtection.ServiceNameCollection ServiceNameCollection
        {
            get
            {
                return this._serviceNameCollection;
            }
        }

        public bool ShouldCheckServiceBinding
        {
            get
            {
                return this._checkServiceBinding;
            }
        }
    }
}

