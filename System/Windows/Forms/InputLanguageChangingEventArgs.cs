namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class InputLanguageChangingEventArgs : CancelEventArgs
    {
        private readonly CultureInfo culture;
        private readonly System.Windows.Forms.InputLanguage inputLanguage;
        private readonly bool sysCharSet;

        public InputLanguageChangingEventArgs(CultureInfo culture, bool sysCharSet)
        {
            this.inputLanguage = System.Windows.Forms.InputLanguage.FromCulture(culture);
            this.culture = culture;
            this.sysCharSet = sysCharSet;
        }

        public InputLanguageChangingEventArgs(System.Windows.Forms.InputLanguage inputLanguage, bool sysCharSet)
        {
            if (inputLanguage == null)
            {
                throw new ArgumentNullException("inputLanguage");
            }
            this.inputLanguage = inputLanguage;
            this.culture = inputLanguage.Culture;
            this.sysCharSet = sysCharSet;
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

        public bool SysCharSet
        {
            get
            {
                return this.sysCharSet;
            }
        }
    }
}

