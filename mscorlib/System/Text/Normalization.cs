namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security;

    internal class Normalization
    {
        private const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        private const int ERROR_INVALID_PARAMETER = 0x57;
        private const int ERROR_NO_UNICODE_TRANSLATION = 0x459;
        private const int ERROR_NOT_ENOUGH_MEMORY = 8;
        private const int ERROR_SUCCESS = 0;
        private static Normalization IDNA;
        private static Normalization IDNADisallowUnassigned;
        private static Normalization NFC;
        private static Normalization NFCDisallowUnassigned;
        private static Normalization NFD;
        private static Normalization NFDDisallowUnassigned;
        private static Normalization NFKC;
        private static Normalization NFKCDisallowUnassigned;
        private static Normalization NFKD;
        private static Normalization NFKDDisallowUnassigned;
        private NormalizationForm normalizationForm;

        [SecurityCritical]
        internal unsafe Normalization(NormalizationForm form, string strDataFile)
        {
            this.normalizationForm = form;
            if (!nativeLoadNormalizationDLL())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNormalizationForm"));
            }
            byte* globalizationResourceBytePtr = GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(Normalization).Assembly, strDataFile);
            if (globalizationResourceBytePtr == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNormalizationForm"));
            }
            if (nativeNormalizationInitNormalization(form, globalizationResourceBytePtr) == null)
            {
                throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));
            }
        }

        [SecurityCritical]
        internal static Normalization GetFormC()
        {
            if (NFC == null)
            {
                NFC = new Normalization(NormalizationForm.FormC, "normnfc.nlp");
            }
            return NFC;
        }

        [SecurityCritical]
        internal static Normalization GetFormCDisallowUnassigned()
        {
            if (NFCDisallowUnassigned == null)
            {
                NFCDisallowUnassigned = new Normalization((NormalizationForm) 0x101, "normnfc.nlp");
            }
            return NFCDisallowUnassigned;
        }

        [SecurityCritical]
        internal static Normalization GetFormD()
        {
            if (NFD == null)
            {
                NFD = new Normalization(NormalizationForm.FormD, "normnfd.nlp");
            }
            return NFD;
        }

        [SecurityCritical]
        internal static Normalization GetFormDDisallowUnassigned()
        {
            if (NFDDisallowUnassigned == null)
            {
                NFDDisallowUnassigned = new Normalization((NormalizationForm) 0x102, "normnfd.nlp");
            }
            return NFDDisallowUnassigned;
        }

        [SecurityCritical]
        internal static Normalization GetFormIDNA()
        {
            if (IDNA == null)
            {
                IDNA = new Normalization((NormalizationForm) 13, "normidna.nlp");
            }
            return IDNA;
        }

        [SecurityCritical]
        internal static Normalization GetFormIDNADisallowUnassigned()
        {
            if (IDNADisallowUnassigned == null)
            {
                IDNADisallowUnassigned = new Normalization((NormalizationForm) 0x10d, "normidna.nlp");
            }
            return IDNADisallowUnassigned;
        }

        [SecurityCritical]
        internal static Normalization GetFormKC()
        {
            if (NFKC == null)
            {
                NFKC = new Normalization(NormalizationForm.FormKC, "normnfkc.nlp");
            }
            return NFKC;
        }

        [SecurityCritical]
        internal static Normalization GetFormKCDisallowUnassigned()
        {
            if (NFKCDisallowUnassigned == null)
            {
                NFKCDisallowUnassigned = new Normalization((NormalizationForm) 0x105, "normnfkc.nlp");
            }
            return NFKCDisallowUnassigned;
        }

        [SecurityCritical]
        internal static Normalization GetFormKD()
        {
            if (NFKD == null)
            {
                NFKD = new Normalization(NormalizationForm.FormKD, "normnfkd.nlp");
            }
            return NFKD;
        }

        [SecurityCritical]
        internal static Normalization GetFormKDDisallowUnassigned()
        {
            if (NFKDDisallowUnassigned == null)
            {
                NFKDDisallowUnassigned = new Normalization((NormalizationForm) 0x106, "normnfkd.nlp");
            }
            return NFKDDisallowUnassigned;
        }

        [SecurityCritical]
        internal static Normalization GetNormalization(NormalizationForm form)
        {
            switch (((ExtendedNormalizationForms) form))
            {
                case ExtendedNormalizationForms.FormC:
                    return GetFormC();

                case ExtendedNormalizationForms.FormD:
                    return GetFormD();

                case ExtendedNormalizationForms.FormKC:
                    return GetFormKC();

                case ExtendedNormalizationForms.FormKD:
                    return GetFormKD();

                case ExtendedNormalizationForms.FormIdna:
                    return GetFormIDNA();

                case ExtendedNormalizationForms.FormCDisallowUnassigned:
                    return GetFormCDisallowUnassigned();

                case ExtendedNormalizationForms.FormDDisallowUnassigned:
                    return GetFormDDisallowUnassigned();

                case ExtendedNormalizationForms.FormKCDisallowUnassigned:
                    return GetFormKCDisallowUnassigned();

                case ExtendedNormalizationForms.FormKDDisallowUnassigned:
                    return GetFormKDDisallowUnassigned();

                case ExtendedNormalizationForms.FormIdnaDisallowUnassigned:
                    return GetFormIDNADisallowUnassigned();
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNormalizationForm"));
        }

        [SecurityCritical]
        internal int GuessLength(string strInput)
        {
            if (strInput == null)
            {
                throw new ArgumentNullException("strInput", Environment.GetResourceString("ArgumentNull_String"));
            }
            int iError = 0;
            int num2 = nativeNormalizationNormalizeString(this.normalizationForm, ref iError, strInput, strInput.Length, null, 0);
            switch (iError)
            {
                case 0:
                    return num2;

                case 8:
                    throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));
            }
            throw new InvalidOperationException(Environment.GetRuntimeResourceString("UnknownError_Num", new object[] { iError }));
        }

        [SecurityCritical]
        private bool IsNormalized(string strInput)
        {
            if (strInput == null)
            {
                throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"), "strInput");
            }
            int iError = 0;
            int num2 = nativeNormalizationIsNormalizedString(this.normalizationForm, ref iError, strInput, strInput.Length);
            switch (iError)
            {
                case 0:
                    return ((num2 & 1) == 1);

                case 8:
                    throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));

                case 0x459:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"), "strInput");
            }
            throw new InvalidOperationException(Environment.GetRuntimeResourceString("UnknownError_Num", new object[] { iError }));
        }

        [SecurityCritical]
        internal static bool IsNormalized(string strInput, NormalizationForm normForm)
        {
            return GetNormalization(normForm).IsNormalized(strInput);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool nativeLoadNormalizationDLL();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe byte* nativeNormalizationInitNormalization(NormalizationForm NormForm, byte* pTableData);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int nativeNormalizationIsNormalizedString(NormalizationForm NormForm, ref int iError, string lpString, int cwLength);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int nativeNormalizationNormalizeString(NormalizationForm NormForm, ref int iError, string lpSrcString, int cwSrcLength, char[] lpDstString, int cwDstLength);
        [SecurityCritical]
        internal string Normalize(string strInput)
        {
            if (strInput == null)
            {
                throw new ArgumentNullException("strInput", Environment.GetResourceString("ArgumentNull_String"));
            }
            int length = this.GuessLength(strInput);
            if (length == 0)
            {
                return string.Empty;
            }
            char[] lpDstString = null;
            int iError = 0x7a;
            while (iError == 0x7a)
            {
                lpDstString = new char[length];
                length = nativeNormalizationNormalizeString(this.normalizationForm, ref iError, strInput, strInput.Length, lpDstString, lpDstString.Length);
                if (iError != 0)
                {
                    switch (iError)
                    {
                        case 8:
                            throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));

                        case 0x7a:
                        {
                            continue;
                        }
                        case 0x459:
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequence", new object[] { length }), "strInput");
                    }
                    throw new InvalidOperationException(Environment.GetRuntimeResourceString("UnknownError_Num", new object[] { iError }));
                }
            }
            return new string(lpDstString, 0, length);
        }

        [SecurityCritical]
        internal static string Normalize(string strInput, NormalizationForm normForm)
        {
            return GetNormalization(normForm).Normalize(strInput);
        }
    }
}

