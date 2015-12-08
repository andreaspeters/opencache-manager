// 
//  Copyright 2010  Kyle Campbell
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using Cairo;
using Gtk;
using Pango;
using System.Text;
using Mono.Unix;
using ocmengine;

namespace ocmgtk.printing
{


	public class CachePrinter
	{
		private PrintOperation m_print;
        private static double headerHeight = (10*72/25.4);
        private static double headerGap = (3*72/25.4);
        private static int pangoScale = 1024;
        private double m_fontSize = 10.0;
        private int m_linesPerPage;
        private string[] m_Lines;
        private int m_numLines;
        private int m_numPages;
		private Geocache m_cache;
		
		private bool m_IncludeQuickInfo = true;
		public bool IncludeQuickInfo
		{
			get { return m_IncludeQuickInfo;}
			set { m_IncludeQuickInfo = true;}
		}
		
		private bool m_IncludeShortDesc = true;
		public bool IncludeShortDec
		{
			get { return m_IncludeShortDesc;}
			set { m_IncludeShortDesc = value;}
		}
		
		private bool m_IncludeLongDesc = true;
		public bool IncludeLongDesc
		{
			get { return m_IncludeLongDesc;}
			set { m_IncludeLongDesc = value;}
		}

		private bool m_IncludeHint = true;
		public bool IncludeHint
		{
			get { return m_IncludeHint;}
			set { m_IncludeHint = value;}
		}
		
		private bool m_IncludeNotes = true;
		public bool IncludeNotes
		{
			get { return m_IncludeNotes;}
			set { m_IncludeNotes = true;}
		}
		
		public CachePrinter ()
		{
			m_print = new PrintOperation();
			m_print.BeginPrint += HandleM_printBeginPrint;
			m_print.DrawPage += HandleM_printDrawPage;
			m_print.EndPrint += HandleM_printEndPrint;
		}
		
		public void StartPrinting(Geocache cache, Window win)
		{
			m_cache = cache;
			m_print.Run(PrintOperationAction.PrintDialog, win);
		}
		

		void HandleM_printBeginPrint (object o, BeginPrintArgs args)
		{
			string contents;
            double height;

            // Get the Context of the Print Operation
            PrintContext context = args.Context;
           
            // Get the Height of the Context
            height = context.Height;
			
			StringBuilder builder = new StringBuilder();
         
            // From the FontSize and the Height of the Page determine the Lines available per page
            m_linesPerPage = (int)Math.Floor(height / (m_fontSize + 0.5)) - 4;
			int charsPerLine = 80;
           	contents = Utilities.HTMLtoText(m_cache.LongDesc);
			contents = GLib.Markup.EscapeText (contents);
			builder.Append("<b>");
			builder.Append(Utilities.getCoordString(m_cache.Lat, m_cache.Lon));
			builder.Append("</b>\n");
			builder.Append(Geocache.GetCTypeString(m_cache.TypeOfCache));
			builder.Append("\n");
			builder.Append(Catalog.GetString("<b>A cache by:</b>"));
			builder.Append(" ");
			builder.Append(m_cache.PlacedBy);
			builder.Append("\t");
			builder.Append(Catalog.GetString("<b>Hidden on:</b>"));
			builder.Append(" ");
			builder.Append(m_cache.Time.ToShortDateString());
			builder.Append("\n");
			builder.Append(Catalog.GetString("<b>Cache Size:</b>"));
			builder.Append(" ");
			builder.Append(m_cache.Container);
			builder.Append("\t");
			builder.Append(Catalog.GetString("<b>Difficulty:</b>"));
			builder.Append(" ");
			builder.Append(m_cache.Difficulty.ToString("0.0"));
			builder.Append("\t");
			builder.Append(Catalog.GetString("<b>Terrain:</b>"));
			builder.Append(" ");
			builder.Append(m_cache.Difficulty.ToString("0.0"));
			builder.Append("\n\n");
			if (!String.IsNullOrEmpty(m_cache.ShortDesc))
			{
				builder.Append(Catalog.GetString("<b>Short Description:</b>"));
				builder.Append("\n\n");
				WordWrapText(HTMLtoPango(m_cache.ShortDesc), builder, charsPerLine);
				builder.Append("\n\n");
			}
			builder.Append(Catalog.GetString("<b>Long Description:</b>"));
			builder.Append("\n\n");
			WordWrapText (HTMLtoPango(m_cache.LongDesc), builder, charsPerLine);
			builder.Append("\n\n");
			if (!String.IsNullOrEmpty(m_cache.Notes))
			{
				builder.Append(Catalog.GetString("<b>Notes:</b>"));
				builder.Append("\n\n");
				builder.Append(GLib.Markup.EscapeText(m_cache.Notes));
				builder.Append("\n\n");
			}
			if (!String.IsNullOrEmpty(m_cache.Hint))
			{
				builder.Append(Catalog.GetString("<b>Hint:</b>"));
				builder.Append("\n\n");
				string hint = Utilities.HTMLtoText(m_cache.Hint);
				hint = GLib.Markup.EscapeText(hint);
				WordWrapText(hint, builder, charsPerLine);
			}
			contents = builder.ToString();
            // Split the Content into seperate lines
            m_Lines = contents.Split('\n');
           
           
            // Get the Number of lines
            m_numLines = m_Lines.Length;
           
            // Calculate the Number of Pages by how many lines there are and how many lines are available per page
            m_numPages = (m_numLines - 1) / m_linesPerPage + 1;
           
            // Tell the Print Operation how many pages there are
            m_print.NPages = m_numPages;
		}
		
