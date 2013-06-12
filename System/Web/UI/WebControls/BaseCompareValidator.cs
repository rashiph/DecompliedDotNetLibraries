namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;

    public abstract class BaseCompareValidator : BaseValidator
    {
        protected BaseCompareValidator()
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            if (base.RenderUplevel)
            {
                ValidationDataType enumValue = this.Type;
                if (enumValue != ValidationDataType.String)
                {
                    string clientID = this.ClientID;
                    HtmlTextWriter writer2 = base.EnableLegacyRendering ? writer : null;
                    base.AddExpandoAttribute(writer2, clientID, "type", PropertyConverter.EnumToString(typeof(ValidationDataType), enumValue), false);
                    NumberFormatInfo currentInfo = NumberFormatInfo.CurrentInfo;
                    switch (enumValue)
                    {
                        case ValidationDataType.Double:
                        {
                            string numberDecimalSeparator = currentInfo.NumberDecimalSeparator;
                            base.AddExpandoAttribute(writer2, clientID, "decimalchar", numberDecimalSeparator);
                            return;
                        }
                        case ValidationDataType.Currency:
                        {
                            string currencyDecimalSeparator = currentInfo.CurrencyDecimalSeparator;
                            base.AddExpandoAttribute(writer2, clientID, "decimalchar", currencyDecimalSeparator);
                            string currencyGroupSeparator = currentInfo.CurrencyGroupSeparator;
                            if (currencyGroupSeparator[0] == '\x00a0')
                            {
                                currencyGroupSeparator = " ";
                            }
                            base.AddExpandoAttribute(writer2, clientID, "groupchar", currencyGroupSeparator);
                            base.AddExpandoAttribute(writer2, clientID, "digits", currentInfo.CurrencyDecimalDigits.ToString(NumberFormatInfo.InvariantInfo), false);
                            int currencyGroupSize = GetCurrencyGroupSize(currentInfo);
                            if (currencyGroupSize > 0)
                            {
                                base.AddExpandoAttribute(writer2, clientID, "groupsize", currencyGroupSize.ToString(NumberFormatInfo.InvariantInfo), false);
                                return;
                            }
                            break;
                        }
                        case ValidationDataType.Date:
                        {
                            base.AddExpandoAttribute(writer2, clientID, "dateorder", GetDateElementOrder(), false);
                            base.AddExpandoAttribute(writer2, clientID, "cutoffyear", CutoffYear.ToString(NumberFormatInfo.InvariantInfo), false);
                            int year = DateTime.Today.Year;
                            base.AddExpandoAttribute(writer2, clientID, "century", (year - (year % 100)).ToString(NumberFormatInfo.InvariantInfo), false);
                            break;
                        }
                    }
                }
            }
        }

        public static bool CanConvert(string text, ValidationDataType type)
        {
            return CanConvert(text, type, false);
        }

        public static bool CanConvert(string text, ValidationDataType type, bool cultureInvariant)
        {
            object obj2 = null;
            return Convert(text, type, cultureInvariant, out obj2);
        }

        protected static bool Compare(string leftText, string rightText, ValidationCompareOperator op, ValidationDataType type)
        {
            return Compare(leftText, false, rightText, false, op, type);
        }

        protected static bool Compare(string leftText, bool cultureInvariantLeftText, string rightText, bool cultureInvariantRightText, ValidationCompareOperator op, ValidationDataType type)
        {
            object obj2;
            int num;
            if (!Convert(leftText, type, cultureInvariantLeftText, out obj2))
            {
                return false;
            }
            if (op != ValidationCompareOperator.DataTypeCheck)
            {
                object obj3;
                if (!Convert(rightText, type, cultureInvariantRightText, out obj3))
                {
                    return true;
                }
                switch (type)
                {
                    case ValidationDataType.String:
                        num = string.Compare((string) obj2, (string) obj3, false, CultureInfo.CurrentCulture);
                        goto Label_00AC;

                    case ValidationDataType.Integer:
                        num = ((int) obj2).CompareTo(obj3);
                        goto Label_00AC;

                    case ValidationDataType.Double:
                        num = ((double) obj2).CompareTo(obj3);
                        goto Label_00AC;

                    case ValidationDataType.Date:
                        num = ((DateTime) obj2).CompareTo(obj3);
                        goto Label_00AC;

                    case ValidationDataType.Currency:
                        num = ((decimal) obj2).CompareTo(obj3);
                        goto Label_00AC;
                }
            }
            return true;
        Label_00AC:
            switch (op)
            {
                case ValidationCompareOperator.Equal:
                    return (num == 0);

                case ValidationCompareOperator.NotEqual:
                    return (num != 0);

                case ValidationCompareOperator.GreaterThan:
                    return (num > 0);

                case ValidationCompareOperator.GreaterThanEqual:
                    return (num >= 0);

                case ValidationCompareOperator.LessThan:
                    return (num < 0);

                case ValidationCompareOperator.LessThanEqual:
                    return (num <= 0);
            }
            return true;
        }

        protected static bool Convert(string text, ValidationDataType type, out object value)
        {
            return Convert(text, type, false, out value);
        }

        protected static bool Convert(string text, ValidationDataType type, bool cultureInvariant, out object value)
        {
            value = null;
            try
            {
                string str;
                string str3;
                switch (type)
                {
                    case ValidationDataType.String:
                        value = text;
                        goto Label_0118;

                    case ValidationDataType.Integer:
                        value = int.Parse(text, CultureInfo.InvariantCulture);
                        goto Label_0118;

                    case ValidationDataType.Double:
                        if (!cultureInvariant)
                        {
                            break;
                        }
                        str = ConvertDouble(text, CultureInfo.InvariantCulture.NumberFormat);
                        goto Label_0065;

                    case ValidationDataType.Date:
                        if (!cultureInvariant)
                        {
                            goto Label_0094;
                        }
                        value = ConvertDate(text, "ymd");
                        goto Label_0118;

                    case ValidationDataType.Currency:
                        if (!cultureInvariant)
                        {
                            goto Label_00EF;
                        }
                        str3 = ConvertCurrency(text, CultureInfo.InvariantCulture.NumberFormat);
                        goto Label_00FB;

                    default:
                        goto Label_0118;
                }
                str = ConvertDouble(text, NumberFormatInfo.CurrentInfo);
            Label_0065:
                if (str != null)
                {
                    value = double.Parse(str, CultureInfo.InvariantCulture);
                }
                goto Label_0118;
            Label_0094:
                if (!(DateTimeFormatInfo.CurrentInfo.Calendar.GetType() == typeof(GregorianCalendar)))
                {
                    value = DateTime.Parse(text, CultureInfo.CurrentCulture);
                }
                else
                {
                    string dateElementOrder = GetDateElementOrder();
                    value = ConvertDate(text, dateElementOrder);
                }
                goto Label_0118;
            Label_00EF:
                str3 = ConvertCurrency(text, NumberFormatInfo.CurrentInfo);
            Label_00FB:
                if (str3 != null)
                {
                    value = decimal.Parse(str3, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                value = null;
            }
        Label_0118:
            return (value != null);
        }

        internal string ConvertCultureInvariantToCurrentCultureFormat(string valueInString, ValidationDataType type)
        {
            object obj2;
            Convert(valueInString, type, true, out obj2);
            if (obj2 is DateTime)
            {
                DateTime time = (DateTime) obj2;
                return time.ToShortDateString();
            }
            return System.Convert.ToString(obj2, CultureInfo.CurrentCulture);
        }

        private static string ConvertCurrency(string text, NumberFormatInfo info)
        {
            string str3;
            string str4;
            string currencyDecimalSeparator = info.CurrencyDecimalSeparator;
            string currencyGroupSeparator = info.CurrencyGroupSeparator;
            int currencyGroupSize = GetCurrencyGroupSize(info);
            if (currencyGroupSize > 0)
            {
                string str5 = currencyGroupSize.ToString(NumberFormatInfo.InvariantInfo);
                str3 = "{1," + str5 + "}";
                str4 = "{" + str5 + "}";
            }
            else
            {
                str3 = str4 = "+";
            }
            if (currencyGroupSeparator[0] == '\x00a0')
            {
                currencyGroupSeparator = " ";
            }
            int currencyDecimalDigits = info.CurrencyDecimalDigits;
            bool flag = currencyDecimalDigits > 0;
            string pattern = @"^\s*([-\+])?((\d" + str3 + @"(\" + currencyGroupSeparator + @"\d" + str4 + @")+)|\d*)" + (flag ? (@"\" + currencyDecimalSeparator + @"?(\d{0," + currencyDecimalDigits.ToString(NumberFormatInfo.InvariantInfo) + "})") : string.Empty) + @"\s*$";
            Match match = Regex.Match(text, pattern);
            if (!match.Success)
            {
                return null;
            }
            if (((match.Groups[2].Length == 0) && flag) && (match.Groups[5].Length == 0))
            {
                return null;
            }
            return (match.Groups[1].Value + match.Groups[2].Value.Replace(currencyGroupSeparator, string.Empty) + ((flag && (match.Groups[5].Length > 0)) ? ("." + match.Groups[5].Value) : string.Empty));
        }

        private static object ConvertDate(string text, string dateElementOrder)
        {
            int num;
            int num2;
            int fullYear;
            string pattern = @"^\s*((\d{4})|(\d{2}))([-/]|\. ?)(\d{1,2})\4(\d{1,2})\.?\s*$";
            Match match = Regex.Match(text, pattern);
            if (match.Success && (match.Groups[2].Success || (dateElementOrder == "ymd")))
            {
                num = int.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
                num2 = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                if (match.Groups[2].Success)
                {
                    fullYear = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                }
                else
                {
                    fullYear = GetFullYear(int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture));
                }
            }
            else
            {
                if (dateElementOrder == "ymd")
                {
                    return null;
                }
                string str2 = @"^\s*(\d{1,2})([-/]|\. ?)(\d{1,2})(?:\s|\2)((\d{4})|(\d{2}))(?:\sг\.|\.)?\s*$";
                match = Regex.Match(text, str2);
                if (!match.Success)
                {
                    return null;
                }
                if (dateElementOrder == "mdy")
                {
                    num = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                    num2 = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                }
                else
                {
                    num = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    num2 = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                }
                if (match.Groups[5].Success)
                {
                    fullYear = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                }
                else
                {
                    fullYear = GetFullYear(int.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture));
                }
            }
            return new DateTime(fullYear, num2, num);
        }

        private static string ConvertDouble(string text, NumberFormatInfo info)
        {
            if (text.Length == 0)
            {
                return "0";
            }
            string numberDecimalSeparator = info.NumberDecimalSeparator;
            string pattern = @"^\s*([-\+])?(\d*)\" + numberDecimalSeparator + @"?(\d*)\s*$";
            Match match = Regex.Match(text, pattern);
            if (!match.Success)
            {
                return null;
            }
            if ((match.Groups[2].Length == 0) && (match.Groups[3].Length == 0))
            {
                return null;
            }
            return (match.Groups[1].Value + ((match.Groups[2].Length > 0) ? match.Groups[2].Value : "0") + ((match.Groups[3].Length > 0) ? ("." + match.Groups[3].Value) : string.Empty));
        }

        internal string ConvertToShortDateString(string text)
        {
            DateTime time;
            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out time))
            {
                text = time.ToShortDateString();
            }
            return text;
        }

        protected override bool DetermineRenderUplevel()
        {
            if ((this.Type == ValidationDataType.Date) && (DateTimeFormatInfo.CurrentInfo.Calendar.GetType() != typeof(GregorianCalendar)))
            {
                return false;
            }
            return base.DetermineRenderUplevel();
        }

        private static int GetCurrencyGroupSize(NumberFormatInfo info)
        {
            int[] currencyGroupSizes = info.CurrencyGroupSizes;
            if ((currencyGroupSizes != null) && (currencyGroupSizes.Length == 1))
            {
                return currencyGroupSizes[0];
            }
            return -1;
        }

        protected static string GetDateElementOrder()
        {
            string shortDatePattern = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            if (shortDatePattern.IndexOf('y') < shortDatePattern.IndexOf('M'))
            {
                return "ymd";
            }
            if (shortDatePattern.IndexOf('M') < shortDatePattern.IndexOf('d'))
            {
                return "mdy";
            }
            return "dmy";
        }

        protected static int GetFullYear(int shortYear)
        {
            return DateTimeFormatInfo.CurrentInfo.Calendar.ToFourDigitYear(shortYear);
        }

        internal bool IsInStandardDateFormat(string date)
        {
            return Regex.Match(date, @"^\s*(\d+)([-/]|\. ?)(\d+)\2(\d+)\s*$").Success;
        }

        [DefaultValue(false), WebSysDescription("BaseCompareValidator_CultureInvariantValues"), WebCategory("Behavior"), Themeable(false)]
        public bool CultureInvariantValues
        {
            get
            {
                object obj2 = this.ViewState["CultureInvariantValues"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["CultureInvariantValues"] = value;
            }
        }

        protected static int CutoffYear
        {
            get
            {
                return DateTimeFormatInfo.CurrentInfo.Calendar.TwoDigitYearMax;
            }
        }

        [WebCategory("Behavior"), DefaultValue(0), WebSysDescription("RangeValidator_Type"), Themeable(false)]
        public ValidationDataType Type
        {
            get
            {
                object obj2 = this.ViewState["Type"];
                if (obj2 != null)
                {
                    return (ValidationDataType) obj2;
                }
                return ValidationDataType.String;
            }
            set
            {
                if ((value < ValidationDataType.String) || (value > ValidationDataType.Currency))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["Type"] = value;
            }
        }
    }
}

