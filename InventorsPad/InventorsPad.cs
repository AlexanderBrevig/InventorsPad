using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Tables;

namespace InventorsPad
{
    public class InventorsPad
    {
        private const int PROJECT_COUNT = 100;
        private const int LINECOUNT = 42;
        private const int FOOTER_HEIGHT = 20;
        private const int HEADEROFFSET = 30;
        private const int PAGE_NUMBER_HEIGHT = 10;
        private const int PROJECT_PAGE_START = 4;

        public InventorsPad()
        {
            margin.Top = unitCvtr.ConvertUnits(2.54f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Bottom = margin.Top;
            margin.Left = unitCvtr.ConvertUnits(2.54f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Right = margin.Left;

            String fontFileName = "segoeui.ttf";

            font = (PdfFontBase)new PdfTrueTypeFont(fontFileName, 13f);


            headerFont = (PdfFontBase)new PdfTrueTypeFont(fontFileName, 18f);

            small = (PdfFontBase)new PdfTrueTypeFont(fontFileName, 10f);

            PdfPageBase page = doc.Pages.Add(PdfPageSize.Letter, margin);

            page.Canvas.DrawString("DELETE THIS PAGE", font, defaultBrush, 0, 0);

            //AddFrontPage();
            AddAboutPage();
            AddTOC(30);
            AddPages();
            AddBlankNotes(30);
        }

        private void AddFrontPage()
        {
            PdfMargins margin = new PdfMargins();
            margin.All = 0;
            PdfPageBase page = doc.Pages.Add(PdfPageSize.Letter, margin);
            PdfImage img = PdfImage.FromFile("front.png");
            page.Canvas.DrawImage(img, 0, 0, page.Canvas.ClientSize.Width, page.Canvas.ClientSize.Height);
        }

        private void AddAboutPage()
        {

            PdfPageBase page = doc.Pages.Add(PdfPageSize.Letter, margin);

            Action<PdfPageBase, string, int, int, int> gen = (pg, title, x, y, height) => {
                int marg = 40;
                int lineOffset = 16;
                pg.Canvas.DrawString(title, font, defaultBrush, x, y + marg);
                if (height >= 0) {
                    pg.Canvas.DrawLine(defaultPen, x, y + marg + lineOffset, page.Canvas.ClientSize.Width - x, y + marg + lineOffset);
                }
                if (height > 0) {
                    pg.Canvas.DrawLine(defaultPen, x, y + marg + lineOffset, x, y + marg + lineOffset + height);
                    pg.Canvas.DrawLine(defaultPen, pg.Canvas.ClientSize.Width - x, y + marg + lineOffset, pg.Canvas.ClientSize.Width - x, y + marg + lineOffset + height);
                    pg.Canvas.DrawLine(defaultPen, x, y + marg + lineOffset + height, pg.Canvas.ClientSize.Width - x, y + marg + lineOffset + height);
                }
            };


            page.Canvas.DrawString("INVENTOR'S PAD", headerFont, defaultBrush, page.Canvas.ClientSize.Width / 2, 0, center);

            PdfPen linePen = new PdfPen(defaultBrush, 10);
            page.Canvas.DrawLine(linePen, 0, 30, page.Canvas.ClientSize.Width, 30);

            //page.Canvas.DrawString("inventorspad.com", font, defaultBrush, page.Canvas.ClientSize.Width, 50, right);

            gen(page, "PAD NO:", 0, 20, 0);
            //gen(page, "RETURN TO:", 0, 60, -1);
            page.Canvas.DrawString("RETURN TO:", font, defaultBrush, page.Canvas.ClientSize.Width / 2, 60 + 40, center);
            gen(page, "NAME:", 20, 80, 0);
            gen(page, "SURNAME:", 20, 100, 0);
            gen(page, "EMAIL:", 20, 140, 0);
            gen(page, "PHONE:", 20, 160, 0);
            gen(page, "ADDRESS:", 20, 200, 0);
            gen(page, "CITY/ZIP:", 20, 220, 0);
            gen(page, "COUNTRY:", 20, 240, 0);
            gen(page, "COMPANY:", 20, 280, 0);
            int h = 240;
            gen(page, "NOTES:", 0, 330, h);
            page.Canvas.DrawString("Open Source Notepad at inventorspad.com", small, defaultBrush, page.Canvas.ClientSize.Width, page.Canvas.ClientSize.Height - 10, right);
        }

        private void AddTOC(int blank)
        {
            PdfPageBase page = doc.Pages.Add(PdfPageSize.Letter, margin);

            page.Canvas.DrawString("TABLE OF CONTENTS", font, defaultBrush, 0, 0);

            String[][] dataSource = new String[PROJECT_COUNT + blank][];
            for (int i = 0; i < PROJECT_COUNT; i++) {
                if (i == 0) {
                    dataSource[i] = new String[] { "PROJECT\t\tTITLE" }; //
                } else {
                    string num = "" + i;
                    if (num.Length == 1) {
                        num = "0" + num;
                    }
                    dataSource[i] = new String[] { "\tP" + num + ":\t\t|" }; //
                }
            }
            for (int i = 0; i < blank; i++) {
                string num = "" + (i + 1);
                if (num.Length == 1) {
                    num = "0" + num;
                }
                dataSource[PROJECT_COUNT + i] = new String[] { "\tN" + num + ":\t\t|" };
            }

            PdfTable table = new PdfTable();
            table.Style.CellPadding = 2;
            table.Style.HeaderSource = PdfHeaderSource.Rows;
            table.Style.BorderPen = defaultPen;
            table.Style.DefaultStyle.BorderPen = defaultPen;
            table.Style.HeaderRowCount = 1;
            table.Style.ShowHeader = true;
            table.DataSource = dataSource;


            PdfLayoutResult result = table.Draw(page, 0, 40);
            PdfBrush brush2 = PdfBrushes.Gray;
        }

        private void AddPages()
        {
            int pages = doc.Pages.Count;
            if (pages % 2 == 1) {
                PdfPageBase page = doc.Pages.Add(PdfPageSize.Letter, margin);
                page.Canvas.DrawString("LEFT BLANK", font, defaultBrush, 0, 0);
            }
            for (int i = 0; i < PROJECT_COUNT; i++) {
                AddFigurePage();
                AddNotesPage();
            }
        }

        private void AddBlankNotes(int notes)
        {
            for (int i = 0; i < notes; i++) {
                PdfPageBase page = doc.Pages.Add(PdfPageSize.Letter, margin);
                AddProjectHeader(page, false);
                AddProjectFooter(page);
                AddPageNumber(page);
            }
        }

        private void AddFigurePage()
        {
            PdfPageBase page = doc.Pages.Add(PdfPageSize.Letter, margin);
            AddProjectHeader(page, true);
            float x0 = 0;
            float x1 = page.Canvas.ClientSize.Width - 0;
            float y0 = HEADEROFFSET + 20;
            float y1 = page.Canvas.ClientSize.Height - FOOTER_HEIGHT;
            page.Canvas.DrawLine(defaultPen, x0, y0, x1, y0);
            page.Canvas.DrawLine(defaultPen, x0, y0, x0, y1);
            page.Canvas.DrawLine(defaultPen, x1, y1, x0, y1);
            page.Canvas.DrawLine(defaultPen, x1, y1, x1, y0);
            AddProjectFooter(page);
            AddPageNumber(page);
        }

        private void AddNotesPage()
        {
            PdfPageBase page = doc.Pages.Add(PdfPageSize.Letter, margin);
            AddProjectHeader(page, false);

            String[][] dataSource = new String[LINECOUNT][];
            for (int j = 0; j < LINECOUNT; j++) {
                dataSource[j] = new String[] { " " };
            }

            PdfTable table = new PdfTable();
            table.Style.CellPadding = 2;
            table.Style.BorderPen = defaultPen;
            table.Style.DefaultStyle.BorderPen = defaultPen;
            table.Style.HeaderSource = PdfHeaderSource.Rows;
            table.Style.HeaderRowCount = 0;
            table.Style.ShowHeader = false;

            table.AllowCrossPages = false;
            table.DataSource = dataSource;

            var format = new PdfTableLayoutFormat();
            format.Layout = PdfLayoutType.OnePage;
            float width = (float)page.Canvas.ClientSize.Width - 0f;
            PdfLayoutResult result = table.Draw(page, 0, HEADEROFFSET);
            AddPageNumber(page);
        }

        private void AddProjectHeader(PdfPageBase page, bool showFullHeader)
        {
            int margin = 10;
            if (showFullHeader) {
                page.Canvas.DrawString("PROJECT:", headerFont, defaultBrush, 0, margin, left);
                page.Canvas.DrawString("TITLE:", headerFont, defaultBrush, 0, HEADEROFFSET-8);
                //page.Canvas.DrawString("REF:", font, brush, page.Canvas.ClientSize.Width / 2, HEADEROFFSET);
            }
            int pages = doc.Pages.Count - 5;
            int proj = (int)(Math.Floor(((doc.Pages.Count - 5) / 2) + 0.5f));
            page.Canvas.DrawString((proj <= PROJECT_COUNT ? "P" + proj : "N" + (pages - (PROJECT_COUNT*2) - 1)), headerFont, defaultBrush, page.Canvas.ClientSize.Width, margin, right);

            var pen = new PdfPen(defaultBrush, 3);
            page.Canvas.DrawLine(pen, 0, margin + 13, page.Canvas.ClientSize.Width, margin + 16);
        }

        private void AddProjectFooter(PdfPageBase page)
        {
            float y = page.Canvas.ClientSize.Height - FOOTER_HEIGHT + 10;

            page.Canvas.DrawString("SIGNATURE:", font, defaultBrush, 0, y, left);
            page.Canvas.DrawString("DATE:", font, defaultBrush, page.Canvas.ClientSize.Width - 200, y, right);
            var pen = new PdfPen(defaultBrush, 1);
            page.Canvas.DrawLine(pen, 0, y + 10, page.Canvas.ClientSize.Width, y + 10);
            /*
            page.Canvas.DrawString("PREV", font, brush, 0, y2, left);
            page.Canvas.DrawString("NEXT", font, brush, page.Canvas.ClientSize.Width, y2, right);*/
        }

        private void AddPageNumber(PdfPageBase page)
        {
            page.Canvas.DrawString("PAGE: " + doc.Pages.Count + "/" + ((PROJECT_COUNT * 2) + PROJECT_PAGE_START + 32), font, defaultBrush, page.Canvas.ClientSize.Width, page.Canvas.ClientSize.Height - PAGE_NUMBER_HEIGHT, right);
        }

        public void Save()
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var time = rgx.Replace(DateTime.Now.ToLongTimeString(), "");
            doc.SaveToFile("test" + time + ".pdf");
            doc.Close();
            System.Diagnostics.Process.Start("test" + time + ".pdf");
        }

        private PdfDocument doc = new PdfDocument();
        PdfMargins margin = new PdfMargins();
        PdfUnitConvertor unitCvtr = new PdfUnitConvertor();
        PdfFontBase font;
        PdfFontBase small;
        PdfFontBase headerFont;
        PdfStringFormat left = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
        PdfStringFormat justify = new PdfStringFormat(PdfTextAlignment.Justify, PdfVerticalAlignment.Middle);
        PdfStringFormat right = new PdfStringFormat(PdfTextAlignment.Right, PdfVerticalAlignment.Middle);
        PdfStringFormat center = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
        PdfPen defaultPen = PdfPens.DarkSlateGray;
        PdfBrush defaultBrush = PdfBrushes.DarkSlateGray;
    }
}
