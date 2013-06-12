namespace System
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal class ConfigTreeParser : BaseConfigHandler
    {
        private int attributeEntry;
        private bool bNoSearchPath;
        private ConfigNode currentNode;
        private int depth;
        private string fileName;
        private string key;
        private string lastProcessed;
        private bool parsing;
        private int pathDepth;
        private ConfigNode rootNode;
        private int searchDepth;
        private string[] treeRootPath;

        public override void BeginChildren(int size, ConfigNodeSubType subType, ConfigNodeType nType, int terminal, [MarshalAs(UnmanagedType.LPWStr)] string text, int textLength, int prefixLength)
        {
            if ((!this.parsing && !this.bNoSearchPath) && ((this.depth == (this.searchDepth + 1)) && (string.Compare(text, this.treeRootPath[this.searchDepth], StringComparison.Ordinal) == 0)))
            {
                this.searchDepth++;
            }
        }

        public override void CreateAttribute(int size, ConfigNodeSubType subType, ConfigNodeType nType, int terminal, [MarshalAs(UnmanagedType.LPWStr)] string text, int textLength, int prefixLength)
        {
            if (this.parsing)
            {
                if (nType == ConfigNodeType.Attribute)
                {
                    this.attributeEntry = this.currentNode.AddAttribute(text, "");
                    this.key = text;
                }
                else
                {
                    if (nType != ConfigNodeType.PCData)
                    {
                        throw new ApplicationException(Environment.GetResourceString("XML_Syntax_InvalidSyntaxInFile", new object[] { this.fileName, this.lastProcessed }));
                    }
                    this.currentNode.ReplaceAttribute(this.attributeEntry, this.key, text);
                }
            }
        }

        public override void CreateNode(int size, ConfigNodeSubType subType, ConfigNodeType nType, int terminal, [MarshalAs(UnmanagedType.LPWStr)] string text, int textLength, int prefixLength)
        {
            if (nType == ConfigNodeType.Element)
            {
                this.lastProcessed = "<" + text + ">";
                if ((this.parsing || (this.bNoSearchPath && (string.Compare(text, this.treeRootPath[0], StringComparison.OrdinalIgnoreCase) == 0))) || (((this.depth == this.searchDepth) && (this.searchDepth == this.pathDepth)) && (string.Compare(text, this.treeRootPath[this.pathDepth], StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    this.parsing = true;
                    ConfigNode currentNode = this.currentNode;
                    this.currentNode = new ConfigNode(text, currentNode);
                    if (this.rootNode == null)
                    {
                        this.rootNode = this.currentNode;
                    }
                    else
                    {
                        currentNode.AddChild(this.currentNode);
                    }
                }
                else
                {
                    this.depth++;
                }
            }
            else if ((nType == ConfigNodeType.PCData) && (this.currentNode != null))
            {
                this.currentNode.Value = text;
            }
        }

        public override void EndChildren(int fEmpty, int size, ConfigNodeSubType subType, ConfigNodeType nType, int terminal, [MarshalAs(UnmanagedType.LPWStr)] string text, int textLength, int prefixLength)
        {
            this.lastProcessed = "</" + text + ">";
            if (this.parsing)
            {
                if (this.currentNode == this.rootNode)
                {
                    this.parsing = false;
                }
                this.currentNode = this.currentNode.Parent;
            }
            else if (nType == ConfigNodeType.Element)
            {
                if ((this.depth == this.searchDepth) && (string.Compare(text, this.treeRootPath[this.searchDepth - 1], StringComparison.Ordinal) == 0))
                {
                    this.searchDepth--;
                    this.depth--;
                }
                else
                {
                    this.depth--;
                }
            }
        }

        public override void Error(int size, ConfigNodeSubType subType, ConfigNodeType nType, int terminal, [MarshalAs(UnmanagedType.LPWStr)] string text, int textLength, int prefixLength)
        {
        }

        public override void NotifyEvent(ConfigEvents nEvent)
        {
        }

        internal ConfigNode Parse(string fileName, string configPath)
        {
            return this.Parse(fileName, configPath, false);
        }

        [SecuritySafeCritical]
        internal ConfigNode Parse(string fileName, string configPath, bool skipSecurityStuff)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            this.fileName = fileName;
            if (configPath[0] == '/')
            {
                this.treeRootPath = configPath.Substring(1).Split(new char[] { '/' });
                this.pathDepth = this.treeRootPath.Length - 1;
                this.bNoSearchPath = false;
            }
            else
            {
                this.treeRootPath = new string[] { configPath };
                this.bNoSearchPath = true;
            }
            if (!skipSecurityStuff)
            {
                new FileIOPermission(FileIOPermissionAccess.Read, Path.GetFullPathInternal(fileName)).Demand();
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            try
            {
                ConfigServer.RunParser(this, fileName);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (DirectoryNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (FileLoadException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ApplicationException(Environment.GetResourceString("XML_Syntax_InvalidSyntaxInFile", new object[] { fileName, this.lastProcessed }), exception);
            }
            return this.rootNode;
        }

        [Conditional("_LOGGING")]
        private void Trace(string name, int size, ConfigNodeSubType subType, ConfigNodeType nType, int terminal, [MarshalAs(UnmanagedType.LPWStr)] string text, int textLength, int prefixLength, int fEmpty)
        {
        }
    }
}

