namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class FaultReason
    {
        private SynchronizedReadOnlyCollection<FaultReasonText> translations;

        public FaultReason(FaultReasonText translation)
        {
            if (translation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("translation");
            }
            this.Init(translation);
        }

        public FaultReason(IEnumerable<FaultReasonText> translations)
        {
            if (translations == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("translations"));
            }
            int num = 0;
            using (IEnumerator<FaultReasonText> enumerator = translations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    FaultReasonText current = enumerator.Current;
                    num++;
                }
            }
            if (num == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("AtLeastOneFaultReasonMustBeSpecified"), "translations"));
            }
            FaultReasonText[] textArray = new FaultReasonText[num];
            int num2 = 0;
            foreach (FaultReasonText text in translations)
            {
                if (text == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("translations", System.ServiceModel.SR.GetString("NoNullTranslations"));
                }
                textArray[num2++] = text;
            }
            this.Init(textArray);
        }

        public FaultReason(string text)
        {
            this.Init(new FaultReasonText(text));
        }

        internal FaultReason(string text, CultureInfo cultureInfo)
        {
            this.Init(new FaultReasonText(text, cultureInfo));
        }

        internal FaultReason(string text, string xmlLang)
        {
            this.Init(new FaultReasonText(text, xmlLang));
        }

        public FaultReasonText GetMatchingTranslation()
        {
            return this.GetMatchingTranslation(CultureInfo.CurrentCulture);
        }

        public FaultReasonText GetMatchingTranslation(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("cultureInfo"));
            }
            if (this.translations.Count == 1)
            {
                return this.translations[0];
            }
            for (int i = 0; i < this.translations.Count; i++)
            {
                if (this.translations[i].Matches(cultureInfo))
                {
                    return this.translations[i];
                }
            }
            if (this.translations.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("NoMatchingTranslationFoundForFaultText")));
            }
            string name = cultureInfo.Name;
            while (true)
            {
                int length = name.LastIndexOf('-');
                if (length == -1)
                {
                    return this.translations[0];
                }
                name = name.Substring(0, length);
                for (int j = 0; j < this.translations.Count; j++)
                {
                    if (this.translations[j].XmlLang == name)
                    {
                        return this.translations[j];
                    }
                }
            }
        }

        private void Init(FaultReasonText translation)
        {
            this.Init(new FaultReasonText[] { translation });
        }

        private void Init(FaultReasonText[] translations)
        {
            this.translations = new SynchronizedReadOnlyCollection<FaultReasonText>(new object(), Array.AsReadOnly<FaultReasonText>(translations));
        }

        public override string ToString()
        {
            if (this.translations.Count == 0)
            {
                return string.Empty;
            }
            return this.GetMatchingTranslation().Text;
        }

        public SynchronizedReadOnlyCollection<FaultReasonText> Translations
        {
            get
            {
                return this.translations;
            }
        }
    }
}

