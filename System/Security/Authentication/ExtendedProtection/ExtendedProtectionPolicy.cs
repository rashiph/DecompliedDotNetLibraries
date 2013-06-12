namespace System.Security.Authentication.ExtendedProtection
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, TypeConverter(typeof(ExtendedProtectionPolicyTypeConverter))]
    public class ExtendedProtectionPolicy : ISerializable
    {
        private ChannelBinding customChannelBinding;
        private const string customChannelBindingName = "customChannelBinding";
        private ServiceNameCollection customServiceNames;
        private const string customServiceNamesName = "customServiceNames";
        private System.Security.Authentication.ExtendedProtection.PolicyEnforcement policyEnforcement;
        private const string policyEnforcementName = "policyEnforcement";
        private System.Security.Authentication.ExtendedProtection.ProtectionScenario protectionScenario;
        private const string protectionScenarioName = "protectionScenario";

        public ExtendedProtectionPolicy(System.Security.Authentication.ExtendedProtection.PolicyEnforcement policyEnforcement)
        {
            this.policyEnforcement = policyEnforcement;
            this.protectionScenario = System.Security.Authentication.ExtendedProtection.ProtectionScenario.TransportSelected;
        }

        protected ExtendedProtectionPolicy(SerializationInfo info, StreamingContext context)
        {
            this.policyEnforcement = (System.Security.Authentication.ExtendedProtection.PolicyEnforcement) info.GetInt32("policyEnforcement");
            this.protectionScenario = (System.Security.Authentication.ExtendedProtection.ProtectionScenario) info.GetInt32("protectionScenario");
            this.customServiceNames = (ServiceNameCollection) info.GetValue("customServiceNames", typeof(ServiceNameCollection));
            byte[] source = (byte[]) info.GetValue("customChannelBinding", typeof(byte[]));
            if (source != null)
            {
                this.customChannelBinding = SafeLocalFreeChannelBinding.LocalAlloc(source.Length);
                Marshal.Copy(source, 0, this.customChannelBinding.DangerousGetHandle(), source.Length);
            }
        }

        public ExtendedProtectionPolicy(System.Security.Authentication.ExtendedProtection.PolicyEnforcement policyEnforcement, ChannelBinding customChannelBinding)
        {
            if (policyEnforcement == System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never)
            {
                throw new ArgumentException(SR.GetString("security_ExtendedProtectionPolicy_UseDifferentConstructorForNever"), "policyEnforcement");
            }
            if (customChannelBinding == null)
            {
                throw new ArgumentNullException("customChannelBinding");
            }
            this.policyEnforcement = policyEnforcement;
            this.protectionScenario = System.Security.Authentication.ExtendedProtection.ProtectionScenario.TransportSelected;
            this.customChannelBinding = customChannelBinding;
        }

        public ExtendedProtectionPolicy(System.Security.Authentication.ExtendedProtection.PolicyEnforcement policyEnforcement, System.Security.Authentication.ExtendedProtection.ProtectionScenario protectionScenario, ICollection customServiceNames) : this(policyEnforcement, protectionScenario, (customServiceNames == null) ? null : new ServiceNameCollection(customServiceNames))
        {
        }

        public ExtendedProtectionPolicy(System.Security.Authentication.ExtendedProtection.PolicyEnforcement policyEnforcement, System.Security.Authentication.ExtendedProtection.ProtectionScenario protectionScenario, ServiceNameCollection customServiceNames)
        {
            if (policyEnforcement == System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never)
            {
                throw new ArgumentException(SR.GetString("security_ExtendedProtectionPolicy_UseDifferentConstructorForNever"), "policyEnforcement");
            }
            if ((customServiceNames != null) && (customServiceNames.Count == 0))
            {
                throw new ArgumentException(SR.GetString("security_ExtendedProtectionPolicy_NoEmptyServiceNameCollection"), "customServiceNames");
            }
            this.policyEnforcement = policyEnforcement;
            this.protectionScenario = protectionScenario;
            this.customServiceNames = customServiceNames;
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("policyEnforcement", (int) this.policyEnforcement);
            info.AddValue("protectionScenario", (int) this.protectionScenario);
            info.AddValue("customServiceNames", this.customServiceNames, typeof(ServiceNameCollection));
            if (this.customChannelBinding == null)
            {
                info.AddValue("customChannelBinding", null, typeof(byte[]));
            }
            else
            {
                byte[] destination = new byte[this.customChannelBinding.Size];
                Marshal.Copy(this.customChannelBinding.DangerousGetHandle(), destination, 0, this.customChannelBinding.Size);
                info.AddValue("customChannelBinding", destination, typeof(byte[]));
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("ProtectionScenario=");
            builder.Append(this.protectionScenario.ToString());
            builder.Append("; PolicyEnforcement=");
            builder.Append(this.policyEnforcement.ToString());
            builder.Append("; CustomChannelBinding=");
            if (this.customChannelBinding == null)
            {
                builder.Append("<null>");
            }
            else
            {
                builder.Append(this.customChannelBinding.ToString());
            }
            builder.Append("; ServiceNames=");
            if (this.customServiceNames == null)
            {
                builder.Append("<null>");
            }
            else
            {
                bool flag = true;
                foreach (string str in this.customServiceNames)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }
                    builder.Append(str);
                }
            }
            return builder.ToString();
        }

        public ChannelBinding CustomChannelBinding
        {
            get
            {
                return this.customChannelBinding;
            }
        }

        public ServiceNameCollection CustomServiceNames
        {
            get
            {
                return this.customServiceNames;
            }
        }

        public static bool OSSupportsExtendedProtection
        {
            get
            {
                return AuthenticationManager.OSSupportsExtendedProtection;
            }
        }

        public System.Security.Authentication.ExtendedProtection.PolicyEnforcement PolicyEnforcement
        {
            get
            {
                return this.policyEnforcement;
            }
        }

        public System.Security.Authentication.ExtendedProtection.ProtectionScenario ProtectionScenario
        {
            get
            {
                return this.protectionScenario;
            }
        }
    }
}

