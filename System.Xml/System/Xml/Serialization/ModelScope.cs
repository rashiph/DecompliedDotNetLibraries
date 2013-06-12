namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class ModelScope
    {
        private Hashtable arrayModels = new Hashtable();
        private Hashtable models = new Hashtable();
        private System.Xml.Serialization.TypeScope typeScope;

        internal ModelScope(System.Xml.Serialization.TypeScope typeScope)
        {
            this.typeScope = typeScope;
        }

        internal ArrayModel GetArrayModel(Type type)
        {
            TypeModel typeModel = (TypeModel) this.arrayModels[type];
            if (typeModel == null)
            {
                typeModel = this.GetTypeModel(type);
                if (!(typeModel is ArrayModel))
                {
                    TypeDesc arrayTypeDesc = this.typeScope.GetArrayTypeDesc(type);
                    typeModel = new ArrayModel(type, arrayTypeDesc, this);
                }
                this.arrayModels.Add(type, typeModel);
            }
            return (ArrayModel) typeModel;
        }

        internal TypeModel GetTypeModel(Type type)
        {
            return this.GetTypeModel(type, true);
        }

        internal TypeModel GetTypeModel(Type type, bool directReference)
        {
            TypeModel model = (TypeModel) this.models[type];
            if (model == null)
            {
                TypeDesc typeDesc = this.typeScope.GetTypeDesc(type, null, directReference);
                switch (typeDesc.Kind)
                {
                    case TypeKind.Root:
                    case TypeKind.Struct:
                    case TypeKind.Class:
                        model = new StructModel(type, typeDesc, this);
                        break;

                    case TypeKind.Primitive:
                        model = new PrimitiveModel(type, typeDesc, this);
                        break;

                    case TypeKind.Enum:
                        model = new EnumModel(type, typeDesc, this);
                        break;

                    case TypeKind.Array:
                    case TypeKind.Collection:
                    case TypeKind.Enumerable:
                        model = new ArrayModel(type, typeDesc, this);
                        break;

                    default:
                        if (!typeDesc.IsSpecial)
                        {
                            throw new NotSupportedException(Res.GetString("XmlUnsupportedTypeKind", new object[] { type.FullName }));
                        }
                        model = new SpecialModel(type, typeDesc, this);
                        break;
                }
                this.models.Add(type, model);
            }
            return model;
        }

        internal System.Xml.Serialization.TypeScope TypeScope
        {
            get
            {
                return this.typeScope;
            }
        }
    }
}

