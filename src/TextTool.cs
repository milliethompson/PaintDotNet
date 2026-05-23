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
        private enum EditingMode
        {
            NotEditing,
            EmptyEdit,
            Editing
        }
        
        private RenderArgs ra;
        private EditingMode mode;
        private ArrayList lines;
        private int linePos;
        private int textPos;
        private Point clickPoint;
        private Font font;
        private TextAlignment alignment;
        private IrregularSurface saved;
        private const int cursorInterval = 300;
        private bool pulseEnabled;
        private System.DateTime startTime;
        private bool lastPulseCursorState;

        #region Event Handlers

        private EventHandler fontChangedDelegate;
        private void FontChangedHandler(object sender, EventArgs a)
        {
            font = Workspace.Environment.FontInfo.CreateFont();
            if (mode != EditingMode.NotEditing)
            {
                RedrawText(true);
            }
        }

        private EventHandler alignmentChangedDelegate;
        private void AlignmentChangedHandler(object sender, EventArgs a)
        {
            alignment = Workspace.Environment.TextAlignment;
            if (mode != EditingMode.NotEditing)
            {
                RedrawText(true);
            }
        }

        private EventHandler brushChangedDelegate;
        private void BrushChangedHandler(object sender, EventArgs a)
        {
            if (mode != EditingMode.NotEditing)
            {
                RedrawText(true);
            }
        }

        private EventHandler antiAliasChangedDelegate;
        private void AntiAliasChangedHandler(object sender, EventArgs a)
        {
            if (mode != EditingMode.NotEditing)
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

            switch (mode)
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

            if (ra != null)
            {
                ra.Dispose();
                ra = null;
            }

            if (saved != null)
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
            linePos = 0;
            textPos = 0;
            lines = new ArrayList();
            lines.Add("");
            startTime = DateTime.Now;
            mode = EditingMode.EmptyEdit;
            pulseEnabled = true;
        }
        #endregion

        #region Special Keys

        private void PerformEnter()
        {
            string currentLine = (string)lines[linePos];

            if (textPos == currentLine.Length)
            {   // If we are at the end of a line, insert an empty line at the next line
                lines.Insert(linePos + 1, "");  
            }
            else
            {
                lines.Insert(linePos + 1, currentLine.Substring(textPos, currentLine.Length - textPos));
                lines[linePos] = ((string)lines[linePos]).Substring(0, textPos);
            }

            linePos++;
            textPos = 0;
        }

        private void PerformBackspace()
        {   
            if (textPos == 0 && linePos > 0)
            {
                int ntp = ((string)lines[linePos - 1]).Length;

                lines[linePos - 1] = ((string)lines[linePos - 1]) + ((string)lines[linePos]);
                lines.RemoveAt(linePos);
                linePos--;
                textPos = ntp;              
            }
            else if (textPos > 0)
            {
                string ln = (string)lines[linePos];

                // If we are at the end of a line, we don't need to place a compound string
                if (textPos == ln.Length)
                {
                    lines[linePos] = ln.Substring(0, ln.Length - 1);
                }
                else
                {
                    lines[linePos] = ln.Substring(0, textPos - 1) + ln.Substring(textPos);
                }                   

                textPos--;
            }
        }

        private void PerformControlBackspace()
        {
            if (textPos == 0 && linePos > 0)
            {
                PerformBackspace();
            }
            else if (textPos > 0)
            {
                string currentLine = (string)lines[linePos];
                int ntp = textPos;

                if (Char.IsLetterOrDigit(currentLine[ntp - 1]))
                {
                    while (ntp > 0 && (Char.IsLetterOrDigit(currentLine[ntp - 1])))
                    {
                        ntp--;
                    }
                }
                else if (Char.IsWhiteSpace(currentLine[ntp - 1]))
                {
                    while (ntp > 0 && (Char.IsWhiteSpace(currentLine[ntp - 1])))
                    {
                        ntp--;
                    }
                }
                else if (Char.IsPunctuation(currentLine[ntp - 1]))
                {
                    while (ntp > 0 && (Char.IsPunctuation(currentLine[ntp - 1])))
                    {
                        ntp--;
                    }
                }
                else
                {
                    ntp--;
                }

                lines[linePos] = currentLine.Substring(0, ntp) + currentLine.Substring(textPos);
                textPos = ntp;
            }
        }


        private void PerformDelete()
        {   
            // Where are we?!
            if ((linePos == lines.Count - 1) && (textPos == ((string)lines[lines.Count - 1]).Length))
            {   // If the cursor is at the end of the text block
                return;
            }
            else if (textPos == ((string)lines[linePos]).Length)
            {   // End of a line, must merge strings
                lines[linePos] = ((string)lines[linePos]) + ((string)lines[linePos + 1]);
                lines.RemoveAt(linePos + 1);
            }
            else 
            {   // Middle of a line somewhere
                lines[linePos] = ((string)lines[linePos]).Substring(0, textPos) + ((string)lines[linePos]).Substring(textPos + 1);
            }

            // Check for state change
            if (lines.Count == 1 && ((string)lines[0]) == "")
            {
                mode = EditingMode.EmptyEdit;
            }
        }

        private void PerformControlDelete()
        {
            // where are we?!
            if ((linePos == lines.Count - 1) && (textPos == ((string)lines[lines.Count - 1]).Length))
            {   // If the cursor is at the end of the text block
                return;
            }
            else if (textPos == ((string)lines[linePos]).Length)
            {   // End of a line, must merge strings
                lines[linePos] = ((string)lines[linePos]) + ((string)lines[linePos + 1]);
                lines.RemoveAt(linePos + 1);
            }
            else 
            {   // Middle of a line somewhere
                int ntp = textPos;
                string currentLine = (string)lines[linePos];

                if (Char.IsLetterOrDigit(currentLine[ntp]))
                {
                    while (ntp < currentLine.Length && (Char.IsLetterOrDigit(currentLine[ntp])))
                    {
                        currentLine = currentLine.Remove(ntp, 1);
                    }
                }
                else if (Char.IsWhiteSpace(currentLine[ntp]))
                {
                    while (ntp < currentLine.Length && (Char.IsWhiteSpace(currentLine[ntp])))
                    {
                        currentLine = currentLine.Remove(ntp, 1);
                    }
                }
                else if (Char.IsPunctuation(currentLine[ntp]))
                {
                    while (ntp < currentLine.Length && (Char.IsPunctuation(currentLine[ntp])))
                    {
                        currentLine = currentLine.Remove(ntp, 1);
                    }
                }
                else
                {
                    ntp--;
                }

                lines[linePos] = currentLine;
            }

            // Check for state change
            if (lines.Count == 1 && ((string)lines[0]) == "")
            {
                mode = EditingMode.EmptyEdit;
            }
        }

        private void PerformLeft()
        {
            if (textPos > 0)
            {
                textPos--;
            }
            else if (textPos == 0 && linePos > 0)
            {
                linePos--;
                textPos = ((string)lines[linePos]).Length;
            }
        }

        private void PerformControlLeft()
        {
            if (textPos > 0)
            {
                int ntp = textPos;
                string currentLine = (string)lines[linePos];

                if (Char.IsLetterOrDigit(currentLine[ntp - 1]))
                {
                    while (ntp > 0 && (Char.IsLetterOrDigit(currentLine[ntp - 1])))
                    {
                        ntp--;
                    }
                }
                else if (Char.IsWhiteSpace(currentLine[ntp - 1]))
                {
                    while (ntp > 0 && (Char.IsWhiteSpace(currentLine[ntp - 1])))
                    {
                        ntp--;
                    }
                }
                else if (ntp > 0 && Char.IsPunctuation(currentLine[ntp - 1]))
                {
                    while (ntp > 0 && Char.IsPunctuation(currentLine[ntp - 1]))
                    {
                        ntp--;
                    }
                }
                else
                {
                    ntp--;
                }

                textPos = ntp;
            }
            else if (textPos == 0 && linePos > 0)
            {
                linePos--;
                textPos = ((string)lines[linePos]).Length;
            }
        }

        private void PerformRight()
        {
            if (textPos < ((string)lines[linePos]).Length)
            {
                textPos++;
            }
            else if (textPos == ((string)lines[linePos]).Length && linePos < lines.Count - 1)
            {
                linePos++;
                textPos = 0;
            }
        }

        private void PerformControlRight()
        {
            if (textPos < ((string)lines[linePos]).Length)
            {
                int ntp = textPos;
                string currentLine = (string)lines[linePos];

                if (Char.IsLetterOrDigit(currentLine[ntp]))
                {
                    while (ntp < currentLine.Length && (Char.IsLetterOrDigit(currentLine[ntp])))
                    {
                        ntp++;
                    }
                }
                else if (Char.IsWhiteSpace(currentLine[ntp]))
                {
                    while (ntp < currentLine.Length && (Char.IsWhiteSpace(currentLine[ntp])))
                    {
                        ntp++;
                    }
                }
                else if (ntp > 0 && Char.IsPunctuation(currentLine[ntp]))
                {
                    while (ntp < currentLine.Length && Char.IsPunctuation(currentLine[ntp]))
                    {
                        ntp++;
                    }
                }
                else
                {
                    ntp++;
                }

                textPos = ntp;
            }
            else if (textPos == ((string)lines[linePos]).Length && linePos < lines.Count - 1)
            {
                linePos++;
                textPos = 0;
            }
        }

        private void PerformUp()
        {
            PointF p = TextPositionToPoint(new Position(linePos, textPos));
            p.Y -= font.Height;
            Position np = PointToTextPosition(p);
            linePos = np.Line;
            textPos = np.Offset;
        }

        private void PerformDown()
        {
            if (linePos == lines.Count - 1)
            {
                // last line -> don't do squat
            }
            else
            {
                PointF p = TextPositionToPoint(new Position(linePos, textPos));
                p.Y += font.Height;
                Position np = PointToTextPosition(p);
                linePos = np.Line;
                textPos = np.Offset;
            }
        }

        #endregion

        #region String Measuring
        private Point GetUpperLeft(Size sz, int line)
        {
            Point p = clickPoint;
            p.Y = (int)(p.Y - (0.5 * sz.Height) + (line * sz.Height));

            switch (alignment)
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
            private int line;
            public int Line
            {
                get
                {
                    return line;
                }
                set
                {
                    if (value >= 0)
                    {
                        line = value;
                    }
                    else
                    {
                        line = 0;
                    }
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
                    if (value >= 0)
                    {
                        offset = value;
                    }
                    else
                    {
                        offset = 0;
                    }
                }
            }

            public Position(int ln, int off)
            {
                line = ln;
                offset = off;
            }
        }
        #endregion


        private void SaveHistoryAction()
        {
            pulseEnabled = false;
            RedrawText(false);

            if (saved != null)
            {
                PdnRegion hitTest;

                if (Workspace.Environment.IsSelectionEmpty)
                {
                    hitTest = new PdnRegion(ra.Bounds);
                }
                else
                {
                    hitTest = Workspace.Environment.CreateSelectedRegion();
                }

                hitTest.Intersect(saved.Region);

                if (!hitTest.IsEmpty())
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
            if (saved != null)
            {
                saved.Draw(ra.Surface); 
                Workspace.ActiveLayer.Invalidate(saved.Region);
                saved.Dispose();
                saved = null;
            }

            #region Set Anti-Alias
            if (Workspace.Environment.AntiAliasing)
            {
                ra.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            }
            else
            {
                ra.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            }
            #endregion

            #region Set Clipping
            if (Workspace.Environment.IsSelectionEmpty)
            {
                ra.Graphics.SetClip(new PdnRegion(), CombineMode.Replace);
            }
            else
            {
                ra.Graphics.SetClip(Workspace.Environment.CreateSelectedRegion(), CombineMode.Replace);
            }
            #endregion

            // Save the Space behind the lines
            Rectangle[] rects = new Rectangle[lines.Count + 1];
            Point[] uls = new Point[lines.Count + 1];

            // All Lines
            for (int i = 0; i <= lines.Count - 1; i++)
            {
                string line = (string)lines[i];
                Size sz = StringSize(line);
                Point upperLeft = GetUpperLeft(sz, i);
                uls[i] = upperLeft;
                Rectangle rect = new Rectangle(upperLeft, sz);
                rects[i] = rect;
            }

            // The Cursor Line
            string cursorLine = ((string)lines[linePos]).Substring(0, textPos);
            Size cursorLineSize;
            Point cursorUL;
            Rectangle cursorRect;
            bool emptyCursorLineFlag;

            if (cursorLine == "")
            {
                emptyCursorLineFlag = true;

                string fullLine = (string)lines[linePos];
                Size fullLineSize = StringSize(fullLine);

                cursorLineSize = new Size(2, (int)(Math.Ceiling(font.GetHeight())));
                
                cursorUL = GetUpperLeft(fullLineSize, linePos);
                cursorRect = new Rectangle(cursorUL, cursorLineSize);
            }
            else
            {
                emptyCursorLineFlag = false;
                cursorLineSize = StringSize(cursorLine);
                cursorUL = uls[linePos];
                cursorRect = new Rectangle(cursorUL, cursorLineSize);
            }

            rects[lines.Count] = cursorRect;

            // Set the saved region
            using (PdnRegion reg = Utility.RectanglesToRegion(Utility.InflateRectangles(rects, (int)Math.Ceiling(font.Size))))
            {
                saved = new IrregularSurface(ra.Surface, reg);
            }

            // Draw the Lines
            using (Brush brush = Workspace.Environment.CreateBrush(false))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    ra.Graphics.DrawString((string)lines[i], font, brush, uls[i]);
                }
            }

            // Draw the Cursor
            if (cursorOn)
            {           
                using (Pen cursorPen = new Pen(Workspace.Environment.ForeColor.ToColor(), 2))
                {
                    if (emptyCursorLineFlag)
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

            if (mode != EditingMode.NotEditing)
            {
                if (mode == EditingMode.EmptyEdit)
                {
                    mode = EditingMode.Editing;
                }

                if (!char.IsControl(e.KeyChar)) 
                {
                    InsertCharIntoString(e.KeyChar);
                    textPos++;
                    RedrawText(true);
                }
            }
        }

        protected override void OnKeyPress(Keys keyData)
        {
            base.OnKeyPress (keyData);

            Keys key = keyData & Keys.KeyCode;
            Keys modifier = keyData & Keys.Modifiers;

            if (mode != EditingMode.NotEditing)
            {
                switch (key)
                {
                    case Keys.Back: 
                        if (modifier == Keys.Control)
                        {
                            PerformControlBackspace();
                        }
                        else
                        {
                            PerformBackspace();
                        }

                        break;

                    case Keys.Delete:
                        if (modifier == Keys.Control)
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
                        if (mode == EditingMode.Editing)
                        {
                            SaveHistoryAction();
                        }
                        else if (mode == EditingMode.EmptyEdit)
                        {
                            RedrawText(false);
                        }

                        StopEditing();
                        break;

                    case Keys.Left: 
                        if (modifier == Keys.Control)
                        {
                            PerformControlLeft();
                        }
                        else
                        {
                            PerformLeft();
                        }

                        break;

                    case Keys.Right: 
                        if (modifier == Keys.Control)
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
                        if (modifier == Keys.Control)
                        {
                            linePos = 0;
                        }

                        textPos = 0;
                        break;

                    case Keys.End:
                        if (modifier == Keys.Control)
                        {
                            linePos = lines.Count - 1;
                        }

                        textPos = ((string)lines[linePos]).Length;
                        break;
                }

                this.startTime = DateTime.Now;

                if (this.mode != EditingMode.NotEditing)
                {
                    RedrawText(true);
                }
            }
        }

        PointF TextPositionToPoint(Position p)
        {
            PointF pf = new PointF(0,0);

            Size sz = StringSize(((string)lines[p.Line]).Substring(0, p.Offset));
            Size fullSz = StringSize((string)lines[p.Line]);

            switch (alignment)
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
                    
                default: 
                    throw new InvalidEnumArgumentException("Invalid Alignment");
            }

            return pf;
        }

        int FindOffsetPosition(float offset, string line, int lno)
        {
            for (int i = 0; i < line.Length; i++)
            {
                PointF pf = TextPositionToPoint(new Position(lno, i));
                float dx = pf.X - clickPoint.X;

                if (dx >= offset)
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

            if (line < 0)
            {
                line = 0;
            }
            else if (line >= lines.Count)
            {
                line = lines.Count - 1;
            }

            int offset =  FindOffsetPosition(dx, (string)lines[line], line);
            Position p = new Position(line, offset);

            if (p.Offset >= ((string)lines[p.Line]).Length)
            {
                p.Offset = ((string)lines[p.Line]).Length;
            }

            return p;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);

            if (e.Button == MouseButtons.Left)
            {
                if (saved != null)
                {
                    Rectangle bounds = Utility.GetRegionBounds(saved.Region);

                    if (Utility.IsPointInRectangle(new Point(e.X, e.Y), bounds))
                    {
                        Position p = PointToTextPosition(new PointF(e.X, e.Y + (font.Height / 2)));
                        linePos = p.Line;
                        textPos = p.Offset;
                        RedrawText(true);
                        return;
                    }
                }

                switch (mode)
                {
                    case EditingMode.Editing:
                        SaveHistoryAction();
                        StopEditing();
                        break;

                    case EditingMode.EmptyEdit:
                        RedrawText(false); 
                        StopEditing(); 
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

            if (!pulseEnabled)
            {
                return;
            }

            TimeSpan ts = (DateTime.Now - startTime);
            long ms = Utility.TicksToMs(ts.Ticks);
            
            bool pulseCursorState;

            if (0 == ((ms / cursorInterval) % 2))
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

        protected override void OnPasteQuery(IDataObject data, out bool canHandle)
        {
            base.OnPasteQuery(data, out canHandle);

            if (data.GetDataPresent(DataFormats.StringFormat, true) &&
                this.Active &&
                this.mode != EditingMode.NotEditing)
            {
                canHandle = true;
            }
        }

        protected override void OnPaste(IDataObject data, out bool handled)
        {
            base.OnPaste (data, out handled);

            if (data.GetDataPresent(DataFormats.StringFormat, true) &&
                this.Active &&
                this.mode != EditingMode.NotEditing)
            {
                string text = (string)data.GetData(DataFormats.StringFormat, true);

                foreach (char c in text)
                {
                    if (c == '\n')
                    {
                        this.PerformEnter();
                    }
                    else
                    {
                        this.PerformKeyPress(new KeyPressEventArgs(c));
                    }
                }

                handled = true;
            }
        }

        private void InsertCharIntoString(char c)
        {
            lines[linePos] = ((string)lines[linePos]).Insert(textPos, c.ToString());
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
