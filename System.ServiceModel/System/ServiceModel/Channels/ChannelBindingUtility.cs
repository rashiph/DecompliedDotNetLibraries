namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Authentication.ExtendedProtection.Configuration;
    using System.ServiceModel;

    internal static class ChannelBindingUtility
    {
        private static ExtendedProtectionPolicy defaultPolicy = disabledPolicy;
        private static ExtendedProtectionPolicy disabledPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);

        public static bool AreEqual(ExtendedProtectionPolicy policy1, ExtendedProtectionPolicy policy2)
        {
            if ((policy1.PolicyEnforcement != PolicyEnforcement.Never) || (policy2.PolicyEnforcement != PolicyEnforcement.Never))
            {
                if (policy1.PolicyEnforcement != policy2.PolicyEnforcement)
                {
                    return false;
                }
                if (policy1.ProtectionScenario != policy2.ProtectionScenario)
                {
                    return false;
                }
                if (policy1.CustomChannelBinding != policy2.CustomChannelBinding)
                {
                    return false;
                }
            }
            return true;
        }

        public static ExtendedProtectionPolicy BuildPolicy(ExtendedProtectionPolicyElement configurationPolicy)
        {
            if (configurationPolicy.ElementInformation.IsPresent)
            {
                return configurationPolicy.BuildPolicy();
            }
            return DefaultPolicy;
        }

        public static void CopyFrom(ExtendedProtectionPolicyElement source, ExtendedProtectionPolicyElement destination)
        {
            destination.PolicyEnforcement = source.PolicyEnforcement;
            destination.ProtectionScenario = source.ProtectionScenario;
            destination.CustomServiceNames.Clear();
            foreach (ServiceNameElement element in source.CustomServiceNames)
            {
                ServiceNameElement element2 = new ServiceNameElement {
                    Name = element.Name
                };
                destination.CustomServiceNames.Add(element2);
            }
        }

        public static ChannelBinding DuplicateToken(ChannelBinding source)
        {
            if (source == null)
            {
                return null;
            }
            return DuplicatedChannelBinding.CreateCopy(source);
        }

        public static ChannelBinding GetToken(SslStream stream)
        {
            return GetToken(stream.TransportContext);
        }

        public static ChannelBinding GetToken(TransportContext context)
        {
            ChannelBinding channelBinding = null;
            if (context != null)
            {
                channelBinding = context.GetChannelBinding(ChannelBindingKind.Endpoint);
            }
            return channelBinding;
        }

        public static void InitializeFrom(ExtendedProtectionPolicy source, ExtendedProtectionPolicyElement destination)
        {
            if (!IsDefaultPolicy(source))
            {
                destination.PolicyEnforcement = source.PolicyEnforcement;
                destination.ProtectionScenario = source.ProtectionScenario;
                destination.CustomServiceNames.Clear();
                if (source.CustomServiceNames != null)
                {
                    foreach (string str in source.CustomServiceNames)
                    {
                        ServiceNameElement element = new ServiceNameElement {
                            Name = str
                        };
                        destination.CustomServiceNames.Add(element);
                    }
                }
            }
        }

        public static bool IsDefaultPolicy(ExtendedProtectionPolicy policy)
        {
            return object.ReferenceEquals(policy, defaultPolicy);
        }

        public static bool IsSubset(ServiceNameCollection primaryList, ServiceNameCollection subset)
        {
            if ((subset == null) || (subset.Count == 0))
            {
                return true;
            }
            if ((primaryList == null) || (primaryList.Count < subset.Count))
            {
                return false;
            }
            return (primaryList.Merge(subset).Count == primaryList.Count);
        }

        public static void TryAddToMessage(ChannelBinding channelBindingToken, Message message, bool messagePropertyOwnsCleanup)
        {
            if (channelBindingToken != null)
            {
                ChannelBindingMessageProperty property = new ChannelBindingMessageProperty(channelBindingToken, messagePropertyOwnsCleanup);
                property.AddTo(message);
                property.Dispose();
            }
        }

        public static ExtendedProtectionPolicy DefaultPolicy
        {
            get
            {
                return defaultPolicy;
            }
        }

        public static ExtendedProtectionPolicy DisabledPolicy
        {
            get
            {
                return disabledPolicy;
            }
        }

        private class DuplicatedChannelBinding : ChannelBinding
        {
            [SecurityCritical]
            private int size;

            private DuplicatedChannelBinding()
            {
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            private void AllocateMemory(int bytesToAllocate)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    base.SetHandle(Marshal.AllocHGlobal(bytesToAllocate));
                }
            }

            [SecuritySafeCritical]
            internal static ChannelBinding CreateCopy(ChannelBinding source)
            {
                if (source.IsInvalid || source.IsClosed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(source.GetType().FullName));
                }
                if (source.Size <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("source.Size", source.Size, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                ChannelBindingUtility.DuplicatedChannelBinding binding = new ChannelBindingUtility.DuplicatedChannelBinding();
                binding.Initialize(source);
                return binding;
            }

            [SecurityCritical]
            private unsafe void Initialize(ChannelBinding source)
            {
                this.AllocateMemory(source.Size);
                byte* numPtr = (byte*) source.DangerousGetHandle().ToPointer();
                byte* numPtr2 = (byte*) this.handle.ToPointer();
                for (int i = 0; i < source.Size; i++)
                {
                    numPtr2[i] = numPtr[i];
                }
                this.size = source.Size;
            }

            protected override bool ReleaseHandle()
            {
                Marshal.FreeHGlobal(base.handle);
                base.SetHandle(IntPtr.Zero);
                return true;
            }

            public override int Size
            {
                [SecuritySafeCritical]
                get
                {
                    return this.size;
                }
            }
        }
    }
}

