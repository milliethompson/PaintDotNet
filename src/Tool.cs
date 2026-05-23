/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates the functionality for a tool that goes in the main window's toolbar
    /// and that affects the Document.
    /// A Tool should only emit a HistoryAction when it actually modifies the canvas.
    /// So, for instance, if the user draws a line but that line doesn't fall within
    /// the canvas (like if the seleciton region excludes it), then since the user
    /// hasn't really done anything there should be no HistoryAction emitted.
    /// </summary>
    /// <remarks>
    /// A bit about the eventing model:
    /// * Perform[Event]() methods are ALWAYS used to trigger the events. This can be called by
    ///   either instance methods or outside (client) callers.
    /// * [Event]() methods are called first by Perform[Event](). This gives the base Tool class a
    ///   first chance at handling the event. These methods are private and non-overridable.
    /// * On[Event]() methods are then called by [Event]() if necessary, and should be overrided 
    ///   as necessary by derived classes. Always call the base implementation unless the
    ///   documentation says otherwise. The base implementation gives the Tool a chance to provide
    ///   default, overridable behavior for an event.
    /// </remarks>
    public class Tool
        : IDisposable
    {
        private Image toolBarImage;
        private Cursor cursor;
        private ToolInfo toolInfo;
        private int mouseDown = 0; // incremented for every MouseDown, decremented for every MouseUp
        private int ignoreMouseMove = 0; // when >0, MouseMove is ignored and then this is decremented

        protected Cursor handCursor;
        protected Cursor handCursorMouseDown;
        protected Cursor handCursorInvalid;
        private Cursor panOldCursor;
        private Point lastMouseXY;
        private Point lastPanMouseXY;
        private bool panMode = false; // 'true' when the user is holding down the spacebar
        private bool panTracking = false; // 'true' when panMode is true, and when the mouse is down (which is when MouseMove should do panning)
        private MoveNubRenderer trackingNub = null; // when we are in pan-tracking mode, we draw this in the center of the screen

        private DocumentWorkspace workspace;
        private bool active = false;
        protected bool autoScroll = true;
        private Hashtable keysThatAreDown = new Hashtable();
        private MouseButtons lastButton = MouseButtons.None;
        private Surface scratchSurface;
        private PdnRegion saveRegion;
#if DEBUG
        private bool haveClearedScratch = false;
#endif

        private int mouseEnter; // increments on MouseEnter, decrements on MouseLeave. The MouseLeave event is ONLY raised when this value decrements to 0, and MouseEnter is ONLY raised when this value increments to 1

        protected Surface ScratchSurface
        {
            get
            {
#if DEBUG
                if (!haveClearedScratch)
                {
                    scratchSurface.Clear(ColorBgra.FromBgra(64, 128, 192, 128));
                    haveClearedScratch = true;
                }
#endif

                return scratchSurface;
            }
        }

        public void ClearSavedMemory()
        {
            this.savedTiles = null;
        }

        public void ClearSavedRegion()
        {
            if (this.saveRegion != null)
            {
                this.saveRegion.Dispose();
                this.saveRegion = null;
            }
        }

        public void RestoreRegion(PdnRegion region)
        {
            if (region != null)
            {
                BitmapLayer activeLayer = (BitmapLayer)Workspace.ActiveLayer;
                activeLayer.Surface.CopySurface(this.ScratchSurface, region);
                activeLayer.Invalidate(region);
            }
        }

        public void RestoreSavedRegion()
        {
            if (this.saveRegion != null)
            {
                BitmapLayer activeLayer = (BitmapLayer)Workspace.ActiveLayer;
                activeLayer.Surface.CopySurface(this.ScratchSurface, this.saveRegion);
                activeLayer.Invalidate(this.saveRegion);
                this.saveRegion.Dispose();
                this.saveRegion = null;
            }
        }

        private const int saveTileGranularity = 32;
        private BitVector2D savedTiles;

        public void SaveRegion(PdnRegion saveMeRegion, Rectangle saveMeBounds)
        {
            BitmapLayer activeLayer = (BitmapLayer)Workspace.ActiveLayer;

            if (savedTiles == null)
            {
                savedTiles = new BitVector2D(
                    (activeLayer.Width + saveTileGranularity - 1) / saveTileGranularity,
                    (activeLayer.Height + saveTileGranularity - 1) / saveTileGranularity);

                savedTiles.Clear(false);
            }

            Rectangle regionBounds;
            if (saveMeRegion == null)
            {
                regionBounds = saveMeBounds;
            }
            else
            {
                regionBounds = saveMeRegion.GetBoundsInt();
            }

            Rectangle bounds = Rectangle.Union(regionBounds, saveMeBounds);
            bounds.Intersect(activeLayer.Bounds);

            int leftTile = bounds.Left / saveTileGranularity;
            int topTile = bounds.Top / saveTileGranularity;
            int rightTile = (bounds.Right - 1) / saveTileGranularity;
            int bottomTile = (bounds.Bottom - 1) / saveTileGranularity;

            for (int tileY = topTile; tileY <= bottomTile; ++tileY)
            {
                Rectangle rowAccumBounds = Rectangle.Empty;

                for (int tileX = leftTile; tileX <= rightTile; ++tileX)
                {
                    if (!savedTiles.Get(tileX, tileY))
                    {
                        Rectangle tileBounds = new Rectangle(tileX * saveTileGranularity, tileY * saveTileGranularity,
                            saveTileGranularity, saveTileGranularity);

                        tileBounds.Intersect(activeLayer.Bounds);

                        if (rowAccumBounds == Rectangle.Empty)
                        {
                            rowAccumBounds = tileBounds;
                        }
                        else
                        {
                            rowAccumBounds = Rectangle.Union(rowAccumBounds, tileBounds);
                        }

                        savedTiles.Set(tileX, tileY, true);
                    }
                    else
                    {
                        if (rowAccumBounds != Rectangle.Empty)
                        {
                            using (Surface dst = ScratchSurface.CreateWindow(rowAccumBounds),
                                           src = activeLayer.Surface.CreateWindow(rowAccumBounds))
                            {
                                dst.CopySurface(src);
                            }

                            rowAccumBounds = Rectangle.Empty;
                        }
                    }
                }

                if (rowAccumBounds != Rectangle.Empty)
                {
                    using (Surface dst = ScratchSurface.CreateWindow(rowAccumBounds),
                                   src = activeLayer.Surface.CreateWindow(rowAccumBounds))
                    {
                        dst.CopySurface(src);
                    }

                    rowAccumBounds = Rectangle.Empty;
                }
            }

            if (this.saveRegion != null)
            {
                this.saveRegion.Dispose();
                this.saveRegion = null;
            }

            if (saveMeRegion != null)
            {
                this.saveRegion = saveMeRegion.Clone();
            }
        }

        private sealed class KeyTimeInfo
        {
            public DateTime KeyDownTime;
            public DateTime LastKeyPressPulse;
            private int repeats = 0;

            public int Repeats 
            {
                get 
                {
                    return repeats;
                }

                set 
                {
                    repeats = value;
                }
            }

            public KeyTimeInfo()
            {
                KeyDownTime = DateTime.Now;
                LastKeyPressPulse = KeyDownTime;
            }
        }

        /// <summary>
        /// Tells you whether the tool is "active" or not. If the tool is not active
        /// it is not safe to call any other method besides PerformActivate. All
        /// properties are safe to get values from.
        /// </summary>
        public bool Active
        {
            get
            {
                return active;
            }
        }

        /// <summary>
        /// Returns true if the Tool has the input focus, or false if it does not.
        /// </summary>
        /// <remarks>
        /// This is used, for instanced, by the Text Tool so that it doesn't blink the
        /// cursor unless it's actually going to do something in response to your
        /// keyboard input!
        /// </remarks>
        public bool Focused
        {
            get
            {
                return Workspace.DocumentView.Focused;
            }
        }

        public bool IsMouseDown
        {
            get
            {
                return this.mouseDown > 0;
            }
        }

        /// <summary>
        /// Gets a flag that determines whether the Tool is deactivated while the current
        /// layer is changing, and then reactivated afterwards.
        /// </summary>
        /// <remarks>
        /// This property is queried every time the ActiveLayer property of DocumentWorkspace
        /// is changed. If false is returned, then the tool is not deactivated during the
        /// layer change and must manually maintain coherency.
        /// </remarks>
        public virtual bool DeactivateOnLayerChange
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Tells you which keys are pressed
        /// </summary>
        public Keys ModifierKeys
        {
            get
            {
                return Control.ModifierKeys;
            }
        }

        /// <summary>
        /// Represents the Image that is displayed in the toolbar.
        /// </summary>
        public Image Image
        {
            get
            {
                return toolBarImage;
            }
        }

        protected void DisposeImage()
        {
            if (this.toolBarImage != null)
            {
                this.toolBarImage.Dispose();
                this.toolBarImage = null;
            }
        }

        public event EventHandler CursorChanging;
        protected virtual void OnCursorChanging()
        {
            if (CursorChanging != null)
            {
                CursorChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler CursorChanged;
        protected virtual void OnCursorChanged()
        {
            if (CursorChanged != null)
            {
                CursorChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The Cursor that is displayed when this Tool is active and the
        /// mouse cursor is inside the document view.
        /// </summary>
        public Cursor Cursor
        {
            get
            {
                return cursor;
            }

            set
            {
                OnCursorChanging();
                cursor = value;
                OnCursorChanged();
            }
        }

        /// <summary>
        /// The name of the Tool. For instance, "Pencil". This name should *not* end in "Tool", e.g. "Pencil Tool"
        /// </summary>
        public string Name
        {
            get
            {
                return toolInfo.Name;
            }
        }

        /// <summary>
        /// A short description of how to use the tool.
        /// </summary>
        public string HelpText
        {
            get
            {
                return toolInfo.HelpText;
            }
        }

        public ToolInfo Info
        {
            get
            {
                return toolInfo;
            }
        }

        /// <summary>
        /// Specifies whether or not an inherited tool should take Ink commands
        /// </summary>
        protected virtual bool SupportsInk 
        {
            get
            {
                return false;
            }
        }

        public char HotKey
        {
            get
            {
                return toolInfo.HotKey;
            }
        }

        /// <summary>
        /// A reference to the workspace that contains this Tool.
        /// </summary>
        public DocumentWorkspace Workspace
        {
            get
            {
                return workspace;
            }
        }

        // Methods to send messages to this class
        public void PerformActivate()
        {
            Activate();
        }

        public void PerformDeactivate()
        {
            Deactivate();
        }

        private bool IsOverflow(MouseEventArgs e) 
        {
            PointF clientPt = workspace.DocumentView.DocumentToClient(new PointF(e.X, e.Y));
            return clientPt.X < -16384 || clientPt.Y < -16384;
        }

        public void PerformMouseEnter()
        {
            MouseEnter();
        }

        private void MouseEnter()
        {
            ++this.mouseEnter;

            if (this.mouseEnter == 1)
            {
                OnMouseEnter();
            }
        }

        protected virtual void OnMouseEnter()
        {
        }

        public void PerformMouseLeave()
        {
            MouseLeave();
        }

        private void MouseLeave()
        {
            --this.mouseEnter;

            if (this.mouseEnter == 0)
            {
                OnMouseLeave();
            }
        }

        protected virtual void OnMouseLeave()
        {
        }

        public void PerformMouseMove(MouseEventArgs e)
        {
            if (IsOverflow(e)) 
            {
                return;
            }

            if (e is StylusEventArgs)
            {
                if (this.SupportsInk) 
                {
                    StylusMove(e as StylusEventArgs);
                }

                // if the tool does not claim ink support, discard
            }
            else
            {
                MouseMove(e);
            }
        }

        public void PerformMouseDown(MouseEventArgs e)
        {
            if (IsOverflow(e)) 
            {
                return;
            }

            if (e is StylusEventArgs) 
            {
                if (this.SupportsInk) 
                {
                    StylusDown(e as StylusEventArgs);
                }

                // if the tool does not claim ink support, discard
            }
            else
            {
                if (this.SupportsInk) 
                {
                    Workspace.DocumentView.Focus();
                } 

                MouseDown(e);
            }
        }

        public void PerformMouseUp(MouseEventArgs e)
        {
            if (IsOverflow(e)) 
            {
                return;
            }

            if (e is StylusEventArgs) 
            {
                if (this.SupportsInk) 
                {
                    StylusUp(e as StylusEventArgs);
                }

                // if the tool does not claim ink support, discard
            }
            else
            {
                MouseUp(e);
            }
        }

        public void PerformKeyPress(KeyPressEventArgs e)
        {
            KeyPress(e);
        }

        public void PerformKeyPress(Keys key)
        {
            KeyPress(key);
        }

        public void PerformKeyUp(KeyEventArgs e)
        {
            KeyUp(e);
        }

        public void PerformKeyDown(KeyEventArgs e)
        {
            KeyDown(e);
        }

        public void PerformClick()
        {
            Click();
        }

        public void PerformPulse()
        {
            Pulse();
        }

        public void PerformPaste(IDataObject data, out bool handled)
        {
            Paste(data, out handled);
        }

        public void PerformPasteQuery(IDataObject data, out bool canHandle)
        {
            PasteQuery(data, out canHandle);
        }

        private void Activate()
        {
            Debug.Assert(active != true, "already active!");
            active = true;

            this.panTracking = false;
            this.panMode = false;
            this.mouseDown = 0;
            this.savedTiles = null;
            this.saveRegion = null;

            this.scratchSurface = Workspace.ScratchSurface;
#if DEBUG
            this.haveClearedScratch = false;
#endif
            Workspace.ScratchSurface = null;

            Workspace.Environment.Selection.Changing += new EventHandler(SelectionChangingHandler);
            Workspace.Environment.Selection.Changed += new EventHandler(SelectionChangedHandler);
            Workspace.History.ExecutingHistoryAction += new ExecutingHistoryActionEventHandler(ExecutingHistoryAction);
            Workspace.History.ExecutedHistoryAction += new ExecutedHistoryActionEventHandler(ExecutedHistoryAction);
            Workspace.History.FinishedStepGroup += new EventHandler(FinishedHistoryStepGroup);

            this.trackingNub = new MoveNubRenderer(this.Renderers);
            this.trackingNub.Visible = false;
            this.trackingNub.Size = 10;
            this.trackingNub.Shape = MoveNubShape.Compass;
            this.Renderers.Add(this.trackingNub, false);

            OnActivate();
        }

        void FinishedHistoryStepGroup(object sender, EventArgs e)
        {
            OnFinishedHistoryStepGroup();
        }

        protected virtual void OnFinishedHistoryStepGroup()
        {
        }

        /// <summary>
        /// This method is called when the tool is being activated; that is, when the
        /// user has chosen to use this tool by clicking on it on a toolbar.
        /// </summary>
        protected virtual void OnActivate()
        {
        }

        private void Deactivate()
        {
            Debug.Assert(active != false, "not active!");

            active = false;
            Workspace.Environment.Selection.Changing -= new EventHandler(SelectionChangingHandler);
            Workspace.Environment.Selection.Changed -= new EventHandler(SelectionChangedHandler);
            Workspace.History.ExecutingHistoryAction -= new ExecutingHistoryActionEventHandler(ExecutingHistoryAction);
            Workspace.History.ExecutedHistoryAction -= new ExecutedHistoryActionEventHandler(ExecutedHistoryAction);
            Workspace.History.FinishedStepGroup -= new EventHandler(FinishedHistoryStepGroup);

            OnDeactivate();

            this.Renderers.Remove(this.trackingNub);
            this.trackingNub.Dispose();
            this.trackingNub = null;

            Workspace.ScratchSurface = this.scratchSurface;
            this.scratchSurface = null;

            if (this.saveRegion != null)
            {
                this.saveRegion.Dispose();
                this.saveRegion = null;
            }

            this.savedTiles = null;
        }

        /// <summary>
        /// This method is called when the tool is being deactivated; that is, when the
        /// user has chosen to use another tool by clicking on another tool on a
        /// toolbar.
        /// </summary>
        protected virtual void OnDeactivate()
        {
        }

        private void StylusDown(StylusEventArgs e)
        {
            if (!this.panMode)
            {
                OnStylusDown(e);
            }
        }

        protected virtual void OnStylusDown(StylusEventArgs e)
        {
        }

        private void StylusMove(StylusEventArgs e)
        {
            if (!this.panMode)
            {
                OnStylusMove(e);
            }
        }

        protected virtual void OnStylusMove(StylusEventArgs e)
        {
            if (this.mouseDown > 0)
            {
                ScrollIfNecessary(new PointF(e.X, e.Y));
            }
        }

        private void StylusUp(StylusEventArgs e)
        {
            if (this.panTracking)
            {
                this.trackingNub.Visible = false;
                this.panTracking = false;
                this.Cursor = this.handCursor;
            }
            else
            {
                OnStylusUp(e);
            }
        }

        protected virtual void OnStylusUp(StylusEventArgs e)
        {
        }

        private void MouseMove(MouseEventArgs e)
        {
            if (this.ignoreMouseMove > 0)
            {
                --this.ignoreMouseMove;
            }
            else if (this.panTracking && e.Button == MouseButtons.Left)
            {
                // Pan the document, using Stylus coordinates. This is done in
                // MouseMove instead of StylusMove because StylusMove is
                // asynchronous, and would not 'feel' right (pan motions would
                // stack up)

                Point position = new Point(e.X, e.Y);
                RectangleF visibleRect = Workspace.VisibleDocumentRectangleF;
                PointF visibleCenterPt = Utility.GetRectangleCenter(visibleRect);
                PointF delta = new PointF(e.X - lastPanMouseXY.X, e.Y - lastPanMouseXY.Y);
                PointF newScroll = Workspace.DocumentView.DocumentScrollPositionF;

                if (delta.X != 0 || delta.Y != 0)
                {
                    newScroll.X -= delta.X;
                    newScroll.Y -= delta.Y;

                    lastPanMouseXY = new Point(e.X, e.Y);
                    lastPanMouseXY.X -= (int)Math.Truncate(delta.X);
                    lastPanMouseXY.Y -= (int)Math.Truncate(delta.Y);

                    ++this.ignoreMouseMove; // setting DocumentScrollPosition incurs a MouseMove event. ignore it prevents 'jittering' at non-integral zoom levels (like, say, 743%)
                    Workspace.DocumentView.DocumentScrollPositionF = newScroll;
                    Update();
                }

            }
            else if (!this.panMode)
            {
                OnMouseMove(e);
            }

            this.lastMouseXY = new Point(e.X, e.Y);
            this.lastButton = e.Button;
        }

        /// <summary>
        /// This method is called when the Tool is active and the mouse is moving within
        /// the document canvas area.
        /// </summary>
        /// <param name="e">Contains information about where the mouse cursor is, in document coordinates.</param>
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (this.panMode || this.mouseDown > 0)
            {
                ScrollIfNecessary(new PointF(e.X, e.Y));
            }
        }

        private void MouseDown(MouseEventArgs e)
        {
            ++this.mouseDown;
            
            if (this.panMode)
            {
                this.panTracking = true;
                this.lastPanMouseXY = new Point(e.X, e.Y);

                if (this.CanPan())
                {
                    this.Cursor = this.handCursorMouseDown;
                }
            }
            else
            {
                OnMouseDown(e);
            }

            this.lastMouseXY = new Point(e.X, e.Y);
        }

        /// <summary>
        /// This method is called when the Tool is active and a mouse button has been
        /// pressed within the document area.
        /// </summary>
        /// <param name="e">Contains information about where the mouse cursor is, in document coordinates, and which mouse buttons were pressed.</param>
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            this.lastButton = e.Button;
        }

        private void MouseUp(MouseEventArgs e)
        {
            --this.mouseDown;

            if (!this.panMode)
            {
                OnMouseUp(e);
            }

            this.lastMouseXY = new Point(e.X, e.Y);
        }

        /// <summary>
        /// This method is called when the Tool is active and a mouse button has been
        /// released within the document area.
        /// </summary>
        /// <param name="e">Contains information about where the mouse cursor is, in document coordinates, and which mouse buttons were released.</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            this.lastButton = e.Button;
        }

        private void Click()
        {
            OnClick();
        }

        /// <summary>
        /// This method is called when the Tool is active and a mouse button has been
        /// clicked within the document area. If you need more specific information,
        /// such as where the mouse was clicked and which button was used, respond to
        /// the MouseDown/MouseUp events.
        /// </summary>
        protected virtual void OnClick()
        {
        }

        private void KeyPress(KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private static DateTime lastToolSwitch = DateTime.MinValue;

        // if we are pressing 'S' to switch to the selection tools, then consecutive
        // presses of 'S' should switch to the next selection tol in the list. however,
        // if we wait awhile then pressing 'S' should go to the *first* selection
        // tool. 'awhile' is defined by this variable.
        private static readonly TimeSpan toolSwitchReset = new TimeSpan(0, 0, 0, 2, 0);

        /// <summary>
        /// This method is called when the tool is active and a keyboard key is pressed
        /// and released. If you respond to the keyboard key, set e.Handled to true.
        /// </summary>
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            if (!e.Handled && workspace.DocumentView.Focused) 
            {
                ToolInfo[] toolInfos = Workspace.ToolInfos;
                Type currentToolType = Workspace.Environment.Tool.GetType();
                int currentTool = 0;

                if (0 != (ModifierKeys & Keys.Shift))
                {
                    Array.Reverse(toolInfos);
                }

                if (char.ToLower(this.HotKey) != char.ToLower(e.KeyChar) ||
                    (DateTime.Now - lastToolSwitch) > toolSwitchReset)
                {
                    currentTool = -1;
                }
                else
                {
                    for (int t = 0; t < toolInfos.Length; ++t) 
                    {
                        if (toolInfos[t].ToolType == currentToolType)
                        {
                            currentTool = t;
                            break;
                        }
                    }
                }

                for (int t = 0; t < toolInfos.Length; ++t) 
                {
                    int newTool = (t + currentTool + 1) % toolInfos.Length;
                    ToolInfo toolInfo = toolInfos[newTool];

                    if (char.ToLower(toolInfo.HotKey) == char.ToLower(e.KeyChar))
                    {
                        if (!this.IsMouseDown)
                        {
                            Workspace.Widgets.MainToolBar.SelectTool(toolInfo.ToolType);
                        }

                        e.Handled = true;
                        lastToolSwitch = DateTime.Now;
                        break;
                    }
                }

                // If the keypress is still not handled ...
                if (!e.Handled)
                {
                    switch (e.KeyChar)
                    {
                        // By default, Esc/Enter clear the current selection if there is any
                        case (char)13: // Enter
                        case (char)27: // Escape
                            if (this.mouseDown == 0 && !Workspace.Environment.Selection.IsEmpty)
                            {
                                e.Handled = true;
                                Workspace.PerformAction(typeof(DeselectAction));
                            }

                            break;
                    }
                }
            }
        }

        private DateTime lastKeyboardMove = DateTime.MinValue;
        private Keys lastKey;
        private int keyboardMoveSpeed = 1;
        private int keyboardMoveRepeats = 0;

        private void KeyPress(Keys key)
        {
            OnKeyPress(key);
        }

        /// <summary>
        /// This method is called when the tool is active and a keyboard key is pressed
        /// and released that is not representable with a regular Unicode chararacter.
        /// An example would be the arrow keys.
        /// </summary>
        protected virtual void OnKeyPress(Keys key)
        {
            Point dir = Point.Empty;

            if (key != lastKey) 
            {
                lastKeyboardMove = DateTime.MinValue;
            }

            lastKey = key;

            switch (key) 
            {
                case Keys.Left:
                    --dir.X;
                    break;

                case Keys.Right:
                    ++dir.X;
                    break;

                case Keys.Up:
                    --dir.Y;
                    break;

                case Keys.Down:
                    ++dir.Y;
                    break;              
            }

            if (!dir.Equals(Point.Empty)) 
            {
                long span = DateTime.Now.Ticks - lastKeyboardMove.Ticks;
                
                if ((span * 4) > TimeSpan.TicksPerSecond) 
                {
                    keyboardMoveRepeats = 0;
                    keyboardMoveSpeed = 1;
                }
                else
                {
                    keyboardMoveRepeats++;

                    if (keyboardMoveRepeats > 15 && (keyboardMoveRepeats % 4) == 0)
                    {
                        keyboardMoveSpeed++;
                    }
                }

                lastKeyboardMove = DateTime.Now;
                
                int offset = (int)(Math.Ceiling(workspace.DocumentView.ScaleFactor.Ratio) * (double)keyboardMoveSpeed);
                Cursor.Position = new Point(Cursor.Position.X + offset * dir.X, Cursor.Position.Y + offset * dir.Y);
                
                Point location = workspace.DocumentView.PointToScreen(Point.Truncate(workspace.DocumentView.DocumentToClient(PointF.Empty)));

                PointF stylusLocF = new PointF((float)Cursor.Position.X - (float)location.X, (float)Cursor.Position.Y - (float)location.Y);
                Point stylusLoc = new Point(Cursor.Position.X - location.X, Cursor.Position.Y - location.Y);

                stylusLoc = workspace.DocumentView.ScaleFactor.UnscalePoint(stylusLoc);
                stylusLocF = workspace.DocumentView.ScaleFactor.UnscalePoint(stylusLocF);

                workspace.DocumentView.PerformDocumentMouseMove(new StylusEventArgs(lastButton, 1, stylusLocF.X, stylusLocF.Y, 0, 1.0f));
                workspace.DocumentView.PerformDocumentMouseMove(new MouseEventArgs(lastButton, 1, stylusLoc.X, stylusLoc.Y, 0));
            }
        }

        private bool CanPan()
        {
            Rectangle vis = Utility.RoundRectangle(Workspace.DocumentView.VisibleDocumentRectangleF);
            vis.Intersect(Workspace.Document.Bounds);

            if (vis == Workspace.Document.Bounds)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void KeyUp(KeyEventArgs e)
        {
            if (this.panMode)
            {
                this.panMode = false;
                this.panTracking = false;
                this.trackingNub.Visible = false;
                this.Cursor = this.panOldCursor;
                this.panOldCursor = null;
                e.Handled = true;
            }

            OnKeyUp(e);
        }

        /// <summary>
        /// This method is called when the tool is active and a keyboard key is pressed.
        /// If you respond to the keyboard key, set e.Handled to true.
        /// </summary>
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            keysThatAreDown.Clear();
        }

        private void KeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        /// <summary>
        /// This method is called when the tool is active and a keyboard key is released
        /// Before responding, check that e.Handled is false, and if you then respond to 
        /// the keyboard key, set e.Handled to true.
        /// </summary>
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (!keysThatAreDown.Contains(e.KeyData))
                {
                    keysThatAreDown.Add(e.KeyData, new KeyTimeInfo());
                }

                if (!this.IsMouseDown && 
                    !this.panMode && 
                    e.KeyCode == Keys.Space)
                {
                    this.panMode = true;
                    this.panOldCursor = this.Cursor;

                    if (CanPan())
                    {
                        this.Cursor = this.handCursor;
                    }
                    else
                    {
                        this.Cursor = this.handCursorInvalid;
                    }
                }

                // arrow keys are processed in another way
                // we get their KeyDown but no KeyUp, so they can not be handled
                // by our normal methods
                OnKeyPress(e.KeyData);
                
            }
        }

        private void SelectionChanging()
        {
            OnSelectionChanging();
        }

        /// <summary>
        /// This method is called when the Tool is active and the selection area is
        /// about to be changed.
        /// </summary>
        protected virtual void OnSelectionChanging()
        {
        }

        private void SelectionChanged()
        {
            OnSelectionChanged();
        }

        /// <summary>
        /// This method is called when the Tool is active and the selection area has
        /// been changed.
        /// </summary>
        protected virtual void OnSelectionChanged()
        {
        }

        private void ExecutingHistoryAction(object sender, ExecutingHistoryActionEventArgs e)
        {
            OnExecutingHistoryAction(e);
        }

        protected virtual void OnExecutingHistoryAction(ExecutingHistoryActionEventArgs e)
        {
        }

        private void ExecutedHistoryAction(object sender, ExecutedHistoryActionEventArgs e)
        {
            OnExecutedHistoryAction(e);
        }

        protected virtual void OnExecutedHistoryAction(ExecutedHistoryActionEventArgs e)
        {
        }

        private void PasteQuery(IDataObject data, out bool canHandle)
        {
            OnPasteQuery(data, out canHandle);
        }

        /// <summary>
        /// This method is called when the system is querying a tool as to whether
        /// it can handle a pasted object.
        /// </summary>
        /// <param name="data">
        /// The clipboard data that was pasted by the user that should be inspected.
        /// </param>
        /// <param name="canHandle">
        /// <b>true</b> if the data can be handled by the tool, <b>false</b> if not.
        /// </param>
        /// <remarks>
        /// If you do not set canHandle to <b>true</b> then the tool will not be
        /// able to respond to the Edit menu's Paste item.
        /// </remarks>
        protected virtual void OnPasteQuery(IDataObject data, out bool canHandle)
        {
            canHandle = false;
        }

        private void Paste(IDataObject data, out bool handled)
        {
            OnPaste(data, out handled);
        }

        /// <summary>
        /// This method is called when the user invokes a paste operation. Tools get
        /// the first chance to handle this data.
        /// </summary>
        /// <param name="data">
        /// The data that was pasted by the user.
        /// </param>
        /// <param name="handled">
        /// <b>true</b> if the data was handled and pasted, <b>false</b> if not.
        /// </param>
        /// <remarks>
        /// If you do not set handled to <b>true</b> the event will be passed to the 
        /// global paste handler.
        /// </remarks>
        protected virtual void OnPaste(IDataObject data, out bool handled)
        {
            handled = false;
        }

        private void Pulse()
        {
            OnPulse();
        }

        protected bool IsFormActive
        {
            get
            {
                return (object.ReferenceEquals(Form.ActiveForm, Workspace.FindForm()));
            }
        }

        /// <summary>
        /// This method is called many times per second, called by the DocumentWorkspace.
        /// </summary>
        protected virtual void OnPulse()
        {
            if (this.panTracking && this.lastButton == MouseButtons.Right)
            {
                Point position = this.lastMouseXY; //new Point(e.X, e.Y);
                RectangleF visibleRect = Workspace.VisibleDocumentRectangleF;
                PointF visibleCenterPt = Utility.GetRectangleCenter(visibleRect);
                PointF delta = new PointF(position.X - visibleCenterPt.X, position.Y - visibleCenterPt.Y);
                PointF newScroll = Workspace.DocumentView.DocumentScrollPositionF;

                this.trackingNub.Visible = true;

                if (delta.X != 0 || delta.Y != 0)
                {
                    newScroll.X += delta.X;
                    newScroll.Y += delta.Y;

                    ++this.ignoreMouseMove; // setting DocumentScrollPosition incurs a MouseMove event. ignore it prevents 'jittering' at non-integral zoom levels (like, say, 743%)
                    UI.SetControlRedraw(Workspace.DocumentView, false);
                    Workspace.DocumentView.DocumentScrollPositionF = newScroll;
                    this.trackingNub.Visible = true;
                    this.trackingNub.Location = Utility.GetRectangleCenter(Workspace.VisibleDocumentRectangleF);
                    UI.SetControlRedraw(Workspace.DocumentView, true);
                    Workspace.DocumentView.Invalidate(true);
                    Update();
                }
            }
        }

        protected bool ScrollIfNecessary(PointF position) 
        {
            if (!autoScroll || !CanPan()) 
            {
                return false;
            }

            RectangleF visible = Workspace.DocumentView.VisibleDocumentRectangleF;
            PointF lastScrollPosition = Workspace.DocumentView.DocumentScrollPositionF;
            PointF delta = PointF.Empty;
            PointF zoomedPoint = PointF.Empty;

            zoomedPoint.X = Utility.Lerp((visible.Left + visible.Right) / 2.0f, position.X, 1.02f);
            zoomedPoint.Y = Utility.Lerp((visible.Top + visible.Bottom) / 2.0f, position.Y, 1.02f);

            if (zoomedPoint.X < visible.Left) 
            {
                delta.X = zoomedPoint.X - visible.Left;
            }
            else if (zoomedPoint.X > visible.Right) 
            {
                delta.X = zoomedPoint.X - visible.Right;
            } 

            if (zoomedPoint.Y < visible.Top) 
            {
                delta.Y = zoomedPoint.Y - visible.Top;
            } 
            else if (zoomedPoint.Y > visible.Bottom) 
            {
                delta.Y = zoomedPoint.Y - visible.Bottom;
            }

            if (!delta.IsEmpty) 
            {
                PointF newScrollPosition = new PointF(lastScrollPosition.X + delta.X, lastScrollPosition.Y + delta.Y);
                Workspace.DocumentView.DocumentScrollPositionF = newScrollPosition;
                Update();
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private void SelectionChangingHandler(object sender, EventArgs e)
        {
            OnSelectionChanging();
        }

        private void SelectionChangedHandler(object sender, EventArgs e)
        {
            OnSelectionChanged();
        }

        public event EventHandler StatusChanged;
        protected virtual void OnStatusChanged()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }

        private Icon statusIcon;
        public Icon StatusIcon
        {
            get
            {
                return this.statusIcon;
            }
        }

        private string statusText;
        public string StatusText
        {
            get
            {
                return this.statusText;
            }
        }

        protected void SetStatus(Icon statusIcon, string statusText)
        {
            if (statusIcon == null && statusText != null)
            {
                this.statusIcon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuHelpHelpTopicsIcon.png"), true);
            }
            else
            {
                this.statusIcon = statusIcon;
            }

            this.statusText = statusText;
            OnStatusChanged();
        }

        protected SurfaceBoxRendererList Renderers
        {
            get
            {
                return this.workspace.DocumentView.Renderers;
            }
        }

        protected void Update()
        {
            Workspace.Update();
        }

        protected object GetStaticData()
        {
            return Workspace.Environment.GetStaticToolData(this.GetType());
        }

        protected void SetStaticData(object data)
        {
            Workspace.Environment.SetStaticToolData(this.GetType(), data);
        }

        public Tool(DocumentWorkspace workspace,
                    Image toolBarImage,
                    string name,
                    string helpText,
                    char hotKey)
        {
            this.workspace = workspace;
            this.toolBarImage = toolBarImage;
            this.toolInfo = new ToolInfo(name, helpText, toolBarImage, hotKey, this.GetType());
            this.handCursor = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursor.cur"));
            this.handCursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
            this.handCursorInvalid = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorInvalid.cur"));
        }

        public static Tool CreateTool(Type toolType, DocumentWorkspace workspace)
        {
            ConstructorInfo ci = toolType.GetConstructor(new Type[] { typeof(DocumentWorkspace) });
            Tool tool = (Tool)ci.Invoke(new object[] { workspace });
            return tool;
        }

        ~Tool()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Debug.Assert(!this.active, "Tool is still active!");

            if (disposing)
            {
                if (this.handCursor != null)
                {
                    this.handCursor.Dispose();
                    this.handCursor = null;
                }

                if (this.handCursorMouseDown != null)
                {
                    this.handCursorMouseDown.Dispose();
                    this.handCursorMouseDown = null;
                }

                if (this.handCursorInvalid != null)
                {
                    this.handCursorInvalid.Dispose();
                    this.handCursorInvalid = null;
                }

                if (this.saveRegion != null)
                {
                    this.saveRegion.Dispose();
                    this.saveRegion = null;
                }
            }
        }
    }
}
