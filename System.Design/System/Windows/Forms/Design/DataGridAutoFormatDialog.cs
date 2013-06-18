namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml;

    internal class DataGridAutoFormatDialog : Form
    {
        private Button button1;
        private Button button2;
        internal const string data = "<pulica><Scheme><SchemeName>Default</SchemeName><SchemePicture>default.bmp</SchemePicture><BorderStyle></BorderStyle><FlatMode></FlatMode><CaptionFont></CaptionFont><Font></Font><HeaderFont></HeaderFont><AlternatingBackColor></AlternatingBackColor><BackColor></BackColor><CaptionForeColor></CaptionForeColor><CaptionBackColor></CaptionBackColor><ForeColor></ForeColor><GridLineColor></GridLineColor><GridLineStyle></GridLineStyle><HeaderBackColor></HeaderBackColor><HeaderForeColor></HeaderForeColor><LinkColor></LinkColor><LinkHoverColor></LinkHoverColor><ParentRowsBackColor></ParentRowsBackColor><ParentRowsForeColor></ParentRowsForeColor><SelectionForeColor></SelectionForeColor><SelectionBackColor></SelectionBackColor></Scheme><Scheme><SchemeName>Professional 1</SchemeName><SchemePicture>professional1.bmp</SchemePicture><CaptionFont>Verdana, 10pt</CaptionFont><AlternatingBackColor>LightGray</AlternatingBackColor><CaptionForeColor>Navy</CaptionForeColor><CaptionBackColor>White</CaptionBackColor><ForeColor>Black</ForeColor><BackColor>DarkGray</BackColor><GridLineColor>Black</GridLineColor><GridLineStyle>None</GridLineStyle><HeaderBackColor>Silver</HeaderBackColor><HeaderForeColor>Black</HeaderForeColor><LinkColor>Navy</LinkColor><LinkHoverColor>Blue</LinkHoverColor><ParentRowsBackColor>White</ParentRowsBackColor><ParentRowsForeColor>Black</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>Navy</SelectionBackColor></Scheme><Scheme><SchemeName>Professional 2</SchemeName><SchemePicture>professional2.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><CaptionFont>Tahoma, 8pt</CaptionFont><AlternatingBackColor>Gainsboro</AlternatingBackColor><BackColor>Silver</BackColor><CaptionForeColor>White</CaptionForeColor><CaptionBackColor>DarkSlateBlue</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>White</GridLineColor><HeaderBackColor>DarkGray</HeaderBackColor><HeaderForeColor>Black</HeaderForeColor><LinkColor>DarkSlateBlue</LinkColor><LinkHoverColor>RoyalBlue</LinkHoverColor><ParentRowsBackColor>Black</ParentRowsBackColor><ParentRowsForeColor>White</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>DarkSlateBlue</SelectionBackColor></Scheme><Scheme><SchemeName>Professional 3</SchemeName><SchemePicture>professional3.bmp</SchemePicture><BorderStyle>None</BorderStyle><FlatMode>True</FlatMode><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><Font>Tahoma, 8pt</Font><AlternatingBackColor>LightGray</AlternatingBackColor><BackColor>Gainsboro</BackColor><BackgroundColor>Silver</BackgroundColor><CaptionForeColor>MidnightBlue</CaptionForeColor><CaptionBackColor>LightSteelBlue</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>DimGray</GridLineColor><GridLineStyle>None</GridLineStyle><HeaderBackColor>MidnightBlue</HeaderBackColor><HeaderForeColor>White</HeaderForeColor><LinkColor>MidnightBlue</LinkColor><LinkHoverColor>RoyalBlue</LinkHoverColor><ParentRowsBackColor>DarkGray</ParentRowsBackColor><ParentRowsForeColor>Black</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>CadetBlue</SelectionBackColor></Scheme><Scheme><SchemeName>Professional 4</SchemeName><SchemePicture>professional4.bmp</SchemePicture><BorderStyle>None</BorderStyle><FlatMode>True</FlatMode><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><Font>Tahoma, 8pt</Font><AlternatingBackColor>Lavender</AlternatingBackColor><BackColor>WhiteSmoke</BackColor><BackgroundColor>LightGray</BackgroundColor><CaptionForeColor>MidnightBlue</CaptionForeColor><CaptionBackColor>LightSteelBlue</CaptionBackColor><ForeColor>MidnightBlue</ForeColor><GridLineColor>Gainsboro</GridLineColor><GridLineStyle>None</GridLineStyle><HeaderBackColor>MidnightBlue</HeaderBackColor><HeaderForeColor>WhiteSmoke</HeaderForeColor><LinkColor>Teal</LinkColor><LinkHoverColor>DarkMagenta</LinkHoverColor><ParentRowsBackColor>Gainsboro</ParentRowsBackColor><ParentRowsForeColor>MidnightBlue</ParentRowsForeColor><SelectionForeColor>WhiteSmoke</SelectionForeColor><SelectionBackColor>CadetBlue</SelectionBackColor></Scheme><Scheme><SchemeName>Classic</SchemeName><SchemePicture>classic.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><Font>Times New Roman, 9pt</Font><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><AlternatingBackColor>WhiteSmoke</AlternatingBackColor><BackColor>Gainsboro</BackColor><BackgroundColor>DarkGray</BackgroundColor><CaptionForeColor>Black</CaptionForeColor><CaptionBackColor>DarkKhaki</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>Silver</GridLineColor><HeaderBackColor>Black</HeaderBackColor><HeaderForeColor>White</HeaderForeColor><LinkColor>DarkSlateBlue</LinkColor><LinkHoverColor>Firebrick</LinkHoverColor><ParentRowsForeColor>Black</ParentRowsForeColor><ParentRowsBackColor>LightGray</ParentRowsBackColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>Firebrick</SelectionBackColor></Scheme><Scheme><SchemeName>Simple</SchemeName><SchemePicture>Simple.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><Font>Courier New, 9pt</Font><HeaderFont>Courier New, 10pt, style=1</HeaderFont><CaptionFont>Courier New, 10pt, style=1</CaptionFont><AlternatingBackColor>White</AlternatingBackColor><BackColor>White</BackColor><BackgroundColor>Gainsboro</BackgroundColor><CaptionForeColor>Black</CaptionForeColor><CaptionBackColor>Silver</CaptionBackColor><ForeColor>DarkSlateGray</ForeColor><GridLineColor>DarkGray</GridLineColor><HeaderBackColor>DarkGreen</HeaderBackColor><HeaderForeColor>White</HeaderForeColor><LinkColor>DarkGreen</LinkColor><LinkHoverColor>Blue</LinkHoverColor><ParentRowsForeColor>Black</ParentRowsForeColor><ParentRowsBackColor>Gainsboro</ParentRowsBackColor><SelectionForeColor>Black</SelectionForeColor><SelectionBackColor>DarkSeaGreen</SelectionBackColor></Scheme><Scheme><SchemeName>Colorful 1</SchemeName><SchemePicture>colorful1.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 9pt, style=1</CaptionFont><HeaderFont>Tahoma, 9pt, style=1</HeaderFont><AlternatingBackColor>LightGoldenrodYellow</AlternatingBackColor><BackColor>White</BackColor><BackgroundColor>LightGoldenrodYellow</BackgroundColor><CaptionForeColor>DarkSlateBlue</CaptionForeColor><CaptionBackColor>LightGoldenrodYellow</CaptionBackColor><ForeColor>DarkSlateBlue</ForeColor><GridLineColor>Peru</GridLineColor><GridLineStyle>None</GridLineStyle><HeaderBackColor>Maroon</HeaderBackColor><HeaderForeColor>LightGoldenrodYellow</HeaderForeColor><LinkColor>Maroon</LinkColor><LinkHoverColor>SlateBlue</LinkHoverColor><ParentRowsBackColor>BurlyWood</ParentRowsBackColor><ParentRowsForeColor>DarkSlateBlue</ParentRowsForeColor><SelectionForeColor>GhostWhite</SelectionForeColor><SelectionBackColor>DarkSlateBlue</SelectionBackColor></Scheme><Scheme><SchemeName>Colorful 2</SchemeName><SchemePicture>colorful2.bmp</SchemePicture><BorderStyle>None</BorderStyle><FlatMode>True</FlatMode><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><AlternatingBackColor>GhostWhite</AlternatingBackColor><BackColor>GhostWhite</BackColor><BackgroundColor>Lavender</BackgroundColor><CaptionForeColor>White</CaptionForeColor><CaptionBackColor>RoyalBlue</CaptionBackColor><ForeColor>MidnightBlue</ForeColor><GridLineColor>RoyalBlue</GridLineColor><HeaderBackColor>MidnightBlue</HeaderBackColor><HeaderForeColor>Lavender</HeaderForeColor><LinkColor>Teal</LinkColor><LinkHoverColor>DodgerBlue</LinkHoverColor><ParentRowsBackColor>Lavender</ParentRowsBackColor><ParentRowsForeColor>MidnightBlue</ParentRowsForeColor><SelectionForeColor>PaleGreen</SelectionForeColor><SelectionBackColor>Teal</SelectionBackColor></Scheme><Scheme><SchemeName>Colorful 3</SchemeName><SchemePicture>colorful3.bmp</SchemePicture><BorderStyle>None</BorderStyle><FlatMode>True</FlatMode><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><AlternatingBackColor>OldLace</AlternatingBackColor><BackColor>OldLace</BackColor><BackgroundColor>Tan</BackgroundColor><CaptionForeColor>OldLace</CaptionForeColor><CaptionBackColor>SaddleBrown</CaptionBackColor><ForeColor>DarkSlateGray</ForeColor><GridLineColor>Tan</GridLineColor><GridLineStyle>Solid</GridLineStyle><HeaderBackColor>Wheat</HeaderBackColor><HeaderForeColor>SaddleBrown</HeaderForeColor><LinkColor>DarkSlateBlue</LinkColor><LinkHoverColor>Teal</LinkHoverColor><ParentRowsBackColor>OldLace</ParentRowsBackColor><ParentRowsForeColor>DarkSlateGray</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>SlateGray</SelectionBackColor></Scheme><Scheme><SchemeName>Colorful 4</SchemeName><SchemePicture>colorful4.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><AlternatingBackColor>White</AlternatingBackColor><BackColor>White</BackColor><BackgroundColor>Ivory</BackgroundColor><CaptionForeColor>Lavender</CaptionForeColor><CaptionBackColor>DarkSlateBlue</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>Wheat</GridLineColor><HeaderBackColor>CadetBlue</HeaderBackColor><HeaderForeColor>Black</HeaderForeColor><LinkColor>DarkSlateBlue</LinkColor><LinkHoverColor>LightSeaGreen</LinkHoverColor><ParentRowsBackColor>Ivory</ParentRowsBackColor><ParentRowsForeColor>Black</ParentRowsForeColor><SelectionForeColor>DarkSlateBlue</SelectionForeColor><SelectionBackColor>Wheat</SelectionBackColor></Scheme><Scheme><SchemeName>256 Color 1</SchemeName><SchemePicture>256_1.bmp</SchemePicture><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 8 pt</CaptionFont><HeaderFont>Tahoma, 8pt</HeaderFont><AlternatingBackColor>Silver</AlternatingBackColor><BackColor>White</BackColor><CaptionForeColor>White</CaptionForeColor><CaptionBackColor>Maroon</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>Silver</GridLineColor><HeaderBackColor>Silver</HeaderBackColor><HeaderForeColor>Black</HeaderForeColor><LinkColor>Maroon</LinkColor><LinkHoverColor>Red</LinkHoverColor><ParentRowsBackColor>Silver</ParentRowsBackColor><ParentRowsForeColor>Black</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>Maroon</SelectionBackColor></Scheme><Scheme><SchemeName>256 Color 2</SchemeName><SchemePicture>256_2.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><CaptionFont>Microsoft Sans Serif, 10 pt, style=1</CaptionFont><Font>Tahoma, 8pt</Font><HeaderFont>Tahoma, 8pt</HeaderFont><AlternatingBackColor>White</AlternatingBackColor><BackColor>White</BackColor><CaptionForeColor>White</CaptionForeColor><CaptionBackColor>Teal</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>Silver</GridLineColor><HeaderBackColor>Black</HeaderBackColor><HeaderForeColor>White</HeaderForeColor><LinkColor>Purple</LinkColor><LinkHoverColor>Fuchsia</LinkHoverColor><ParentRowsBackColor>Gray</ParentRowsBackColor><ParentRowsForeColor>White</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>Maroon</SelectionBackColor></Scheme></pulica>";
        private AutoFormatDataGrid dataGrid;
        private DataSet dataSet = new DataSet();
        private DataGrid dgrid;
        private Label formats;
        private bool IMBusy;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private Label preview;
        internal const string scheme = "<xsd:schema id=\"pulica\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\"><xsd:element name=\"Scheme\"><xsd:complexType><xsd:all><xsd:element name=\"SchemeName\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"SchemePicture\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"FlatMode\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"Font\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"CaptionFont\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"HeaderFont\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"AlternatingBackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"BackgroundColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"CaptionForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"CaptionBackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"GridLineColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"GridLineStyle\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"HeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"HeaderForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"LinkColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"LinkHoverColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"ParentRowsBackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"ParentRowsForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"SelectionForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"SelectionBackColor\" minOccurs=\"0\" type=\"xsd:string\"/></xsd:all></xsd:complexType></xsd:element></xsd:schema>";
        private ListBox schemeName;
        private DataTable schemeTable;
        private int selectedIndex = -1;
        private DataGridTableStyle tableStyle;

        internal DataGridAutoFormatDialog(DataGrid dgrid)
        {
            this.dgrid = dgrid;
            base.ShowInTaskbar = false;
            this.dataSet.Locale = CultureInfo.InvariantCulture;
            this.dataSet.ReadXmlSchema(new XmlTextReader(new StringReader("<xsd:schema id=\"pulica\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\"><xsd:element name=\"Scheme\"><xsd:complexType><xsd:all><xsd:element name=\"SchemeName\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"SchemePicture\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"FlatMode\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"Font\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"CaptionFont\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"HeaderFont\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"AlternatingBackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"BackgroundColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"CaptionForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"CaptionBackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"GridLineColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"GridLineStyle\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"HeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"HeaderForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"LinkColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"LinkHoverColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"ParentRowsBackColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"ParentRowsForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"SelectionForeColor\" minOccurs=\"0\" type=\"xsd:string\"/><xsd:element name=\"SelectionBackColor\" minOccurs=\"0\" type=\"xsd:string\"/></xsd:all></xsd:complexType></xsd:element></xsd:schema>")));
            this.dataSet.ReadXml(new StringReader("<pulica><Scheme><SchemeName>Default</SchemeName><SchemePicture>default.bmp</SchemePicture><BorderStyle></BorderStyle><FlatMode></FlatMode><CaptionFont></CaptionFont><Font></Font><HeaderFont></HeaderFont><AlternatingBackColor></AlternatingBackColor><BackColor></BackColor><CaptionForeColor></CaptionForeColor><CaptionBackColor></CaptionBackColor><ForeColor></ForeColor><GridLineColor></GridLineColor><GridLineStyle></GridLineStyle><HeaderBackColor></HeaderBackColor><HeaderForeColor></HeaderForeColor><LinkColor></LinkColor><LinkHoverColor></LinkHoverColor><ParentRowsBackColor></ParentRowsBackColor><ParentRowsForeColor></ParentRowsForeColor><SelectionForeColor></SelectionForeColor><SelectionBackColor></SelectionBackColor></Scheme><Scheme><SchemeName>Professional 1</SchemeName><SchemePicture>professional1.bmp</SchemePicture><CaptionFont>Verdana, 10pt</CaptionFont><AlternatingBackColor>LightGray</AlternatingBackColor><CaptionForeColor>Navy</CaptionForeColor><CaptionBackColor>White</CaptionBackColor><ForeColor>Black</ForeColor><BackColor>DarkGray</BackColor><GridLineColor>Black</GridLineColor><GridLineStyle>None</GridLineStyle><HeaderBackColor>Silver</HeaderBackColor><HeaderForeColor>Black</HeaderForeColor><LinkColor>Navy</LinkColor><LinkHoverColor>Blue</LinkHoverColor><ParentRowsBackColor>White</ParentRowsBackColor><ParentRowsForeColor>Black</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>Navy</SelectionBackColor></Scheme><Scheme><SchemeName>Professional 2</SchemeName><SchemePicture>professional2.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><CaptionFont>Tahoma, 8pt</CaptionFont><AlternatingBackColor>Gainsboro</AlternatingBackColor><BackColor>Silver</BackColor><CaptionForeColor>White</CaptionForeColor><CaptionBackColor>DarkSlateBlue</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>White</GridLineColor><HeaderBackColor>DarkGray</HeaderBackColor><HeaderForeColor>Black</HeaderForeColor><LinkColor>DarkSlateBlue</LinkColor><LinkHoverColor>RoyalBlue</LinkHoverColor><ParentRowsBackColor>Black</ParentRowsBackColor><ParentRowsForeColor>White</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>DarkSlateBlue</SelectionBackColor></Scheme><Scheme><SchemeName>Professional 3</SchemeName><SchemePicture>professional3.bmp</SchemePicture><BorderStyle>None</BorderStyle><FlatMode>True</FlatMode><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><Font>Tahoma, 8pt</Font><AlternatingBackColor>LightGray</AlternatingBackColor><BackColor>Gainsboro</BackColor><BackgroundColor>Silver</BackgroundColor><CaptionForeColor>MidnightBlue</CaptionForeColor><CaptionBackColor>LightSteelBlue</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>DimGray</GridLineColor><GridLineStyle>None</GridLineStyle><HeaderBackColor>MidnightBlue</HeaderBackColor><HeaderForeColor>White</HeaderForeColor><LinkColor>MidnightBlue</LinkColor><LinkHoverColor>RoyalBlue</LinkHoverColor><ParentRowsBackColor>DarkGray</ParentRowsBackColor><ParentRowsForeColor>Black</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>CadetBlue</SelectionBackColor></Scheme><Scheme><SchemeName>Professional 4</SchemeName><SchemePicture>professional4.bmp</SchemePicture><BorderStyle>None</BorderStyle><FlatMode>True</FlatMode><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><Font>Tahoma, 8pt</Font><AlternatingBackColor>Lavender</AlternatingBackColor><BackColor>WhiteSmoke</BackColor><BackgroundColor>LightGray</BackgroundColor><CaptionForeColor>MidnightBlue</CaptionForeColor><CaptionBackColor>LightSteelBlue</CaptionBackColor><ForeColor>MidnightBlue</ForeColor><GridLineColor>Gainsboro</GridLineColor><GridLineStyle>None</GridLineStyle><HeaderBackColor>MidnightBlue</HeaderBackColor><HeaderForeColor>WhiteSmoke</HeaderForeColor><LinkColor>Teal</LinkColor><LinkHoverColor>DarkMagenta</LinkHoverColor><ParentRowsBackColor>Gainsboro</ParentRowsBackColor><ParentRowsForeColor>MidnightBlue</ParentRowsForeColor><SelectionForeColor>WhiteSmoke</SelectionForeColor><SelectionBackColor>CadetBlue</SelectionBackColor></Scheme><Scheme><SchemeName>Classic</SchemeName><SchemePicture>classic.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><Font>Times New Roman, 9pt</Font><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><AlternatingBackColor>WhiteSmoke</AlternatingBackColor><BackColor>Gainsboro</BackColor><BackgroundColor>DarkGray</BackgroundColor><CaptionForeColor>Black</CaptionForeColor><CaptionBackColor>DarkKhaki</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>Silver</GridLineColor><HeaderBackColor>Black</HeaderBackColor><HeaderForeColor>White</HeaderForeColor><LinkColor>DarkSlateBlue</LinkColor><LinkHoverColor>Firebrick</LinkHoverColor><ParentRowsForeColor>Black</ParentRowsForeColor><ParentRowsBackColor>LightGray</ParentRowsBackColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>Firebrick</SelectionBackColor></Scheme><Scheme><SchemeName>Simple</SchemeName><SchemePicture>Simple.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><Font>Courier New, 9pt</Font><HeaderFont>Courier New, 10pt, style=1</HeaderFont><CaptionFont>Courier New, 10pt, style=1</CaptionFont><AlternatingBackColor>White</AlternatingBackColor><BackColor>White</BackColor><BackgroundColor>Gainsboro</BackgroundColor><CaptionForeColor>Black</CaptionForeColor><CaptionBackColor>Silver</CaptionBackColor><ForeColor>DarkSlateGray</ForeColor><GridLineColor>DarkGray</GridLineColor><HeaderBackColor>DarkGreen</HeaderBackColor><HeaderForeColor>White</HeaderForeColor><LinkColor>DarkGreen</LinkColor><LinkHoverColor>Blue</LinkHoverColor><ParentRowsForeColor>Black</ParentRowsForeColor><ParentRowsBackColor>Gainsboro</ParentRowsBackColor><SelectionForeColor>Black</SelectionForeColor><SelectionBackColor>DarkSeaGreen</SelectionBackColor></Scheme><Scheme><SchemeName>Colorful 1</SchemeName><SchemePicture>colorful1.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 9pt, style=1</CaptionFont><HeaderFont>Tahoma, 9pt, style=1</HeaderFont><AlternatingBackColor>LightGoldenrodYellow</AlternatingBackColor><BackColor>White</BackColor><BackgroundColor>LightGoldenrodYellow</BackgroundColor><CaptionForeColor>DarkSlateBlue</CaptionForeColor><CaptionBackColor>LightGoldenrodYellow</CaptionBackColor><ForeColor>DarkSlateBlue</ForeColor><GridLineColor>Peru</GridLineColor><GridLineStyle>None</GridLineStyle><HeaderBackColor>Maroon</HeaderBackColor><HeaderForeColor>LightGoldenrodYellow</HeaderForeColor><LinkColor>Maroon</LinkColor><LinkHoverColor>SlateBlue</LinkHoverColor><ParentRowsBackColor>BurlyWood</ParentRowsBackColor><ParentRowsForeColor>DarkSlateBlue</ParentRowsForeColor><SelectionForeColor>GhostWhite</SelectionForeColor><SelectionBackColor>DarkSlateBlue</SelectionBackColor></Scheme><Scheme><SchemeName>Colorful 2</SchemeName><SchemePicture>colorful2.bmp</SchemePicture><BorderStyle>None</BorderStyle><FlatMode>True</FlatMode><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><AlternatingBackColor>GhostWhite</AlternatingBackColor><BackColor>GhostWhite</BackColor><BackgroundColor>Lavender</BackgroundColor><CaptionForeColor>White</CaptionForeColor><CaptionBackColor>RoyalBlue</CaptionBackColor><ForeColor>MidnightBlue</ForeColor><GridLineColor>RoyalBlue</GridLineColor><HeaderBackColor>MidnightBlue</HeaderBackColor><HeaderForeColor>Lavender</HeaderForeColor><LinkColor>Teal</LinkColor><LinkHoverColor>DodgerBlue</LinkHoverColor><ParentRowsBackColor>Lavender</ParentRowsBackColor><ParentRowsForeColor>MidnightBlue</ParentRowsForeColor><SelectionForeColor>PaleGreen</SelectionForeColor><SelectionBackColor>Teal</SelectionBackColor></Scheme><Scheme><SchemeName>Colorful 3</SchemeName><SchemePicture>colorful3.bmp</SchemePicture><BorderStyle>None</BorderStyle><FlatMode>True</FlatMode><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><AlternatingBackColor>OldLace</AlternatingBackColor><BackColor>OldLace</BackColor><BackgroundColor>Tan</BackgroundColor><CaptionForeColor>OldLace</CaptionForeColor><CaptionBackColor>SaddleBrown</CaptionBackColor><ForeColor>DarkSlateGray</ForeColor><GridLineColor>Tan</GridLineColor><GridLineStyle>Solid</GridLineStyle><HeaderBackColor>Wheat</HeaderBackColor><HeaderForeColor>SaddleBrown</HeaderForeColor><LinkColor>DarkSlateBlue</LinkColor><LinkHoverColor>Teal</LinkHoverColor><ParentRowsBackColor>OldLace</ParentRowsBackColor><ParentRowsForeColor>DarkSlateGray</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>SlateGray</SelectionBackColor></Scheme><Scheme><SchemeName>Colorful 4</SchemeName><SchemePicture>colorful4.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 8pt, style=1</CaptionFont><HeaderFont>Tahoma, 8pt, style=1</HeaderFont><AlternatingBackColor>White</AlternatingBackColor><BackColor>White</BackColor><BackgroundColor>Ivory</BackgroundColor><CaptionForeColor>Lavender</CaptionForeColor><CaptionBackColor>DarkSlateBlue</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>Wheat</GridLineColor><HeaderBackColor>CadetBlue</HeaderBackColor><HeaderForeColor>Black</HeaderForeColor><LinkColor>DarkSlateBlue</LinkColor><LinkHoverColor>LightSeaGreen</LinkHoverColor><ParentRowsBackColor>Ivory</ParentRowsBackColor><ParentRowsForeColor>Black</ParentRowsForeColor><SelectionForeColor>DarkSlateBlue</SelectionForeColor><SelectionBackColor>Wheat</SelectionBackColor></Scheme><Scheme><SchemeName>256 Color 1</SchemeName><SchemePicture>256_1.bmp</SchemePicture><Font>Tahoma, 8pt</Font><CaptionFont>Tahoma, 8 pt</CaptionFont><HeaderFont>Tahoma, 8pt</HeaderFont><AlternatingBackColor>Silver</AlternatingBackColor><BackColor>White</BackColor><CaptionForeColor>White</CaptionForeColor><CaptionBackColor>Maroon</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>Silver</GridLineColor><HeaderBackColor>Silver</HeaderBackColor><HeaderForeColor>Black</HeaderForeColor><LinkColor>Maroon</LinkColor><LinkHoverColor>Red</LinkHoverColor><ParentRowsBackColor>Silver</ParentRowsBackColor><ParentRowsForeColor>Black</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>Maroon</SelectionBackColor></Scheme><Scheme><SchemeName>256 Color 2</SchemeName><SchemePicture>256_2.bmp</SchemePicture><BorderStyle>FixedSingle</BorderStyle><FlatMode>True</FlatMode><CaptionFont>Microsoft Sans Serif, 10 pt, style=1</CaptionFont><Font>Tahoma, 8pt</Font><HeaderFont>Tahoma, 8pt</HeaderFont><AlternatingBackColor>White</AlternatingBackColor><BackColor>White</BackColor><CaptionForeColor>White</CaptionForeColor><CaptionBackColor>Teal</CaptionBackColor><ForeColor>Black</ForeColor><GridLineColor>Silver</GridLineColor><HeaderBackColor>Black</HeaderBackColor><HeaderForeColor>White</HeaderForeColor><LinkColor>Purple</LinkColor><LinkHoverColor>Fuchsia</LinkHoverColor><ParentRowsBackColor>Gray</ParentRowsBackColor><ParentRowsForeColor>White</ParentRowsForeColor><SelectionForeColor>White</SelectionForeColor><SelectionBackColor>Maroon</SelectionBackColor></Scheme></pulica>"), XmlReadMode.IgnoreSchema);
            this.schemeTable = this.dataSet.Tables["Scheme"];
            this.IMBusy = true;
            this.InitializeComponent();
            this.schemeName.DataSource = this.schemeTable;
            this.AddDataToDataGrid();
            this.AddStyleSheetInformationToDataGrid();
            if (dgrid.Site != null)
            {
                IUIService service = (IUIService) dgrid.Site.GetService(typeof(IUIService));
                if (service != null)
                {
                    Font font = (Font) service.Styles["DialogFont"];
                    if (font != null)
                    {
                        this.Font = font;
                    }
                }
            }
            this.IMBusy = false;
        }

        private void AddDataToDataGrid()
        {
            DataTable dataSource = new DataTable("Table1") {
                Locale = CultureInfo.InvariantCulture
            };
            dataSource.Columns.Add(new DataColumn("First Name"));
            dataSource.Columns.Add(new DataColumn("Last Name"));
            DataRow row = dataSource.NewRow();
            row["First Name"] = "Robert";
            row["Last Name"] = "Brown";
            dataSource.Rows.Add(row);
            row = dataSource.NewRow();
            row["First Name"] = "Nate";
            row["Last Name"] = "Sun";
            dataSource.Rows.Add(row);
            row = dataSource.NewRow();
            row["First Name"] = "Carole";
            row["Last Name"] = "Poland";
            dataSource.Rows.Add(row);
            this.dataGrid.SetDataBinding(dataSource, "");
        }

        private void AddStyleSheetInformationToDataGrid()
        {
            DataGridTableStyle table = new DataGridTableStyle {
                MappingName = "Table1"
            };
            DataGridColumnStyle column = new DataGridTextBoxColumn {
                MappingName = "First Name",
                HeaderText = System.Design.SR.GetString("DataGridAutoFormatTableFirstColumn")
            };
            DataGridColumnStyle style3 = new DataGridTextBoxColumn {
                MappingName = "Last Name",
                HeaderText = System.Design.SR.GetString("DataGridAutoFormatTableSecondColumn")
            };
            table.GridColumnStyles.Add(column);
            table.GridColumnStyles.Add(style3);
            DataRowCollection rows = this.dataSet.Tables["Scheme"].Rows;
            DataRow row = rows[0];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameDefault");
            row = rows[1];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameProfessional1");
            row = rows[2];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameProfessional2");
            row = rows[3];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameProfessional3");
            row = rows[4];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameProfessional4");
            row = rows[5];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameClassic");
            row = rows[6];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameSimple");
            row = rows[7];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameColorful1");
            row = rows[8];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameColorful2");
            row = rows[9];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameColorful3");
            row = rows[10];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeNameColorful4");
            row = rows[11];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeName256Color1");
            row = rows[12];
            row["SchemeName"] = System.Design.SR.GetString("DataGridAutoFormatSchemeName256Color2");
            this.dataGrid.TableStyles.Add(table);
            this.tableStyle = table;
        }

        private void AutoFormat_HelpRequested(object sender, HelpEventArgs e)
        {
            if ((this.dgrid != null) && (this.dgrid.Site != null))
            {
                IDesignerHost host = this.dgrid.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null)
                {
                    IHelpService service = host.GetService(typeof(IHelpService)) as IHelpService;
                    if (service != null)
                    {
                        service.ShowHelpFromKeyword("vs.DataGridAutoFormatDialog");
                    }
                }
            }
        }

        private void Button1_Clicked(object sender, EventArgs e)
        {
            this.selectedIndex = this.schemeName.SelectedIndex;
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(DataGridAutoFormatDialog));
            this.formats = new Label();
            this.schemeName = new ListBox();
            this.dataGrid = new AutoFormatDataGrid();
            this.preview = new Label();
            this.button1 = new Button();
            this.button2 = new Button();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.dataGrid.BeginInit();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.formats, "formats");
            this.formats.Margin = new Padding(0, 0, 3, 0);
            this.formats.Name = "formats";
            manager.ApplyResources(this.schemeName, "schemeName");
            this.schemeName.DisplayMember = "SchemeName";
            this.schemeName.FormattingEnabled = true;
            this.schemeName.Margin = new Padding(0, 2, 3, 3);
            this.schemeName.Name = "schemeName";
            this.schemeName.SelectedIndexChanged += new EventHandler(this.SchemeName_SelectionChanged);
            manager.ApplyResources(this.dataGrid, "dataGrid");
            this.dataGrid.DataMember = "";
            this.dataGrid.HeaderForeColor = SystemColors.ControlText;
            this.dataGrid.Margin = new Padding(3, 2, 0, 3);
            this.dataGrid.Name = "dataGrid";
            manager.ApplyResources(this.preview, "preview");
            this.preview.Margin = new Padding(3, 0, 0, 0);
            this.preview.Name = "preview";
            manager.ApplyResources(this.button1, "button1");
            this.button1.DialogResult = DialogResult.OK;
            this.button1.Margin = new Padding(0, 0, 3, 0);
            this.button1.MinimumSize = new Size(0x4b, 0x17);
            this.button1.Name = "button1";
            this.button1.Padding = new Padding(10, 0, 10, 0);
            this.button1.Click += new EventHandler(this.Button1_Clicked);
            manager.ApplyResources(this.button2, "button2");
            this.button2.DialogResult = DialogResult.Cancel;
            this.button2.Margin = new Padding(3, 0, 0, 0);
            this.button2.MinimumSize = new Size(0x4b, 0x17);
            this.button2.Name = "button2";
            this.button2.Padding = new Padding(10, 0, 10, 0);
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.overarchingTableLayoutPanel.SetColumnSpan(this.okCancelTableLayoutPanel, 2);
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.Controls.Add(this.button1, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.button2, 1, 0);
            this.okCancelTableLayoutPanel.Margin = new Padding(0, 6, 0, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 146f));
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 182f));
            this.overarchingTableLayoutPanel.Controls.Add(this.okCancelTableLayoutPanel, 0, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.preview, 1, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.dataGrid, 1, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.formats, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.schemeName, 0, 1);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            base.AcceptButton = this.button1;
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.button2;
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "DataGridAutoFormatDialog";
            base.ShowIcon = false;
            base.HelpRequested += new HelpEventHandler(this.AutoFormat_HelpRequested);
            this.dataGrid.EndInit();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
        }

        private static bool IsTableProperty(string propName)
        {
            return (propName.Equals("HeaderColor") || (propName.Equals("AlternatingBackColor") || (propName.Equals("BackColor") || (propName.Equals("ForeColor") || (propName.Equals("GridLineColor") || (propName.Equals("GridLineStyle") || (propName.Equals("HeaderBackColor") || (propName.Equals("HeaderForeColor") || (propName.Equals("LinkColor") || (propName.Equals("LinkHoverColor") || (propName.Equals("SelectionForeColor") || (propName.Equals("SelectionBackColor") || propName.Equals("HeaderFont")))))))))))));
        }

        private void SchemeName_SelectionChanged(object sender, EventArgs e)
        {
            if (!this.IMBusy)
            {
                DataRow row = ((DataRowView) this.schemeName.SelectedItem).Row;
                if (row != null)
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(DataGrid));
                    PropertyDescriptorCollection descriptors2 = TypeDescriptor.GetProperties(typeof(DataGridTableStyle));
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        PropertyDescriptor descriptor;
                        object tableStyle;
                        object obj2 = row[column];
                        if (IsTableProperty(column.ColumnName))
                        {
                            descriptor = descriptors2[column.ColumnName];
                            tableStyle = this.tableStyle;
                        }
                        else
                        {
                            descriptor = properties[column.ColumnName];
                            tableStyle = this.dataGrid;
                        }
                        if (descriptor != null)
                        {
                            if (Convert.IsDBNull(obj2) || (obj2.ToString().Length == 0))
                            {
                                descriptor.ResetValue(tableStyle);
                            }
                            else
                            {
                                try
                                {
                                    object obj4 = descriptor.Converter.ConvertFromString(obj2.ToString());
                                    descriptor.SetValue(tableStyle, obj4);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        public DataRow SelectedData
        {
            get
            {
                if (this.schemeName != null)
                {
                    return ((DataRowView) this.schemeName.Items[this.selectedIndex]).Row;
                }
                return null;
            }
        }

        private class AutoFormatDataGrid : DataGrid
        {
            protected override void OnKeyDown(KeyEventArgs e)
            {
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                return false;
            }

            protected override bool ProcessKeyPreview(ref Message m)
            {
                return false;
            }
        }
    }
}

