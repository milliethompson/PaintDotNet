/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for SaveConfigDialog.
    /// </summary>
    public class SaveConfigDialog 
        : PaintDotNet.PdnBaseDialog
    {
        private string fileSizeTextFormat;
        private System.Threading.Timer fileSizeTimer;
        private const int timerDelayTime = 100;

        private Cursor handIcon = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursor.cur"));
        private Cursor handIconMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
        private Hashtable fileTypeToSaveToken = new Hashtable();
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label fileSizeText;
        private System.Windows.Forms.ProgressBar fileSizeProgressBar;
        private FileType fileType;
        private System.Windows.Forms.Button defaultsButton;
        private Document document;
        private bool disposeDocument = false;
        private HeaderLabel previewHeader;
        private PaintDotNet.DocumentView documentView;
        private PaintDotNet.SaveConfigWidget saveConfigWidget;
        private System.Windows.Forms.Panel saveConfigPanel;
        private Surface renderSurface;

        private int previewRightMargin = -1;
        private PaintDotNet.HeaderLabel settingsHeader;
        private int previewBottomMargin = -1;

        /// <summary>
        /// Gets or sets the Document instance that is to be saved.
        /// If this is changed after the dialog is shown, the results are undefined.
        /// </summary>
        [Browsable(false)]
        public Document Document
        {
            get
            {
                return this.document;
            }

            set
            {   
                this.document = value;
            }
        }

        /// <summary>
        /// Gets or sets the Surface that is used for rendering the preview.
        /// This can be used to optimize memory use if you already have allocated
        /// a Surface that is the same dimensions as the Document.
        /// </summary>
        [Browsable(false)]
        public Surface RenderSurface
        {
            get
            {
                return renderSurface;
            }

            set
            {
                this.renderSurface = value;
            }
        }

        [Browsable(false)]
        public FileType FileType
        {
            get
            {
                return fileType;
            }

            set
            {
                if (this.fileType != null && this.fileType.Name == value.Name)
                {
                    return;
                }

                if (this.fileType != null)
                {
                    fileTypeToSaveToken[this.fileType] = this.SaveConfigToken;
                }

                this.fileType = value;
                SaveConfigToken token = (SaveConfigToken)fileTypeToSaveToken[this.fileType];

                if (token == null)
                {
                    token = this.fileType.GetLastSaveConfigToken();
                }

                SaveConfigWidget newWidget = this.fileType.CreateSaveConfigWidget();
                newWidget.Token = token;
                newWidget.Location = this.saveConfigWidget.Location;
                this.TokenChangedHandler(this, EventArgs.Empty);
                this.saveConfigWidget.TokenChanged -= new EventHandler(TokenChangedHandler);
                this.saveConfigPanel.Controls.Remove(this.saveConfigWidget);
                this.saveConfigWidget = newWidget;
                this.saveConfigPanel.Controls.Add(this.saveConfigWidget);
                this.saveConfigWidget.TokenChanged += new EventHandler(TokenChangedHandler);

                if (this.saveConfigWidget is NoSaveConfigWidget)
                {
                    this.defaultsButton.Enabled = false;
                }
                else
                {
                    this.defaultsButton.Enabled = true;
                }
            }
        }

        [Browsable(false)]
        public SaveConfigToken SaveConfigToken
        {
            get
            {
                return this.saveConfigWidget.Token;
            }

            set
            {
                saveConfigWidget.Token = value;
            }
        }

        public SaveConfigDialog()
        {
            this.fileSizeTimer = new System.Threading.Timer(new System.Threading.TimerCallback(FileSizeTimerCallback), 
                null, 1000, System.Threading.Timeout.Infinite);

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.Text = PdnResources.GetString("SaveConfigDialog.Text");
            this.fileSizeTextFormat = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Format");
            this.settingsHeader.Text = PdnResources.GetString("SaveConfigDialog.SettingsHeader.Text");
            this.defaultsButton.Text = PdnResources.GetString("SaveConfigDialog.DefaultsButton.Text");
            this.previewHeader.Text = PdnResources.GetString("SaveConfigDialog.PreviewHeader.Text");

            this.Icon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuFileSaveIcon.bmp"));

            if (this.renderSurface != null)
            {
                this.documentView.SetRenderSurface(this.renderSurface);
            }

            this.documentView.Cursor = handIcon;

            this.previewBottomMargin = this.Height - this.documentView.Bottom;
            this.previewRightMargin = this.Width - this.documentView.Right;

            this.MinimumSize = this.Size;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);

            // adjust heights
            Rectangle oldBounds = documentView.Bounds;
            int newWidth = this.Width - oldBounds.X - this.previewRightMargin;
            int newHeight = this.Height - oldBounds.Y - this.previewBottomMargin;
            Rectangle newBounds = new Rectangle(oldBounds.X, oldBounds.Y, newWidth, newHeight);
            documentView.Bounds = newBounds;

            int heightDelta = newBounds.Height - oldBounds.Height;
            this.saveConfigPanel.Height += heightDelta;

            this.previewHeader.Width = 1 + this.documentView.Width;

            int h2 = Math.Min(this.saveConfigWidget.Height, this.saveConfigPanel.Height);
            this.defaultsButton.Location = new Point(defaultsButton.Left, 8 + saveConfigPanel.Top + h2);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.disposeDocument && this.documentView.Document != null)
                {
                    Document disposeMe = this.documentView.Document;
                    this.documentView.Document = null;
                    disposeMe.Dispose();
                }

                CleanupTimer();

                if (this.handIcon != null)
                {
                    this.handIcon.Dispose();
                    this.handIcon = null;
                }

                if (this.handIconMouseDown != null)
                {
                    this.handIconMouseDown.Dispose();
                    this.handIconMouseDown = null;
                }
                                
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.saveConfigPanel = new System.Windows.Forms.Panel();
            this.defaultsButton = new System.Windows.Forms.Button();
            this.saveConfigWidget = new PaintDotNet.SaveConfigWidget();
            this.fileSizeText = new System.Windows.Forms.Label();
            this.fileSizeProgressBar = new System.Windows.Forms.ProgressBar();
            this.previewHeader = new PaintDotNet.HeaderLabel();
            this.documentView = new PaintDotNet.DocumentView();
            this.settingsHeader = new PaintDotNet.HeaderLabel();
            this.SuspendLayout();
            // 
            // baseOkButton
            // 
            this.baseOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseOkButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseOkButton.Location = new System.Drawing.Point(431, 319);
            this.baseOkButton.Name = "baseOkButton";
            this.baseOkButton.TabIndex = 2;
            this.baseOkButton.Click += new System.EventHandler(this.baseOkButton_Click);
            // 
            // baseCancelButton
            // 
            this.baseCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseCancelButton.Location = new System.Drawing.Point(511, 319);
            this.baseCancelButton.Name = "baseCancelButton";
            this.baseCancelButton.TabIndex = 3;
            this.baseCancelButton.Click += new System.EventHandler(this.baseCancelButton_Click);
            // 
            // saveConfigPanel
            // 
            this.saveConfigPanel.AutoScroll = true;
            this.saveConfigPanel.Location = new System.Drawing.Point(9, 29);
            this.saveConfigPanel.Name = "saveConfigPanel";
            this.saveConfigPanel.Size = new System.Drawing.Size(180, 239);
            this.saveConfigPanel.TabIndex = 0;
            this.saveConfigPanel.TabStop = true;
            // 
            // defaultsButton
            // 
            this.defaultsButton.Location = new System.Drawing.Point(107, 279);
            this.defaultsButton.Name = "defaultsButton";
            this.defaultsButton.TabIndex = 1;
            this.defaultsButton.Click += new System.EventHandler(this.defaultsButton_Click);
            // 
            // saveConfigWidget
            // 
            this.saveConfigWidget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveConfigWidget.Location = new System.Drawing.Point(0, 0);
            this.saveConfigWidget.Name = "saveConfigWidget";
            this.saveConfigWidget.Size = new System.Drawing.Size(176, 222);
            this.saveConfigWidget.TabIndex = 9;
            this.saveConfigWidget.Token = null;
            // 
            // fileSizeText
            // 
            this.fileSizeText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fileSizeText.Location = new System.Drawing.Point(200, 306);
            this.fileSizeText.Name = "fileSizeText";
            this.fileSizeText.Size = new System.Drawing.Size(160, 16);
            this.fileSizeText.TabIndex = 8;
            //
            // fileSizeProgressBar
            //
            this.fileSizeProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fileSizeProgressBar.Location = new System.Drawing.Point(202, 325);
            this.fileSizeProgressBar.Name = "fileSizeProgressBar";
            this.fileSizeProgressBar.Size = new System.Drawing.Size(192, 16);
            this.fileSizeProgressBar.Minimum = 0;
            this.fileSizeProgressBar.Maximum = 100;
            this.fileSizeProgressBar.Visible = false;
            // 
            // previewHeader
            // 
            this.previewHeader.Location = new System.Drawing.Point(198, 8);
            this.previewHeader.Name = "previewHeader";
            this.previewHeader.RightMargin = 0;
            this.previewHeader.Size = new System.Drawing.Size(144, 14);
            this.previewHeader.TabIndex = 11;
            this.previewHeader.TabStop = false;
            this.previewHeader.Text = "Header";
            // 
            // documentView
            // 
            this.documentView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.documentView.Document = null;
            this.documentView.DrawGrid = false;
            this.documentView.Location = new System.Drawing.Point(200, 29);
            this.documentView.Name = "documentView";
            this.documentView.PanelAutoScroll = true;
            this.documentView.RulersEnabled = false;
            this.documentView.Size = new System.Drawing.Size(385, 272);
            this.documentView.TabIndex = 12;
            this.documentView.DocumentMouseMove += new System.Windows.Forms.MouseEventHandler(this.documentView_DocumentMouseMove);
            this.documentView.DocumentMouseDown += new System.Windows.Forms.MouseEventHandler(this.documentView_DocumentMouseDown);
            this.documentView.DocumentMouseUp += new System.Windows.Forms.MouseEventHandler(this.documentView_DocumentMouseUp);
            this.documentView.Visible = false;
            // 
            // settingsHeader
            // 
            this.settingsHeader.Location = new System.Drawing.Point(6, 8);
            this.settingsHeader.Name = "settingsHeader";
            this.settingsHeader.Size = new System.Drawing.Size(192, 14);
            this.settingsHeader.TabIndex = 13;
            this.settingsHeader.TabStop = false;
            this.settingsHeader.Text = "Header";
            // 
            // SaveConfigDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(592, 351);
            this.Controls.Add(this.defaultsButton);
            this.Controls.Add(this.settingsHeader);
            this.Controls.Add(this.previewHeader);
            this.Controls.Add(this.fileSizeText);
            this.Controls.Add(this.fileSizeProgressBar);
            this.Controls.Add(this.documentView);
            this.Controls.Add(this.saveConfigPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.Name = "SaveConfigDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.saveConfigPanel, 0);
            this.Controls.SetChildIndex(this.documentView, 0);
            this.Controls.SetChildIndex(this.fileSizeText, 0);
            this.Controls.SetChildIndex(this.fileSizeProgressBar, 0);
            this.Controls.SetChildIndex(this.baseOkButton, 0);
            this.Controls.SetChildIndex(this.baseCancelButton, 0);
            this.Controls.SetChildIndex(this.previewHeader, 0);
            this.Controls.SetChildIndex(this.settingsHeader, 0);
            this.Controls.SetChildIndex(this.defaultsButton, 0);
            this.ResumeLayout(false);

        }
        #endregion

        private void defaultsButton_Click(object sender, System.EventArgs e)
        {
            this.SaveConfigToken = this.FileType.CreateDefaultSaveConfigToken();
        }

        private void TokenChangedHandler(object sender, EventArgs e)
        {
            QueueFileSizeTextUpdate();
        }

        private void QueueFileSizeTextUpdate()
        {
            callbackDoneEvent.Reset();

            string computing = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Computing");
            this.fileSizeText.Text = string.Format(this.fileSizeTextFormat, computing);
            this.fileSizeTimer.Change(timerDelayTime, 0);
            this.fileSizeProgressBar.Value = 0;
        }

        private volatile bool callbackBusy = false;
        private ManualResetEvent callbackDoneEvent = new ManualResetEvent(true);

        private delegate void VoidLongDelegate(long longValue);
        private delegate void VoidStringDelegate(string stringValue);
        private delegate void VoidIntDelegate(int intValue);

        private void UpdateFileSizeAndPreview(string tempFileName)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (tempFileName == null)
            {
                string error = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Error");
                this.fileSizeText.Text = string.Format(this.fileSizeTextFormat, error);
                this.fileSizeProgressBar.Visible = false;
            }
            else
            {
                FileInfo fi = new FileInfo(tempFileName);
                long fileSize = fi.Length;
                this.fileSizeText.Text = string.Format(fileSizeTextFormat, Utility.SizeStringFromBytes(fileSize));
                this.fileSizeProgressBar.Visible = false;
                this.documentView.Visible = true;

                // note: see comments for DocumentView.SuspendRefresh() for why we do these two backwards
                this.documentView.ResumeRefresh();

                Document disposeMe = null;
                try
                {
                    if (this.disposeDocument && this.documentView.Document != null)
                    {
                        disposeMe = this.documentView.Document;
                    }

                    if (this.fileType.IsReflexive(this.SaveConfigToken))
                    {
                        this.documentView.Document = this.Document;
                        this.documentView.Document.Invalidate();
                        this.disposeDocument = false;
                    }
                    else
                    {
                        FileStream stream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                        Document previewDoc;
                
                        try
                        {
                            Utility.GCFullCollect();
                            previewDoc = fileType.Load(stream);
                        }

                        catch
                        {
                            previewDoc = null;
                            TokenChangedHandler(this, EventArgs.Empty);
                        }

                        stream.Close();

                        if (previewDoc != null)
                        {
                            this.documentView.Document = previewDoc;
                            this.disposeDocument = true;
                        }

                        Utility.GCFullCollect();
                    }

                    try
                    {
                        fi.Delete();
                    }

                    catch
                    {
                    }
                }

                finally
                {
                    this.documentView.SuspendRefresh();

                    if (disposeMe != null)
                    {
                        disposeMe.Dispose();
                    }
                }
            }
        }

        private void SetFileSizeProgress(int percent)
        {
            string computingFormat = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Computing.Format");
            string computing = string.Format(computingFormat, percent);
            this.fileSizeText.Text = string.Format(this.fileSizeTextFormat, computing);
            this.fileSizeProgressBar.Value = Utility.Clamp(percent, 0, 100);
            this.fileSizeProgressBar.Visible = true;
        }

        private void FileSizeProgressEventHandler(object state, ProgressEventArgs e)
        {
            this.BeginInvoke(new VoidIntDelegate(SetFileSizeProgress), new object[] { (int)e.Percent });
        }

        private void FileSizeTimerCallback(object state)
        {
            if (!this.IsHandleCreated)
            {
                return;
            }

            if (callbackBusy)
            {
                this.Invoke(new VoidVoidDelegate(QueueFileSizeTextUpdate));
            }
            else
            {
#if !DEBUG
                try
                {
#endif
                    FileSizeTimerCallbackImpl(state);
#if !DEBUG
                }

                // Catch rare instance where BeginInvoke gets called after the form's window handle is destroyed
                catch (InvalidOperationException)
                {

                }
#endif
            }
        }

        private void FileSizeTimerCallbackImpl(object state)
        {
            callbackBusy = true;

#if !DEBUG
            try
            {
#endif
                if (this.Document != null)
                {
                    string tempName = Path.GetTempFileName();
                    FileStream stream = new FileStream(tempName, FileMode.Create, FileAccess.Write, FileShare.Read);

                    this.FileType.Save(this.Document, stream, this.SaveConfigToken, 
                        new ProgressEventHandler(FileSizeProgressEventHandler), true);

                    stream.Flush();
                    stream.Close();

                    this.BeginInvoke(new VoidStringDelegate(UpdateFileSizeAndPreview), new object[] { tempName });
                }
#if !DEBUG
            }

            catch
            {
                this.BeginInvoke(new VoidStringDelegate(UpdateFileSizeAndPreview), new object[] { null } );
            }

            finally
            {
#endif
                callbackDoneEvent.Set();
                callbackBusy = false;
#if !DEBUG
            }
#endif
        }

        private void CleanupTimer()
        {
            if (this.fileSizeTimer != null)
            {
                this.fileSizeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.fileSizeTimer.Dispose();
                this.fileSizeTimer = null;
            }
        }

        private void baseOkButton_Click(object sender, System.EventArgs e)
        {
            // TODO: if this takes too long, put up a dialog box saying "waiting for background task to finish ..."
            //       and with progress if ISaveWithProgress!
            using (new WaitCursorChanger(this))
            {
                callbackDoneEvent.WaitOne();
            }

            CleanupTimer();
        }

        private void baseCancelButton_Click(object sender, System.EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                callbackDoneEvent.WaitOne();
            }

            CleanupTimer();
        }

        private bool documentMouseDown = false;
        private Point lastMouseXY;
        private void documentView_DocumentMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e is StylusEventArgs)
            {
                return;
            }
                    
            if (e.Button == MouseButtons.Left)
            {
                documentMouseDown = true;
                documentView.Cursor = handIconMouseDown;
                lastMouseXY = new Point(e.X, e.Y);
            }
        }

        private void documentView_DocumentMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e is StylusEventArgs)
            {
                return;
            }

            if (documentMouseDown)
            {
                Point mouseXY = new Point(e.X, e.Y);
                Size delta = new Size(mouseXY.X - lastMouseXY.X, mouseXY.Y - lastMouseXY.Y);

                if (delta.Width != 0 || delta.Height != 0)
                {
                    Point scrollPos = Point.Truncate(documentView.DocumentScrollPosition);
                    Point newScrollPos = new Point(scrollPos.X - delta.Width, scrollPos.Y - delta.Height);
                    
                    documentView.DocumentScrollPosition = newScrollPos;
                    documentView.Update();

                    lastMouseXY = mouseXY;
                    lastMouseXY.X -= delta.Width;
                    lastMouseXY.Y -= delta.Height;
                }
            }        
        }

        private void documentView_DocumentMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e is StylusEventArgs)
            {
                return;
            }

            documentMouseDown = false;
            documentView.Cursor = handIcon;
        }
    }
}
