namespace System.Xml.Xsl
{
    using System;
    using System.Collections;
    using System.Xml;

    public class XsltArgumentList
    {
        private Hashtable extensions = new Hashtable();
        private Hashtable parameters = new Hashtable();

        public event XsltMessageEncounteredEventHandler XsltMessageEncountered;

        public void AddExtensionObject(string namespaceUri, object extension)
        {
            CheckArgumentNull(namespaceUri, "namespaceUri");
            CheckArgumentNull(extension, "extension");
            this.extensions.Add(namespaceUri, extension);
        }

        public void AddParam(string name, string namespaceUri, object parameter)
        {
            CheckArgumentNull(name, "name");
            CheckArgumentNull(namespaceUri, "namespaceUri");
            CheckArgumentNull(parameter, "parameter");
            XmlQualifiedName key = new XmlQualifiedName(name, namespaceUri);
            key.Verify();
            this.parameters.Add(key, parameter);
        }

        private static void CheckArgumentNull(object param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public void Clear()
        {
            this.parameters.Clear();
            this.extensions.Clear();
            this.xsltMessageEncountered = null;
        }

        public object GetExtensionObject(string namespaceUri)
        {
            return this.extensions[namespaceUri];
        }

        public object GetParam(string name, string namespaceUri)
        {
            return this.parameters[new XmlQualifiedName(name, namespaceUri)];
        }

        public object RemoveExtensionObject(string namespaceUri)
        {
            object obj2 = this.extensions[namespaceUri];
            this.extensions.Remove(namespaceUri);
            return obj2;
        }

        public object RemoveParam(string name, string namespaceUri)
        {
            XmlQualifiedName key = new XmlQualifiedName(name, namespaceUri);
            object obj2 = this.parameters[key];
            this.parameters.Remove(key);
            return obj2;
        }
    }
}

