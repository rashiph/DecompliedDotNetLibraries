namespace System.Drawing
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class FontConverter : TypeConverter
    {
        private FontNameConverter fontNameConverter;
        private const string styleHdr = "style=";

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
            string str = value as string;
            if (str == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            string text = str.Trim();
            if (text.Length == 0)
            {
                return null;
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }
            char ch = culture.TextInfo.ListSeparator[0];
            string str3 = text;
            string str4 = null;
            string str5 = null;
            float emSize = 8.25f;
            FontStyle regular = FontStyle.Regular;
            GraphicsUnit point = GraphicsUnit.Point;
            int index = text.IndexOf(ch);
            if (index > 0)
            {
                str3 = text.Substring(0, index);
                if (index < (text.Length - 1))
                {
                    int startIndex = text.IndexOf("style=");
                    if (startIndex != -1)
                    {
                        str4 = text.Substring(startIndex, text.Length - startIndex);
                        if (!str4.StartsWith("style="))
                        {
                            throw this.GetFormatException(text, ch);
                        }
                        str5 = text.Substring(index + 1, (startIndex - index) - 1);
                    }
                    else
                    {
                        str5 = text.Substring(index + 1, (text.Length - index) - 1);
                    }
                    string[] strArray = this.ParseSizeTokens(str5, ch);
                    if (strArray[0] != null)
                    {
                        try
                        {
                            emSize = (float) TypeDescriptor.GetConverter(typeof(float)).ConvertFromString(context, culture, strArray[0]);
                        }
                        catch
                        {
                            throw this.GetFormatException(text, ch);
                        }
                    }
                    if (strArray[1] != null)
                    {
                        point = this.ParseGraphicsUnits(strArray[1]);
                    }
                    if (str4 != null)
                    {
                        int num4 = str4.IndexOf("=");
                        foreach (string str6 in str4.Substring(num4 + 1, str4.Length - "style=".Length).Split(new char[] { ch }))
                        {
                            str6 = str6.Trim();
                            try
                            {
                                regular |= (FontStyle) Enum.Parse(typeof(FontStyle), str6, true);
                            }
                            catch (Exception exception)
                            {
                                if (exception is InvalidEnumArgumentException)
                                {
                                    throw;
                                }
                                throw this.GetFormatException(text, ch);
                            }
                            FontStyle style2 = FontStyle.Strikeout | FontStyle.Underline | FontStyle.Italic | FontStyle.Bold;
                            if ((regular | style2) != style2)
                            {
                                throw new InvalidEnumArgumentException("style", (int) regular, typeof(FontStyle));
                            }
                        }
                    }
                }
            }
            if (this.fontNameConverter == null)
            {
                this.fontNameConverter = new FontNameConverter();
            }
            return new Font((string) this.fontNameConverter.ConvertFrom(context, culture, str3), emSize, regular, point);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == typeof(string))
            {
                Font font = value as Font;
                if (font == null)
                {
                    return System.Drawing.SR.GetString("toStringNone");
                }
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }
                string separator = culture.TextInfo.ListSeparator + " ";
                int num = 2;
                if (font.Style != FontStyle.Regular)
                {
                    num++;
                }
                string[] strArray = new string[num];
                int num2 = 0;
                strArray[num2++] = font.Name;
                strArray[num2++] = TypeDescriptor.GetConverter(font.Size).ConvertToString(context, culture, font.Size) + this.GetGraphicsUnitText(font.Unit);
                if (font.Style != FontStyle.Regular)
                {
                    strArray[num2++] = "style=" + font.Style.ToString("G");
                }
                return string.Join(separator, strArray);
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is Font))
            {
                Font font2 = (Font) value;
                int num3 = 2;
                if (font2.GdiVerticalFont)
                {
                    num3 = 6;
                }
                else if (font2.GdiCharSet != 1)
                {
                    num3 = 5;
                }
                else if (font2.Unit != GraphicsUnit.Point)
                {
                    num3 = 4;
                }
                else if (font2.Style != FontStyle.Regular)
                {
                    num3++;
                }
                object[] arguments = new object[num3];
                Type[] types = new Type[num3];
                arguments[0] = font2.Name;
                types[0] = typeof(string);
                arguments[1] = font2.Size;
                types[1] = typeof(float);
                if (num3 > 2)
                {
                    arguments[2] = font2.Style;
                    types[2] = typeof(FontStyle);
                }
                if (num3 > 3)
                {
                    arguments[3] = font2.Unit;
                    types[3] = typeof(GraphicsUnit);
                }
                if (num3 > 4)
                {
                    arguments[4] = font2.GdiCharSet;
                    types[4] = typeof(byte);
                }
                if (num3 > 5)
                {
                    arguments[5] = font2.GdiVerticalFont;
                    types[5] = typeof(bool);
                }
                MemberInfo constructor = typeof(Font).GetConstructor(types);
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, arguments);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null)
            {
                throw new ArgumentNullException("propertyValues");
            }
            object obj2 = propertyValues["Name"];
            object obj3 = propertyValues["Size"];
            object point = propertyValues["Unit"];
            object obj5 = propertyValues["Bold"];
            object obj6 = propertyValues["Italic"];
            object obj7 = propertyValues["Strikeout"];
            object obj8 = propertyValues["Underline"];
            object obj9 = propertyValues["GdiCharSet"];
            object obj10 = propertyValues["GdiVerticalFont"];
            if (obj2 == null)
            {
                obj2 = "Tahoma";
            }
            if (obj3 == null)
            {
                obj3 = 8f;
            }
            if (point == null)
            {
                point = GraphicsUnit.Point;
            }
            if (obj5 == null)
            {
                obj5 = false;
            }
            if (obj6 == null)
            {
                obj6 = false;
            }
            if (obj7 == null)
            {
                obj7 = false;
            }
            if (obj8 == null)
            {
                obj8 = false;
            }
            if (obj9 == null)
            {
                obj9 = (byte) 0;
            }
            if (obj10 == null)
            {
                obj10 = false;
            }
            if (((!(obj2 is string) || !(obj3 is float)) || (!(obj9 is byte) || !(point is GraphicsUnit))) || ((!(obj5 is bool) || !(obj6 is bool)) || ((!(obj7 is bool) || !(obj8 is bool)) || !(obj10 is bool))))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("PropertyValueInvalidEntry"));
            }
            FontStyle regular = FontStyle.Regular;
            if ((obj5 != null) && ((bool) obj5))
            {
                regular |= FontStyle.Bold;
            }
            if ((obj6 != null) && ((bool) obj6))
            {
                regular |= FontStyle.Italic;
            }
            if ((obj7 != null) && ((bool) obj7))
            {
                regular |= FontStyle.Strikeout;
            }
            if ((obj8 != null) && ((bool) obj8))
            {
                regular |= FontStyle.Underline;
            }
            return new Font((string) obj2, (float) obj3, regular, (GraphicsUnit) point, (byte) obj9, (bool) obj10);
        }

        ~FontConverter()
        {
            if (this.fontNameConverter != null)
            {
                ((IDisposable) this.fontNameConverter).Dispose();
            }
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private ArgumentException GetFormatException(string text, char separator)
        {
            string str = string.Format(CultureInfo.CurrentCulture, "name{0} size[units[{0} style=style1[{0} style2{0} ...]]]", new object[] { separator });
            return new ArgumentException(System.Drawing.SR.GetString("TextParseFailedFormat", new object[] { text, str }));
        }

        private string GetGraphicsUnitText(GraphicsUnit units)
        {
            for (int i = 0; i < UnitName.names.Length; i++)
            {
                if (UnitName.names[i].unit == units)
                {
                    return UnitName.names[i].name;
                }
            }
            return "";
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(Font), attributes).Sort(new string[] { "Name", "Size", "Unit", "Weight" });
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private GraphicsUnit ParseGraphicsUnits(string units)
        {
            UnitName name = null;
            for (int i = 0; i < UnitName.names.Length; i++)
            {
                if (string.Equals(UnitName.names[i].name, units, StringComparison.OrdinalIgnoreCase))
                {
                    name = UnitName.names[i];
                    break;
                }
            }
            if (name == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "units", units }));
            }
            return name.unit;
        }

        private string[] ParseSizeTokens(string text, char separator)
        {
            string str = null;
            string str2 = null;
            text = text.Trim();
            int length = text.Length;
            if (length > 0)
            {
                int num2 = 0;
                while (num2 < length)
                {
                    if (char.IsLetter(text[num2]))
                    {
                        break;
                    }
                    num2++;
                }
                char[] trimChars = new char[] { separator, ' ' };
                if (num2 > 0)
                {
                    str = text.Substring(0, num2).Trim(trimChars);
                }
                if (num2 < length)
                {
                    str2 = text.Substring(num2).TrimEnd(trimChars);
                }
            }
            return new string[] { str, str2 };
        }

        public sealed class FontNameConverter : TypeConverter, IDisposable
        {
            private TypeConverter.StandardValuesCollection values;

            public FontNameConverter()
            {
                SystemEvents.InstalledFontsChanged += new EventHandler(this.OnInstalledFontsChanged);
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    return this.MatchFontName((string) value, context);
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (this.values == null)
                {
                    FontFamily[] families = FontFamily.Families;
                    Hashtable hashtable = new Hashtable();
                    for (int i = 0; i < families.Length; i++)
                    {
                        string name = families[i].Name;
                        hashtable[name.ToLower(CultureInfo.InvariantCulture)] = name;
                    }
                    object[] array = new object[hashtable.Values.Count];
                    hashtable.Values.CopyTo(array, 0);
                    Array.Sort(array, Comparer.Default);
                    this.values = new TypeConverter.StandardValuesCollection(array);
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

            private string MatchFontName(string name, ITypeDescriptorContext context)
            {
                string str = null;
                name = name.ToLower(CultureInfo.InvariantCulture);
                IEnumerator enumerator = this.GetStandardValues(context).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string str2 = enumerator.Current.ToString().ToLower(CultureInfo.InvariantCulture);
                    if (str2.Equals(name))
                    {
                        return enumerator.Current.ToString();
                    }
                    if (str2.StartsWith(name) && ((str == null) || (str2.Length <= str.Length)))
                    {
                        str = enumerator.Current.ToString();
                    }
                }
                if (str == null)
                {
                    str = name;
                }
                return str;
            }

            private void OnInstalledFontsChanged(object sender, EventArgs e)
            {
                this.values = null;
            }

            void IDisposable.Dispose()
            {
                SystemEvents.InstalledFontsChanged -= new EventHandler(this.OnInstalledFontsChanged);
            }
        }

        public class FontUnitConverter : EnumConverter
        {
            public FontUnitConverter() : base(typeof(GraphicsUnit))
            {
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (base.Values == null)
                {
                    base.GetStandardValues(context);
                    ArrayList values = new ArrayList(base.Values);
                    values.Remove(GraphicsUnit.Display);
                    base.Values = new TypeConverter.StandardValuesCollection(values);
                }
                return base.Values;
            }
        }

        internal class UnitName
        {
            internal string name;
            internal static readonly FontConverter.UnitName[] names = new FontConverter.UnitName[] { new FontConverter.UnitName("world", GraphicsUnit.World), new FontConverter.UnitName("display", GraphicsUnit.Display), new FontConverter.UnitName("px", GraphicsUnit.Pixel), new FontConverter.UnitName("pt", GraphicsUnit.Point), new FontConverter.UnitName("in", GraphicsUnit.Inch), new FontConverter.UnitName("doc", GraphicsUnit.Document), new FontConverter.UnitName("mm", GraphicsUnit.Millimeter) };
            internal GraphicsUnit unit;

            internal UnitName(string name, GraphicsUnit unit)
            {
                this.name = name;
                this.unit = unit;
            }
        }
    }
}

