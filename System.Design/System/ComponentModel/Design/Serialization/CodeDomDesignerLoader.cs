namespace System.ComponentModel.Design.Serialization
{
    using Microsoft.Internal.Performance;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class CodeDomDesignerLoader : BasicDesignerLoader, INameCreationService, IDesignerSerializationService
    {
        private ICodeGenerator _codeGenerator;
        private CodeCompileUnit _documentCompileUnit;
        private CodeNamespace _documentNamespace;
        private CodeTypeDeclaration _documentType;
        private IExtenderProvider[] _extenderProviders;
        private IExtenderProviderService _extenderProviderService;
        private CodeDomSerializer _rootSerializer;
        private BitVector32 _state = new BitVector32();
        private TypeCodeDomSerializer _typeSerializer;
        private static CodeMarkers codemarkers = CodeMarkers.Instance;
        private static readonly int StateCodeDomDirty = BitVector32.CreateMask();
        private static readonly int StateCodeParserChecked = BitVector32.CreateMask(StateCodeDomDirty);
        private static readonly int StateOwnTypeResolution = BitVector32.CreateMask(StateCodeParserChecked);
        private static TraceSwitch traceCDLoader = new TraceSwitch("CodeDomDesignerLoader", "Trace CodeDomDesignerLoader");

        protected CodeDomDesignerLoader()
        {
        }

        private void ClearDocument()
        {
            if (this._documentType != null)
            {
                base.LoaderHost.RemoveService(typeof(CodeTypeDeclaration));
                this._documentType = null;
                this._documentNamespace = null;
                this._documentCompileUnit = null;
                this._rootSerializer = null;
                this._typeSerializer = null;
            }
        }

        public override void Dispose()
        {
            IDesignerHost host = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            IComponentChangeService service = base.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
            }
            if (host != null)
            {
                host.RemoveService(typeof(INameCreationService));
                host.RemoveService(typeof(IDesignerSerializationService));
                host.RemoveService(typeof(ComponentSerializationService));
                if (this._state[StateOwnTypeResolution])
                {
                    host.RemoveService(typeof(ITypeResolutionService));
                    this._state[StateOwnTypeResolution] = false;
                }
            }
            if (this._extenderProviderService != null)
            {
                foreach (IExtenderProvider provider in this._extenderProviders)
                {
                    this._extenderProviderService.RemoveExtenderProvider(provider);
                }
            }
            base.Dispose();
        }

        private void EnsureDocument(IDesignerSerializationManager manager)
        {
            if (this._documentCompileUnit == null)
            {
                this._documentCompileUnit = this.Parse();
                if (this._documentCompileUnit == null)
                {
                    Exception exception = new NotSupportedException(System.Design.SR.GetString("CodeDomDesignerLoaderNoLanguageSupport")) {
                        HelpLink = "CodeDomDesignerLoaderNoLanguageSupport"
                    };
                    throw exception;
                }
            }
            if (this._documentType == null)
            {
                ArrayList list = null;
                bool flag = true;
                if (this._documentCompileUnit.UserData[typeof(InvalidOperationException)] != null)
                {
                    InvalidOperationException exception2 = this._documentCompileUnit.UserData[typeof(InvalidOperationException)] as InvalidOperationException;
                    if (exception2 != null)
                    {
                        this._documentCompileUnit = null;
                        throw exception2;
                    }
                }
                foreach (CodeNamespace namespace2 in this._documentCompileUnit.Namespaces)
                {
                    foreach (CodeTypeDeclaration declaration in namespace2.Types)
                    {
                        Type componentType = null;
                        foreach (CodeTypeReference reference in declaration.BaseTypes)
                        {
                            Type type = base.LoaderHost.GetType(CodeDomSerializerBase.GetTypeNameFromCodeTypeReference(manager, reference));
                            if ((type != null) && !type.IsInterface)
                            {
                                componentType = type;
                                break;
                            }
                            if (type == null)
                            {
                                if (list == null)
                                {
                                    list = new ArrayList();
                                }
                                list.Add(System.Design.SR.GetString("CodeDomDesignerLoaderDocumentFailureTypeNotFound", new object[] { declaration.Name, reference.BaseType }));
                            }
                        }
                        if (componentType != null)
                        {
                            bool flag2 = false;
                            foreach (Attribute attribute in TypeDescriptor.GetAttributes(componentType))
                            {
                                if (attribute is RootDesignerSerializerAttribute)
                                {
                                    RootDesignerSerializerAttribute attribute2 = (RootDesignerSerializerAttribute) attribute;
                                    string serializerBaseTypeName = attribute2.SerializerBaseTypeName;
                                    if ((serializerBaseTypeName != null) && (base.LoaderHost.GetType(serializerBaseTypeName) == typeof(CodeDomSerializer)))
                                    {
                                        Type type3 = base.LoaderHost.GetType(attribute2.SerializerTypeName);
                                        if ((type3 != null) && (type3 != typeof(RootCodeDomSerializer)))
                                        {
                                            flag2 = true;
                                            if (!flag)
                                            {
                                                throw new InvalidOperationException(System.Design.SR.GetString("CodeDomDesignerLoaderSerializerTypeNotFirstType", new object[] { declaration.Name }));
                                            }
                                            this._rootSerializer = (CodeDomSerializer) Activator.CreateInstance(type3, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
                                            break;
                                        }
                                    }
                                }
                            }
                            if ((this._rootSerializer == null) && this.HasRootDesignerAttribute(componentType))
                            {
                                this._typeSerializer = manager.GetSerializer(componentType, typeof(TypeCodeDomSerializer)) as TypeCodeDomSerializer;
                                if (!flag && (this._typeSerializer != null))
                                {
                                    this._typeSerializer = null;
                                    this._documentCompileUnit = null;
                                    throw new InvalidOperationException(System.Design.SR.GetString("CodeDomDesignerLoaderSerializerTypeNotFirstType", new object[] { declaration.Name }));
                                }
                            }
                            if ((this._rootSerializer == null) && (this._typeSerializer == null))
                            {
                                if (list == null)
                                {
                                    list = new ArrayList();
                                }
                                if (flag2)
                                {
                                    list.Add(System.Design.SR.GetString("CodeDomDesignerLoaderDocumentFailureTypeDesignerNotInstalled", new object[] { declaration.Name, componentType.FullName }));
                                }
                                else
                                {
                                    list.Add(System.Design.SR.GetString("CodeDomDesignerLoaderDocumentFailureTypeNotDesignable", new object[] { declaration.Name, componentType.FullName }));
                                }
                            }
                        }
                        if ((this._rootSerializer != null) || (this._typeSerializer != null))
                        {
                            this._documentNamespace = namespace2;
                            this._documentType = declaration;
                            break;
                        }
                        flag = false;
                    }
                    if (this._documentType != null)
                    {
                        break;
                    }
                }
                if (this._documentType == null)
                {
                    Exception exception3;
                    this._documentCompileUnit = null;
                    if (list != null)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (string str2 in list)
                        {
                            builder.Append("\r\n");
                            builder.Append(str2);
                        }
                        exception3 = new InvalidOperationException(System.Design.SR.GetString("CodeDomDesignerLoaderNoRootSerializerWithFailures", new object[] { builder.ToString() })) {
                            HelpLink = "CodeDomDesignerLoaderNoRootSerializer"
                        };
                    }
                    else
                    {
                        exception3 = new InvalidOperationException(System.Design.SR.GetString("CodeDomDesignerLoaderNoRootSerializer")) {
                            HelpLink = "CodeDomDesignerLoaderNoRootSerializer"
                        };
                    }
                    throw exception3;
                }
                base.LoaderHost.AddService(typeof(CodeTypeDeclaration), this._documentType);
            }
            codemarkers.CodeMarker(CodeMarkerEvent.perfFXGetDocumentType);
        }

        private bool HasRootDesignerAttribute(Type t)
        {
            AttributeCollection attributes = TypeDescriptor.GetAttributes(t);
            for (int i = 0; i < attributes.Count; i++)
            {
                DesignerAttribute attribute = attributes[i] as DesignerAttribute;
                if (attribute != null)
                {
                    Type type = Type.GetType(attribute.DesignerBaseTypeName);
                    if ((type != null) && (type == typeof(IRootDesigner)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
            base.LoaderHost.AddService(typeof(ComponentSerializationService), callback);
            base.LoaderHost.AddService(typeof(INameCreationService), this);
            base.LoaderHost.AddService(typeof(IDesignerSerializationService), this);
            if (base.GetService(typeof(ITypeResolutionService)) == null)
            {
                ITypeResolutionService typeResolutionService = this.TypeResolutionService;
                if (typeResolutionService == null)
                {
                    throw new InvalidOperationException(System.Design.SR.GetString("CodeDomDesignerLoaderNoTypeResolution"));
                }
                base.LoaderHost.AddService(typeof(ITypeResolutionService), typeResolutionService);
                this._state[StateOwnTypeResolution] = true;
            }
            this._extenderProviderService = base.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
            if (this._extenderProviderService != null)
            {
                this._extenderProviders = new IExtenderProvider[] { new ModifiersExtenderProvider(), new ModifiersInheritedExtenderProvider() };
                foreach (IExtenderProvider provider in this._extenderProviders)
                {
                    this._extenderProviderService.AddExtenderProvider(provider);
                }
            }
        }

        private bool IntegrateSerializedTree(IDesignerSerializationManager manager, CodeTypeDeclaration newDecl)
        {
            this.EnsureDocument(manager);
            CodeTypeDeclaration declaration = this._documentType;
            bool caseInsensitive = false;
            bool flag2 = false;
            System.CodeDom.Compiler.CodeDomProvider codeDomProvider = this.CodeDomProvider;
            if (codeDomProvider != null)
            {
                caseInsensitive = (codeDomProvider.LanguageOptions & LanguageOptions.CaseInsensitive) != LanguageOptions.None;
            }
            if (!string.Equals(declaration.Name, newDecl.Name, caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            {
                declaration.Name = newDecl.Name;
                flag2 = true;
            }
            if (!declaration.Attributes.Equals(newDecl.Attributes))
            {
                declaration.Attributes = newDecl.Attributes;
                flag2 = true;
            }
            int index = 0;
            bool flag3 = false;
            int num2 = 0;
            bool flag4 = false;
            IDictionary dictionary = new HybridDictionary(declaration.Members.Count, caseInsensitive);
            int count = declaration.Members.Count;
            for (int i = 0; i < count; i++)
            {
                string name;
                CodeTypeMember member = declaration.Members[i];
                if (member is CodeConstructor)
                {
                    name = ".ctor";
                }
                else if (member is CodeTypeConstructor)
                {
                    name = ".cctor";
                }
                else
                {
                    name = member.Name;
                }
                dictionary[name] = i;
                if (member is CodeMemberField)
                {
                    if (!flag3)
                    {
                        index = i;
                    }
                }
                else if (index > 0)
                {
                    flag3 = true;
                }
                if (member is CodeMemberMethod)
                {
                    if (!flag4)
                    {
                        num2 = i;
                    }
                }
                else if (num2 > 0)
                {
                    flag4 = true;
                }
            }
            ArrayList list = new ArrayList();
            foreach (CodeTypeMember member2 in newDecl.Members)
            {
                string str2;
                if (member2 is CodeConstructor)
                {
                    str2 = ".ctor";
                }
                else
                {
                    str2 = member2.Name;
                }
                object obj2 = dictionary[str2];
                if (obj2 != null)
                {
                    int num5 = (int) obj2;
                    CodeTypeMember member3 = declaration.Members[num5];
                    if (member3 != member2)
                    {
                        if (member2 is CodeMemberField)
                        {
                            if (member3 is CodeMemberField)
                            {
                                CodeMemberField field = (CodeMemberField) member3;
                                CodeMemberField field2 = (CodeMemberField) member2;
                                if ((string.Equals(field2.Name, field.Name) && (field2.Attributes == field.Attributes)) && TypesEqual(field2.Type, field.Type))
                                {
                                    continue;
                                }
                                declaration.Members[num5] = member2;
                            }
                            else
                            {
                                list.Add(member2);
                            }
                        }
                        else if (member2 is CodeMemberMethod)
                        {
                            if ((member3 is CodeMemberMethod) && !(member3 is CodeConstructor))
                            {
                                CodeMemberMethod method = (CodeMemberMethod) member3;
                                CodeMemberMethod method2 = (CodeMemberMethod) member2;
                                method.Statements.Clear();
                                method.Statements.AddRange(method2.Statements);
                            }
                        }
                        else
                        {
                            declaration.Members[num5] = member2;
                        }
                        flag2 = true;
                    }
                }
                else
                {
                    list.Add(member2);
                }
            }
            foreach (CodeTypeMember member4 in list)
            {
                if (member4 is CodeMemberField)
                {
                    if (index >= declaration.Members.Count)
                    {
                        declaration.Members.Add(member4);
                    }
                    else
                    {
                        declaration.Members.Insert(index, member4);
                    }
                    index++;
                    num2++;
                    flag2 = true;
                }
                else if (member4 is CodeMemberMethod)
                {
                    if (num2 >= declaration.Members.Count)
                    {
                        declaration.Members.Add(member4);
                    }
                    else
                    {
                        declaration.Members.Insert(num2, member4);
                    }
                    num2++;
                    flag2 = true;
                }
                else
                {
                    declaration.Members.Add(member4);
                    flag2 = true;
                }
            }
            return flag2;
        }

        protected override bool IsReloadNeeded()
        {
            if (!base.IsReloadNeeded())
            {
                return false;
            }
            if (this._documentType == null)
            {
                return true;
            }
            ICodeDomDesignerReload codeDomProvider = this.CodeDomProvider as ICodeDomDesignerReload;
            if (codeDomProvider == null)
            {
                return true;
            }
            bool flag = true;
            string name = this._documentType.Name;
            try
            {
                this.ClearDocument();
                this.EnsureDocument(base.GetService(typeof(IDesignerSerializationManager)) as IDesignerSerializationManager);
            }
            catch
            {
            }
            if (this._documentCompileUnit != null)
            {
                flag = codeDomProvider.ShouldReloadDesigner(this._documentCompileUnit) | ((this._documentType == null) || !this._documentType.Name.Equals(name));
            }
            return flag;
        }

        protected override void OnBeginLoad()
        {
            IComponentChangeService service = (IComponentChangeService) base.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
            }
            base.OnBeginLoad();
        }

        protected override void OnBeginUnload()
        {
            base.OnBeginUnload();
            this.ClearDocument();
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            string name = e.Component.Site.Name;
            this.RemoveDeclaration(name);
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            this.OnComponentRename(e.Component, e.OldName, e.NewName);
        }

        protected virtual void OnComponentRename(object component, string oldName, string newName)
        {
            if (base.LoaderHost.RootComponent == component)
            {
                if (this._documentType != null)
                {
                    this._documentType.Name = newName;
                }
            }
            else if (this._documentType != null)
            {
                CodeTypeMemberCollection members = this._documentType.Members;
                for (int i = 0; i < members.Count; i++)
                {
                    if (((members[i] is CodeMemberField) && members[i].Name.Equals(oldName)) && ((CodeMemberField) members[i]).Type.BaseType.Equals(TypeDescriptor.GetClassName(component)))
                    {
                        members[i].Name = newName;
                        return;
                    }
                }
            }
        }

        private object OnCreateService(IServiceContainer container, Type serviceType)
        {
            if (serviceType == typeof(ComponentSerializationService))
            {
                return new CodeDomComponentSerializationService(base.LoaderHost);
            }
            return null;
        }

        protected override void OnEndLoad(bool successful, ICollection errors)
        {
            base.OnEndLoad(successful, errors);
            if (successful)
            {
                IComponentChangeService service = (IComponentChangeService) base.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                    service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
                }
            }
        }

        protected abstract CodeCompileUnit Parse();
        protected override void PerformFlush(IDesignerSerializationManager manager)
        {
            CodeTypeDeclaration newDecl = null;
            if (this._rootSerializer != null)
            {
                newDecl = this._rootSerializer.Serialize(manager, base.LoaderHost.RootComponent) as CodeTypeDeclaration;
            }
            else if (this._typeSerializer != null)
            {
                newDecl = this._typeSerializer.Serialize(manager, base.LoaderHost.RootComponent, base.LoaderHost.Container.Components);
            }
            codemarkers.CodeMarker(CodeMarkerEvent.perfFXGenerateCodeTreeEnd);
            if ((newDecl != null) && this.IntegrateSerializedTree(manager, newDecl))
            {
                codemarkers.CodeMarker(CodeMarkerEvent.perfFXIntegrateSerializedTreeEnd);
                this.Write(this._documentCompileUnit);
            }
        }

        protected override void PerformLoad(IDesignerSerializationManager manager)
        {
            this.EnsureDocument(manager);
            codemarkers.CodeMarker(CodeMarkerEvent.perfFXDeserializeStart);
            if (this._rootSerializer != null)
            {
                this._rootSerializer.Deserialize(manager, this._documentType);
            }
            else
            {
                this._typeSerializer.Deserialize(manager, this._documentType);
            }
            codemarkers.CodeMarker(CodeMarkerEvent.perfFXDeserializeEnd);
            string name = string.Format(CultureInfo.CurrentCulture, "{0}.{1}", new object[] { this._documentNamespace.Name, this._documentType.Name });
            base.SetBaseComponentClassName(name);
        }

        private void RemoveDeclaration(string name)
        {
            if (this._documentType != null)
            {
                CodeTypeMemberCollection members = this._documentType.Members;
                for (int i = 0; i < members.Count; i++)
                {
                    if ((members[i] is CodeMemberField) && members[i].Name.Equals(name))
                    {
                        members.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        ICollection IDesignerSerializationService.Deserialize(object serializationData)
        {
            if (!(serializationData is SerializationStore))
            {
                Exception exception = new ArgumentException(System.Design.SR.GetString("CodeDomDesignerLoaderBadSerializationObject")) {
                    HelpLink = "CodeDomDesignerLoaderBadSerializationObject"
                };
                throw exception;
            }
            ComponentSerializationService service = base.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
            if (service == null)
            {
                this.ThrowMissingService(typeof(ComponentSerializationService));
            }
            return service.Deserialize((SerializationStore) serializationData, base.LoaderHost.Container);
        }

        object IDesignerSerializationService.Serialize(ICollection objects)
        {
            if (objects == null)
            {
                objects = new object[0];
            }
            ComponentSerializationService service = base.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
            if (service == null)
            {
                this.ThrowMissingService(typeof(ComponentSerializationService));
            }
            SerializationStore store = service.CreateStore();
            using (store)
            {
                foreach (object obj2 in objects)
                {
                    service.Serialize(store, obj2);
                }
            }
            return store;
        }

        string INameCreationService.CreateName(IContainer container, Type dataType)
        {
            string str;
            if (dataType == null)
            {
                throw new ArgumentNullException("dataType");
            }
            string name = dataType.Name;
            StringBuilder builder = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && (((i == 0) || (i == (name.Length - 1))) || char.IsUpper(name[i + 1])))
                {
                    builder.Append(char.ToLower(name[i], CultureInfo.CurrentCulture));
                }
                else
                {
                    builder.Append(name.Substring(i));
                    break;
                }
            }
            builder.Replace('`', '_');
            name = builder.ToString();
            CodeTypeDeclaration declaration = this._documentType;
            Hashtable hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            if (declaration != null)
            {
                foreach (CodeTypeMember member in declaration.Members)
                {
                    hashtable[member.Name] = member;
                }
            }
            if (container != null)
            {
                bool flag;
                int num2 = 0;
                do
                {
                    num2++;
                    flag = false;
                    str = string.Format(CultureInfo.CurrentCulture, "{0}{1}", new object[] { name, num2.ToString(CultureInfo.InvariantCulture) });
                    if ((container != null) && (container.Components[str] != null))
                    {
                        flag = true;
                    }
                    if (!flag && (hashtable[str] != null))
                    {
                        flag = true;
                    }
                }
                while (flag);
            }
            else
            {
                str = name;
            }
            if (this._codeGenerator == null)
            {
                System.CodeDom.Compiler.CodeDomProvider codeDomProvider = this.CodeDomProvider;
                if (codeDomProvider != null)
                {
                    this._codeGenerator = codeDomProvider.CreateGenerator();
                }
            }
            if (this._codeGenerator != null)
            {
                str = this._codeGenerator.CreateValidIdentifier(str);
            }
            return str;
        }

        bool INameCreationService.IsValidName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                return false;
            }
            if (this._codeGenerator == null)
            {
                System.CodeDom.Compiler.CodeDomProvider codeDomProvider = this.CodeDomProvider;
                if (codeDomProvider != null)
                {
                    this._codeGenerator = codeDomProvider.CreateGenerator();
                }
            }
            if (this._codeGenerator != null)
            {
                if (!this._codeGenerator.IsValidIdentifier(name))
                {
                    return false;
                }
                if (!this._codeGenerator.IsValidIdentifier(name + "Handler"))
                {
                    return false;
                }
            }
            if (!this.Loading)
            {
                CodeTypeDeclaration declaration = this._documentType;
                if (declaration != null)
                {
                    foreach (CodeTypeMember member in declaration.Members)
                    {
                        if (string.Equals(member.Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                }
                if (this.Modified && (base.LoaderHost.Container.Components[name] != null))
                {
                    return false;
                }
            }
            return true;
        }

        void INameCreationService.ValidateName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                Exception exception = new ArgumentException(System.Design.SR.GetString("CodeDomDesignerLoaderInvalidBlankIdentifier")) {
                    HelpLink = "CodeDomDesignerLoaderInvalidIdentifier"
                };
                throw exception;
            }
            if (this._codeGenerator == null)
            {
                System.CodeDom.Compiler.CodeDomProvider codeDomProvider = this.CodeDomProvider;
                if (codeDomProvider != null)
                {
                    this._codeGenerator = codeDomProvider.CreateGenerator();
                }
            }
            if (this._codeGenerator != null)
            {
                this._codeGenerator.ValidateIdentifier(name);
                try
                {
                    this._codeGenerator.ValidateIdentifier(name + "_");
                }
                catch
                {
                    Exception exception2 = new ArgumentException(System.Design.SR.GetString("CodeDomDesignerLoaderInvalidIdentifier", new object[] { name })) {
                        HelpLink = "CodeDomDesignerLoaderInvalidIdentifier"
                    };
                    throw exception2;
                }
            }
            if (!this.Loading)
            {
                bool flag = false;
                CodeTypeDeclaration declaration = this._documentType;
                if (declaration != null)
                {
                    foreach (CodeTypeMember member in declaration.Members)
                    {
                        if (string.Equals(member.Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if ((!flag && this.Modified) && (base.LoaderHost.Container.Components[name] != null))
                {
                    flag = true;
                }
                if (flag)
                {
                    Exception exception3 = new ArgumentException(System.Design.SR.GetString("CodeDomDesignerLoaderDupComponentName", new object[] { name })) {
                        HelpLink = "CodeDomDesignerLoaderDupComponentName"
                    };
                    throw exception3;
                }
            }
        }

        private void ThrowMissingService(Type serviceType)
        {
            Exception exception = new InvalidOperationException(System.Design.SR.GetString("BasicDesignerLoaderMissingService", new object[] { serviceType.Name })) {
                HelpLink = "BasicDesignerLoaderMissingService"
            };
            throw exception;
        }

        private static bool TypesEqual(CodeTypeReference typeLeft, CodeTypeReference typeRight)
        {
            if (typeLeft.ArrayRank != typeRight.ArrayRank)
            {
                return false;
            }
            if (!typeLeft.BaseType.Equals(typeRight.BaseType))
            {
                return false;
            }
            if ((typeLeft.TypeArguments != null) && (typeRight.TypeArguments == null))
            {
                return false;
            }
            if ((typeLeft.TypeArguments == null) && (typeRight.TypeArguments != null))
            {
                return false;
            }
            if ((typeLeft.TypeArguments != null) && (typeRight.TypeArguments != null))
            {
                if (typeLeft.TypeArguments.Count != typeRight.TypeArguments.Count)
                {
                    return false;
                }
                for (int i = 0; i < typeLeft.TypeArguments.Count; i++)
                {
                    if (!TypesEqual(typeLeft.TypeArguments[i], typeRight.TypeArguments[i]))
                    {
                        return false;
                    }
                }
            }
            if (typeLeft.ArrayRank > 0)
            {
                return TypesEqual(typeLeft.ArrayElementType, typeRight.ArrayElementType);
            }
            return true;
        }

        protected abstract void Write(CodeCompileUnit unit);

        protected abstract System.CodeDom.Compiler.CodeDomProvider CodeDomProvider { get; }

        protected abstract ITypeResolutionService TypeResolutionService { get; }

        private class ModifierConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return this.GetConverter(context).CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return this.GetConverter(context).CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return this.GetConverter(context).ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                return this.GetConverter(context).ConvertTo(context, culture, value, destinationType);
            }

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                return this.GetConverter(context).CreateInstance(context, propertyValues);
            }

            private TypeConverter GetConverter(ITypeDescriptorContext context)
            {
                TypeConverter converter = null;
                if (context != null)
                {
                    CodeDomProvider service = (CodeDomProvider) context.GetService(typeof(CodeDomProvider));
                    if (service != null)
                    {
                        converter = service.GetConverter(typeof(MemberAttributes));
                    }
                }
                if (converter == null)
                {
                    converter = TypeDescriptor.GetConverter(typeof(MemberAttributes));
                }
                return converter;
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return this.GetConverter(context).GetCreateInstanceSupported(context);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return this.GetConverter(context).GetProperties(context, value, attributes);
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return this.GetConverter(context).GetPropertiesSupported(context);
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                TypeConverter.StandardValuesCollection standardValues = this.GetConverter(context).GetStandardValues(context);
                if ((standardValues == null) || (standardValues.Count <= 0))
                {
                    return standardValues;
                }
                bool flag = false;
                foreach (MemberAttributes attributes in standardValues)
                {
                    if ((attributes & MemberAttributes.AccessMask) == ((MemberAttributes) 0))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    return standardValues;
                }
                ArrayList values = new ArrayList(standardValues.Count);
                foreach (MemberAttributes attributes2 in standardValues)
                {
                    if (((attributes2 & MemberAttributes.AccessMask) != ((MemberAttributes) 0)) && (attributes2 != MemberAttributes.AccessMask))
                    {
                        values.Add(attributes2);
                    }
                }
                return new TypeConverter.StandardValuesCollection(values);
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return this.GetConverter(context).GetStandardValuesExclusive(context);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return this.GetConverter(context).GetStandardValuesSupported(context);
            }

            public override bool IsValid(ITypeDescriptorContext context, object value)
            {
                return this.GetConverter(context).IsValid(context, value);
            }
        }

        [ProvideProperty("GenerateMember", typeof(IComponent)), ProvideProperty("Modifiers", typeof(IComponent))]
        private class ModifiersExtenderProvider : IExtenderProvider
        {
            private IDesignerHost _host;

            public bool CanExtend(object o)
            {
                IComponent c = o as IComponent;
                if (c == null)
                {
                    return false;
                }
                IComponent baseComponent = this.GetBaseComponent(c);
                if (o == baseComponent)
                {
                    return false;
                }
                if (!TypeDescriptor.GetAttributes(o)[typeof(InheritanceAttribute)].Equals(InheritanceAttribute.NotInherited))
                {
                    return false;
                }
                return true;
            }

            private IComponent GetBaseComponent(IComponent c)
            {
                IComponent rootComponent = null;
                if (c == null)
                {
                    return null;
                }
                if (this._host == null)
                {
                    ISite site = c.Site;
                    if (site != null)
                    {
                        this._host = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                    }
                }
                if (this._host != null)
                {
                    rootComponent = this._host.RootComponent;
                }
                return rootComponent;
            }

            [HelpKeyword("Designer_GenerateMember"), DefaultValue(true), System.Design.SRDescription("CodeDomDesignerLoaderPropGenerateMember"), Category("Design"), DesignOnly(true)]
            public bool GetGenerateMember(IComponent comp)
            {
                ISite site = comp.Site;
                if (site != null)
                {
                    IDictionaryService service = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                    if (service != null)
                    {
                        object obj2 = service.GetValue("GenerateMember");
                        if (obj2 is bool)
                        {
                            return (bool) obj2;
                        }
                    }
                }
                return true;
            }

            [DefaultValue(0x5000), System.Design.SRDescription("CodeDomDesignerLoaderPropModifiers"), Category("Design"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), TypeConverter(typeof(CodeDomDesignerLoader.ModifierConverter)), HelpKeyword("Designer_Modifiers"), DesignOnly(true)]
            public MemberAttributes GetModifiers(IComponent comp)
            {
                ISite site = comp.Site;
                if (site != null)
                {
                    IDictionaryService service = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                    if (service != null)
                    {
                        object obj2 = service.GetValue("Modifiers");
                        if (obj2 is MemberAttributes)
                        {
                            return (MemberAttributes) obj2;
                        }
                    }
                }
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(comp)["DefaultModifiers"];
                if ((descriptor != null) && (descriptor.PropertyType == typeof(MemberAttributes)))
                {
                    return (MemberAttributes) descriptor.GetValue(comp);
                }
                return MemberAttributes.Private;
            }

            public void SetGenerateMember(IComponent comp, bool generate)
            {
                ISite site = comp.Site;
                if (site != null)
                {
                    IDictionaryService service = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                    bool generateMember = this.GetGenerateMember(comp);
                    if (service != null)
                    {
                        service.SetValue("GenerateMember", generate);
                    }
                    if (generateMember && !generate)
                    {
                        CodeTypeDeclaration declaration = site.GetService(typeof(CodeTypeDeclaration)) as CodeTypeDeclaration;
                        string name = site.Name;
                        if ((declaration != null) && (name != null))
                        {
                            foreach (CodeTypeMember member in declaration.Members)
                            {
                                CodeMemberField field = member as CodeMemberField;
                                if ((field != null) && field.Name.Equals(name))
                                {
                                    declaration.Members.Remove(field);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            public void SetModifiers(IComponent comp, MemberAttributes modifiers)
            {
                ISite site = comp.Site;
                if (site != null)
                {
                    IDictionaryService service = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                    if (service != null)
                    {
                        service.SetValue("Modifiers", modifiers);
                    }
                }
            }
        }

        [ProvideProperty("Modifiers", typeof(IComponent))]
        private class ModifiersInheritedExtenderProvider : IExtenderProvider
        {
            private IDesignerHost _host;

            public bool CanExtend(object o)
            {
                IComponent c = o as IComponent;
                if (c == null)
                {
                    return false;
                }
                IComponent baseComponent = this.GetBaseComponent(c);
                if (o == baseComponent)
                {
                    return false;
                }
                return !TypeDescriptor.GetAttributes(o)[typeof(InheritanceAttribute)].Equals(InheritanceAttribute.NotInherited);
            }

            private IComponent GetBaseComponent(IComponent c)
            {
                IComponent rootComponent = null;
                if (c == null)
                {
                    return null;
                }
                if (this._host == null)
                {
                    ISite site = c.Site;
                    if (site != null)
                    {
                        this._host = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                    }
                }
                if (this._host != null)
                {
                    rootComponent = this._host.RootComponent;
                }
                return rootComponent;
            }

            [DefaultValue(0x5000), System.Design.SRDescription("CodeDomDesignerLoaderPropModifiers"), Category("Design"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), TypeConverter(typeof(CodeDomDesignerLoader.ModifierConverter)), DesignOnly(true)]
            public MemberAttributes GetModifiers(IComponent comp)
            {
                Type type = this.GetBaseComponent(comp).GetType();
                ISite site = comp.Site;
                if (site != null)
                {
                    string name = site.Name;
                    if (name != null)
                    {
                        FieldInfo field = TypeDescriptor.GetReflectionType(type).GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (field != null)
                        {
                            if (field.IsPrivate)
                            {
                                return MemberAttributes.Private;
                            }
                            if (field.IsPublic)
                            {
                                return MemberAttributes.Public;
                            }
                            if (field.IsFamily)
                            {
                                return MemberAttributes.Family;
                            }
                            if (field.IsAssembly)
                            {
                                return MemberAttributes.Assembly;
                            }
                            if (field.IsFamilyOrAssembly)
                            {
                                return MemberAttributes.FamilyOrAssembly;
                            }
                            if (field.IsFamilyAndAssembly)
                            {
                                return MemberAttributes.FamilyAndAssembly;
                            }
                        }
                        else
                        {
                            PropertyInfo property = TypeDescriptor.GetReflectionType(type).GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            if (property != null)
                            {
                                MethodInfo[] accessors = property.GetAccessors(true);
                                if ((accessors != null) && (accessors.Length > 0))
                                {
                                    MethodInfo info3 = accessors[0];
                                    if (info3 != null)
                                    {
                                        if (info3.IsPrivate)
                                        {
                                            return MemberAttributes.Private;
                                        }
                                        if (info3.IsPublic)
                                        {
                                            return MemberAttributes.Public;
                                        }
                                        if (info3.IsFamily)
                                        {
                                            return MemberAttributes.Family;
                                        }
                                        if (info3.IsAssembly)
                                        {
                                            return MemberAttributes.Assembly;
                                        }
                                        if (info3.IsFamilyOrAssembly)
                                        {
                                            return MemberAttributes.FamilyOrAssembly;
                                        }
                                        if (info3.IsFamilyAndAssembly)
                                        {
                                            return MemberAttributes.FamilyAndAssembly;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return MemberAttributes.Private;
            }
        }
    }
}

