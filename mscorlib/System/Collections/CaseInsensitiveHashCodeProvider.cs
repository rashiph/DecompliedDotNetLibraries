namespace System.Collections
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, Obsolete("Please use StringComparer instead."), ComVisible(true)]
    public class CaseInsensitiveHashCodeProvider : IHashCodeProvider
    {
        private static CaseInsensitiveHashCodeProvider m_InvariantCaseInsensitiveHashCodeProvider;
        private TextInfo m_text;

        public CaseInsensitiveHashCodeProvider()
        {
            this.m_text = CultureInfo.CurrentCulture.TextInfo;
        }

        public CaseInsensitiveHashCodeProvider(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            this.m_text = culture.TextInfo;
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            string str = obj as string;
            if (str == null)
            {
                return obj.GetHashCode();
            }
            return this.m_text.GetCaseInsensitiveHashCode(str);
        }

        public static CaseInsensitiveHashCodeProvider Default
        {
            get
            {
                return new CaseInsensitiveHashCodeProvider(CultureInfo.CurrentCulture);
            }
        }

        public static CaseInsensitiveHashCodeProvider DefaultInvariant
        {
            get
            {
                if (m_InvariantCaseInsensitiveHashCodeProvider == null)
                {
                    m_InvariantCaseInsensitiveHashCodeProvider = new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture);
                }
                return m_InvariantCaseInsensitiveHashCodeProvider;
            }
        }
    }
}

