namespace System.Net
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    public interface ICertificatePolicy
    {
        bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem);
    }
}

