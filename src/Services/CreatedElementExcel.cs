using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Security;
using System.Text;
namespace CadLayerTools
{
    // Open XML workbook using the framework's OPC writer; Excel installation is not required.
    public static class CreatedElementExcel
    {
        const string Rel="http://schemas.openxmlformats.org/officeDocument/2006/relationships/";
        const string Main="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        public static void Save(string path,IList<CreatedElementRow> rows)
        {
            string temporary=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)),"."+Guid.NewGuid().ToString("N")+".xlsx");
            try
            {
                using(var package=Package.Open(temporary,FileMode.CreateNew,FileAccess.ReadWrite))
                {
                    var workbook=Part(package,"/xl/workbook.xml","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                        "<workbook xmlns=\""+Main+"\" xmlns:r=\""+Rel.TrimEnd('/')+"\"><sheets><sheet name=\"Created elements\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                    package.CreateRelationship(new Uri("/xl/workbook.xml",UriKind.Relative),TargetMode.Internal,Rel+"officeDocument");
                    workbook.CreateRelationship(new Uri("worksheets/sheet1.xml",UriKind.Relative),TargetMode.Internal,Rel+"worksheet","rId1");
                    workbook.CreateRelationship(new Uri("styles.xml",UriKind.Relative),TargetMode.Internal,Rel+"styles","rId2");
                    Part(package,"/xl/styles.xml","application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml",
                        "<styleSheet xmlns=\""+Main+"\"><fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font><font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font></fonts><fills count=\"3\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill><fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFD94B5B\"/><bgColor indexed=\"64\"/></patternFill></fill></fills><borders count=\"1\"><border/></borders><cellStyleXfs count=\"1\"><xf/></cellStyleXfs><cellXfs count=\"3\"><xf xfId=\"0\"/><xf xfId=\"0\" fontId=\"1\" fillId=\"2\" applyFont=\"1\" applyFill=\"1\"/><xf xfId=\"0\" numFmtId=\"4\" applyNumberFormat=\"1\"/></cellXfs><cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles></styleSheet>");
                    var xml=new StringBuilder("<worksheet xmlns=\""+Main+"\"><sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews><cols>");
                    int[] widths={16,26,36,32,24,20,20,20,26,18,45};
                    for(int i=0;i<widths.Length;i++)xml.Append($"<col min=\"{i+1}\" max=\"{i+1}\" width=\"{widths[i]}\" customWidth=\"1\"/>");
                    xml.Append("</cols><sheetData><row r=\"1\" ht=\"26\" customHeight=\"1\">");
                    string[] headers={"Element ID","Category","Family","Type","Level","X (mm)","Y (mm)","Z (mm)","Workset","Mark","Comments"};
                    for(int i=0;i<headers.Length;i++)Cell(xml,i,1,headers[i],1);
                    xml.Append("</row>");int rowNumber=2;
                    foreach(var row in rows)
                    {
                        xml.Append($"<row r=\"{rowNumber}\">");
                        object[] values={row.Id,row.Category,row.Family,row.Type,row.Level,row.X,row.Y,row.Z,row.Workset,row.Mark,row.Comments};
                        for(int i=0;i<values.Length;i++)Cell(xml,i,rowNumber,values[i],values[i] is double?2:0);
                        xml.Append("</row>");rowNumber++;
                    }
                    xml.Append($"</sheetData><autoFilter ref=\"A1:K{Math.Max(1,rowNumber-1)}\"/></worksheet>");
                    Part(package,"/xl/worksheets/sheet1.xml","application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml",xml.ToString());
                }
                File.Copy(temporary,path,true);
            }
            finally{if(File.Exists(temporary))File.Delete(temporary);}
        }
        static PackagePart Part(Package package,string name,string contentType,string xml)
        {
            var part=package.CreatePart(new Uri(name,UriKind.Relative),contentType,CompressionOption.Normal);
            using(var writer=new StreamWriter(part.GetStream(),new UTF8Encoding(false)))writer.Write(xml);
            return part;
        }
        static void Cell(StringBuilder xml,int column,int row,object value,int style)
        {
            string reference=((char)('A'+column)).ToString()+row;
            if(value is double number)
                xml.Append($"<c r=\"{reference}\" s=\"{style}\"><v>{number.ToString("R",CultureInfo.InvariantCulture)}</v></c>");
            else
            {
                string text=Convert.ToString(value,CultureInfo.InvariantCulture)??"";
                var clean=new StringBuilder();foreach(char c in text)if(c=='\t'||c=='\n'||c=='\r'||c>=32&&c!=0xfffe&&c!=0xffff)clean.Append(c);
                xml.Append($"<c r=\"{reference}\" s=\"{style}\" t=\"inlineStr\"><is><t xml:space=\"preserve\">{SecurityElement.Escape(clean.ToString())}</t></is></c>");
            }
        }
    }
}

