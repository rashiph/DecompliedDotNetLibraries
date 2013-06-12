namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public sealed class X509BasicConstraintsExtension : X509Extension
    {
        private bool m_decoded;
        private bool m_hasPathLenConstraint;
        private bool m_isCA;
        private int m_pathLenConstraint;

        public X509BasicConstraintsExtension() : base("2.5.29.19")
        {
            this.m_decoded = true;
        }

        public X509BasicConstraintsExtension(AsnEncodedData encodedBasicConstraints, bool critical) : base("2.5.29.19", encodedBasicConstraints.RawData, critical)
        {
        }

        public X509BasicConstraintsExtension(bool certificateAuthority, bool hasPathLengthConstraint, int pathLengthConstraint, bool critical) : base("2.5.29.19", EncodeExtension(certificateAuthority, hasPathLengthConstraint, pathLengthConstraint), critical)
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            base.CopyFrom(asnEncodedData);
            this.m_decoded = false;
        }

        private void DecodeExtension()
        {
            uint cbDecodedValue = 0;
            SafeLocalAllocHandle decodedValue = null;
            if (base.Oid.Value == "2.5.29.10")
            {
                if (!CAPI.DecodeObject(new IntPtr(13L), base.m_rawData, out decodedValue, out cbDecodedValue))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                CAPIBase.CERT_BASIC_CONSTRAINTS_INFO cert_basic_constraints_info = (CAPIBase.CERT_BASIC_CONSTRAINTS_INFO) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CERT_BASIC_CONSTRAINTS_INFO));
                byte[] destination = new byte[1];
                Marshal.Copy(cert_basic_constraints_info.SubjectType.pbData, destination, 0, 1);
                this.m_isCA = (destination[0] & 0x80) != 0;
                this.m_hasPathLenConstraint = cert_basic_constraints_info.fPathLenConstraint;
                this.m_pathLenConstraint = (int) cert_basic_constraints_info.dwPathLenConstraint;
            }
            else
            {
                if (!CAPI.DecodeObject(new IntPtr(15L), base.m_rawData, out decodedValue, out cbDecodedValue))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                CAPIBase.CERT_BASIC_CONSTRAINTS2_INFO cert_basic_constraints_info2 = (CAPIBase.CERT_BASIC_CONSTRAINTS2_INFO) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CERT_BASIC_CONSTRAINTS2_INFO));
                this.m_isCA = cert_basic_constraints_info2.fCA != 0;
                this.m_hasPathLenConstraint = cert_basic_constraints_info2.fPathLenConstraint != 0;
                this.m_pathLenConstraint = (int) cert_basic_constraints_info2.dwPathLenConstraint;
            }
            this.m_decoded = true;
            decodedValue.Dispose();
        }

        private static unsafe byte[] EncodeExtension(bool certificateAuthority, bool hasPathLengthConstraint, int pathLengthConstraint)
        {
            CAPIBase.CERT_BASIC_CONSTRAINTS2_INFO cert_basic_constraints_info = new CAPIBase.CERT_BASIC_CONSTRAINTS2_INFO {
                fCA = certificateAuthority ? 1 : 0,
                fPathLenConstraint = hasPathLengthConstraint ? 1 : 0
            };
            if (hasPathLengthConstraint)
            {
                if (pathLengthConstraint < 0)
                {
                    throw new ArgumentOutOfRangeException("pathLengthConstraint", SR.GetString("Arg_OutOfRange_NeedNonNegNum"));
                }
                cert_basic_constraints_info.dwPathLenConstraint = (uint) pathLengthConstraint;
            }
            byte[] encodedData = null;
            if (!CAPI.EncodeObject("2.5.29.19", new IntPtr((void*) &cert_basic_constraints_info), out encodedData))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return encodedData;
        }

        public bool CertificateAuthority
        {
            get
            {
                if (!this.m_decoded)
                {
                    this.DecodeExtension();
                }
                return this.m_isCA;
            }
        }

        public bool HasPathLengthConstraint
        {
            get
            {
                if (!this.m_decoded)
                {
                    this.DecodeExtension();
                }
                return this.m_hasPathLenConstraint;
            }
        }

        public int PathLengthConstraint
        {
            get
            {
                if (!this.m_decoded)
                {
                    this.DecodeExtension();
                }
                return this.m_pathLenConstraint;
            }
        }
    }
}

