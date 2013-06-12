namespace System.Security.AccessControl
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public abstract class QualifiedAce : KnownAce
    {
        private readonly bool _isCallback;
        private byte[] _opaque;
        private readonly System.Security.AccessControl.AceQualifier _qualifier;

        internal QualifiedAce(AceType type, AceFlags flags, int accessMask, SecurityIdentifier sid, byte[] opaque) : base(type, flags, accessMask, sid)
        {
            this._qualifier = this.QualifierFromType(type, out this._isCallback);
            this.SetOpaque(opaque);
        }

        public byte[] GetOpaque()
        {
            return this._opaque;
        }

        private System.Security.AccessControl.AceQualifier QualifierFromType(AceType type, out bool isCallback)
        {
            switch (type)
            {
                case AceType.AccessAllowed:
                    isCallback = false;
                    return System.Security.AccessControl.AceQualifier.AccessAllowed;

                case AceType.AccessDenied:
                    isCallback = false;
                    return System.Security.AccessControl.AceQualifier.AccessDenied;

                case AceType.SystemAudit:
                    isCallback = false;
                    return System.Security.AccessControl.AceQualifier.SystemAudit;

                case AceType.SystemAlarm:
                    isCallback = false;
                    return System.Security.AccessControl.AceQualifier.SystemAlarm;

                case AceType.AccessAllowedObject:
                    isCallback = false;
                    return System.Security.AccessControl.AceQualifier.AccessAllowed;

                case AceType.AccessDeniedObject:
                    isCallback = false;
                    return System.Security.AccessControl.AceQualifier.AccessDenied;

                case AceType.SystemAuditObject:
                    isCallback = false;
                    return System.Security.AccessControl.AceQualifier.SystemAudit;

                case AceType.SystemAlarmObject:
                    isCallback = false;
                    return System.Security.AccessControl.AceQualifier.SystemAlarm;

                case AceType.AccessAllowedCallback:
                    isCallback = true;
                    return System.Security.AccessControl.AceQualifier.AccessAllowed;

                case AceType.AccessDeniedCallback:
                    isCallback = true;
                    return System.Security.AccessControl.AceQualifier.AccessDenied;

                case AceType.AccessAllowedCallbackObject:
                    isCallback = true;
                    return System.Security.AccessControl.AceQualifier.AccessAllowed;

                case AceType.AccessDeniedCallbackObject:
                    isCallback = true;
                    return System.Security.AccessControl.AceQualifier.AccessDenied;

                case AceType.SystemAuditCallback:
                    isCallback = true;
                    return System.Security.AccessControl.AceQualifier.SystemAudit;

                case AceType.SystemAlarmCallback:
                    isCallback = true;
                    return System.Security.AccessControl.AceQualifier.SystemAlarm;

                case AceType.SystemAuditCallbackObject:
                    isCallback = true;
                    return System.Security.AccessControl.AceQualifier.SystemAudit;

                case AceType.SystemAlarmCallbackObject:
                    isCallback = true;
                    return System.Security.AccessControl.AceQualifier.SystemAlarm;
            }
            throw new SystemException();
        }

        public void SetOpaque(byte[] opaque)
        {
            if (opaque != null)
            {
                if (opaque.Length > this.MaxOpaqueLengthInternal)
                {
                    throw new ArgumentOutOfRangeException("opaque", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_ArrayLength"), new object[] { 0, this.MaxOpaqueLengthInternal }));
                }
                if ((opaque.Length % 4) != 0)
                {
                    throw new ArgumentOutOfRangeException("opaque", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_ArrayLengthMultiple"), new object[] { 4 }));
                }
            }
            this._opaque = opaque;
        }

        public System.Security.AccessControl.AceQualifier AceQualifier
        {
            get
            {
                return this._qualifier;
            }
        }

        public bool IsCallback
        {
            get
            {
                return this._isCallback;
            }
        }

        internal abstract int MaxOpaqueLengthInternal { get; }

        public int OpaqueLength
        {
            get
            {
                if (this._opaque != null)
                {
                    return this._opaque.Length;
                }
                return 0;
            }
        }
    }
}

