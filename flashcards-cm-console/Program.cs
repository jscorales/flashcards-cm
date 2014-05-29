using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI;
using NPOI.XWPF;
using NPOI.XWPF.UserModel;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace flashcards_cm_console 
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FileStream fs = new FileStream(@"..\..\GMAT Flashcard 059.docx", FileMode.Open))
            {
                var doc = new XWPFDocument(fs);
                var tables = doc.Tables;
                foreach (var table in tables)
                {
                    ProcessTable(table);
                }
            }

            Console.ReadLine();
        }

        static void ProcessTable(XWPFTable table)
        {
            if (table.Rows.Count > 1)
            {
                var text = table.Rows[0].GetCell(0).GetText();
                if (string.IsNullOrEmpty(text))
                    return;

                switch (text.ToLower())
                {
                    case "card":
                        Console.WriteLine("<card id=\"{0}\">", table.Rows[1].GetCell(0).GetText());
                        break;
                    case "topic":
                        Console.WriteLine("<topic>{0}</topic>", table.Rows[1].GetCell(0).GetText());
                        break;
                    case "subtopic":
                        Console.WriteLine("<subtopic>{0}</subtopic>", table.Rows[1].GetCell(0).GetText());
                        break;
                    case "question":
                        Console.WriteLine("<question><div>{0}</div></question>", GetFormattedText(table.Rows[1].GetCell(0)));
                        break;
                    case "answer":
                        Console.WriteLine("<answer><div>{0}</div></answer>", GetFormattedText(table.Rows[1].GetCell(0)));
                        break;
                    case "author":
                        Console.WriteLine("</card>");
                        break;
                }

                
            }
        }

        static string GetFormattedText(XWPFTableCell tableCell)
        {
            var sb = new StringBuilder();
            var paras = tableCell.Paragraphs;
            foreach (var para in paras)
            {
                sb.Append("<p>");

                var runs = tableCell.Paragraphs[0].Runs;
                if (runs.Count > 0)
                {
                    foreach (var run in runs)
                    {
                        if (run.IsBold)
                        {
                            sb.AppendFormat("<b>{0}</b>", run.GetText(0));
                            continue;
                        }

                        if (run.IsItalic)
                        {
                            sb.AppendFormat("<i>{0}</i>", run.GetText(0));
                            continue;
                        }

                        if (run.Underline == UnderlinePatterns.Single)
                        {
                            sb.AppendFormat("<u>{0}</u>", run.GetText(0));
                            continue;
                        }

                        sb.Append(run.GetText(0));
                    }
                }
                else
                {
                    sb.Append(para.Text);
                }

                sb.Append("</p>");
            }


            return sb.ToString();
        }
    }
}
