using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using StudentCourse.Models;

namespace StudentCourse.Services
{
    public sealed class ExportService
    {
        private static readonly string[] Headers = { "学号", "姓名", "专业", "年级", "选课时间" };

        public byte[] BuildClassStudentsExcel(IList<StudentListDto> students)
        {
            using MemoryStream stream = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                WriteTextEntry(archive, "[Content_Types].xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                      <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                      <Default Extension="xml" ContentType="application/xml"/>
                      <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                      <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                      <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                    </Types>
                    """);
                WriteTextEntry(archive, "_rels/.rels", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                    </Relationships>
                    """);
                WriteTextEntry(archive, "xl/workbook.xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                      <sheets><sheet name="选课名单" sheetId="1" r:id="rId1"/></sheets>
                    </workbook>
                    """);
                WriteTextEntry(archive, "xl/_rels/workbook.xml.rels", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                      <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                    </Relationships>
                    """);
                WriteTextEntry(archive, "xl/styles.xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                      <fonts count="2"><font><sz val="11"/><name val="Microsoft YaHei"/></font><font><b/><color rgb="FFFFFFFF"/><sz val="11"/><name val="Microsoft YaHei"/></font></fonts>
                      <fills count="3"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FF3457D5"/><bgColor indexed="64"/></patternFill></fill></fills>
                      <borders count="2"><border/><border><left style="thin"><color rgb="FFD9E2F3"/></left><right style="thin"><color rgb="FFD9E2F3"/></right><top style="thin"><color rgb="FFD9E2F3"/></top><bottom style="thin"><color rgb="FFD9E2F3"/></bottom></border></borders>
                      <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
                      <cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" applyAlignment="1"><alignment vertical="center"/></xf><xf numFmtId="0" fontId="1" fillId="2" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="center" vertical="center"/></xf></cellXfs>
                    </styleSheet>
                    """);
                WriteWorksheet(archive, students);
            }
            return stream.ToArray();
        }

        private static void WriteWorksheet(ZipArchive archive, IList<StudentListDto> students)
        {
            ZipArchiveEntry entry = archive.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Fastest);
            using Stream output = entry.Open();
            using XmlWriter writer = XmlWriter.Create(output, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true });
            const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            writer.WriteStartDocument(true);
            writer.WriteStartElement("worksheet", ns);
            writer.WriteStartElement("dimension", ns);
            writer.WriteAttributeString("ref", $"A1:E{students.Count + 1}");
            writer.WriteEndElement();
            writer.WriteStartElement("sheetViews", ns);
            writer.WriteStartElement("sheetView", ns);
            writer.WriteAttributeString("workbookViewId", "0");
            writer.WriteStartElement("pane", ns);
            writer.WriteAttributeString("ySplit", "1");
            writer.WriteAttributeString("topLeftCell", "A2");
            writer.WriteAttributeString("activePane", "bottomLeft");
            writer.WriteAttributeString("state", "frozen");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteStartElement("cols", ns);
            WriteColumn(writer, ns, 1, 16);
            WriteColumn(writer, ns, 2, 14);
            WriteColumn(writer, ns, 3, 24);
            WriteColumn(writer, ns, 4, 12);
            WriteColumn(writer, ns, 5, 22);
            writer.WriteEndElement();
            writer.WriteStartElement("sheetData", ns);
            WriteRow(writer, ns, 1, Headers, true);
            for (int index = 0; index < students.Count; index++)
            {
                StudentListDto student = students[index];
                WriteRow(writer, ns, index + 2, new[] { student.StudentNo, student.StudentName, student.Major, student.Grade, student.SelectTime }, false);
            }
            writer.WriteEndElement();
            writer.WriteStartElement("autoFilter", ns);
            writer.WriteAttributeString("ref", $"A1:E{students.Count + 1}");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        private static void WriteColumn(XmlWriter writer, string ns, int index, double width)
        {
            writer.WriteStartElement("col", ns);
            writer.WriteAttributeString("min", index.ToString());
            writer.WriteAttributeString("max", index.ToString());
            writer.WriteAttributeString("width", width.ToString(System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteAttributeString("customWidth", "1");
            writer.WriteEndElement();
        }

        private static void WriteRow(XmlWriter writer, string ns, int rowNumber, IEnumerable<string?> values, bool header)
        {
            writer.WriteStartElement("row", ns);
            writer.WriteAttributeString("r", rowNumber.ToString());
            writer.WriteAttributeString("ht", header ? "24" : "21");
            writer.WriteAttributeString("customHeight", "1");
            int column = 0;
            foreach (string? value in values)
            {
                writer.WriteStartElement("c", ns);
                writer.WriteAttributeString("r", $"{(char)('A' + column)}{rowNumber}");
                writer.WriteAttributeString("t", "inlineStr");
                writer.WriteAttributeString("s", header ? "1" : "0");
                writer.WriteStartElement("is", ns);
                writer.WriteElementString("t", ns, value ?? string.Empty);
                writer.WriteEndElement();
                writer.WriteEndElement();
                column++;
            }
            writer.WriteEndElement();
        }

        private static void WriteTextEntry(ZipArchive archive, string path, string content)
        {
            ZipArchiveEntry entry = archive.CreateEntry(path, CompressionLevel.Fastest);
            using StreamWriter writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write(content.Trim());
        }
    }
}
