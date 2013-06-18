namespace System.Configuration
{
    using System;

    internal class DeclarationUpdate : Update
    {
        internal DeclarationUpdate(string configKey, bool moved, string updatedXml) : base(configKey, moved, updatedXml)
        {
        }
    }
}

