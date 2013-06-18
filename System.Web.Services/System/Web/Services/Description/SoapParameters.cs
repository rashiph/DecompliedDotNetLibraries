namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Xml.Serialization;

    internal class SoapParameters
    {
        private int checkSpecifiedCount;
        private int inCheckSpecifiedCount;
        private ArrayList inParameters = new ArrayList();
        private int outCheckSpecifiedCount;
        private ArrayList outParameters = new ArrayList();
        private ArrayList parameters = new ArrayList();
        private XmlMemberMapping ret;

        internal SoapParameters(XmlMembersMapping request, XmlMembersMapping response, string[] parameterOrder, CodeIdentifiers identifiers)
        {
            ArrayList mappingsList = new ArrayList();
            ArrayList list2 = new ArrayList();
            AddMappings(mappingsList, request);
            if (response != null)
            {
                AddMappings(list2, response);
            }
            if (parameterOrder != null)
            {
                for (int i = 0; i < parameterOrder.Length; i++)
                {
                    string elementName = parameterOrder[i];
                    XmlMemberMapping requestMapping = FindMapping(mappingsList, elementName);
                    SoapParameter parameter = new SoapParameter();
                    if (requestMapping != null)
                    {
                        if (RemoveByRefMapping(list2, requestMapping))
                        {
                            parameter.codeFlags = CodeFlags.IsByRef;
                        }
                        parameter.mapping = requestMapping;
                        mappingsList.Remove(requestMapping);
                        this.AddParameter(parameter);
                    }
                    else
                    {
                        XmlMemberMapping mapping2 = FindMapping(list2, elementName);
                        if (mapping2 != null)
                        {
                            parameter.codeFlags = CodeFlags.IsOut;
                            parameter.mapping = mapping2;
                            list2.Remove(mapping2);
                            this.AddParameter(parameter);
                        }
                    }
                }
            }
            foreach (XmlMemberMapping mapping3 in mappingsList)
            {
                SoapParameter parameter2 = new SoapParameter();
                if (RemoveByRefMapping(list2, mapping3))
                {
                    parameter2.codeFlags = CodeFlags.IsByRef;
                }
                parameter2.mapping = mapping3;
                this.AddParameter(parameter2);
            }
            if (list2.Count > 0)
            {
                if (!((XmlMemberMapping) list2[0]).CheckSpecified)
                {
                    this.ret = (XmlMemberMapping) list2[0];
                    list2.RemoveAt(0);
                }
                foreach (XmlMemberMapping mapping4 in list2)
                {
                    SoapParameter parameter3 = new SoapParameter {
                        mapping = mapping4,
                        codeFlags = CodeFlags.IsOut
                    };
                    this.AddParameter(parameter3);
                }
            }
            foreach (SoapParameter parameter4 in this.parameters)
            {
                parameter4.name = identifiers.MakeUnique(CodeIdentifier.MakeValid(parameter4.mapping.MemberName));
            }
        }

        private static void AddMappings(ArrayList mappingsList, XmlMembersMapping mappings)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                mappingsList.Add(mappings[i]);
            }
        }

        private void AddParameter(SoapParameter parameter)
        {
            this.parameters.Add(parameter);
            if (parameter.mapping.CheckSpecified)
            {
                this.checkSpecifiedCount++;
            }
            if (parameter.IsByRef)
            {
                this.inParameters.Add(parameter);
                this.outParameters.Add(parameter);
                if (parameter.mapping.CheckSpecified)
                {
                    this.inCheckSpecifiedCount++;
                    this.outCheckSpecifiedCount++;
                }
            }
            else if (parameter.IsOut)
            {
                this.outParameters.Add(parameter);
                if (parameter.mapping.CheckSpecified)
                {
                    this.outCheckSpecifiedCount++;
                }
            }
            else
            {
                this.inParameters.Add(parameter);
                if (parameter.mapping.CheckSpecified)
                {
                    this.inCheckSpecifiedCount++;
                }
            }
        }

        private static XmlMemberMapping FindMapping(ArrayList mappingsList, string elementName)
        {
            foreach (XmlMemberMapping mapping in mappingsList)
            {
                if (mapping.ElementName == elementName)
                {
                    return mapping;
                }
            }
            return null;
        }

        private static bool RemoveByRefMapping(ArrayList responseList, XmlMemberMapping requestMapping)
        {
            XmlMemberMapping mapping = FindMapping(responseList, requestMapping.ElementName);
            if (mapping == null)
            {
                return false;
            }
            if (requestMapping.TypeFullName != mapping.TypeFullName)
            {
                return false;
            }
            if (requestMapping.Namespace != mapping.Namespace)
            {
                return false;
            }
            if (requestMapping.MemberName != mapping.MemberName)
            {
                return false;
            }
            responseList.Remove(mapping);
            return true;
        }

        internal int CheckSpecifiedCount
        {
            get
            {
                return this.checkSpecifiedCount;
            }
        }

        internal int InCheckSpecifiedCount
        {
            get
            {
                return this.inCheckSpecifiedCount;
            }
        }

        internal IList InParameters
        {
            get
            {
                return this.inParameters;
            }
        }

        internal int OutCheckSpecifiedCount
        {
            get
            {
                return this.outCheckSpecifiedCount;
            }
        }

        internal IList OutParameters
        {
            get
            {
                return this.outParameters;
            }
        }

        internal IList Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        internal XmlMemberMapping Return
        {
            get
            {
                return this.ret;
            }
        }
    }
}

