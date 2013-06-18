namespace System.ServiceModel.Security
{
    using System;

    public abstract class BasicSecurityProfileVersion
    {
        internal BasicSecurityProfileVersion()
        {
        }

        public static BasicSecurityProfileVersion BasicSecurityProfile10
        {
            get
            {
                return BasicSecurityProfile10BasicSecurityProfileVersion.Instance;
            }
        }

        private class BasicSecurityProfile10BasicSecurityProfileVersion : BasicSecurityProfileVersion
        {
            private static BasicSecurityProfileVersion.BasicSecurityProfile10BasicSecurityProfileVersion instance = new BasicSecurityProfileVersion.BasicSecurityProfile10BasicSecurityProfileVersion();

            public override string ToString()
            {
                return "BasicSecurityProfile10";
            }

            public static BasicSecurityProfileVersion.BasicSecurityProfile10BasicSecurityProfileVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }
    }
}

