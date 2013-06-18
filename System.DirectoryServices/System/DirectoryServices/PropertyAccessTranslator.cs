namespace System.DirectoryServices
{
    using System;
    using System.ComponentModel;

    internal sealed class PropertyAccessTranslator
    {
        internal static int AccessMaskFromPropertyAccess(PropertyAccess access)
        {
            if ((access < PropertyAccess.Read) || (access > PropertyAccess.Write))
            {
                throw new InvalidEnumArgumentException("access", (int) access, typeof(PropertyAccess));
            }
            switch (access)
            {
                case PropertyAccess.Read:
                    return ActiveDirectoryRightsTranslator.AccessMaskFromRights(ActiveDirectoryRights.ReadProperty);

                case PropertyAccess.Write:
                    return ActiveDirectoryRightsTranslator.AccessMaskFromRights(ActiveDirectoryRights.WriteProperty);
            }
            throw new ArgumentException("access");
        }
    }
}

