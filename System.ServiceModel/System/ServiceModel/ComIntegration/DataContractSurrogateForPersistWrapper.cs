namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.CodeDom;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    internal class DataContractSurrogateForPersistWrapper : IDataContractSurrogate
    {
        private Guid[] allowedClasses;

        public DataContractSurrogateForPersistWrapper(Guid[] allowedClasses)
        {
            this.allowedClasses = allowedClasses;
        }

        public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType)
        {
            return null;
        }

        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            return null;
        }

        public Type GetDataContractType(Type type)
        {
            if (type.IsInterface)
            {
                return typeof(PersistStreamTypeWrapper);
            }
            return type;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            if ((targetType == typeof(object)) || targetType.IsInterface)
            {
                PersistStreamTypeWrapper wrapper = obj as PersistStreamTypeWrapper;
                if (wrapper != null)
                {
                    if (!this.IsAllowedClass(wrapper.clsid))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NotAllowedPersistableCLSID", new object[] { wrapper.clsid.ToString("B") })));
                    }
                    return PersistHelper.ActivateAndLoadFromByteStream(wrapper.clsid, wrapper.dataStream);
                }
                if (targetType.IsInterface)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TargetTypeIsAnIntefaceButCorrespoindingTypeIsNotPersistStreamTypeWrapper")));
                }
            }
            return obj;
        }

        public void GetKnownCustomDataTypes(Collection<Type> customDataTypes)
        {
            customDataTypes.Add(typeof(PersistStreamTypeWrapper));
        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            if ((targetType == typeof(object)) || targetType.IsInterface)
            {
                IPersistStream persistableObject = obj as IPersistStream;
                if (persistableObject != null)
                {
                    PersistStreamTypeWrapper wrapper = new PersistStreamTypeWrapper();
                    persistableObject.GetClassID(out wrapper.clsid);
                    wrapper.dataStream = PersistHelper.PersistIPersistStreamToByteArray(persistableObject);
                    return wrapper;
                }
                if (targetType.IsInterface)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TargetObjectDoesNotSupportIPersistStream")));
                }
            }
            return obj;
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            return null;
        }

        private bool IsAllowedClass(Guid clsid)
        {
            foreach (Guid guid in this.allowedClasses)
            {
                if (clsid == guid)
                {
                    return true;
                }
            }
            return false;
        }

        public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit)
        {
            return null;
        }
    }
}

