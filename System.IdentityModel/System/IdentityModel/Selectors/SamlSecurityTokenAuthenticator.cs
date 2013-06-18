namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Security.Principal;

    public class SamlSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        private Collection<string> allowedAudienceUris;
        private System.IdentityModel.Selectors.AudienceUriMode audienceUriMode;
        private TimeSpan maxClockSkew;
        private List<SecurityTokenAuthenticator> supportingAuthenticators;

        public SamlSecurityTokenAuthenticator(IList<SecurityTokenAuthenticator> supportingAuthenticators) : this(supportingAuthenticators, TimeSpan.Zero)
        {
        }

        public SamlSecurityTokenAuthenticator(IList<SecurityTokenAuthenticator> supportingAuthenticators, TimeSpan maxClockSkew)
        {
            if (supportingAuthenticators == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("supportingAuthenticators");
            }
            this.supportingAuthenticators = new List<SecurityTokenAuthenticator>(supportingAuthenticators.Count);
            for (int i = 0; i < supportingAuthenticators.Count; i++)
            {
                this.supportingAuthenticators.Add(supportingAuthenticators[i]);
            }
            this.maxClockSkew = maxClockSkew;
            this.audienceUriMode = System.IdentityModel.Selectors.AudienceUriMode.Always;
            this.allowedAudienceUris = new Collection<string>();
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is SamlSecurityToken);
        }

        private bool IsCurrentlyTimeEffective(SamlSecurityToken token)
        {
            if (token.Assertion.Conditions != null)
            {
                return System.IdentityModel.SecurityUtils.IsCurrentlyTimeEffective(token.Assertion.Conditions.NotBefore, token.Assertion.Conditions.NotOnOrAfter, this.maxClockSkew);
            }
            return true;
        }

        public virtual ClaimSet ResolveClaimSet(SecurityKeyIdentifier keyIdentifier)
        {
            RsaKeyIdentifierClause clause;
            EncryptedKeyIdentifierClause clause2;
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            if (keyIdentifier.TryFind<RsaKeyIdentifierClause>(out clause))
            {
                return new DefaultClaimSet(new Claim[] { new Claim(ClaimTypes.Rsa, clause.Rsa, Rights.PossessProperty) });
            }
            if (keyIdentifier.TryFind<EncryptedKeyIdentifierClause>(out clause2))
            {
                return new DefaultClaimSet(new Claim[] { Claim.CreateHashClaim(clause2.GetBuffer()) });
            }
            return null;
        }

        public virtual ClaimSet ResolveClaimSet(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            for (int i = 0; i < this.supportingAuthenticators.Count; i++)
            {
                if (this.supportingAuthenticators[i].CanValidateToken(token))
                {
                    AuthorizationContext context = AuthorizationContext.CreateDefaultAuthorizationContext(this.supportingAuthenticators[i].ValidateToken(token));
                    if (context.ClaimSets.Count > 0)
                    {
                        return context.ClaimSets[0];
                    }
                }
            }
            return null;
        }

        public virtual IIdentity ResolveIdentity(SecurityKeyIdentifier keyIdentifier)
        {
            RsaKeyIdentifierClause clause;
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            if (keyIdentifier.TryFind<RsaKeyIdentifierClause>(out clause))
            {
                return System.IdentityModel.SecurityUtils.CreateIdentity(clause.Rsa.ToXmlString(false), base.GetType().Name);
            }
            return null;
        }

        public virtual IIdentity ResolveIdentity(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            for (int i = 0; i < this.supportingAuthenticators.Count; i++)
            {
                if (this.supportingAuthenticators[i].CanValidateToken(token))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> onlys = this.supportingAuthenticators[i].ValidateToken(token);
                    if ((onlys != null) && (onlys.Count != 0))
                    {
                        for (int j = 0; j < onlys.Count; j++)
                        {
                            IAuthorizationPolicy policy = onlys[j];
                            if (policy is UnconditionalPolicy)
                            {
                                return ((UnconditionalPolicy) policy).PrimaryIdentity;
                            }
                        }
                    }
                }
            }
            return null;
        }

        protected virtual bool ValidateAudienceRestriction(SamlAudienceRestrictionCondition audienceRestrictionCondition)
        {
            for (int i = 0; i < audienceRestrictionCondition.Audiences.Count; i++)
            {
                if (audienceRestrictionCondition.Audiences[i] != null)
                {
                    for (int j = 0; j < this.allowedAudienceUris.Count; j++)
                    {
                        if (StringComparer.Ordinal.Compare(audienceRestrictionCondition.Audiences[i].AbsoluteUri, this.allowedAudienceUris[j]) == 0)
                        {
                            return true;
                        }
                        if (Uri.IsWellFormedUriString(this.allowedAudienceUris[j], UriKind.Absolute))
                        {
                            Uri uri = new Uri(this.allowedAudienceUris[j]);
                            if (audienceRestrictionCondition.Audiences[i].Equals(uri))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            SamlSecurityToken token2 = token as SamlSecurityToken;
            if (token2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SamlTokenAuthenticatorCanOnlyProcessSamlTokens", new object[] { token.GetType().ToString() })));
            }
            if (token2.Assertion.Signature == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SamlTokenMissingSignature")));
            }
            if (!this.IsCurrentlyTimeEffective(token2))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLTokenTimeInvalid", new object[] { DateTime.UtcNow.ToUniversalTime(), token2.ValidFrom.ToString(), token2.ValidTo.ToString() })));
            }
            if (token2.Assertion.SigningToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SamlSigningTokenMissing")));
            }
            ClaimSet issuer = null;
            bool flag = false;
            for (int i = 0; i < this.supportingAuthenticators.Count; i++)
            {
                flag = this.supportingAuthenticators[i].CanValidateToken(token2.Assertion.SigningToken);
                if (flag)
                {
                    break;
                }
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SamlInvalidSigningToken")));
            }
            issuer = this.ResolveClaimSet(token2.Assertion.SigningToken) ?? ClaimSet.Anonymous;
            List<IAuthorizationPolicy> list = new List<IAuthorizationPolicy>();
            for (int j = 0; j < token2.Assertion.Statements.Count; j++)
            {
                list.Add(token2.Assertion.Statements[j].CreatePolicy(issuer, this));
            }
            if ((this.audienceUriMode == System.IdentityModel.Selectors.AudienceUriMode.Always) || ((this.audienceUriMode == System.IdentityModel.Selectors.AudienceUriMode.BearerKeyOnly) && (token2.SecurityKeys.Count < 1)))
            {
                bool flag2 = false;
                if (this.allowedAudienceUris == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAudienceUrisNotFound")));
                }
                for (int k = 0; k < token2.Assertion.Conditions.Conditions.Count; k++)
                {
                    SamlAudienceRestrictionCondition audienceRestrictionCondition = token2.Assertion.Conditions.Conditions[k] as SamlAudienceRestrictionCondition;
                    if (audienceRestrictionCondition != null)
                    {
                        flag2 = true;
                        if (!this.ValidateAudienceRestriction(audienceRestrictionCondition))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAudienceUriValidationFailed")));
                        }
                    }
                }
                if (!flag2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAudienceUriValidationFailed")));
                }
            }
            return list.AsReadOnly();
        }

        public IList<string> AllowedAudienceUris
        {
            get
            {
                return this.allowedAudienceUris;
            }
        }

        public System.IdentityModel.Selectors.AudienceUriMode AudienceUriMode
        {
            get
            {
                return this.audienceUriMode;
            }
            set
            {
                AudienceUriModeValidationHelper.Validate(this.audienceUriMode);
                this.audienceUriMode = value;
            }
        }
    }
}

