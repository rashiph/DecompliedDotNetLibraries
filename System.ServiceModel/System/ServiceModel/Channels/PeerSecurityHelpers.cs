namespace System.ServiceModel.Channels
{
    using System;
    using System.IdentityModel.Claims;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Text;

    internal class PeerSecurityHelpers
    {
        private static void ArrayClear(byte[] buffer)
        {
            if (buffer != null)
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        public static bool Authenticate(Claim claim, string password, byte[] authenticator)
        {
            bool flag = false;
            if (authenticator == null)
            {
                return false;
            }
            byte[] buffer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                buffer = ComputeHash(claim, password);
                if (buffer.Length != authenticator.Length)
                {
                    return flag;
                }
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] != authenticator[i])
                    {
                        flag = false;
                        break;
                    }
                }
                flag = true;
            }
            finally
            {
                ArrayClear(buffer);
            }
            return flag;
        }

        public static bool AuthenticateRequest(Claim claim, string password, Message message)
        {
            return PeerRequestSecurityToken.CreateHashTokenFrom(message).Validate(claim, password);
        }

        public static bool AuthenticateResponse(Claim claim, string password, Message message)
        {
            return PeerRequestSecurityTokenResponse.CreateHashTokenFrom(message).Validate(claim, password);
        }

        public static byte[] ComputeHash(Claim claim, string pwd)
        {
            RSACryptoServiceProvider resource = claim.Resource as RSACryptoServiceProvider;
            if (resource == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }
            using (resource)
            {
                byte[] message = resource.ExportCspBlob(false);
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
                return ComputeHash(message, pwd);
            }
        }

        public static byte[] ComputeHash(X509Certificate2 cert, string pwd)
        {
            RSACryptoServiceProvider key = cert.PublicKey.Key as RSACryptoServiceProvider;
            return ComputeHash(key.ExportCspBlob(false), pwd);
        }

        public static byte[] ComputeHash(byte[] message, string pwd)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            byte[] key = null;
            byte[] sourceArray = null;
            byte[] destinationArray = null;
            try
            {
                key = Encoding.Unicode.GetBytes(pwd.Trim());
                using (HMACSHA256 hmacsha = new HMACSHA256(key))
                {
                    using (SHA256Managed managed = new SHA256Managed())
                    {
                        sourceArray = managed.ComputeHash(key);
                        destinationArray = DiagnosticUtility.Utility.AllocateByteArray(message.Length + sourceArray.Length);
                        Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                        Array.Copy(message, 0, destinationArray, sourceArray.Length, message.Length);
                        return hmacsha.ComputeHash(destinationArray);
                    }
                }
            }
            finally
            {
                ArrayClear(key);
                ArrayClear(sourceArray);
                ArrayClear(destinationArray);
            }
            return null;
        }
    }
}

