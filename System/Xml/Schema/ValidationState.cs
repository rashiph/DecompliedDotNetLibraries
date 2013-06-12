namespace System.Xml.Schema
{
    using System;
    using System.Collections.Generic;

    internal sealed class ValidationState
    {
        public BitSet AllElementsSet;
        public bool CheckRequiredAttribute;
        public ConstraintStruct[] Constr;
        public BitSet[] CurPos = new BitSet[2];
        public StateUnion CurrentState;
        public int Depth;
        public SchemaElementDecl ElementDecl;
        public SchemaElementDecl ElementDeclBeforeXsi;
        public bool HasMatched;
        public bool IsDefault;
        public bool IsNill;
        public string LocalName;
        public string Namespace;
        public bool NeedValidateChildren;
        public XmlSchemaContentProcessing ProcessContents;
        public List<RangePositionInfo> RunningPositions;
        public bool TooComplex;
        public bool ValidationSkipped;
        public XmlSchemaValidity Validity;
    }
}

