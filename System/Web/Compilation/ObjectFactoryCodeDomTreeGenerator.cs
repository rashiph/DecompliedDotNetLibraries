namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.UI;

    internal class ObjectFactoryCodeDomTreeGenerator
    {
        private System.CodeDom.CodeCompileUnit _codeCompileUnit;
        private CodeTypeDeclaration _factoryClass;
        private const string factoryClassNameBase = "FastObjectFactory_";
        private const string factoryFullClassNameBase = "__ASP.FastObjectFactory_";

        internal ObjectFactoryCodeDomTreeGenerator(string outputAssemblyName)
        {
            CodeConstructor constructor;
            this._codeCompileUnit = new System.CodeDom.CodeCompileUnit();
            CodeNamespace namespace2 = new CodeNamespace("__ASP");
            this._codeCompileUnit.Namespaces.Add(namespace2);
            string name = "FastObjectFactory_" + Util.MakeValidTypeNameFromString(outputAssemblyName).ToLower(CultureInfo.InvariantCulture);
            this._factoryClass = new CodeTypeDeclaration(name);
            this._factoryClass.TypeAttributes &= ~TypeAttributes.Public;
            CodeSnippetTypeMember member = new CodeSnippetTypeMember(string.Empty) {
                LinePragma = new CodeLinePragma(@"c:\\dummy.txt", 1)
            };
            this._factoryClass.Members.Add(member);
            constructor = new CodeConstructor {
                Attributes = constructor.Attributes | MemberAttributes.Private
            };
            this._factoryClass.Members.Add(constructor);
            namespace2.Types.Add(this._factoryClass);
        }

        internal void AddFactoryMethod(string typeToCreate)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = GetCreateMethodNameForType(typeToCreate),
                ReturnType = new CodeTypeReference(typeof(object)),
                Attributes = MemberAttributes.Static
            };
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement(new CodeObjectCreateExpression(typeToCreate, new CodeExpression[0]));
            method.Statements.Add(statement);
            this._factoryClass.Members.Add(method);
        }

        private static string GetCreateMethodNameForType(string typeToCreate)
        {
            return ("Create_" + Util.MakeValidTypeNameFromString(typeToCreate));
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        internal static InstantiateObject GetFastObjectCreationDelegate(Type t)
        {
            Assembly assembly = t.Assembly;
            string s = Util.GetAssemblyShortName(t.Assembly).ToLower(CultureInfo.InvariantCulture);
            Type target = assembly.GetType("__ASP.FastObjectFactory_" + Util.MakeValidTypeNameFromString(s));
            if (target == null)
            {
                return null;
            }
            string createMethodNameForType = GetCreateMethodNameForType(t.FullName);
            try
            {
                return (InstantiateObject) Delegate.CreateDelegate(typeof(InstantiateObject), target, createMethodNameForType);
            }
            catch
            {
                return null;
            }
        }

        internal System.CodeDom.CodeCompileUnit CodeCompileUnit
        {
            get
            {
                return this._codeCompileUnit;
            }
        }
    }
}

