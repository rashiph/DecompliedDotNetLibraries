namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;

    internal class LinkUtilities
    {
        private static System.Drawing.Color ieactiveLinkColor = System.Drawing.Color.Empty;
        private const string IEAnchorColor = "Anchor Color";
        private const string IEAnchorColorHover = "Anchor Color Hover";
        private const string IEAnchorColorVisited = "Anchor Color Visited";
        private static System.Drawing.Color ielinkColor = System.Drawing.Color.Empty;
        public const string IEMainRegPath = @"Software\Microsoft\Internet Explorer\Main";
        private const string IESettingsRegPath = @"Software\Microsoft\Internet Explorer\Settings";
        private static System.Drawing.Color ievisitedLinkColor = System.Drawing.Color.Empty;

        public static void EnsureLinkFonts(Font baseFont, LinkBehavior link, ref Font linkFont, ref Font hoverLinkFont)
        {
            if ((linkFont == null) || (hoverLinkFont == null))
            {
                bool flag = true;
                bool flag2 = true;
                if (link == LinkBehavior.SystemDefault)
                {
                    link = GetIELinkBehavior();
                }
                switch (link)
                {
                    case LinkBehavior.AlwaysUnderline:
                        flag = true;
                        flag2 = true;
                        break;

                    case LinkBehavior.HoverUnderline:
                        flag = false;
                        flag2 = true;
                        break;

                    case LinkBehavior.NeverUnderline:
                        flag = false;
                        flag2 = false;
                        break;
                }
                Font prototype = baseFont;
                if (flag2 == flag)
                {
                    FontStyle newStyle = prototype.Style;
                    if (flag2)
                    {
                        newStyle |= FontStyle.Underline;
                    }
                    else
                    {
                        newStyle &= ~FontStyle.Underline;
                    }
                    hoverLinkFont = new Font(prototype, newStyle);
                    linkFont = hoverLinkFont;
                }
                else
                {
                    FontStyle style = prototype.Style;
                    if (flag2)
                    {
                        style |= FontStyle.Underline;
                    }
                    else
                    {
                        style &= ~FontStyle.Underline;
                    }
                    hoverLinkFont = new Font(prototype, style);
                    FontStyle style3 = prototype.Style;
                    if (flag)
                    {
                        style3 |= FontStyle.Underline;
                    }
                    else
                    {
                        style3 &= ~FontStyle.Underline;
                    }
                    linkFont = new Font(prototype, style3);
                }
            }
        }

        private static System.Drawing.Color GetIEColor(string name)
        {
            System.Drawing.Color red;
            new RegistryPermission(PermissionState.Unrestricted).Assert();
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Settings");
                if (key != null)
                {
                    string str = (string) key.GetValue(name);
                    key.Close();
                    if (str != null)
                    {
                        string[] strArray = str.Split(new char[] { ',' });
                        int[] numArray = new int[3];
                        int num = Math.Min(numArray.Length, strArray.Length);
                        for (int i = 0; i < num; i++)
                        {
                            int.TryParse(strArray[i], out numArray[i]);
                        }
                        return System.Drawing.Color.FromArgb(numArray[0], numArray[1], numArray[2]);
                    }
                }
                if (string.Equals(name, "Anchor Color", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Drawing.Color.Blue;
                }
                if (string.Equals(name, "Anchor Color Visited", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Drawing.Color.Purple;
                }
                if (string.Equals(name, "Anchor Color Hover", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Drawing.Color.Red;
                }
                red = System.Drawing.Color.Red;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return red;
        }

        public static LinkBehavior GetIELinkBehavior()
        {
            new RegistryPermission(PermissionState.Unrestricted).Assert();
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main");
                if (key != null)
                {
                    string strA = (string) key.GetValue("Anchor Underline");
                    key.Close();
                    if ((strA != null) && (string.Compare(strA, "no", true, CultureInfo.InvariantCulture) == 0))
                    {
                        return LinkBehavior.NeverUnderline;
                    }
                    if ((strA != null) && (string.Compare(strA, "hover", true, CultureInfo.InvariantCulture) == 0))
                    {
                        return LinkBehavior.HoverUnderline;
                    }
                    return LinkBehavior.AlwaysUnderline;
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return LinkBehavior.AlwaysUnderline;
        }

        public static System.Drawing.Color IEActiveLinkColor
        {
            get
            {
                if (ieactiveLinkColor.IsEmpty)
                {
                    ieactiveLinkColor = GetIEColor("Anchor Color Hover");
                }
                return ieactiveLinkColor;
            }
        }

        public static System.Drawing.Color IELinkColor
        {
            get
            {
                if (ielinkColor.IsEmpty)
                {
                    ielinkColor = GetIEColor("Anchor Color");
                }
                return ielinkColor;
            }
        }

        public static System.Drawing.Color IEVisitedLinkColor
        {
            get
            {
                if (ievisitedLinkColor.IsEmpty)
                {
                    ievisitedLinkColor = GetIEColor("Anchor Color Visited");
                }
                return ievisitedLinkColor;
            }
        }
    }
}

