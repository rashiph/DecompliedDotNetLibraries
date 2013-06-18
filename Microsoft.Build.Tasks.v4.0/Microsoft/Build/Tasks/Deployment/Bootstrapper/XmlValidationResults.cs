namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;
    using System.Xml.Schema;

    internal class XmlValidationResults
    {
        private string filePath;
        private ArrayList validationErrors;
        private ArrayList validationWarnings;

        public XmlValidationResults(string filePath)
        {
            this.filePath = filePath;
            this.validationErrors = new ArrayList();
            this.validationWarnings = new ArrayList();
        }

        public void SchemaValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                this.validationErrors.Add(e.Message);
            }
            else
            {
                this.validationWarnings.Add(e.Message);
            }
        }

        public string FilePath
        {
            get
            {
                return this.filePath;
            }
        }

        public string[] ValidationErrors
        {
            get
            {
                string[] array = new string[this.validationErrors.Count];
                this.validationErrors.CopyTo(array);
                return array;
            }
        }

        public bool ValidationPassed
        {
            get
            {
                return ((this.validationErrors.Count == 0) && (this.validationWarnings.Count == 0));
            }
        }

        public string[] ValidationWarnings
        {
            get
            {
                string[] array = new string[this.validationWarnings.Count];
                this.validationWarnings.CopyTo(array);
                return array;
            }
        }
    }
}

