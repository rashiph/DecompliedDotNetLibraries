namespace System.Media
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, UI=true)]
    public sealed class SystemSounds
    {
        private static SystemSound asterisk;
        private static SystemSound beep;
        private static SystemSound exclamation;
        private static SystemSound hand;
        private static SystemSound question;

        private SystemSounds()
        {
        }

        public static SystemSound Asterisk
        {
            get
            {
                if (asterisk == null)
                {
                    asterisk = new SystemSound(0x40);
                }
                return asterisk;
            }
        }

        public static SystemSound Beep
        {
            get
            {
                if (beep == null)
                {
                    beep = new SystemSound(0);
                }
                return beep;
            }
        }

        public static SystemSound Exclamation
        {
            get
            {
                if (exclamation == null)
                {
                    exclamation = new SystemSound(0x30);
                }
                return exclamation;
            }
        }

        public static SystemSound Hand
        {
            get
            {
                if (hand == null)
                {
                    hand = new SystemSound(0x10);
                }
                return hand;
            }
        }

        public static SystemSound Question
        {
            get
            {
                if (question == null)
                {
                    question = new SystemSound(0x20);
                }
                return question;
            }
        }

        private class NativeMethods
        {
            internal const int MB_ICONASTERISK = 0x40;
            internal const int MB_ICONEXCLAMATION = 0x30;
            internal const int MB_ICONHAND = 0x10;
            internal const int MB_ICONQUESTION = 0x20;

            private NativeMethods()
            {
            }
        }
    }
}

