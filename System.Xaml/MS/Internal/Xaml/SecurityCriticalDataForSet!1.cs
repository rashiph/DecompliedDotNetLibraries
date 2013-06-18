namespace MS.Internal.Xaml
{
    using MS.Internal.WindowsBase;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), FriendAccessAllowed]
    internal struct SecurityCriticalDataForSet<T>
    {
        [SecurityCritical]
        private T _value;
        [SecurityCritical]
        internal SecurityCriticalDataForSet(T value)
        {
            this._value = value;
        }

        internal T Value
        {
            [SecurityTreatAsSafe, SecurityCritical]
            get
            {
                return this._value;
            }
            [SecurityCritical]
            set
            {
                this._value = value;
            }
        }
    }
}

