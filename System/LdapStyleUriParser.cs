namespace System
{
    public class LdapStyleUriParser : UriParser
    {
        public LdapStyleUriParser() : base(UriParser.LdapUri.Flags)
        {
        }
    }
}

