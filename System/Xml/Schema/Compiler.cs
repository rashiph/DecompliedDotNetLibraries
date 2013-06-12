namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class Compiler : BaseProcessor
    {
        private XmlSchemaObjectTable attributeGroups;
        private XmlSchemaObjectTable attributes;
        private Stack complexTypeStack;
        private XmlSchemaObjectTable elements;
        private XmlSchemaObjectTable examplars;
        private XmlSchemaObjectTable groups;
        private XmlSchemaObjectTable identityConstraints;
        private Hashtable importedSchemas;
        private XmlSchemaObjectTable notations;
        private string restrictionErrorMsg;
        private XmlSchema schemaForSchema;
        private Hashtable schemasToCompile;
        private XmlSchemaObjectTable schemaTypes;

        public Compiler(XmlNameTable nameTable, ValidationEventHandler eventHandler, XmlSchema schemaForSchema, XmlSchemaCompilationSettings compilationSettings) : base(nameTable, null, eventHandler, compilationSettings)
        {
            this.attributes = new XmlSchemaObjectTable();
            this.attributeGroups = new XmlSchemaObjectTable();
            this.elements = new XmlSchemaObjectTable();
            this.schemaTypes = new XmlSchemaObjectTable();
            this.groups = new XmlSchemaObjectTable();
            this.notations = new XmlSchemaObjectTable();
            this.examplars = new XmlSchemaObjectTable();
            this.identityConstraints = new XmlSchemaObjectTable();
            this.complexTypeStack = new Stack();
            this.schemasToCompile = new Hashtable();
            this.importedSchemas = new Hashtable();
            this.schemaForSchema = schemaForSchema;
        }

        private bool BuildParticleContentModel(ParticleContentValidator contentValidator, XmlSchemaParticle particle)
        {
            bool flag = false;
            if (particle is XmlSchemaElement)
            {
                XmlSchemaElement element = (XmlSchemaElement) particle;
                contentValidator.AddName(element.QualifiedName, element);
            }
            else if (particle is XmlSchemaAny)
            {
                flag = true;
                XmlSchemaAny any = (XmlSchemaAny) particle;
                contentValidator.AddNamespaceList(any.NamespaceList, any);
            }
            else if (particle is XmlSchemaGroupBase)
            {
                XmlSchemaObjectCollection items = ((XmlSchemaGroupBase) particle).Items;
                bool flag2 = particle is XmlSchemaChoice;
                contentValidator.OpenGroup();
                bool flag3 = true;
                for (int i = 0; i < items.Count; i++)
                {
                    if (flag3)
                    {
                        flag3 = false;
                    }
                    else if (flag2)
                    {
                        contentValidator.AddChoice();
                    }
                    else
                    {
                        contentValidator.AddSequence();
                    }
                    flag = this.BuildParticleContentModel(contentValidator, (XmlSchemaParticle) items[i]);
                }
                contentValidator.CloseGroup();
            }
            if ((particle.MinOccurs != 1M) || (particle.MaxOccurs != 1M))
            {
                if ((particle.MinOccurs == 0M) && (particle.MaxOccurs == 1M))
                {
                    contentValidator.AddQMark();
                    return flag;
                }
                if ((particle.MinOccurs == 0M) && (particle.MaxOccurs == 79228162514264337593543950335M))
                {
                    contentValidator.AddStar();
                    return flag;
                }
                if ((particle.MinOccurs == 1M) && (particle.MaxOccurs == 79228162514264337593543950335M))
                {
                    contentValidator.AddPlus();
                    return flag;
                }
                contentValidator.AddLeafRange(particle.MinOccurs, particle.MaxOccurs);
            }
            return flag;
        }

        private void CalculateEffectiveTotalRange(XmlSchemaParticle particle, out decimal minOccurs, out decimal maxOccurs)
        {
            XmlSchemaChoice choice = particle as XmlSchemaChoice;
            if ((particle is XmlSchemaElement) || (particle is XmlSchemaAny))
            {
                minOccurs = particle.MinOccurs;
                maxOccurs = particle.MaxOccurs;
            }
            else if (choice != null)
            {
                if (choice.Items.Count == 0)
                {
                    minOccurs = maxOccurs = 0M;
                }
                else
                {
                    minOccurs = 79228162514264337593543950335M;
                    maxOccurs = 0M;
                    for (int i = 0; i < choice.Items.Count; i++)
                    {
                        decimal num2;
                        decimal num3;
                        this.CalculateEffectiveTotalRange((XmlSchemaParticle) choice.Items[i], out num2, out num3);
                        if (num2 < minOccurs)
                        {
                            minOccurs = num2;
                        }
                        if (num3 > maxOccurs)
                        {
                            maxOccurs = num3;
                        }
                    }
                    minOccurs *= particle.MinOccurs;
                    if (maxOccurs != 79228162514264337593543950335M)
                    {
                        if (particle.MaxOccurs == 79228162514264337593543950335M)
                        {
                            maxOccurs = 79228162514264337593543950335M;
                        }
                        else
                        {
                            maxOccurs *= particle.MaxOccurs;
                        }
                    }
                }
            }
            else
            {
                XmlSchemaObjectCollection items = ((XmlSchemaGroupBase) particle).Items;
                if (items.Count == 0)
                {
                    minOccurs = maxOccurs = 0M;
                }
                else
                {
                    minOccurs = 0M;
                    maxOccurs = 0M;
                    for (int j = 0; j < items.Count; j++)
                    {
                        decimal num5;
                        decimal num6;
                        this.CalculateEffectiveTotalRange((XmlSchemaParticle) items[j], out num5, out num6);
                        minOccurs += num5;
                        if (maxOccurs != 79228162514264337593543950335M)
                        {
                            if (num6 == 79228162514264337593543950335M)
                            {
                                maxOccurs = 79228162514264337593543950335M;
                            }
                            else
                            {
                                maxOccurs += num6;
                            }
                        }
                    }
                    minOccurs *= particle.MinOccurs;
                    if (maxOccurs != 79228162514264337593543950335M)
                    {
                        if (particle.MaxOccurs == 79228162514264337593543950335M)
                        {
                            maxOccurs = 79228162514264337593543950335M;
                        }
                        else
                        {
                            maxOccurs *= particle.MaxOccurs;
                        }
                    }
                }
            }
        }

        private XmlSchemaParticle CannonicalizeAll(XmlSchemaAll all, bool root)
        {
            if (all.Items.Count > 0)
            {
                XmlSchemaAll to = new XmlSchemaAll {
                    MinOccurs = all.MinOccurs,
                    MaxOccurs = all.MaxOccurs
                };
                this.CopyPosition(to, all, true);
                for (int i = 0; i < all.Items.Count; i++)
                {
                    XmlSchemaParticle item = this.CannonicalizeParticle((XmlSchemaElement) all.Items[i], false);
                    if (item != XmlSchemaParticle.Empty)
                    {
                        to.Items.Add(item);
                    }
                }
                all = to;
            }
            if (all.Items.Count == 0)
            {
                return XmlSchemaParticle.Empty;
            }
            if (!root)
            {
                base.SendValidationEvent("Sch_NotAllAlone", all);
                return XmlSchemaParticle.Empty;
            }
            return all;
        }

        private XmlSchemaParticle CannonicalizeChoice(XmlSchemaChoice choice, bool root)
        {
            XmlSchemaChoice source = choice;
            if (choice.Items.Count > 0)
            {
                XmlSchemaChoice to = new XmlSchemaChoice {
                    MinOccurs = choice.MinOccurs,
                    MaxOccurs = choice.MaxOccurs
                };
                this.CopyPosition(to, choice, true);
                for (int i = 0; i < choice.Items.Count; i++)
                {
                    XmlSchemaParticle item = this.CannonicalizeParticle((XmlSchemaParticle) choice.Items[i], false);
                    if (item != XmlSchemaParticle.Empty)
                    {
                        if (((item.MinOccurs == 1M) && (item.MaxOccurs == 1M)) && (item is XmlSchemaChoice))
                        {
                            XmlSchemaChoice choice4 = item as XmlSchemaChoice;
                            for (int j = 0; j < choice4.Items.Count; j++)
                            {
                                to.Items.Add(choice4.Items[j]);
                            }
                        }
                        else
                        {
                            to.Items.Add(item);
                        }
                    }
                }
                choice = to;
            }
            if (!root && (choice.Items.Count == 0))
            {
                if (choice.MinOccurs != 0M)
                {
                    base.SendValidationEvent("Sch_EmptyChoice", source, XmlSeverityType.Warning);
                }
                return XmlSchemaParticle.Empty;
            }
            if ((!root && (choice.Items.Count == 1)) && ((choice.MinOccurs == 1M) && (choice.MaxOccurs == 1M)))
            {
                return (XmlSchemaParticle) choice.Items[0];
            }
            return choice;
        }

        private XmlSchemaParticle CannonicalizeElement(XmlSchemaElement element)
        {
            if (element.RefName.IsEmpty || ((element.ElementDecl.Block & XmlSchemaDerivationMethod.Substitution) != XmlSchemaDerivationMethod.Empty))
            {
                return element;
            }
            XmlSchemaSubstitutionGroup group = (XmlSchemaSubstitutionGroup) this.examplars[element.QualifiedName];
            if (group == null)
            {
                return element;
            }
            XmlSchemaChoice to = new XmlSchemaChoice();
            for (int i = 0; i < group.Members.Count; i++)
            {
                to.Items.Add((XmlSchemaElement) group.Members[i]);
            }
            to.MinOccurs = element.MinOccurs;
            to.MaxOccurs = element.MaxOccurs;
            this.CopyPosition(to, element, false);
            return to;
        }

        private XmlSchemaParticle CannonicalizeGroupRef(XmlSchemaGroupRef groupRef, bool root)
        {
            XmlSchemaGroup redefined;
            if (groupRef.Redefined != null)
            {
                redefined = groupRef.Redefined;
            }
            else
            {
                redefined = (XmlSchemaGroup) this.groups[groupRef.RefName];
            }
            if (redefined == null)
            {
                base.SendValidationEvent("Sch_UndefGroupRef", groupRef.RefName.ToString(), groupRef);
                return XmlSchemaParticle.Empty;
            }
            if (redefined.CanonicalParticle == null)
            {
                this.CompileGroup(redefined);
            }
            if (redefined.CanonicalParticle == XmlSchemaParticle.Empty)
            {
                return XmlSchemaParticle.Empty;
            }
            XmlSchemaGroupBase canonicalParticle = (XmlSchemaGroupBase) redefined.CanonicalParticle;
            if (canonicalParticle is XmlSchemaAll)
            {
                if (!root)
                {
                    base.SendValidationEvent("Sch_AllRefNotRoot", "", groupRef);
                    return XmlSchemaParticle.Empty;
                }
                if ((groupRef.MinOccurs > 1M) || (groupRef.MaxOccurs != 1M))
                {
                    base.SendValidationEvent("Sch_AllRefMinMax", groupRef);
                    return XmlSchemaParticle.Empty;
                }
            }
            else if ((canonicalParticle is XmlSchemaChoice) && (canonicalParticle.Items.Count == 0))
            {
                if (groupRef.MinOccurs != 0M)
                {
                    base.SendValidationEvent("Sch_EmptyChoice", groupRef, XmlSeverityType.Warning);
                }
                return XmlSchemaParticle.Empty;
            }
            XmlSchemaGroupBase to = (canonicalParticle is XmlSchemaSequence) ? ((XmlSchemaGroupBase) new XmlSchemaSequence()) : ((canonicalParticle is XmlSchemaChoice) ? ((XmlSchemaGroupBase) new XmlSchemaChoice()) : ((XmlSchemaGroupBase) new XmlSchemaAll()));
            to.MinOccurs = groupRef.MinOccurs;
            to.MaxOccurs = groupRef.MaxOccurs;
            this.CopyPosition(to, groupRef, true);
            for (int i = 0; i < canonicalParticle.Items.Count; i++)
            {
                to.Items.Add(canonicalParticle.Items[i]);
            }
            groupRef.SetParticle(to);
            return to;
        }

        private XmlSchemaParticle CannonicalizeParticle(XmlSchemaParticle particle, bool root)
        {
            if ((particle == null) || particle.IsEmpty)
            {
                return XmlSchemaParticle.Empty;
            }
            if (!(particle is XmlSchemaElement))
            {
                if (particle is XmlSchemaGroupRef)
                {
                    return this.CannonicalizeGroupRef((XmlSchemaGroupRef) particle, root);
                }
                if (particle is XmlSchemaAll)
                {
                    return this.CannonicalizeAll((XmlSchemaAll) particle, root);
                }
                if (particle is XmlSchemaChoice)
                {
                    return this.CannonicalizeChoice((XmlSchemaChoice) particle, root);
                }
                if (particle is XmlSchemaSequence)
                {
                    return this.CannonicalizeSequence((XmlSchemaSequence) particle, root);
                }
            }
            return particle;
        }

        private XmlSchemaParticle CannonicalizePointlessRoot(XmlSchemaParticle particle)
        {
            if (particle == null)
            {
                return null;
            }
            decimal num = 1M;
            XmlSchemaSequence sequence = particle as XmlSchemaSequence;
            if (sequence != null)
            {
                XmlSchemaObjectCollection items = sequence.Items;
                if (((items.Count == 1) && (sequence.MinOccurs == num)) && (sequence.MaxOccurs == num))
                {
                    return (XmlSchemaParticle) items[0];
                }
                return particle;
            }
            XmlSchemaChoice choice = particle as XmlSchemaChoice;
            if (choice != null)
            {
                XmlSchemaObjectCollection objects2 = choice.Items;
                int count = objects2.Count;
                if (count == 1)
                {
                    if ((choice.MinOccurs == num) && (choice.MaxOccurs == num))
                    {
                        return (XmlSchemaParticle) objects2[0];
                    }
                    return particle;
                }
                if (count != 0)
                {
                    return particle;
                }
                return XmlSchemaParticle.Empty;
            }
            XmlSchemaAll all = particle as XmlSchemaAll;
            if (all != null)
            {
                XmlSchemaObjectCollection objects3 = all.Items;
                if (((objects3.Count == 1) && (all.MinOccurs == num)) && (all.MaxOccurs == num))
                {
                    return (XmlSchemaParticle) objects3[0];
                }
            }
            return particle;
        }

        private XmlSchemaParticle CannonicalizeSequence(XmlSchemaSequence sequence, bool root)
        {
            if (sequence.Items.Count > 0)
            {
                XmlSchemaSequence to = new XmlSchemaSequence {
                    MinOccurs = sequence.MinOccurs,
                    MaxOccurs = sequence.MaxOccurs
                };
                this.CopyPosition(to, sequence, true);
                for (int i = 0; i < sequence.Items.Count; i++)
                {
                    XmlSchemaParticle item = this.CannonicalizeParticle((XmlSchemaParticle) sequence.Items[i], false);
                    if (item != XmlSchemaParticle.Empty)
                    {
                        XmlSchemaSequence sequence3 = item as XmlSchemaSequence;
                        if (((item.MinOccurs == 1M) && (item.MaxOccurs == 1M)) && (sequence3 != null))
                        {
                            for (int j = 0; j < sequence3.Items.Count; j++)
                            {
                                to.Items.Add(sequence3.Items[j]);
                            }
                        }
                        else
                        {
                            to.Items.Add(item);
                        }
                    }
                }
                sequence = to;
            }
            if (sequence.Items.Count == 0)
            {
                return XmlSchemaParticle.Empty;
            }
            if ((!root && (sequence.Items.Count == 1)) && ((sequence.MinOccurs == 1M) && (sequence.MaxOccurs == 1M)))
            {
                return (XmlSchemaParticle) sequence.Items[0];
            }
            return sequence;
        }

        private void CheckAtrributeGroupRestriction(XmlSchemaAttributeGroup baseAttributeGroup, XmlSchemaAttributeGroup derivedAttributeGroup)
        {
            XmlSchemaAnyAttribute attributeWildcard = baseAttributeGroup.AttributeWildcard;
            XmlSchemaAnyAttribute sub = derivedAttributeGroup.AttributeWildcard;
            if ((sub != null) && (((attributeWildcard == null) || !XmlSchemaAnyAttribute.IsSubset(sub, attributeWildcard)) || !this.IsProcessContentsRestricted(null, sub, attributeWildcard)))
            {
                base.SendValidationEvent("Sch_InvalidAnyAttributeRestriction", derivedAttributeGroup);
            }
            foreach (XmlSchemaAttribute attribute3 in baseAttributeGroup.AttributeUses.Values)
            {
                XmlSchemaAttribute source = (XmlSchemaAttribute) derivedAttributeGroup.AttributeUses[attribute3.QualifiedName];
                if (source != null)
                {
                    if ((attribute3.Use == XmlSchemaUse.Prohibited) && (source.Use != XmlSchemaUse.Prohibited))
                    {
                        base.SendValidationEvent("Sch_AttributeRestrictionProhibited", source);
                    }
                    else if ((attribute3.Use == XmlSchemaUse.Required) && (source.Use != XmlSchemaUse.Required))
                    {
                        base.SendValidationEvent("Sch_AttributeUseInvalid", source);
                    }
                    else if (source.Use != XmlSchemaUse.Prohibited)
                    {
                        if (((attribute3.AttributeSchemaType == null) || (source.AttributeSchemaType == null)) || !XmlSchemaType.IsDerivedFrom(source.AttributeSchemaType, attribute3.AttributeSchemaType, XmlSchemaDerivationMethod.Empty))
                        {
                            base.SendValidationEvent("Sch_AttributeRestrictionInvalid", source);
                        }
                        else if (!this.IsFixedEqual(attribute3.AttDef, source.AttDef))
                        {
                            base.SendValidationEvent("Sch_AttributeFixedInvalid", source);
                        }
                    }
                }
                else if (attribute3.Use == XmlSchemaUse.Required)
                {
                    base.SendValidationEvent("Sch_NoDerivedAttribute", attribute3.QualifiedName.ToString(), baseAttributeGroup.QualifiedName.ToString(), derivedAttributeGroup);
                }
            }
            foreach (XmlSchemaAttribute attribute5 in derivedAttributeGroup.AttributeUses.Values)
            {
                if ((((XmlSchemaAttribute) baseAttributeGroup.AttributeUses[attribute5.QualifiedName]) == null) && ((attributeWildcard == null) || !attributeWildcard.Allows(attribute5.QualifiedName)))
                {
                    base.SendValidationEvent("Sch_AttributeRestrictionInvalidFromWildcard", attribute5);
                }
            }
        }

        private void CheckParticleDerivation(XmlSchemaComplexType complexType)
        {
            XmlSchemaComplexType baseXmlSchemaType = complexType.BaseXmlSchemaType as XmlSchemaComplexType;
            this.restrictionErrorMsg = null;
            if (((baseXmlSchemaType != null) && (baseXmlSchemaType != XmlSchemaComplexType.AnyType)) && (complexType.DerivedBy == XmlSchemaDerivationMethod.Restriction))
            {
                XmlSchemaParticle derivedParticle = this.CannonicalizePointlessRoot(complexType.ContentTypeParticle);
                XmlSchemaParticle baseParticle = this.CannonicalizePointlessRoot(baseXmlSchemaType.ContentTypeParticle);
                if (!this.IsValidRestriction(derivedParticle, baseParticle))
                {
                    if (this.restrictionErrorMsg != null)
                    {
                        base.SendValidationEvent("Sch_InvalidParticleRestrictionDetailed", this.restrictionErrorMsg, complexType);
                    }
                    else
                    {
                        base.SendValidationEvent("Sch_InvalidParticleRestriction", complexType);
                    }
                }
            }
            else if (baseXmlSchemaType == XmlSchemaComplexType.AnyType)
            {
                foreach (XmlSchemaElement element in complexType.LocalElements.Values)
                {
                    if (!element.IsLocalTypeDerivationChecked)
                    {
                        XmlSchemaComplexType elementSchemaType = element.ElementSchemaType as XmlSchemaComplexType;
                        if (((elementSchemaType != null) && (element.SchemaTypeName == XmlQualifiedName.Empty)) && (element.RefName == XmlQualifiedName.Empty))
                        {
                            element.IsLocalTypeDerivationChecked = true;
                            this.CheckParticleDerivation(elementSchemaType);
                        }
                    }
                }
            }
        }

        private void CheckParticleDerivation(XmlSchemaParticle derivedParticle, XmlSchemaParticle baseParticle)
        {
            this.restrictionErrorMsg = null;
            derivedParticle = this.CannonicalizePointlessRoot(derivedParticle);
            baseParticle = this.CannonicalizePointlessRoot(baseParticle);
            if (!this.IsValidRestriction(derivedParticle, baseParticle))
            {
                if (this.restrictionErrorMsg != null)
                {
                    base.SendValidationEvent("Sch_InvalidParticleRestrictionDetailed", this.restrictionErrorMsg, derivedParticle);
                }
                else
                {
                    base.SendValidationEvent("Sch_InvalidParticleRestriction", derivedParticle);
                }
            }
        }

        private void CheckUnionType(XmlSchemaSimpleType unionMember, ArrayList memberTypeDefinitions, XmlSchemaSimpleType parentType)
        {
            XmlSchemaDatatype datatype = unionMember.Datatype;
            if ((unionMember.DerivedBy == XmlSchemaDerivationMethod.Restriction) && (datatype.HasLexicalFacets || datatype.HasValueFacets))
            {
                base.SendValidationEvent("Sch_UnionFromUnion", parentType);
            }
            else
            {
                Datatype_union _union = unionMember.Datatype as Datatype_union;
                memberTypeDefinitions.AddRange(_union.BaseMemberTypes);
            }
        }

        private void CleanupAttribute(XmlSchemaAttribute attribute)
        {
            if (attribute.SchemaType != null)
            {
                this.CleanupSimpleType(attribute.SchemaType);
            }
            attribute.AttDef = null;
        }

        private void CleanupAttributeGroup(XmlSchemaAttributeGroup attributeGroup)
        {
            this.CleanupAttributes(attributeGroup.Attributes);
            attributeGroup.AttributeUses.Clear();
            attributeGroup.AttributeWildcard = null;
            if (attributeGroup.Redefined != null)
            {
                this.CleanupAttributeGroup(attributeGroup.Redefined);
            }
        }

        private void CleanupAttributes(XmlSchemaObjectCollection attributes)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                XmlSchemaAttribute attribute = attributes[i] as XmlSchemaAttribute;
                if (attribute != null)
                {
                    this.CleanupAttribute(attribute);
                }
            }
        }

        private void CleanupComplexType(XmlSchemaComplexType complexType)
        {
            if (complexType.QualifiedName != DatatypeImplementation.QnAnyType)
            {
                if (complexType.ContentModel != null)
                {
                    if (complexType.ContentModel is XmlSchemaSimpleContent)
                    {
                        XmlSchemaSimpleContent contentModel = (XmlSchemaSimpleContent) complexType.ContentModel;
                        if (contentModel.Content is XmlSchemaSimpleContentExtension)
                        {
                            XmlSchemaSimpleContentExtension content = (XmlSchemaSimpleContentExtension) contentModel.Content;
                            this.CleanupAttributes(content.Attributes);
                        }
                        else
                        {
                            XmlSchemaSimpleContentRestriction restriction = (XmlSchemaSimpleContentRestriction) contentModel.Content;
                            this.CleanupAttributes(restriction.Attributes);
                        }
                    }
                    else
                    {
                        XmlSchemaComplexContent content2 = (XmlSchemaComplexContent) complexType.ContentModel;
                        if (content2.Content is XmlSchemaComplexContentExtension)
                        {
                            XmlSchemaComplexContentExtension extension2 = (XmlSchemaComplexContentExtension) content2.Content;
                            this.CleanupParticle(extension2.Particle);
                            this.CleanupAttributes(extension2.Attributes);
                        }
                        else
                        {
                            XmlSchemaComplexContentRestriction restriction2 = (XmlSchemaComplexContentRestriction) content2.Content;
                            this.CleanupParticle(restriction2.Particle);
                            this.CleanupAttributes(restriction2.Attributes);
                        }
                    }
                }
                else
                {
                    this.CleanupParticle(complexType.Particle);
                    this.CleanupAttributes(complexType.Attributes);
                }
                complexType.LocalElements.Clear();
                complexType.AttributeUses.Clear();
                complexType.SetAttributeWildcard(null);
                complexType.SetContentTypeParticle(XmlSchemaParticle.Empty);
                complexType.ElementDecl = null;
                complexType.HasWildCard = false;
                if (complexType.Redefined != null)
                {
                    this.CleanupComplexType(complexType.Redefined as XmlSchemaComplexType);
                }
            }
        }

        private void CleanupElement(XmlSchemaElement element)
        {
            if (element.SchemaType != null)
            {
                XmlSchemaComplexType schemaType = element.SchemaType as XmlSchemaComplexType;
                if (schemaType != null)
                {
                    this.CleanupComplexType(schemaType);
                }
                else
                {
                    this.CleanupSimpleType((XmlSchemaSimpleType) element.SchemaType);
                }
            }
            for (int i = 0; i < element.Constraints.Count; i++)
            {
                ((XmlSchemaIdentityConstraint) element.Constraints[i]).CompiledConstraint = null;
            }
            element.ElementDecl = null;
            element.IsLocalTypeDerivationChecked = false;
        }

        private void CleanupGroup(XmlSchemaGroup group)
        {
            this.CleanupParticle(group.Particle);
            group.CanonicalParticle = null;
            if (group.Redefined != null)
            {
                this.CleanupGroup(group.Redefined);
            }
        }

        private void CleanupParticle(XmlSchemaParticle particle)
        {
            XmlSchemaElement element = particle as XmlSchemaElement;
            if (element != null)
            {
                this.CleanupElement(element);
            }
            else
            {
                XmlSchemaGroupBase base2 = particle as XmlSchemaGroupBase;
                if (base2 != null)
                {
                    for (int i = 0; i < base2.Items.Count; i++)
                    {
                        this.CleanupParticle((XmlSchemaParticle) base2.Items[i]);
                    }
                }
            }
        }

        private void CleanupSimpleType(XmlSchemaSimpleType simpleType)
        {
            if (simpleType != XmlSchemaType.GetBuiltInSimpleType(simpleType.TypeCode))
            {
                simpleType.ElementDecl = null;
                if (simpleType.Redefined != null)
                {
                    this.CleanupSimpleType(simpleType.Redefined as XmlSchemaSimpleType);
                }
            }
        }

        internal bool Compile()
        {
            this.schemaTypes.Insert(DatatypeImplementation.QnAnyType, XmlSchemaComplexType.AnyType);
            if (this.schemaForSchema != null)
            {
                this.schemaForSchema.SchemaTypes.Replace(DatatypeImplementation.QnAnyType, XmlSchemaComplexType.AnyType);
                this.UpdateSForSSimpleTypes();
            }
            foreach (XmlSchemaGroup group in this.groups.Values)
            {
                this.CompileGroup(group);
            }
            foreach (XmlSchemaAttributeGroup group2 in this.attributeGroups.Values)
            {
                this.CompileAttributeGroup(group2);
            }
            foreach (XmlSchemaType type in this.schemaTypes.Values)
            {
                XmlSchemaComplexType complexType = type as XmlSchemaComplexType;
                if (complexType != null)
                {
                    this.CompileComplexType(complexType);
                }
                else
                {
                    this.CompileSimpleType((XmlSchemaSimpleType) type);
                }
            }
            foreach (XmlSchemaElement element in this.elements.Values)
            {
                if (element.ElementDecl == null)
                {
                    this.CompileElement(element);
                }
            }
            foreach (XmlSchemaAttribute attribute in this.attributes.Values)
            {
                if (attribute.AttDef == null)
                {
                    this.CompileAttribute(attribute);
                }
            }
            foreach (XmlSchemaIdentityConstraint constraint in this.identityConstraints.Values)
            {
                if (constraint.CompiledConstraint == null)
                {
                    this.CompileIdentityConstraint(constraint);
                }
            }
            while (this.complexTypeStack.Count > 0)
            {
                XmlSchemaComplexType type3 = (XmlSchemaComplexType) this.complexTypeStack.Pop();
                this.CompileComplexTypeElements(type3);
            }
            this.ProcessSubstitutionGroups();
            foreach (XmlSchemaType type4 in this.schemaTypes.Values)
            {
                XmlSchemaComplexType type5 = type4 as XmlSchemaComplexType;
                if (type5 != null)
                {
                    this.CheckParticleDerivation(type5);
                }
            }
            foreach (XmlSchemaElement element2 in this.elements.Values)
            {
                XmlSchemaComplexType elementSchemaType = element2.ElementSchemaType as XmlSchemaComplexType;
                if ((elementSchemaType != null) && (element2.SchemaTypeName == XmlQualifiedName.Empty))
                {
                    this.CheckParticleDerivation(elementSchemaType);
                }
            }
            foreach (XmlSchemaGroup group3 in this.groups.Values)
            {
                XmlSchemaGroup redefined = group3.Redefined;
                if (redefined != null)
                {
                    this.RecursivelyCheckRedefinedGroups(group3, redefined);
                }
            }
            foreach (XmlSchemaAttributeGroup group5 in this.attributeGroups.Values)
            {
                XmlSchemaAttributeGroup baseAttributeGroup = group5.Redefined;
                if (baseAttributeGroup != null)
                {
                    this.RecursivelyCheckRedefinedAttributeGroups(group5, baseAttributeGroup);
                }
            }
            return !base.HasErrors;
        }

        private XmlSchemaAnyAttribute CompileAnyAttributeIntersection(XmlSchemaAnyAttribute a, XmlSchemaAnyAttribute b)
        {
            if (a == null)
            {
                return b;
            }
            if (b == null)
            {
                return a;
            }
            XmlSchemaAnyAttribute attribute = XmlSchemaAnyAttribute.Intersection(a, b, false);
            if (attribute == null)
            {
                base.SendValidationEvent("Sch_UnexpressibleAnyAttribute", a);
            }
            return attribute;
        }

        private XmlSchemaAnyAttribute CompileAnyAttributeUnion(XmlSchemaAnyAttribute a, XmlSchemaAnyAttribute b)
        {
            if (a == null)
            {
                return b;
            }
            if (b == null)
            {
                return a;
            }
            XmlSchemaAnyAttribute attribute = XmlSchemaAnyAttribute.Union(a, b, false);
            if (attribute == null)
            {
                base.SendValidationEvent("Sch_UnexpressibleAnyAttribute", a);
            }
            return attribute;
        }

        private void CompileAttribute(XmlSchemaAttribute xa)
        {
            if (xa.IsProcessing)
            {
                base.SendValidationEvent("Sch_AttributeCircularRef", xa);
            }
            else if (xa.AttDef == null)
            {
                xa.IsProcessing = true;
                SchemaAttDef decl = null;
                try
                {
                    if (!xa.RefName.IsEmpty)
                    {
                        XmlSchemaAttribute attribute = (XmlSchemaAttribute) this.attributes[xa.RefName];
                        if (attribute == null)
                        {
                            throw new XmlSchemaException("Sch_UndeclaredAttribute", xa.RefName.ToString(), xa);
                        }
                        this.CompileAttribute(attribute);
                        if (attribute.AttDef == null)
                        {
                            throw new XmlSchemaException("Sch_RefInvalidAttribute", xa.RefName.ToString(), xa);
                        }
                        decl = attribute.AttDef.Clone();
                        XmlSchemaDatatype datatype = decl.Datatype;
                        if (datatype != null)
                        {
                            if ((attribute.FixedValue == null) && (attribute.DefaultValue == null))
                            {
                                this.SetDefaultFixed(xa, decl);
                            }
                            else if (attribute.FixedValue != null)
                            {
                                if (xa.DefaultValue != null)
                                {
                                    throw new XmlSchemaException("Sch_FixedDefaultInRef", xa.RefName.ToString(), xa);
                                }
                                if (xa.FixedValue != null)
                                {
                                    object obj2 = datatype.ParseValue(xa.FixedValue, base.NameTable, new SchemaNamespaceManager(xa), true);
                                    if (!datatype.IsEqual(decl.DefaultValueTyped, obj2))
                                    {
                                        throw new XmlSchemaException("Sch_FixedInRef", xa.RefName.ToString(), xa);
                                    }
                                }
                            }
                        }
                        xa.SetAttributeType(attribute.AttributeSchemaType);
                    }
                    else
                    {
                        decl = new SchemaAttDef(xa.QualifiedName);
                        if (xa.SchemaType != null)
                        {
                            this.CompileSimpleType(xa.SchemaType);
                            xa.SetAttributeType(xa.SchemaType);
                            decl.SchemaType = xa.SchemaType;
                            decl.Datatype = xa.SchemaType.Datatype;
                        }
                        else if (!xa.SchemaTypeName.IsEmpty)
                        {
                            XmlSchemaSimpleType simpleType = this.GetSimpleType(xa.SchemaTypeName);
                            if (simpleType == null)
                            {
                                throw new XmlSchemaException("Sch_UndeclaredSimpleType", xa.SchemaTypeName.ToString(), xa);
                            }
                            xa.SetAttributeType(simpleType);
                            decl.Datatype = simpleType.Datatype;
                            decl.SchemaType = simpleType;
                        }
                        else
                        {
                            decl.SchemaType = DatatypeImplementation.AnySimpleType;
                            decl.Datatype = DatatypeImplementation.AnySimpleType.Datatype;
                            xa.SetAttributeType(DatatypeImplementation.AnySimpleType);
                        }
                        if (decl.Datatype != null)
                        {
                            decl.Datatype.VerifySchemaValid(this.notations, xa);
                        }
                        this.SetDefaultFixed(xa, decl);
                    }
                    decl.SchemaAttribute = xa;
                    xa.AttDef = decl;
                }
                catch (XmlSchemaException exception)
                {
                    if (exception.SourceSchemaObject == null)
                    {
                        exception.SetSource(xa);
                    }
                    base.SendValidationEvent(exception);
                    xa.AttDef = SchemaAttDef.Empty;
                }
                finally
                {
                    xa.IsProcessing = false;
                }
            }
        }

        private void CompileAttributeGroup(XmlSchemaAttributeGroup attributeGroup)
        {
            if (attributeGroup.IsProcessing)
            {
                base.SendValidationEvent("Sch_AttributeGroupCircularRef", attributeGroup);
            }
            else if (attributeGroup.AttributeUses.Count <= 0)
            {
                attributeGroup.IsProcessing = true;
                XmlSchemaAnyAttribute anyAttribute = attributeGroup.AnyAttribute;
                try
                {
                    for (int i = 0; i < attributeGroup.Attributes.Count; i++)
                    {
                        XmlSchemaAttribute xa = attributeGroup.Attributes[i] as XmlSchemaAttribute;
                        if (xa != null)
                        {
                            if (xa.Use != XmlSchemaUse.Prohibited)
                            {
                                this.CompileAttribute(xa);
                                if (attributeGroup.AttributeUses[xa.QualifiedName] == null)
                                {
                                    attributeGroup.AttributeUses.Add(xa.QualifiedName, xa);
                                }
                                else
                                {
                                    base.SendValidationEvent("Sch_DupAttributeUse", xa.QualifiedName.ToString(), xa);
                                }
                            }
                        }
                        else
                        {
                            XmlSchemaAttributeGroup redefined;
                            XmlSchemaAttributeGroupRef source = (XmlSchemaAttributeGroupRef) attributeGroup.Attributes[i];
                            if ((attributeGroup.Redefined != null) && (source.RefName == attributeGroup.Redefined.QualifiedName))
                            {
                                redefined = attributeGroup.Redefined;
                            }
                            else
                            {
                                redefined = (XmlSchemaAttributeGroup) this.attributeGroups[source.RefName];
                            }
                            if (redefined != null)
                            {
                                this.CompileAttributeGroup(redefined);
                                foreach (XmlSchemaAttribute attribute3 in redefined.AttributeUses.Values)
                                {
                                    if (attributeGroup.AttributeUses[attribute3.QualifiedName] == null)
                                    {
                                        attributeGroup.AttributeUses.Add(attribute3.QualifiedName, attribute3);
                                    }
                                    else
                                    {
                                        base.SendValidationEvent("Sch_DupAttributeUse", attribute3.QualifiedName.ToString(), attribute3);
                                    }
                                }
                                anyAttribute = this.CompileAnyAttributeIntersection(anyAttribute, redefined.AttributeWildcard);
                            }
                            else
                            {
                                base.SendValidationEvent("Sch_UndefAttributeGroupRef", source.RefName.ToString(), source);
                            }
                        }
                    }
                    attributeGroup.AttributeWildcard = anyAttribute;
                }
                finally
                {
                    attributeGroup.IsProcessing = false;
                }
            }
        }

        private XmlSchemaSimpleType[] CompileBaseMemberTypes(XmlSchemaSimpleType simpleType)
        {
            ArrayList memberTypeDefinitions = new ArrayList();
            XmlSchemaSimpleTypeUnion content = (XmlSchemaSimpleTypeUnion) simpleType.Content;
            XmlQualifiedName[] memberTypes = content.MemberTypes;
            if (memberTypes != null)
            {
                for (int i = 0; i < memberTypes.Length; i++)
                {
                    XmlSchemaSimpleType unionMember = this.GetSimpleType(memberTypes[i]);
                    if (unionMember == null)
                    {
                        throw new XmlSchemaException("Sch_UndeclaredSimpleType", memberTypes[i].ToString(), content);
                    }
                    if (unionMember.Datatype.Variety == XmlSchemaDatatypeVariety.Union)
                    {
                        this.CheckUnionType(unionMember, memberTypeDefinitions, simpleType);
                    }
                    else
                    {
                        memberTypeDefinitions.Add(unionMember);
                    }
                    if ((unionMember.FinalResolved & XmlSchemaDerivationMethod.Union) != XmlSchemaDerivationMethod.Empty)
                    {
                        base.SendValidationEvent("Sch_BaseFinalUnion", simpleType);
                    }
                }
            }
            XmlSchemaObjectCollection baseTypes = content.BaseTypes;
            if (baseTypes != null)
            {
                for (int j = 0; j < baseTypes.Count; j++)
                {
                    XmlSchemaSimpleType type2 = (XmlSchemaSimpleType) baseTypes[j];
                    this.CompileSimpleType(type2);
                    if (type2.Datatype.Variety == XmlSchemaDatatypeVariety.Union)
                    {
                        this.CheckUnionType(type2, memberTypeDefinitions, simpleType);
                    }
                    else
                    {
                        memberTypeDefinitions.Add(type2);
                    }
                }
            }
            content.SetBaseMemberTypes(memberTypeDefinitions.ToArray(typeof(XmlSchemaSimpleType)) as XmlSchemaSimpleType[]);
            return content.BaseMemberTypes;
        }

        private ContentValidator CompileComplexContent(XmlSchemaComplexType complexType)
        {
            if (complexType.ContentType == XmlSchemaContentType.Empty)
            {
                return ContentValidator.Empty;
            }
            if (complexType.ContentType == XmlSchemaContentType.TextOnly)
            {
                return ContentValidator.TextOnly;
            }
            XmlSchemaParticle contentTypeParticle = complexType.ContentTypeParticle;
            if ((contentTypeParticle == null) || (contentTypeParticle == XmlSchemaParticle.Empty))
            {
                if (complexType.ContentType == XmlSchemaContentType.ElementOnly)
                {
                    return ContentValidator.Empty;
                }
                return ContentValidator.Mixed;
            }
            this.PushComplexType(complexType);
            if (contentTypeParticle is XmlSchemaAll)
            {
                XmlSchemaAll all = (XmlSchemaAll) contentTypeParticle;
                AllElementsContentValidator validator = new AllElementsContentValidator(complexType.ContentType, all.Items.Count, all.MinOccurs == 0M);
                for (int i = 0; i < all.Items.Count; i++)
                {
                    XmlSchemaElement particle = (XmlSchemaElement) all.Items[i];
                    if (!validator.AddElement(particle.QualifiedName, particle, particle.MinOccurs == 0M))
                    {
                        base.SendValidationEvent("Sch_DupElement", particle.QualifiedName.ToString(), particle);
                    }
                }
                return validator;
            }
            ParticleContentValidator contentValidator = new ParticleContentValidator(complexType.ContentType, base.CompilationSettings.EnableUpaCheck);
            try
            {
                contentValidator.Start();
                complexType.HasWildCard = this.BuildParticleContentModel(contentValidator, contentTypeParticle);
                return contentValidator.Finish(true);
            }
            catch (UpaException exception)
            {
                if (exception.Particle1 is XmlSchemaElement)
                {
                    if (exception.Particle2 is XmlSchemaElement)
                    {
                        base.SendValidationEvent("Sch_NonDeterministic", ((XmlSchemaElement) exception.Particle1).QualifiedName.ToString(), (XmlSchemaElement) exception.Particle2);
                    }
                    else
                    {
                        base.SendValidationEvent("Sch_NonDeterministicAnyEx", ((XmlSchemaAny) exception.Particle2).ResolvedNamespace, ((XmlSchemaElement) exception.Particle1).QualifiedName.ToString(), (XmlSchemaAny) exception.Particle2);
                    }
                }
                else if (exception.Particle2 is XmlSchemaElement)
                {
                    base.SendValidationEvent("Sch_NonDeterministicAnyEx", ((XmlSchemaAny) exception.Particle1).ResolvedNamespace, ((XmlSchemaElement) exception.Particle2).QualifiedName.ToString(), (XmlSchemaElement) exception.Particle2);
                }
                else
                {
                    base.SendValidationEvent("Sch_NonDeterministicAnyAny", ((XmlSchemaAny) exception.Particle1).ResolvedNamespace, ((XmlSchemaAny) exception.Particle2).ResolvedNamespace, (XmlSchemaAny) exception.Particle2);
                }
                return XmlSchemaComplexType.AnyTypeContentValidator;
            }
            catch (NotSupportedException)
            {
                base.SendValidationEvent("Sch_ComplexContentModel", complexType, XmlSeverityType.Warning);
                return XmlSchemaComplexType.AnyTypeContentValidator;
            }
        }

        private void CompileComplexContentExtension(XmlSchemaComplexType complexType, XmlSchemaComplexContent complexContent, XmlSchemaComplexContentExtension complexExtension)
        {
            XmlSchemaComplexType redefined = null;
            if ((complexType.Redefined != null) && (complexExtension.BaseTypeName == complexType.Redefined.QualifiedName))
            {
                redefined = (XmlSchemaComplexType) complexType.Redefined;
                this.CompileComplexType(redefined);
            }
            else
            {
                redefined = this.GetComplexType(complexExtension.BaseTypeName);
                if (redefined == null)
                {
                    base.SendValidationEvent("Sch_UndefBaseExtension", complexExtension.BaseTypeName.ToString(), complexExtension);
                    return;
                }
            }
            if ((redefined.FinalResolved & XmlSchemaDerivationMethod.Extension) != XmlSchemaDerivationMethod.Empty)
            {
                base.SendValidationEvent("Sch_BaseFinalExtension", complexType);
            }
            this.CompileLocalAttributes(redefined, complexType, complexExtension.Attributes, complexExtension.AnyAttribute, XmlSchemaDerivationMethod.Extension);
            XmlSchemaParticle contentTypeParticle = redefined.ContentTypeParticle;
            XmlSchemaParticle item = this.CannonicalizeParticle(complexExtension.Particle, true);
            if (contentTypeParticle != XmlSchemaParticle.Empty)
            {
                if (item != XmlSchemaParticle.Empty)
                {
                    XmlSchemaSequence particle = new XmlSchemaSequence();
                    particle.Items.Add(contentTypeParticle);
                    particle.Items.Add(item);
                    complexType.SetContentTypeParticle(this.CompileContentTypeParticle(particle));
                }
                else
                {
                    complexType.SetContentTypeParticle(contentTypeParticle);
                }
            }
            else
            {
                complexType.SetContentTypeParticle(item);
            }
            XmlSchemaContentType contentType = this.GetSchemaContentType(complexType, complexContent, item);
            if (contentType == XmlSchemaContentType.Empty)
            {
                contentType = redefined.ContentType;
                if (contentType == XmlSchemaContentType.TextOnly)
                {
                    complexType.SetDatatype(redefined.Datatype);
                }
            }
            complexType.SetContentType(contentType);
            if ((redefined.ContentType != XmlSchemaContentType.Empty) && (complexType.ContentType != redefined.ContentType))
            {
                base.SendValidationEvent("Sch_DifContentType", complexType);
            }
            else
            {
                complexType.SetBaseSchemaType(redefined);
                complexType.SetDerivedBy(XmlSchemaDerivationMethod.Extension);
            }
        }

        private void CompileComplexContentRestriction(XmlSchemaComplexType complexType, XmlSchemaComplexContent complexContent, XmlSchemaComplexContentRestriction complexRestriction)
        {
            XmlSchemaComplexType redefined = null;
            if ((complexType.Redefined != null) && (complexRestriction.BaseTypeName == complexType.Redefined.QualifiedName))
            {
                redefined = (XmlSchemaComplexType) complexType.Redefined;
                this.CompileComplexType(redefined);
            }
            else
            {
                redefined = this.GetComplexType(complexRestriction.BaseTypeName);
                if (redefined == null)
                {
                    base.SendValidationEvent("Sch_UndefBaseRestriction", complexRestriction.BaseTypeName.ToString(), complexRestriction);
                    return;
                }
            }
            complexType.SetBaseSchemaType(redefined);
            if ((redefined.FinalResolved & XmlSchemaDerivationMethod.Restriction) != XmlSchemaDerivationMethod.Empty)
            {
                base.SendValidationEvent("Sch_BaseFinalRestriction", complexType);
            }
            this.CompileLocalAttributes(redefined, complexType, complexRestriction.Attributes, complexRestriction.AnyAttribute, XmlSchemaDerivationMethod.Restriction);
            complexType.SetContentTypeParticle(this.CompileContentTypeParticle(complexRestriction.Particle));
            XmlSchemaContentType type2 = this.GetSchemaContentType(complexType, complexContent, complexType.ContentTypeParticle);
            complexType.SetContentType(type2);
            switch (type2)
            {
                case XmlSchemaContentType.Empty:
                    if ((redefined.ElementDecl != null) && !redefined.ElementDecl.ContentValidator.IsEmptiable)
                    {
                        base.SendValidationEvent("Sch_InvalidContentRestrictionDetailed", Res.GetString("Sch_InvalidBaseToEmpty"), complexType);
                    }
                    break;

                case XmlSchemaContentType.Mixed:
                    if (redefined.ContentType != XmlSchemaContentType.Mixed)
                    {
                        base.SendValidationEvent("Sch_InvalidContentRestrictionDetailed", Res.GetString("Sch_InvalidBaseToMixed"), complexType);
                    }
                    break;
            }
            complexType.SetDerivedBy(XmlSchemaDerivationMethod.Restriction);
        }

        private void CompileComplexType(XmlSchemaComplexType complexType)
        {
            if (complexType.ElementDecl == null)
            {
                if (complexType.IsProcessing)
                {
                    base.SendValidationEvent("Sch_TypeCircularRef", complexType);
                }
                else
                {
                    complexType.IsProcessing = true;
                    try
                    {
                        if (complexType.ContentModel != null)
                        {
                            if (complexType.ContentModel is XmlSchemaSimpleContent)
                            {
                                XmlSchemaSimpleContent contentModel = (XmlSchemaSimpleContent) complexType.ContentModel;
                                complexType.SetContentType(XmlSchemaContentType.TextOnly);
                                if (contentModel.Content is XmlSchemaSimpleContentExtension)
                                {
                                    this.CompileSimpleContentExtension(complexType, (XmlSchemaSimpleContentExtension) contentModel.Content);
                                }
                                else
                                {
                                    this.CompileSimpleContentRestriction(complexType, (XmlSchemaSimpleContentRestriction) contentModel.Content);
                                }
                            }
                            else
                            {
                                XmlSchemaComplexContent complexContent = (XmlSchemaComplexContent) complexType.ContentModel;
                                if (complexContent.Content is XmlSchemaComplexContentExtension)
                                {
                                    this.CompileComplexContentExtension(complexType, complexContent, (XmlSchemaComplexContentExtension) complexContent.Content);
                                }
                                else
                                {
                                    this.CompileComplexContentRestriction(complexType, complexContent, (XmlSchemaComplexContentRestriction) complexContent.Content);
                                }
                            }
                        }
                        else
                        {
                            complexType.SetBaseSchemaType(XmlSchemaComplexType.AnyType);
                            this.CompileLocalAttributes(XmlSchemaComplexType.AnyType, complexType, complexType.Attributes, complexType.AnyAttribute, XmlSchemaDerivationMethod.Restriction);
                            complexType.SetDerivedBy(XmlSchemaDerivationMethod.Restriction);
                            complexType.SetContentTypeParticle(this.CompileContentTypeParticle(complexType.Particle));
                            complexType.SetContentType(this.GetSchemaContentType(complexType, null, complexType.ContentTypeParticle));
                        }
                        if (complexType.ContainsIdAttribute(true))
                        {
                            base.SendValidationEvent("Sch_TwoIdAttrUses", complexType);
                        }
                        SchemaElementDecl decl = new SchemaElementDecl {
                            ContentValidator = this.CompileComplexContent(complexType),
                            SchemaType = complexType,
                            IsAbstract = complexType.IsAbstract,
                            Datatype = complexType.Datatype,
                            Block = complexType.BlockResolved,
                            AnyAttribute = complexType.AttributeWildcard
                        };
                        foreach (XmlSchemaAttribute attribute in complexType.AttributeUses.Values)
                        {
                            if (attribute.Use == XmlSchemaUse.Prohibited)
                            {
                                if (!decl.ProhibitedAttributes.ContainsKey(attribute.QualifiedName))
                                {
                                    decl.ProhibitedAttributes.Add(attribute.QualifiedName, attribute.QualifiedName);
                                }
                            }
                            else if ((!decl.AttDefs.ContainsKey(attribute.QualifiedName) && (attribute.AttDef != null)) && ((attribute.AttDef.Name != XmlQualifiedName.Empty) && (attribute.AttDef != SchemaAttDef.Empty)))
                            {
                                decl.AddAttDef(attribute.AttDef);
                            }
                        }
                        complexType.ElementDecl = decl;
                    }
                    finally
                    {
                        complexType.IsProcessing = false;
                    }
                }
            }
        }

        private void CompileComplexTypeElements(XmlSchemaComplexType complexType)
        {
            if (complexType.IsProcessing)
            {
                base.SendValidationEvent("Sch_TypeCircularRef", complexType);
            }
            else
            {
                complexType.IsProcessing = true;
                try
                {
                    if (complexType.ContentTypeParticle != XmlSchemaParticle.Empty)
                    {
                        this.CompileParticleElements(complexType, complexType.ContentTypeParticle);
                    }
                }
                finally
                {
                    complexType.IsProcessing = false;
                }
            }
        }

        private XmlSchemaParticle CompileContentTypeParticle(XmlSchemaParticle particle)
        {
            XmlSchemaParticle particle2 = this.CannonicalizeParticle(particle, true);
            XmlSchemaChoice source = particle2 as XmlSchemaChoice;
            if ((source == null) || (source.Items.Count != 0))
            {
                return particle2;
            }
            if (source.MinOccurs != 0M)
            {
                base.SendValidationEvent("Sch_EmptyChoice", source, XmlSeverityType.Warning);
            }
            return XmlSchemaParticle.Empty;
        }

        private void CompileElement(XmlSchemaElement xe)
        {
            if (xe.IsProcessing)
            {
                base.SendValidationEvent("Sch_ElementCircularRef", xe);
            }
            else if (xe.ElementDecl == null)
            {
                xe.IsProcessing = true;
                SchemaElementDecl decl = null;
                try
                {
                    if (!xe.RefName.IsEmpty)
                    {
                        XmlSchemaElement element = (XmlSchemaElement) this.elements[xe.RefName];
                        if (element == null)
                        {
                            throw new XmlSchemaException("Sch_UndeclaredElement", xe.RefName.ToString(), xe);
                        }
                        this.CompileElement(element);
                        if (element.ElementDecl == null)
                        {
                            throw new XmlSchemaException("Sch_RefInvalidElement", xe.RefName.ToString(), xe);
                        }
                        xe.SetElementType(element.ElementSchemaType);
                        decl = element.ElementDecl.Clone();
                    }
                    else
                    {
                        if (xe.SchemaType != null)
                        {
                            xe.SetElementType(xe.SchemaType);
                        }
                        else if (!xe.SchemaTypeName.IsEmpty)
                        {
                            xe.SetElementType(this.GetAnySchemaType(xe.SchemaTypeName));
                            if (xe.ElementSchemaType == null)
                            {
                                throw new XmlSchemaException("Sch_UndeclaredType", xe.SchemaTypeName.ToString(), xe);
                            }
                        }
                        else if (!xe.SubstitutionGroup.IsEmpty)
                        {
                            XmlSchemaElement element2 = (XmlSchemaElement) this.elements[xe.SubstitutionGroup];
                            if (element2 == null)
                            {
                                throw new XmlSchemaException("Sch_UndeclaredEquivClass", xe.SubstitutionGroup.Name.ToString(CultureInfo.InvariantCulture), xe);
                            }
                            if (element2.IsProcessing)
                            {
                                return;
                            }
                            this.CompileElement(element2);
                            if (element2.ElementDecl == null)
                            {
                                xe.SetElementType(XmlSchemaComplexType.AnyType);
                                decl = XmlSchemaComplexType.AnyType.ElementDecl.Clone();
                            }
                            else
                            {
                                xe.SetElementType(element2.ElementSchemaType);
                                decl = element2.ElementDecl.Clone();
                            }
                        }
                        else
                        {
                            xe.SetElementType(XmlSchemaComplexType.AnyType);
                            decl = XmlSchemaComplexType.AnyType.ElementDecl.Clone();
                        }
                        if (decl == null)
                        {
                            if (xe.ElementSchemaType is XmlSchemaComplexType)
                            {
                                XmlSchemaComplexType complexType = (XmlSchemaComplexType) xe.ElementSchemaType;
                                this.CompileComplexType(complexType);
                                if (complexType.ElementDecl != null)
                                {
                                    decl = complexType.ElementDecl.Clone();
                                }
                            }
                            else if (xe.ElementSchemaType is XmlSchemaSimpleType)
                            {
                                XmlSchemaSimpleType simpleType = (XmlSchemaSimpleType) xe.ElementSchemaType;
                                this.CompileSimpleType(simpleType);
                                if (simpleType.ElementDecl != null)
                                {
                                    decl = simpleType.ElementDecl.Clone();
                                }
                            }
                        }
                        decl.Name = xe.QualifiedName;
                        decl.IsAbstract = xe.IsAbstract;
                        XmlSchemaComplexType elementSchemaType = xe.ElementSchemaType as XmlSchemaComplexType;
                        if (elementSchemaType != null)
                        {
                            decl.IsAbstract |= elementSchemaType.IsAbstract;
                        }
                        decl.IsNillable = xe.IsNillable;
                        decl.Block |= xe.BlockResolved;
                    }
                    if (decl.Datatype != null)
                    {
                        decl.Datatype.VerifySchemaValid(this.notations, xe);
                    }
                    if (((xe.DefaultValue != null) || (xe.FixedValue != null)) && (decl.ContentValidator != null))
                    {
                        if ((decl.ContentValidator.ContentType != XmlSchemaContentType.TextOnly) && ((decl.ContentValidator.ContentType != XmlSchemaContentType.Mixed) || !decl.ContentValidator.IsEmptiable))
                        {
                            throw new XmlSchemaException("Sch_ElementCannotHaveValue", xe);
                        }
                        if (xe.DefaultValue != null)
                        {
                            decl.Presence = SchemaDeclBase.Use.Default;
                            decl.DefaultValueRaw = xe.DefaultValue;
                        }
                        else
                        {
                            decl.Presence = SchemaDeclBase.Use.Fixed;
                            decl.DefaultValueRaw = xe.FixedValue;
                        }
                        if (decl.Datatype != null)
                        {
                            if (decl.Datatype.TypeCode == XmlTypeCode.Id)
                            {
                                base.SendValidationEvent("Sch_DefaultIdValue", xe);
                            }
                            else
                            {
                                decl.DefaultValueTyped = decl.Datatype.ParseValue(decl.DefaultValueRaw, base.NameTable, new SchemaNamespaceManager(xe), true);
                            }
                        }
                        else
                        {
                            decl.DefaultValueTyped = DatatypeImplementation.AnySimpleType.Datatype.ParseValue(decl.DefaultValueRaw, base.NameTable, new SchemaNamespaceManager(xe));
                        }
                    }
                    if (xe.HasConstraints)
                    {
                        XmlSchemaObjectCollection constraints = xe.Constraints;
                        CompiledIdentityConstraint[] constraintArray = new CompiledIdentityConstraint[constraints.Count];
                        int num = 0;
                        for (int i = 0; i < constraints.Count; i++)
                        {
                            XmlSchemaIdentityConstraint xi = (XmlSchemaIdentityConstraint) constraints[i];
                            this.CompileIdentityConstraint(xi);
                            constraintArray[num++] = xi.CompiledConstraint;
                        }
                        decl.Constraints = constraintArray;
                    }
                    decl.SchemaElement = xe;
                    xe.ElementDecl = decl;
                }
                catch (XmlSchemaException exception)
                {
                    if (exception.SourceSchemaObject == null)
                    {
                        exception.SetSource(xe);
                    }
                    base.SendValidationEvent(exception);
                    xe.ElementDecl = SchemaElementDecl.Empty;
                }
                finally
                {
                    xe.IsProcessing = false;
                }
            }
        }

        private void CompileGroup(XmlSchemaGroup group)
        {
            if (group.IsProcessing)
            {
                base.SendValidationEvent("Sch_GroupCircularRef", group);
                group.CanonicalParticle = XmlSchemaParticle.Empty;
            }
            else
            {
                group.IsProcessing = true;
                if (group.CanonicalParticle == null)
                {
                    group.CanonicalParticle = this.CannonicalizeParticle(group.Particle, true);
                }
                group.IsProcessing = false;
            }
        }

        private void CompileIdentityConstraint(XmlSchemaIdentityConstraint xi)
        {
            if (xi.IsProcessing)
            {
                xi.CompiledConstraint = CompiledIdentityConstraint.Empty;
                base.SendValidationEvent("Sch_IdentityConstraintCircularRef", xi);
            }
            else if (xi.CompiledConstraint == null)
            {
                xi.IsProcessing = true;
                CompiledIdentityConstraint constraint = null;
                try
                {
                    SchemaNamespaceManager nsmgr = new SchemaNamespaceManager(xi);
                    constraint = new CompiledIdentityConstraint(xi, nsmgr);
                    if (xi is XmlSchemaKeyref)
                    {
                        XmlSchemaIdentityConstraint constraint2 = (XmlSchemaIdentityConstraint) this.identityConstraints[((XmlSchemaKeyref) xi).Refer];
                        if (constraint2 == null)
                        {
                            throw new XmlSchemaException("Sch_UndeclaredIdentityConstraint", ((XmlSchemaKeyref) xi).Refer.ToString(), xi);
                        }
                        this.CompileIdentityConstraint(constraint2);
                        if (constraint2.CompiledConstraint == null)
                        {
                            throw new XmlSchemaException("Sch_RefInvalidIdentityConstraint", ((XmlSchemaKeyref) xi).Refer.ToString(), xi);
                        }
                        if (constraint2.Fields.Count != xi.Fields.Count)
                        {
                            throw new XmlSchemaException("Sch_RefInvalidCardin", xi.QualifiedName.ToString(), xi);
                        }
                        if (constraint2.CompiledConstraint.Role == CompiledIdentityConstraint.ConstraintRole.Keyref)
                        {
                            throw new XmlSchemaException("Sch_ReftoKeyref", xi.QualifiedName.ToString(), xi);
                        }
                    }
                    xi.CompiledConstraint = constraint;
                }
                catch (XmlSchemaException exception)
                {
                    if (exception.SourceSchemaObject == null)
                    {
                        exception.SetSource(xi);
                    }
                    base.SendValidationEvent(exception);
                    xi.CompiledConstraint = CompiledIdentityConstraint.Empty;
                }
                finally
                {
                    xi.IsProcessing = false;
                }
            }
        }

        private void CompileLocalAttributes(XmlSchemaComplexType baseType, XmlSchemaComplexType derivedType, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute, XmlSchemaDerivationMethod derivedBy)
        {
            XmlSchemaAnyAttribute b = (baseType != null) ? baseType.AttributeWildcard : null;
            for (int i = 0; i < attributes.Count; i++)
            {
                XmlSchemaAttribute xa = attributes[i] as XmlSchemaAttribute;
                if (xa != null)
                {
                    if (xa.Use != XmlSchemaUse.Prohibited)
                    {
                        this.CompileAttribute(xa);
                    }
                    if ((xa.Use != XmlSchemaUse.Prohibited) || (((xa.Use == XmlSchemaUse.Prohibited) && (derivedBy == XmlSchemaDerivationMethod.Restriction)) && (baseType != XmlSchemaComplexType.AnyType)))
                    {
                        if (derivedType.AttributeUses[xa.QualifiedName] == null)
                        {
                            derivedType.AttributeUses.Add(xa.QualifiedName, xa);
                        }
                        else
                        {
                            base.SendValidationEvent("Sch_DupAttributeUse", xa.QualifiedName.ToString(), xa);
                        }
                    }
                    else
                    {
                        base.SendValidationEvent("Sch_AttributeIgnored", xa.QualifiedName.ToString(), xa, XmlSeverityType.Warning);
                    }
                }
                else
                {
                    XmlSchemaAttributeGroupRef source = (XmlSchemaAttributeGroupRef) attributes[i];
                    XmlSchemaAttributeGroup attributeGroup = (XmlSchemaAttributeGroup) this.attributeGroups[source.RefName];
                    if (attributeGroup != null)
                    {
                        this.CompileAttributeGroup(attributeGroup);
                        foreach (XmlSchemaAttribute attribute3 in attributeGroup.AttributeUses.Values)
                        {
                            if ((attribute3.Use != XmlSchemaUse.Prohibited) || (((attribute3.Use == XmlSchemaUse.Prohibited) && (derivedBy == XmlSchemaDerivationMethod.Restriction)) && (baseType != XmlSchemaComplexType.AnyType)))
                            {
                                if (derivedType.AttributeUses[attribute3.QualifiedName] == null)
                                {
                                    derivedType.AttributeUses.Add(attribute3.QualifiedName, attribute3);
                                }
                                else
                                {
                                    base.SendValidationEvent("Sch_DupAttributeUse", attribute3.QualifiedName.ToString(), source);
                                }
                            }
                            else
                            {
                                base.SendValidationEvent("Sch_AttributeIgnored", attribute3.QualifiedName.ToString(), attribute3, XmlSeverityType.Warning);
                            }
                        }
                        anyAttribute = this.CompileAnyAttributeIntersection(anyAttribute, attributeGroup.AttributeWildcard);
                    }
                    else
                    {
                        base.SendValidationEvent("Sch_UndefAttributeGroupRef", source.RefName.ToString(), source);
                    }
                }
            }
            if (baseType != null)
            {
                if (derivedBy == XmlSchemaDerivationMethod.Extension)
                {
                    derivedType.SetAttributeWildcard(this.CompileAnyAttributeUnion(anyAttribute, b));
                    foreach (XmlSchemaAttribute attribute4 in baseType.AttributeUses.Values)
                    {
                        XmlSchemaAttribute attribute5 = (XmlSchemaAttribute) derivedType.AttributeUses[attribute4.QualifiedName];
                        if (attribute5 == null)
                        {
                            derivedType.AttributeUses.Add(attribute4.QualifiedName, attribute4);
                        }
                        else if ((attribute4.Use != XmlSchemaUse.Prohibited) && (attribute5.AttributeSchemaType != attribute4.AttributeSchemaType))
                        {
                            base.SendValidationEvent("Sch_InvalidAttributeExtension", attribute5);
                        }
                    }
                }
                else
                {
                    if ((anyAttribute != null) && (((b == null) || !XmlSchemaAnyAttribute.IsSubset(anyAttribute, b)) || !this.IsProcessContentsRestricted(baseType, anyAttribute, b)))
                    {
                        base.SendValidationEvent("Sch_InvalidAnyAttributeRestriction", derivedType);
                    }
                    else
                    {
                        derivedType.SetAttributeWildcard(anyAttribute);
                    }
                    foreach (XmlSchemaAttribute attribute6 in baseType.AttributeUses.Values)
                    {
                        XmlSchemaAttribute attribute7 = (XmlSchemaAttribute) derivedType.AttributeUses[attribute6.QualifiedName];
                        if (attribute7 == null)
                        {
                            derivedType.AttributeUses.Add(attribute6.QualifiedName, attribute6);
                        }
                        else if ((attribute6.Use == XmlSchemaUse.Prohibited) && (attribute7.Use != XmlSchemaUse.Prohibited))
                        {
                            base.SendValidationEvent("Sch_AttributeRestrictionProhibited", attribute7);
                        }
                        else if ((attribute6.Use == XmlSchemaUse.Required) && (attribute7.Use != XmlSchemaUse.Required))
                        {
                            base.SendValidationEvent("Sch_AttributeUseInvalid", attribute7);
                        }
                        else if (attribute7.Use != XmlSchemaUse.Prohibited)
                        {
                            if (((attribute6.AttributeSchemaType == null) || (attribute7.AttributeSchemaType == null)) || !XmlSchemaType.IsDerivedFrom(attribute7.AttributeSchemaType, attribute6.AttributeSchemaType, XmlSchemaDerivationMethod.Empty))
                            {
                                base.SendValidationEvent("Sch_AttributeRestrictionInvalid", attribute7);
                            }
                            else if (!this.IsFixedEqual(attribute6.AttDef, attribute7.AttDef))
                            {
                                base.SendValidationEvent("Sch_AttributeFixedInvalid", attribute7);
                            }
                        }
                    }
                    foreach (XmlSchemaAttribute attribute8 in derivedType.AttributeUses.Values)
                    {
                        if ((((XmlSchemaAttribute) baseType.AttributeUses[attribute8.QualifiedName]) == null) && ((b == null) || !b.Allows(attribute8.QualifiedName)))
                        {
                            base.SendValidationEvent("Sch_AttributeRestrictionInvalidFromWildcard", attribute8);
                        }
                    }
                }
            }
            else
            {
                derivedType.SetAttributeWildcard(anyAttribute);
            }
        }

        private void CompileParticleElements(XmlSchemaParticle particle)
        {
            if (particle is XmlSchemaElement)
            {
                XmlSchemaElement xe = (XmlSchemaElement) particle;
                this.CompileElement(xe);
            }
            else if (particle is XmlSchemaGroupBase)
            {
                XmlSchemaObjectCollection items = ((XmlSchemaGroupBase) particle).Items;
                for (int i = 0; i < items.Count; i++)
                {
                    this.CompileParticleElements((XmlSchemaParticle) items[i]);
                }
            }
        }

        private void CompileParticleElements(XmlSchemaComplexType complexType, XmlSchemaParticle particle)
        {
            if (particle is XmlSchemaElement)
            {
                XmlSchemaElement xe = (XmlSchemaElement) particle;
                this.CompileElement(xe);
                if (complexType.LocalElements[xe.QualifiedName] == null)
                {
                    complexType.LocalElements.Add(xe.QualifiedName, xe);
                }
                else
                {
                    XmlSchemaElement element2 = (XmlSchemaElement) complexType.LocalElements[xe.QualifiedName];
                    if (element2.ElementSchemaType != xe.ElementSchemaType)
                    {
                        base.SendValidationEvent("Sch_ElementTypeCollision", particle);
                    }
                }
            }
            else if (particle is XmlSchemaGroupBase)
            {
                XmlSchemaObjectCollection items = ((XmlSchemaGroupBase) particle).Items;
                for (int i = 0; i < items.Count; i++)
                {
                    this.CompileParticleElements(complexType, (XmlSchemaParticle) items[i]);
                }
            }
        }

        private void CompileSimpleContentExtension(XmlSchemaComplexType complexType, XmlSchemaSimpleContentExtension simpleExtension)
        {
            XmlSchemaComplexType redefined = null;
            if ((complexType.Redefined != null) && (simpleExtension.BaseTypeName == complexType.Redefined.QualifiedName))
            {
                redefined = (XmlSchemaComplexType) complexType.Redefined;
                this.CompileComplexType(redefined);
                complexType.SetBaseSchemaType(redefined);
                complexType.SetDatatype(redefined.Datatype);
            }
            else
            {
                XmlSchemaType anySchemaType = this.GetAnySchemaType(simpleExtension.BaseTypeName);
                if (anySchemaType == null)
                {
                    base.SendValidationEvent("Sch_UndeclaredType", simpleExtension.BaseTypeName.ToString(), simpleExtension);
                }
                else
                {
                    complexType.SetBaseSchemaType(anySchemaType);
                    complexType.SetDatatype(anySchemaType.Datatype);
                }
                redefined = anySchemaType as XmlSchemaComplexType;
            }
            if (redefined != null)
            {
                if ((redefined.FinalResolved & XmlSchemaDerivationMethod.Extension) != XmlSchemaDerivationMethod.Empty)
                {
                    base.SendValidationEvent("Sch_BaseFinalExtension", complexType);
                }
                if (redefined.ContentType != XmlSchemaContentType.TextOnly)
                {
                    base.SendValidationEvent("Sch_NotSimpleContent", complexType);
                }
            }
            complexType.SetDerivedBy(XmlSchemaDerivationMethod.Extension);
            this.CompileLocalAttributes(redefined, complexType, simpleExtension.Attributes, simpleExtension.AnyAttribute, XmlSchemaDerivationMethod.Extension);
        }

        private void CompileSimpleContentRestriction(XmlSchemaComplexType complexType, XmlSchemaSimpleContentRestriction simpleRestriction)
        {
            XmlSchemaComplexType redefined = null;
            XmlSchemaDatatype datatype = null;
            if ((complexType.Redefined != null) && (simpleRestriction.BaseTypeName == complexType.Redefined.QualifiedName))
            {
                redefined = (XmlSchemaComplexType) complexType.Redefined;
                this.CompileComplexType(redefined);
                datatype = redefined.Datatype;
            }
            else
            {
                redefined = this.GetComplexType(simpleRestriction.BaseTypeName);
                if (redefined == null)
                {
                    base.SendValidationEvent("Sch_UndefBaseRestriction", simpleRestriction.BaseTypeName.ToString(), simpleRestriction);
                    return;
                }
                if (redefined.ContentType == XmlSchemaContentType.TextOnly)
                {
                    if (simpleRestriction.BaseType == null)
                    {
                        datatype = redefined.Datatype;
                    }
                    else
                    {
                        this.CompileSimpleType(simpleRestriction.BaseType);
                        if (!XmlSchemaType.IsDerivedFromDatatype(simpleRestriction.BaseType.Datatype, redefined.Datatype, XmlSchemaDerivationMethod.None))
                        {
                            base.SendValidationEvent("Sch_DerivedNotFromBase", simpleRestriction);
                        }
                        datatype = simpleRestriction.BaseType.Datatype;
                    }
                }
                else if ((redefined.ContentType == XmlSchemaContentType.Mixed) && redefined.ElementDecl.ContentValidator.IsEmptiable)
                {
                    if (simpleRestriction.BaseType != null)
                    {
                        this.CompileSimpleType(simpleRestriction.BaseType);
                        complexType.SetBaseSchemaType(simpleRestriction.BaseType);
                        datatype = simpleRestriction.BaseType.Datatype;
                    }
                    else
                    {
                        base.SendValidationEvent("Sch_NeedSimpleTypeChild", simpleRestriction);
                    }
                }
                else
                {
                    base.SendValidationEvent("Sch_NotSimpleContent", complexType);
                }
            }
            if (((redefined != null) && (redefined.ElementDecl != null)) && ((redefined.FinalResolved & XmlSchemaDerivationMethod.Restriction) != XmlSchemaDerivationMethod.Empty))
            {
                base.SendValidationEvent("Sch_BaseFinalRestriction", complexType);
            }
            if (redefined != null)
            {
                complexType.SetBaseSchemaType(redefined);
            }
            if (datatype != null)
            {
                try
                {
                    complexType.SetDatatype(datatype.DeriveByRestriction(simpleRestriction.Facets, base.NameTable, complexType));
                }
                catch (XmlSchemaException exception)
                {
                    if (exception.SourceSchemaObject == null)
                    {
                        exception.SetSource(complexType);
                    }
                    base.SendValidationEvent(exception);
                    complexType.SetDatatype(DatatypeImplementation.AnySimpleType.Datatype);
                }
            }
            complexType.SetDerivedBy(XmlSchemaDerivationMethod.Restriction);
            this.CompileLocalAttributes(redefined, complexType, simpleRestriction.Attributes, simpleRestriction.AnyAttribute, XmlSchemaDerivationMethod.Restriction);
        }

        private void CompileSimpleType(XmlSchemaSimpleType simpleType)
        {
            if (simpleType.IsProcessing)
            {
                throw new XmlSchemaException("Sch_TypeCircularRef", simpleType);
            }
            if (simpleType.ElementDecl == null)
            {
                simpleType.IsProcessing = true;
                try
                {
                    if (simpleType.Content is XmlSchemaSimpleTypeList)
                    {
                        XmlSchemaDatatype datatype;
                        XmlSchemaSimpleTypeList content = (XmlSchemaSimpleTypeList) simpleType.Content;
                        simpleType.SetBaseSchemaType(DatatypeImplementation.AnySimpleType);
                        if (content.ItemTypeName.IsEmpty)
                        {
                            this.CompileSimpleType(content.ItemType);
                            content.BaseItemType = content.ItemType;
                            datatype = content.ItemType.Datatype;
                        }
                        else
                        {
                            XmlSchemaSimpleType type = this.GetSimpleType(content.ItemTypeName);
                            if (type == null)
                            {
                                throw new XmlSchemaException("Sch_UndeclaredSimpleType", content.ItemTypeName.ToString(), content);
                            }
                            if ((type.FinalResolved & XmlSchemaDerivationMethod.List) != XmlSchemaDerivationMethod.Empty)
                            {
                                base.SendValidationEvent("Sch_BaseFinalList", simpleType);
                            }
                            content.BaseItemType = type;
                            datatype = type.Datatype;
                        }
                        simpleType.SetDatatype(datatype.DeriveByList(simpleType));
                        simpleType.SetDerivedBy(XmlSchemaDerivationMethod.List);
                    }
                    else if (simpleType.Content is XmlSchemaSimpleTypeRestriction)
                    {
                        XmlSchemaDatatype datatype2;
                        XmlSchemaSimpleTypeRestriction source = (XmlSchemaSimpleTypeRestriction) simpleType.Content;
                        if (source.BaseTypeName.IsEmpty)
                        {
                            this.CompileSimpleType(source.BaseType);
                            simpleType.SetBaseSchemaType(source.BaseType);
                            datatype2 = source.BaseType.Datatype;
                        }
                        else if ((simpleType.Redefined != null) && (source.BaseTypeName == simpleType.Redefined.QualifiedName))
                        {
                            this.CompileSimpleType((XmlSchemaSimpleType) simpleType.Redefined);
                            simpleType.SetBaseSchemaType(simpleType.Redefined.BaseXmlSchemaType);
                            datatype2 = simpleType.Redefined.Datatype;
                        }
                        else
                        {
                            if (source.BaseTypeName.Equals(DatatypeImplementation.QnAnySimpleType) && (Preprocessor.GetParentSchema(simpleType).TargetNamespace != "http://www.w3.org/2001/XMLSchema"))
                            {
                                throw new XmlSchemaException("Sch_InvalidSimpleTypeRestriction", source.BaseTypeName.ToString(), simpleType);
                            }
                            XmlSchemaSimpleType type2 = this.GetSimpleType(source.BaseTypeName);
                            if (type2 == null)
                            {
                                throw new XmlSchemaException("Sch_UndeclaredSimpleType", source.BaseTypeName.ToString(), source);
                            }
                            if ((type2.FinalResolved & XmlSchemaDerivationMethod.Restriction) != XmlSchemaDerivationMethod.Empty)
                            {
                                base.SendValidationEvent("Sch_BaseFinalRestriction", simpleType);
                            }
                            simpleType.SetBaseSchemaType(type2);
                            datatype2 = type2.Datatype;
                        }
                        simpleType.SetDatatype(datatype2.DeriveByRestriction(source.Facets, base.NameTable, simpleType));
                        simpleType.SetDerivedBy(XmlSchemaDerivationMethod.Restriction);
                    }
                    else
                    {
                        XmlSchemaSimpleType[] types = this.CompileBaseMemberTypes(simpleType);
                        simpleType.SetBaseSchemaType(DatatypeImplementation.AnySimpleType);
                        simpleType.SetDatatype(XmlSchemaDatatype.DeriveByUnion(types, simpleType));
                        simpleType.SetDerivedBy(XmlSchemaDerivationMethod.Union);
                    }
                }
                catch (XmlSchemaException exception)
                {
                    if (exception.SourceSchemaObject == null)
                    {
                        exception.SetSource(simpleType);
                    }
                    base.SendValidationEvent(exception);
                    simpleType.SetDatatype(DatatypeImplementation.AnySimpleType.Datatype);
                }
                finally
                {
                    SchemaElementDecl decl = new SchemaElementDecl {
                        ContentValidator = ContentValidator.TextOnly,
                        SchemaType = simpleType,
                        Datatype = simpleType.Datatype
                    };
                    simpleType.ElementDecl = decl;
                    simpleType.IsProcessing = false;
                }
            }
        }

        private void CompileSubstitutionGroup(XmlSchemaSubstitutionGroup substitutionGroup)
        {
            if (substitutionGroup.IsProcessing && (substitutionGroup.Members.Count > 0))
            {
                base.SendValidationEvent("Sch_SubstitutionCircularRef", (XmlSchemaElement) substitutionGroup.Members[0]);
            }
            else
            {
                XmlSchemaElement item = (XmlSchemaElement) this.elements[substitutionGroup.Examplar];
                if (!substitutionGroup.Members.Contains(item))
                {
                    substitutionGroup.IsProcessing = true;
                    try
                    {
                        if (item.FinalResolved == XmlSchemaDerivationMethod.All)
                        {
                            base.SendValidationEvent("Sch_InvalidExamplar", item);
                        }
                        ArrayList list = null;
                        for (int i = 0; i < substitutionGroup.Members.Count; i++)
                        {
                            XmlSchemaElement element2 = (XmlSchemaElement) substitutionGroup.Members[i];
                            if ((element2.ElementDecl.Block & XmlSchemaDerivationMethod.Substitution) == XmlSchemaDerivationMethod.Empty)
                            {
                                XmlSchemaSubstitutionGroup group = (XmlSchemaSubstitutionGroup) this.examplars[element2.QualifiedName];
                                if (group != null)
                                {
                                    this.CompileSubstitutionGroup(group);
                                    for (int j = 0; j < group.Members.Count; j++)
                                    {
                                        if (group.Members[j] != element2)
                                        {
                                            if (list == null)
                                            {
                                                list = new ArrayList();
                                            }
                                            list.Add(group.Members[j]);
                                        }
                                    }
                                }
                            }
                        }
                        if (list != null)
                        {
                            for (int k = 0; k < list.Count; k++)
                            {
                                substitutionGroup.Members.Add(list[k]);
                            }
                        }
                        substitutionGroup.Members.Add(item);
                    }
                    finally
                    {
                        substitutionGroup.IsProcessing = false;
                    }
                }
            }
        }

        private void CopyPosition(XmlSchemaAnnotated to, XmlSchemaAnnotated from, bool copyParent)
        {
            to.SourceUri = from.SourceUri;
            to.LinePosition = from.LinePosition;
            to.LineNumber = from.LineNumber;
            to.SetUnhandledAttributes(from.UnhandledAttributes);
            if (copyParent)
            {
                to.Parent = from.Parent;
            }
        }

        public bool Execute(XmlSchemaSet schemaSet, SchemaInfo schemaCompiledInfo)
        {
            this.Compile();
            if (!base.HasErrors)
            {
                this.Output(schemaCompiledInfo);
                schemaSet.elements = this.elements;
                schemaSet.attributes = this.attributes;
                schemaSet.schemaTypes = this.schemaTypes;
                schemaSet.substitutionGroups = this.examplars;
            }
            return !base.HasErrors;
        }

        private XmlSchemaType GetAnySchemaType(XmlQualifiedName name)
        {
            XmlSchemaType type = (XmlSchemaType) this.schemaTypes[name];
            if (type == null)
            {
                return DatatypeImplementation.GetSimpleTypeFromXsdType(name);
            }
            if (type is XmlSchemaComplexType)
            {
                this.CompileComplexType((XmlSchemaComplexType) type);
                return type;
            }
            this.CompileSimpleType((XmlSchemaSimpleType) type);
            return type;
        }

        private XmlSchemaComplexType GetComplexType(XmlQualifiedName name)
        {
            XmlSchemaComplexType complexType = this.schemaTypes[name] as XmlSchemaComplexType;
            if (complexType != null)
            {
                this.CompileComplexType(complexType);
            }
            return complexType;
        }

        private int GetMappingParticle(XmlSchemaParticle particle, XmlSchemaObjectCollection collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (this.IsValidRestriction(particle, (XmlSchemaParticle) collection[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private XmlSchemaContentType GetSchemaContentType(XmlSchemaComplexType complexType, XmlSchemaComplexContent complexContent, XmlSchemaParticle particle)
        {
            if (((complexContent != null) && complexContent.IsMixed) || ((complexContent == null) && complexType.IsMixed))
            {
                return XmlSchemaContentType.Mixed;
            }
            if ((particle != null) && !particle.IsEmpty)
            {
                return XmlSchemaContentType.ElementOnly;
            }
            return XmlSchemaContentType.Empty;
        }

        private XmlSchemaSimpleType GetSimpleType(XmlQualifiedName name)
        {
            XmlSchemaSimpleType simpleType = this.schemaTypes[name] as XmlSchemaSimpleType;
            if (simpleType != null)
            {
                this.CompileSimpleType(simpleType);
                return simpleType;
            }
            return DatatypeImplementation.GetSimpleTypeFromXsdType(name);
        }

        internal void ImportAllCompiledSchemas(XmlSchemaSet schemaSet)
        {
            SortedList sortedSchemas = schemaSet.SortedSchemas;
            for (int i = 0; i < sortedSchemas.Count; i++)
            {
                XmlSchema byIndex = (XmlSchema) sortedSchemas.GetByIndex(i);
                if (byIndex.IsCompiledBySet)
                {
                    this.Prepare(byIndex, false);
                }
            }
        }

        private bool IsAnyFromAny(XmlSchemaAny derivedAny, XmlSchemaAny baseAny)
        {
            if (!this.IsValidOccurrenceRangeRestriction(derivedAny, baseAny))
            {
                this.restrictionErrorMsg = Res.GetString("Sch_AnyFromAnyRule1");
                return false;
            }
            if (!NamespaceList.IsSubset(derivedAny.NamespaceList, baseAny.NamespaceList))
            {
                this.restrictionErrorMsg = Res.GetString("Sch_AnyFromAnyRule2");
                return false;
            }
            if (derivedAny.ProcessContentsCorrect < baseAny.ProcessContentsCorrect)
            {
                this.restrictionErrorMsg = Res.GetString("Sch_AnyFromAnyRule3");
                return false;
            }
            return true;
        }

        private bool IsChoiceFromChoiceSubstGroup(XmlSchemaChoice derivedChoice, XmlSchemaChoice baseChoice)
        {
            if (!this.IsValidOccurrenceRangeRestriction(derivedChoice, baseChoice))
            {
                this.restrictionErrorMsg = Res.GetString("Sch_GroupBaseRestRangeInvalid");
                return false;
            }
            for (int i = 0; i < derivedChoice.Items.Count; i++)
            {
                if (this.GetMappingParticle((XmlSchemaParticle) derivedChoice.Items[i], baseChoice.Items) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsElementFromAny(XmlSchemaElement derivedElement, XmlSchemaAny baseAny)
        {
            if (!baseAny.Allows(derivedElement.QualifiedName))
            {
                this.restrictionErrorMsg = Res.GetString("Sch_ElementFromAnyRule1", new object[] { derivedElement.QualifiedName.ToString() });
                return false;
            }
            if (!this.IsValidOccurrenceRangeRestriction(derivedElement, baseAny))
            {
                this.restrictionErrorMsg = Res.GetString("Sch_ElementFromAnyRule2", new object[] { derivedElement.QualifiedName.ToString() });
                return false;
            }
            return true;
        }

        private bool IsElementFromElement(XmlSchemaElement derivedElement, XmlSchemaElement baseElement)
        {
            XmlSchemaDerivationMethod method = (baseElement.ElementDecl.Block == XmlSchemaDerivationMethod.All) ? (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution) : baseElement.ElementDecl.Block;
            XmlSchemaDerivationMethod method2 = (derivedElement.ElementDecl.Block == XmlSchemaDerivationMethod.All) ? (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution) : derivedElement.ElementDecl.Block;
            if ((((derivedElement.QualifiedName == baseElement.QualifiedName) && (baseElement.IsNillable || !derivedElement.IsNillable)) && (this.IsValidOccurrenceRangeRestriction(derivedElement, baseElement) && ((baseElement.FixedValue == null) || this.IsFixedEqual(baseElement.ElementDecl, derivedElement.ElementDecl)))) && ((((method2 | method) == method2) && (derivedElement.ElementSchemaType != null)) && ((baseElement.ElementSchemaType != null) && XmlSchemaType.IsDerivedFrom(derivedElement.ElementSchemaType, baseElement.ElementSchemaType, ~(XmlSchemaDerivationMethod.Union | XmlSchemaDerivationMethod.List | XmlSchemaDerivationMethod.Restriction)))))
            {
                return true;
            }
            this.restrictionErrorMsg = Res.GetString("Sch_ElementFromElement", new object[] { derivedElement.QualifiedName, baseElement.QualifiedName });
            return false;
        }

        private bool IsElementFromGroupBase(XmlSchemaElement derivedElement, XmlSchemaGroupBase baseGroupBase)
        {
            if (baseGroupBase is XmlSchemaSequence)
            {
                XmlSchemaSequence derivedGroupBase = new XmlSchemaSequence {
                    MinOccurs = 1M,
                    MaxOccurs = 1M
                };
                derivedGroupBase.Items.Add(derivedElement);
                if (this.IsGroupBaseFromGroupBase(derivedGroupBase, baseGroupBase, true))
                {
                    return true;
                }
                this.restrictionErrorMsg = Res.GetString("Sch_ElementFromGroupBase1", new object[] { derivedElement.QualifiedName.ToString(), derivedElement.LineNumber.ToString(NumberFormatInfo.InvariantInfo), derivedElement.LinePosition.ToString(NumberFormatInfo.InvariantInfo), baseGroupBase.LineNumber.ToString(NumberFormatInfo.InvariantInfo), baseGroupBase.LinePosition.ToString(NumberFormatInfo.InvariantInfo) });
            }
            else if (baseGroupBase is XmlSchemaChoice)
            {
                XmlSchemaChoice choice = new XmlSchemaChoice {
                    MinOccurs = 1M,
                    MaxOccurs = 1M
                };
                choice.Items.Add(derivedElement);
                if (this.IsGroupBaseFromGroupBase(choice, baseGroupBase, false))
                {
                    return true;
                }
                this.restrictionErrorMsg = Res.GetString("Sch_ElementFromGroupBase2", new object[] { derivedElement.QualifiedName.ToString(), derivedElement.LineNumber.ToString(NumberFormatInfo.InvariantInfo), derivedElement.LinePosition.ToString(NumberFormatInfo.InvariantInfo), baseGroupBase.LineNumber.ToString(NumberFormatInfo.InvariantInfo), baseGroupBase.LinePosition.ToString(NumberFormatInfo.InvariantInfo) });
            }
            else if (baseGroupBase is XmlSchemaAll)
            {
                XmlSchemaAll all = new XmlSchemaAll {
                    MinOccurs = 1M,
                    MaxOccurs = 1M
                };
                all.Items.Add(derivedElement);
                if (this.IsGroupBaseFromGroupBase(all, baseGroupBase, true))
                {
                    return true;
                }
                this.restrictionErrorMsg = Res.GetString("Sch_ElementFromGroupBase3", new object[] { derivedElement.QualifiedName.ToString(), derivedElement.LineNumber.ToString(NumberFormatInfo.InvariantInfo), derivedElement.LinePosition.ToString(NumberFormatInfo.InvariantInfo), baseGroupBase.LineNumber.ToString(NumberFormatInfo.InvariantInfo), baseGroupBase.LinePosition.ToString(NumberFormatInfo.InvariantInfo) });
            }
            return false;
        }

        private bool IsFixedEqual(SchemaDeclBase baseDecl, SchemaDeclBase derivedDecl)
        {
            if ((baseDecl.Presence == SchemaDeclBase.Use.Fixed) || (baseDecl.Presence == SchemaDeclBase.Use.RequiredFixed))
            {
                object defaultValueTyped = baseDecl.DefaultValueTyped;
                object obj3 = derivedDecl.DefaultValueTyped;
                if ((derivedDecl.Presence != SchemaDeclBase.Use.Fixed) && (derivedDecl.Presence != SchemaDeclBase.Use.RequiredFixed))
                {
                    return false;
                }
                XmlSchemaDatatype datatype = baseDecl.Datatype;
                XmlSchemaDatatype dtype = derivedDecl.Datatype;
                if (datatype.Variety == XmlSchemaDatatypeVariety.Union)
                {
                    if (dtype.Variety != XmlSchemaDatatypeVariety.Union)
                    {
                        XsdSimpleValue value2 = baseDecl.DefaultValueTyped as XsdSimpleValue;
                        if (!value2.XmlType.Datatype.IsComparable(dtype) || !dtype.IsEqual(value2.TypedValue, obj3))
                        {
                            return false;
                        }
                    }
                    else if (!dtype.IsEqual(defaultValueTyped, obj3))
                    {
                        return false;
                    }
                }
                else if (!dtype.IsEqual(defaultValueTyped, obj3))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsGroupBaseFromAny(XmlSchemaGroupBase derivedGroupBase, XmlSchemaAny baseAny)
        {
            decimal num;
            decimal num2;
            this.CalculateEffectiveTotalRange(derivedGroupBase, out num, out num2);
            if (!this.IsValidOccurrenceRangeRestriction(num, num2, baseAny.MinOccurs, baseAny.MaxOccurs))
            {
                this.restrictionErrorMsg = Res.GetString("Sch_GroupBaseFromAny2", new object[] { derivedGroupBase.LineNumber.ToString(NumberFormatInfo.InvariantInfo), derivedGroupBase.LinePosition.ToString(NumberFormatInfo.InvariantInfo), baseAny.LineNumber.ToString(NumberFormatInfo.InvariantInfo), baseAny.LinePosition.ToString(NumberFormatInfo.InvariantInfo) });
                return false;
            }
            string minOccursString = baseAny.MinOccursString;
            baseAny.MinOccurs = 0M;
            for (int i = 0; i < derivedGroupBase.Items.Count; i++)
            {
                if (!this.IsValidRestriction((XmlSchemaParticle) derivedGroupBase.Items[i], baseAny))
                {
                    this.restrictionErrorMsg = Res.GetString("Sch_GroupBaseFromAny1");
                    baseAny.MinOccursString = minOccursString;
                    return false;
                }
            }
            baseAny.MinOccursString = minOccursString;
            return true;
        }

        private bool IsGroupBaseFromGroupBase(XmlSchemaGroupBase derivedGroupBase, XmlSchemaGroupBase baseGroupBase, bool skipEmptableOnly)
        {
            if (!this.IsValidOccurrenceRangeRestriction(derivedGroupBase, baseGroupBase))
            {
                this.restrictionErrorMsg = Res.GetString("Sch_GroupBaseRestRangeInvalid");
                return false;
            }
            if (derivedGroupBase.Items.Count > baseGroupBase.Items.Count)
            {
                this.restrictionErrorMsg = Res.GetString("Sch_GroupBaseRestNoMap");
                return false;
            }
            int num = 0;
            for (int i = 0; i < baseGroupBase.Items.Count; i++)
            {
                XmlSchemaParticle baseParticle = (XmlSchemaParticle) baseGroupBase.Items[i];
                if ((num < derivedGroupBase.Items.Count) && this.IsValidRestriction((XmlSchemaParticle) derivedGroupBase.Items[num], baseParticle))
                {
                    num++;
                }
                else if (skipEmptableOnly && !this.IsParticleEmptiable(baseParticle))
                {
                    if (this.restrictionErrorMsg == null)
                    {
                        this.restrictionErrorMsg = Res.GetString("Sch_GroupBaseRestNotEmptiable");
                    }
                    return false;
                }
            }
            if (num < derivedGroupBase.Items.Count)
            {
                return false;
            }
            return true;
        }

        private bool IsParticleEmptiable(XmlSchemaParticle particle)
        {
            decimal num;
            decimal num2;
            this.CalculateEffectiveTotalRange(particle, out num, out num2);
            return (num == 0M);
        }

        private bool IsProcessContentsRestricted(XmlSchemaComplexType baseType, XmlSchemaAnyAttribute derivedAttributeWildcard, XmlSchemaAnyAttribute baseAttributeWildcard)
        {
            return ((baseType == XmlSchemaComplexType.AnyType) || (derivedAttributeWildcard.ProcessContentsCorrect >= baseAttributeWildcard.ProcessContentsCorrect));
        }

        private bool IsSequenceFromAll(XmlSchemaSequence derivedSequence, XmlSchemaAll baseAll)
        {
            if (!this.IsValidOccurrenceRangeRestriction(derivedSequence, baseAll) || (derivedSequence.Items.Count > baseAll.Items.Count))
            {
                return false;
            }
            BitSet set = new BitSet(baseAll.Items.Count);
            for (int i = 0; i < derivedSequence.Items.Count; i++)
            {
                int mappingParticle = this.GetMappingParticle((XmlSchemaParticle) derivedSequence.Items[i], baseAll.Items);
                if (mappingParticle < 0)
                {
                    return false;
                }
                if (set[mappingParticle])
                {
                    return false;
                }
                set.Set(mappingParticle);
            }
            for (int j = 0; j < baseAll.Items.Count; j++)
            {
                if (!set[j] && !this.IsParticleEmptiable((XmlSchemaParticle) baseAll.Items[j]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsSequenceFromChoice(XmlSchemaSequence derivedSequence, XmlSchemaChoice baseChoice)
        {
            decimal num2;
            decimal minOccurs = derivedSequence.MinOccurs * derivedSequence.Items.Count;
            if (derivedSequence.MaxOccurs == 79228162514264337593543950335M)
            {
                num2 = 79228162514264337593543950335M;
            }
            else
            {
                num2 = derivedSequence.MaxOccurs * derivedSequence.Items.Count;
            }
            if (!this.IsValidOccurrenceRangeRestriction(minOccurs, num2, baseChoice.MinOccurs, baseChoice.MaxOccurs) || (derivedSequence.Items.Count > baseChoice.Items.Count))
            {
                return false;
            }
            for (int i = 0; i < derivedSequence.Items.Count; i++)
            {
                if (this.GetMappingParticle((XmlSchemaParticle) derivedSequence.Items[i], baseChoice.Items) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsValidOccurrenceRangeRestriction(XmlSchemaParticle derivedParticle, XmlSchemaParticle baseParticle)
        {
            return this.IsValidOccurrenceRangeRestriction(derivedParticle.MinOccurs, derivedParticle.MaxOccurs, baseParticle.MinOccurs, baseParticle.MaxOccurs);
        }

        private bool IsValidOccurrenceRangeRestriction(decimal minOccurs, decimal maxOccurs, decimal baseMinOccurs, decimal baseMaxOccurs)
        {
            return ((baseMinOccurs <= minOccurs) && (maxOccurs <= baseMaxOccurs));
        }

        private bool IsValidRestriction(XmlSchemaParticle derivedParticle, XmlSchemaParticle baseParticle)
        {
            if (derivedParticle == baseParticle)
            {
                return true;
            }
            if ((derivedParticle == null) || (derivedParticle == XmlSchemaParticle.Empty))
            {
                return this.IsParticleEmptiable(baseParticle);
            }
            if ((baseParticle != null) && (baseParticle != XmlSchemaParticle.Empty))
            {
                if (derivedParticle is XmlSchemaElement)
                {
                    XmlSchemaElement element = (XmlSchemaElement) derivedParticle;
                    derivedParticle = this.CannonicalizeElement(element);
                }
                if (baseParticle is XmlSchemaElement)
                {
                    XmlSchemaElement element2 = (XmlSchemaElement) baseParticle;
                    XmlSchemaParticle particle = this.CannonicalizeElement(element2);
                    if (particle is XmlSchemaChoice)
                    {
                        return this.IsValidRestriction(derivedParticle, particle);
                    }
                    if (derivedParticle is XmlSchemaElement)
                    {
                        return this.IsElementFromElement((XmlSchemaElement) derivedParticle, element2);
                    }
                    this.restrictionErrorMsg = Res.GetString("Sch_ForbiddenDerivedParticleForElem");
                    return false;
                }
                if (baseParticle is XmlSchemaAny)
                {
                    if (derivedParticle is XmlSchemaElement)
                    {
                        return this.IsElementFromAny((XmlSchemaElement) derivedParticle, (XmlSchemaAny) baseParticle);
                    }
                    if (derivedParticle is XmlSchemaAny)
                    {
                        return this.IsAnyFromAny((XmlSchemaAny) derivedParticle, (XmlSchemaAny) baseParticle);
                    }
                    return this.IsGroupBaseFromAny((XmlSchemaGroupBase) derivedParticle, (XmlSchemaAny) baseParticle);
                }
                if (baseParticle is XmlSchemaAll)
                {
                    if (derivedParticle is XmlSchemaElement)
                    {
                        return this.IsElementFromGroupBase((XmlSchemaElement) derivedParticle, (XmlSchemaGroupBase) baseParticle);
                    }
                    if (derivedParticle is XmlSchemaAll)
                    {
                        if (this.IsGroupBaseFromGroupBase((XmlSchemaGroupBase) derivedParticle, (XmlSchemaGroupBase) baseParticle, true))
                        {
                            return true;
                        }
                    }
                    else if (derivedParticle is XmlSchemaSequence)
                    {
                        if (this.IsSequenceFromAll((XmlSchemaSequence) derivedParticle, (XmlSchemaAll) baseParticle))
                        {
                            return true;
                        }
                        this.restrictionErrorMsg = Res.GetString("Sch_SeqFromAll", new object[] { derivedParticle.LineNumber.ToString(NumberFormatInfo.InvariantInfo), derivedParticle.LinePosition.ToString(NumberFormatInfo.InvariantInfo), baseParticle.LineNumber.ToString(NumberFormatInfo.InvariantInfo), baseParticle.LinePosition.ToString(NumberFormatInfo.InvariantInfo) });
                    }
                    else if ((derivedParticle is XmlSchemaChoice) || (derivedParticle is XmlSchemaAny))
                    {
                        this.restrictionErrorMsg = Res.GetString("Sch_ForbiddenDerivedParticleForAll");
                    }
                    return false;
                }
                if (baseParticle is XmlSchemaChoice)
                {
                    if (derivedParticle is XmlSchemaElement)
                    {
                        return this.IsElementFromGroupBase((XmlSchemaElement) derivedParticle, (XmlSchemaGroupBase) baseParticle);
                    }
                    if (derivedParticle is XmlSchemaChoice)
                    {
                        XmlSchemaChoice baseChoice = baseParticle as XmlSchemaChoice;
                        XmlSchemaChoice derivedChoice = derivedParticle as XmlSchemaChoice;
                        if ((baseChoice.Parent == null) || (derivedChoice.Parent == null))
                        {
                            return this.IsChoiceFromChoiceSubstGroup(derivedChoice, baseChoice);
                        }
                        if (this.IsGroupBaseFromGroupBase(derivedChoice, baseChoice, false))
                        {
                            return true;
                        }
                    }
                    else if (derivedParticle is XmlSchemaSequence)
                    {
                        if (this.IsSequenceFromChoice((XmlSchemaSequence) derivedParticle, (XmlSchemaChoice) baseParticle))
                        {
                            return true;
                        }
                        this.restrictionErrorMsg = Res.GetString("Sch_SeqFromChoice", new object[] { derivedParticle.LineNumber.ToString(NumberFormatInfo.InvariantInfo), derivedParticle.LinePosition.ToString(NumberFormatInfo.InvariantInfo), baseParticle.LineNumber.ToString(NumberFormatInfo.InvariantInfo), baseParticle.LinePosition.ToString(NumberFormatInfo.InvariantInfo) });
                    }
                    else
                    {
                        this.restrictionErrorMsg = Res.GetString("Sch_ForbiddenDerivedParticleForChoice");
                    }
                    return false;
                }
                if (baseParticle is XmlSchemaSequence)
                {
                    if (derivedParticle is XmlSchemaElement)
                    {
                        return this.IsElementFromGroupBase((XmlSchemaElement) derivedParticle, (XmlSchemaGroupBase) baseParticle);
                    }
                    if ((derivedParticle is XmlSchemaSequence) || ((derivedParticle is XmlSchemaAll) && (((XmlSchemaGroupBase) derivedParticle).Items.Count == 1)))
                    {
                        if (this.IsGroupBaseFromGroupBase((XmlSchemaGroupBase) derivedParticle, (XmlSchemaGroupBase) baseParticle, true))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        this.restrictionErrorMsg = Res.GetString("Sch_ForbiddenDerivedParticleForSeq");
                    }
                }
            }
            return false;
        }

        private void Output(SchemaInfo schemaInfo)
        {
            foreach (XmlSchema schema in this.schemasToCompile.Values)
            {
                string targetNamespace = schema.TargetNamespace;
                if (targetNamespace == null)
                {
                    targetNamespace = string.Empty;
                }
                schemaInfo.TargetNamespaces[targetNamespace] = true;
            }
            foreach (XmlSchemaElement element in this.elements.Values)
            {
                schemaInfo.ElementDecls.Add(element.QualifiedName, element.ElementDecl);
            }
            foreach (XmlSchemaAttribute attribute in this.attributes.Values)
            {
                schemaInfo.AttributeDecls.Add(attribute.QualifiedName, attribute.AttDef);
            }
            foreach (XmlSchemaType type in this.schemaTypes.Values)
            {
                schemaInfo.ElementDeclsByType.Add(type.QualifiedName, type.ElementDecl);
            }
            foreach (XmlSchemaNotation notation in this.notations.Values)
            {
                SchemaNotation notation2 = new SchemaNotation(notation.QualifiedName) {
                    SystemLiteral = notation.System,
                    Pubid = notation.Public
                };
                if (!schemaInfo.Notations.ContainsKey(notation2.Name.Name))
                {
                    schemaInfo.Notations.Add(notation2.Name.Name, notation2);
                }
            }
        }

        internal void Prepare(XmlSchema schema, bool cleanup)
        {
            if (this.schemasToCompile[schema] == null)
            {
                this.schemasToCompile.Add(schema, schema);
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    if (cleanup)
                    {
                        this.CleanupElement(element);
                    }
                    base.AddToTable(this.elements, element.QualifiedName, element);
                }
                foreach (XmlSchemaAttribute attribute in schema.Attributes.Values)
                {
                    if (cleanup)
                    {
                        this.CleanupAttribute(attribute);
                    }
                    base.AddToTable(this.attributes, attribute.QualifiedName, attribute);
                }
                foreach (XmlSchemaGroup group in schema.Groups.Values)
                {
                    if (cleanup)
                    {
                        this.CleanupGroup(group);
                    }
                    base.AddToTable(this.groups, group.QualifiedName, group);
                }
                foreach (XmlSchemaAttributeGroup group2 in schema.AttributeGroups.Values)
                {
                    if (cleanup)
                    {
                        this.CleanupAttributeGroup(group2);
                    }
                    base.AddToTable(this.attributeGroups, group2.QualifiedName, group2);
                }
                foreach (XmlSchemaType type in schema.SchemaTypes.Values)
                {
                    if (cleanup)
                    {
                        XmlSchemaComplexType complexType = type as XmlSchemaComplexType;
                        if (complexType != null)
                        {
                            this.CleanupComplexType(complexType);
                        }
                        else
                        {
                            this.CleanupSimpleType(type as XmlSchemaSimpleType);
                        }
                    }
                    base.AddToTable(this.schemaTypes, type.QualifiedName, type);
                }
                foreach (XmlSchemaNotation notation in schema.Notations.Values)
                {
                    base.AddToTable(this.notations, notation.QualifiedName, notation);
                }
                foreach (XmlSchemaIdentityConstraint constraint in schema.IdentityConstraints.Values)
                {
                    base.AddToTable(this.identityConstraints, constraint.QualifiedName, constraint);
                }
            }
        }

        private void ProcessSubstitutionGroups()
        {
            foreach (XmlSchemaElement element in this.elements.Values)
            {
                if (!element.SubstitutionGroup.IsEmpty)
                {
                    XmlSchemaElement element2 = this.elements[element.SubstitutionGroup] as XmlSchemaElement;
                    if (element2 == null)
                    {
                        base.SendValidationEvent("Sch_NoExamplar", element);
                    }
                    else
                    {
                        if (!XmlSchemaType.IsDerivedFrom(element.ElementSchemaType, element2.ElementSchemaType, element2.FinalResolved))
                        {
                            base.SendValidationEvent("Sch_InvalidSubstitutionMember", element.QualifiedName.ToString(), element2.QualifiedName.ToString(), element);
                        }
                        XmlSchemaSubstitutionGroup group = (XmlSchemaSubstitutionGroup) this.examplars[element.SubstitutionGroup];
                        if (group == null)
                        {
                            group = new XmlSchemaSubstitutionGroup {
                                Examplar = element.SubstitutionGroup
                            };
                            this.examplars.Add(element.SubstitutionGroup, group);
                        }
                        ArrayList members = group.Members;
                        if (!members.Contains(element))
                        {
                            members.Add(element);
                        }
                    }
                }
            }
            foreach (XmlSchemaSubstitutionGroup group2 in this.examplars.Values)
            {
                this.CompileSubstitutionGroup(group2);
            }
        }

        private void PushComplexType(XmlSchemaComplexType complexType)
        {
            this.complexTypeStack.Push(complexType);
        }

        private void RecursivelyCheckRedefinedAttributeGroups(XmlSchemaAttributeGroup attributeGroup, XmlSchemaAttributeGroup baseAttributeGroup)
        {
            if (baseAttributeGroup.Redefined != null)
            {
                this.RecursivelyCheckRedefinedAttributeGroups(baseAttributeGroup, baseAttributeGroup.Redefined);
            }
            if (attributeGroup.SelfReferenceCount == 0)
            {
                this.CompileAttributeGroup(baseAttributeGroup);
                this.CompileAttributeGroup(attributeGroup);
                this.CheckAtrributeGroupRestriction(baseAttributeGroup, attributeGroup);
            }
        }

        private void RecursivelyCheckRedefinedGroups(XmlSchemaGroup redefinedGroup, XmlSchemaGroup baseGroup)
        {
            if (baseGroup.Redefined != null)
            {
                this.RecursivelyCheckRedefinedGroups(baseGroup, baseGroup.Redefined);
            }
            if (redefinedGroup.SelfReferenceCount == 0)
            {
                if (baseGroup.CanonicalParticle == null)
                {
                    baseGroup.CanonicalParticle = this.CannonicalizeParticle(baseGroup.Particle, true);
                }
                if (redefinedGroup.CanonicalParticle == null)
                {
                    redefinedGroup.CanonicalParticle = this.CannonicalizeParticle(redefinedGroup.Particle, true);
                }
                this.CompileParticleElements(redefinedGroup.CanonicalParticle);
                this.CompileParticleElements(baseGroup.CanonicalParticle);
                this.CheckParticleDerivation(redefinedGroup.CanonicalParticle, baseGroup.CanonicalParticle);
            }
        }

        private void SetDefaultFixed(XmlSchemaAttribute xa, SchemaAttDef decl)
        {
            if ((xa.DefaultValue != null) || (xa.FixedValue != null))
            {
                if (xa.DefaultValue != null)
                {
                    decl.Presence = SchemaDeclBase.Use.Default;
                    decl.DefaultValueRaw = decl.DefaultValueExpanded = xa.DefaultValue;
                }
                else
                {
                    if (xa.Use == XmlSchemaUse.Required)
                    {
                        decl.Presence = SchemaDeclBase.Use.RequiredFixed;
                    }
                    else
                    {
                        decl.Presence = SchemaDeclBase.Use.Fixed;
                    }
                    decl.DefaultValueRaw = decl.DefaultValueExpanded = xa.FixedValue;
                }
                if (decl.Datatype != null)
                {
                    if (decl.Datatype.TypeCode == XmlTypeCode.Id)
                    {
                        base.SendValidationEvent("Sch_DefaultIdValue", xa);
                    }
                    else
                    {
                        decl.DefaultValueTyped = decl.Datatype.ParseValue(decl.DefaultValueRaw, base.NameTable, new SchemaNamespaceManager(xa), true);
                    }
                }
            }
            else
            {
                switch (xa.Use)
                {
                    case XmlSchemaUse.None:
                    case XmlSchemaUse.Optional:
                        decl.Presence = SchemaDeclBase.Use.Implied;
                        return;

                    case XmlSchemaUse.Prohibited:
                        return;

                    case XmlSchemaUse.Required:
                        decl.Presence = SchemaDeclBase.Use.Required;
                        return;
                }
            }
        }

        private void UpdateSForSSimpleTypes()
        {
            XmlSchemaSimpleType[] builtInTypes = DatatypeImplementation.GetBuiltInTypes();
            int num = builtInTypes.Length - 3;
            for (int i = 12; i < num; i++)
            {
                XmlSchemaSimpleType type = builtInTypes[i];
                this.schemaForSchema.SchemaTypes.Replace(type.QualifiedName, type);
                this.schemaTypes.Replace(type.QualifiedName, type);
            }
        }
    }
}

