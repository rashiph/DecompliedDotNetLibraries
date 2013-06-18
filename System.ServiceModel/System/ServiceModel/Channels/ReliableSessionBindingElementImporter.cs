namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class ReliableSessionBindingElementImporter : IPolicyImportExtension
    {
        private static ReliableSessionBindingElement GetReliableSessionBindingElement(PolicyConversionContext context)
        {
            ReliableSessionBindingElement item = context.BindingElements.Find<ReliableSessionBindingElement>();
            if (item == null)
            {
                item = new ReliableSessionBindingElement();
                context.BindingElements.Add(item);
            }
            return item;
        }

        private static bool Is11Assertion(System.Xml.XmlNode node, string assertion)
        {
            return IsElement(node, "http://schemas.microsoft.com/ws-rx/wsrmp/200702", assertion);
        }

        private static bool IsElement(System.Xml.XmlNode node, string ns, string assertion)
        {
            if (assertion == null)
            {
                throw Fx.AssertAndThrow("Argument assertion cannot be null.");
            }
            return ((((node != null) && (node.NodeType == XmlNodeType.Element)) && (node.NamespaceURI == ns)) && (node.LocalName == assertion));
        }

        private static bool IsFeb2005Assertion(System.Xml.XmlNode node, string assertion)
        {
            return IsElement(node, "http://schemas.xmlsoap.org/ws/2005/02/rm/policy", assertion);
        }

        private static void ProcessReliableSession11Assertion(MetadataImporter importer, XmlElement element, ReliableSessionBindingElement settings)
        {
            settings.ReliableMessagingVersion = ReliableMessagingVersion.WSReliableMessaging11;
            IEnumerator nodes = element.ChildNodes.GetEnumerator();
            System.Xml.XmlNode node = SkipToNode(nodes);
            ProcessWsrm11Policy(importer, node, settings);
            node = SkipToNode(nodes);
            State inactivityTimeout = State.InactivityTimeout;
            while (node != null)
            {
                if ((inactivityTimeout == State.InactivityTimeout) && Is11Assertion(node, "InactivityTimeout"))
                {
                    SetInactivityTimeout(settings, ReadMillisecondsAttribute(node, true), node.LocalName);
                    inactivityTimeout = State.AcknowledgementInterval;
                    node = SkipToNode(nodes);
                }
                else
                {
                    if (Is11Assertion(node, "AcknowledgementInterval"))
                    {
                        SetAcknowledgementInterval(settings, ReadMillisecondsAttribute(node, true), node.LocalName);
                        return;
                    }
                    if (inactivityTimeout == State.AcknowledgementInterval)
                    {
                        return;
                    }
                    node = SkipToNode(nodes);
                }
            }
        }

        private static void ProcessReliableSessionFeb2005Assertion(XmlElement element, ReliableSessionBindingElement settings)
        {
            settings.ReliableMessagingVersion = ReliableMessagingVersion.WSReliableMessagingFebruary2005;
            IEnumerator nodes = element.ChildNodes.GetEnumerator();
            System.Xml.XmlNode node = SkipToNode(nodes);
            if (IsFeb2005Assertion(node, "InactivityTimeout"))
            {
                SetInactivityTimeout(settings, ReadMillisecondsAttribute(node, true), node.LocalName);
                node = SkipToNode(nodes);
            }
            if (IsFeb2005Assertion(node, "BaseRetransmissionInterval"))
            {
                ReadMillisecondsAttribute(node, false);
                node = SkipToNode(nodes);
            }
            if (IsFeb2005Assertion(node, "ExponentialBackoff"))
            {
                node = SkipToNode(nodes);
            }
            if (IsFeb2005Assertion(node, "AcknowledgementInterval"))
            {
                SetAcknowledgementInterval(settings, ReadMillisecondsAttribute(node, true), node.LocalName);
            }
        }

        private static void ProcessWsrm11Policy(MetadataImporter importer, System.Xml.XmlNode node, ReliableSessionBindingElement settings)
        {
            XmlElement element = ThrowIfNotPolicyElement(node, ReliableMessagingVersion.WSReliableMessaging11);
            IEnumerable<IEnumerable<XmlElement>> enumerable = importer.NormalizePolicy(new XmlElement[] { element });
            List<Wsrm11PolicyAlternative> list = new List<Wsrm11PolicyAlternative>();
            foreach (IEnumerable<XmlElement> enumerable2 in enumerable)
            {
                Wsrm11PolicyAlternative item = Wsrm11PolicyAlternative.ImportAlternative(importer, enumerable2);
                list.Add(item);
            }
            if (list.Count != 0)
            {
                foreach (Wsrm11PolicyAlternative alternative2 in list)
                {
                    if (alternative2.HasValidPolicy)
                    {
                        alternative2.TransferSettings(settings);
                        return;
                    }
                }
                Wsrm11PolicyAlternative.ThrowInvalidBindingException();
            }
        }

        private static TimeSpan ReadMillisecondsAttribute(System.Xml.XmlNode wsrmNode, bool convertToTimeSpan)
        {
            TimeSpan span;
            System.Xml.XmlAttribute attribute = wsrmNode.Attributes["Milliseconds"];
            if (attribute == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("RequiredAttributeIsMissing", new object[] { "Milliseconds", wsrmNode.LocalName, "RMAssertion" })));
            }
            ulong num = 0L;
            Exception innerException = null;
            try
            {
                num = XmlConvert.ToUInt64(attribute.Value);
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            if (innerException != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("RequiredMillisecondsAttributeIncorrect", new object[] { wsrmNode.LocalName }), innerException));
            }
            if (!convertToTimeSpan)
            {
                return new TimeSpan();
            }
            try
            {
                span = TimeSpan.FromMilliseconds(Convert.ToDouble(num));
            }
            catch (OverflowException exception4)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("MillisecondsNotConvertibleToBindingRange", new object[] { wsrmNode.LocalName }), exception4));
            }
            return span;
        }

        private static void SetAcknowledgementInterval(ReliableSessionBindingElement settings, TimeSpan acknowledgementInterval, string localName)
        {
            try
            {
                settings.AcknowledgementInterval = acknowledgementInterval;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("MillisecondsNotConvertibleToBindingRange", new object[] { localName }), exception));
            }
        }

        private static void SetInactivityTimeout(ReliableSessionBindingElement settings, TimeSpan inactivityTimeout, string localName)
        {
            try
            {
                settings.InactivityTimeout = inactivityTimeout;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("MillisecondsNotConvertibleToBindingRange", new object[] { localName }), exception));
            }
        }

        private static bool ShouldSkipNodeType(XmlNodeType type)
        {
            if (((type != XmlNodeType.Comment) && (type != XmlNodeType.SignificantWhitespace)) && (type != XmlNodeType.Whitespace))
            {
                return (type == XmlNodeType.Notation);
            }
            return true;
        }

        private static System.Xml.XmlNode SkipToNode(IEnumerator nodes)
        {
            while (nodes.MoveNext())
            {
                System.Xml.XmlNode current = (System.Xml.XmlNode) nodes.Current;
                if (!ShouldSkipNodeType(current.NodeType))
                {
                    return current;
                }
            }
            return null;
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            bool flag = false;
            XmlElement element = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(), "RMAssertion", "http://schemas.xmlsoap.org/ws/2005/02/rm/policy", true);
            if (element != null)
            {
                ProcessReliableSessionFeb2005Assertion(element, GetReliableSessionBindingElement(context));
                flag = true;
            }
            element = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(), "RMAssertion", "http://docs.oasis-open.org/ws-rx/wsrmp/200702", true);
            if (element != null)
            {
                if (flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("MultipleVersionsFoundInPolicy", new object[] { "RMAssertion" })));
                }
                ProcessReliableSession11Assertion(importer, element, GetReliableSessionBindingElement(context));
            }
        }

        private static XmlElement ThrowIfNotPolicyElement(System.Xml.XmlNode node, ReliableMessagingVersion reliableMessagingVersion)
        {
            string assertion = "Policy";
            if (!IsElement(node, "http://schemas.xmlsoap.org/ws/2004/09/policy", assertion) && !IsElement(node, "http://www.w3.org/ns/ws-policy", assertion))
            {
                string str2 = (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) ? "wsrm" : "wsrmp";
                string message = (node == null) ? System.ServiceModel.SR.GetString("ElementRequired", new object[] { str2, "RMAssertion", "wsp", "Policy" }) : System.ServiceModel.SR.GetString("ElementFound", new object[] { str2, "RMAssertion", "wsp", "Policy", node.LocalName, node.NamespaceURI });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(message));
            }
            return (XmlElement) node;
        }

        private enum State
        {
            Security,
            DeliveryAssurance,
            Assurance,
            Order,
            InactivityTimeout,
            AcknowledgementInterval,
            Done
        }

        private class Wsrm11PolicyAlternative
        {
            private bool hasValidPolicy = true;
            private bool isOrdered;

            public static ReliableSessionBindingElementImporter.Wsrm11PolicyAlternative ImportAlternative(MetadataImporter importer, IEnumerable<XmlElement> alternative)
            {
                ReliableSessionBindingElementImporter.State security = ReliableSessionBindingElementImporter.State.Security;
                ReliableSessionBindingElementImporter.Wsrm11PolicyAlternative alternative2 = new ReliableSessionBindingElementImporter.Wsrm11PolicyAlternative();
                foreach (XmlElement element in alternative)
                {
                    switch (security)
                    {
                        case ReliableSessionBindingElementImporter.State.Security:
                            security = ReliableSessionBindingElementImporter.State.DeliveryAssurance;
                            if (alternative2.TryImportSequenceSTR(element))
                            {
                                continue;
                            }
                            break;

                        case ReliableSessionBindingElementImporter.State.DeliveryAssurance:
                            security = ReliableSessionBindingElementImporter.State.Done;
                            if (alternative2.TryImportDeliveryAssurance(importer, element))
                            {
                                continue;
                            }
                            break;
                    }
                    string message = System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { element.LocalName, element.NodeType, "RMAssertion" });
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(message));
                }
                return alternative2;
            }

            public static void ThrowInvalidBindingException()
            {
                string message = System.ServiceModel.SR.GetString("AssertionNotSupported", new object[] { "wsrmp", "SequenceTransportSecurity" });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(message));
            }

            public void TransferSettings(ReliableSessionBindingElement settings)
            {
                settings.Ordered = this.isOrdered;
            }

            private bool TryImportDeliveryAssurance(MetadataImporter importer, XmlElement node)
            {
                string ns = "http://docs.oasis-open.org/ws-rx/wsrmp/200702";
                if (!ReliableSessionBindingElementImporter.IsElement(node, ns, "DeliveryAssurance"))
                {
                    return false;
                }
                IEnumerator nodes = node.ChildNodes.GetEnumerator();
                XmlElement element = ReliableSessionBindingElementImporter.ThrowIfNotPolicyElement(ReliableSessionBindingElementImporter.SkipToNode(nodes), ReliableMessagingVersion.WSReliableMessaging11);
                foreach (IEnumerable<XmlElement> enumerable2 in importer.NormalizePolicy(new XmlElement[] { element }))
                {
                    ReliableSessionBindingElementImporter.State assurance = ReliableSessionBindingElementImporter.State.Assurance;
                    foreach (XmlElement element2 in enumerable2)
                    {
                        switch (assurance)
                        {
                            case ReliableSessionBindingElementImporter.State.Assurance:
                            {
                                assurance = ReliableSessionBindingElementImporter.State.Order;
                                if ((!ReliableSessionBindingElementImporter.IsElement(element2, ns, "ExactlyOnce") && !ReliableSessionBindingElementImporter.IsElement(element2, ns, "AtMostOnce")) && !ReliableSessionBindingElementImporter.IsElement(element2, ns, "AtMostOnce"))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("DeliveryAssuranceRequired", new object[] { ns, element2.LocalName, element2.NamespaceURI })));
                                }
                                continue;
                            }
                            case ReliableSessionBindingElementImporter.State.Order:
                                assurance = ReliableSessionBindingElementImporter.State.Done;
                                if (ReliableSessionBindingElementImporter.IsElement(element2, ns, "InOrder"))
                                {
                                    if (!this.isOrdered)
                                    {
                                        this.isOrdered = true;
                                    }
                                    continue;
                                }
                                break;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { element2.LocalName, element2.NodeType, "DeliveryAssurance" })));
                    }
                    if (assurance == ReliableSessionBindingElementImporter.State.Assurance)
                    {
                        string message = System.ServiceModel.SR.GetString("DeliveryAssuranceRequiredNothingFound", new object[] { ns });
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(message));
                    }
                }
                System.Xml.XmlNode node2 = ReliableSessionBindingElementImporter.SkipToNode(nodes);
                if (node2 != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { node2.LocalName, node2.NodeType, node.LocalName })));
                }
                return true;
            }

            private bool TryImportSequenceSTR(XmlElement node)
            {
                string ns = "http://docs.oasis-open.org/ws-rx/wsrmp/200702";
                if (ReliableSessionBindingElementImporter.IsElement(node, ns, "SequenceSTR"))
                {
                    return true;
                }
                if (ReliableSessionBindingElementImporter.IsElement(node, ns, "SequenceTransportSecurity"))
                {
                    this.hasValidPolicy = false;
                    return true;
                }
                return false;
            }

            public bool HasValidPolicy
            {
                get
                {
                    return this.hasValidPolicy;
                }
            }
        }
    }
}

