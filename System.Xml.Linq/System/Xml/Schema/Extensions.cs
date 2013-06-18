namespace System.Xml.Schema
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    public static class Extensions
    {
        public static IXmlSchemaInfo GetSchemaInfo(this XAttribute source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source.Annotation<IXmlSchemaInfo>();
        }

        public static IXmlSchemaInfo GetSchemaInfo(this XElement source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source.Annotation<IXmlSchemaInfo>();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void Validate(this XDocument source, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
        {
            source.Validate(schemas, validationEventHandler, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void Validate(this XAttribute source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
        {
            source.Validate(partialValidationType, schemas, validationEventHandler, false);
        }

        public static void Validate(this XDocument source, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (schemas == null)
            {
                throw new ArgumentNullException("schemas");
            }
            new XNodeValidator(schemas, validationEventHandler).Validate(source, null, addSchemaInfo);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void Validate(this XElement source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
        {
            source.Validate(partialValidationType, schemas, validationEventHandler, false);
        }

        public static void Validate(this XAttribute source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (partialValidationType == null)
            {
                throw new ArgumentNullException("partialValidationType");
            }
            if (schemas == null)
            {
                throw new ArgumentNullException("schemas");
            }
            new XNodeValidator(schemas, validationEventHandler).Validate(source, partialValidationType, addSchemaInfo);
        }

        public static void Validate(this XElement source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (partialValidationType == null)
            {
                throw new ArgumentNullException("partialValidationType");
            }
            if (schemas == null)
            {
                throw new ArgumentNullException("schemas");
            }
            new XNodeValidator(schemas, validationEventHandler).Validate(source, partialValidationType, addSchemaInfo);
        }
    }
}

