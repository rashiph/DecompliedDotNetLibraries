namespace System.ServiceModel
{
    using System;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security;

    public abstract class MessageSecurityVersion
    {
        internal MessageSecurityVersion()
        {
        }

        public abstract System.ServiceModel.Security.BasicSecurityProfileVersion BasicSecurityProfileVersion { get; }

        public static MessageSecurityVersion Default
        {
            get
            {
                return WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11MessageSecurityVersion.Instance;
            }
        }

        internal abstract System.ServiceModel.Security.MessageSecurityTokenVersion MessageSecurityTokenVersion { get; }

        public System.ServiceModel.Security.SecureConversationVersion SecureConversationVersion
        {
            get
            {
                return this.MessageSecurityTokenVersion.SecureConversationVersion;
            }
        }

        public abstract System.ServiceModel.Security.SecurityPolicyVersion SecurityPolicyVersion { get; }

        public System.IdentityModel.Selectors.SecurityTokenVersion SecurityTokenVersion
        {
            get
            {
                return this.MessageSecurityTokenVersion;
            }
        }

        public System.ServiceModel.Security.SecurityVersion SecurityVersion
        {
            get
            {
                return this.MessageSecurityTokenVersion.SecurityVersion;
            }
        }

        public System.ServiceModel.Security.TrustVersion TrustVersion
        {
            get
            {
                return this.MessageSecurityTokenVersion.TrustVersion;
            }
        }

        public static MessageSecurityVersion WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10
        {
            get
            {
                return WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10MessageSecurityVersion.Instance;
            }
        }

        public static MessageSecurityVersion WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10
        {
            get
            {
                return WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10MessageSecurityVersion.Instance;
            }
        }

        public static MessageSecurityVersion WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12
        {
            get
            {
                return WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12MessageSecurityVersion.Instance;
            }
        }

        public static MessageSecurityVersion WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10
        {
            get
            {
                return WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10MessageSecurityVersion.Instance;
            }
        }

        public static MessageSecurityVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11
        {
            get
            {
                return WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11MessageSecurityVersion.Instance;
            }
        }

        public static MessageSecurityVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10
        {
            get
            {
                return WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10MessageSecurityVersion.Instance;
            }
        }

        internal static MessageSecurityVersion WSSXDefault
        {
            get
            {
                return WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12MessageSecurityVersion.Instance;
            }
        }

        private class WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10MessageSecurityVersion : MessageSecurityVersion
        {
            private static MessageSecurityVersion instance = new MessageSecurityVersion.WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10MessageSecurityVersion();

            public override string ToString()
            {
                return "WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10";
            }

            public override System.ServiceModel.Security.BasicSecurityProfileVersion BasicSecurityProfileVersion
            {
                get
                {
                    return null;
                }
            }

            public static MessageSecurityVersion Instance
            {
                get
                {
                    return instance;
                }
            }

            internal override System.ServiceModel.Security.MessageSecurityTokenVersion MessageSecurityTokenVersion
            {
                get
                {
                    return System.ServiceModel.Security.MessageSecurityTokenVersion.WSSecurity10WSTrust13WSSecureConversation13BasicSecurityProfile10;
                }
            }

            public override System.ServiceModel.Security.SecurityPolicyVersion SecurityPolicyVersion
            {
                get
                {
                    return System.ServiceModel.Security.SecurityPolicyVersion.WSSecurityPolicy12;
                }
            }
        }

        private class WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10MessageSecurityVersion : MessageSecurityVersion
        {
            private static MessageSecurityVersion instance = new MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10MessageSecurityVersion();

            public override string ToString()
            {
                return "WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10";
            }

            public override System.ServiceModel.Security.BasicSecurityProfileVersion BasicSecurityProfileVersion
            {
                get
                {
                    return System.ServiceModel.Security.BasicSecurityProfileVersion.BasicSecurityProfile10;
                }
            }

            public static MessageSecurityVersion Instance
            {
                get
                {
                    return instance;
                }
            }

            internal override System.ServiceModel.Security.MessageSecurityTokenVersion MessageSecurityTokenVersion
            {
                get
                {
                    return System.ServiceModel.Security.MessageSecurityTokenVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10;
                }
            }

            public override System.ServiceModel.Security.SecurityPolicyVersion SecurityPolicyVersion
            {
                get
                {
                    return System.ServiceModel.Security.SecurityPolicyVersion.WSSecurityPolicy11;
                }
            }
        }

        private class WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10MessageSecurityVersion : MessageSecurityVersion
        {
            private static MessageSecurityVersion instance = new MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10MessageSecurityVersion();

            public override string ToString()
            {
                return "WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10";
            }

            public override System.ServiceModel.Security.BasicSecurityProfileVersion BasicSecurityProfileVersion
            {
                get
                {
                    return null;
                }
            }

            public static MessageSecurityVersion Instance
            {
                get
                {
                    return instance;
                }
            }

            internal override System.ServiceModel.Security.MessageSecurityTokenVersion MessageSecurityTokenVersion
            {
                get
                {
                    return System.ServiceModel.Security.MessageSecurityTokenVersion.WSSecurity11WSTrust13WSSecureConversation13BasicSecurityProfile10;
                }
            }

            public override System.ServiceModel.Security.SecurityPolicyVersion SecurityPolicyVersion
            {
                get
                {
                    return System.ServiceModel.Security.SecurityPolicyVersion.WSSecurityPolicy12;
                }
            }
        }

        private class WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12MessageSecurityVersion : MessageSecurityVersion
        {
            private static MessageSecurityVersion instance = new MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12MessageSecurityVersion();

            public override string ToString()
            {
                return "WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12";
            }

            public override System.ServiceModel.Security.BasicSecurityProfileVersion BasicSecurityProfileVersion
            {
                get
                {
                    return null;
                }
            }

            public static MessageSecurityVersion Instance
            {
                get
                {
                    return instance;
                }
            }

            internal override System.ServiceModel.Security.MessageSecurityTokenVersion MessageSecurityTokenVersion
            {
                get
                {
                    return System.ServiceModel.Security.MessageSecurityTokenVersion.WSSecurity11WSTrust13WSSecureConversation13;
                }
            }

            public override System.ServiceModel.Security.SecurityPolicyVersion SecurityPolicyVersion
            {
                get
                {
                    return System.ServiceModel.Security.SecurityPolicyVersion.WSSecurityPolicy12;
                }
            }
        }

        private class WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10MessageSecurityVersion : MessageSecurityVersion
        {
            private static MessageSecurityVersion instance = new MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10MessageSecurityVersion();

            public override string ToString()
            {
                return "WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10";
            }

            public override System.ServiceModel.Security.BasicSecurityProfileVersion BasicSecurityProfileVersion
            {
                get
                {
                    return System.ServiceModel.Security.BasicSecurityProfileVersion.BasicSecurityProfile10;
                }
            }

            public static MessageSecurityVersion Instance
            {
                get
                {
                    return instance;
                }
            }

            internal override System.ServiceModel.Security.MessageSecurityTokenVersion MessageSecurityTokenVersion
            {
                get
                {
                    return System.ServiceModel.Security.MessageSecurityTokenVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10;
                }
            }

            public override System.ServiceModel.Security.SecurityPolicyVersion SecurityPolicyVersion
            {
                get
                {
                    return System.ServiceModel.Security.SecurityPolicyVersion.WSSecurityPolicy11;
                }
            }
        }

        private class WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11MessageSecurityVersion : MessageSecurityVersion
        {
            private static MessageSecurityVersion instance = new MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11MessageSecurityVersion();

            public override string ToString()
            {
                return "WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11";
            }

            public override System.ServiceModel.Security.BasicSecurityProfileVersion BasicSecurityProfileVersion
            {
                get
                {
                    return null;
                }
            }

            public static MessageSecurityVersion Instance
            {
                get
                {
                    return instance;
                }
            }

            internal override System.ServiceModel.Security.MessageSecurityTokenVersion MessageSecurityTokenVersion
            {
                get
                {
                    return System.ServiceModel.Security.MessageSecurityTokenVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005;
                }
            }

            public override System.ServiceModel.Security.SecurityPolicyVersion SecurityPolicyVersion
            {
                get
                {
                    return System.ServiceModel.Security.SecurityPolicyVersion.WSSecurityPolicy11;
                }
            }
        }
    }
}

