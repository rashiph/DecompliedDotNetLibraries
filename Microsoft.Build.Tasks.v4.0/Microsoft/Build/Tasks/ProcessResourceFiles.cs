namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using System.Xml;

    internal sealed class ProcessResourceFiles : MarshalByRefObject
    {
        private ITaskItem[] assemblyFiles;
        private AssemblyNameExtension[] assemblyNames;
        private ResolveEventHandler eventHandler;
        private List<ITaskItem> inFiles;
        private TaskLoggingHelper logger;
        private List<ITaskItem> outFiles;
        private ArrayList resources = new ArrayList();
        private Hashtable resourcesHashTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private bool stronglyTypedClassIsPublic;
        private string stronglyTypedClassName;
        private string stronglyTypedFilename;
        private string stronglyTypedLanguage;
        private string stronglyTypedNamespace;
        private string stronglyTypedResourcesNamespace;
        private bool stronglyTypedResourceSuccessfullyCreated;
        private Microsoft.Build.Tasks.AssemblyNamesTypeResolutionService typeResolver;
        private ArrayList unsuccessfullyCreatedOutFiles;
        private bool useSourcePath;

        private void AddResource(string name, object value, string inputFileName)
        {
            Entry entry = new Entry(name, value);
            if (this.resourcesHashTable.ContainsKey(name))
            {
                this.logger.LogWarningWithCodeFromResources(null, inputFileName, 0, 0, 0, 0, "GenerateResource.DuplicateResourceName", new object[] { name });
            }
            else
            {
                this.resources.Add(entry);
                this.resourcesHashTable.Add(name, value);
            }
        }

        private void AddResource(string name, object value, string inputFileName, int lineNumber, int linePosition)
        {
            Entry entry = new Entry(name, value);
            if (this.resourcesHashTable.ContainsKey(name))
            {
                this.logger.LogWarningWithCodeFromResources(null, inputFileName, lineNumber, linePosition, 0, 0, "GenerateResource.DuplicateResourceName", new object[] { name });
            }
            else
            {
                this.resources.Add(entry);
                this.resourcesHashTable.Add(name, value);
            }
        }

        private void CreateStronglyTypedResources(string outFile, string inputFileName)
        {
            CodeDomProvider provider = null;
            if (TryCreateCodeDomProvider(this.logger, this.stronglyTypedLanguage, out provider))
            {
                string[] strArray;
                if (this.stronglyTypedClassName == null)
                {
                    this.stronglyTypedClassName = Path.GetFileNameWithoutExtension(outFile);
                }
                if (this.stronglyTypedFilename == null)
                {
                    this.stronglyTypedFilename = GenerateDefaultStronglyTypedFilename(provider, outFile);
                }
                this.logger.LogMessageFromResources("GenerateResource.CreatingSTR", new object[] { this.stronglyTypedFilename });
                bool internalClass = !this.stronglyTypedClassIsPublic;
                CodeCompileUnit compileUnit = StronglyTypedResourceBuilder.Create(this.resourcesHashTable, this.stronglyTypedClassName, this.stronglyTypedNamespace, this.stronglyTypedResourcesNamespace, provider, internalClass, out strArray);
                CodeGeneratorOptions options = new CodeGeneratorOptions();
                using (TextWriter writer = new StreamWriter(this.stronglyTypedFilename))
                {
                    provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);
                }
                if (strArray.Length > 0)
                {
                    this.logger.LogErrorWithCodeFromResources("GenerateResource.ErrorFromCodeDom", new object[] { inputFileName });
                    foreach (string str in strArray)
                    {
                        this.logger.LogErrorWithCodeFromResources("GenerateResource.CodeDomError", new object[] { str });
                    }
                }
                else
                {
                    this.stronglyTypedResourceSuccessfullyCreated = true;
                }
            }
        }

        public static string GenerateDefaultStronglyTypedFilename(CodeDomProvider provider, string outputResourcesFile)
        {
            return Path.ChangeExtension(outputResourcesFile, provider.FileExtension);
        }

        private Format GetFormat(string filename)
        {
            string strA = string.Empty;
            try
            {
                strA = Path.GetExtension(filename);
            }
            catch (ArgumentException exception)
            {
                this.logger.LogErrorWithCodeFromResources("GenerateResource.InvalidFilename", new object[] { filename, exception.Message });
                return Format.Error;
            }
            if ((string.Compare(strA, ".txt", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, ".restext", StringComparison.OrdinalIgnoreCase) == 0))
            {
                return Format.Text;
            }
            if (string.Compare(strA, ".resx", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return Format.XML;
            }
            if (string.Compare(strA, ".resources", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return Format.Binary;
            }
            this.logger.LogErrorWithCodeFromResources("GenerateResource.UnknownFileExtension", new object[] { Path.GetExtension(filename), filename });
            return Format.Error;
        }

        private bool ProcessFile(string inFile, string outFile)
        {
            if ((this.GetFormat(inFile) == Format.Error) || (this.GetFormat(outFile) == Format.Error))
            {
                return false;
            }
            this.logger.LogMessageFromResources("GenerateResource.ProcessingFile", new object[] { inFile, outFile });
            this.resources.Clear();
            this.resourcesHashTable.Clear();
            try
            {
                this.ReadResources(inFile, this.useSourcePath);
            }
            catch (ArgumentException exception)
            {
                if (exception.InnerException is XmlException)
                {
                    XmlException innerException = (XmlException) exception.InnerException;
                    this.logger.LogErrorWithCodeFromResources(null, Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(inFile), innerException.LineNumber, innerException.LinePosition, 0, 0, "General.InvalidResxFile", new object[] { innerException.Message });
                }
                else
                {
                    this.logger.LogErrorWithCodeFromResources(null, Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(inFile), 0, 0, 0, 0, "General.InvalidResxFile", new object[] { exception.Message });
                }
                return false;
            }
            catch (TextFileException exception3)
            {
                this.logger.LogErrorWithCodeFromResources(null, exception3.FileName, exception3.LineNumber, exception3.LinePosition, 1, 1, "GenerateResource.MessageTunnel", new object[] { exception3.Message });
                return false;
            }
            catch (XmlException exception4)
            {
                this.logger.LogErrorWithCodeFromResources(null, Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(inFile), exception4.LineNumber, exception4.LinePosition, 0, 0, "General.InvalidResxFile", new object[] { exception4.Message });
                return false;
            }
            catch (Exception exception5)
            {
                if ((exception5 is SerializationException) || (exception5 is TargetInvocationException))
                {
                    this.logger.LogErrorWithCodeFromResources(null, Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(inFile), 0, 0, 0, 0, "General.InvalidResxFile", new object[] { exception5.Message });
                    this.logger.LogErrorFromException(exception5, true, true, Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(inFile));
                    return false;
                }
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception5))
                {
                    throw;
                }
                this.logger.LogErrorWithCodeFromResources(null, Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(inFile), 0, 0, 0, 0, "General.InvalidResxFile", new object[] { exception5.Message });
                return false;
            }
            try
            {
                this.WriteResources(outFile);
                if (this.stronglyTypedLanguage != null)
                {
                    try
                    {
                        this.CreateStronglyTypedResources(outFile, inFile);
                    }
                    catch (Exception exception6)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception6))
                        {
                            throw;
                        }
                        this.logger.LogErrorWithCodeFromResources("GenerateResource.CannotWriteSTRFile", new object[] { this.stronglyTypedFilename, exception6.Message });
                    }
                }
            }
            catch (IOException exception7)
            {
                this.logger.LogErrorWithCodeFromResources("GenerateResource.CannotWriteOutput", new object[] { Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(outFile), exception7.Message });
                if (File.Exists(outFile))
                {
                    this.logger.LogErrorWithCodeFromResources("GenerateResource.CorruptOutput", new object[] { Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(outFile) });
                    try
                    {
                        File.Delete(outFile);
                    }
                    catch (Exception exception8)
                    {
                        this.logger.LogWarningWithCodeFromResources("GenerateResource.DeleteCorruptOutputFailed", new object[] { Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(outFile), exception8.Message });
                    }
                }
                return false;
            }
            catch (Exception exception9)
            {
                if ((exception9 is SerializationException) || (exception9 is TargetInvocationException))
                {
                    this.logger.LogErrorWithCodeFromResources("GenerateResource.CannotWriteOutput", new object[] { Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(inFile), exception9.Message });
                    this.logger.LogErrorFromException(exception9, true, true, Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(inFile));
                    return false;
                }
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception9))
                {
                    throw;
                }
                this.logger.LogErrorWithCodeFromResources("GenerateResource.CannotWriteOutput", new object[] { Microsoft.Build.Shared.FileUtilities.GetFullPathNoThrow(outFile), exception9.Message });
                return false;
            }
            return true;
        }

        private void ReadResources(IResourceReader reader, string fileName)
        {
            using (reader)
            {
                IDictionaryEnumerator enumerator = reader.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = (string) enumerator.Key;
                    object obj2 = enumerator.Value;
                    this.AddResource(key, obj2, fileName);
                }
            }
        }

        private void ReadResources(string filename, bool shouldUseSourcePath)
        {
            ResXResourceReader reader;
            switch (this.GetFormat(filename))
            {
                case Format.Text:
                    this.ReadTextResources(filename);
                    goto Label_0073;

                case Format.XML:
                    reader = null;
                    if (this.typeResolver == null)
                    {
                        reader = new ResXResourceReader(filename);
                        break;
                    }
                    reader = new ResXResourceReader(filename, this.typeResolver);
                    break;

                case Format.Binary:
                    this.ReadResources(new ResourceReader(filename), filename);
                    goto Label_0073;

                default:
                    return;
            }
            if (shouldUseSourcePath)
            {
                string fullPath = Path.GetFullPath(filename);
                reader.BasePath = Path.GetDirectoryName(fullPath);
            }
            this.ReadResources(reader, filename);
        Label_0073:;
            this.logger.LogMessageFromResources(MessageImportance.Low, "GenerateResource.ReadResourceMessage", new object[] { this.resources.Count, filename });
        }

        private void ReadTextResources(string fileName)
        {
            using (LineNumberStreamReader reader = new LineNumberStreamReader(fileName, new UTF8Encoding(true), true))
            {
                StringBuilder builder = new StringBuilder(0xff);
                StringBuilder builder2 = new StringBuilder(0x800);
                int num = reader.Read();
                while (num != -1)
                {
                    switch (num)
                    {
                        case 10:
                        case 13:
                        {
                            num = reader.Read();
                            continue;
                        }
                        case 0x23:
                        case 9:
                        case 0x20:
                        case 0x3b:
                        {
                            reader.ReadLine();
                            num = reader.Read();
                            continue;
                        }
                        case 0x5b:
                        {
                            string str = reader.ReadLine();
                            if (!str.Equals("strings]"))
                            {
                                throw new TextFileException(this.logger.FormatResourceString("GenerateResource.UnexpectedInfBracket", new object[] { "[" + str }), fileName, reader.LineNumber - 1, 1);
                            }
                            this.logger.LogWarningWithCodeFromResources(null, fileName, reader.LineNumber - 1, 1, 0, 0, "GenerateResource.ObsoleteStringsTag", new object[0]);
                            num = reader.Read();
                            continue;
                        }
                        default:
                            builder.Length = 0;
                            while (num != 0x3d)
                            {
                                if ((num == 13) || (num == 10))
                                {
                                    throw new TextFileException(this.logger.FormatResourceString("GenerateResource.NoEqualsInLine", new object[] { builder }), fileName, reader.LineNumber, reader.LinePosition);
                                }
                                builder.Append((char) num);
                                num = reader.Read();
                                if (num == -1)
                                {
                                    break;
                                }
                            }
                            break;
                    }
                    if (builder.Length == 0)
                    {
                        throw new TextFileException(this.logger.FormatResourceString("GenerateResource.NoNameInLine", new object[0]), fileName, reader.LineNumber, reader.LinePosition);
                    }
                    if (builder[builder.Length - 1] == ' ')
                    {
                        builder.Length--;
                    }
                    num = reader.Read();
                    if (num == 0x20)
                    {
                        num = reader.Read();
                    }
                    builder2.Length = 0;
                    while (num != -1)
                    {
                        char[] chArray;
                        int num2;
                        int num3;
                        int num4;
                        bool flag = false;
                        if (num != 0x5c)
                        {
                            goto Label_03AA;
                        }
                        num = reader.Read();
                        switch (num)
                        {
                            case 0x72:
                                num = 13;
                                flag = true;
                                goto Label_03AA;

                            case 0x74:
                                num = 9;
                                goto Label_03AA;

                            case 0x75:
                                chArray = new char[4];
                                num2 = 4;
                                num3 = 0;
                                goto Label_02B0;

                            case 110:
                                num = 10;
                                flag = true;
                                goto Label_03AA;

                            case 0x22:
                                num = 0x22;
                                goto Label_03AA;

                            case 0x5c:
                                goto Label_03AA;

                            default:
                                throw new TextFileException(this.logger.FormatResourceString("GenerateResource.InvalidEscape", new object[] { builder.ToString(), (char) num }), fileName, reader.LineNumber, reader.LinePosition);
                        }
                    Label_024E:
                        num4 = reader.Read(chArray, num3, num2);
                        if (num4 == 0)
                        {
                            throw new TextFileException(this.logger.FormatResourceString("GenerateResource.InvalidEscape", new object[] { builder.ToString(), (char) num }), fileName, reader.LineNumber, reader.LinePosition);
                        }
                        num3 += num4;
                        num2 -= num4;
                    Label_02B0:
                        if (num2 > 0)
                        {
                            goto Label_024E;
                        }
                        try
                        {
                            num = ushort.Parse(new string(chArray), NumberStyles.HexNumber, CultureInfo.CurrentCulture);
                        }
                        catch (FormatException)
                        {
                            throw new TextFileException(this.logger.FormatResourceString("GenerateResource.InvalidHexEscapeValue", new object[] { builder.ToString(), new string(chArray) }), fileName, reader.LineNumber, reader.LinePosition);
                        }
                        catch (OverflowException)
                        {
                            throw new TextFileException(this.logger.FormatResourceString("GenerateResource.InvalidHexEscapeValue", new object[] { builder.ToString(), new string(chArray) }), fileName, reader.LineNumber, reader.LinePosition);
                        }
                        flag = (num == 10) || (num == 13);
                    Label_03AA:
                        if (!flag)
                        {
                            if (num == 13)
                            {
                                num = reader.Read();
                                if (num != -1)
                                {
                                    if (num != 10)
                                    {
                                        goto Label_03DA;
                                    }
                                    num = reader.Read();
                                }
                                break;
                            }
                            if (num == 10)
                            {
                                num = reader.Read();
                                break;
                            }
                        }
                    Label_03DA:
                        builder2.Append((char) num);
                        num = reader.Read();
                    }
                    this.AddResource(builder.ToString(), builder2.ToString(), fileName, reader.LineNumber, reader.LinePosition);
                }
            }
        }

        internal Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyNameExtension that = new AssemblyNameExtension(args.Name);
            if (this.assemblyFiles != null)
            {
                if (this.assemblyNames == null)
                {
                    this.assemblyNames = new AssemblyNameExtension[this.assemblyFiles.Length];
                    for (int k = 0; k < this.assemblyFiles.Length; k++)
                    {
                        ITaskItem item = this.assemblyFiles[k];
                        this.assemblyNames[k] = null;
                        if ((item.ItemSpec != null) && File.Exists(item.ItemSpec))
                        {
                            string metadata = item.GetMetadata("FusionName");
                            if ((metadata != null) && (metadata.Length > 0))
                            {
                                this.assemblyNames[k] = new AssemblyNameExtension(metadata);
                            }
                        }
                    }
                }
                for (int i = 0; i < this.assemblyNames.Length; i++)
                {
                    AssemblyNameExtension extension2 = this.assemblyNames[i];
                    if ((extension2 != null) && (extension2.CompareTo(that) == 0))
                    {
                        return Assembly.UnsafeLoadFrom(this.assemblyFiles[i].ItemSpec);
                    }
                }
                for (int j = 0; j < this.assemblyNames.Length; j++)
                {
                    AssemblyNameExtension extension3 = this.assemblyNames[j];
                    if ((extension3 != null) && (string.Compare(that.Name, extension3.Name, StringComparison.CurrentCultureIgnoreCase) == 0))
                    {
                        return Assembly.UnsafeLoadFrom(this.assemblyFiles[j].ItemSpec);
                    }
                }
            }
            return null;
        }

        internal void Run(TaskLoggingHelper log, ITaskItem[] assemblyFilesList, List<ITaskItem> inputs, List<ITaskItem> outputs, bool sourcePath, string language, string namespacename, string resourcesNamespace, string filename, string classname, bool publicClass)
        {
            this.logger = log;
            this.assemblyFiles = assemblyFilesList;
            this.inFiles = inputs;
            this.outFiles = outputs;
            this.useSourcePath = sourcePath;
            this.stronglyTypedLanguage = language;
            this.stronglyTypedNamespace = namespacename;
            this.stronglyTypedResourcesNamespace = resourcesNamespace;
            this.stronglyTypedFilename = filename;
            this.stronglyTypedClassName = classname;
            this.stronglyTypedClassIsPublic = publicClass;
            if ((this.assemblyFiles != null) && (this.assemblyFiles.Length > 0))
            {
                this.typeResolver = new Microsoft.Build.Tasks.AssemblyNamesTypeResolutionService(this.assemblyFiles);
            }
            try
            {
                this.eventHandler = new ResolveEventHandler(this.ResolveAssembly);
                AppDomain.CurrentDomain.AssemblyResolve += this.eventHandler;
                for (int i = 0; i < this.inFiles.Count; i++)
                {
                    if (!this.ProcessFile(this.inFiles[i].ItemSpec, this.outFiles[i].ItemSpec))
                    {
                        this.UnsuccessfullyCreatedOutFiles.Add(this.outFiles[i].ItemSpec);
                    }
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.eventHandler;
                this.eventHandler = null;
            }
        }

        public static bool TryCreateCodeDomProvider(TaskLoggingHelper logger, string stronglyTypedLanguage, out CodeDomProvider provider)
        {
            provider = null;
            try
            {
                provider = CodeDomProvider.CreateProvider(stronglyTypedLanguage);
            }
            catch (ConfigurationException exception)
            {
                logger.LogErrorWithCodeFromResources("GenerateResource.STRCodeDomProviderFailed", new object[] { stronglyTypedLanguage, exception.Message });
                return false;
            }
            catch (SecurityException exception2)
            {
                logger.LogErrorWithCodeFromResources("GenerateResource.STRCodeDomProviderFailed", new object[] { stronglyTypedLanguage, exception2.Message });
                return false;
            }
            return (provider != null);
        }

        private void WriteResources(IResourceWriter writer)
        {
            try
            {
                foreach (Entry entry in this.resources)
                {
                    string name = entry.name;
                    object obj2 = entry.value;
                    writer.AddResource(name, obj2);
                }
            }
            finally
            {
                writer.Close();
            }
        }

        private void WriteResources(string filename)
        {
            switch (this.GetFormat(filename))
            {
                case Format.Text:
                    this.WriteTextResources(filename);
                    return;

                case Format.XML:
                    this.WriteResources(new ResXResourceWriter(filename));
                    return;

                case Format.Binary:
                    this.WriteResources(new ResourceWriter(filename));
                    return;
            }
        }

        private void WriteTextResources(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                foreach (Entry entry in this.resources)
                {
                    string name = entry.name;
                    object obj2 = entry.value;
                    string str2 = obj2 as string;
                    if (str2 == null)
                    {
                        this.logger.LogErrorWithCodeFromResources(null, fileName, 0, 0, 0, 0, "GenerateResource.OnlyStringsSupported", new object[] { name, obj2.GetType().FullName });
                    }
                    else
                    {
                        str2 = str2.Replace(@"\", @"\\").Replace("\n", @"\n").Replace("\r", @"\r").Replace("\t", @"\t");
                        writer.WriteLine("{0}={1}", name, str2);
                    }
                }
            }
        }

        internal string StronglyTypedClassName
        {
            get
            {
                return this.stronglyTypedClassName;
            }
        }

        internal string StronglyTypedFilename
        {
            get
            {
                return this.stronglyTypedFilename;
            }
        }

        internal bool StronglyTypedResourceSuccessfullyCreated
        {
            get
            {
                return this.stronglyTypedResourceSuccessfullyCreated;
            }
        }

        internal ArrayList UnsuccessfullyCreatedOutFiles
        {
            get
            {
                if (this.unsuccessfullyCreatedOutFiles == null)
                {
                    this.unsuccessfullyCreatedOutFiles = new ArrayList();
                }
                return this.unsuccessfullyCreatedOutFiles;
            }
        }

        private class Entry
        {
            public string name;
            public object value;

            public Entry(string name, object value)
            {
                this.name = name;
                this.value = value;
            }
        }

        private enum Format
        {
            Text,
            XML,
            Binary,
            Error
        }

        internal sealed class LineNumberStreamReader : StreamReader
        {
            private int _col;
            private int _lineNumber;

            internal LineNumberStreamReader(Stream stream) : base(stream)
            {
                this._lineNumber = 1;
                this._col = 0;
            }

            internal LineNumberStreamReader(string fileName, Encoding encoding, bool detectEncoding) : base(fileName, encoding, detectEncoding)
            {
                this._lineNumber = 1;
                this._col = 0;
            }

            public override int Read()
            {
                int num = base.Read();
                if (num != -1)
                {
                    this._col++;
                    if (num == 10)
                    {
                        this._lineNumber++;
                        this._col = 0;
                    }
                }
                return num;
            }

            public override int Read([In, Out] char[] chars, int index, int count)
            {
                int num = base.Read(chars, index, count);
                for (int i = 0; i < num; i++)
                {
                    if (chars[i + index] == '\n')
                    {
                        this._lineNumber++;
                        this._col = 0;
                    }
                    else
                    {
                        this._col++;
                    }
                }
                return num;
            }

            public override string ReadLine()
            {
                string str = base.ReadLine();
                if (str != null)
                {
                    this._lineNumber++;
                    this._col = 0;
                }
                return str;
            }

            public override string ReadToEnd()
            {
                throw new NotImplementedException("NYI");
            }

            internal int LineNumber
            {
                get
                {
                    return this._lineNumber;
                }
            }

            internal int LinePosition
            {
                get
                {
                    return this._col;
                }
            }
        }

        [Serializable]
        internal sealed class TextFileException : Exception
        {
            private int _column;
            private string _fileName;
            private int _lineNumber;

            private TextFileException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }

            internal TextFileException(string message, string fileName, int lineNumber, int linePosition) : base(message)
            {
                this._fileName = fileName;
                this._lineNumber = lineNumber;
                this._column = linePosition;
            }

            internal string FileName
            {
                get
                {
                    return this._fileName;
                }
            }

            internal int LineNumber
            {
                get
                {
                    return this._lineNumber;
                }
            }

            internal int LinePosition
            {
                get
                {
                    return this._column;
                }
            }
        }
    }
}

