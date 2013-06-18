namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class Helper
    {
        internal static FieldAttributes ConvertToFieldAttributes(MemberAttributes memberAttributes)
        {
            FieldAttributes privateScope = FieldAttributes.PrivateScope;
            if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Assembly)
            {
                privateScope |= FieldAttributes.Assembly;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Family)
            {
                privateScope |= FieldAttributes.Family;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyAndAssembly)
            {
                privateScope |= FieldAttributes.FamANDAssem;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyOrAssembly)
            {
                privateScope |= FieldAttributes.FamORAssem;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Private)
            {
                privateScope |= FieldAttributes.Private;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Public)
            {
                privateScope |= FieldAttributes.Public;
            }
            if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Const)
            {
                return (privateScope | (FieldAttributes.Literal | FieldAttributes.Static));
            }
            if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Static)
            {
                privateScope |= FieldAttributes.Static;
            }
            return privateScope;
        }

        internal static MethodAttributes ConvertToMethodAttributes(MemberAttributes memberAttributes)
        {
            MethodAttributes privateScope = MethodAttributes.PrivateScope;
            if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Assembly)
            {
                privateScope |= MethodAttributes.Assembly;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Family)
            {
                privateScope |= MethodAttributes.Family;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyAndAssembly)
            {
                privateScope |= MethodAttributes.FamANDAssem;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyOrAssembly)
            {
                privateScope |= MethodAttributes.FamORAssem;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Private)
            {
                privateScope |= MethodAttributes.Private;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Public)
            {
                privateScope |= MethodAttributes.Public;
            }
            if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract)
            {
                privateScope |= MethodAttributes.Abstract;
            }
            else if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Final)
            {
                privateScope |= MethodAttributes.Final;
            }
            else if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Static)
            {
                privateScope |= MethodAttributes.Static;
            }
            if ((memberAttributes & MemberAttributes.VTableMask) == MemberAttributes.New)
            {
                privateScope |= MethodAttributes.NewSlot;
            }
            return privateScope;
        }

        internal static ParameterAttributes ConvertToParameterAttributes(FieldDirection direction)
        {
            switch (direction)
            {
                case FieldDirection.In:
                    return ParameterAttributes.In;

                case FieldDirection.Out:
                    return ParameterAttributes.Out;
            }
            return ParameterAttributes.None;
        }

        internal static TypeAttributes ConvertToTypeAttributes(MemberAttributes memberAttributes, Type declaringType)
        {
            TypeAttributes ansiClass = TypeAttributes.AnsiClass;
            if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Assembly)
            {
                ansiClass |= (declaringType != null) ? TypeAttributes.NestedAssembly : TypeAttributes.AnsiClass;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Family)
            {
                ansiClass |= (declaringType != null) ? TypeAttributes.NestedFamily : TypeAttributes.AnsiClass;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyAndAssembly)
            {
                ansiClass |= (declaringType != null) ? TypeAttributes.NestedFamANDAssem : TypeAttributes.AnsiClass;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyOrAssembly)
            {
                ansiClass |= (declaringType != null) ? TypeAttributes.NestedFamORAssem : TypeAttributes.AnsiClass;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Private)
            {
                ansiClass |= (declaringType != null) ? TypeAttributes.NestedPrivate : TypeAttributes.AnsiClass;
            }
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Public)
            {
                ansiClass |= (declaringType != null) ? TypeAttributes.NestedPublic : TypeAttributes.Public;
            }
            if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract)
            {
                return (ansiClass | TypeAttributes.Abstract);
            }
            if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Final)
            {
                return (ansiClass | TypeAttributes.Sealed);
            }
            if ((memberAttributes & MemberAttributes.Static) == MemberAttributes.Static)
            {
                ansiClass |= TypeAttributes.Sealed | TypeAttributes.Abstract;
            }
            return ansiClass;
        }

        internal static string EnsureTypeName(string typeName)
        {
            if ((typeName != null) && (typeName.Length != 0))
            {
                if (typeName.IndexOf('.') == -1)
                {
                    if (typeName.StartsWith("@", StringComparison.Ordinal))
                    {
                        typeName = typeName.Substring(1);
                        return typeName;
                    }
                    if (typeName.StartsWith("[", StringComparison.Ordinal) && typeName.EndsWith("]", StringComparison.Ordinal))
                    {
                        typeName = typeName.Substring(1, typeName.Length - 1);
                    }
                    return typeName;
                }
                string[] strArray = typeName.Split(new char[] { '.' });
                typeName = string.Empty;
                int index = 0;
                while (index < (strArray.Length - 1))
                {
                    typeName = typeName + EnsureTypeName(strArray[index]);
                    typeName = typeName + ".";
                    index++;
                }
                typeName = typeName + EnsureTypeName(strArray[index]);
            }
            return typeName;
        }

        internal static object[] GetCustomAttributes(Type attributeType, bool inherit, Attribute[] attributes, MemberInfo memberInfo)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            if (attributeType == typeof(object))
            {
                list.AddRange(attributes);
            }
            else
            {
                foreach (AttributeInfoAttribute attribute in attributes)
                {
                    if (attribute.AttributeInfo.AttributeType == attributeType)
                    {
                        list.Add(attribute);
                        list2.Add(attributeType);
                    }
                }
            }
            if (inherit)
            {
                MemberInfo baseType = null;
                if (memberInfo is Type)
                {
                    baseType = ((Type) memberInfo).BaseType;
                }
                else
                {
                    baseType = ((DesignTimeType) memberInfo.DeclaringType).GetBaseMember(memberInfo.GetType(), memberInfo.DeclaringType.BaseType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, new DesignTimeType.MemberSignature(memberInfo));
                }
                if (baseType != null)
                {
                    foreach (Attribute attribute2 in baseType.GetCustomAttributes(attributeType, inherit))
                    {
                        if (!(attribute2 is AttributeInfoAttribute) || !list2.Contains(((AttributeInfoAttribute) attribute2).AttributeInfo.AttributeType))
                        {
                            list.Add(attribute2);
                        }
                    }
                }
            }
            return list.ToArray();
        }

        internal static bool IncludeAccessor(MethodInfo accessor, bool nonPublic)
        {
            if (accessor == null)
            {
                return false;
            }
            return (nonPublic || accessor.IsPublic);
        }

        internal static bool IsDefined(Type attributeType, bool inherit, Attribute[] attributes, MemberInfo memberInfo)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            foreach (Attribute attribute in attributes)
            {
                if ((attribute is AttributeInfoAttribute) && ((attribute as AttributeInfoAttribute).AttributeInfo.AttributeType == attributeType))
                {
                    return true;
                }
            }
            MemberInfo baseType = null;
            if (memberInfo is Type)
            {
                baseType = ((Type) memberInfo).BaseType;
            }
            else
            {
                baseType = ((DesignTimeType) memberInfo.DeclaringType).GetBaseMember(memberInfo.GetType(), memberInfo.DeclaringType.BaseType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, new DesignTimeType.MemberSignature(memberInfo));
            }
            return ((baseType != null) && baseType.IsDefined(attributeType, inherit));
        }

        internal static Attribute[] LoadCustomAttributes(CodeAttributeDeclarationCollection codeAttributeCollection, DesignTimeType declaringType)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }
            if (codeAttributeCollection == null)
            {
                return new Attribute[0];
            }
            List<Attribute> list = new List<Attribute>();
            foreach (CodeAttributeDeclaration declaration in codeAttributeCollection)
            {
                string[] argumentNames = new string[declaration.Arguments.Count];
                object[] argumentValues = new object[declaration.Arguments.Count];
                Type attributeType = declaringType.ResolveType(declaration.Name);
                if (attributeType != null)
                {
                    int index = 0;
                    foreach (CodeAttributeArgument argument in declaration.Arguments)
                    {
                        argumentNames[index] = argument.Name;
                        if (argument.Value is CodePrimitiveExpression)
                        {
                            argumentValues[index] = (argument.Value as CodePrimitiveExpression).Value;
                        }
                        else if (argument.Value is CodeTypeOfExpression)
                        {
                            argumentValues[index] = argument.Value;
                        }
                        else if (argument.Value is CodeSnippetExpression)
                        {
                            argumentValues[index] = (argument.Value as CodeSnippetExpression).Value;
                        }
                        else
                        {
                            argumentValues[index] = new ArgumentException(SR.GetString("Error_TypeSystemAttributeArgument"));
                        }
                        index++;
                    }
                    bool flag = false;
                    foreach (AttributeInfoAttribute attribute in list)
                    {
                        if (attribute.AttributeInfo.AttributeType.FullName.Equals(attributeType.FullName))
                        {
                            flag = true;
                            break;
                        }
                    }
                    bool allowMultiple = false;
                    if (flag && (attributeType.Assembly != null))
                    {
                        object[] customAttributes = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), true);
                        if ((customAttributes != null) && (customAttributes.Length > 0))
                        {
                            AttributeUsageAttribute attribute2 = customAttributes[0] as AttributeUsageAttribute;
                            allowMultiple = attribute2.AllowMultiple;
                        }
                    }
                    if (!flag || allowMultiple)
                    {
                        list.Add(AttributeInfoAttribute.CreateAttributeInfoAttribute(attributeType, argumentNames, argumentValues));
                    }
                }
            }
            return list.ToArray();
        }
    }
}

