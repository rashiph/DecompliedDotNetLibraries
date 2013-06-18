namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    public class DiscoveryClientProtocol : HttpWebClientProtocol
    {
        private ArrayList additionalInformation;
        private DiscoveryClientDocumentCollection documents;
        private DiscoveryExceptionDictionary errors;
        private Hashtable inlinedSchemas;
        private DiscoveryClientReferenceCollection references;

        public DiscoveryClientProtocol()
        {
            this.references = new DiscoveryClientReferenceCollection();
            this.documents = new DiscoveryClientDocumentCollection();
            this.inlinedSchemas = new Hashtable();
            this.additionalInformation = new ArrayList();
            this.errors = new DiscoveryExceptionDictionary();
        }

        internal DiscoveryClientProtocol(HttpWebClientProtocol protocol) : base(protocol)
        {
            this.references = new DiscoveryClientReferenceCollection();
            this.documents = new DiscoveryClientDocumentCollection();
            this.inlinedSchemas = new Hashtable();
            this.additionalInformation = new ArrayList();
            this.errors = new DiscoveryExceptionDictionary();
        }

        private static void AddFilename(Hashtable filenames, string path)
        {
            filenames.Add(path.ToLower(CultureInfo.InvariantCulture), path);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public DiscoveryDocument Discover(string url)
        {
            DiscoveryDocument document = this.Documents[url] as DiscoveryDocument;
            if (document != null)
            {
                return document;
            }
            DiscoveryDocumentReference reference = new DiscoveryDocumentReference(url) {
                ClientProtocol = this
            };
            this.References[url] = reference;
            this.Errors.Clear();
            return reference.Document;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public DiscoveryDocument DiscoverAny(string url)
        {
            Type[] discoveryReferenceTypes = WebServicesSection.Current.DiscoveryReferenceTypes;
            DiscoveryReference reference = null;
            string contentType = null;
            Stream stream = this.Download(ref url, ref contentType);
            this.Errors.Clear();
            bool flag = true;
            Exception innerException = null;
            ArrayList list = new ArrayList();
            foreach (Type type in discoveryReferenceTypes)
            {
                if (typeof(DiscoveryReference).IsAssignableFrom(type))
                {
                    reference = (DiscoveryReference) Activator.CreateInstance(type);
                    reference.Url = url;
                    reference.ClientProtocol = this;
                    stream.Position = 0L;
                    Exception exception2 = reference.AttemptResolve(contentType, stream);
                    if (exception2 == null)
                    {
                        break;
                    }
                    this.Errors[type.FullName] = exception2;
                    reference = null;
                    InvalidContentTypeException exception3 = exception2 as InvalidContentTypeException;
                    if ((exception3 == null) || !ContentType.MatchesBase(exception3.ContentType, "text/html"))
                    {
                        flag = false;
                    }
                    if (exception2 is InvalidDocumentContentsException)
                    {
                        innerException = exception2;
                        break;
                    }
                    if ((exception2.InnerException != null) && (exception2.InnerException.InnerException == null))
                    {
                        list.Add(exception2.InnerException.Message);
                    }
                }
            }
            if (reference == null)
            {
                if (innerException != null)
                {
                    StringBuilder builder = new StringBuilder(Res.GetString("TheDocumentWasUnderstoodButContainsErrors"));
                    while (innerException != null)
                    {
                        builder.Append("\n  - ").Append(innerException.Message);
                        innerException = innerException.InnerException;
                    }
                    throw new InvalidOperationException(builder.ToString());
                }
                if (flag)
                {
                    throw new InvalidOperationException(Res.GetString("TheHTMLDocumentDoesNotContainDiscoveryInformation"));
                }
                bool flag2 = (list.Count == this.Errors.Count) && (this.Errors.Count > 0);
                for (int i = 1; flag2 && (i < list.Count); i++)
                {
                    if (((string) list[i - 1]) != ((string) list[i]))
                    {
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    throw new InvalidOperationException(Res.GetString("TheDocumentWasNotRecognizedAsAKnownDocumentType", new object[] { list[0] }));
                }
                StringBuilder builder2 = new StringBuilder(Res.GetString("WebMissingResource", new object[] { url }));
                foreach (DictionaryEntry entry in this.Errors)
                {
                    Exception exception5 = (Exception) entry.Value;
                    string key = (string) entry.Key;
                    if (string.Compare(key, typeof(ContractReference).FullName, StringComparison.Ordinal) == 0)
                    {
                        key = Res.GetString("WebContractReferenceName");
                    }
                    else if (string.Compare(key, typeof(SchemaReference).FullName, StringComparison.Ordinal) == 0)
                    {
                        key = Res.GetString("WebShemaReferenceName");
                    }
                    else if (string.Compare(key, typeof(DiscoveryDocumentReference).FullName, StringComparison.Ordinal) == 0)
                    {
                        key = Res.GetString("WebDiscoveryDocumentReferenceName");
                    }
                    builder2.Append("\n- ").Append(Res.GetString("WebDiscoRefReport", new object[] { key, exception5.Message }));
                    while (exception5.InnerException != null)
                    {
                        builder2.Append("\n  - ").Append(exception5.InnerException.Message);
                        exception5 = exception5.InnerException;
                    }
                }
                throw new InvalidOperationException(builder2.ToString());
            }
            if (reference is DiscoveryDocumentReference)
            {
                return ((DiscoveryDocumentReference) reference).Document;
            }
            this.References[reference.Url] = reference;
            DiscoveryDocument document = new DiscoveryDocument();
            document.References.Add(reference);
            return document;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public Stream Download(ref string url)
        {
            string contentType = null;
            return this.Download(ref url, ref contentType);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public Stream Download(ref string url, ref string contentType)
        {
            Stream stream2;
            WebRequest webRequest = this.GetWebRequest(new Uri(url));
            webRequest.Method = "GET";
            WebResponse webResponse = null;
            try
            {
                webResponse = this.GetWebResponse(webRequest);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new WebException(Res.GetString("ThereWasAnErrorDownloading0", new object[] { url }), exception);
            }
            HttpWebResponse response2 = webResponse as HttpWebResponse;
            if ((response2 != null) && (response2.StatusCode != HttpStatusCode.OK))
            {
                string message = RequestResponseUtils.CreateResponseExceptionString(response2);
                throw new WebException(Res.GetString("ThereWasAnErrorDownloading0", new object[] { url }), new WebException(message, null, WebExceptionStatus.ProtocolError, webResponse));
            }
            Stream responseStream = webResponse.GetResponseStream();
            try
            {
                url = webResponse.ResponseUri.ToString();
                contentType = webResponse.ContentType;
                if ((webResponse.ResponseUri.Scheme == Uri.UriSchemeFtp) || (webResponse.ResponseUri.Scheme == Uri.UriSchemeFile))
                {
                    string str2;
                    int num = webResponse.ResponseUri.AbsolutePath.LastIndexOf('.');
                    if (((num != -1) && ((str2 = webResponse.ResponseUri.AbsolutePath.Substring(num + 1).ToLower(CultureInfo.InvariantCulture)) != null)) && (((str2 == "xml") || (str2 == "wsdl")) || ((str2 == "xsd") || (str2 == "disco"))))
                    {
                        contentType = "text/xml";
                    }
                }
                stream2 = RequestResponseUtils.StreamToMemoryStream(responseStream);
            }
            finally
            {
                responseStream.Close();
            }
            return stream2;
        }

        internal void FixupReferences()
        {
            foreach (DiscoveryReference reference in this.References.Values)
            {
                reference.LoadExternals(this.InlinedSchemas);
            }
            foreach (string str in this.InlinedSchemas.Keys)
            {
                this.Documents.Remove(str);
            }
        }

        private static string GetRelativePath(string fullPath, string relativeTo)
        {
            string directoryName = Path.GetDirectoryName(Path.GetFullPath(relativeTo));
            string str2 = "";
            while (directoryName.Length > 0)
            {
                if ((directoryName.Length <= fullPath.Length) && (string.Compare(directoryName, fullPath.Substring(0, directoryName.Length), StringComparison.OrdinalIgnoreCase) == 0))
                {
                    str2 = str2 + fullPath.Substring(directoryName.Length);
                    if (str2.StartsWith(@"\", StringComparison.Ordinal))
                    {
                        str2 = str2.Substring(1);
                    }
                    return str2;
                }
                str2 = str2 + @"..\";
                if (directoryName.Length < 2)
                {
                    return fullPath;
                }
                int num = directoryName.LastIndexOf('\\', directoryName.Length - 2);
                directoryName = directoryName.Substring(0, num + 1);
            }
            return fullPath;
        }

        private static string GetUniqueFilename(Hashtable filenames, string path)
        {
            if (IsFilenameInUse(filenames, path))
            {
                string extension = Path.GetExtension(path);
                string str2 = path.Substring(0, path.Length - extension.Length);
                int num = 0;
                do
                {
                    path = str2 + num.ToString(CultureInfo.InvariantCulture) + extension;
                    num++;
                }
                while (IsFilenameInUse(filenames, path));
            }
            AddFilename(filenames, path);
            return path;
        }

        private static bool IsFilenameInUse(Hashtable filenames, string path)
        {
            return (filenames[path.ToLower(CultureInfo.InvariantCulture)] != null);
        }

        [Obsolete("This method will be removed from a future version. The method call is no longer required for resource discovery", false), ComVisible(false)]
        public void LoadExternals()
        {
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public DiscoveryClientResultCollection ReadAll(string topLevelFilename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DiscoveryClientResultsFile));
            Stream stream = System.IO.File.OpenRead(topLevelFilename);
            string directoryName = Path.GetDirectoryName(topLevelFilename);
            DiscoveryClientResultsFile file = null;
            try
            {
                file = (DiscoveryClientResultsFile) serializer.Deserialize(stream);
                for (int i = 0; i < file.Results.Count; i++)
                {
                    if (file.Results[i] == null)
                    {
                        throw new InvalidOperationException(Res.GetString("WebNullRef"));
                    }
                    string referenceTypeName = file.Results[i].ReferenceTypeName;
                    if ((referenceTypeName == null) || (referenceTypeName.Length == 0))
                    {
                        throw new InvalidOperationException(Res.GetString("WebRefInvalidAttribute", new object[] { "referenceType" }));
                    }
                    DiscoveryReference reference = (DiscoveryReference) Activator.CreateInstance(Type.GetType(referenceTypeName));
                    reference.ClientProtocol = this;
                    string url = file.Results[i].Url;
                    if ((url == null) || (url.Length == 0))
                    {
                        throw new InvalidOperationException(Res.GetString("WebRefInvalidAttribute2", new object[] { reference.GetType().FullName, "url" }));
                    }
                    reference.Url = url;
                    string filename = file.Results[i].Filename;
                    if ((filename == null) || (filename.Length == 0))
                    {
                        throw new InvalidOperationException(Res.GetString("WebRefInvalidAttribute2", new object[] { reference.GetType().FullName, "filename" }));
                    }
                    Stream stream2 = System.IO.File.OpenRead(Path.Combine(directoryName, file.Results[i].Filename));
                    try
                    {
                        this.Documents[reference.Url] = reference.ReadDocument(stream2);
                    }
                    finally
                    {
                        stream2.Close();
                    }
                    this.References[reference.Url] = reference;
                }
                this.ResolveAll();
            }
            finally
            {
                stream.Close();
            }
            return file.Results;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void ResolveAll()
        {
            this.Errors.Clear();
            int count = this.InlinedSchemas.Keys.Count;
            while (count != this.References.Count)
            {
                count = this.References.Count;
                DiscoveryReference[] array = new DiscoveryReference[this.References.Count];
                this.References.Values.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    DiscoveryReference reference = array[i];
                    if (reference is DiscoveryDocumentReference)
                    {
                        try
                        {
                            ((DiscoveryDocumentReference) reference).ResolveAll(true);
                        }
                        catch (Exception exception)
                        {
                            if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                            {
                                throw;
                            }
                            this.Errors[reference.Url] = exception;
                            if (Tracing.On)
                            {
                                Tracing.ExceptionCatch(TraceEventType.Warning, this, "ResolveAll", exception);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            reference.Resolve();
                        }
                        catch (Exception exception2)
                        {
                            if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                            {
                                throw;
                            }
                            this.Errors[reference.Url] = exception2;
                            if (Tracing.On)
                            {
                                Tracing.ExceptionCatch(TraceEventType.Warning, this, "ResolveAll", exception2);
                            }
                        }
                    }
                }
            }
            this.FixupReferences();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void ResolveOneLevel()
        {
            this.Errors.Clear();
            DiscoveryReference[] array = new DiscoveryReference[this.References.Count];
            this.References.Values.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                try
                {
                    array[i].Resolve();
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    this.Errors[array[i].Url] = exception;
                    if (Tracing.On)
                    {
                        Tracing.ExceptionCatch(TraceEventType.Warning, this, "ResolveOneLevel", exception);
                    }
                }
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public DiscoveryClientResultCollection WriteAll(string directory, string topLevelFilename)
        {
            DiscoveryClientResultsFile o = new DiscoveryClientResultsFile();
            Hashtable filenames = new Hashtable();
            string relativeTo = Path.Combine(directory, topLevelFilename);
            DictionaryEntry[] entryArray = new DictionaryEntry[this.Documents.Count + this.InlinedSchemas.Keys.Count];
            int num = 0;
            foreach (DictionaryEntry entry in this.Documents)
            {
                entryArray[num++] = entry;
            }
            foreach (DictionaryEntry entry2 in this.InlinedSchemas)
            {
                entryArray[num++] = entry2;
            }
            foreach (DictionaryEntry entry3 in entryArray)
            {
                string key = (string) entry3.Key;
                object document = entry3.Value;
                if (document != null)
                {
                    DiscoveryReference reference = this.References[key];
                    string uniqueFilename = (reference == null) ? DiscoveryReference.FilenameFromUrl(base.Url) : reference.DefaultFilename;
                    uniqueFilename = GetUniqueFilename(filenames, Path.GetFullPath(Path.Combine(directory, uniqueFilename)));
                    o.Results.Add(new DiscoveryClientResult((reference == null) ? null : reference.GetType(), key, GetRelativePath(uniqueFilename, relativeTo)));
                    Stream stream = System.IO.File.Create(uniqueFilename);
                    try
                    {
                        reference.WriteDocument(document, stream);
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
            }
            XmlSerializer serializer = new XmlSerializer(typeof(DiscoveryClientResultsFile));
            Stream stream2 = System.IO.File.Create(relativeTo);
            try
            {
                serializer.Serialize((TextWriter) new StreamWriter(stream2, new UTF8Encoding(false)), o);
            }
            finally
            {
                stream2.Close();
            }
            return o.Results;
        }

        public IList AdditionalInformation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.additionalInformation;
            }
        }

        public DiscoveryClientDocumentCollection Documents
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.documents;
            }
        }

        public DiscoveryExceptionDictionary Errors
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errors;
            }
        }

        internal Hashtable InlinedSchemas
        {
            get
            {
                return this.inlinedSchemas;
            }
        }

        public DiscoveryClientReferenceCollection References
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.references;
            }
        }

        public sealed class DiscoveryClientResultsFile
        {
            private DiscoveryClientResultCollection results = new DiscoveryClientResultCollection();

            public DiscoveryClientResultCollection Results
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.results;
                }
            }
        }
    }
}

