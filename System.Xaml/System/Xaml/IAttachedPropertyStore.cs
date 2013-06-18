namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public interface IAttachedPropertyStore
    {
        void CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index);
        bool RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier);
        void SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value);
        bool TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value);

        int PropertyCount { get; }
    }
}

