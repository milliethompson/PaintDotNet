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
	{
		protected Image toolBarImage;
        protected Cursor cursor;
		protected string name;
        protected string description;
        private DocumentWorkspace workspace;
		private EventHandler selectionChangedDelegate;
		private EventHandler selectionChangingDelegate;
        private bool active = false;
        private Hashtable keysThatAreDown = new Hashtable();

        private class KeyTimeInfo
        {
            public DateTime KeyDownTime;
            public DateTime LastKeyPressPulse;

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
        }

        /// <summary>
        /// The name of the Tool. For instance, "Pencil"
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// A short description of what the Tool does.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
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

        public void PerformMouseMove(MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        public void PerformMouseDown(MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        public void PerformMouseUp(MouseEventArgs e)
        {
            OnMouseUp(e);
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

        // Messages for derived classes to override

		/// <summary>
		/// This method is called when the tool is being activated; that is, when the
		/// user has chosen to use this tool by clicking on it on a toolbar.
		/// </summary>
        protected virtual void OnActivate()
        {
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
            active = false;
			Workspace.Environment.SelectedPathChanging -= selectionChangingDelegate;
			Workspace.Environment.SelectedPathChanged -= selectionChangedDelegate;
		}

		/// <summary>
		/// This method is called when the Tool is active and the mouse is moving within
		/// the document canvas area.
		/// </summary>
		/// <param name="e">Contains information about where the mouse cursor is, in document coordinates.</param>
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
        }

		/// <summary>
		/// This method is called when the Tool is active and a mouse button has been
		/// pressed within the document area.
		/// </summary>
		/// <param name="e">Contains information about where the mouse cursor is, in document coordinates, and which mouse buttons were pressed.</param>
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
        }

		/// <summary>
		/// This method is called when the Tool is active and a mouse button has been
		/// released within the document area.
		/// </summary>
		/// <param name="e">Contains information about where the mouse cursor is, in document coordinates, and which mouse buttons were released.</param>
		protected virtual void OnMouseUp(MouseEventArgs e)
        {
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
        }

        /// <summary>
        /// This method is called when the tool is active and a keyboard key is pressed
        /// and released that is not representable with a regular Unicode chararacter.
        /// An example would be the arrow keys.
        /// </summary>
        protected virtual void OnKeyPress(Keys key)
        {
        }

        /// <summary>
        /// This method is called when the tool is active and a keyboard key is pressed.
        /// If you respond to the keyboard key, set e.Handled to true.
        /// </summary>
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            //Debug.WriteLine("up: " + e.KeyData.ToString());
            keysThatAreDown.Clear();
        }

        /// <summary>
        /// This method is called when the tool is active and a keyboard key is released
        /// Before responding, check the e.Handled is false, and if you then respond to 
        /// the keyboard key, set e.Handled to true.
        /// </summary>
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            //Debug.WriteLine("down: " + e.KeyData.ToString());
            if (!e.Handled)
            {
                try
                {
                    keysThatAreDown.Add(e.KeyData, new KeyTimeInfo());
                }

                catch (ArgumentException)
                {
                    // item was already in the hashtable
                    // ignored because we don't really care
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
        /// This method is called many times per second, called by the DocumentWorkspace.
        /// </summary>
        protected virtual void OnPulse()
        {
            uint kbDelay;
            uint kbRepeat;
            DateTime now = DateTime.Now;

            unsafe
            {
                uint *kbDelayPtr = &kbDelay;
                uint *kbRepeatPtr = &kbRepeat;

                NativeMethods.SystemParametersInfo(NativeMethods.SpiConstants.SPI_GETKEYBOARDDELAY, 0, kbDelayPtr, 0);
                NativeMethods.SystemParametersInfo(NativeMethods.SpiConstants.SPI_GETKEYBOARDSPEED, 0, kbRepeatPtr, 0);
            }

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
                        if (now > firstRepeat)
                        {
                            keyTimeInfo.LastKeyPressPulse = now;
                            OnKeyPress(key);
                        }
                    }
                    else
                    {
                        if ((now - keyTimeInfo.LastKeyPressPulse) > kbRepeatSpan)
                        {
                            keyTimeInfo.LastKeyPressPulse = now;
                            OnKeyPress(key);
                        }
                    }
                }
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

		public Tool(DocumentWorkspace workspace)
		{
			this.workspace = workspace;
            this.toolBarImage = null;
            this.name = string.Empty;
			this.selectionChangingDelegate = new EventHandler(SelectionChangingHandler);
			this.selectionChangedDelegate = new EventHandler(SelectionChangedHandler);
		}

        public static Tool CreateTool(Type toolType, DocumentWorkspace workspace)
        {
            ConstructorInfo ci = toolType.GetConstructor(new Type[] { typeof(DocumentWorkspace) });
            Tool tool = (Tool)ci.Invoke(new object[] { workspace });
            return tool;
        }
	}
}