		private static void WordWrapText (string contents, StringBuilder builder, int charsPerLine)
		{
			int iPos = 0;
			for (int i=0; i < contents.Length; i++)
			{
				builder.Append(contents[i]);
				if (contents[i] == '\n')
					iPos = 0;
				if (iPos >= charsPerLine - 2)
				{
					if (char.IsWhiteSpace(contents[i]))
					{
						builder.Append('\n');
						iPos = 0;
					}
				}
				else
				{
					iPos ++;
				}
			}
		}
		
		void HandleM_printDrawPage (object o, DrawPageArgs args)
		{
			 // Create a Print Context from the Print Operation
            PrintContext context = args.Context;

            // Create a Cairo Context from the Print Context
            Cairo.Context cr = context.CairoContext;
           
            // Get the width of the Print Context
            double width = context.Width;

            // Create a rectangle to be used for the Content
            cr.Rectangle (0, 0, width, headerHeight);
            cr.SetSourceRGB (0.95, 0.95, 0.95);
            cr.FillPreserve ();

            // Create a Stroke to outline the Content
            cr.SetSourceRGB (0, 0, 0);
            cr.LineWidth = 1;
            cr.Stroke();
			
            // Create a Pango Layout for the Text
            Pango.Layout layout = context.CreatePangoLayout ();
			// Get the Text Height fromt the Height of the layout and the Height of the Page
          	int layoutWidth, layoutHeight;
            layout.GetSize (out layoutWidth, out layoutHeight);         
            double textHeight = (double)layoutHeight / (double)pangoScale;
			cr.MoveTo(5, (headerHeight - textHeight) / 2);
           
            // Set the Font and Font Size desired
            Pango.FontDescription desc = Pango.FontDescription.FromString ("sans 12");
            layout.FontDescription = desc;

            // Create a Header with the FileName and center it on the page
            layout.SetText (m_cache.Name + " : " + m_cache.CacheName);
			//layout.Width = (int) width *3;
            layout.Alignment = Pango.Alignment.Left;
			Pango.CairoHelper.ShowLayout (cr, layout);
		

           // cr.MoveTo (width/2, (headerHeight - textHeight) / 2);
          

            // Set the Page Number in the Footer with a right alignment
            string pageStr = String.Format (Catalog.GetString("Page {0} of {1}"), args.PageNr + 1, m_numPages);
            layout.SetText (pageStr);
            layout.Alignment = Pango.Alignment.Right;

            cr.MoveTo (width - 75, (headerHeight - textHeight) / 2);
            Pango.CairoHelper.ShowLayout (cr, layout);

            // Create a new Pango Layout for the Content
            layout = null;
            layout = context.CreatePangoLayout ();

            // Set the Description of the Content
            desc = Pango.FontDescription.FromString ("sans");
            desc.Size = (int)(m_fontSize * pangoScale);
            layout.FontDescription = desc;
           
            // Move to the beginning of the Content, which is after the Header Height and Gap
            cr.MoveTo (0, headerHeight + headerGap);
           
            int line = args.PageNr * m_linesPerPage;
							
            // Draw the lines on the page according to how many lines there are left and how many lines can fit on the page
            for (int i=0; i < m_linesPerPage && line < m_numLines; i++)
            {
              layout.SetMarkup (m_Lines[line].TrimStart());
              Pango.CairoHelper.ShowLayout (cr, layout);
              cr.RelMoveTo (0, m_fontSize + 0.5);
              line++;
            }
           
            layout = null;
			context.Dispose();
		}
		
		void HandleM_printEndPrint (object o, EndPrintArgs args)
		{
		
		}
		
		
		private string HTMLtoPango(string str)
		{
			str = Utilities.HTMLtoText(str);
			return GLib.Markup.EscapeText(str);
		}
	}
}
