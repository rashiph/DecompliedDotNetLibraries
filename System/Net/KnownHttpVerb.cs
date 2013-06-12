namespace System.Net
{
    using System;
    using System.Collections.Specialized;

    internal class KnownHttpVerb
    {
        internal static KnownHttpVerb Connect = new KnownHttpVerb("CONNECT", false, true, true, false);
        internal bool ConnectRequest;
        internal bool ContentBodyNotAllowed;
        internal bool ExpectNoContentResponse;
        internal static KnownHttpVerb Get = new KnownHttpVerb("GET", false, true, false, false);
        internal static KnownHttpVerb Head = new KnownHttpVerb("HEAD", false, true, false, true);
        internal static KnownHttpVerb MkCol = new KnownHttpVerb("MKCOL", false, false, false, false);
        internal string Name;
        private static ListDictionary NamedHeaders = new ListDictionary(CaseInsensitiveAscii.StaticInstance);
        internal static KnownHttpVerb Post = new KnownHttpVerb("POST", true, false, false, false);
        internal static KnownHttpVerb Put = new KnownHttpVerb("PUT", true, false, false, false);
        internal bool RequireContentBody;

        static KnownHttpVerb()
        {
            NamedHeaders[Get.Name] = Get;
            NamedHeaders[Connect.Name] = Connect;
            NamedHeaders[Head.Name] = Head;
            NamedHeaders[Put.Name] = Put;
            NamedHeaders[Post.Name] = Post;
            NamedHeaders[MkCol.Name] = MkCol;
        }

        internal KnownHttpVerb(string name, bool requireContentBody, bool contentBodyNotAllowed, bool connectRequest, bool expectNoContentResponse)
        {
            this.Name = name;
            this.RequireContentBody = requireContentBody;
            this.ContentBodyNotAllowed = contentBodyNotAllowed;
            this.ConnectRequest = connectRequest;
            this.ExpectNoContentResponse = expectNoContentResponse;
        }

        public bool Equals(KnownHttpVerb verb)
        {
            if (this != verb)
            {
                return (string.Compare(this.Name, verb.Name, StringComparison.OrdinalIgnoreCase) == 0);
            }
            return true;
        }

        public static KnownHttpVerb Parse(string name)
        {
            KnownHttpVerb verb = NamedHeaders[name] as KnownHttpVerb;
            if (verb == null)
            {
                verb = new KnownHttpVerb(name, false, false, false, false);
            }
            return verb;
        }
    }
}

