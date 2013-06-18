namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;

    internal interface IWrappedBodyTypeGenerator
    {
        void AddMemberAttributes(XmlName messageName, MessagePartDescription part, CodeAttributeDeclarationCollection attributesImported, CodeAttributeDeclarationCollection typeAttributes, CodeAttributeDeclarationCollection fieldAttributes);
        void AddTypeAttributes(string messageName, string typeNS, CodeAttributeDeclarationCollection typeAttributes, bool isEncoded);
        void ValidateForParameterMode(OperationDescription operationDescription);
    }
}

