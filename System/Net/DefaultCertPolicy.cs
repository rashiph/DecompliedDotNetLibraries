namespace System.Net
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    internal class DefaultCertPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest request, int problem)
        {
            return (problem == 0);
        }
    }
}

