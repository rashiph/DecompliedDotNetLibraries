namespace System.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security;
    using System.Threading;

    internal interface IResourceGroveler
    {
        [SecuritySafeCritical]
        ResourceSet GrovelForResourceSet(CultureInfo culture, Dictionary<string, ResourceSet> localResourceSets, bool tryParents, bool createIfNotExists, ref StackCrawlMark stackMark);
        bool HasNeutralResources(CultureInfo culture, string defaultResName);
    }
}

