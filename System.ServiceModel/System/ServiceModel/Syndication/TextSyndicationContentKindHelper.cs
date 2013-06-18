namespace System.ServiceModel.Syndication
{
    using System;

    internal static class TextSyndicationContentKindHelper
    {
        public static bool IsDefined(TextSyndicationContentKind kind)
        {
            if ((kind != TextSyndicationContentKind.Plaintext) && (kind != TextSyndicationContentKind.Html))
            {
                return (kind == TextSyndicationContentKind.XHtml);
            }
            return true;
        }
    }
}

