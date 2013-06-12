namespace System.Drawing
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class ColorConverter : TypeConverter
    {
        private static Hashtable colorConstants;
        private static string ColorConstantsLock = "colorConstants";
        private static Hashtable systemColorConstants;
        private static string SystemColorConstantsLock = "systemColorConstants";
        private static TypeConverter.StandardValuesCollection values;
        private static string ValuesLock = "values";

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
            object namedColor = null;
            string name = str.Trim();
            if (name.Length == 0)
            {
                return Color.Empty;
            }
            namedColor = GetNamedColor(name);
            if (namedColor == null)
            {
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }
                char ch = culture.TextInfo.ListSeparator[0];
                bool flag = true;
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
                if (name.IndexOf(ch) == -1)
                {
                    if (((name.Length >= 2) && ((name[0] == '\'') || (name[0] == '"'))) && (name[0] == name[name.Length - 1]))
                    {
                        namedColor = Color.FromName(name.Substring(1, name.Length - 2));
                        flag = false;
                    }
                    else if ((((name.Length == 7) && (name[0] == '#')) || ((name.Length == 8) && (name.StartsWith("0x") || name.StartsWith("0X")))) || ((name.Length == 8) && (name.StartsWith("&h") || name.StartsWith("&H"))))
                    {
                        namedColor = Color.FromArgb(-16777216 | ((int) converter.ConvertFromString(context, culture, name)));
                    }
                }
                if (namedColor == null)
                {
                    string[] strArray = name.Split(new char[] { ch });
                    int[] numArray = new int[strArray.Length];
                    for (int i = 0; i < numArray.Length; i++)
                    {
                        numArray[i] = (int) converter.ConvertFromString(context, culture, strArray[i]);
                    }
                    switch (numArray.Length)
                    {
                        case 1:
                            namedColor = Color.FromArgb(numArray[0]);
                            break;

                        case 3:
                            namedColor = Color.FromArgb(numArray[0], numArray[1], numArray[2]);
                            break;

                        case 4:
                            namedColor = Color.FromArgb(numArray[0], numArray[1], numArray[2], numArray[3]);
                            break;
                    }
                    flag = true;
                }
                if ((namedColor != null) && flag)
                {
                    int num2 = ((Color) namedColor).ToArgb();
                    foreach (Color color in Colors.Values)
                    {
                        if (color.ToArgb() == num2)
                        {
                            namedColor = color;
                            break;
                        }
                    }
                }
            }
            if (namedColor == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidColor", new object[] { name }));
            }
            return namedColor;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (value is Color)
            {
                if (destinationType == typeof(string))
                {
                    string[] strArray;
                    Color color = (Color) value;
                    if (color == Color.Empty)
                    {
                        return string.Empty;
                    }
                    if (color.IsKnownColor)
                    {
                        return color.Name;
                    }
                    if (color.IsNamedColor)
                    {
                        return ("'" + color.Name + "'");
                    }
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    string separator = culture.TextInfo.ListSeparator + " ";
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
                    int num = 0;
                    if (color.A < 0xff)
                    {
                        strArray = new string[4];
                        strArray[num++] = converter.ConvertToString(context, culture, color.A);
                    }
                    else
                    {
                        strArray = new string[3];
                    }
                    strArray[num++] = converter.ConvertToString(context, culture, color.R);
                    strArray[num++] = converter.ConvertToString(context, culture, color.G);
                    strArray[num++] = converter.ConvertToString(context, culture, color.B);
                    return string.Join(separator, strArray);
                }
                if (destinationType == typeof(InstanceDescriptor))
                {
                    MemberInfo member = null;
                    object[] arguments = null;
                    Color color2 = (Color) value;
                    if (color2.IsEmpty)
                    {
                        member = typeof(Color).GetField("Empty");
                    }
                    else if (color2.IsSystemColor)
                    {
                        member = typeof(System.Drawing.SystemColors).GetProperty(color2.Name);
                    }
                    else if (color2.IsKnownColor)
                    {
                        member = typeof(Color).GetProperty(color2.Name);
                    }
                    else if (color2.A != 0xff)
                    {
                        member = typeof(Color).GetMethod("FromArgb", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) });
                        arguments = new object[] { color2.A, color2.R, color2.G, color2.B };
                    }
                    else if (color2.IsNamedColor)
                    {
                        member = typeof(Color).GetMethod("FromName", new Type[] { typeof(string) });
                        arguments = new object[] { color2.Name };
                    }
                    else
                    {
                        member = typeof(Color).GetMethod("FromArgb", new Type[] { typeof(int), typeof(int), typeof(int) });
                        arguments = new object[] { color2.R, color2.G, color2.B };
                    }
                    if (member != null)
                    {
                        return new InstanceDescriptor(member, arguments);
                    }
                    return null;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static void FillConstants(Hashtable hash, Type enumType)
        {
            MethodAttributes attributes = MethodAttributes.Static | MethodAttributes.Public;
            foreach (PropertyInfo info in enumType.GetProperties())
            {
                if (info.PropertyType == typeof(Color))
                {
                    MethodInfo getMethod = info.GetGetMethod();
                    if ((getMethod != null) && ((getMethod.Attributes & attributes) == attributes))
                    {
                        object[] index = null;
                        hash[info.Name] = info.GetValue(null, index);
                    }
                }
            }
        }

        internal static object GetNamedColor(string name)
        {
            object obj2 = null;
            obj2 = Colors[name];
            if (obj2 != null)
            {
                return obj2;
            }
            return SystemColors[name];
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (values == null)
            {
                lock (ValuesLock)
                {
                    if (values == null)
                    {
                        ArrayList list = new ArrayList();
                        list.AddRange(Colors.Values);
                        list.AddRange(SystemColors.Values);
                        int count = list.Count;
                        for (int i = 0; i < (count - 1); i++)
                        {
                            for (int j = i + 1; j < count; j++)
                            {
                                if (list[i].Equals(list[j]))
                                {
                                    list.RemoveAt(j);
                                    count--;
                                    j--;
                                }
                            }
                        }
                        list.Sort(0, list.Count, new ColorComparer());
                        values = new TypeConverter.StandardValuesCollection(list.ToArray());
                    }
                }
            }
            return values;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private static Hashtable Colors
        {
            get
            {
                if (colorConstants == null)
                {
                    lock (ColorConstantsLock)
                    {
                        if (colorConstants == null)
                        {
                            Hashtable hash = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            FillConstants(hash, typeof(Color));
                            colorConstants = hash;
                        }
                    }
                }
                return colorConstants;
            }
        }

        private static Hashtable SystemColors
        {
            get
            {
                if (systemColorConstants == null)
                {
                    lock (SystemColorConstantsLock)
                    {
                        if (systemColorConstants == null)
                        {
                            Hashtable hash = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            FillConstants(hash, typeof(System.Drawing.SystemColors));
                            systemColorConstants = hash;
                        }
                    }
                }
                return systemColorConstants;
            }
        }

        private class ColorComparer : IComparer
        {
            public int Compare(object left, object right)
            {
                Color color = (Color) left;
                Color color2 = (Color) right;
                return string.Compare(color.Name, color2.Name, false, CultureInfo.InvariantCulture);
            }
        }
    }
}

