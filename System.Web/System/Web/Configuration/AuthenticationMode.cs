namespace System.Web.Configuration
{
    using System;

    public enum AuthenticationMode
    {
        Forms = 3,
        None = 0,
        [Obsolete("This field is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
        Passport = 2,
        Windows = 1
    }
}

