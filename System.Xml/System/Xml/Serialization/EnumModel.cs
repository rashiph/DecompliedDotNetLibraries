namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class EnumModel : TypeModel
    {
        private ConstantModel[] constants;

        internal EnumModel(Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope)
        {
        }

        private ConstantModel GetConstantModel(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsSpecialName)
            {
                return null;
            }
            return new ConstantModel(fieldInfo, ((IConvertible) fieldInfo.GetValue(null)).ToInt64(null));
        }

        internal ConstantModel[] Constants
        {
            get
            {
                if (this.constants == null)
                {
                    ArrayList list = new ArrayList();
                    foreach (FieldInfo info in base.Type.GetFields())
                    {
                        ConstantModel constantModel = this.GetConstantModel(info);
                        if (constantModel != null)
                        {
                            list.Add(constantModel);
                        }
                    }
                    this.constants = (ConstantModel[]) list.ToArray(typeof(ConstantModel));
                }
                return this.constants;
            }
        }
    }
}

