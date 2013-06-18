namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;

    public abstract class ResolveProjectBase : TaskExtension
    {
        private const string attributeAbsolutePath = "AbsolutePath";
        private const string attributeProject = "Project";
        private Dictionary<string, XmlElement> cachedProjectElements = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, XmlElement> cachedProjectElementsByAbsolutePath = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
        private ITaskItem[] projectReferences;

        protected ResolveProjectBase()
        {
        }

        internal void CacheProjectElementsFromXml(string xmlString)
        {
            XmlDocument document = null;
            if (!string.IsNullOrEmpty(xmlString))
            {
                document = new XmlDocument();
                document.LoadXml(xmlString);
            }
            if ((document != null) && (document.DocumentElement != null))
            {
                foreach (XmlElement element in document.DocumentElement.ChildNodes)
                {
                    string attribute = element.GetAttribute("Project");
                    string str2 = element.GetAttribute("AbsolutePath");
                    if (!string.IsNullOrEmpty(attribute))
                    {
                        this.cachedProjectElements[attribute] = element;
                        if (!string.IsNullOrEmpty(str2))
                        {
                            this.cachedProjectElementsByAbsolutePath[str2] = element;
                        }
                    }
                }
            }
        }

        protected XmlElement GetProjectElement(ITaskItem projectRef)
        {
            string metadata = projectRef.GetMetadata("Project");
            XmlElement element = null;
            if (this.cachedProjectElements.TryGetValue(metadata, out element) && (element != null))
            {
                return element;
            }
            string key = projectRef.GetMetadata("FullPath");
            if (this.cachedProjectElementsByAbsolutePath.TryGetValue(key, out element) && (element != null))
            {
                return element;
            }
            return null;
        }

        protected string GetProjectItem(ITaskItem projectRef)
        {
            XmlElement projectElement = this.GetProjectElement(projectRef);
            if (projectElement == null)
            {
                return null;
            }
            return projectElement.InnerText;
        }

        internal bool VerifyProjectReferenceItems(ITaskItem[] references, bool treatAsError)
        {
            bool flag = true;
            foreach (ITaskItem item in references)
            {
                string str;
                if (!this.VerifyReferenceAttributes(item, out str))
                {
                    if (treatAsError)
                    {
                        base.Log.LogErrorWithCodeFromResources("General.MissingOrUnknownProjectReferenceAttribute", new object[] { item.ItemSpec, str });
                        flag = false;
                    }
                    else
                    {
                        base.Log.LogWarningWithCodeFromResources("General.MissingOrUnknownProjectReferenceAttribute", new object[] { item.ItemSpec, str });
                    }
                }
            }
            return flag;
        }

        internal bool VerifyReferenceAttributes(ITaskItem reference, out string missingAttribute)
        {
            missingAttribute = "Project";
            string metadata = reference.GetMetadata(missingAttribute);
            if (metadata.Length > 0)
            {
                try
                {
                    new Guid(metadata);
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            missingAttribute = null;
            return true;
        }

        [Required]
        public ITaskItem[] ProjectReferences
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.projectReferences, "projectReferences");
                return this.projectReferences;
            }
            set
            {
                this.projectReferences = value;
            }
        }
    }
}

