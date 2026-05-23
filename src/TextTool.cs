using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for TextTool.
	/// </summary>
	public class TextTool
		: Tool
	{
		private RenderArgs ra;
		private EditingMode mode;
		private ArrayList lines;
		private int linepos;
		private int textpos;
		private Point clickPoint;
		private Font font;
		private TextAlignment alignment;
		private IrregularSurface saved;
		private static int cursorInterval = 300;
		private bool pulseEnabled;
		private System.DateTime startTime;
		private bool lastPulseCursorState;

		#region Event Handlers

		private EventHandler fontChangedDelegate;
		private void FontChangedHandler(object sender, EventArgs a)
		{
			font = Workspace.Environment.FontInfo.CreateFont();
			if(mode != EditingMode.NotEditing)
			{
				RedrawText(true);
			}
		}

		private EventHandler alignmentChangedDelegate;
		private void AlignmentChangedHandler(object sender, EventArgs a)
		{
			alignment = Workspace.Environment.TextAlignment;
			if(mode != EditingMode.NotEditing)
			{
				RedrawText(true);
			}
		}

		private EventHandler brushChangedDelegate;
		private void BrushChangedHandler(object sender, EventArgs a)
		{
			if(mode != EditingMode.NotEditing)
			{
				RedrawText(true);
			}
		}

		private EventHandler antiAliasChangedDelegate;
		private void AntiAliasChangedHandler(object sender, EventArgs a)
		{
			if(mode != EditingMode.NotEditing)
			{
				RedrawText(true);
			}
		}

        private EventHandler foreColorChangedDelegate;
        private void ForeColorChangedHandler(object sender, EventArgs e)
        {
            if (mode != EditingMode.NotEditing)
            {
                RedrawText(true);
            }
        }

		#endregion

		#region Activation and De-Activation
		protected override void OnActivate()
		{
			base.OnActivate ();
			ra = new RenderArgs(((BitmapLayer)Workspace.ActiveLayer).Surface);
			mode = EditingMode.NotEditing;
			
			font = Workspace.Environment.FontInfo.CreateFont();
			alignment = Workspace.Environment.TextAlignment;

			Workspace.Environment.BrushInfoChanged += brushChangedDelegate;
			Workspace.Environment.FontInfoChanged += fontChangedDelegate;
			Workspace.Environment.TextAlignmentChanged += alignmentChangedDelegate;
			Workspace.Environment.AntiAliasingChanged += antiAliasChangedDelegate;
            Workspace.Environment.ForeColorChanged += foreColorChangedDelegate;
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate ();

			switch(mode)
			{
				case EditingMode.Editing: 
					SaveHistoryAction();	
					break;

				case EditingMode.EmptyEdit: 
					RedrawText(false); 
					break;

				case EditingMode.NotEditing: 
					break;

				default: 
					throw new InvalidEnumArgumentException("Invalid Editing Mode");
			}

			if(ra != null)
			{
				ra.Dispose();
				ra = null;
			}

			if(saved != null)
			{
				saved.Dispose();
				saved = null;
			}

			Workspace.Environment.BrushInfoChanged -= brushChangedDelegate;
			Workspace.Environment.FontInfoChanged -= fontChangedDelegate;
			Workspace.Environment.TextAlignmentChanged -= alignmentChangedDelegate;
			Workspace.Environment.AntiAliasingChanged -= antiAliasChangedDelegate;
            Workspace.Environment.ForeColorChanged -= foreColorChangedDelegate;

			StopEditing();
		}
		#endregion

		#region Start and Stop Editing
		private void StopEditing()
		{
			mode = EditingMode.NotEditing;
			pulseEnabled = false;
		}

		private void StartEditing()
		{
			linepos = 0;
			textpos = 0;
			lines = new ArrayList();
			lines.Add("");
			startTime = DateTime.Now;
			mode = EditingMode.EmptyEdit;
			pulseEnabled = true;
			//pulseCount = 0;
		}
		#endregion

		#region Special Keys

		private void PerformEnter()
		{
			string currentLine = (string)lines[linepos];

			if(textpos == currentLine.Length)
			{
				// If we are at the end of a line, insert an empty line at the next line
				lines.Insert(linepos + 1, "");	
			}
			else
			{
				lines.Insert(linepos + 1, currentLine.Substring(textpos, currentLine.Length - textpos));
				lines[linepos] = ((string)lines[linepos]).Substring(0, textpos);
			}
			linepos++;
			textpos = 0;
			//RedrawText(true);
		}

		private void PerformBackspace()
		{	
			if(textpos == 0 && linepos > 0)
			{
				int ntp = ((string)lines[linepos - 1]).Length;
				lines[linepos - 1] = ((string)lines[linepos - 1]) + ((string)lines[linepos]);
				lines.RemoveAt(linepos);
				linepos--;
				textpos = ntp;				
			}
			else if(textpos > 0)
			{
				string ln = (string)lines[linepos];

				// If we are at the end of a line, we don't need to place a compound string
				if(textpos == ln.Length)
				{
					lines[linepos] = ln.Substring(0, ln.Length - 1);
				}
				else
				{
					lines[linepos] = ln.Substring(0, textpos - 1) + ln.Substring(textpos);
				}					
				textpos--;
			}
			//RedrawText(true);
		}

		private void PerformControlBackspace()
		{
			if(textpos == 0 && linepos > 0)
			{
				PerformBackspace();
			}
			else if(textpos > 0)
			{
				string currentLine = (string)lines[linepos];
				int ntp = textpos;

				if(Char.IsLetterOrDigit(currentLine[ntp - 1]))
				{
					while(ntp > 0 && (Char.IsLetterOrDigit(currentLine[ntp - 1]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp--;
					}
				}
				else if(Char.IsWhiteSpace(currentLine[ntp - 1]))
				{
					while(ntp > 0 && (Char.IsWhiteSpace(currentLine[ntp - 1]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp--;
					}
				}
				else if(Char.IsPunctuation(currentLine[ntp - 1]))
				{
					while(ntp > 0 && (Char.IsPunctuation(currentLine[ntp - 1]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp--;
					}
				}
				else
				{
					ntp--;
				}
				lines[linepos] = currentLine.Substring(0, ntp) + currentLine.Substring(textpos);
				textpos = ntp;
			}
			//RedrawText(true);
		}


		private void PerformDelete()
		{
			// If the cursor is at the end of the text block
			if((linepos == lines.Count - 1) && (textpos == ((string)lines[lines.Count - 1]).Length))
			{
				return;
			}

			// End of a line, must merge strings
			else if(textpos == ((string)lines[linepos]).Length)
			{
				lines[linepos] = ((string)lines[linepos]) + ((string)lines[linepos + 1]);
				lines.RemoveAt(linepos + 1);
			}
			else // Middle of a line somewhere
			{
				lines[linepos] = ((string)lines[linepos]).Substring(0, textpos) + ((string)lines[linepos]).Substring(textpos + 1);
			}
			// Check for state change
			if(lines.Count == 1 && ((string)lines[0]) == "")
			{
				mode = EditingMode.EmptyEdit;
			}
			//RedrawText(true);
		}

		private void PerformControlDelete()
		{
			// If the cursor is at the end of the text block
			if((linepos == lines.Count - 1) && (textpos == ((string)lines[lines.Count - 1]).Length))
			{
				return;
			}

			// End of a line, must merge strings
			else if(textpos == ((string)lines[linepos]).Length)
			{
				lines[linepos] = ((string)lines[linepos]) + ((string)lines[linepos + 1]);
				lines.RemoveAt(linepos + 1);
			}
			else // Middle of a line somewhere
			{
				int ntp = textpos;
				string currentLine = (string)lines[linepos];

				if(Char.IsLetterOrDigit(currentLine[ntp]))
				{
					while(ntp < currentLine.Length && (Char.IsLetterOrDigit(currentLine[ntp]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						currentLine = currentLine.Remove(ntp, 1);
					}
				}
				else if(Char.IsWhiteSpace(currentLine[ntp]))
				{
					while(ntp < currentLine.Length && (Char.IsWhiteSpace(currentLine[ntp]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						currentLine = currentLine.Remove(ntp, 1);
					}
				}
				else if(Char.IsPunctuation(currentLine[ntp]))
				{
					while(ntp < currentLine.Length && (Char.IsPunctuation(currentLine[ntp]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						currentLine = currentLine.Remove(ntp, 1);
					}
				}
				else
				{
					ntp--;
				}
				lines[linepos] = currentLine;
				//lines[linepos] = ((string)lines[linepos]).Substring(0, textpos) + ((string)lines[linepos]).Substring(textpos + 1);
			}
			// Check for state change
			if(lines.Count == 1 && ((string)lines[0]) == "")
			{
				mode = EditingMode.EmptyEdit;
			}
		}

		private void PerformLeft()
		{
			if(textpos > 0)
			{
				textpos--;
				//return;
			}
			else if(textpos == 0 && linepos > 0)
			{
				linepos--;
				textpos = ((string)lines[linepos]).Length;
			}
			//RedrawText(true);
		}

		private void PerformControlLeft()
		{
			if(textpos > 0)
			{
				int ntp = textpos;
				string currentLine = (string)lines[linepos];

				if(Char.IsLetterOrDigit(currentLine[ntp - 1]))
				{
					while(ntp > 0 && (Char.IsLetterOrDigit(currentLine[ntp - 1]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp--;
					}
				}
				else if(Char.IsWhiteSpace(currentLine[ntp - 1]))
				{
					while(ntp > 0 && (Char.IsWhiteSpace(currentLine[ntp - 1]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp--;
					}
				}
				else if(ntp > 0 && Char.IsPunctuation(currentLine[ntp - 1]))
				{
					while(ntp > 0 && Char.IsPunctuation(currentLine[ntp - 1])) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp--;
					}
				}
				else
				{
					ntp--;
				}
				textpos = ntp;
			}
			else if(textpos == 0 && linepos > 0)
			{
				linepos--;
				textpos = ((string)lines[linepos]).Length;
			}
		}

		private void PerformRight()
		{
			if(textpos < ((string)lines[linepos]).Length)
			{
				textpos++;
			}
			else if(textpos == ((string)lines[linepos]).Length && linepos < lines.Count - 1)
			{
				linepos++;
				textpos = 0;
			}
			//RedrawText(true);
		}

		private void PerformControlRight()
		{
			if(textpos < ((string)lines[linepos]).Length)
			{
				int ntp = textpos;
				string currentLine = (string)lines[linepos];

				if(Char.IsLetterOrDigit(currentLine[ntp]))
				{
					while(ntp < currentLine.Length && (Char.IsLetterOrDigit(currentLine[ntp]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp++;
					}
				}
				else if(Char.IsWhiteSpace(currentLine[ntp]))
				{
					while(ntp < currentLine.Length && (Char.IsWhiteSpace(currentLine[ntp]))) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp++;
					}
				}
				else if(ntp > 0 && Char.IsPunctuation(currentLine[ntp]))
				{
					while(ntp < currentLine.Length && Char.IsPunctuation(currentLine[ntp])) /*|| Char.IsWhiteSpace(currentLine[ntp - 1]))*/
					{
						ntp++;
					}
				}
				else
				{
					ntp++;
				}
				textpos = ntp;
			}
			else if(textpos == ((string)lines[linepos]).Length && linepos < lines.Count - 1)
			{
				linepos++;
				textpos = 0;
			}
		}

		private void PerformUp()
		{
			PointF p = TextPositionToPoint(new Position(linepos, textpos));
			p.Y -= font.Height;
			Position np = PointToTextPosition(p);
			linepos = np.Line;
			textpos = np.Offset;

			//RedrawText(true);
		}

		private void PerformDown()
		{
            if(linepos == lines.Count - 1)
            {
            }
            else
            {
                PointF p = TextPositionToPoint(new Position(linepos, textpos));
                p.Y += font.Height;
                Position np = PointToTextPosition(p);
                linepos = np.Line;
                textpos = np.Offset;
            }
			//RedrawText(true);
		}

		#endregion

		#region String Measuring
		private Point GetUpperLeft(Size sz, int line)
		{

			Point p = clickPoint;
			p.Y = (int)(p.Y - (0.5 * sz.Height) + (line * sz.Height));

			switch(alignment)
			{
				case TextAlignment.Center:
					p.X = (int)(p.X - (0.5) * sz.Width); 
					break;

				case TextAlignment.Right: 
					p.X = (int)(p.X - sz.Width);		 
					break;
			}
			return p;
		}

		private Size StringSize(string s)
		{
			StringFormat format = (StringFormat)StringFormat.GenericDefault.Clone();
			format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
			SizeF sf = ra.Graphics.MeasureString(s, font, new PointF(0, 0), format);
			sf.Height = font.GetHeight();
            return Size.Ceiling(sf);
		}
		#endregion

		#region Private Position Class
		private class Position
		{
			// overload equals, gethashcode
			private int line;
			public int Line
			{
				get
				{
					return line;
				}
				set
				{
					if(value >= 0)
					{
						line = value;
					}
					else
					{
						line = 0;
					}
					//else throw new ArgumentOutOfRangeException("Line must be non-negative");
				}
			}

			private int offset;
			public int Offset
			{
				get
				{
					return offset;
				}
				set
				{
					if(value >= 0)
					{
						offset = value;
					}
					else
					{
						offset = 0;
					}
					//else throw new ArgumentOutOfRangeException("Offset must be non-negative");
				}
			}

			public Position(int ln, int off)
			{
				line = ln;
				offset = off;
			}
			
			public Position()
			{
				line = 0;
				offset = 0;
			}
		}
        #endregion


		private void SaveHistoryAction()
		{
			pulseEnabled = false;
			RedrawText(false);

			if(saved != null)
			{
				Region hitTest;
				if (Workspace.Environment.IsSelectionEmpty)
				{
					hitTest = new Region(ra.Bounds);
				}
				else
				{
					hitTest = Workspace.Environment.CreateSelectedRegion();
				}

				hitTest.Intersect(saved.Region);

				if (!hitTest.IsEmpty(ra.Graphics))
				{
					Workspace.History.PushNewAction(((BitmapLayer)Workspace.ActiveLayer).CreateHistoryAction(name, Image, saved));
				}

				hitTest.Dispose();
				saved.Dispose();
				saved = null;
			}
		}


		/// <summary>
		/// Redraws the Text on the screen
		/// </summary>
		/// <remarks>
		/// assumes that the <b>font</b> and the <b>alignment</b> are already set
		/// </remarks>
		/// <param name="cursorOn"></param>
		private void RedrawText(bool cursorOn)
		{
			if(saved != null)
			{
				saved.Draw(ra.Surface);	
				Workspace.ActiveLayer.Invalidate(saved.Region);
				saved.Dispose();
				saved = null;
			}

			#region Set Anti-Alias
			if(Workspace.Environment.AntiAliasing)
			{
				ra.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			else
			{
				ra.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
			}
			#endregion

			#region Set Clipping
			if(Workspace.Environment.IsSelectionEmpty)
			{
				ra.Graphics.SetClip(new Region(), CombineMode.Replace);
			}
			else
			{
				ra.Graphics.SetClip(Workspace.Environment.CreateSelectedRegion(), CombineMode.Replace);
			}
			#endregion

			// Save the Space behind the lines
			Rectangle[] rects = new Rectangle[lines.Count + 1];
			Point[] uls	= new Point[lines.Count + 1];

			// All Lines
			for(int i = 0; i <= lines.Count - 1; i++)
			{
				string line = (string)lines[i];
				Size sz = StringSize(line);
				Point upperLeft = GetUpperLeft(sz, i);
				uls[i] = upperLeft;
				Rectangle rect = new Rectangle(upperLeft, sz);
				rects[i] = rect;
			}

			// The Cursor Line
			string cursorLine = ((string)lines[linepos]).Substring(0, textpos);
			Size cursorLineSize;
			Point cursorUL;
			Rectangle cursorRect;
			bool emptyCursorLineFlag;

			if(cursorLine == "")
			{
				emptyCursorLineFlag = true;

				string fullLine = (string)lines[linepos];
				Size fullLineSize = StringSize(fullLine);

				cursorLineSize = StringSize(" ");
				cursorLineSize.Width = 2;
				cursorLineSize.Height = (int)(Math.Ceiling(font.GetHeight()));
				
				cursorUL = GetUpperLeft(fullLineSize, linepos);
				cursorRect = new Rectangle(cursorUL, cursorLineSize);
			}
			else
			{
				emptyCursorLineFlag = false;
				cursorLineSize = StringSize(cursorLine);
				cursorUL = uls[linepos];
				cursorRect = new Rectangle(cursorUL, cursorLineSize);
			}

			rects[lines.Count] = cursorRect;

			// Set the saved region
			using(Region reg = Utility.RectanglesToRegion(Utility.InflateRectangles(rects, (int)Math.Ceiling(font.Size))))
			{
				saved = new IrregularSurface(ra.Surface, reg);
			}

			// Draw the Lines
			using(Brush brush = Workspace.Environment.CreateBrush(false))
			{
				for(int i = 0; i < lines.Count; i++)
				{
					ra.Graphics.DrawString((string)lines[i], font, brush, uls[i]);
				}
			}

			// Draw the Cursor
			if(cursorOn)
			{			
				using(Pen cursorPen = new Pen(/*Color.Black*/ Workspace.Environment.ForeColor.ToColor(), 2))
				{
					if(emptyCursorLineFlag)
					{
						ra.Graphics.FillRectangle(cursorPen.Brush, cursorRect);
					}
					else
					{
						ra.Graphics.DrawLine(cursorPen, new Point(cursorRect.Right - 2, cursorRect.Top), new Point(cursorRect.Right - 2, cursorRect.Bottom));
					}
				}
			}

			Workspace.ActiveLayer.Invalidate(saved.Region);
            Workspace.Update();
		}


		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress (e);

			if(mode != EditingMode.NotEditing)
			{
				// Evaluate State Dependent Clauses
				if(mode == EditingMode.EmptyEdit)
				{
					mode = EditingMode.Editing;
				}

				if (!char.IsControl(e.KeyChar)) 
				{
					InsertCharIntoString(e.KeyChar);
					textpos++;
					RedrawText(true);
				}
			}
		}

		protected override void OnKeyPress(Keys keyData)
		{
			base.OnKeyPress (keyData);
			Keys key = keyData & Keys.KeyCode;
			Keys modifier = keyData & Keys.Modifiers;

			if(mode != EditingMode.NotEditing)
			{
				switch(key)
				{
					case Keys.Back: 
						if(modifier == Keys.Control)
						{
							PerformControlBackspace();
						}
						else
						{
							PerformBackspace();
						}
						break;

					case Keys.Delete:
						if(modifier == Keys.Control)
						{
							PerformControlDelete();
						}
						else
						{
							PerformDelete();
						}
						break;

					case Keys.Enter:  
						PerformEnter();		
						break;

					case Keys.Escape: 
						if(mode == EditingMode.Editing)
						{
							SaveHistoryAction();
						}
						StopEditing();
						break;

					case Keys.Left: 
						if(modifier == Keys.Control)
						{
							PerformControlLeft();
						}
						else
						{
							PerformLeft();
						}
						break;

					case Keys.Right: 
						if(modifier == Keys.Control)
						{
							PerformControlRight();
						}
						else
						{
							PerformRight();
						}
						break;

					case Keys.Up: 
						PerformUp();					
						break;

					case Keys.Down: 
						PerformDown();				
						break;

					case Keys.Home:
						if(modifier == Keys.Control)
						{
							linepos = 0;
						}
						textpos = 0;
						break;

					case Keys.End:
						if(modifier == Keys.Control)
						{
							linepos = lines.Count - 1;
						}
						textpos = ((string)lines[linepos]).Length;
						break;
				}

                this.startTime = DateTime.Now;
                RedrawText(true);
			}
		}

		PointF TextPositionToPoint(Position p)
		{
			PointF pf = new PointF(0,0);

			Size sz = StringSize(((string)lines[p.Line]).Substring(0, p.Offset));
			Size fullSz = StringSize((string)lines[p.Line]);

			switch(alignment)
			{
				case TextAlignment.Left: 
					pf = new PointF(clickPoint.X + sz.Width, clickPoint.Y + (sz.Height * p.Line));
					break;

				case TextAlignment.Center: 
					pf = new PointF(clickPoint.X + (sz.Width - (fullSz.Width/2)), clickPoint.Y + (sz.Height * p.Line));
					break;

				case TextAlignment.Right: 
					pf = new PointF(clickPoint.X + (sz.Width - fullSz.Width), clickPoint.Y + (sz.Height * p.Line));
					break;
					
				default: throw new InvalidEnumArgumentException("Invalid Alignment");
			}

			return pf;
		}

		int FindOffsetPosition(float offset, string line, int lno)
		{
			for(int i = 0; i < line.Length; i++)
			{
				PointF pf = TextPositionToPoint(new Position(lno, i));
				float dx = pf.X - clickPoint.X;
				if(dx >= offset)
				{
					return i;
				}
			}
			return line.Length;
		}


		Position PointToTextPosition(PointF pf)
		{
			float dx = pf.X - clickPoint.X;
			float dy = pf.Y - clickPoint.Y;
			int line = (int)Math.Floor(dy / font.Height);

			if(line < 0)
			{
				line = 0;
			}
			else if(line >= lines.Count)
			{
				line = lines.Count - 1;
			}

			int offset =  FindOffsetPosition(dx, (string)lines[line], line);
			Position p = new Position(line, offset);

			if(p.Offset >= ((string)lines[p.Line]).Length)
			{
				p.Offset = ((string)lines[p.Line]).Length;
			}

			return p;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown (e);

			if(e.Button == MouseButtons.Left)
			{
				if(saved != null)
				{
					RectangleF bounds = Utility.GetRegionBounds(saved.Region);
					if(Utility.IsPointInRectangle(new Point(e.X, e.Y), Utility.RoundRectangle(bounds)))
					{
						Position p = PointToTextPosition(new PointF(e.X, e.Y + (font.Height / 2)));
						linepos = p.Line;
						textpos = p.Offset;
						RedrawText(true);
						return;
					}
				}

				switch(mode)
				{
					case EditingMode.Editing   : SaveHistoryAction();
						StopEditing();
						break;

					case EditingMode.EmptyEdit : RedrawText(false); StopEditing(); 
						break;
				}

				clickPoint = Utility.GetPointFromMouseXY(e);
				StartEditing();
				RedrawText(true);
			}
		}
	
		protected override void OnPulse()
		{
			base.OnPulse ();

			if(!pulseEnabled)
			{
				return;
			}

			TimeSpan ts = (DateTime.Now - startTime);
			long ms = Utility.TicksToMs(ts.Ticks);
			
			bool pulseCursorState;

			if(0 == ((ms / cursorInterval) % 2))
			{
				pulseCursorState = true;
			}
			else
			{
				pulseCursorState = false;
			}

			if (pulseCursorState != lastPulseCursorState)
			{
				RedrawText(pulseCursorState);
				lastPulseCursorState = pulseCursorState;
			}
		}


		private void InsertCharIntoString(char c)
		{
			lines[linepos] = ((string)lines[linepos]).Insert(textpos, c.ToString());
		}

		public TextTool(DocumentWorkspace parent)
			: base(parent)
		{
			toolBarImage = Utility.GetImageResource("Icons.TextToolIcon.bmp");
			cursor = Cursors.IBeam;
			name = "Text";
			description = "Draws Text";

			fontChangedDelegate = new EventHandler(FontChangedHandler);
			alignmentChangedDelegate = new EventHandler(AlignmentChangedHandler);
			brushChangedDelegate = new EventHandler(BrushChangedHandler);
			antiAliasChangedDelegate = new EventHandler(AntiAliasChangedHandler);
            foreColorChangedDelegate = new EventHandler(ForeColorChangedHandler);
		}
	}
}
