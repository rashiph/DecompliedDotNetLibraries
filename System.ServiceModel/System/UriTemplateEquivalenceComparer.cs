namespace System
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UriTemplateEquivalenceComparer : IEqualityComparer<UriTemplate>
    {
        private static UriTemplateEquivalenceComparer instance;

        public bool Equals(UriTemplate x, UriTemplate y)
        {
            if (x == null)
            {
                return (y == null);
            }
            return x.IsEquivalentTo(y);
        }

        public int GetHashCode(UriTemplate obj)
        {
            if (obj == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("obj");
            }
            for (int i = obj.segments.Count - 1; i >= 0; i--)
            {
                if (obj.segments[i].Nature == UriTemplatePartType.Literal)
                {
                    return obj.segments[i].GetHashCode();
                }
            }
            return (obj.segments.Count + obj.queries.Count);
        }

        internal static UriTemplateEquivalenceComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UriTemplateEquivalenceComparer();
                }
                return instance;
            }
        }
    }
}

