namespace System.Drawing
{
    using System;

    public sealed class SystemIcons
    {
        private static Icon _application;
        private static Icon _asterisk;
        private static Icon _error;
        private static Icon _exclamation;
        private static Icon _hand;
        private static Icon _information;
        private static Icon _question;
        private static Icon _shield;
        private static Icon _warning;
        private static Icon _winlogo;

        private SystemIcons()
        {
        }

        public static Icon Application
        {
            get
            {
                if (_application == null)
                {
                    _application = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f00));
                }
                return _application;
            }
        }

        public static Icon Asterisk
        {
            get
            {
                if (_asterisk == null)
                {
                    _asterisk = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f04));
                }
                return _asterisk;
            }
        }

        public static Icon Error
        {
            get
            {
                if (_error == null)
                {
                    _error = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f01));
                }
                return _error;
            }
        }

        public static Icon Exclamation
        {
            get
            {
                if (_exclamation == null)
                {
                    _exclamation = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f03));
                }
                return _exclamation;
            }
        }

        public static Icon Hand
        {
            get
            {
                if (_hand == null)
                {
                    _hand = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f01));
                }
                return _hand;
            }
        }

        public static Icon Information
        {
            get
            {
                if (_information == null)
                {
                    _information = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f04));
                }
                return _information;
            }
        }

        public static Icon Question
        {
            get
            {
                if (_question == null)
                {
                    _question = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f02));
                }
                return _question;
            }
        }

        public static Icon Shield
        {
            get
            {
                if (_shield == null)
                {
                    _shield = new Icon(typeof(SystemIcons), "ShieldIcon.ico");
                }
                return _shield;
            }
        }

        public static Icon Warning
        {
            get
            {
                if (_warning == null)
                {
                    _warning = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f03));
                }
                return _warning;
            }
        }

        public static Icon WinLogo
        {
            get
            {
                if (_winlogo == null)
                {
                    _winlogo = new Icon(SafeNativeMethods.LoadIcon(NativeMethods.NullHandleRef, 0x7f05));
                }
                return _winlogo;
            }
        }
    }
}

