namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal class MaskDescriptorTemplate : MaskDescriptor
    {
        private CultureInfo culture;
        private string mask;
        private string name;
        private string sample;
        private Type type;

        public MaskDescriptorTemplate(string mask, string name, string sample, Type validatingType, CultureInfo culture) : this(mask, name, sample, validatingType, culture, false)
        {
        }

        public MaskDescriptorTemplate(string mask, string name, string sample, Type validatingType, CultureInfo culture, bool skipValidation)
        {
            string str;
            this.mask = mask;
            this.name = name;
            this.sample = sample;
            this.type = validatingType;
            this.culture = culture;
            if (!skipValidation && !MaskDescriptor.IsValidMaskDescriptor(this, out str))
            {
                this.mask = null;
            }
        }

        public static List<MaskDescriptor> GetLocalizedMaskDescriptors(CultureInfo culture)
        {
            ValidMaskDescriptorList list = new ValidMaskDescriptorList();
            switch (culture.Parent.Name)
            {
                case "en":
                    break;

                case "ar":
                    list.Add(new MaskDescriptorTemplate("(999)000-0000", "Phone Number", "0123456789", null, culture));
                    list.Add(new MaskDescriptorTemplate("000-0000", "Phone Number no Area Code", "1234567", null, culture));
                    list.Add(new MaskDescriptorTemplate("00 /00 /0000", "Short Date", "26102005", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00 /00 /0000 00:00", "Short Date/Time", "261020051430", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("000-00-0000", "Social Security Number", "123456789", null, culture));
                    list.Add(new MaskDescriptorTemplate("90:00", "Time", " 230", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00:00", "Time (24 Hour)", "1430", typeof(DateTime), culture));
                    goto Label_0D60;

                case "de":
                    list.Add(new MaskDescriptorTemplate("00/00/0000", "Datum kurz", "28112005", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00/00/0000 00:00", "Datum lang", "281120051430", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("90:00", "Zeit", "1430", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00000", "Postleitzahl", "91450", null, culture));
                    goto Label_0D60;

                case "fr":
                {
                    string sample = (culture.Name == "fr-CA") ? "11282005" : "28112005";
                    list.Add(new MaskDescriptorTemplate("99999", "Num\x00e9rique (5 chiffres)", "12345", typeof(int), culture));
                    list.Add(new MaskDescriptorTemplate("00 00 00 00 00 00", "Num\x00e9ro de t\x00e9l\x00e9phone (France)", "0123456789", null, culture));
                    list.Add(new MaskDescriptorTemplate("00/00/0000", "Date (format court)", sample, typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00/00/0000 00:00", "Date et heure (format long)", sample + "1430", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0 00 00 00 000 000 00", "Num\x00e9ro de S\x00e9curit\x00e9 Sociale (France)", "163117801234548", null, culture));
                    list.Add(new MaskDescriptorTemplate("00:00", "Heure", "1430", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00000", "Code postal (France)", "91450", null, culture));
                    goto Label_0D60;
                }
                case "it":
                    list.Add(new MaskDescriptorTemplate("99999", "Numerico (5 Cifre)", "12345", typeof(int), culture));
                    list.Add(new MaskDescriptorTemplate("0000 00000", "Numero di telefono", "012345678", null, culture));
                    list.Add(new MaskDescriptorTemplate("000 0000000", "Numero di cellulare", "1234567890", null, culture));
                    list.Add(new MaskDescriptorTemplate("00/00/0000", "Data breve", "26102005", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00/00/0000 00:00", "Data e ora", "261020051430", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00:00", "Ora", "1430", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00000", "Codice postale", "12345", null, culture));
                    goto Label_0D60;

                case "es":
                    list.Add(new MaskDescriptorTemplate("99999", "Num\x00e9rico", "12345", typeof(int), culture));
                    list.Add(new MaskDescriptorTemplate("(999)000-0000", "N\x00famero de tel\x00e9fono", "0123456789", null, culture));
                    list.Add(new MaskDescriptorTemplate("000-000-0000", "N\x00famero de tel\x00e9fono m\x00f3vil", "0001234567", null, culture));
                    list.Add(new MaskDescriptorTemplate("000-0000", "N\x00famero de tel\x00e9fono sin c\x00f3digo de \x00e1rea", "1234567", null, culture));
                    list.Add(new MaskDescriptorTemplate("00/00/0000", "Fecha", "26102005", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00/00/0000 00:00", "Fecha y hora", "261020051430", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("000-00-0000", "N\x00famero del seguro social", "123456789", null, culture));
                    list.Add(new MaskDescriptorTemplate("00:00", "Hora", "0830", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00000", "C\x00f3digo postal", "12345", null, culture));
                    goto Label_0D60;

                case "ja":
                    list.Add(new MaskDescriptorTemplate("99999", "数値（５桁）", "12345", typeof(int), culture));
                    list.Add(new MaskDescriptorTemplate("99900-9990-0000", "電話番号", "  012- 345-6789", null, culture));
                    list.Add(new MaskDescriptorTemplate("000-0000-0000", "携帯電話番号", "00001234567", null, culture));
                    list.Add(new MaskDescriptorTemplate("0000/00/00", "日付（西暦）", "20050620", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000/00/00 00:00:00", "日付と時間（西暦）", "2005/06/11 04:33:22", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("90:00", "時間", " 633", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("000-0000", "郵便番号", "1820021", null, culture));
                    list.Add(new MaskDescriptorTemplate("0000年90月90日", "日付（西暦、日本語）", "2005年 6月11日", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00/00/00", "日付（和暦）", "170611", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("AA90年90月90日", "日付（和暦、日本語）", "平成17年 6月11日", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000年90月90日 90時90分", "日付と時間（日本語）", "2005年 6月11日  3時33分", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("00/00/00 00:00:00", "日付と時間（和暦）", "170611043322", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("AA00年90月90日 90時90分", "日付と時間（和暦、日本語）", "平成17年 6月11日  3時33分", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("90時90分", "時間（日本語）", " 633", typeof(DateTime), culture));
                    goto Label_0D60;

                case "zh-CHS":
                case "zh-Hans":
                    list.Add(new MaskDescriptorTemplate("99999", "数字(最长5位)", "12345", typeof(int), culture));
                    list.Add(new MaskDescriptorTemplate("(900)9000-0000", "（区号）电话号码", " 1234567890", null, culture));
                    list.Add(new MaskDescriptorTemplate("9000-0000", "电话号码", "12345678", null, culture));
                    list.Add(new MaskDescriptorTemplate("000-0000-0000", "移动电话号码", "12345678901", null, culture));
                    list.Add(new MaskDescriptorTemplate("0000-00-00", "短日期格式", "20050611", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000年90月90日", "长日期格式", "20051211", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000-00-00 90:00:00", "短日期时间", "2005-06-11  6:30:22", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000年90月90日 90时00分", "长日期时间", "2005年 6月11日  6时33分", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("000000-000000-000", "15位身份证号码", "123456789012345", null, culture));
                    list.Add(new MaskDescriptorTemplate("000000-00000000-000A", "18位身份证号码", "123456789012345678", null, culture));
                    list.Add(new MaskDescriptorTemplate("90:00", "时间格式", " 633", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("90时90分", "中文时间格式", " 6时33分", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("000000", "邮政编码", "100080", null, culture));
                    goto Label_0D60;

                case "zh-CHT":
                case "zh-Hant":
                    list.Add(new MaskDescriptorTemplate("(00)9000-0000", "電話號碼", "01 2345678", null, culture));
                    list.Add(new MaskDescriptorTemplate("0000-000-000", "行動電話號碼", "1234567890", null, culture));
                    list.Add(new MaskDescriptorTemplate("0000/00/00", "西曆簡短日期", "20050620", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000年90月90日", "西曆完整日期", "2005年10月 2日", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000/00/00 00:00:00", "西曆簡短日期時間", "20050611043322", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000年90月90日 90時90分", "西曆完整日期時間", "2005年 6月 2日  6時22分", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("L000000000", "身分證字號", "A123456789", null, culture));
                    list.Add(new MaskDescriptorTemplate("90:00", "時間格式", " 633", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("90時90分", "中文時間格式", " 6時 3分", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("99000", "3+2郵遞區號", "80407", null, culture));
                    goto Label_0D60;

                case "ko":
                    list.Add(new MaskDescriptorTemplate("99999", "숫자(5자리)", "12345", typeof(int), culture));
                    list.Add(new MaskDescriptorTemplate("(999)9000-0000", "전화 번호", "01234567890", null, culture));
                    list.Add(new MaskDescriptorTemplate("000-9000-0000", "휴대폰 번호", "01012345678", null, culture));
                    list.Add(new MaskDescriptorTemplate("9000-0000", "지역 번호를 제외한 전화 번호", "12345678", null, culture));
                    list.Add(new MaskDescriptorTemplate("0000-00-00", "간단한 날짜", "20050620", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000-00-00 90:00", "간단한 날짜 및 시간", "2005-06-20  9:20", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000년90월90일 90시90분", "자세한 날짜 및 시간", "2005년 6월20일  6시33분", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("000000-0000000", "주민 등록 번호", "1234561234567", null, culture));
                    list.Add(new MaskDescriptorTemplate("90:00", "시간", " 633", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("000-000", "우편 번호", "182021", null, culture));
                    list.Add(new MaskDescriptorTemplate("90시90분", "자세한 시간", " 6시33분", typeof(DateTime), culture));
                    list.Add(new MaskDescriptorTemplate("0000년 90월 90일", "자세한 날짜", "20050620", typeof(DateTime), culture));
                    goto Label_0D60;

                default:
                    culture = CultureInfo.InvariantCulture;
                    break;
            }
            list.Add(new MaskDescriptorTemplate("00000", "Numeric (5-digits)", "12345", typeof(int), culture));
            list.Add(new MaskDescriptorTemplate("(999) 000-0000", "Phone number", "5745550123", null, culture));
            list.Add(new MaskDescriptorTemplate("000-0000", "Phone number no area code", "5550123", null, culture));
            list.Add(new MaskDescriptorTemplate("00/00/0000", "Short date", "12112003", typeof(DateTime), culture));
            list.Add(new MaskDescriptorTemplate("00/00/0000 90:00", "Short date and time (US)", "121120031120", typeof(DateTime), culture));
            list.Add(new MaskDescriptorTemplate("000-00-0000", "Social security number", "000001234", null, culture));
            list.Add(new MaskDescriptorTemplate("90:00", "Time (US)", "1120", typeof(DateTime), culture));
            list.Add(new MaskDescriptorTemplate("00:00", "Time (European/Military)", "2320", typeof(DateTime), culture));
            list.Add(new MaskDescriptorTemplate("00000-9999", "Zip Code", "980526399", null, culture));
        Label_0D60:
            return list.List;
        }

        public override CultureInfo Culture
        {
            get
            {
                return this.culture;
            }
        }

        public override string Mask
        {
            get
            {
                return this.mask;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override string Sample
        {
            get
            {
                return this.sample;
            }
        }

        public override Type ValidatingType
        {
            get
            {
                return this.type;
            }
        }

        private class ValidMaskDescriptorList
        {
            private List<MaskDescriptor> dx = new List<MaskDescriptor>();

            public void Add(MaskDescriptorTemplate mdt)
            {
                if (mdt.Mask != null)
                {
                    this.dx.Add(mdt);
                }
            }

            public List<MaskDescriptor> List
            {
                get
                {
                    return this.dx;
                }
            }
        }
    }
}

