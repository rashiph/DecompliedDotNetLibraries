namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    internal class ConversionResolution
    {
        private static readonly ConversionClass[][] ConversionTable = new ConversionClass[][] { 
            new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, 
                ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Identity, ConversionClass.Bad, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, 
                ConversionClass.Widening, ConversionClass.Bad, ConversionClass.Widening
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, 
                ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Identity, ConversionClass.None, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.None, ConversionClass.Identity, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Narrowing, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Narrowing, ConversionClass.Widening, ConversionClass.Narrowing, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Narrowing, ConversionClass.Widening, ConversionClass.Narrowing, ConversionClass.Widening, ConversionClass.Narrowing, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Narrowing, ConversionClass.Widening, ConversionClass.Narrowing, ConversionClass.Widening, ConversionClass.Narrowing, ConversionClass.Widening, ConversionClass.Narrowing, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Identity, ConversionClass.Narrowing, ConversionClass.Widening, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Identity, ConversionClass.Widening, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.None, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Widening, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Identity, 
                ConversionClass.None, ConversionClass.Bad, ConversionClass.Narrowing
             }, 
            new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, ConversionClass.None, 
                ConversionClass.Identity, ConversionClass.Bad, ConversionClass.Narrowing
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad, 
                ConversionClass.Bad, ConversionClass.Bad, ConversionClass.Bad
             }, new ConversionClass[] { 
                ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Narrowing, ConversionClass.Widening, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, ConversionClass.Narrowing, 
                ConversionClass.Narrowing, ConversionClass.Bad, ConversionClass.Identity
             }
         };
        internal static readonly TypeCode[][] ForLoopWidestTypeCode;
        internal static readonly int[] NumericSpecificityRank = new int[0x13];

        static ConversionResolution()
        {
            NumericSpecificityRank[6] = 1;
            NumericSpecificityRank[5] = 2;
            NumericSpecificityRank[7] = 3;
            NumericSpecificityRank[8] = 4;
            NumericSpecificityRank[9] = 5;
            NumericSpecificityRank[10] = 6;
            NumericSpecificityRank[11] = 7;
            NumericSpecificityRank[12] = 8;
            NumericSpecificityRank[15] = 9;
            NumericSpecificityRank[13] = 10;
            NumericSpecificityRank[14] = 11;
            ForLoopWidestTypeCode = new TypeCode[][] { 
                new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Int16, TypeCode.Empty, TypeCode.SByte, TypeCode.Int16, TypeCode.Int16, TypeCode.Int32, TypeCode.Int32, TypeCode.Int64, TypeCode.Int64, TypeCode.Decimal, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.SByte, TypeCode.Empty, TypeCode.SByte, TypeCode.Int16, TypeCode.Int16, TypeCode.Int32, TypeCode.Int32, TypeCode.Int64, TypeCode.Int64, TypeCode.Decimal, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Int16, TypeCode.Empty, TypeCode.Int16, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Int16, TypeCode.Empty, TypeCode.Int16, TypeCode.Int16, TypeCode.Int16, TypeCode.Int32, TypeCode.Int32, TypeCode.Int64, TypeCode.Int64, TypeCode.Decimal, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Int32, TypeCode.Empty, TypeCode.Int32, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Int32, TypeCode.Empty, TypeCode.Int32, TypeCode.Int32, TypeCode.Int32, TypeCode.Int32, TypeCode.Int32, TypeCode.Int64, TypeCode.Int64, TypeCode.Decimal, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Int64, TypeCode.Empty, TypeCode.Int64, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Int64, TypeCode.Empty, TypeCode.Int64, TypeCode.Int64, TypeCode.Int64, TypeCode.Int64, TypeCode.Int64, TypeCode.Int64, TypeCode.Int64, TypeCode.Decimal, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Decimal, TypeCode.Empty, TypeCode.Decimal, TypeCode.UInt64, TypeCode.Decimal, TypeCode.UInt64, TypeCode.Decimal, TypeCode.UInt64, TypeCode.Decimal, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Single, TypeCode.Empty, TypeCode.Single, TypeCode.Single, TypeCode.Single, TypeCode.Single, TypeCode.Single, TypeCode.Single, TypeCode.Single, TypeCode.Single, TypeCode.Single, TypeCode.Double, TypeCode.Single, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Double, TypeCode.Empty, TypeCode.Double, TypeCode.Double, TypeCode.Double, TypeCode.Double, TypeCode.Double, TypeCode.Double, TypeCode.Double, TypeCode.Double, TypeCode.Double, TypeCode.Double, TypeCode.Double, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Decimal, TypeCode.Empty, TypeCode.Decimal, TypeCode.Decimal, TypeCode.Decimal, TypeCode.Decimal, TypeCode.Decimal, TypeCode.Decimal, TypeCode.Decimal, TypeCode.Decimal, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, 
                new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }, new TypeCode[] { 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, TypeCode.Empty, 
                    TypeCode.Empty, TypeCode.Empty, TypeCode.Empty
                 }
             };
        }

        private ConversionResolution()
        {
        }

        private static ConversionClass ClassifyCLRArrayToInterfaceConversion(Type TargetInterface, Type SourceArrayType)
        {
            if (!Symbols.Implements(SourceArrayType, TargetInterface))
            {
                if (SourceArrayType.GetArrayRank() > 1)
                {
                    return ConversionClass.Narrowing;
                }
                Type elementType = SourceArrayType.GetElementType();
                ConversionClass none = ConversionClass.None;
                if (TargetInterface.IsGenericType && !TargetInterface.IsGenericTypeDefinition)
                {
                    Type genericTypeDefinition = TargetInterface.GetGenericTypeDefinition();
                    if (((genericTypeDefinition == typeof(IList<>)) || (genericTypeDefinition == typeof(ICollection<>))) || (genericTypeDefinition == typeof(IEnumerable<>)))
                    {
                        none = ClassifyCLRConversionForArrayElementTypes(TargetInterface.GetGenericArguments()[0], elementType);
                    }
                }
                else
                {
                    none = ClassifyPredefinedCLRConversion(TargetInterface, typeof(IList<>).MakeGenericType(new Type[] { elementType }));
                }
                if ((none != ConversionClass.Identity) && (none != ConversionClass.Widening))
                {
                    return ConversionClass.Narrowing;
                }
            }
            return ConversionClass.Widening;
        }

        private static ConversionClass ClassifyCLRConversionForArrayElementTypes(Type TargetElementType, Type SourceElementType)
        {
            if (Symbols.IsReferenceType(SourceElementType) && Symbols.IsReferenceType(TargetElementType))
            {
                return ClassifyPredefinedCLRConversion(TargetElementType, SourceElementType);
            }
            if (Symbols.IsValueType(SourceElementType) && Symbols.IsValueType(TargetElementType))
            {
                return ClassifyPredefinedCLRConversion(TargetElementType, SourceElementType);
            }
            if (Symbols.IsGenericParameter(SourceElementType) && Symbols.IsGenericParameter(TargetElementType))
            {
                if (SourceElementType == TargetElementType)
                {
                    return ConversionClass.Identity;
                }
                if (Symbols.IsReferenceType(SourceElementType) && Symbols.IsOrInheritsFrom(SourceElementType, TargetElementType))
                {
                    return ConversionClass.Widening;
                }
                if (Symbols.IsReferenceType(TargetElementType) && Symbols.IsOrInheritsFrom(TargetElementType, SourceElementType))
                {
                    return ConversionClass.Narrowing;
                }
            }
            return ConversionClass.None;
        }

        internal static ConversionClass ClassifyConversion(Type TargetType, Type SourceType, ref Symbols.Method OperatorMethod)
        {
            ConversionClass class3 = ClassifyPredefinedConversion(TargetType, SourceType);
            if ((((class3 != ConversionClass.None) || Symbols.IsInterface(SourceType)) || Symbols.IsInterface(TargetType)) || (!Symbols.IsClassOrValueType(SourceType) && !Symbols.IsClassOrValueType(TargetType)))
            {
                return class3;
            }
            if (Symbols.IsIntrinsicType(SourceType) && Symbols.IsIntrinsicType(TargetType))
            {
                return class3;
            }
            return ClassifyUserDefinedConversion(TargetType, SourceType, ref OperatorMethod);
        }

        internal static ConversionClass ClassifyIntrinsicConversion(TypeCode TargetTypeCode, TypeCode SourceTypeCode)
        {
            return ConversionTable[(int) TargetTypeCode][(int) SourceTypeCode];
        }

        internal static ConversionClass ClassifyPredefinedCLRConversion(Type TargetType, Type SourceType)
        {
            if (TargetType == SourceType)
            {
                return ConversionClass.Identity;
            }
            if (Symbols.IsRootObjectType(TargetType) || Symbols.IsOrInheritsFrom(SourceType, TargetType))
            {
                return ConversionClass.Widening;
            }
            if (Symbols.IsRootObjectType(SourceType) || Symbols.IsOrInheritsFrom(TargetType, SourceType))
            {
                return ConversionClass.Narrowing;
            }
            if (Symbols.IsInterface(SourceType))
            {
                if ((Symbols.IsClass(TargetType) || Symbols.IsArrayType(TargetType)) || Symbols.IsGenericParameter(TargetType))
                {
                    return ConversionClass.Narrowing;
                }
                if (Symbols.IsInterface(TargetType))
                {
                    return ConversionClass.Narrowing;
                }
                if (!Symbols.IsValueType(TargetType))
                {
                    return ConversionClass.Narrowing;
                }
                if (Symbols.Implements(TargetType, SourceType))
                {
                    return ConversionClass.Narrowing;
                }
                return ConversionClass.None;
            }
            if (Symbols.IsInterface(TargetType))
            {
                if (Symbols.IsArrayType(SourceType))
                {
                    return ClassifyCLRArrayToInterfaceConversion(TargetType, SourceType);
                }
                if (Symbols.IsValueType(SourceType))
                {
                    if (Symbols.Implements(SourceType, TargetType))
                    {
                        return ConversionClass.Widening;
                    }
                    return ConversionClass.None;
                }
                if (Symbols.IsClass(SourceType))
                {
                    if (Symbols.Implements(SourceType, TargetType))
                    {
                        return ConversionClass.Widening;
                    }
                    return ConversionClass.Narrowing;
                }
            }
            if (Symbols.IsEnum(SourceType) || Symbols.IsEnum(TargetType))
            {
                if (Symbols.GetTypeCode(SourceType) != Symbols.GetTypeCode(TargetType))
                {
                    return ConversionClass.None;
                }
                if (Symbols.IsEnum(TargetType))
                {
                    return ConversionClass.Narrowing;
                }
                return ConversionClass.Widening;
            }
            if (Symbols.IsGenericParameter(SourceType))
            {
                if (!Symbols.IsClassOrInterface(TargetType))
                {
                    return ConversionClass.None;
                }
                foreach (Type type2 in Symbols.GetInterfaceConstraints(SourceType))
                {
                    switch (ClassifyPredefinedConversion(TargetType, type2))
                    {
                        case ConversionClass.Widening:
                        case ConversionClass.Identity:
                            return ConversionClass.Widening;
                    }
                }
                Type classConstraint = Symbols.GetClassConstraint(SourceType);
                if (classConstraint != null)
                {
                    switch (ClassifyPredefinedConversion(TargetType, classConstraint))
                    {
                        case ConversionClass.Widening:
                        case ConversionClass.Identity:
                            return ConversionClass.Widening;
                    }
                }
                return Interaction.IIf<ConversionClass>(Symbols.IsInterface(TargetType), ConversionClass.Narrowing, ConversionClass.None);
            }
            if (Symbols.IsGenericParameter(TargetType))
            {
                Type derived = Symbols.GetClassConstraint(TargetType);
                if ((derived != null) && Symbols.IsOrInheritsFrom(derived, SourceType))
                {
                    return ConversionClass.Narrowing;
                }
                return ConversionClass.None;
            }
            if ((Symbols.IsArrayType(SourceType) && Symbols.IsArrayType(TargetType)) && (SourceType.GetArrayRank() == TargetType.GetArrayRank()))
            {
                return ClassifyCLRConversionForArrayElementTypes(TargetType.GetElementType(), SourceType.GetElementType());
            }
            return ConversionClass.None;
        }

        internal static ConversionClass ClassifyPredefinedConversion(Type TargetType, Type SourceType)
        {
            if (TargetType == SourceType)
            {
                return ConversionClass.Identity;
            }
            TypeCode typeCode = Symbols.GetTypeCode(SourceType);
            TypeCode code2 = Symbols.GetTypeCode(TargetType);
            if (Symbols.IsIntrinsicType(typeCode) && Symbols.IsIntrinsicType(code2))
            {
                if ((Symbols.IsEnum(TargetType) && Symbols.IsIntegralType(typeCode)) && Symbols.IsIntegralType(code2))
                {
                    return ConversionClass.Narrowing;
                }
                if ((typeCode == code2) && Symbols.IsEnum(SourceType))
                {
                    return ConversionClass.Widening;
                }
                return ClassifyIntrinsicConversion(code2, typeCode);
            }
            if (Symbols.IsCharArrayRankOne(SourceType) && Symbols.IsStringType(TargetType))
            {
                return ConversionClass.Widening;
            }
            if (Symbols.IsCharArrayRankOne(TargetType) && Symbols.IsStringType(SourceType))
            {
                return ConversionClass.Narrowing;
            }
            return ClassifyPredefinedCLRConversion(TargetType, SourceType);
        }

        internal static ConversionClass ClassifyUserDefinedConversion(Type TargetType, Type SourceType, ref Symbols.Method OperatorMethod)
        {
            ConversionClass class3;
            OperatorCaches.FixedList conversionCache = OperatorCaches.ConversionCache;
            lock (conversionCache)
            {
                if (OperatorCaches.UnconvertibleTypeCache.Lookup(TargetType) && OperatorCaches.UnconvertibleTypeCache.Lookup(SourceType))
                {
                    return ConversionClass.None;
                }
                if (OperatorCaches.ConversionCache.Lookup(TargetType, SourceType, ref class3, ref OperatorMethod))
                {
                    return class3;
                }
            }
            bool foundTargetTypeOperators = false;
            bool foundSourceTypeOperators = false;
            class3 = DoClassifyUserDefinedConversion(TargetType, SourceType, ref OperatorMethod, ref foundTargetTypeOperators, ref foundSourceTypeOperators);
            OperatorCaches.FixedList list2 = OperatorCaches.ConversionCache;
            lock (list2)
            {
                if (!foundTargetTypeOperators)
                {
                    OperatorCaches.UnconvertibleTypeCache.Insert(TargetType);
                }
                if (!foundSourceTypeOperators)
                {
                    OperatorCaches.UnconvertibleTypeCache.Insert(SourceType);
                }
                if (!foundTargetTypeOperators && !foundSourceTypeOperators)
                {
                    return class3;
                }
                OperatorCaches.ConversionCache.Insert(TargetType, SourceType, class3, OperatorMethod);
            }
            return class3;
        }

        private static List<Symbols.Method> CollectConversionOperators(Type TargetType, Type SourceType, ref bool FoundTargetTypeOperators, ref bool FoundSourceTypeOperators)
        {
            if (Symbols.IsIntrinsicType(TargetType))
            {
                TargetType = typeof(object);
            }
            if (Symbols.IsIntrinsicType(SourceType))
            {
                SourceType = typeof(object);
            }
            List<Symbols.Method> list3 = Operators.CollectOperators(Symbols.UserDefinedOperator.Widen, TargetType, SourceType, ref FoundTargetTypeOperators, ref FoundSourceTypeOperators);
            List<Symbols.Method> collection = Operators.CollectOperators(Symbols.UserDefinedOperator.Narrow, TargetType, SourceType, ref FoundTargetTypeOperators, ref FoundSourceTypeOperators);
            list3.AddRange(collection);
            return list3;
        }

        private static ConversionClass DoClassifyUserDefinedConversion(Type TargetType, Type SourceType, ref Symbols.Method OperatorMethod, ref bool FoundTargetTypeOperators, ref bool FoundSourceTypeOperators)
        {
            OperatorMethod = null;
            List<Symbols.Method> operatorSet = CollectConversionOperators(TargetType, SourceType, ref FoundTargetTypeOperators, ref FoundSourceTypeOperators);
            if (operatorSet.Count == 0)
            {
                return ConversionClass.None;
            }
            bool resolutionIsAmbiguous = false;
            List<Symbols.Method> list = ResolveConversion(TargetType, SourceType, operatorSet, true, ref resolutionIsAmbiguous);
            if (list.Count == 1)
            {
                OperatorMethod = list[0];
                OperatorMethod.ArgumentsValidated = true;
                return ConversionClass.Widening;
            }
            if ((list.Count == 0) && !resolutionIsAmbiguous)
            {
                list = ResolveConversion(TargetType, SourceType, operatorSet, false, ref resolutionIsAmbiguous);
                if (list.Count == 1)
                {
                    OperatorMethod = list[0];
                    OperatorMethod.ArgumentsValidated = true;
                    return ConversionClass.Narrowing;
                }
                if (list.Count == 0)
                {
                    return ConversionClass.None;
                }
            }
            return ConversionClass.Ambiguous;
        }

        private static bool Encompasses(Type Larger, Type Smaller)
        {
            ConversionClass class2 = ClassifyPredefinedConversion(Larger, Smaller);
            if ((class2 != ConversionClass.Widening) && (class2 != ConversionClass.Identity))
            {
                return false;
            }
            return true;
        }

        private static void FindBestMatch(Type TargetType, Type SourceType, List<Symbols.Method> SearchList, List<Symbols.Method> ResultList, ref bool GenericMembersExistInList)
        {
            List<Symbols.Method>.Enumerator enumerator;
            try
            {
                enumerator = SearchList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Symbols.Method current = enumerator.Current;
                    MethodBase base2 = current.AsMethod();
                    Type parameterType = base2.GetParameters()[0].ParameterType;
                    Type returnType = ((MethodInfo) base2).ReturnType;
                    if ((parameterType == SourceType) && (returnType == TargetType))
                    {
                        InsertInOperatorListIfLessGenericThanExisting(current, ResultList, ref GenericMembersExistInList);
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private static void InsertInOperatorListIfLessGenericThanExisting(Symbols.Method OperatorToInsert, List<Symbols.Method> OperatorList, ref bool GenericMembersExistInList)
        {
            if (Symbols.IsGeneric(OperatorToInsert.DeclaringType))
            {
                GenericMembersExistInList = true;
            }
            if (GenericMembersExistInList)
            {
                for (int i = OperatorList.Count - 1; i >= 0; i += -1)
                {
                    Symbols.Method left = OperatorList[i];
                    Symbols.Method method2 = OverloadResolution.LeastGenericProcedure(left, OperatorToInsert);
                    if (method2 == left)
                    {
                        return;
                    }
                    if (method2 != null)
                    {
                        OperatorList.Remove(left);
                    }
                }
            }
            OperatorList.Add(OperatorToInsert);
        }

        private static Type MostEncompassed(List<Type> Types)
        {
            Type larger = Types[0];
            int num2 = Types.Count - 1;
            for (int i = 1; i <= num2; i++)
            {
                Type smaller = Types[i];
                if (Encompasses(larger, smaller))
                {
                    larger = smaller;
                }
                else if (!Encompasses(smaller, larger))
                {
                    return null;
                }
            }
            return larger;
        }

        private static Type MostEncompassing(List<Type> Types)
        {
            Type smaller = Types[0];
            int num2 = Types.Count - 1;
            for (int i = 1; i <= num2; i++)
            {
                Type larger = Types[i];
                if (Encompasses(larger, smaller))
                {
                    smaller = larger;
                }
                else if (!Encompasses(smaller, larger))
                {
                    return null;
                }
            }
            return smaller;
        }

        private static bool NotEncompasses(Type Larger, Type Smaller)
        {
            ConversionClass class2 = ClassifyPredefinedConversion(Larger, Smaller);
            if ((class2 != ConversionClass.Narrowing) && (class2 != ConversionClass.Identity))
            {
                return false;
            }
            return true;
        }

        private static List<Symbols.Method> ResolveConversion(Type TargetType, Type SourceType, List<Symbols.Method> OperatorSet, bool WideningOnly, ref bool ResolutionIsAmbiguous)
        {
            ResolutionIsAmbiguous = false;
            Type sourceType = null;
            Type targetType = null;
            bool genericMembersExistInList = false;
            List<Symbols.Method> operatorList = new List<Symbols.Method>(OperatorSet.Count);
            List<Symbols.Method> searchList = new List<Symbols.Method>(OperatorSet.Count);
            List<Type> types = new List<Type>(OperatorSet.Count);
            List<Type> list7 = new List<Type>(OperatorSet.Count);
            List<Type> list5 = null;
            List<Type> list6 = null;
            if (!WideningOnly)
            {
                list5 = new List<Type>(OperatorSet.Count);
                list6 = new List<Type>(OperatorSet.Count);
            }
            foreach (Symbols.Method method in OperatorSet)
            {
                MethodBase base2 = method.AsMethod();
                if (WideningOnly && Symbols.IsNarrowingConversionOperator(base2))
                {
                    break;
                }
                Type parameterType = base2.GetParameters()[0].ParameterType;
                Type returnType = ((MethodInfo) base2).ReturnType;
                if ((!Symbols.IsGeneric(base2) && !Symbols.IsGeneric(base2.DeclaringType)) || (ClassifyPredefinedConversion(returnType, parameterType) == ConversionClass.None))
                {
                    if ((parameterType == SourceType) && (returnType == TargetType))
                    {
                        InsertInOperatorListIfLessGenericThanExisting(method, operatorList, ref genericMembersExistInList);
                    }
                    else if (operatorList.Count == 0)
                    {
                        if (Encompasses(parameterType, SourceType) && Encompasses(TargetType, returnType))
                        {
                            searchList.Add(method);
                            if (parameterType == SourceType)
                            {
                                sourceType = parameterType;
                            }
                            else
                            {
                                types.Add(parameterType);
                            }
                            if (returnType == TargetType)
                            {
                                targetType = returnType;
                            }
                            else
                            {
                                list7.Add(returnType);
                            }
                        }
                        else if ((!WideningOnly && Encompasses(parameterType, SourceType)) && NotEncompasses(TargetType, returnType))
                        {
                            searchList.Add(method);
                            if (parameterType == SourceType)
                            {
                                sourceType = parameterType;
                            }
                            else
                            {
                                types.Add(parameterType);
                            }
                            if (returnType == TargetType)
                            {
                                targetType = returnType;
                            }
                            else
                            {
                                list6.Add(returnType);
                            }
                        }
                        else if ((!WideningOnly && NotEncompasses(parameterType, SourceType)) && NotEncompasses(TargetType, returnType))
                        {
                            searchList.Add(method);
                            if (parameterType == SourceType)
                            {
                                sourceType = parameterType;
                            }
                            else
                            {
                                list5.Add(parameterType);
                            }
                            if (returnType == TargetType)
                            {
                                targetType = returnType;
                            }
                            else
                            {
                                list6.Add(returnType);
                            }
                        }
                    }
                }
            }
            if ((operatorList.Count == 0) && (searchList.Count > 0))
            {
                if (sourceType == null)
                {
                    if (types.Count > 0)
                    {
                        sourceType = MostEncompassed(types);
                    }
                    else
                    {
                        sourceType = MostEncompassing(list5);
                    }
                }
                if (targetType == null)
                {
                    if (list7.Count > 0)
                    {
                        targetType = MostEncompassing(list7);
                    }
                    else
                    {
                        targetType = MostEncompassed(list6);
                    }
                }
                if ((sourceType == null) || (targetType == null))
                {
                    ResolutionIsAmbiguous = true;
                    return new List<Symbols.Method>();
                }
                FindBestMatch(targetType, sourceType, searchList, operatorList, ref genericMembersExistInList);
            }
            if (operatorList.Count > 1)
            {
                ResolutionIsAmbiguous = true;
            }
            return operatorList;
        }

        [Conditional("DEBUG")]
        private static void VerifyTypeCodeEnum()
        {
        }

        internal enum ConversionClass : sbyte
        {
            Ambiguous = 5,
            Bad = 0,
            Identity = 1,
            Narrowing = 3,
            None = 4,
            Widening = 2
        }
    }
}

