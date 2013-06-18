namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlConditions
    {
        private readonly ImmutableCollection<SamlCondition> conditions;
        private bool isReadOnly;
        private DateTime notBefore;
        private DateTime notOnOrAfter;

        public SamlConditions()
        {
            this.conditions = new ImmutableCollection<SamlCondition>();
            this.notBefore = System.IdentityModel.SecurityUtils.MinUtcDateTime;
            this.notOnOrAfter = System.IdentityModel.SecurityUtils.MaxUtcDateTime;
        }

        public SamlConditions(DateTime notBefore, DateTime notOnOrAfter) : this(notBefore, notOnOrAfter, null)
        {
        }

        public SamlConditions(DateTime notBefore, DateTime notOnOrAfter, IEnumerable<SamlCondition> conditions)
        {
            this.conditions = new ImmutableCollection<SamlCondition>();
            this.notBefore = System.IdentityModel.SecurityUtils.MinUtcDateTime;
            this.notOnOrAfter = System.IdentityModel.SecurityUtils.MaxUtcDateTime;
            this.notBefore = notBefore.ToUniversalTime();
            this.notOnOrAfter = notOnOrAfter.ToUniversalTime();
            if (conditions != null)
            {
                foreach (SamlCondition condition in conditions)
                {
                    if (condition == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.Condition.Value }));
                    }
                    this.conditions.Add(condition);
                }
            }
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.conditions.MakeReadOnly();
                foreach (SamlCondition condition in this.conditions)
                {
                    condition.MakeReadOnly();
                }
                this.isReadOnly = true;
            }
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            string attribute = reader.GetAttribute(samlDictionary.NotBefore, null);
            if (!string.IsNullOrEmpty(attribute))
            {
                this.notBefore = DateTime.ParseExact(attribute, SamlConstants.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
            }
            attribute = reader.GetAttribute(samlDictionary.NotOnOrAfter, null);
            if (!string.IsNullOrEmpty(attribute))
            {
                this.notOnOrAfter = DateTime.ParseExact(attribute, SamlConstants.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
            }
            if (reader.IsEmptyElement)
            {
                reader.MoveToContent();
                reader.Read();
            }
            else
            {
                reader.MoveToContent();
                reader.Read();
                while (reader.IsStartElement())
                {
                    SamlCondition item = samlSerializer.LoadCondition(reader, keyInfoSerializer, outOfBandTokenResolver);
                    if (item == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLUnableToLoadCondtion")));
                    }
                    this.conditions.Add(item);
                }
                reader.MoveToContent();
                reader.ReadEndElement();
            }
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.Conditions, samlDictionary.Namespace);
            if (this.notBefore != System.IdentityModel.SecurityUtils.MinUtcDateTime)
            {
                writer.WriteStartAttribute(samlDictionary.NotBefore, null);
                writer.WriteString(this.notBefore.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", DateTimeFormatInfo.InvariantInfo));
                writer.WriteEndAttribute();
            }
            if (this.notOnOrAfter != System.IdentityModel.SecurityUtils.MaxUtcDateTime)
            {
                writer.WriteStartAttribute(samlDictionary.NotOnOrAfter, null);
                writer.WriteString(this.notOnOrAfter.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", DateTimeFormatInfo.InvariantInfo));
                writer.WriteEndAttribute();
            }
            for (int i = 0; i < this.conditions.Count; i++)
            {
                this.conditions[i].WriteXml(writer, samlSerializer, keyInfoSerializer);
            }
            writer.WriteEndElement();
        }

        public IList<SamlCondition> Conditions
        {
            get
            {
                return this.conditions;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public DateTime NotBefore
        {
            get
            {
                return this.notBefore;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.notBefore = value;
            }
        }

        public DateTime NotOnOrAfter
        {
            get
            {
                return this.notOnOrAfter;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.notOnOrAfter = value;
            }
        }
    }
}

