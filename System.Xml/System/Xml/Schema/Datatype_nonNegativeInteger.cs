﻿namespace System.Xml.Schema
{
    using System;

    internal class Datatype_nonNegativeInteger : Datatype_integer
    {
        private static readonly System.Xml.Schema.FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(0M, 79228162514264337593543950335M);

        internal override System.Xml.Schema.FacetsChecker FacetsChecker
        {
            get
            {
                return numeric10FacetsChecker;
            }
        }

        internal override bool HasValueFacets
        {
            get
            {
                return true;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.NonNegativeInteger;
            }
        }
    }
}

