namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class CultureInfoConverter : TypeConverter
    {
        private TypeConverter.StandardValuesCollection values;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string strA = (string) value;
            if (this.GetCultureName(CultureInfo.InvariantCulture).Equals(""))
            {
                strA = CultureInfoMapper.GetCultureInfoName((string) value);
            }
            CultureInfo invariantCulture = null;
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
            if ((culture != null) && culture.Equals(CultureInfo.InvariantCulture))
            {
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            try
            {
                if (((strA == null) || (strA.Length == 0)) || (string.Compare(strA, this.DefaultCultureString, StringComparison.Ordinal) == 0))
                {
                    invariantCulture = CultureInfo.InvariantCulture;
                }
                if (invariantCulture == null)
                {
                    IEnumerator enumerator = ((IEnumerable) this.GetStandardValues(context)).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        CultureInfo current = (CultureInfo) enumerator.Current;
                        if ((current != null) && (string.Compare(this.GetCultureName(current), strA, StringComparison.Ordinal) == 0))
                        {
                            invariantCulture = current;
                            break;
                        }
                    }
                }
                if (invariantCulture == null)
                {
                    try
                    {
                        invariantCulture = new CultureInfo(strA);
                    }
                    catch
                    {
                    }
                }
                if (invariantCulture == null)
                {
                    strA = strA.ToLower(CultureInfo.CurrentCulture);
                    IEnumerator enumerator2 = this.values.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        CultureInfo info4 = (CultureInfo) enumerator2.Current;
                        if ((info4 != null) && this.GetCultureName(info4).ToLower(CultureInfo.CurrentCulture).StartsWith(strA))
                        {
                            invariantCulture = info4;
                            goto Label_0138;
                        }
                    }
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
            }
        Label_0138:
            if (invariantCulture == null)
            {
                throw new ArgumentException(SR.GetString("CultureInfoConverterInvalidCulture", new object[] { (string) value }));
            }
            return invariantCulture;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == typeof(string))
            {
                string cultureName;
                CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
                if ((culture != null) && culture.Equals(CultureInfo.InvariantCulture))
                {
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
                try
                {
                    if ((value == null) || (value == CultureInfo.InvariantCulture))
                    {
                        return this.DefaultCultureString;
                    }
                    cultureName = this.GetCultureName((CultureInfo) value);
                }
                finally
                {
                    Thread.CurrentThread.CurrentUICulture = currentUICulture;
                }
                return cultureName;
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is CultureInfo))
            {
                CultureInfo info2 = (CultureInfo) value;
                ConstructorInfo constructor = typeof(CultureInfo).GetConstructor(new Type[] { typeof(string) });
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, new object[] { info2.Name });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        protected virtual string GetCultureName(CultureInfo culture)
        {
            return culture.Name;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                CultureInfo[] infoArray2;
                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures);
                int index = Array.IndexOf<CultureInfo>(cultures, CultureInfo.InvariantCulture);
                if (index != -1)
                {
                    cultures[index] = null;
                    infoArray2 = new CultureInfo[cultures.Length];
                }
                else
                {
                    infoArray2 = new CultureInfo[cultures.Length + 1];
                }
                Array.Copy(cultures, infoArray2, cultures.Length);
                Array.Sort(infoArray2, new CultureComparer(this));
                if (infoArray2[0] == null)
                {
                    infoArray2[0] = CultureInfo.InvariantCulture;
                }
                this.values = new TypeConverter.StandardValuesCollection(infoArray2);
            }
            return this.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private string DefaultCultureString
        {
            get
            {
                return SR.GetString("CultureInfoConverterDefaultCultureString");
            }
        }

        private class CultureComparer : IComparer
        {
            private CultureInfoConverter converter;

            public CultureComparer(CultureInfoConverter cultureConverter)
            {
                this.converter = cultureConverter;
            }

            public int Compare(object item1, object item2)
            {
                if (item1 == null)
                {
                    if (item2 == null)
                    {
                        return 0;
                    }
                    return -1;
                }
                if (item2 == null)
                {
                    return 1;
                }
                string cultureName = this.converter.GetCultureName((CultureInfo) item1);
                string str2 = this.converter.GetCultureName((CultureInfo) item2);
                return CultureInfo.CurrentCulture.CompareInfo.Compare(cultureName, str2, CompareOptions.StringSort);
            }
        }

        private static class CultureInfoMapper
        {
            private static Dictionary<string, string> cultureInfoNameMap;

            public static string GetCultureInfoName(string cultureInfoDisplayName)
            {
                if (cultureInfoNameMap == null)
                {
                    InitializeCultureInfoMap();
                }
                if (cultureInfoNameMap.ContainsKey(cultureInfoDisplayName))
                {
                    return cultureInfoNameMap[cultureInfoDisplayName];
                }
                return cultureInfoDisplayName;
            }

            private static void InitializeCultureInfoMap()
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("Afrikaans", "af");
                dictionary.Add("Afrikaans (South Africa)", "af-ZA");
                dictionary.Add("Albanian", "sq");
                dictionary.Add("Albanian (Albania)", "sq-AL");
                dictionary.Add("Alsatian (France)", "gsw-FR");
                dictionary.Add("Amharic (Ethiopia)", "am-ET");
                dictionary.Add("Arabic", "ar");
                dictionary.Add("Arabic (Algeria)", "ar-DZ");
                dictionary.Add("Arabic (Bahrain)", "ar-BH");
                dictionary.Add("Arabic (Egypt)", "ar-EG");
                dictionary.Add("Arabic (Iraq)", "ar-IQ");
                dictionary.Add("Arabic (Jordan)", "ar-JO");
                dictionary.Add("Arabic (Kuwait)", "ar-KW");
                dictionary.Add("Arabic (Lebanon)", "ar-LB");
                dictionary.Add("Arabic (Libya)", "ar-LY");
                dictionary.Add("Arabic (Morocco)", "ar-MA");
                dictionary.Add("Arabic (Oman)", "ar-OM");
                dictionary.Add("Arabic (Qatar)", "ar-QA");
                dictionary.Add("Arabic (Saudi Arabia)", "ar-SA");
                dictionary.Add("Arabic (Syria)", "ar-SY");
                dictionary.Add("Arabic (Tunisia)", "ar-TN");
                dictionary.Add("Arabic (U.A.E.)", "ar-AE");
                dictionary.Add("Arabic (Yemen)", "ar-YE");
                dictionary.Add("Armenian", "hy");
                dictionary.Add("Armenian (Armenia)", "hy-AM");
                dictionary.Add("Assamese (India)", "as-IN");
                dictionary.Add("Azeri", "az");
                dictionary.Add("Azeri (Cyrillic, Azerbaijan)", "az-Cyrl-AZ");
                dictionary.Add("Azeri (Latin, Azerbaijan)", "az-Latn-AZ");
                dictionary.Add("Bashkir (Russia)", "ba-RU");
                dictionary.Add("Basque", "eu");
                dictionary.Add("Basque (Basque)", "eu-ES");
                dictionary.Add("Belarusian", "be");
                dictionary.Add("Belarusian (Belarus)", "be-BY");
                dictionary.Add("Bengali (Bangladesh)", "bn-BD");
                dictionary.Add("Bengali (India)", "bn-IN");
                dictionary.Add("Bosnian (Cyrillic, Bosnia and Herzegovina)", "bs-Cyrl-BA");
                dictionary.Add("Bosnian (Latin, Bosnia and Herzegovina)", "bs-Latn-BA");
                dictionary.Add("Breton (France)", "br-FR");
                dictionary.Add("Bulgarian", "bg");
                dictionary.Add("Bulgarian (Bulgaria)", "bg-BG");
                dictionary.Add("Catalan", "ca");
                dictionary.Add("Catalan (Catalan)", "ca-ES");
                dictionary.Add("Chinese (Hong Kong S.A.R.)", "zh-HK");
                dictionary.Add("Chinese (Macao S.A.R.)", "zh-MO");
                dictionary.Add("Chinese (People's Republic of China)", "zh-CN");
                dictionary.Add("Chinese (Simplified)", "zh-CHS");
                dictionary.Add("Chinese (Singapore)", "zh-SG");
                dictionary.Add("Chinese (Taiwan)", "zh-TW");
                dictionary.Add("Chinese (Traditional)", "zh-CHT");
                dictionary.Add("Corsican (France)", "co-FR");
                dictionary.Add("Croatian", "hr");
                dictionary.Add("Croatian (Croatia)", "hr-HR");
                dictionary.Add("Croatian (Latin, Bosnia and Herzegovina)", "hr-BA");
                dictionary.Add("Czech", "cs");
                dictionary.Add("Czech (Czech Republic)", "cs-CZ");
                dictionary.Add("Danish", "da");
                dictionary.Add("Danish (Denmark)", "da-DK");
                dictionary.Add("Dari (Afghanistan)", "prs-AF");
                dictionary.Add("Divehi", "dv");
                dictionary.Add("Divehi (Maldives)", "dv-MV");
                dictionary.Add("Dutch", "nl");
                dictionary.Add("Dutch (Belgium)", "nl-BE");
                dictionary.Add("Dutch (Netherlands)", "nl-NL");
                dictionary.Add("English", "en");
                dictionary.Add("English (Australia)", "en-AU");
                dictionary.Add("English (Belize)", "en-BZ");
                dictionary.Add("English (Canada)", "en-CA");
                dictionary.Add("English (Caribbean)", "en-029");
                dictionary.Add("English (India)", "en-IN");
                dictionary.Add("English (Ireland)", "en-IE");
                dictionary.Add("English (Jamaica)", "en-JM");
                dictionary.Add("English (Malaysia)", "en-MY");
                dictionary.Add("English (New Zealand)", "en-NZ");
                dictionary.Add("English (Republic of the Philippines)", "en-PH");
                dictionary.Add("English (Singapore)", "en-SG");
                dictionary.Add("English (South Africa)", "en-ZA");
                dictionary.Add("English (Trinidad and Tobago)", "en-TT");
                dictionary.Add("English (United Kingdom)", "en-GB");
                dictionary.Add("English (United States)", "en-US");
                dictionary.Add("English (Zimbabwe)", "en-ZW");
                dictionary.Add("Estonian", "et");
                dictionary.Add("Estonian (Estonia)", "et-EE");
                dictionary.Add("Faroese", "fo");
                dictionary.Add("Faroese (Faroe Islands)", "fo-FO");
                dictionary.Add("Filipino (Philippines)", "fil-PH");
                dictionary.Add("Finnish", "fi");
                dictionary.Add("Finnish (Finland)", "fi-FI");
                dictionary.Add("French", "fr");
                dictionary.Add("French (Belgium)", "fr-BE");
                dictionary.Add("French (Canada)", "fr-CA");
                dictionary.Add("French (France)", "fr-FR");
                dictionary.Add("French (Luxembourg)", "fr-LU");
                dictionary.Add("French (Principality of Monaco)", "fr-MC");
                dictionary.Add("French (Switzerland)", "fr-CH");
                dictionary.Add("Frisian (Netherlands)", "fy-NL");
                dictionary.Add("Galician", "gl");
                dictionary.Add("Galician (Galician)", "gl-ES");
                dictionary.Add("Georgian", "ka");
                dictionary.Add("Georgian (Georgia)", "ka-GE");
                dictionary.Add("German", "de");
                dictionary.Add("German (Austria)", "de-AT");
                dictionary.Add("German (Germany)", "de-DE");
                dictionary.Add("German (Liechtenstein)", "de-LI");
                dictionary.Add("German (Luxembourg)", "de-LU");
                dictionary.Add("German (Switzerland)", "de-CH");
                dictionary.Add("Greek", "el");
                dictionary.Add("Greek (Greece)", "el-GR");
                dictionary.Add("Greenlandic (Greenland)", "kl-GL");
                dictionary.Add("Gujarati", "gu");
                dictionary.Add("Gujarati (India)", "gu-IN");
                dictionary.Add("Hausa (Latin, Nigeria)", "ha-Latn-NG");
                dictionary.Add("Hebrew", "he");
                dictionary.Add("Hebrew (Israel)", "he-IL");
                dictionary.Add("Hindi", "hi");
                dictionary.Add("Hindi (India)", "hi-IN");
                dictionary.Add("Hungarian", "hu");
                dictionary.Add("Hungarian (Hungary)", "hu-HU");
                dictionary.Add("Icelandic", "is");
                dictionary.Add("Icelandic (Iceland)", "is-IS");
                dictionary.Add("Igbo (Nigeria)", "ig-NG");
                dictionary.Add("Indonesian", "id");
                dictionary.Add("Indonesian (Indonesia)", "id-ID");
                dictionary.Add("Inuktitut (Latin, Canada)", "iu-Latn-CA");
                dictionary.Add("Inuktitut (Syllabics, Canada)", "iu-Cans-CA");
                dictionary.Add("Invariant Language (Invariant Country)", "");
                dictionary.Add("Irish (Ireland)", "ga-IE");
                dictionary.Add("isiXhosa (South Africa)", "xh-ZA");
                dictionary.Add("isiZulu (South Africa)", "zu-ZA");
                dictionary.Add("Italian", "it");
                dictionary.Add("Italian (Italy)", "it-IT");
                dictionary.Add("Italian (Switzerland)", "it-CH");
                dictionary.Add("Japanese", "ja");
                dictionary.Add("Japanese (Japan)", "ja-JP");
                dictionary.Add("K'iche (Guatemala)", "qut-GT");
                dictionary.Add("Kannada", "kn");
                dictionary.Add("Kannada (India)", "kn-IN");
                dictionary.Add("Kazakh", "kk");
                dictionary.Add("Kazakh (Kazakhstan)", "kk-KZ");
                dictionary.Add("Khmer (Cambodia)", "km-KH");
                dictionary.Add("Kinyarwanda (Rwanda)", "rw-RW");
                dictionary.Add("Kiswahili", "sw");
                dictionary.Add("Kiswahili (Kenya)", "sw-KE");
                dictionary.Add("Konkani", "kok");
                dictionary.Add("Konkani (India)", "kok-IN");
                dictionary.Add("Korean", "ko");
                dictionary.Add("Korean (Korea)", "ko-KR");
                dictionary.Add("Kyrgyz", "ky");
                dictionary.Add("Kyrgyz (Kyrgyzstan)", "ky-KG");
                dictionary.Add("Lao (Lao P.D.R.)", "lo-LA");
                dictionary.Add("Latvian", "lv");
                dictionary.Add("Latvian (Latvia)", "lv-LV");
                dictionary.Add("Lithuanian", "lt");
                dictionary.Add("Lithuanian (Lithuania)", "lt-LT");
                dictionary.Add("Lower Sorbian (Germany)", "dsb-DE");
                dictionary.Add("Luxembourgish (Luxembourg)", "lb-LU");
                dictionary.Add("Macedonian", "mk");
                dictionary.Add("Macedonian (Former Yugoslav Republic of Macedonia)", "mk-MK");
                dictionary.Add("Malay", "ms");
                dictionary.Add("Malay (Brunei Darussalam)", "ms-BN");
                dictionary.Add("Malay (Malaysia)", "ms-MY");
                dictionary.Add("Malayalam (India)", "ml-IN");
                dictionary.Add("Maltese (Malta)", "mt-MT");
                dictionary.Add("Maori (New Zealand)", "mi-NZ");
                dictionary.Add("Mapudungun (Chile)", "arn-CL");
                dictionary.Add("Marathi", "mr");
                dictionary.Add("Marathi (India)", "mr-IN");
                dictionary.Add("Mohawk (Mohawk)", "moh-CA");
                dictionary.Add("Mongolian", "mn");
                dictionary.Add("Mongolian (Cyrillic, Mongolia)", "mn-MN");
                dictionary.Add("Mongolian (Traditional Mongolian, PRC)", "mn-Mong-CN");
                dictionary.Add("Nepali (Nepal)", "ne-NP");
                dictionary.Add("Norwegian", "no");
                dictionary.Add("Norwegian, Bokm\x00e5l (Norway)", "nb-NO");
                dictionary.Add("Norwegian, Nynorsk (Norway)", "nn-NO");
                dictionary.Add("Occitan (France)", "oc-FR");
                dictionary.Add("Oriya (India)", "or-IN");
                dictionary.Add("Pashto (Afghanistan)", "ps-AF");
                dictionary.Add("Persian", "fa");
                dictionary.Add("Persian (Iran)", "fa-IR");
                dictionary.Add("Polish", "pl");
                dictionary.Add("Polish (Poland)", "pl-PL");
                dictionary.Add("Portuguese", "pt");
                dictionary.Add("Portuguese (Brazil)", "pt-BR");
                dictionary.Add("Portuguese (Portugal)", "pt-PT");
                dictionary.Add("Punjabi", "pa");
                dictionary.Add("Punjabi (India)", "pa-IN");
                dictionary.Add("Quechua (Bolivia)", "quz-BO");
                dictionary.Add("Quechua (Ecuador)", "quz-EC");
                dictionary.Add("Quechua (Peru)", "quz-PE");
                dictionary.Add("Romanian", "ro");
                dictionary.Add("Romanian (Romania)", "ro-RO");
                dictionary.Add("Romansh (Switzerland)", "rm-CH");
                dictionary.Add("Russian", "ru");
                dictionary.Add("Russian (Russia)", "ru-RU");
                dictionary.Add("Sami, Inari (Finland)", "smn-FI");
                dictionary.Add("Sami, Lule (Norway)", "smj-NO");
                dictionary.Add("Sami, Lule (Sweden)", "smj-SE");
                dictionary.Add("Sami, Northern (Finland)", "se-FI");
                dictionary.Add("Sami, Northern (Norway)", "se-NO");
                dictionary.Add("Sami, Northern (Sweden)", "se-SE");
                dictionary.Add("Sami, Skolt (Finland)", "sms-FI");
                dictionary.Add("Sami, Southern (Norway)", "sma-NO");
                dictionary.Add("Sami, Southern (Sweden)", "sma-SE");
                dictionary.Add("Sanskrit", "sa");
                dictionary.Add("Sanskrit (India)", "sa-IN");
                dictionary.Add("Serbian", "sr");
                dictionary.Add("Serbian (Cyrillic, Bosnia and Herzegovina)", "sr-Cyrl-BA");
                dictionary.Add("Serbian (Cyrillic, Serbia)", "sr-Cyrl-CS");
                dictionary.Add("Serbian (Latin, Bosnia and Herzegovina)", "sr-Latn-BA");
                dictionary.Add("Serbian (Latin, Serbia)", "sr-Latn-CS");
                dictionary.Add("Sesotho sa Leboa (South Africa)", "nso-ZA");
                dictionary.Add("Setswana (South Africa)", "tn-ZA");
                dictionary.Add("Sinhala (Sri Lanka)", "si-LK");
                dictionary.Add("Slovak", "sk");
                dictionary.Add("Slovak (Slovakia)", "sk-SK");
                dictionary.Add("Slovenian", "sl");
                dictionary.Add("Slovenian (Slovenia)", "sl-SI");
                dictionary.Add("Spanish", "es");
                dictionary.Add("Spanish (Argentina)", "es-AR");
                dictionary.Add("Spanish (Bolivia)", "es-BO");
                dictionary.Add("Spanish (Chile)", "es-CL");
                dictionary.Add("Spanish (Colombia)", "es-CO");
                dictionary.Add("Spanish (Costa Rica)", "es-CR");
                dictionary.Add("Spanish (Dominican Republic)", "es-DO");
                dictionary.Add("Spanish (Ecuador)", "es-EC");
                dictionary.Add("Spanish (El Salvador)", "es-SV");
                dictionary.Add("Spanish (Guatemala)", "es-GT");
                dictionary.Add("Spanish (Honduras)", "es-HN");
                dictionary.Add("Spanish (Mexico)", "es-MX");
                dictionary.Add("Spanish (Nicaragua)", "es-NI");
                dictionary.Add("Spanish (Panama)", "es-PA");
                dictionary.Add("Spanish (Paraguay)", "es-PY");
                dictionary.Add("Spanish (Peru)", "es-PE");
                dictionary.Add("Spanish (Puerto Rico)", "es-PR");
                dictionary.Add("Spanish (Spain)", "es-ES");
                dictionary.Add("Spanish (United States)", "es-US");
                dictionary.Add("Spanish (Uruguay)", "es-UY");
                dictionary.Add("Spanish (Venezuela)", "es-VE");
                dictionary.Add("Swedish", "sv");
                dictionary.Add("Swedish (Finland)", "sv-FI");
                dictionary.Add("Swedish (Sweden)", "sv-SE");
                dictionary.Add("Syriac", "syr");
                dictionary.Add("Syriac (Syria)", "syr-SY");
                dictionary.Add("Tajik (Cyrillic, Tajikistan)", "tg-Cyrl-TJ");
                dictionary.Add("Tamazight (Latin, Algeria)", "tzm-Latn-DZ");
                dictionary.Add("Tamil", "ta");
                dictionary.Add("Tamil (India)", "ta-IN");
                dictionary.Add("Tatar", "tt");
                dictionary.Add("Tatar (Russia)", "tt-RU");
                dictionary.Add("Telugu", "te");
                dictionary.Add("Telugu (India)", "te-IN");
                dictionary.Add("Thai", "th");
                dictionary.Add("Thai (Thailand)", "th-TH");
                dictionary.Add("Tibetan (PRC)", "bo-CN");
                dictionary.Add("Turkish", "tr");
                dictionary.Add("Turkish (Turkey)", "tr-TR");
                dictionary.Add("Turkmen (Turkmenistan)", "tk-TM");
                dictionary.Add("Uighur (PRC)", "ug-CN");
                dictionary.Add("Ukrainian", "uk");
                dictionary.Add("Ukrainian (Ukraine)", "uk-UA");
                dictionary.Add("Upper Sorbian (Germany)", "hsb-DE");
                dictionary.Add("Urdu", "ur");
                dictionary.Add("Urdu (Islamic Republic of Pakistan)", "ur-PK");
                dictionary.Add("Uzbek", "uz");
                dictionary.Add("Uzbek (Cyrillic, Uzbekistan)", "uz-Cyrl-UZ");
                dictionary.Add("Uzbek (Latin, Uzbekistan)", "uz-Latn-UZ");
                dictionary.Add("Vietnamese", "vi");
                dictionary.Add("Vietnamese (Vietnam)", "vi-VN");
                dictionary.Add("Welsh (United Kingdom)", "cy-GB");
                dictionary.Add("Wolof (Senegal)", "wo-SN");
                dictionary.Add("Yakut (Russia)", "sah-RU");
                dictionary.Add("Yi (PRC)", "ii-CN");
                dictionary.Add("Yoruba (Nigeria)", "yo-NG");
                cultureInfoNameMap = dictionary;
            }
        }
    }
}

