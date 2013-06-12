namespace System.Xml.Schema
{
    internal class Datatype_negativeInteger : Datatype_nonPositiveInteger
    {
        private static readonly System.Xml.Schema.FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(-79228162514264337593543950335M, -1M);

        internal override System.Xml.Schema.FacetsChecker FacetsChecker
        {
            get
            {
                return numeric10FacetsChecker;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.NegativeInteger;
            }
        }
    }
}

