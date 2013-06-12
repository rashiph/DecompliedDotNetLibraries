namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Web.Util;

    public class WebColorConverter : ColorConverter
    {
        private static Hashtable htmlSysColorTable;

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string str = ((string) value).Trim();
                Color empty = Color.Empty;
                if (string.IsNullOrEmpty(str))
                {
                    return empty;
                }
                if (str[0] == '#')
                {
                    return base.ConvertFrom(context, culture, value);
                }
                if (StringUtil.EqualsIgnoreCase(str, "LightGrey"))
                {
                    return Color.LightGray;
                }
                if (htmlSysColorTable == null)
                {
                    InitializeHTMLSysColorTable();
                }
                object obj2 = htmlSysColorTable[str];
                if (obj2 != null)
                {
                    return (Color) obj2;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(string)) && (value != null))
            {
                Color color = (Color) value;
                if (color == Color.Empty)
                {
                    return string.Empty;
                }
                if (!color.IsKnownColor)
                {
                    StringBuilder builder = new StringBuilder("#", 7);
                    builder.Append(color.R.ToString("X2", CultureInfo.InvariantCulture));
                    builder.Append(color.G.ToString("X2", CultureInfo.InvariantCulture));
                    builder.Append(color.B.ToString("X2", CultureInfo.InvariantCulture));
                    return builder.ToString();
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static void InitializeHTMLSysColorTable()
        {
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            hashtable["activeborder"] = Color.FromKnownColor(KnownColor.ActiveBorder);
            hashtable["activecaption"] = Color.FromKnownColor(KnownColor.ActiveCaption);
            hashtable["appworkspace"] = Color.FromKnownColor(KnownColor.AppWorkspace);
            hashtable["background"] = Color.FromKnownColor(KnownColor.Desktop);
            hashtable["buttonface"] = Color.FromKnownColor(KnownColor.Control);
            hashtable["buttonhighlight"] = Color.FromKnownColor(KnownColor.ControlLightLight);
            hashtable["buttonshadow"] = Color.FromKnownColor(KnownColor.ControlDark);
            hashtable["buttontext"] = Color.FromKnownColor(KnownColor.ControlText);
            hashtable["captiontext"] = Color.FromKnownColor(KnownColor.ActiveCaptionText);
            hashtable["graytext"] = Color.FromKnownColor(KnownColor.GrayText);
            hashtable["highlight"] = Color.FromKnownColor(KnownColor.Highlight);
            hashtable["highlighttext"] = Color.FromKnownColor(KnownColor.HighlightText);
            hashtable["inactiveborder"] = Color.FromKnownColor(KnownColor.InactiveBorder);
            hashtable["inactivecaption"] = Color.FromKnownColor(KnownColor.InactiveCaption);
            hashtable["inactivecaptiontext"] = Color.FromKnownColor(KnownColor.InactiveCaptionText);
            hashtable["infobackground"] = Color.FromKnownColor(KnownColor.Info);
            hashtable["infotext"] = Color.FromKnownColor(KnownColor.InfoText);
            hashtable["menu"] = Color.FromKnownColor(KnownColor.Menu);
            hashtable["menutext"] = Color.FromKnownColor(KnownColor.MenuText);
            hashtable["scrollbar"] = Color.FromKnownColor(KnownColor.ScrollBar);
            hashtable["threeddarkshadow"] = Color.FromKnownColor(KnownColor.ControlDarkDark);
            hashtable["threedface"] = Color.FromKnownColor(KnownColor.Control);
            hashtable["threedhighlight"] = Color.FromKnownColor(KnownColor.ControlLight);
            hashtable["threedlightshadow"] = Color.FromKnownColor(KnownColor.ControlLightLight);
            hashtable["window"] = Color.FromKnownColor(KnownColor.Window);
            hashtable["windowframe"] = Color.FromKnownColor(KnownColor.WindowFrame);
            hashtable["windowtext"] = Color.FromKnownColor(KnownColor.WindowText);
            htmlSysColorTable = hashtable;
        }
    }
}

