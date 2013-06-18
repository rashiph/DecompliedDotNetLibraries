namespace System.Web.UI
{
    using System;

    internal abstract class RegisterDirectiveEntry : SourceLineInfo
    {
        private string _tagPrefix;

        internal RegisterDirectiveEntry(string tagPrefix)
        {
            this._tagPrefix = tagPrefix;
        }

        internal string TagPrefix
        {
            get
            {
                return this._tagPrefix;
            }
        }
    }
}

