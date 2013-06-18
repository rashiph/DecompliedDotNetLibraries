namespace System
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UriTemplateMatch
    {
        private Uri baseUri;
        private NameValueCollection boundVariables;
        private object data;
        private Uri originalBaseUri;
        private NameValueCollection queryParameters;
        private Collection<string> relativePathSegments;
        private HttpRequestMessageProperty requestProp;
        private Uri requestUri;
        private UriTemplate template;
        private Collection<string> wildcardPathSegments;
        private int wildcardSegmentsStartOffset = -1;

        private void PopulateQueryParameters()
        {
            if (this.requestUri != null)
            {
                this.queryParameters = UriTemplateHelpers.ParseQueryString(this.requestUri.Query);
            }
            else
            {
                this.queryParameters = new NameValueCollection();
            }
        }

        private void PopulateWildcardSegments()
        {
            if (this.wildcardSegmentsStartOffset != -1)
            {
                this.wildcardPathSegments = new Collection<string>();
                for (int i = this.wildcardSegmentsStartOffset; i < this.RelativePathSegments.Count; i++)
                {
                    this.wildcardPathSegments.Add(this.RelativePathSegments[i]);
                }
            }
            else
            {
                this.wildcardPathSegments = new Collection<string>();
            }
        }

        internal void SetBaseUri(Uri originalBaseUri, HttpRequestMessageProperty requestProp)
        {
            this.baseUri = null;
            this.originalBaseUri = originalBaseUri;
            this.requestProp = requestProp;
        }

        internal void SetQueryParameters(NameValueCollection queryParameters)
        {
            this.queryParameters = new NameValueCollection(queryParameters);
        }

        internal void SetRelativePathSegments(Collection<string> segments)
        {
            this.relativePathSegments = segments;
        }

        internal void SetWildcardPathSegmentsStart(int startOffset)
        {
            this.wildcardSegmentsStartOffset = startOffset;
        }

        public Uri BaseUri
        {
            get
            {
                if ((this.baseUri == null) && (this.originalBaseUri != null))
                {
                    this.baseUri = UriTemplate.RewriteUri(this.originalBaseUri, this.requestProp.Headers[HttpRequestHeader.Host]);
                }
                return this.baseUri;
            }
            set
            {
                this.baseUri = value;
                this.originalBaseUri = null;
                this.requestProp = null;
            }
        }

        public NameValueCollection BoundVariables
        {
            get
            {
                if (this.boundVariables == null)
                {
                    this.boundVariables = new NameValueCollection();
                }
                return this.boundVariables;
            }
        }

        public object Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }

        public NameValueCollection QueryParameters
        {
            get
            {
                if (this.queryParameters == null)
                {
                    this.PopulateQueryParameters();
                }
                return this.queryParameters;
            }
        }

        public Collection<string> RelativePathSegments
        {
            get
            {
                if (this.relativePathSegments == null)
                {
                    this.relativePathSegments = new Collection<string>();
                }
                return this.relativePathSegments;
            }
        }

        public Uri RequestUri
        {
            get
            {
                return this.requestUri;
            }
            set
            {
                this.requestUri = value;
            }
        }

        public UriTemplate Template
        {
            get
            {
                return this.template;
            }
            set
            {
                this.template = value;
            }
        }

        public Collection<string> WildcardPathSegments
        {
            get
            {
                if (this.wildcardPathSegments == null)
                {
                    this.PopulateWildcardSegments();
                }
                return this.wildcardPathSegments;
            }
        }
    }
}

