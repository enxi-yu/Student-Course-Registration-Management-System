using System.IO.Compression;
using System.Text;
using System.Xml;
using StudentCourse.Models;

namespace StudentCourse.Services
{
    public sealed class SystemLogExportService
    {
        private static readonly string[] Headers = { "时间", "操作人", "操作类型", "操作描述", "操作对象", "结果", "错误信息", "IP地址" };

        public byte[] BuildExcel(IList<SystemLogDto> logs)
        {
            using MemoryStream stream = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                Entry(archive, "[Content_Types].xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>
                    """);
                Entry(archive, "_rels/.rels", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>
                    """);
                Entry(archive, "xl/workbook.xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="系统日志" sheetId="1" r:id="rId1"/></sheets></workbook>
                    """);
                Entry(archive, "xl/_rels/workbook.xml.rels", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>
                    """);
                Entry(archive, "xl/styles.xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><fonts count="2"><font><sz val="11"/><name val="Microsoft YaHei"/></font><font><b/><color rgb="FFFFFFFF"/><sz val="11"/><name val="Microsoft YaHei"/></font></fonts><fills count="3"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FF3457D5"/></patternFill></fill></fills><borders count="2"><border/><border><left style="thin"/><right style="thin"/><top style="thin"/><bottom style="thin"/></border></borders><cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0"/><xf numFmtId="0" fontId="1" fillId="2" borderId="1" xfId="0" applyAlignment="1"><alignment horizontal="center"/></xf></cellXfs></styleSheet>
                    """);
                Worksheet(archive, logs);
            }
            return stream.ToArray();
        }

        private static void Worksheet(ZipArchive archive, IList<SystemLogDto> logs)
        {
            using Stream output = archive.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Fastest).Open();
            using XmlWriter writer = XmlWriter.Create(output, new XmlWriterSettings { Encoding = new UTF8Encoding(false) });
            const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            writer.WriteStartElement("worksheet", ns);
            writer.WriteStartElement("sheetViews", ns); writer.WriteStartElement("sheetView", ns); writer.WriteAttributeString("workbookViewId", "0"); writer.WriteStartElement("pane", ns); writer.WriteAttributeString("ySplit", "1"); writer.WriteAttributeString("state", "frozen"); writer.WriteEndElement(); writer.WriteEndElement(); writer.WriteEndElement();
            writer.WriteStartElement("cols", ns);
            double[] widths = { 21, 18, 14, 34, 18, 12, 28, 18 };
            for (int i = 0; i < widths.Length; i++) { writer.WriteStartElement("col", ns); writer.WriteAttributeString("min", (i + 1).ToString()); writer.WriteAttributeString("max", (i + 1).ToString()); writer.WriteAttributeString("width", widths[i].ToString(System.Globalization.CultureInfo.InvariantCulture)); writer.WriteAttributeString("customWidth", "1"); writer.WriteEndElement(); }
            writer.WriteEndElement(); writer.WriteStartElement("sheetData", ns); Row(writer, ns, 1, Headers, true);
            for (int i = 0; i < logs.Count; i++) { SystemLogDto x = logs[i]; Row(writer, ns, i + 2, new[] { x.LogTime, x.Username, x.OperationType, x.OperationDesc, x.TargetId, x.ResultStatus, x.ErrorMessage, x.IpAddress }, false); }
            writer.WriteEndElement(); writer.WriteStartElement("autoFilter", ns); writer.WriteAttributeString("ref", $"A1:H{logs.Count + 1}"); writer.WriteEndElement(); writer.WriteEndElement();
        }

        private static void Row(XmlWriter writer, string ns, int number, IEnumerable<string?> values, bool header)
        {
            writer.WriteStartElement("row", ns); writer.WriteAttributeString("r", number.ToString()); int column = 0;
            foreach (string? value in values) { writer.WriteStartElement("c", ns); writer.WriteAttributeString("r", $"{(char)('A' + column++)}{number}"); writer.WriteAttributeString("t", "inlineStr"); writer.WriteAttributeString("s", header ? "1" : "0"); writer.WriteStartElement("is", ns); writer.WriteElementString("t", ns, value ?? ""); writer.WriteEndElement(); writer.WriteEndElement(); }
            writer.WriteEndElement();
        }

        private static void Entry(ZipArchive archive, string path, string content)
        {
            using StreamWriter writer = new StreamWriter(archive.CreateEntry(path, CompressionLevel.Fastest).Open(), new UTF8Encoding(false)); writer.Write(content.Trim());
        }
    }
}
