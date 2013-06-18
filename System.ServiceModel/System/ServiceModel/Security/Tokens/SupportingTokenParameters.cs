namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.ServiceModel;
    using System.Text;

    public class SupportingTokenParameters
    {
        private Collection<SecurityTokenParameters> endorsing;
        private Collection<SecurityTokenParameters> signed;
        private Collection<SecurityTokenParameters> signedEncrypted;
        private Collection<SecurityTokenParameters> signedEndorsing;

        public SupportingTokenParameters()
        {
            this.signed = new Collection<SecurityTokenParameters>();
            this.signedEncrypted = new Collection<SecurityTokenParameters>();
            this.endorsing = new Collection<SecurityTokenParameters>();
            this.signedEndorsing = new Collection<SecurityTokenParameters>();
        }

        private SupportingTokenParameters(SupportingTokenParameters other)
        {
            this.signed = new Collection<SecurityTokenParameters>();
            this.signedEncrypted = new Collection<SecurityTokenParameters>();
            this.endorsing = new Collection<SecurityTokenParameters>();
            this.signedEndorsing = new Collection<SecurityTokenParameters>();
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            }
            foreach (SecurityTokenParameters parameters in other.signed)
            {
                this.signed.Add(parameters.Clone());
            }
            foreach (SecurityTokenParameters parameters2 in other.signedEncrypted)
            {
                this.signedEncrypted.Add(parameters2.Clone());
            }
            foreach (SecurityTokenParameters parameters3 in other.endorsing)
            {
                this.endorsing.Add(parameters3.Clone());
            }
            foreach (SecurityTokenParameters parameters4 in other.signedEndorsing)
            {
                this.signedEndorsing.Add(parameters4.Clone());
            }
        }

        public SupportingTokenParameters Clone()
        {
            return new SupportingTokenParameters(this);
        }

        internal bool IsEmpty()
        {
            return ((((this.signed.Count == 0) && (this.signedEncrypted.Count == 0)) && (this.endorsing.Count == 0)) && (this.signedEndorsing.Count == 0));
        }

        internal bool IsSetKeyDerivation(bool requireDerivedKeys)
        {
            foreach (SecurityTokenParameters parameters in this.endorsing)
            {
                if (parameters.RequireDerivedKeys != requireDerivedKeys)
                {
                    return false;
                }
            }
            foreach (SecurityTokenParameters parameters2 in this.signedEndorsing)
            {
                if (parameters2.RequireDerivedKeys != requireDerivedKeys)
                {
                    return false;
                }
            }
            return true;
        }

        public void SetKeyDerivation(bool requireDerivedKeys)
        {
            foreach (SecurityTokenParameters parameters in this.endorsing)
            {
                if (parameters.HasAsymmetricKey)
                {
                    parameters.RequireDerivedKeys = false;
                }
                else
                {
                    parameters.RequireDerivedKeys = requireDerivedKeys;
                }
            }
            foreach (SecurityTokenParameters parameters2 in this.signedEndorsing)
            {
                if (parameters2.HasAsymmetricKey)
                {
                    parameters2.RequireDerivedKeys = false;
                }
                else
                {
                    parameters2.RequireDerivedKeys = requireDerivedKeys;
                }
            }
        }

        public override string ToString()
        {
            int num;
            StringBuilder builder = new StringBuilder();
            if (this.endorsing.Count == 0)
            {
                builder.AppendLine("No endorsing tokens.");
            }
            else
            {
                for (num = 0; num < this.endorsing.Count; num++)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Endorsing[{0}]", new object[] { num.ToString(CultureInfo.InvariantCulture) }));
                    builder.AppendLine("  " + this.endorsing[num].ToString().Trim().Replace("\n", "\n  "));
                }
            }
            if (this.signed.Count == 0)
            {
                builder.AppendLine("No signed tokens.");
            }
            else
            {
                for (num = 0; num < this.signed.Count; num++)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Signed[{0}]", new object[] { num.ToString(CultureInfo.InvariantCulture) }));
                    builder.AppendLine("  " + this.signed[num].ToString().Trim().Replace("\n", "\n  "));
                }
            }
            if (this.signedEncrypted.Count == 0)
            {
                builder.AppendLine("No signed encrypted tokens.");
            }
            else
            {
                for (num = 0; num < this.signedEncrypted.Count; num++)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "SignedEncrypted[{0}]", new object[] { num.ToString(CultureInfo.InvariantCulture) }));
                    builder.AppendLine("  " + this.signedEncrypted[num].ToString().Trim().Replace("\n", "\n  "));
                }
            }
            if (this.signedEndorsing.Count == 0)
            {
                builder.AppendLine("No signed endorsing tokens.");
            }
            else
            {
                for (num = 0; num < this.signedEndorsing.Count; num++)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "SignedEndorsing[{0}]", new object[] { num.ToString(CultureInfo.InvariantCulture) }));
                    builder.AppendLine("  " + this.signedEndorsing[num].ToString().Trim().Replace("\n", "\n  "));
                }
            }
            return builder.ToString().Trim();
        }

        public Collection<SecurityTokenParameters> Endorsing
        {
            get
            {
                return this.endorsing;
            }
        }

        public Collection<SecurityTokenParameters> Signed
        {
            get
            {
                return this.signed;
            }
        }

        public Collection<SecurityTokenParameters> SignedEncrypted
        {
            get
            {
                return this.signedEncrypted;
            }
        }

        public Collection<SecurityTokenParameters> SignedEndorsing
        {
            get
            {
                return this.signedEndorsing;
            }
        }
    }
}

