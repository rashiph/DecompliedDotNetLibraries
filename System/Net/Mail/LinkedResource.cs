namespace System.Net.Mail
{
    using System;
    using System.IO;
    using System.Net.Mime;
    using System.Text;

    public class LinkedResource : AttachmentBase
    {
        internal LinkedResource()
        {
        }

        public LinkedResource(Stream contentStream) : base(contentStream)
        {
        }

        public LinkedResource(string fileName) : base(fileName)
        {
        }

        public LinkedResource(Stream contentStream, ContentType contentType) : base(contentStream, contentType)
        {
        }

        public LinkedResource(Stream contentStream, string mediaType) : base(contentStream, mediaType)
        {
        }

        public LinkedResource(string fileName, ContentType contentType) : base(fileName, contentType)
        {
        }

        public LinkedResource(string fileName, string mediaType) : base(fileName, mediaType)
        {
        }

        public static LinkedResource CreateLinkedResourceFromString(string content)
        {
            LinkedResource resource = new LinkedResource();
            resource.SetContentFromString(content, null, string.Empty);
            return resource;
        }

        public static LinkedResource CreateLinkedResourceFromString(string content, ContentType contentType)
        {
            LinkedResource resource = new LinkedResource();
            resource.SetContentFromString(content, contentType);
            return resource;
        }

        public static LinkedResource CreateLinkedResourceFromString(string content, Encoding contentEncoding, string mediaType)
        {
            LinkedResource resource = new LinkedResource();
            resource.SetContentFromString(content, contentEncoding, mediaType);
            return resource;
        }

        public Uri ContentLink
        {
            get
            {
                return base.ContentLocation;
            }
            set
            {
                base.ContentLocation = value;
            }
        }
    }
}

