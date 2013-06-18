namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;

    internal class SecurityTokenParametersEnumerable : IEnumerable<SecurityTokenParameters>, IEnumerable
    {
        private bool clientTokensOnly;
        private SecurityBindingElement sbe;

        public SecurityTokenParametersEnumerable(SecurityBindingElement sbe) : this(sbe, false)
        {
        }

        public SecurityTokenParametersEnumerable(SecurityBindingElement sbe, bool clientTokensOnly)
        {
            if (sbe == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sbe");
            }
            this.sbe = sbe;
            this.clientTokensOnly = clientTokensOnly;
        }

        public IEnumerator<SecurityTokenParameters> GetEnumerator()
        {
            if (this.sbe is SymmetricSecurityBindingElement)
            {
                SymmetricSecurityBindingElement sbe = (SymmetricSecurityBindingElement) this.sbe;
                if ((sbe.ProtectionTokenParameters != null) && (!this.clientTokensOnly || !sbe.ProtectionTokenParameters.HasAsymmetricKey))
                {
                    yield return sbe.ProtectionTokenParameters;
                }
            }
            else if (this.sbe is AsymmetricSecurityBindingElement)
            {
                AsymmetricSecurityBindingElement iteratorVariable1 = (AsymmetricSecurityBindingElement) this.sbe;
                if (iteratorVariable1.InitiatorTokenParameters != null)
                {
                    yield return iteratorVariable1.InitiatorTokenParameters;
                }
                if ((iteratorVariable1.RecipientTokenParameters != null) && !this.clientTokensOnly)
                {
                    yield return iteratorVariable1.RecipientTokenParameters;
                }
            }
            IEnumerator<SecurityTokenParameters> enumerator = this.sbe.EndpointSupportingTokenParameters.Endorsing.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SecurityTokenParameters current = enumerator.Current;
                if (current != null)
                {
                    yield return current;
                }
            }
            foreach (SecurityTokenParameters iteratorVariable3 in this.sbe.EndpointSupportingTokenParameters.SignedEncrypted)
            {
                if (iteratorVariable3 == null)
                {
                    continue;
                }
                yield return iteratorVariable3;
            }
            foreach (SecurityTokenParameters iteratorVariable4 in this.sbe.EndpointSupportingTokenParameters.SignedEndorsing)
            {
                if (iteratorVariable4 == null)
                {
                    continue;
                }
                yield return iteratorVariable4;
            }
            foreach (SecurityTokenParameters iteratorVariable5 in this.sbe.EndpointSupportingTokenParameters.Signed)
            {
                if (iteratorVariable5 == null)
                {
                    continue;
                }
                yield return iteratorVariable5;
            }
            foreach (SupportingTokenParameters iteratorVariable6 in this.sbe.OperationSupportingTokenParameters.Values)
            {
                if (iteratorVariable6 != null)
                {
                    foreach (SecurityTokenParameters iteratorVariable7 in iteratorVariable6.Endorsing)
                    {
                        if (iteratorVariable7 == null)
                        {
                            continue;
                        }
                        yield return iteratorVariable7;
                    }
                    foreach (SecurityTokenParameters iteratorVariable8 in iteratorVariable6.SignedEncrypted)
                    {
                        if (iteratorVariable8 == null)
                        {
                            continue;
                        }
                        yield return iteratorVariable8;
                    }
                    foreach (SecurityTokenParameters iteratorVariable9 in iteratorVariable6.SignedEndorsing)
                    {
                        if (iteratorVariable9 == null)
                        {
                            continue;
                        }
                        yield return iteratorVariable9;
                    }
                    foreach (SecurityTokenParameters iteratorVariable10 in iteratorVariable6.Signed)
                    {
                        if (iteratorVariable10 == null)
                        {
                            continue;
                        }
                        yield return iteratorVariable10;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

    }
}

