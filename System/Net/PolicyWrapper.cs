namespace System.Net
{
    using System;
    using System.Collections;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    internal class PolicyWrapper
    {
        private ICertificatePolicy fwdPolicy;
        private const uint IgnoreUnmatchedCN = 0x1000;
        private WebRequest request;
        private ServicePoint srvPoint;

        internal PolicyWrapper(ICertificatePolicy policy, ServicePoint sp, WebRequest wr)
        {
            this.fwdPolicy = policy;
            this.srvPoint = sp;
            this.request = wr;
        }

        public bool Accept(X509Certificate Certificate, int CertificateProblem)
        {
            return this.fwdPolicy.CheckValidationResult(this.srvPoint, Certificate, this.request, CertificateProblem);
        }

        internal bool CheckErrors(string hostName, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return this.Accept(certificate, 0);
            }
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != SslPolicyErrors.None)
            {
                return this.Accept(certificate, -2146762491);
            }
            if (((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != SslPolicyErrors.None) || ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != SslPolicyErrors.None))
            {
                bool fatalError = false;
                uint[] numArray = this.GetChainErrors(hostName, chain, ref fatalError);
                if (fatalError)
                {
                    this.Accept(certificate, -2146893052);
                    return false;
                }
                if (numArray.Length == 0)
                {
                    return this.Accept(certificate, 0);
                }
                foreach (uint num in numArray)
                {
                    if (!this.Accept(certificate, (int) num))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private unsafe uint[] GetChainErrors(string hostName, X509Chain chain, ref bool fatalError)
        {
            fatalError = false;
            SafeFreeCertChain chainContext = new SafeFreeCertChain(chain.ChainContext);
            ArrayList list = new ArrayList();
            uint errorCode = 0;
            ChainPolicyParameter cpp = new ChainPolicyParameter {
                cbSize = ChainPolicyParameter.StructSize,
                dwFlags = 0
            };
            SSL_EXTRA_CERT_CHAIN_POLICY_PARA ssl_extra_cert_chain_policy_para = new SSL_EXTRA_CERT_CHAIN_POLICY_PARA(false);
            cpp.pvExtraPolicyPara = &ssl_extra_cert_chain_policy_para;
            fixed (char* str = ((char*) hostName))
            {
                char* chPtr = str;
                if (ServicePointManager.CheckCertificateName)
                {
                    ssl_extra_cert_chain_policy_para.pwszServerName = chPtr;
                }
            Label_006B:
                errorCode = VerifyChainPolicy(chainContext, ref cpp);
                uint num2 = (uint) MapErrorCode(errorCode);
                list.Add(errorCode);
                if (errorCode != 0)
                {
                    if (num2 == 0)
                    {
                        fatalError = true;
                    }
                    else
                    {
                        cpp.dwFlags |= num2;
                        if ((errorCode == 0x800b010f) && ServicePointManager.CheckCertificateName)
                        {
                            ssl_extra_cert_chain_policy_para.fdwChecks = 0x1000;
                        }
                        goto Label_006B;
                    }
                }
            }
            return (uint[]) list.ToArray(typeof(uint));
        }

        private static IgnoreCertProblem MapErrorCode(uint errorCode)
        {
            switch (((CertificateProblem) errorCode))
            {
                case CertificateProblem.CryptNOREVOCATIONCHECK:
                case CertificateProblem.CryptREVOCATIONOFFLINE:
                case CertificateProblem.CertREVOKED:
                case CertificateProblem.CertREVOCATION_FAILURE:
                    return IgnoreCertProblem.all_rev_unknown;

                case CertificateProblem.TrustBASICCONSTRAINTS:
                case CertificateProblem.CertROLE:
                    return IgnoreCertProblem.invalid_basic_constraints;

                case CertificateProblem.CertEXPIRED:
                    return (IgnoreCertProblem.ctl_not_time_valid | IgnoreCertProblem.not_time_valid);

                case CertificateProblem.CertVALIDITYPERIODNESTING:
                    return IgnoreCertProblem.not_time_nested;

                case CertificateProblem.CertPURPOSE:
                case CertificateProblem.CertINVALIDPOLICY:
                    return IgnoreCertProblem.invalid_policy;

                case CertificateProblem.CertUNTRUSTEDROOT:
                case CertificateProblem.CertCHAINING:
                case CertificateProblem.CertUNTRUSTEDCA:
                    return IgnoreCertProblem.allow_unknown_ca;

                case CertificateProblem.CertCN_NO_MATCH:
                case CertificateProblem.CertINVALIDNAME:
                    return IgnoreCertProblem.invalid_name;

                case CertificateProblem.CertWRONG_USAGE:
                    return IgnoreCertProblem.wrong_usage;
            }
            return (IgnoreCertProblem) 0;
        }

        internal static uint VerifyChainPolicy(SafeFreeCertChain chainContext, ref ChainPolicyParameter cpp)
        {
            ChainPolicyStatus ps = new ChainPolicyStatus {
                cbSize = ChainPolicyStatus.StructSize
            };
            UnsafeNclNativeMethods.NativePKI.CertVerifyCertificateChainPolicy((IntPtr) 4L, chainContext, ref cpp, ref ps);
            return ps.dwError;
        }
    }
}

