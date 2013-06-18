namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Globalization;

    internal class ResourcePropertyMemberCodeDomSerializer : MemberCodeDomSerializer
    {
        private CodeDomLocalizationProvider.LanguageExtenders _extender;
        private CodeDomLocalizationModel _model;
        private MemberCodeDomSerializer _serializer;
        private CultureInfo localizationLanguage;

        internal ResourcePropertyMemberCodeDomSerializer(MemberCodeDomSerializer serializer, CodeDomLocalizationProvider.LanguageExtenders extender, CodeDomLocalizationModel model)
        {
            this._serializer = serializer;
            this._extender = extender;
            this._model = model;
        }

        private CultureInfo GetLocalizationLanguage(IDesignerSerializationManager manager)
        {
            if (this.localizationLanguage == null)
            {
                RootContext context = manager.Context[typeof(RootContext)] as RootContext;
                if (context != null)
                {
                    object component = context.Value;
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["LoadLanguage"];
                    if ((descriptor != null) && (descriptor.PropertyType == typeof(CultureInfo)))
                    {
                        this.localizationLanguage = (CultureInfo) descriptor.GetValue(component);
                    }
                }
            }
            return this.localizationLanguage;
        }

        private void OnSerializationComplete(object sender, EventArgs e)
        {
            this.localizationLanguage = null;
            IDesignerSerializationManager manager = sender as IDesignerSerializationManager;
            if (manager != null)
            {
                manager.SerializationComplete -= new EventHandler(this.OnSerializationComplete);
            }
        }

        public override void Serialize(IDesignerSerializationManager manager, object value, MemberDescriptor descriptor, CodeStatementCollection statements)
        {
            manager.Context.Push(this._model);
            try
            {
                this._serializer.Serialize(manager, value, descriptor, statements);
            }
            finally
            {
                manager.Context.Pop();
            }
        }

        public override bool ShouldSerialize(IDesignerSerializationManager manager, object value, MemberDescriptor descriptor)
        {
            bool flag = this._serializer.ShouldSerialize(manager, value, descriptor);
            if (!flag && !descriptor.Attributes.Contains(DesignOnlyAttribute.Yes))
            {
                switch (this._model)
                {
                    case CodeDomLocalizationModel.PropertyAssignment:
                    {
                        InheritanceAttribute notInherited = (InheritanceAttribute) manager.Context[typeof(InheritanceAttribute)];
                        if (notInherited == null)
                        {
                            notInherited = (InheritanceAttribute) TypeDescriptor.GetAttributes(value)[typeof(InheritanceAttribute)];
                            if (notInherited == null)
                            {
                                notInherited = InheritanceAttribute.NotInherited;
                            }
                        }
                        if (notInherited.InheritanceLevel != InheritanceLevel.InheritedReadOnly)
                        {
                            flag = true;
                        }
                        return flag;
                    }
                    case CodeDomLocalizationModel.PropertyReflection:
                        if (!flag)
                        {
                            if (this.localizationLanguage == null)
                            {
                                manager.SerializationComplete += new EventHandler(this.OnSerializationComplete);
                            }
                            if (this.GetLocalizationLanguage(manager) != CultureInfo.InvariantCulture)
                            {
                                flag = true;
                            }
                        }
                        return flag;
                }
            }
            return flag;
        }
    }
}

