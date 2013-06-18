namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;

    internal class DisplayInformation
    {
        private static short bitsPerPixel;
        private static bool dropShadowEnabled;
        private static bool dropShadowSettingValid;
        private static bool highContrast;
        private static bool highContrastSettingValid;
        private static bool isTerminalServerSession;
        private static bool lowRes;
        private static bool lowResSettingValid;
        private static bool menuAccessKeysUnderlined;
        private static bool menuAccessKeysUnderlinedValid;
        private static bool terminalSettingValid;

        static DisplayInformation()
        {
            SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(DisplayInformation.UserPreferenceChanging);
            SystemEvents.DisplaySettingsChanging += new EventHandler(DisplayInformation.DisplaySettingsChanging);
        }

        private static void DisplaySettingsChanging(object obj, EventArgs ea)
        {
            highContrastSettingValid = false;
            lowResSettingValid = false;
            terminalSettingValid = false;
            dropShadowSettingValid = false;
            menuAccessKeysUnderlinedValid = false;
        }

        private static void UserPreferenceChanging(object obj, UserPreferenceChangingEventArgs e)
        {
            highContrastSettingValid = false;
            lowResSettingValid = false;
            terminalSettingValid = false;
            dropShadowSettingValid = false;
            bitsPerPixel = 0;
            if (e.Category == UserPreferenceCategory.General)
            {
                menuAccessKeysUnderlinedValid = false;
            }
        }

        public static short BitsPerPixel
        {
            get
            {
                if (bitsPerPixel == 0)
                {
                    bitsPerPixel = (short) Screen.PrimaryScreen.BitsPerPixel;
                }
                return bitsPerPixel;
            }
        }

        public static bool HighContrast
        {
            get
            {
                if (!highContrastSettingValid)
                {
                    highContrast = SystemInformation.HighContrast;
                    highContrastSettingValid = true;
                }
                return highContrast;
            }
        }

        public static bool IsDropShadowEnabled
        {
            get
            {
                if (!dropShadowSettingValid)
                {
                    dropShadowEnabled = SystemInformation.IsDropShadowEnabled;
                    dropShadowSettingValid = true;
                }
                return dropShadowEnabled;
            }
        }

        public static bool LowResolution
        {
            get
            {
                if (!lowResSettingValid || lowRes)
                {
                    lowRes = BitsPerPixel <= 8;
                    lowResSettingValid = true;
                }
                return lowRes;
            }
        }

        public static bool MenuAccessKeysUnderlined
        {
            get
            {
                if (!menuAccessKeysUnderlinedValid)
                {
                    menuAccessKeysUnderlined = SystemInformation.MenuAccessKeysUnderlined;
                    menuAccessKeysUnderlinedValid = true;
                }
                return menuAccessKeysUnderlined;
            }
        }

        public static bool TerminalServer
        {
            get
            {
                if (!terminalSettingValid)
                {
                    isTerminalServerSession = SystemInformation.TerminalServerSession;
                    terminalSettingValid = true;
                }
                return isTerminalServerSession;
            }
        }
    }
}

