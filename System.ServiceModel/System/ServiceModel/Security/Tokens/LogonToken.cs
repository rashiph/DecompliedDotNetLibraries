namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.Security.Cryptography;
    using System.ServiceModel.Security;
    using System.Text;

    internal class LogonToken : IDisposable
    {
        private ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
        private byte[] passwordHash;
        private byte[] salt;
        private string userName;

        public LogonToken(string userName, string password, byte[] salt, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            this.userName = userName;
            this.passwordHash = ComputeHash(password, salt);
            this.salt = salt;
            this.authorizationPolicies = System.IdentityModel.SecurityUtils.CloneAuthorizationPoliciesIfNecessary(authorizationPolicies);
        }

        private static byte[] ComputeHash(string password, byte[] salt)
        {
            if (string.IsNullOrEmpty(password))
            {
                return salt;
            }
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            int length = salt.Length;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte) (bytes[i] ^ salt[i % length]);
            }
            using (HashAlgorithm algorithm = System.ServiceModel.Security.CryptoHelper.NewSha1HashAlgorithm())
            {
                return algorithm.ComputeHash(bytes);
            }
        }

        public void Dispose()
        {
            System.IdentityModel.SecurityUtils.DisposeAuthorizationPoliciesIfNecessary(this.authorizationPolicies);
        }

        public ReadOnlyCollection<IAuthorizationPolicy> GetAuthorizationPolicies()
        {
            return System.IdentityModel.SecurityUtils.CloneAuthorizationPoliciesIfNecessary(this.authorizationPolicies);
        }

        public bool PasswordEquals(string password)
        {
            byte[] b = ComputeHash(password, this.salt);
            return System.ServiceModel.Security.CryptoHelper.IsEqual(this.passwordHash, b);
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
        }
    }
}

