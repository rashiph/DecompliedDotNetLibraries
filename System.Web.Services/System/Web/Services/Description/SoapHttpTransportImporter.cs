namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.Web.Services;
    using System.Web.Services.Protocols;

    internal class SoapHttpTransportImporter : SoapTransportImporter
    {
        public override void ImportClass()
        {
            SoapAddressBinding binding = (base.ImportContext.Port == null) ? null : ((SoapAddressBinding) base.ImportContext.Port.Extensions.Find(typeof(SoapAddressBinding)));
            if (base.ImportContext.Style == ServiceDescriptionImportStyle.Client)
            {
                base.ImportContext.CodeTypeDeclaration.BaseTypes.Add(typeof(SoapHttpClientProtocol).FullName);
                CodeConstructor ctor = WebCodeGenerator.AddConstructor(base.ImportContext.CodeTypeDeclaration, new string[0], new string[0], null, CodeFlags.IsPublic);
                ctor.Comments.Add(new CodeCommentStatement(Res.GetString("CodeRemarks"), true));
                bool flag = true;
                if (base.ImportContext is Soap12ProtocolImporter)
                {
                    flag = false;
                    CodeTypeReferenceExpression targetObject = new CodeTypeReferenceExpression(typeof(SoapProtocolVersion));
                    CodeFieldReferenceExpression right = new CodeFieldReferenceExpression(targetObject, Enum.Format(typeof(SoapProtocolVersion), SoapProtocolVersion.Soap12, "G"));
                    CodePropertyReferenceExpression left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "SoapVersion");
                    CodeAssignStatement statement = new CodeAssignStatement(left, right);
                    ctor.Statements.Add(statement);
                }
                ServiceDescription serviceDescription = base.ImportContext.Binding.ServiceDescription;
                string url = (binding != null) ? binding.Location : null;
                string appSettingUrlKey = serviceDescription.AppSettingUrlKey;
                string appSettingBaseUrl = serviceDescription.AppSettingBaseUrl;
                ProtocolImporterUtil.GenerateConstructorStatements(ctor, url, appSettingUrlKey, appSettingBaseUrl, flag && !base.ImportContext.IsEncodedBinding);
            }
            else if (base.ImportContext.Style == ServiceDescriptionImportStyle.Server)
            {
                base.ImportContext.CodeTypeDeclaration.BaseTypes.Add(typeof(WebService).FullName);
            }
        }

        public override bool IsSupportedTransport(string transport)
        {
            return (transport == "http://schemas.xmlsoap.org/soap/http");
        }
    }
}

