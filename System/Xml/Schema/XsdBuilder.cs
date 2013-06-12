namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal sealed class XsdBuilder : SchemaBuilder
    {
        private XmlSchemaAll all;
        private static readonly State[] AllSubelements = new State[] { State.Annotation, State.Element };
        private static readonly State[] AnnotatedSubelements = new State[] { State.Annotation };
        private XmlSchemaAnnotation annotation;
        private static readonly XsdAttributeEntry[] AnnotationAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)) };
        private static readonly State[] AnnotationSubelements = new State[] { State.AppInfo, State.Documentation };
        private XmlSchemaAnyAttribute anyAttribute;
        private static readonly XsdAttributeEntry[] AnyAttributeAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaNamespace, new XsdBuildFunction(XsdBuilder.BuildAnyAttribute_Namespace)), new XsdAttributeEntry(SchemaNames.Token.SchemaProcessContents, new XsdBuildFunction(XsdBuilder.BuildAnyAttribute_ProcessContents)) };
        private static readonly XsdAttributeEntry[] AnyAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaMaxOccurs, new XsdBuildFunction(XsdBuilder.BuildParticle_MaxOccurs)), new XsdAttributeEntry(SchemaNames.Token.SchemaMinOccurs, new XsdBuildFunction(XsdBuilder.BuildParticle_MinOccurs)), new XsdAttributeEntry(SchemaNames.Token.SchemaNamespace, new XsdBuildFunction(XsdBuilder.BuildAny_Namespace)), new XsdAttributeEntry(SchemaNames.Token.SchemaProcessContents, new XsdBuildFunction(XsdBuilder.BuildAny_ProcessContents)) };
        private XmlSchemaAny anyElement;
        private XmlSchemaAppInfo appInfo;
        private static readonly XsdAttributeEntry[] AppinfoAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaSource, new XsdBuildFunction(XsdBuilder.BuildAppinfo_Source)) };
        private XmlSchemaAttribute attribute;
        private static readonly XsdAttributeEntry[] AttributeAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaDefault, new XsdBuildFunction(XsdBuilder.BuildAttribute_Default)), new XsdAttributeEntry(SchemaNames.Token.SchemaFixed, new XsdBuildFunction(XsdBuilder.BuildAttribute_Fixed)), new XsdAttributeEntry(SchemaNames.Token.SchemaForm, new XsdBuildFunction(XsdBuilder.BuildAttribute_Form)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaName, new XsdBuildFunction(XsdBuilder.BuildAttribute_Name)), new XsdAttributeEntry(SchemaNames.Token.SchemaRef, new XsdBuildFunction(XsdBuilder.BuildAttribute_Ref)), new XsdAttributeEntry(SchemaNames.Token.SchemaType, new XsdBuildFunction(XsdBuilder.BuildAttribute_Type)), new XsdAttributeEntry(SchemaNames.Token.SchemaUse, new XsdBuildFunction(XsdBuilder.BuildAttribute_Use)) };
        private XmlSchemaAttributeGroup attributeGroup;
        private static readonly XsdAttributeEntry[] AttributeGroupAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaName, new XsdBuildFunction(XsdBuilder.BuildAttributeGroup_Name)) };
        private XmlSchemaAttributeGroupRef attributeGroupRef;
        private static readonly XsdAttributeEntry[] AttributeGroupRefAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaRef, new XsdBuildFunction(XsdBuilder.BuildAttributeGroupRef_Ref)) };
        private static readonly State[] AttributeGroupSubelements = new State[] { State.Annotation, State.Attribute, State.AttributeGroupRef, State.AnyAttribute };
        private static readonly State[] AttributeSubelements = new State[] { State.Annotation, State.SimpleType };
        private bool canIncludeImport;
        private XmlSchemaChoice choice;
        private static readonly State[] ChoiceSequenceSubelements = new State[] { State.Annotation, State.Element, State.GroupRef, State.Choice, State.Sequence, State.Any };
        private XmlSchemaComplexContent complexContent;
        private static readonly XsdAttributeEntry[] ComplexContentAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaMixed, new XsdBuildFunction(XsdBuilder.BuildComplexContent_Mixed)) };
        private XmlSchemaComplexContentExtension complexContentExtension;
        private static readonly XsdAttributeEntry[] ComplexContentExtensionAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaBase, new XsdBuildFunction(XsdBuilder.BuildComplexContentExtension_Base)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)) };
        private static readonly State[] ComplexContentExtensionSubelements = new State[] { State.Annotation, State.GroupRef, State.All, State.Choice, State.Sequence, State.Attribute, State.AttributeGroupRef, State.AnyAttribute };
        private XmlSchemaComplexContentRestriction complexContentRestriction;
        private static readonly XsdAttributeEntry[] ComplexContentRestrictionAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaBase, new XsdBuildFunction(XsdBuilder.BuildComplexContentRestriction_Base)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)) };
        private static readonly State[] ComplexContentRestrictionSubelements = new State[] { State.Annotation, State.GroupRef, State.All, State.Choice, State.Sequence, State.Attribute, State.AttributeGroupRef, State.AnyAttribute };
        private static readonly State[] ComplexContentSubelements = new State[] { State.Annotation, State.ComplexContentRestriction, State.ComplexContentExtension };
        private XmlSchemaComplexType complexType;
        private static readonly XsdAttributeEntry[] ComplexTypeAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaAbstract, new XsdBuildFunction(XsdBuilder.BuildComplexType_Abstract)), new XsdAttributeEntry(SchemaNames.Token.SchemaBlock, new XsdBuildFunction(XsdBuilder.BuildComplexType_Block)), new XsdAttributeEntry(SchemaNames.Token.SchemaFinal, new XsdBuildFunction(XsdBuilder.BuildComplexType_Final)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaMixed, new XsdBuildFunction(XsdBuilder.BuildComplexType_Mixed)), new XsdAttributeEntry(SchemaNames.Token.SchemaName, new XsdBuildFunction(XsdBuilder.BuildComplexType_Name)) };
        private static readonly State[] ComplexTypeSubelements = new State[] { State.Annotation, State.SimpleContent, State.ComplexContent, State.GroupRef, State.All, State.Choice, State.Sequence, State.Attribute, State.AttributeGroupRef, State.AnyAttribute };
        private Stack containerStack = new Stack();
        private XsdEntry currentEntry;
        private static readonly string[] DerivationMethodStrings = new string[] { "substitution", "extension", "restriction", "list", "union", "#all" };
        private static readonly int[] DerivationMethodValues = new int[] { 1, 2, 4, 8, 0x10, 0xff };
        private XmlSchemaDocumentation documentation;
        private static readonly XsdAttributeEntry[] DocumentationAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaSource, new XsdBuildFunction(XsdBuilder.BuildDocumentation_Source)), new XsdAttributeEntry(SchemaNames.Token.XmlLang, new XsdBuildFunction(XsdBuilder.BuildDocumentation_XmlLang)) };
        private XmlSchemaElement element;
        private static readonly XsdAttributeEntry[] ElementAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaAbstract, new XsdBuildFunction(XsdBuilder.BuildElement_Abstract)), new XsdAttributeEntry(SchemaNames.Token.SchemaBlock, new XsdBuildFunction(XsdBuilder.BuildElement_Block)), new XsdAttributeEntry(SchemaNames.Token.SchemaDefault, new XsdBuildFunction(XsdBuilder.BuildElement_Default)), new XsdAttributeEntry(SchemaNames.Token.SchemaFinal, new XsdBuildFunction(XsdBuilder.BuildElement_Final)), new XsdAttributeEntry(SchemaNames.Token.SchemaFixed, new XsdBuildFunction(XsdBuilder.BuildElement_Fixed)), new XsdAttributeEntry(SchemaNames.Token.SchemaForm, new XsdBuildFunction(XsdBuilder.BuildElement_Form)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaMaxOccurs, new XsdBuildFunction(XsdBuilder.BuildElement_MaxOccurs)), new XsdAttributeEntry(SchemaNames.Token.SchemaMinOccurs, new XsdBuildFunction(XsdBuilder.BuildElement_MinOccurs)), new XsdAttributeEntry(SchemaNames.Token.SchemaName, new XsdBuildFunction(XsdBuilder.BuildElement_Name)), new XsdAttributeEntry(SchemaNames.Token.SchemaNillable, new XsdBuildFunction(XsdBuilder.BuildElement_Nillable)), new XsdAttributeEntry(SchemaNames.Token.SchemaRef, new XsdBuildFunction(XsdBuilder.BuildElement_Ref)), new XsdAttributeEntry(SchemaNames.Token.SchemaSubstitutionGroup, new XsdBuildFunction(XsdBuilder.BuildElement_SubstitutionGroup)), new XsdAttributeEntry(SchemaNames.Token.SchemaType, new XsdBuildFunction(XsdBuilder.BuildElement_Type)) };
        private static readonly State[] ElementSubelements = new State[] { State.Annotation, State.SimpleType, State.ComplexType, State.Unique, State.Key, State.KeyRef };
        private XmlSchemaFacet facet;
        private static readonly XsdAttributeEntry[] FacetAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaFixed, new XsdBuildFunction(XsdBuilder.BuildFacet_Fixed)), new XsdAttributeEntry(SchemaNames.Token.SchemaValue, new XsdBuildFunction(XsdBuilder.BuildFacet_Value)) };
        private static readonly XsdAttributeEntry[] FieldAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaXPath, new XsdBuildFunction(XsdBuilder.BuildField_XPath)) };
        private static readonly string[] FormStringValues = new string[] { "qualified", "unqualified" };
        private XmlSchemaGroup group;
        private static readonly XsdAttributeEntry[] GroupAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaName, new XsdBuildFunction(XsdBuilder.BuildGroup_Name)) };
        private XmlSchemaGroupRef groupRef;
        private static readonly XsdAttributeEntry[] GroupRefAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaMaxOccurs, new XsdBuildFunction(XsdBuilder.BuildParticle_MaxOccurs)), new XsdAttributeEntry(SchemaNames.Token.SchemaMinOccurs, new XsdBuildFunction(XsdBuilder.BuildParticle_MinOccurs)), new XsdAttributeEntry(SchemaNames.Token.SchemaRef, new XsdBuildFunction(XsdBuilder.BuildGroupRef_Ref)) };
        private static readonly State[] GroupSubelements = new State[] { State.Annotation, State.All, State.Choice, State.Sequence };
        private bool hasChild;
        private XmlSchemaIdentityConstraint identityConstraint;
        private static readonly XsdAttributeEntry[] IdentityConstraintAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaName, new XsdBuildFunction(XsdBuilder.BuildIdentityConstraint_Name)), new XsdAttributeEntry(SchemaNames.Token.SchemaRefer, new XsdBuildFunction(XsdBuilder.BuildIdentityConstraint_Refer)) };
        private static readonly State[] IdentityConstraintSubelements = new State[] { State.Annotation, State.Selector, State.Field };
        private XmlSchemaImport import;
        private static readonly XsdAttributeEntry[] ImportAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaNamespace, new XsdBuildFunction(XsdBuilder.BuildImport_Namespace)), new XsdAttributeEntry(SchemaNames.Token.SchemaSchemaLocation, new XsdBuildFunction(XsdBuilder.BuildImport_SchemaLocation)) };
        private XmlSchemaInclude include;
        private static readonly XsdAttributeEntry[] IncludeAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaSchemaLocation, new XsdBuildFunction(XsdBuilder.BuildInclude_SchemaLocation)) };
        private XmlNode[] markup;
        private XmlNamespaceManager namespaceManager;
        private Hashtable namespaces;
        private XmlNameTable nameTable;
        private XsdEntry nextEntry;
        private XmlSchemaNotation notation;
        private static readonly XsdAttributeEntry[] NotationAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaName, new XsdBuildFunction(XsdBuilder.BuildNotation_Name)), new XsdAttributeEntry(SchemaNames.Token.SchemaPublic, new XsdBuildFunction(XsdBuilder.BuildNotation_Public)), new XsdAttributeEntry(SchemaNames.Token.SchemaSystem, new XsdBuildFunction(XsdBuilder.BuildNotation_System)) };
        private XmlSchemaParticle particle;
        private static readonly XsdAttributeEntry[] ParticleAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaMaxOccurs, new XsdBuildFunction(XsdBuilder.BuildParticle_MaxOccurs)), new XsdAttributeEntry(SchemaNames.Token.SchemaMinOccurs, new XsdBuildFunction(XsdBuilder.BuildParticle_MinOccurs)) };
        private PositionInfo positionInfo;
        private static readonly string[] ProcessContentsStringValues = new string[] { "skip", "lax", "strict" };
        private XmlReader reader;
        private XmlSchemaRedefine redefine;
        private static readonly XsdAttributeEntry[] RedefineAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaSchemaLocation, new XsdBuildFunction(XsdBuilder.BuildRedefine_SchemaLocation)) };
        private static readonly State[] RedefineSubelements = new State[] { State.Annotation, State.AttributeGroup, State.ComplexType, State.Group, State.SimpleType };
        private XmlSchema schema;
        private static readonly XsdAttributeEntry[] SchemaAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaAttributeFormDefault, new XsdBuildFunction(XsdBuilder.BuildSchema_AttributeFormDefault)), new XsdAttributeEntry(SchemaNames.Token.SchemaElementFormDefault, new XsdBuildFunction(XsdBuilder.BuildSchema_ElementFormDefault)), new XsdAttributeEntry(SchemaNames.Token.SchemaTargetNamespace, new XsdBuildFunction(XsdBuilder.BuildSchema_TargetNamespace)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaVersion, new XsdBuildFunction(XsdBuilder.BuildSchema_Version)), new XsdAttributeEntry(SchemaNames.Token.SchemaFinalDefault, new XsdBuildFunction(XsdBuilder.BuildSchema_FinalDefault)), new XsdAttributeEntry(SchemaNames.Token.SchemaBlockDefault, new XsdBuildFunction(XsdBuilder.BuildSchema_BlockDefault)) };
        private static readonly State[] SchemaElement = new State[] { State.Schema };
        private static readonly XsdEntry[] SchemaEntries = new XsdEntry[] { 
            new XsdEntry(SchemaNames.Token.Empty, State.Root, SchemaElement, null, null, null, true), new XsdEntry(SchemaNames.Token.XsdSchema, State.Schema, SchemaSubelements, SchemaAttributes, new XsdInitFunction(XsdBuilder.InitSchema), null, true), new XsdEntry(SchemaNames.Token.XsdAnnotation, State.Annotation, AnnotationSubelements, AnnotationAttributes, new XsdInitFunction(XsdBuilder.InitAnnotation), null, true), new XsdEntry(SchemaNames.Token.XsdInclude, State.Include, AnnotatedSubelements, IncludeAttributes, new XsdInitFunction(XsdBuilder.InitInclude), null, true), new XsdEntry(SchemaNames.Token.XsdImport, State.Import, AnnotatedSubelements, ImportAttributes, new XsdInitFunction(XsdBuilder.InitImport), null, true), new XsdEntry(SchemaNames.Token.XsdElement, State.Element, ElementSubelements, ElementAttributes, new XsdInitFunction(XsdBuilder.InitElement), null, true), new XsdEntry(SchemaNames.Token.XsdAttribute, State.Attribute, AttributeSubelements, AttributeAttributes, new XsdInitFunction(XsdBuilder.InitAttribute), null, true), new XsdEntry(SchemaNames.Token.xsdAttributeGroup, State.AttributeGroup, AttributeGroupSubelements, AttributeGroupAttributes, new XsdInitFunction(XsdBuilder.InitAttributeGroup), null, true), new XsdEntry(SchemaNames.Token.xsdAttributeGroup, State.AttributeGroupRef, AnnotatedSubelements, AttributeGroupRefAttributes, new XsdInitFunction(XsdBuilder.InitAttributeGroupRef), null, true), new XsdEntry(SchemaNames.Token.XsdAnyAttribute, State.AnyAttribute, AnnotatedSubelements, AnyAttributeAttributes, new XsdInitFunction(XsdBuilder.InitAnyAttribute), null, true), new XsdEntry(SchemaNames.Token.XsdGroup, State.Group, GroupSubelements, GroupAttributes, new XsdInitFunction(XsdBuilder.InitGroup), null, true), new XsdEntry(SchemaNames.Token.XsdGroup, State.GroupRef, AnnotatedSubelements, GroupRefAttributes, new XsdInitFunction(XsdBuilder.InitGroupRef), null, true), new XsdEntry(SchemaNames.Token.XsdAll, State.All, AllSubelements, ParticleAttributes, new XsdInitFunction(XsdBuilder.InitAll), null, true), new XsdEntry(SchemaNames.Token.XsdChoice, State.Choice, ChoiceSequenceSubelements, ParticleAttributes, new XsdInitFunction(XsdBuilder.InitChoice), null, true), new XsdEntry(SchemaNames.Token.XsdSequence, State.Sequence, ChoiceSequenceSubelements, ParticleAttributes, new XsdInitFunction(XsdBuilder.InitSequence), null, true), new XsdEntry(SchemaNames.Token.XsdAny, State.Any, AnnotatedSubelements, AnyAttributes, new XsdInitFunction(XsdBuilder.InitAny), null, true), 
            new XsdEntry(SchemaNames.Token.XsdNotation, State.Notation, AnnotatedSubelements, NotationAttributes, new XsdInitFunction(XsdBuilder.InitNotation), null, true), new XsdEntry(SchemaNames.Token.XsdSimpleType, State.SimpleType, SimpleTypeSubelements, SimpleTypeAttributes, new XsdInitFunction(XsdBuilder.InitSimpleType), null, true), new XsdEntry(SchemaNames.Token.XsdComplexType, State.ComplexType, ComplexTypeSubelements, ComplexTypeAttributes, new XsdInitFunction(XsdBuilder.InitComplexType), null, true), new XsdEntry(SchemaNames.Token.XsdComplexContent, State.ComplexContent, ComplexContentSubelements, ComplexContentAttributes, new XsdInitFunction(XsdBuilder.InitComplexContent), null, true), new XsdEntry(SchemaNames.Token.XsdComplexContentRestriction, State.ComplexContentRestriction, ComplexContentRestrictionSubelements, ComplexContentRestrictionAttributes, new XsdInitFunction(XsdBuilder.InitComplexContentRestriction), null, true), new XsdEntry(SchemaNames.Token.XsdComplexContentExtension, State.ComplexContentExtension, ComplexContentExtensionSubelements, ComplexContentExtensionAttributes, new XsdInitFunction(XsdBuilder.InitComplexContentExtension), null, true), new XsdEntry(SchemaNames.Token.XsdSimpleContent, State.SimpleContent, SimpleContentSubelements, SimpleContentAttributes, new XsdInitFunction(XsdBuilder.InitSimpleContent), null, true), new XsdEntry(SchemaNames.Token.XsdSimpleContentExtension, State.SimpleContentExtension, SimpleContentExtensionSubelements, SimpleContentExtensionAttributes, new XsdInitFunction(XsdBuilder.InitSimpleContentExtension), null, true), new XsdEntry(SchemaNames.Token.XsdSimpleContentRestriction, State.SimpleContentRestriction, SimpleContentRestrictionSubelements, SimpleContentRestrictionAttributes, new XsdInitFunction(XsdBuilder.InitSimpleContentRestriction), null, true), new XsdEntry(SchemaNames.Token.XsdSimpleTypeUnion, State.SimpleTypeUnion, SimpleTypeUnionSubelements, SimpleTypeUnionAttributes, new XsdInitFunction(XsdBuilder.InitSimpleTypeUnion), null, true), new XsdEntry(SchemaNames.Token.XsdSimpleTypeList, State.SimpleTypeList, SimpleTypeListSubelements, SimpleTypeListAttributes, new XsdInitFunction(XsdBuilder.InitSimpleTypeList), null, true), new XsdEntry(SchemaNames.Token.XsdSimpleTypeRestriction, State.SimpleTypeRestriction, SimpleTypeRestrictionSubelements, SimpleTypeRestrictionAttributes, new XsdInitFunction(XsdBuilder.InitSimpleTypeRestriction), null, true), new XsdEntry(SchemaNames.Token.XsdUnique, State.Unique, IdentityConstraintSubelements, IdentityConstraintAttributes, new XsdInitFunction(XsdBuilder.InitIdentityConstraint), null, true), new XsdEntry(SchemaNames.Token.XsdKey, State.Key, IdentityConstraintSubelements, IdentityConstraintAttributes, new XsdInitFunction(XsdBuilder.InitIdentityConstraint), null, true), new XsdEntry(SchemaNames.Token.XsdKeyref, State.KeyRef, IdentityConstraintSubelements, IdentityConstraintAttributes, new XsdInitFunction(XsdBuilder.InitIdentityConstraint), null, true), new XsdEntry(SchemaNames.Token.XsdSelector, State.Selector, AnnotatedSubelements, SelectorAttributes, new XsdInitFunction(XsdBuilder.InitSelector), null, true), 
            new XsdEntry(SchemaNames.Token.XsdField, State.Field, AnnotatedSubelements, FieldAttributes, new XsdInitFunction(XsdBuilder.InitField), null, true), new XsdEntry(SchemaNames.Token.XsdMinExclusive, State.MinExclusive, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdMinInclusive, State.MinInclusive, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdMaxExclusive, State.MaxExclusive, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdMaxInclusive, State.MaxInclusive, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdTotalDigits, State.TotalDigits, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdFractionDigits, State.FractionDigits, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdLength, State.Length, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdMinLength, State.MinLength, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdMaxLength, State.MaxLength, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdEnumeration, State.Enumeration, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdPattern, State.Pattern, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdWhitespace, State.WhiteSpace, AnnotatedSubelements, FacetAttributes, new XsdInitFunction(XsdBuilder.InitFacet), null, true), new XsdEntry(SchemaNames.Token.XsdAppInfo, State.AppInfo, null, AppinfoAttributes, new XsdInitFunction(XsdBuilder.InitAppinfo), new XsdEndChildFunction(XsdBuilder.EndAppinfo), false), new XsdEntry(SchemaNames.Token.XsdDocumentation, State.Documentation, null, DocumentationAttributes, new XsdInitFunction(XsdBuilder.InitDocumentation), new XsdEndChildFunction(XsdBuilder.EndDocumentation), false), new XsdEntry(SchemaNames.Token.XsdRedefine, State.Redefine, RedefineSubelements, RedefineAttributes, new XsdInitFunction(XsdBuilder.InitRedefine), new XsdEndChildFunction(XsdBuilder.EndRedefine), true)
         };
        private SchemaNames schemaNames;
        private static readonly State[] SchemaSubelements = new State[] { State.Annotation, State.Include, State.Import, State.Redefine, State.ComplexType, State.SimpleType, State.Element, State.Attribute, State.AttributeGroup, State.Group, State.Notation };
        private static readonly XsdAttributeEntry[] SelectorAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaXPath, new XsdBuildFunction(XsdBuilder.BuildSelector_XPath)) };
        private XmlSchemaSequence sequence;
        private XmlSchemaSimpleContent simpleContent;
        private static readonly XsdAttributeEntry[] SimpleContentAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)) };
        private XmlSchemaSimpleContentExtension simpleContentExtension;
        private static readonly XsdAttributeEntry[] SimpleContentExtensionAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaBase, new XsdBuildFunction(XsdBuilder.BuildSimpleContentExtension_Base)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)) };
        private static readonly State[] SimpleContentExtensionSubelements = new State[] { State.Annotation, State.Attribute, State.AttributeGroupRef, State.AnyAttribute };
        private XmlSchemaSimpleContentRestriction simpleContentRestriction;
        private static readonly XsdAttributeEntry[] SimpleContentRestrictionAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaBase, new XsdBuildFunction(XsdBuilder.BuildSimpleContentRestriction_Base)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)) };
        private static readonly State[] SimpleContentRestrictionSubelements = new State[] { 
            State.Annotation, State.SimpleType, State.Enumeration, State.Length, State.MaxExclusive, State.MaxInclusive, State.MaxLength, State.MinExclusive, State.MinInclusive, State.MinLength, State.Pattern, State.TotalDigits, State.FractionDigits, State.WhiteSpace, State.Attribute, State.AttributeGroupRef, 
            State.AnyAttribute
         };
        private static readonly State[] SimpleContentSubelements = new State[] { State.Annotation, State.SimpleContentRestriction, State.SimpleContentExtension };
        private XmlSchemaSimpleType simpleType;
        private static readonly XsdAttributeEntry[] SimpleTypeAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaFinal, new XsdBuildFunction(XsdBuilder.BuildSimpleType_Final)), new XsdAttributeEntry(SchemaNames.Token.SchemaName, new XsdBuildFunction(XsdBuilder.BuildSimpleType_Name)) };
        private XmlSchemaSimpleTypeList simpleTypeList;
        private static readonly XsdAttributeEntry[] SimpleTypeListAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaItemType, new XsdBuildFunction(XsdBuilder.BuildSimpleTypeList_ItemType)) };
        private static readonly State[] SimpleTypeListSubelements = new State[] { State.Annotation, State.SimpleType };
        private XmlSchemaSimpleTypeRestriction simpleTypeRestriction;
        private static readonly XsdAttributeEntry[] SimpleTypeRestrictionAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaBase, new XsdBuildFunction(XsdBuilder.BuildSimpleTypeRestriction_Base)), new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)) };
        private static readonly State[] SimpleTypeRestrictionSubelements = new State[] { State.Annotation, State.SimpleType, State.Enumeration, State.Length, State.MaxExclusive, State.MaxInclusive, State.MaxLength, State.MinExclusive, State.MinInclusive, State.MinLength, State.Pattern, State.TotalDigits, State.FractionDigits, State.WhiteSpace };
        private static readonly State[] SimpleTypeSubelements = new State[] { State.Annotation, State.SimpleTypeList, State.SimpleTypeRestriction, State.SimpleTypeUnion };
        private XmlSchemaSimpleTypeUnion simpleTypeUnion;
        private static readonly XsdAttributeEntry[] SimpleTypeUnionAttributes = new XsdAttributeEntry[] { new XsdAttributeEntry(SchemaNames.Token.SchemaId, new XsdBuildFunction(XsdBuilder.BuildAnnotated_Id)), new XsdAttributeEntry(SchemaNames.Token.SchemaMemberTypes, new XsdBuildFunction(XsdBuilder.BuildSimpleTypeUnion_MemberTypes)) };
        private static readonly State[] SimpleTypeUnionSubelements = new State[] { State.Annotation, State.SimpleType };
        private const int STACK_INCREMENT = 10;
        private HWStack stateHistory = new HWStack(10);
        private ArrayList unhandledAttributes = new ArrayList();
        private static readonly string[] UseStringValues = new string[] { "optional", "prohibited", "required" };
        private ValidationEventHandler validationEventHandler;
        private XmlSchemaXPath xpath;
        private XmlSchemaObject xso;

        internal XsdBuilder(XmlReader reader, XmlNamespaceManager curmgr, XmlSchema schema, XmlNameTable nameTable, SchemaNames schemaNames, ValidationEventHandler eventhandler)
        {
            this.reader = reader;
            this.xso = this.schema = schema;
            this.namespaceManager = new BuilderNamespaceManager(curmgr, reader);
            this.validationEventHandler = eventhandler;
            this.nameTable = nameTable;
            this.schemaNames = schemaNames;
            this.stateHistory = new HWStack(10);
            this.currentEntry = SchemaEntries[0];
            this.positionInfo = PositionInfo.GetPositionInfo(reader);
        }

        private void AddAttribute(XmlSchemaObject value)
        {
            switch (this.ParentElement)
            {
                case SchemaNames.Token.XsdComplexContentExtension:
                    if (this.complexContentExtension.AnyAttribute != null)
                    {
                        this.SendValidationEvent("Sch_AnyAttributeLastChild", null);
                    }
                    this.complexContentExtension.Attributes.Add(value);
                    return;

                case SchemaNames.Token.XsdComplexContentRestriction:
                    if (this.complexContentRestriction.AnyAttribute != null)
                    {
                        this.SendValidationEvent("Sch_AnyAttributeLastChild", null);
                    }
                    this.complexContentRestriction.Attributes.Add(value);
                    return;

                case SchemaNames.Token.XsdSimpleContent:
                    break;

                case SchemaNames.Token.XsdSimpleContentExtension:
                    if (this.simpleContentExtension.AnyAttribute != null)
                    {
                        this.SendValidationEvent("Sch_AnyAttributeLastChild", null);
                    }
                    this.simpleContentExtension.Attributes.Add(value);
                    return;

                case SchemaNames.Token.XsdSimpleContentRestriction:
                    if (this.simpleContentRestriction.AnyAttribute != null)
                    {
                        this.SendValidationEvent("Sch_AnyAttributeLastChild", null);
                    }
                    this.simpleContentRestriction.Attributes.Add(value);
                    return;

                case SchemaNames.Token.XsdComplexType:
                    if (this.complexType.ContentModel != null)
                    {
                        this.SendValidationEvent("Sch_AttributeMutuallyExclusive", "attribute");
                    }
                    if (this.complexType.AnyAttribute != null)
                    {
                        this.SendValidationEvent("Sch_AnyAttributeLastChild", null);
                    }
                    this.complexType.Attributes.Add(value);
                    return;

                case SchemaNames.Token.xsdAttributeGroup:
                    if (this.attributeGroup.AnyAttribute != null)
                    {
                        this.SendValidationEvent("Sch_AnyAttributeLastChild", null);
                    }
                    this.attributeGroup.Attributes.Add(value);
                    break;

                default:
                    return;
            }
        }

        private void AddParticle(XmlSchemaParticle particle)
        {
            switch (this.ParentElement)
            {
                case SchemaNames.Token.XsdGroup:
                    if (this.group.Particle != null)
                    {
                        this.SendValidationEvent("Sch_DupGroupParticle", "particle");
                    }
                    this.group.Particle = (XmlSchemaGroupBase) particle;
                    return;

                case SchemaNames.Token.XsdAll:
                    break;

                case SchemaNames.Token.XsdChoice:
                case SchemaNames.Token.XsdSequence:
                    ((XmlSchemaGroupBase) this.ParentContainer).Items.Add(particle);
                    break;

                case SchemaNames.Token.XsdComplexType:
                    if (((this.complexType.ContentModel != null) || (this.complexType.Attributes.Count != 0)) || ((this.complexType.AnyAttribute != null) || (this.complexType.Particle != null)))
                    {
                        this.SendValidationEvent("Sch_ComplexTypeContentModel", "complexType");
                    }
                    this.complexType.Particle = particle;
                    return;

                case SchemaNames.Token.XsdComplexContentExtension:
                    if (((this.complexContentExtension.Particle != null) || (this.complexContentExtension.Attributes.Count != 0)) || (this.complexContentExtension.AnyAttribute != null))
                    {
                        this.SendValidationEvent("Sch_ComplexContentContentModel", "ComplexContentExtension");
                    }
                    this.complexContentExtension.Particle = particle;
                    return;

                case SchemaNames.Token.XsdComplexContentRestriction:
                    if (((this.complexContentRestriction.Particle != null) || (this.complexContentRestriction.Attributes.Count != 0)) || (this.complexContentRestriction.AnyAttribute != null))
                    {
                        this.SendValidationEvent("Sch_ComplexContentContentModel", "ComplexContentExtension");
                    }
                    this.complexContentRestriction.Particle = particle;
                    return;

                default:
                    return;
            }
        }

        private static void BuildAnnotated_Id(XsdBuilder builder, string value)
        {
            builder.xso.IdAttribute = value;
        }

        private static void BuildAny_Namespace(XsdBuilder builder, string value)
        {
            builder.anyElement.Namespace = value;
        }

        private static void BuildAny_ProcessContents(XsdBuilder builder, string value)
        {
            builder.anyElement.ProcessContents = (XmlSchemaContentProcessing) builder.ParseEnum(value, "processContents", ProcessContentsStringValues);
        }

        private static void BuildAnyAttribute_Namespace(XsdBuilder builder, string value)
        {
            builder.anyAttribute.Namespace = value;
        }

        private static void BuildAnyAttribute_ProcessContents(XsdBuilder builder, string value)
        {
            builder.anyAttribute.ProcessContents = (XmlSchemaContentProcessing) builder.ParseEnum(value, "processContents", ProcessContentsStringValues);
        }

        private static void BuildAppinfo_Source(XsdBuilder builder, string value)
        {
            builder.appInfo.Source = ParseUriReference(value);
        }

        private static void BuildAttribute_Default(XsdBuilder builder, string value)
        {
            builder.attribute.DefaultValue = value;
        }

        private static void BuildAttribute_Fixed(XsdBuilder builder, string value)
        {
            builder.attribute.FixedValue = value;
        }

        private static void BuildAttribute_Form(XsdBuilder builder, string value)
        {
            builder.attribute.Form = (XmlSchemaForm) builder.ParseEnum(value, "form", FormStringValues);
        }

        private static void BuildAttribute_Name(XsdBuilder builder, string value)
        {
            builder.attribute.Name = value;
        }

        private static void BuildAttribute_Ref(XsdBuilder builder, string value)
        {
            builder.attribute.RefName = builder.ParseQName(value, "ref");
        }

        private static void BuildAttribute_Type(XsdBuilder builder, string value)
        {
            builder.attribute.SchemaTypeName = builder.ParseQName(value, "type");
        }

        private static void BuildAttribute_Use(XsdBuilder builder, string value)
        {
            builder.attribute.Use = (XmlSchemaUse) builder.ParseEnum(value, "use", UseStringValues);
        }

        private static void BuildAttributeGroup_Name(XsdBuilder builder, string value)
        {
            builder.attributeGroup.Name = value;
        }

        private static void BuildAttributeGroupRef_Ref(XsdBuilder builder, string value)
        {
            builder.attributeGroupRef.RefName = builder.ParseQName(value, "ref");
        }

        private static void BuildComplexContent_Mixed(XsdBuilder builder, string value)
        {
            builder.complexContent.IsMixed = builder.ParseBoolean(value, "mixed");
        }

        private static void BuildComplexContentExtension_Base(XsdBuilder builder, string value)
        {
            builder.complexContentExtension.BaseTypeName = builder.ParseQName(value, "base");
        }

        private static void BuildComplexContentRestriction_Base(XsdBuilder builder, string value)
        {
            builder.complexContentRestriction.BaseTypeName = builder.ParseQName(value, "base");
        }

        private static void BuildComplexType_Abstract(XsdBuilder builder, string value)
        {
            builder.complexType.IsAbstract = builder.ParseBoolean(value, "abstract");
        }

        private static void BuildComplexType_Block(XsdBuilder builder, string value)
        {
            builder.complexType.Block = (XmlSchemaDerivationMethod) builder.ParseBlockFinalEnum(value, "block");
        }

        private static void BuildComplexType_Final(XsdBuilder builder, string value)
        {
            builder.complexType.Final = (XmlSchemaDerivationMethod) builder.ParseBlockFinalEnum(value, "final");
        }

        private static void BuildComplexType_Mixed(XsdBuilder builder, string value)
        {
            builder.complexType.IsMixed = builder.ParseBoolean(value, "mixed");
        }

        private static void BuildComplexType_Name(XsdBuilder builder, string value)
        {
            builder.complexType.Name = value;
        }

        private static void BuildDocumentation_Source(XsdBuilder builder, string value)
        {
            builder.documentation.Source = ParseUriReference(value);
        }

        private static void BuildDocumentation_XmlLang(XsdBuilder builder, string value)
        {
            try
            {
                builder.documentation.Language = value;
            }
            catch (XmlSchemaException exception)
            {
                exception.SetSource(builder.reader.BaseURI, builder.positionInfo.LineNumber, builder.positionInfo.LinePosition);
                builder.SendValidationEvent(exception);
            }
        }

        private static void BuildElement_Abstract(XsdBuilder builder, string value)
        {
            builder.element.IsAbstract = builder.ParseBoolean(value, "abstract");
        }

        private static void BuildElement_Block(XsdBuilder builder, string value)
        {
            builder.element.Block = (XmlSchemaDerivationMethod) builder.ParseBlockFinalEnum(value, "block");
        }

        private static void BuildElement_Default(XsdBuilder builder, string value)
        {
            builder.element.DefaultValue = value;
        }

        private static void BuildElement_Final(XsdBuilder builder, string value)
        {
            builder.element.Final = (XmlSchemaDerivationMethod) builder.ParseBlockFinalEnum(value, "final");
        }

        private static void BuildElement_Fixed(XsdBuilder builder, string value)
        {
            builder.element.FixedValue = value;
        }

        private static void BuildElement_Form(XsdBuilder builder, string value)
        {
            builder.element.Form = (XmlSchemaForm) builder.ParseEnum(value, "form", FormStringValues);
        }

        private static void BuildElement_MaxOccurs(XsdBuilder builder, string value)
        {
            builder.SetMaxOccurs(builder.element, value);
        }

        private static void BuildElement_MinOccurs(XsdBuilder builder, string value)
        {
            builder.SetMinOccurs(builder.element, value);
        }

        private static void BuildElement_Name(XsdBuilder builder, string value)
        {
            builder.element.Name = value;
        }

        private static void BuildElement_Nillable(XsdBuilder builder, string value)
        {
            builder.element.IsNillable = builder.ParseBoolean(value, "nillable");
        }

        private static void BuildElement_Ref(XsdBuilder builder, string value)
        {
            builder.element.RefName = builder.ParseQName(value, "ref");
        }

        private static void BuildElement_SubstitutionGroup(XsdBuilder builder, string value)
        {
            builder.element.SubstitutionGroup = builder.ParseQName(value, "substitutionGroup");
        }

        private static void BuildElement_Type(XsdBuilder builder, string value)
        {
            builder.element.SchemaTypeName = builder.ParseQName(value, "type");
        }

        private static void BuildFacet_Fixed(XsdBuilder builder, string value)
        {
            builder.facet.IsFixed = builder.ParseBoolean(value, "fixed");
        }

        private static void BuildFacet_Value(XsdBuilder builder, string value)
        {
            builder.facet.Value = value;
        }

        private static void BuildField_XPath(XsdBuilder builder, string value)
        {
            builder.xpath.XPath = value;
        }

        private static void BuildGroup_Name(XsdBuilder builder, string value)
        {
            builder.group.Name = value;
        }

        private static void BuildGroupRef_Ref(XsdBuilder builder, string value)
        {
            builder.groupRef.RefName = builder.ParseQName(value, "ref");
        }

        private static void BuildIdentityConstraint_Name(XsdBuilder builder, string value)
        {
            builder.identityConstraint.Name = value;
        }

        private static void BuildIdentityConstraint_Refer(XsdBuilder builder, string value)
        {
            if (builder.identityConstraint is XmlSchemaKeyref)
            {
                ((XmlSchemaKeyref) builder.identityConstraint).Refer = builder.ParseQName(value, "refer");
            }
            else
            {
                builder.SendValidationEvent("Sch_UnsupportedAttribute", "refer");
            }
        }

        private static void BuildImport_Namespace(XsdBuilder builder, string value)
        {
            builder.import.Namespace = value;
        }

        private static void BuildImport_SchemaLocation(XsdBuilder builder, string value)
        {
            builder.import.SchemaLocation = value;
        }

        private static void BuildInclude_SchemaLocation(XsdBuilder builder, string value)
        {
            builder.include.SchemaLocation = value;
        }

        private static void BuildNotation_Name(XsdBuilder builder, string value)
        {
            builder.notation.Name = value;
        }

        private static void BuildNotation_Public(XsdBuilder builder, string value)
        {
            builder.notation.Public = value;
        }

        private static void BuildNotation_System(XsdBuilder builder, string value)
        {
            builder.notation.System = value;
        }

        private static void BuildParticle_MaxOccurs(XsdBuilder builder, string value)
        {
            builder.SetMaxOccurs(builder.particle, value);
        }

        private static void BuildParticle_MinOccurs(XsdBuilder builder, string value)
        {
            builder.SetMinOccurs(builder.particle, value);
        }

        private static void BuildRedefine_SchemaLocation(XsdBuilder builder, string value)
        {
            builder.redefine.SchemaLocation = value;
        }

        private static void BuildSchema_AttributeFormDefault(XsdBuilder builder, string value)
        {
            builder.schema.AttributeFormDefault = (XmlSchemaForm) builder.ParseEnum(value, "attributeFormDefault", FormStringValues);
        }

        private static void BuildSchema_BlockDefault(XsdBuilder builder, string value)
        {
            builder.schema.BlockDefault = (XmlSchemaDerivationMethod) builder.ParseBlockFinalEnum(value, "blockDefault");
        }

        private static void BuildSchema_ElementFormDefault(XsdBuilder builder, string value)
        {
            builder.schema.ElementFormDefault = (XmlSchemaForm) builder.ParseEnum(value, "elementFormDefault", FormStringValues);
        }

        private static void BuildSchema_FinalDefault(XsdBuilder builder, string value)
        {
            builder.schema.FinalDefault = (XmlSchemaDerivationMethod) builder.ParseBlockFinalEnum(value, "finalDefault");
        }

        private static void BuildSchema_TargetNamespace(XsdBuilder builder, string value)
        {
            builder.schema.TargetNamespace = value;
        }

        private static void BuildSchema_Version(XsdBuilder builder, string value)
        {
            builder.schema.Version = value;
        }

        private static void BuildSelector_XPath(XsdBuilder builder, string value)
        {
            builder.xpath.XPath = value;
        }

        private static void BuildSimpleContentExtension_Base(XsdBuilder builder, string value)
        {
            builder.simpleContentExtension.BaseTypeName = builder.ParseQName(value, "base");
        }

        private static void BuildSimpleContentRestriction_Base(XsdBuilder builder, string value)
        {
            builder.simpleContentRestriction.BaseTypeName = builder.ParseQName(value, "base");
        }

        private static void BuildSimpleType_Final(XsdBuilder builder, string value)
        {
            builder.simpleType.Final = (XmlSchemaDerivationMethod) builder.ParseBlockFinalEnum(value, "final");
        }

        private static void BuildSimpleType_Name(XsdBuilder builder, string value)
        {
            builder.simpleType.Name = value;
        }

        private static void BuildSimpleTypeList_ItemType(XsdBuilder builder, string value)
        {
            builder.simpleTypeList.ItemTypeName = builder.ParseQName(value, "itemType");
        }

        private static void BuildSimpleTypeRestriction_Base(XsdBuilder builder, string value)
        {
            builder.simpleTypeRestriction.BaseTypeName = builder.ParseQName(value, "base");
        }

        private static void BuildSimpleTypeUnion_MemberTypes(XsdBuilder builder, string value)
        {
            XmlSchemaDatatype datatype = XmlSchemaDatatype.FromXmlTokenizedTypeXsd(XmlTokenizedType.QName).DeriveByList(null);
            try
            {
                builder.simpleTypeUnion.MemberTypes = (XmlQualifiedName[]) datatype.ParseValue(value, builder.nameTable, builder.namespaceManager);
            }
            catch (XmlSchemaException exception)
            {
                exception.SetSource(builder.reader.BaseURI, builder.positionInfo.LineNumber, builder.positionInfo.LinePosition);
                builder.SendValidationEvent(exception);
            }
        }

        private static void EndAppinfo(XsdBuilder builder)
        {
            builder.appInfo.Markup = builder.markup;
        }

        internal override void EndChildren()
        {
            if (this.currentEntry.EndChildFunc != null)
            {
                this.currentEntry.EndChildFunc(this);
            }
            this.Pop();
        }

        private static void EndDocumentation(XsdBuilder builder)
        {
            builder.documentation.Markup = builder.markup;
        }

        private static void EndRedefine(XsdBuilder builder)
        {
            builder.canIncludeImport = true;
        }

        private XmlSchemaObject GetContainer(State state)
        {
            XmlSchemaObject obj2 = null;
            switch (state)
            {
                case State.Root:
                    return obj2;

                case State.Schema:
                    return this.schema;

                case State.Annotation:
                    return this.annotation;

                case State.Include:
                    return this.include;

                case State.Import:
                    return this.import;

                case State.Element:
                    return this.element;

                case State.Attribute:
                    return this.attribute;

                case State.AttributeGroup:
                    return this.attributeGroup;

                case State.AttributeGroupRef:
                    return this.attributeGroupRef;

                case State.AnyAttribute:
                    return this.anyAttribute;

                case State.Group:
                    return this.group;

                case State.GroupRef:
                    return this.groupRef;

                case State.All:
                    return this.all;

                case State.Choice:
                    return this.choice;

                case State.Sequence:
                    return this.sequence;

                case State.Any:
                    return this.anyElement;

                case State.Notation:
                    return this.notation;

                case State.SimpleType:
                    return this.simpleType;

                case State.ComplexType:
                    return this.complexType;

                case State.ComplexContent:
                    return this.complexContent;

                case State.ComplexContentRestriction:
                    return this.complexContentRestriction;

                case State.ComplexContentExtension:
                    return this.complexContentExtension;

                case State.SimpleContent:
                    return this.simpleContent;

                case State.SimpleContentExtension:
                    return this.simpleContentExtension;

                case State.SimpleContentRestriction:
                    return this.simpleContentRestriction;

                case State.SimpleTypeUnion:
                    return this.simpleTypeUnion;

                case State.SimpleTypeList:
                    return this.simpleTypeList;

                case State.SimpleTypeRestriction:
                    return this.simpleTypeRestriction;

                case State.Unique:
                case State.Key:
                case State.KeyRef:
                    return this.identityConstraint;

                case State.Selector:
                case State.Field:
                    return this.xpath;

                case State.MinExclusive:
                case State.MinInclusive:
                case State.MaxExclusive:
                case State.MaxInclusive:
                case State.TotalDigits:
                case State.FractionDigits:
                case State.Length:
                case State.MinLength:
                case State.MaxLength:
                case State.Enumeration:
                case State.Pattern:
                case State.WhiteSpace:
                    return this.facet;

                case State.AppInfo:
                    return this.appInfo;

                case State.Documentation:
                    return this.documentation;

                case State.Redefine:
                    return this.redefine;
            }
            return obj2;
        }

        private bool GetNextState(XmlQualifiedName qname)
        {
            if (this.currentEntry.NextStates != null)
            {
                for (int i = 0; i < this.currentEntry.NextStates.Length; i++)
                {
                    int index = (int) this.currentEntry.NextStates[i];
                    if (this.schemaNames.TokenToQName[(int) SchemaEntries[index].Name].Equals(qname))
                    {
                        this.nextEntry = SchemaEntries[index];
                        return true;
                    }
                }
            }
            return false;
        }

        private static void InitAll(XsdBuilder builder, string value)
        {
            builder.xso = builder.particle = builder.all = new XmlSchemaAll();
            builder.AddParticle(builder.all);
        }

        private static void InitAnnotation(XsdBuilder builder, string value)
        {
            if ((builder.hasChild && (builder.ParentElement != SchemaNames.Token.XsdSchema)) && (builder.ParentElement != SchemaNames.Token.XsdRedefine))
            {
                builder.SendValidationEvent("Sch_AnnotationLocation", null);
            }
            builder.xso = builder.annotation = new XmlSchemaAnnotation();
            builder.ParentContainer.AddAnnotation(builder.annotation);
        }

        private static void InitAny(XsdBuilder builder, string value)
        {
            builder.xso = builder.particle = builder.anyElement = new XmlSchemaAny();
            builder.AddParticle(builder.anyElement);
        }

        private static void InitAnyAttribute(XsdBuilder builder, string value)
        {
            builder.xso = builder.anyAttribute = new XmlSchemaAnyAttribute();
            switch (builder.ParentElement)
            {
                case SchemaNames.Token.XsdComplexContentExtension:
                    if (builder.complexContentExtension.AnyAttribute != null)
                    {
                        builder.SendValidationEvent("Sch_DupElement", "anyAttribute");
                    }
                    builder.complexContentExtension.AnyAttribute = builder.anyAttribute;
                    return;

                case SchemaNames.Token.XsdComplexContentRestriction:
                    if (builder.complexContentRestriction.AnyAttribute != null)
                    {
                        builder.SendValidationEvent("Sch_DupElement", "anyAttribute");
                    }
                    builder.complexContentRestriction.AnyAttribute = builder.anyAttribute;
                    return;

                case SchemaNames.Token.XsdSimpleContent:
                    break;

                case SchemaNames.Token.XsdSimpleContentExtension:
                    if (builder.simpleContentExtension.AnyAttribute != null)
                    {
                        builder.SendValidationEvent("Sch_DupElement", "anyAttribute");
                    }
                    builder.simpleContentExtension.AnyAttribute = builder.anyAttribute;
                    return;

                case SchemaNames.Token.XsdSimpleContentRestriction:
                    if (builder.simpleContentRestriction.AnyAttribute != null)
                    {
                        builder.SendValidationEvent("Sch_DupElement", "anyAttribute");
                    }
                    builder.simpleContentRestriction.AnyAttribute = builder.anyAttribute;
                    return;

                case SchemaNames.Token.XsdComplexType:
                    if (builder.complexType.ContentModel != null)
                    {
                        builder.SendValidationEvent("Sch_AttributeMutuallyExclusive", "anyAttribute");
                    }
                    if (builder.complexType.AnyAttribute != null)
                    {
                        builder.SendValidationEvent("Sch_DupElement", "anyAttribute");
                    }
                    builder.complexType.AnyAttribute = builder.anyAttribute;
                    return;

                case SchemaNames.Token.xsdAttributeGroup:
                    if (builder.attributeGroup.AnyAttribute != null)
                    {
                        builder.SendValidationEvent("Sch_DupElement", "anyAttribute");
                    }
                    builder.attributeGroup.AnyAttribute = builder.anyAttribute;
                    break;

                default:
                    return;
            }
        }

        private static void InitAppinfo(XsdBuilder builder, string value)
        {
            builder.xso = builder.appInfo = new XmlSchemaAppInfo();
            builder.annotation.Items.Add(builder.appInfo);
            builder.markup = new XmlNode[0];
        }

        private static void InitAttribute(XsdBuilder builder, string value)
        {
            builder.xso = builder.attribute = new XmlSchemaAttribute();
            if (builder.ParentElement == SchemaNames.Token.XsdSchema)
            {
                builder.schema.Items.Add(builder.attribute);
            }
            else
            {
                builder.AddAttribute(builder.attribute);
            }
            builder.canIncludeImport = false;
        }

        private static void InitAttributeGroup(XsdBuilder builder, string value)
        {
            builder.canIncludeImport = false;
            builder.xso = builder.attributeGroup = new XmlSchemaAttributeGroup();
            SchemaNames.Token parentElement = builder.ParentElement;
            if (parentElement != SchemaNames.Token.XsdSchema)
            {
                if (parentElement != SchemaNames.Token.XsdRedefine)
                {
                    return;
                }
            }
            else
            {
                builder.schema.Items.Add(builder.attributeGroup);
                return;
            }
            builder.redefine.Items.Add(builder.attributeGroup);
        }

        private static void InitAttributeGroupRef(XsdBuilder builder, string value)
        {
            builder.xso = builder.attributeGroupRef = new XmlSchemaAttributeGroupRef();
            builder.AddAttribute(builder.attributeGroupRef);
        }

        private static void InitChoice(XsdBuilder builder, string value)
        {
            builder.xso = builder.particle = builder.choice = new XmlSchemaChoice();
            builder.AddParticle(builder.choice);
        }

        private static void InitComplexContent(XsdBuilder builder, string value)
        {
            if (((builder.complexType.ContentModel != null) || (builder.complexType.Particle != null)) || ((builder.complexType.Attributes.Count != 0) || (builder.complexType.AnyAttribute != null)))
            {
                builder.SendValidationEvent("Sch_ComplexTypeContentModel", "complexContent");
            }
            builder.xso = builder.complexContent = new XmlSchemaComplexContent();
            builder.complexType.ContentModel = builder.complexContent;
        }

        private static void InitComplexContentExtension(XsdBuilder builder, string value)
        {
            if (builder.complexContent.Content != null)
            {
                builder.SendValidationEvent("Sch_ComplexContentContentModel", "extension");
            }
            builder.xso = builder.complexContentExtension = new XmlSchemaComplexContentExtension();
            builder.complexContent.Content = builder.complexContentExtension;
        }

        private static void InitComplexContentRestriction(XsdBuilder builder, string value)
        {
            builder.xso = builder.complexContentRestriction = new XmlSchemaComplexContentRestriction();
            builder.complexContent.Content = builder.complexContentRestriction;
        }

        private static void InitComplexType(XsdBuilder builder, string value)
        {
            builder.xso = builder.complexType = new XmlSchemaComplexType();
            switch (builder.ParentElement)
            {
                case SchemaNames.Token.XsdSchema:
                    builder.canIncludeImport = false;
                    builder.schema.Items.Add(builder.complexType);
                    break;

                case SchemaNames.Token.XsdElement:
                    if (builder.element.SchemaType != null)
                    {
                        builder.SendValidationEvent("Sch_DupElement", "complexType");
                    }
                    if (builder.element.Constraints.Count != 0)
                    {
                        builder.SendValidationEvent("Sch_TypeAfterConstraints", null);
                    }
                    builder.element.SchemaType = builder.complexType;
                    break;

                case SchemaNames.Token.XsdRedefine:
                    builder.redefine.Items.Add(builder.complexType);
                    break;
            }
        }

        private static void InitDocumentation(XsdBuilder builder, string value)
        {
            builder.xso = builder.documentation = new XmlSchemaDocumentation();
            builder.annotation.Items.Add(builder.documentation);
            builder.markup = new XmlNode[0];
        }

        private static void InitElement(XsdBuilder builder, string value)
        {
            builder.xso = builder.element = new XmlSchemaElement();
            builder.canIncludeImport = false;
            switch (builder.ParentElement)
            {
                case SchemaNames.Token.XsdAll:
                    builder.all.Items.Add(builder.element);
                    return;

                case SchemaNames.Token.XsdChoice:
                    builder.choice.Items.Add(builder.element);
                    return;

                case SchemaNames.Token.XsdSequence:
                    builder.sequence.Items.Add(builder.element);
                    return;

                case SchemaNames.Token.XsdSchema:
                    builder.schema.Items.Add(builder.element);
                    return;
            }
        }

        private static void InitFacet(XsdBuilder builder, string value)
        {
            switch (builder.CurrentElement)
            {
                case SchemaNames.Token.XsdMinExclusive:
                    builder.facet = new XmlSchemaMinExclusiveFacet();
                    break;

                case SchemaNames.Token.XsdMinInclusive:
                    builder.facet = new XmlSchemaMinInclusiveFacet();
                    break;

                case SchemaNames.Token.XsdMaxExclusive:
                    builder.facet = new XmlSchemaMaxExclusiveFacet();
                    break;

                case SchemaNames.Token.XsdMaxInclusive:
                    builder.facet = new XmlSchemaMaxInclusiveFacet();
                    break;

                case SchemaNames.Token.XsdTotalDigits:
                    builder.facet = new XmlSchemaTotalDigitsFacet();
                    break;

                case SchemaNames.Token.XsdFractionDigits:
                    builder.facet = new XmlSchemaFractionDigitsFacet();
                    break;

                case SchemaNames.Token.XsdLength:
                    builder.facet = new XmlSchemaLengthFacet();
                    break;

                case SchemaNames.Token.XsdMinLength:
                    builder.facet = new XmlSchemaMinLengthFacet();
                    break;

                case SchemaNames.Token.XsdMaxLength:
                    builder.facet = new XmlSchemaMaxLengthFacet();
                    break;

                case SchemaNames.Token.XsdEnumeration:
                    builder.facet = new XmlSchemaEnumerationFacet();
                    break;

                case SchemaNames.Token.XsdPattern:
                    builder.facet = new XmlSchemaPatternFacet();
                    break;

                case SchemaNames.Token.XsdWhitespace:
                    builder.facet = new XmlSchemaWhiteSpaceFacet();
                    break;
            }
            builder.xso = builder.facet;
            if (SchemaNames.Token.XsdSimpleTypeRestriction == builder.ParentElement)
            {
                builder.simpleTypeRestriction.Facets.Add(builder.facet);
            }
            else
            {
                if ((builder.simpleContentRestriction.Attributes.Count != 0) || (builder.simpleContentRestriction.AnyAttribute != null))
                {
                    builder.SendValidationEvent("Sch_InvalidFacetPosition", null);
                }
                builder.simpleContentRestriction.Facets.Add(builder.facet);
            }
        }

        private static void InitField(XsdBuilder builder, string value)
        {
            builder.xso = builder.xpath = new XmlSchemaXPath();
            if (builder.identityConstraint.Selector == null)
            {
                builder.SendValidationEvent("Sch_SelectorBeforeFields", builder.identityConstraint.Name);
            }
            builder.identityConstraint.Fields.Add(builder.xpath);
        }

        private static void InitGroup(XsdBuilder builder, string value)
        {
            builder.xso = builder.group = new XmlSchemaGroup();
            builder.canIncludeImport = false;
            SchemaNames.Token parentElement = builder.ParentElement;
            if (parentElement != SchemaNames.Token.XsdSchema)
            {
                if (parentElement != SchemaNames.Token.XsdRedefine)
                {
                    return;
                }
            }
            else
            {
                builder.schema.Items.Add(builder.group);
                return;
            }
            builder.redefine.Items.Add(builder.group);
        }

        private static void InitGroupRef(XsdBuilder builder, string value)
        {
            builder.xso = builder.particle = builder.groupRef = new XmlSchemaGroupRef();
            builder.AddParticle(builder.groupRef);
        }

        private static void InitIdentityConstraint(XsdBuilder builder, string value)
        {
            if (!builder.element.RefName.IsEmpty)
            {
                builder.SendValidationEvent("Sch_ElementRef", null);
            }
            switch (builder.CurrentElement)
            {
                case SchemaNames.Token.XsdUnique:
                    builder.xso = builder.identityConstraint = new XmlSchemaUnique();
                    break;

                case SchemaNames.Token.XsdKey:
                    builder.xso = builder.identityConstraint = new XmlSchemaKey();
                    break;

                case SchemaNames.Token.XsdKeyref:
                    builder.xso = builder.identityConstraint = new XmlSchemaKeyref();
                    break;
            }
            builder.element.Constraints.Add(builder.identityConstraint);
        }

        private static void InitImport(XsdBuilder builder, string value)
        {
            if (!builder.canIncludeImport)
            {
                builder.SendValidationEvent("Sch_ImportLocation", null);
            }
            builder.xso = builder.import = new XmlSchemaImport();
            builder.schema.Includes.Add(builder.import);
        }

        private static void InitInclude(XsdBuilder builder, string value)
        {
            if (!builder.canIncludeImport)
            {
                builder.SendValidationEvent("Sch_IncludeLocation", null);
            }
            builder.xso = builder.include = new XmlSchemaInclude();
            builder.schema.Includes.Add(builder.include);
        }

        private static void InitNotation(XsdBuilder builder, string value)
        {
            builder.xso = builder.notation = new XmlSchemaNotation();
            builder.canIncludeImport = false;
            builder.schema.Items.Add(builder.notation);
        }

        private static void InitRedefine(XsdBuilder builder, string value)
        {
            if (!builder.canIncludeImport)
            {
                builder.SendValidationEvent("Sch_RedefineLocation", null);
            }
            builder.xso = builder.redefine = new XmlSchemaRedefine();
            builder.schema.Includes.Add(builder.redefine);
        }

        private static void InitSchema(XsdBuilder builder, string value)
        {
            builder.canIncludeImport = true;
            builder.xso = builder.schema;
        }

        private static void InitSelector(XsdBuilder builder, string value)
        {
            builder.xso = builder.xpath = new XmlSchemaXPath();
            if (builder.identityConstraint.Selector == null)
            {
                builder.identityConstraint.Selector = builder.xpath;
            }
            else
            {
                builder.SendValidationEvent("Sch_DupSelector", builder.identityConstraint.Name);
            }
        }

        private static void InitSequence(XsdBuilder builder, string value)
        {
            builder.xso = builder.particle = builder.sequence = new XmlSchemaSequence();
            builder.AddParticle(builder.sequence);
        }

        private static void InitSimpleContent(XsdBuilder builder, string value)
        {
            if (((builder.complexType.ContentModel != null) || (builder.complexType.Particle != null)) || ((builder.complexType.Attributes.Count != 0) || (builder.complexType.AnyAttribute != null)))
            {
                builder.SendValidationEvent("Sch_ComplexTypeContentModel", "simpleContent");
            }
            builder.xso = builder.simpleContent = new XmlSchemaSimpleContent();
            builder.complexType.ContentModel = builder.simpleContent;
        }

        private static void InitSimpleContentExtension(XsdBuilder builder, string value)
        {
            if (builder.simpleContent.Content != null)
            {
                builder.SendValidationEvent("Sch_DupElement", "extension");
            }
            builder.xso = builder.simpleContentExtension = new XmlSchemaSimpleContentExtension();
            builder.simpleContent.Content = builder.simpleContentExtension;
        }

        private static void InitSimpleContentRestriction(XsdBuilder builder, string value)
        {
            if (builder.simpleContent.Content != null)
            {
                builder.SendValidationEvent("Sch_DupElement", "restriction");
            }
            builder.xso = builder.simpleContentRestriction = new XmlSchemaSimpleContentRestriction();
            builder.simpleContent.Content = builder.simpleContentRestriction;
        }

        private static void InitSimpleType(XsdBuilder builder, string value)
        {
            builder.xso = builder.simpleType = new XmlSchemaSimpleType();
            switch (builder.ParentElement)
            {
                case SchemaNames.Token.XsdElement:
                    if (builder.element.SchemaType != null)
                    {
                        builder.SendValidationEvent("Sch_DupXsdElement", "simpleType");
                    }
                    if (builder.element.Constraints.Count != 0)
                    {
                        builder.SendValidationEvent("Sch_TypeAfterConstraints", null);
                    }
                    builder.element.SchemaType = builder.simpleType;
                    return;

                case SchemaNames.Token.XsdAttribute:
                    if (builder.attribute.SchemaType != null)
                    {
                        builder.SendValidationEvent("Sch_DupXsdElement", "simpleType");
                    }
                    builder.attribute.SchemaType = builder.simpleType;
                    return;

                case SchemaNames.Token.XsdSchema:
                    builder.canIncludeImport = false;
                    builder.schema.Items.Add(builder.simpleType);
                    return;

                case SchemaNames.Token.XsdSimpleContentRestriction:
                    if (builder.simpleContentRestriction.BaseType != null)
                    {
                        builder.SendValidationEvent("Sch_DupXsdElement", "simpleType");
                    }
                    if (((builder.simpleContentRestriction.Attributes.Count != 0) || (builder.simpleContentRestriction.AnyAttribute != null)) || (builder.simpleContentRestriction.Facets.Count != 0))
                    {
                        builder.SendValidationEvent("Sch_SimpleTypeRestriction", null);
                    }
                    builder.simpleContentRestriction.BaseType = builder.simpleType;
                    return;

                case SchemaNames.Token.XsdSimpleTypeList:
                    if (builder.simpleTypeList.ItemType != null)
                    {
                        builder.SendValidationEvent("Sch_DupXsdElement", "simpleType");
                    }
                    builder.simpleTypeList.ItemType = builder.simpleType;
                    return;

                case SchemaNames.Token.XsdSimpleTypeRestriction:
                    if (builder.simpleTypeRestriction.BaseType != null)
                    {
                        builder.SendValidationEvent("Sch_DupXsdElement", "simpleType");
                    }
                    builder.simpleTypeRestriction.BaseType = builder.simpleType;
                    return;

                case SchemaNames.Token.XsdSimpleTypeUnion:
                    builder.simpleTypeUnion.BaseTypes.Add(builder.simpleType);
                    break;

                case SchemaNames.Token.XsdWhitespace:
                    break;

                case SchemaNames.Token.XsdRedefine:
                    builder.redefine.Items.Add(builder.simpleType);
                    return;

                default:
                    return;
            }
        }

        private static void InitSimpleTypeList(XsdBuilder builder, string value)
        {
            if (builder.simpleType.Content != null)
            {
                builder.SendValidationEvent("Sch_DupSimpleTypeChild", null);
            }
            builder.xso = builder.simpleTypeList = new XmlSchemaSimpleTypeList();
            builder.simpleType.Content = builder.simpleTypeList;
        }

        private static void InitSimpleTypeRestriction(XsdBuilder builder, string value)
        {
            if (builder.simpleType.Content != null)
            {
                builder.SendValidationEvent("Sch_DupSimpleTypeChild", null);
            }
            builder.xso = builder.simpleTypeRestriction = new XmlSchemaSimpleTypeRestriction();
            builder.simpleType.Content = builder.simpleTypeRestriction;
        }

        private static void InitSimpleTypeUnion(XsdBuilder builder, string value)
        {
            if (builder.simpleType.Content != null)
            {
                builder.SendValidationEvent("Sch_DupSimpleTypeChild", null);
            }
            builder.xso = builder.simpleTypeUnion = new XmlSchemaSimpleTypeUnion();
            builder.simpleType.Content = builder.simpleTypeUnion;
        }

        internal override bool IsContentParsed()
        {
            return this.currentEntry.ParseContent;
        }

        private bool IsSkipableElement(XmlQualifiedName qname)
        {
            if (this.CurrentElement != SchemaNames.Token.XsdDocumentation)
            {
                return (this.CurrentElement == SchemaNames.Token.XsdAppInfo);
            }
            return true;
        }

        private int ParseBlockFinalEnum(string value, string attributeName)
        {
            int num = 0;
            string[] strArray = XmlConvert.SplitString(value);
            for (int i = 0; i < strArray.Length; i++)
            {
                bool flag = false;
                for (int j = 0; j < DerivationMethodStrings.Length; j++)
                {
                    if (strArray[i] == DerivationMethodStrings[j])
                    {
                        if (((num & DerivationMethodValues[j]) != 0) && ((num & DerivationMethodValues[j]) != DerivationMethodValues[j]))
                        {
                            this.SendValidationEvent("Sch_InvalidXsdAttributeValue", attributeName, value, null);
                            return 0;
                        }
                        num |= DerivationMethodValues[j];
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    this.SendValidationEvent("Sch_InvalidXsdAttributeValue", attributeName, value, null);
                    return 0;
                }
                if ((num == 0xff) && (value.Length > 4))
                {
                    this.SendValidationEvent("Sch_InvalidXsdAttributeValue", attributeName, value, null);
                    return 0;
                }
            }
            return num;
        }

        private bool ParseBoolean(string value, string attributeName)
        {
            try
            {
                return XmlConvert.ToBoolean(value);
            }
            catch (Exception)
            {
                this.SendValidationEvent("Sch_InvalidXsdAttributeValue", attributeName, value, null);
                return false;
            }
        }

        private int ParseEnum(string value, string attributeName, string[] values)
        {
            string str = value.Trim();
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == str)
                {
                    return (i + 1);
                }
            }
            this.SendValidationEvent("Sch_InvalidXsdAttributeValue", attributeName, str, null);
            return 0;
        }

        private XmlQualifiedName ParseQName(string value, string attributeName)
        {
            try
            {
                string str;
                value = XmlComplianceUtil.NonCDataNormalize(value);
                return XmlQualifiedName.Parse(value, this.namespaceManager, out str);
            }
            catch (Exception)
            {
                this.SendValidationEvent("Sch_InvalidXsdAttributeValue", attributeName, value, null);
                return XmlQualifiedName.Empty;
            }
        }

        private static string ParseUriReference(string s)
        {
            return s;
        }

        private void Pop()
        {
            this.currentEntry = (XsdEntry) this.stateHistory.Pop();
            this.SetContainer(this.currentEntry.CurrentState, this.containerStack.Pop());
            this.hasChild = true;
        }

        internal override void ProcessAttribute(string prefix, string name, string ns, string value)
        {
            XmlQualifiedName name2 = new XmlQualifiedName(name, ns);
            if (this.currentEntry.Attributes != null)
            {
                for (int i = 0; i < this.currentEntry.Attributes.Length; i++)
                {
                    XsdAttributeEntry entry = this.currentEntry.Attributes[i];
                    if (this.schemaNames.TokenToQName[(int) entry.Attribute].Equals(name2))
                    {
                        try
                        {
                            entry.BuildFunc(this, value);
                        }
                        catch (XmlSchemaException exception)
                        {
                            exception.SetSource(this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition);
                            this.SendValidationEvent("Sch_InvalidXsdAttributeDatatypeValue", new string[] { name, exception.Message }, XmlSeverityType.Error);
                        }
                        return;
                    }
                }
            }
            if ((ns != this.schemaNames.NsXs) && (ns.Length != 0))
            {
                if (ns == this.schemaNames.NsXmlNs)
                {
                    if (this.namespaces == null)
                    {
                        this.namespaces = new Hashtable();
                    }
                    this.namespaces.Add((name == this.schemaNames.QnXmlNs.Name) ? string.Empty : name, value);
                }
                else
                {
                    XmlAttribute attribute = new XmlAttribute(prefix, name, ns, this.schema.Document) {
                        Value = value
                    };
                    this.unhandledAttributes.Add(attribute);
                }
            }
            else
            {
                this.SendValidationEvent("Sch_UnsupportedAttribute", name2.ToString());
            }
        }

        internal override void ProcessCData(string value)
        {
            this.SendValidationEvent("Sch_TextNotAllowed", value);
        }

        internal override bool ProcessElement(string prefix, string name, string ns)
        {
            XmlQualifiedName qname = new XmlQualifiedName(name, ns);
            if (this.GetNextState(qname))
            {
                this.Push();
                this.xso = null;
                this.currentEntry.InitFunc(this, null);
                this.RecordPosition();
                return true;
            }
            if (!this.IsSkipableElement(qname))
            {
                this.SendValidationEvent("Sch_UnsupportedElement", qname.ToString());
            }
            return false;
        }

        internal override void ProcessMarkup(XmlNode[] markup)
        {
            this.markup = markup;
        }

        private void Push()
        {
            this.stateHistory.Push();
            this.stateHistory[this.stateHistory.Length - 1] = this.currentEntry;
            this.containerStack.Push(this.GetContainer(this.currentEntry.CurrentState));
            this.currentEntry = this.nextEntry;
            if (this.currentEntry.Name != SchemaNames.Token.XsdAnnotation)
            {
                this.hasChild = false;
            }
        }

        private void RecordPosition()
        {
            this.xso.SourceUri = this.reader.BaseURI;
            this.xso.LineNumber = this.positionInfo.LineNumber;
            this.xso.LinePosition = this.positionInfo.LinePosition;
            if (this.xso != this.schema)
            {
                this.xso.Parent = this.ParentContainer;
            }
        }

        private void SendValidationEvent(XmlSchemaException e)
        {
            this.SendValidationEvent(e, XmlSeverityType.Error);
        }

        private void SendValidationEvent(string code, string msg)
        {
            this.SendValidationEvent(new XmlSchemaException(code, msg, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        private void SendValidationEvent(XmlSchemaException e, XmlSeverityType severity)
        {
            this.schema.ErrorCount++;
            e.SetSchemaObject(this.schema);
            if (this.validationEventHandler != null)
            {
                this.validationEventHandler(null, new ValidationEventArgs(e, severity));
            }
            else if (severity == XmlSeverityType.Error)
            {
                throw e;
            }
        }

        private void SendValidationEvent(string code, string[] args, XmlSeverityType severity)
        {
            this.SendValidationEvent(new XmlSchemaException(code, args, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition), severity);
        }

        private void SendValidationEvent(string code, string arg0, string arg1, string arg2)
        {
            this.SendValidationEvent(new XmlSchemaException(code, new string[] { arg0, arg1, arg2 }, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        private void SetContainer(State state, object container)
        {
            switch (state)
            {
                case State.Root:
                case State.Schema:
                    break;

                case State.Annotation:
                    this.annotation = (XmlSchemaAnnotation) container;
                    return;

                case State.Include:
                    this.include = (XmlSchemaInclude) container;
                    return;

                case State.Import:
                    this.import = (XmlSchemaImport) container;
                    return;

                case State.Element:
                    this.element = (XmlSchemaElement) container;
                    return;

                case State.Attribute:
                    this.attribute = (XmlSchemaAttribute) container;
                    return;

                case State.AttributeGroup:
                    this.attributeGroup = (XmlSchemaAttributeGroup) container;
                    return;

                case State.AttributeGroupRef:
                    this.attributeGroupRef = (XmlSchemaAttributeGroupRef) container;
                    return;

                case State.AnyAttribute:
                    this.anyAttribute = (XmlSchemaAnyAttribute) container;
                    return;

                case State.Group:
                    this.group = (XmlSchemaGroup) container;
                    return;

                case State.GroupRef:
                    this.groupRef = (XmlSchemaGroupRef) container;
                    return;

                case State.All:
                    this.all = (XmlSchemaAll) container;
                    return;

                case State.Choice:
                    this.choice = (XmlSchemaChoice) container;
                    return;

                case State.Sequence:
                    this.sequence = (XmlSchemaSequence) container;
                    return;

                case State.Any:
                    this.anyElement = (XmlSchemaAny) container;
                    return;

                case State.Notation:
                    this.notation = (XmlSchemaNotation) container;
                    return;

                case State.SimpleType:
                    this.simpleType = (XmlSchemaSimpleType) container;
                    return;

                case State.ComplexType:
                    this.complexType = (XmlSchemaComplexType) container;
                    return;

                case State.ComplexContent:
                    this.complexContent = (XmlSchemaComplexContent) container;
                    return;

                case State.ComplexContentRestriction:
                    this.complexContentRestriction = (XmlSchemaComplexContentRestriction) container;
                    return;

                case State.ComplexContentExtension:
                    this.complexContentExtension = (XmlSchemaComplexContentExtension) container;
                    return;

                case State.SimpleContent:
                    this.simpleContent = (XmlSchemaSimpleContent) container;
                    return;

                case State.SimpleContentExtension:
                    this.simpleContentExtension = (XmlSchemaSimpleContentExtension) container;
                    return;

                case State.SimpleContentRestriction:
                    this.simpleContentRestriction = (XmlSchemaSimpleContentRestriction) container;
                    return;

                case State.SimpleTypeUnion:
                    this.simpleTypeUnion = (XmlSchemaSimpleTypeUnion) container;
                    return;

                case State.SimpleTypeList:
                    this.simpleTypeList = (XmlSchemaSimpleTypeList) container;
                    return;

                case State.SimpleTypeRestriction:
                    this.simpleTypeRestriction = (XmlSchemaSimpleTypeRestriction) container;
                    return;

                case State.Unique:
                case State.Key:
                case State.KeyRef:
                    this.identityConstraint = (XmlSchemaIdentityConstraint) container;
                    return;

                case State.Selector:
                case State.Field:
                    this.xpath = (XmlSchemaXPath) container;
                    return;

                case State.MinExclusive:
                case State.MinInclusive:
                case State.MaxExclusive:
                case State.MaxInclusive:
                case State.TotalDigits:
                case State.FractionDigits:
                case State.Length:
                case State.MinLength:
                case State.MaxLength:
                case State.Enumeration:
                case State.Pattern:
                case State.WhiteSpace:
                    this.facet = (XmlSchemaFacet) container;
                    return;

                case State.AppInfo:
                    this.appInfo = (XmlSchemaAppInfo) container;
                    return;

                case State.Documentation:
                    this.documentation = (XmlSchemaDocumentation) container;
                    return;

                case State.Redefine:
                    this.redefine = (XmlSchemaRedefine) container;
                    break;

                default:
                    return;
            }
        }

        private void SetMaxOccurs(XmlSchemaParticle particle, string value)
        {
            try
            {
                particle.MaxOccursString = value;
            }
            catch (Exception)
            {
                this.SendValidationEvent("Sch_MaxOccursInvalidXsd", null);
            }
        }

        private void SetMinOccurs(XmlSchemaParticle particle, string value)
        {
            try
            {
                particle.MinOccursString = value;
            }
            catch (Exception)
            {
                this.SendValidationEvent("Sch_MinOccursInvalidXsd", null);
            }
        }

        internal override void StartChildren()
        {
            if (this.xso != null)
            {
                if ((this.namespaces != null) && (this.namespaces.Count > 0))
                {
                    this.xso.Namespaces.Namespaces = this.namespaces;
                    this.namespaces = null;
                }
                if (this.unhandledAttributes.Count != 0)
                {
                    this.xso.SetUnhandledAttributes((XmlAttribute[]) this.unhandledAttributes.ToArray(typeof(XmlAttribute)));
                    this.unhandledAttributes.Clear();
                }
            }
        }

        private SchemaNames.Token CurrentElement
        {
            get
            {
                return this.currentEntry.Name;
            }
        }

        private XmlSchemaObject ParentContainer
        {
            get
            {
                return (XmlSchemaObject) this.containerStack.Peek();
            }
        }

        private SchemaNames.Token ParentElement
        {
            get
            {
                return ((XsdEntry) this.stateHistory[this.stateHistory.Length - 1]).Name;
            }
        }

        private class BuilderNamespaceManager : XmlNamespaceManager
        {
            private XmlNamespaceManager nsMgr;
            private XmlReader reader;

            public BuilderNamespaceManager(XmlNamespaceManager nsMgr, XmlReader reader)
            {
                this.nsMgr = nsMgr;
                this.reader = reader;
            }

            public override string LookupNamespace(string prefix)
            {
                string str = this.nsMgr.LookupNamespace(prefix);
                if (str == null)
                {
                    str = this.reader.LookupNamespace(prefix);
                }
                return str;
            }
        }

        private enum State
        {
            Root,
            Schema,
            Annotation,
            Include,
            Import,
            Element,
            Attribute,
            AttributeGroup,
            AttributeGroupRef,
            AnyAttribute,
            Group,
            GroupRef,
            All,
            Choice,
            Sequence,
            Any,
            Notation,
            SimpleType,
            ComplexType,
            ComplexContent,
            ComplexContentRestriction,
            ComplexContentExtension,
            SimpleContent,
            SimpleContentExtension,
            SimpleContentRestriction,
            SimpleTypeUnion,
            SimpleTypeList,
            SimpleTypeRestriction,
            Unique,
            Key,
            KeyRef,
            Selector,
            Field,
            MinExclusive,
            MinInclusive,
            MaxExclusive,
            MaxInclusive,
            TotalDigits,
            FractionDigits,
            Length,
            MinLength,
            MaxLength,
            Enumeration,
            Pattern,
            WhiteSpace,
            AppInfo,
            Documentation,
            Redefine
        }

        private sealed class XsdAttributeEntry
        {
            public SchemaNames.Token Attribute;
            public XsdBuilder.XsdBuildFunction BuildFunc;

            public XsdAttributeEntry(SchemaNames.Token a, XsdBuilder.XsdBuildFunction build)
            {
                this.Attribute = a;
                this.BuildFunc = build;
            }
        }

        private delegate void XsdBuildFunction(XsdBuilder builder, string value);

        private delegate void XsdEndChildFunction(XsdBuilder builder);

        private sealed class XsdEntry
        {
            public XsdBuilder.XsdAttributeEntry[] Attributes;
            public XsdBuilder.State CurrentState;
            public XsdBuilder.XsdEndChildFunction EndChildFunc;
            public XsdBuilder.XsdInitFunction InitFunc;
            public SchemaNames.Token Name;
            public XsdBuilder.State[] NextStates;
            public bool ParseContent;

            public XsdEntry(SchemaNames.Token n, XsdBuilder.State state, XsdBuilder.State[] nextStates, XsdBuilder.XsdAttributeEntry[] attributes, XsdBuilder.XsdInitFunction init, XsdBuilder.XsdEndChildFunction end, bool parseContent)
            {
                this.Name = n;
                this.CurrentState = state;
                this.NextStates = nextStates;
                this.Attributes = attributes;
                this.InitFunc = init;
                this.EndChildFunc = end;
                this.ParseContent = parseContent;
            }
        }

        private delegate void XsdInitFunction(XsdBuilder builder, string value);
    }
}

