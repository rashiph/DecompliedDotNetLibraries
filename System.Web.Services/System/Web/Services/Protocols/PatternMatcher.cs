namespace System.Web.Services.Protocols
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class PatternMatcher
    {
        private MatchType matchType;

        public PatternMatcher(Type type)
        {
            this.matchType = MatchType.Reflect(type);
        }

        public object Match(string text)
        {
            return this.matchType.Match(text);
        }
    }
}

