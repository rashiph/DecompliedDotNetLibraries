namespace System.Windows.Forms
{
    using System;
    using System.Collections.Generic;

    internal sealed class HtmlShimManager : IDisposable
    {
        private Dictionary<HtmlDocument, HtmlDocument.HtmlDocumentShim> htmlDocumentShims;
        private Dictionary<HtmlElement, HtmlElement.HtmlElementShim> htmlElementShims;
        private Dictionary<HtmlWindow, HtmlWindow.HtmlWindowShim> htmlWindowShims;

        internal HtmlShimManager()
        {
        }

        public void AddDocumentShim(HtmlDocument doc)
        {
            HtmlDocument.HtmlDocumentShim addedShim = null;
            if (this.htmlDocumentShims == null)
            {
                this.htmlDocumentShims = new Dictionary<HtmlDocument, HtmlDocument.HtmlDocumentShim>();
                addedShim = new HtmlDocument.HtmlDocumentShim(doc);
                this.htmlDocumentShims[doc] = addedShim;
            }
            else if (!this.htmlDocumentShims.ContainsKey(doc))
            {
                addedShim = new HtmlDocument.HtmlDocumentShim(doc);
                this.htmlDocumentShims[doc] = addedShim;
            }
            if (addedShim != null)
            {
                this.OnShimAdded(addedShim);
            }
        }

        public void AddElementShim(HtmlElement element)
        {
            HtmlElement.HtmlElementShim addedShim = null;
            if (this.htmlElementShims == null)
            {
                this.htmlElementShims = new Dictionary<HtmlElement, HtmlElement.HtmlElementShim>();
                addedShim = new HtmlElement.HtmlElementShim(element);
                this.htmlElementShims[element] = addedShim;
            }
            else if (!this.htmlElementShims.ContainsKey(element))
            {
                addedShim = new HtmlElement.HtmlElementShim(element);
                this.htmlElementShims[element] = addedShim;
            }
            if (addedShim != null)
            {
                this.OnShimAdded(addedShim);
            }
        }

        public void AddWindowShim(HtmlWindow window)
        {
            HtmlWindow.HtmlWindowShim addedShim = null;
            if (this.htmlWindowShims == null)
            {
                this.htmlWindowShims = new Dictionary<HtmlWindow, HtmlWindow.HtmlWindowShim>();
                addedShim = new HtmlWindow.HtmlWindowShim(window);
                this.htmlWindowShims[window] = addedShim;
            }
            else if (!this.htmlWindowShims.ContainsKey(window))
            {
                addedShim = new HtmlWindow.HtmlWindowShim(window);
                this.htmlWindowShims[window] = addedShim;
            }
            if (addedShim != null)
            {
                this.OnShimAdded(addedShim);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.htmlElementShims != null)
                {
                    foreach (HtmlElement.HtmlElementShim shim in this.htmlElementShims.Values)
                    {
                        shim.Dispose();
                    }
                }
                if (this.htmlDocumentShims != null)
                {
                    foreach (HtmlDocument.HtmlDocumentShim shim2 in this.htmlDocumentShims.Values)
                    {
                        shim2.Dispose();
                    }
                }
                if (this.htmlWindowShims != null)
                {
                    foreach (HtmlWindow.HtmlWindowShim shim3 in this.htmlWindowShims.Values)
                    {
                        shim3.Dispose();
                    }
                }
                this.htmlWindowShims = null;
                this.htmlDocumentShims = null;
                this.htmlWindowShims = null;
            }
        }

        ~HtmlShimManager()
        {
            this.Dispose(false);
        }

        internal HtmlDocument.HtmlDocumentShim GetDocumentShim(HtmlDocument document)
        {
            if ((this.htmlDocumentShims != null) && this.htmlDocumentShims.ContainsKey(document))
            {
                return this.htmlDocumentShims[document];
            }
            return null;
        }

        internal HtmlElement.HtmlElementShim GetElementShim(HtmlElement element)
        {
            if ((this.htmlElementShims != null) && this.htmlElementShims.ContainsKey(element))
            {
                return this.htmlElementShims[element];
            }
            return null;
        }

        internal HtmlWindow.HtmlWindowShim GetWindowShim(HtmlWindow window)
        {
            if ((this.htmlWindowShims != null) && this.htmlWindowShims.ContainsKey(window))
            {
                return this.htmlWindowShims[window];
            }
            return null;
        }

        private void OnShimAdded(HtmlShim addedShim)
        {
            if ((addedShim != null) && !(addedShim is HtmlWindow.HtmlWindowShim))
            {
                this.AddWindowShim(new HtmlWindow(this, addedShim.AssociatedWindow));
            }
        }

        internal void OnWindowUnloaded(HtmlWindow unloadedWindow)
        {
            if (unloadedWindow != null)
            {
                if (this.htmlDocumentShims != null)
                {
                    HtmlDocument.HtmlDocumentShim[] array = new HtmlDocument.HtmlDocumentShim[this.htmlDocumentShims.Count];
                    this.htmlDocumentShims.Values.CopyTo(array, 0);
                    foreach (HtmlDocument.HtmlDocumentShim shim in array)
                    {
                        if (shim.AssociatedWindow == unloadedWindow.NativeHtmlWindow)
                        {
                            this.htmlDocumentShims.Remove(shim.Document);
                            shim.Dispose();
                        }
                    }
                }
                if (this.htmlElementShims != null)
                {
                    HtmlElement.HtmlElementShim[] shimArray2 = new HtmlElement.HtmlElementShim[this.htmlElementShims.Count];
                    this.htmlElementShims.Values.CopyTo(shimArray2, 0);
                    foreach (HtmlElement.HtmlElementShim shim2 in shimArray2)
                    {
                        if (shim2.AssociatedWindow == unloadedWindow.NativeHtmlWindow)
                        {
                            this.htmlElementShims.Remove(shim2.Element);
                            shim2.Dispose();
                        }
                    }
                }
                if ((this.htmlWindowShims != null) && this.htmlWindowShims.ContainsKey(unloadedWindow))
                {
                    HtmlWindow.HtmlWindowShim shim3 = this.htmlWindowShims[unloadedWindow];
                    this.htmlWindowShims.Remove(unloadedWindow);
                    shim3.Dispose();
                }
            }
        }
    }
}

