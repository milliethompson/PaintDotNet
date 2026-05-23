/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
	public class Tool
        : IDisposable
	{
		private Image toolBarImage;
		private Cursor cursor;
        private ToolInfo toolInfo;

		private DocumentWorkspace workspace;
		private EventHandler selectionChangedDelegate;
		private EventHandler selectionChangingDelegate;
		private bool active = false;
		protected bool autoScroll = true;
		private Hashtable keysThatAreDown = new Hashtable();
		MouseButtons lastButton = MouseButtons.None;
        private Surface scratchSurface;

        protected Surface ScratchSurface
        {
            get
            {
                return scratchSurface;
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
		/// it is not safe to call any other method besides PerformActive. All
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
		/// A short description of what the Tool does.
		/// </summary>
		public string Description
		{
			get
			{
				return toolInfo.Description;
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
			OnActivate();
		}

		public void PerformDeactivate()
		{
			OnDeactivate();
		}

		private bool IsOverflow(MouseEventArgs e) 
		{
			PointF clientPt = workspace.DocumentView.DocumentToClient(new PointF(e.X, e.Y));
			return clientPt.X < -16384 || clientPt.Y < -16384;
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
					OnStylusMove(e as StylusEventArgs);
				}

                // if the tool does not claim ink support, discard
            }
			else
			{
				OnMouseMove(e);
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
					OnStylusDown(e as StylusEventArgs);
				}

                // if the tool does not claim ink support, discard
			}
			else
			{
				if (this.SupportsInk) 
				{
					Workspace.DocumentView.Focus();
				} 
				else
				{
					OnMouseDown(e);
				}
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
					OnStylusUp(e as StylusEventArgs);
				}

                // if the tool does not claim ink support, discard
            }
			else
			{
				OnMouseUp(e);
			}
		}

		public void PerformKeyPress(KeyPressEventArgs e)
		{
			OnKeyPress(e);
		}

		public void PerformKeyPress(Keys key)
		{
			OnKeyPress(key);
		}

		public void PerformKeyUp(KeyEventArgs e)
		{
			OnKeyUp(e);
		}

		public void PerformKeyDown(KeyEventArgs e)
		{
			OnKeyDown(e);
		}

		public void PerformClick()
		{
			OnClick();
		}

		public void PerformPulse()
		{
			OnPulse();
		}

		public void PerformPaste(IDataObject data, out bool handled)
		{
			OnPaste(data, out handled);
		}

		public void PerformPasteQuery(IDataObject data, out bool canHandle)
		{
			OnPasteQuery(data, out canHandle);
		}

		// Messages for derived classes to override

		/// <summary>
		/// This method is called when the tool is being activated; that is, when the
		/// user has chosen to use this tool by clicking on it on a toolbar.
		/// </summary>
		protected virtual void OnActivate()
		{
            Debug.Assert(active != true, "already active!");

            this.scratchSurface = Workspace.ScratchSurface;
            Workspace.ScratchSurface = null;

			active = true;
			Workspace.Environment.SelectedPathChanging += selectionChangingDelegate;
			Workspace.Environment.SelectedPathChanged += selectionChangedDelegate;
		}

		/// <summary>
		/// This method is called when the tool is being deactivated; that is, when the
		/// user has chosen to use another tool by clicking on another tool on a
		/// toolbar.
		/// </summary>
		protected virtual void OnDeactivate()
		{
            Debug.Assert(active != false, "not active!");

            Workspace.ScratchSurface = this.scratchSurface;
            this.scratchSurface = null;

			active = false;
			Workspace.Environment.SelectedPathChanging -= selectionChangingDelegate;
			Workspace.Environment.SelectedPathChanged -= selectionChangedDelegate;
		}

		protected virtual void OnStylusDown(StylusEventArgs e)
		{
		}

		protected virtual void OnStylusMove(StylusEventArgs e)
		{
			if (e.Button != MouseButtons.None)
			{
				ScrollIfNecessary(new PointF(e.X, e.Y));
			}
		}

		protected virtual void OnStylusUp(StylusEventArgs e)
		{
		}

		/// <summary>
		/// This method is called when the Tool is active and the mouse is moving within
		/// the document canvas area.
		/// </summary>
		/// <param name="e">Contains information about where the mouse cursor is, in document coordinates.</param>
		protected virtual void OnMouseMove(MouseEventArgs e)
		{
			if (e.Button != MouseButtons.None)
			{
				ScrollIfNecessary(new PointF(e.X, e.Y));
			}

			lastButton = e.Button;
		}

		/// <summary>
		/// This method is called when the Tool is active and a mouse button has been
		/// pressed within the document area.
		/// </summary>
		/// <param name="e">Contains information about where the mouse cursor is, in document coordinates, and which mouse buttons were pressed.</param>
		protected virtual void OnMouseDown(MouseEventArgs e)
		{
			lastButton = e.Button;
		}

		/// <summary>
		/// This method is called when the Tool is active and a mouse button has been
		/// released within the document area.
		/// </summary>
		/// <param name="e">Contains information about where the mouse cursor is, in document coordinates, and which mouse buttons were released.</param>
		protected virtual void OnMouseUp(MouseEventArgs e)
		{
			lastButton = e.Button;
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

				for (int t = 0; t < toolInfos.Length; t++) 
				{
					if (toolInfos[t].ToolType == currentToolType)
					{
						currentTool = t;
						break;
					}
				}

				for (int t = 0; t < toolInfos.Length; t++) 
				{
					int newTool = (t + currentTool + 1) % toolInfos.Length;
					ToolInfo toolInfo = toolInfos[newTool];

                    if (char.ToLower(toolInfo.HotKey) == char.ToLower(e.KeyChar))
                    {
                        Workspace.Widgets.MainToolBar.SelectTool(toolInfo.ToolType);
                        e.Handled = true;
                        return;
                    }
				}
			}
		}

		private DateTime lastKeyboardMove = DateTime.MinValue;
		private Keys lastKey;
		private int keyboardMoveSpeed = 1;
		private int keyboardMoveRepeats = 0;

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
				
				if (span * 4 > TimeSpan.TicksPerSecond) 
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

		/// <summary>
		/// This method is called when the tool is active and a keyboard key is pressed.
		/// If you respond to the keyboard key, set e.Handled to true.
		/// </summary>
		protected virtual void OnKeyUp(KeyEventArgs e)
		{
			keysThatAreDown.Clear();
		}

		/// <summary>
		/// This method is called when the tool is active and a keyboard key is released
		/// Before responding, check the e.Handled is false, and if you then respond to 
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

				// arrow keys are processed in another way
				// we get their KeyDown but no KeyUp, so they can not be handled
				// by our normal methods
				OnKeyPress(e.KeyData);
			}
		}

		/// <summary>
		/// This method is called when the Tool is active and the selection area is
		/// about to be changed.
		/// </summary>
		protected virtual void OnSelectionChanging()
		{
		}

		/// <summary>
		/// This method is called when the Tool is active and the selection area has
		/// been changed.
		/// </summary>
		protected virtual void OnSelectionChanged()
		{
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

		/// <summary>
		/// This method is called many times per second, called by the DocumentWorkspace.
		/// </summary>
		protected virtual void OnPulse()
		{
			int kbDelay = Keyboard.GetRepeatDelay();
			int kbRepeat = Keyboard.GetRepeatSpeed();
			DateTime now = DateTime.Now;

			TimeSpan kbDelaySpan = new TimeSpan (0, 0, 0, 0, ((int)kbDelay + 1) * 250);
			TimeSpan kbRepeatSpan = new TimeSpan(0, 0, 0, 0, (12 * (int)kbRepeat) + 33);

			if (keysThatAreDown.Count > 1)
			{
				foreach (Keys key in keysThatAreDown.Keys)
				{
					KeyTimeInfo keyTimeInfo = (KeyTimeInfo)keysThatAreDown[key];
					DateTime firstRepeat = keyTimeInfo.KeyDownTime + kbDelaySpan;

					if (keyTimeInfo.LastKeyPressPulse == keyTimeInfo.KeyDownTime)
					{
						//Send first key repeat after delay
						if (now > firstRepeat)
						{
							keyTimeInfo.LastKeyPressPulse = now;
							keyTimeInfo.Repeats++;
							OnKeyPress(key);
						}
					}
					else
					{
						//Send rapid key repeats
						if ((now - keyTimeInfo.LastKeyPressPulse) > kbRepeatSpan)
						{
							keyTimeInfo.LastKeyPressPulse = now;
							keyTimeInfo.Repeats++;
							OnKeyPress(key);
						}
					}
				}
			}
		}

        protected bool ScrollIfNecessary(PointF position) 
        {
            if (!autoScroll) 
            {
                return false;
            }

            Rectangle visible = Workspace.DocumentView.VisibleDocumentRectangle;
            PointF lastScrollPosition = Workspace.DocumentView.DocumentScrollPosition;
            PointF delta = PointF.Empty, zoomedPoint = PointF.Empty;

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
                lastScrollPosition.X += delta.X;
                lastScrollPosition.Y += delta.Y;
                Workspace.DocumentView.DocumentScrollPosition = lastScrollPosition;
                Workspace.DocumentView.Update();
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

		public Tool(DocumentWorkspace workspace,
                    Image toolBarImage,
                    string name,
                    string description,
                    string helpText,
                    char hotKey)
		{
			this.workspace = workspace;
			this.toolBarImage = toolBarImage;
            this.toolInfo = new ToolInfo(name, description, helpText, toolBarImage, hotKey, this.GetType());
			this.selectionChangingDelegate = new EventHandler(SelectionChangingHandler);
			this.selectionChangedDelegate = new EventHandler(SelectionChangedHandler);
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
        }
    }
}
