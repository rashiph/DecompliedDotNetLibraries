namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Web;

    internal class TagPrefixTagNameToTypeMapper : ITagNameToTypeMapper
    {
        private ArrayList _mappers;
        private string _tagPrefix;

        internal TagPrefixTagNameToTypeMapper(string tagPrefix)
        {
            this._tagPrefix = tagPrefix;
            this._mappers = new ArrayList();
        }

        internal void AddNamespaceMapper(NamespaceTagNameToTypeMapper mapper)
        {
            this._mappers.Add(mapper);
        }

        Type ITagNameToTypeMapper.GetControlType(string tagName, IDictionary attribs)
        {
            Type type = null;
            Exception innerException = null;
            foreach (NamespaceTagNameToTypeMapper mapper in this._mappers)
            {
                Type controlType = ((ITagNameToTypeMapper) mapper).GetControlType(tagName, attribs);
                if (controlType != null)
                {
                    if (type == null)
                    {
                        type = controlType;
                    }
                    else if (type != controlType)
                    {
                        throw new HttpParseException(System.Web.SR.GetString("Ambiguous_server_tag", new object[] { this._tagPrefix + ":" + tagName }), null, mapper.RegisterEntry.VirtualPath, null, mapper.RegisterEntry.Line);
                    }
                }
            }
            if (type == null)
            {
                try
                {
                    foreach (NamespaceTagNameToTypeMapper mapper2 in this._mappers)
                    {
                        mapper2.GetControlType(tagName, attribs, true);
                    }
                }
                catch (FileNotFoundException exception2)
                {
                    innerException = exception2;
                }
                catch (FileLoadException exception3)
                {
                    innerException = exception3;
                }
                catch (BadImageFormatException exception4)
                {
                    innerException = exception4;
                }
            }
            if (innerException != null)
            {
                throw new HttpException(System.Web.SR.GetString("ControlAdapters_TypeNotFound", new object[] { this._tagPrefix + ":" + tagName }) + " " + innerException.Message, innerException);
            }
            if (type == null)
            {
                throw new HttpException(System.Web.SR.GetString("Unknown_server_tag", new object[] { this._tagPrefix + ":" + tagName }));
            }
            return type;
        }
    }
}

