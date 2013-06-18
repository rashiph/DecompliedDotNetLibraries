namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Xml.Serialization;

    internal class SoapParameter
    {
        internal CodeFlags codeFlags;
        internal XmlMemberMapping mapping;
        internal string name;
        internal string specifiedName;

        internal static CodeFlags[] GetCodeFlags(IList parameters, int specifiedCount)
        {
            CodeFlags[] codeFlags = new CodeFlags[parameters.Count + specifiedCount];
            GetCodeFlags(parameters, codeFlags, 0, specifiedCount);
            return codeFlags;
        }

        internal static void GetCodeFlags(IList parameters, CodeFlags[] codeFlags, int start, int specifiedCount)
        {
            int num = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                codeFlags[(i + start) + num] = ((SoapParameter) parameters[i]).codeFlags;
                if (((SoapParameter) parameters[i]).mapping.CheckSpecified)
                {
                    num++;
                    codeFlags[(i + start) + num] = ((SoapParameter) parameters[i]).codeFlags;
                }
            }
        }

        internal static string[] GetNames(IList parameters, int specifiedCount)
        {
            string[] names = new string[parameters.Count + specifiedCount];
            GetNames(parameters, names, 0, specifiedCount);
            return names;
        }

        internal static void GetNames(IList parameters, string[] names, int start, int specifiedCount)
        {
            int num = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                names[(i + start) + num] = ((SoapParameter) parameters[i]).name;
                if (((SoapParameter) parameters[i]).mapping.CheckSpecified)
                {
                    num++;
                    names[(i + start) + num] = ((SoapParameter) parameters[i]).specifiedName;
                }
            }
        }

        internal static string[] GetTypeFullNames(IList parameters, int specifiedCount, CodeDomProvider codeProvider)
        {
            string[] typeFullNames = new string[parameters.Count + specifiedCount];
            GetTypeFullNames(parameters, typeFullNames, 0, specifiedCount, codeProvider);
            return typeFullNames;
        }

        internal static void GetTypeFullNames(IList parameters, string[] typeFullNames, int start, int specifiedCount, CodeDomProvider codeProvider)
        {
            int num = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                typeFullNames[(i + start) + num] = WebCodeGenerator.FullTypeName(((SoapParameter) parameters[i]).mapping, codeProvider);
                if (((SoapParameter) parameters[i]).mapping.CheckSpecified)
                {
                    num++;
                    typeFullNames[(i + start) + num] = typeof(bool).FullName;
                }
            }
        }

        internal bool IsByRef
        {
            get
            {
                return ((this.codeFlags & CodeFlags.IsByRef) != ((CodeFlags) 0));
            }
        }

        internal bool IsOut
        {
            get
            {
                return ((this.codeFlags & CodeFlags.IsOut) != ((CodeFlags) 0));
            }
        }
    }
}

