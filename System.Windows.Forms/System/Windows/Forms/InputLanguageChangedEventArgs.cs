namespace System.Windows.Forms
{
    using System;
    using System.Globalization;

    public class InputLanguageChangedEventArgs : EventArgs
    {
        private readonly byte charSet;
        private readonly CultureInfo culture;
        private readonly System.Windows.Forms.InputLanguage inputLanguage;

        public InputLanguageChangedEventArgs(CultureInfo culture, byte charSet)
        {
            this.inputLanguage = System.Windows.Forms.InputLanguage.FromCulture(culture);
            this.culture = culture;
            this.charSet = charSet;
        }

        public InputLanguageChangedEventArgs(System.Windows.Forms.InputLanguage inputLanguage, byte charSet)
        {
            this.inputLanguage = inputLanguage;
            this.culture = inputLanguage.Culture;
            this.charSet = charSet;
        }

        public byte CharSet
        {
            get
            {
                return this.charSet;
            }
        }

        public CultureInfo Culture
        {
            get
            {
                return this.culture;
            }
        }

        public System.Windows.Forms.InputLanguage InputLanguage
        {
            get
            {
                return this.inputLanguage;
            }
        }
    }
}

