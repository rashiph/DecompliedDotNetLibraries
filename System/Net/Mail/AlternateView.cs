namespace System.Net.Mail
{
    using System;
    using System.IO;
    using System.Net.Mime;
    using System.Text;

    public class AlternateView : AttachmentBase
    {
        private LinkedResourceCollection linkedResources;

        internal AlternateView()
        {
        }

        public AlternateView(Stream contentStream) : base(contentStream)
        {
        }

        public AlternateView(string fileName) : base(fileName)
        {
        }

        public AlternateView(Stream contentStream, ContentType contentType) : base(contentStream, contentType)
        {
        }

        public AlternateView(Stream contentStream, string mediaType) : base(contentStream, mediaType)
        {
        }

        public AlternateView(string fileName, ContentType contentType) : base(fileName, contentType)
        {
        }

        public AlternateView(string fileName, string mediaType) : base(fileName, mediaType)
        {
        }

        public static AlternateView CreateAlternateViewFromString(string content)
        {
            AlternateView view = new AlternateView();
            view.SetContentFromString(content, null, string.Empty);
            return view;
        }

        public static AlternateView CreateAlternateViewFromString(string content, ContentType contentType)
        {
            AlternateView view = new AlternateView();
            view.SetContentFromString(content, contentType);
            return view;
        }

        public static AlternateView CreateAlternateViewFromString(string content, Encoding contentEncoding, string mediaType)
        {
            AlternateView view = new AlternateView();
            view.SetContentFromString(content, contentEncoding, mediaType);
            return view;
        }

        protected override void Dispose(bool disposing)
        {
            if (!base.disposed)
            {
                if (disposing && (this.linkedResources != null))
                {
                    this.linkedResources.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        public Uri BaseUri
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

        public LinkedResourceCollection LinkedResources
        {
            get
            {
                if (base.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                if (this.linkedResources == null)
                {
                    this.linkedResources = new LinkedResourceCollection();
                }
                return this.linkedResources;
            }
        }
    }
}

