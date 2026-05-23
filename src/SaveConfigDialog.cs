/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Base;
using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class SaveConfigDialog 
        : PdnBaseDialog
    {
        private static class SettingNames
        {
            // We store the bounds of the window relative to its owner.
            public const string Left = "SaveConfigDialog.Left";
            public const string Top = "SaveConfigDialog.Top";
            public const string Width = "SaveConfigDialog.Width";
            public const string Height = "SaveConfigDialog.Height";
            public const string WindowState = "SaveConfigDialog.WindowState";
            public const string ShowDonate = "SaveConfigDialog.ShowDonate";
        }

        private void LoadPositions()
        {
            FormWindowState newFws;
            int newLeft;
            int newTop;
            int newWidth;
            int newHeight;

            Size minSize = UI.ScaleSize(new Size(700, 500));
            Rectangle defaultRelativeBounds; 

            // Load the relative bounds for the dialog
            Form owner = Owner;

            if (owner != null)
            {
                // Default is centered to parent.
                defaultRelativeBounds = new Rectangle(
                    (owner.ClientSize.Width - minSize.Width) / 2,
                    (owner.ClientSize.Height - minSize.Height) / 2,
                    minSize.Width, minSize.Height);
            }
            else
            {
                defaultRelativeBounds = new Rectangle(0, 0, minSize.Width, minSize.Height);
            }

            try
            {
                string newFwsString = Settings.CurrentUser.GetString(SettingNames.WindowState, FormWindowState.Normal.ToString());
                newFws = (FormWindowState)Enum.Parse(typeof(FormWindowState), newFwsString);

                newLeft = Settings.CurrentUser.GetInt32(SettingNames.Left, defaultRelativeBounds.Left);
                newTop = Settings.CurrentUser.GetInt32(SettingNames.Top, defaultRelativeBounds.Top);
                newWidth = Math.Max(minSize.Width, Settings.CurrentUser.GetInt32(SettingNames.Width, defaultRelativeBounds.Width));
                newHeight = Math.Max(minSize.Height, Settings.CurrentUser.GetInt32(SettingNames.Height, defaultRelativeBounds.Height));
            }

            catch (Exception)
            {
                newLeft = defaultRelativeBounds.X;
                newTop = defaultRelativeBounds.Y;
                newWidth = defaultRelativeBounds.Width;
                newHeight = defaultRelativeBounds.Height;
                newFws = FormWindowState.Normal;
            }

            // Apply the values. Bounds are converted from owner-client-rect space to screen space

            WindowState = newFws;

            Point origin;

            if (owner != null)
            {
                origin = owner.RectangleToScreen(owner.ClientRectangle).Location;
            }
            else
            {
                origin = new Point(0, 0);
            }

            Rectangle defaultBounds = new Rectangle(
                origin.X + defaultRelativeBounds.X,
                origin.Y + defaultRelativeBounds.Y,
                defaultRelativeBounds.Width,
                defaultRelativeBounds.Height);

            Rectangle newBounds;

            if (newFws != FormWindowState.Maximized)
            {
                Rectangle newBounds1 = new Rectangle(origin.X + newLeft, origin.Y + newTop, newWidth, newHeight);
                Rectangle newBounds2 = ValidateAndAdjustNewBounds(owner, newBounds1, defaultBounds);

                newBounds = newBounds2;
            }
            else
            {
                newBounds = defaultBounds;
            }

            Bounds = newBounds;
        }

        private Rectangle ValidateAndAdjustNewBounds(Form owner, Rectangle newBounds, Rectangle defaultBounds)
        {
            Rectangle returnBounds;

            // Ensure that the bounds they want are in bounds *somewhere*
            bool intersects = false;
            foreach (Screen screen in Screen.AllScreens)
            {
                intersects |= screen.Bounds.IntersectsWith(newBounds);
            }

            Rectangle newBounds2;

            if (intersects)
            {
                newBounds2 = newBounds;
            }
            else
            {
                newBounds2 = defaultBounds;
            }

            Screen ourScreen;
            if (owner != null)
            {
                ourScreen = Screen.FromControl(owner);
            }
            else
            {
                ourScreen = Screen.PrimaryScreen;
            }

            // Now make sure that the bounds are forced to be on the same screen as the owner window
            Rectangle onScreenBounds = EnsureRectIsOnScreen(ourScreen, newBounds2);

            returnBounds = onScreenBounds;

            return returnBounds;
        }

        private void SavePositions()
        {
            if (WindowState != FormWindowState.Minimized)
            {
                Form owner = Owner;
                Point origin;

                if (owner != null)
                {
                    Rectangle ownerClientBounds = owner.RectangleToScreen(owner.ClientRectangle);
                    origin = owner.Location;
                }
                else
                {
                    origin = new Point(0, 0);
                }

                Settings.CurrentUser.SetInt32(SettingNames.Left, Left - origin.X);
                Settings.CurrentUser.SetInt32(SettingNames.Top, Top - origin.Y);
                Settings.CurrentUser.SetInt32(SettingNames.Width, Width);
                Settings.CurrentUser.SetInt32(SettingNames.Height, Height);
                Settings.CurrentUser.SetString(SettingNames.WindowState, WindowState.ToString());
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SavePositions();
            base.OnClosing(e);
        }

        private string fileSizeTextFormat;
        private System.Threading.Timer fileSizeTimer;
        private const int timerDelayTime = 100;

        private Cursor handIcon = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursor.cur"));
        private Cursor handIconMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
        private Hashtable fileTypeToSaveToken = new Hashtable();
        private System.ComponentModel.IContainer components = null;
        private FileType fileType;
        private System.Windows.Forms.Button defaultsButton;
        private Document document;
        private bool disposeDocument = false;
        private HeaderLabel previewHeader;
        private PaintDotNet.DocumentView documentView;
        private PaintDotNet.SaveConfigWidget saveConfigWidget;
        private System.Windows.Forms.Panel saveConfigPanel;

        private PictureBox donateImage;
        //private LinkLabel donateLink;

        private PaintDotNet.HeaderLabel settingsHeader;

        private Surface scratchSurface;
        public Surface ScratchSurface
        {
            set
            {
                if (this.scratchSurface != null)
                {
                    throw new InvalidOperationException("May only set ScratchSurface once, and only before the dialog is shown");
                }

                this.scratchSurface = value;
            }
        }

        public event ProgressEventHandler Progress;
        protected virtual void OnProgress(int percent)
        {
            if (Progress != null)
            {
                Progress(this, new ProgressEventArgs((double)percent));
            }
        }

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
                SuspendLayout();
                this.saveConfigPanel.Controls.Remove(this.saveConfigWidget);
                this.saveConfigWidget = newWidget;
                this.saveConfigPanel.Controls.Add(this.saveConfigWidget);
                ResumeLayout(true);
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

        protected override void OnShown(EventArgs e)
        {
            if (this.scratchSurface == null)
            {
                throw new InvalidOperationException("ScratchSurface was never set: it is null");
            }

            LoadPositions();

            base.OnShown(e);
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
            this.fileSizeTextFormat = PdnResources.GetString("SaveConfigDialog.PreviewHeader.Text.Format");
            this.settingsHeader.Text = PdnResources.GetString("SaveConfigDialog.SettingsHeader.Text");
            this.defaultsButton.Text = PdnResources.GetString("SaveConfigDialog.DefaultsButton.Text");
            this.previewHeader.Text = PdnResources.GetString("SaveConfigDialog.PreviewHeader.Text");
            //this.donateLink.Text = "Hello!"; //PdnResources.GetString("

            this.Icon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuFileSaveIcon.png"));

            this.donateImage.Image = ImageResource.Get("Images.PayPalDonate.gif").Reference;

            this.documentView.Cursor = handIcon;

            this.MinimumSize = this.Size;
        }

        private bool ShouldShowDonate()
        {
            bool shouldShow = true;

            try
            {
                shouldShow = Settings.SystemWide.GetBoolean(SettingNames.ShowDonate, shouldShow);
            }

            catch (Exception)
            {
            }

            try
            {
                shouldShow = Settings.CurrentUser.GetBoolean(SettingNames.ShowDonate, shouldShow);
            }

            catch (Exception)
            {
            }

            return shouldShow;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            // Donate button
            if (this.donateImage.Image != null)
            {
                this.donateImage.Size = UI.ScaleSize(this.donateImage.Image.Size);
                this.donateImage.Visible = true & ShouldShowDonate();
                //this.donateLink.Visible = true & ShouldShowDonate();
            }
            else
            {
                this.donateImage.Size = new Size(1, 1);
                this.donateImage.Visible = false;
                //this.donateLink.Visible = false;
            }

            int donateBottomMargin = UI.ScaleHeight(8);
            int donateLeftMargin = UI.ScaleWidth(8);
            int donateHorizSpacing = UI.ScaleWidth(8);

            this.donateImage.Location = new Point(donateLeftMargin, ClientSize.Height - this.donateImage.Height - donateBottomMargin);

            //this.donateLink.PerformLayout();
            //this.donateLink.Location = new Point(this.donateImage.Right + donateHorizSpacing, this.donateImage.Top + (this.donateImage.Height - this.donateLink.Height) / 2);

            // Other stuff
            int buttonsBottomMargin = UI.ScaleHeight(8);
            int buttonsRightMargin = UI.ScaleWidth(8);
            int buttonsHMargin = UI.ScaleWidth(8);

            this.baseCancelButton.Location = new Point(
                ClientSize.Width - this.baseOkButton.Width - buttonsRightMargin, 
                ClientSize.Height - buttonsBottomMargin - this.baseCancelButton.Height);

            this.baseOkButton.Location = new Point(
                this.baseCancelButton.Left - buttonsHMargin - this.baseOkButton.Width, 
                ClientSize.Height - buttonsBottomMargin - this.baseOkButton.Height);

            int previewBottomMargin = UI.ScaleHeight(8);

            Point dvLocation = UI.ScalePoint(new System.Drawing.Point(200, 29));

            Size dvSize = new Size(
                this.baseCancelButton.Right - dvLocation.X,
                this.baseCancelButton.Top - dvLocation.Y - previewBottomMargin);

            this.documentView.Bounds = new Rectangle(dvLocation, dvSize);

            this.defaultsButton.PerformLayout();
            int defaultsButtonTopMargin = UI.ScaleHeight(8);

            this.saveConfigPanel.Location = UI.ScalePoint(new System.Drawing.Point(9, 29));

            this.saveConfigPanel.Size = new Size(
                UI.ScaleWidth(180), 
                this.documentView.Bottom - this.saveConfigPanel.Top - this.defaultsButton.Height - defaultsButtonTopMargin);

            int h2 = Math.Min(this.saveConfigWidget.Height, this.saveConfigPanel.Height);

            this.defaultsButton.Location = new Point(
                this.saveConfigPanel.Right - this.defaultsButton.Width,
                this.saveConfigPanel.Top + h2 + defaultsButtonTopMargin);

            this.previewHeader.Location = UI.ScalePoint(new System.Drawing.Point(198, 8));
            this.previewHeader.Size = new Size(this.documentView.Right - this.previewHeader.Left, this.previewHeader.Height);

            this.settingsHeader.Location = UI.ScalePoint(new System.Drawing.Point(6, 8));
            this.settingsHeader.Size = UI.ScaleSize(new System.Drawing.Size(192, 14));

            base.OnLayout(levent);
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
            this.previewHeader = new PaintDotNet.HeaderLabel();
            this.documentView = new PaintDotNet.DocumentView();
            this.settingsHeader = new PaintDotNet.HeaderLabel();
            this.donateImage = new PictureBox();
            //this.donateLink = new LinkLabel();
            this.SuspendLayout();
            // 
            // baseOkButton
            // 
            this.baseOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseOkButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseOkButton.Name = "baseOkButton";
            this.baseOkButton.TabIndex = 2;
            this.baseOkButton.Click += new System.EventHandler(this.BaseOkButton_Click);
            // 
            // baseCancelButton
            // 
            this.baseCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseCancelButton.Name = "baseCancelButton";
            this.baseCancelButton.TabIndex = 3;
            this.baseCancelButton.Click += new System.EventHandler(this.BaseCancelButton_Click);
            // 
            // saveConfigPanel
            // 
            this.saveConfigPanel.AutoScroll = true;
            this.saveConfigPanel.Name = "saveConfigPanel";
            this.saveConfigPanel.TabIndex = 0;
            this.saveConfigPanel.TabStop = true;
            // 
            // defaultsButton
            // 
            this.defaultsButton.Name = "defaultsButton";
            this.defaultsButton.AutoSize = true;
            this.defaultsButton.TabIndex = 1;
            this.defaultsButton.Click += new System.EventHandler(this.DefaultsButton_Click);
            // 
            // saveConfigWidget
            // 
            this.saveConfigWidget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveConfigWidget.Name = "saveConfigWidget";
            this.saveConfigWidget.TabIndex = 9;
            this.saveConfigWidget.Token = null;
            // 
            // previewHeader
            // 
            this.previewHeader.Name = "previewHeader";
            this.previewHeader.RightMargin = 0;
            this.previewHeader.TabIndex = 11;
            this.previewHeader.TabStop = false;
            this.previewHeader.Text = "Header";
            // 
            // documentView
            // 
            this.documentView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.documentView.Document = null;
            this.documentView.Name = "documentView";
            this.documentView.PanelAutoScroll = true;
            this.documentView.RulersEnabled = false;
            this.documentView.TabIndex = 12;
            this.documentView.DocumentMouseMove += new System.Windows.Forms.MouseEventHandler(this.DocumentView_DocumentMouseMove);
            this.documentView.DocumentMouseDown += new System.Windows.Forms.MouseEventHandler(this.DocumentView_DocumentMouseDown);
            this.documentView.DocumentMouseUp += new System.Windows.Forms.MouseEventHandler(this.DocumentView_DocumentMouseUp);
            this.documentView.Visible = false;
            // 
            // settingsHeader
            // 
            this.settingsHeader.Name = "settingsHeader";
            this.settingsHeader.TabIndex = 13;
            this.settingsHeader.TabStop = false;
            this.settingsHeader.Text = "Header";
            //
            // donateImage
            //
            this.donateImage.Name = "donateImage";
            this.donateImage.SizeMode = PictureBoxSizeMode.StretchImage;
            this.donateImage.Cursor = Cursors.Hand;
            this.donateImage.Click += new EventHandler(DonateImage_Click);
            //
            // donateLink
            //
            //this.donateLink.Name = "donateLink";
            //this.donateLink.AutoSize = true;
            //this.donateLink.LinkClicked += new LinkLabelLinkClickedEventHandler(DonateLink_LinkClicked);
            // 
            // SaveConfigDialog
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(592, 351);
            this.Controls.Add(this.defaultsButton);
            this.Controls.Add(this.settingsHeader);
            this.Controls.Add(this.previewHeader);
            this.Controls.Add(this.documentView);
            this.Controls.Add(this.saveConfigPanel);
            this.Controls.Add(this.donateImage);
            //this.Controls.Add(this.donateLink);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimizeBox = false;
            this.MaximizeBox = true;
            this.Name = "SaveConfigDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.saveConfigPanel, 0);
            this.Controls.SetChildIndex(this.documentView, 0);
            this.Controls.SetChildIndex(this.baseOkButton, 0);
            this.Controls.SetChildIndex(this.baseCancelButton, 0);
            this.Controls.SetChildIndex(this.previewHeader, 0);
            this.Controls.SetChildIndex(this.settingsHeader, 0);
            this.Controls.SetChildIndex(this.defaultsButton, 0);
            this.Controls.SetChildIndex(this.donateImage, 0);
            //this.Controls.SetChildIndex(this.donateLink, 0);
            this.ResumeLayout(false);
        }
        #endregion

        /*
        private void DonateLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DonateClicked();
        }
         * */

        private void DonateImage_Click(object sender, EventArgs e)
        {
            DonateClicked();
        }

        private void DonateClicked()
        {
            PdnInfo.LaunchWebSite(this, InvariantStrings.DonateSaveConfigDialogPage);
        }

        private void DefaultsButton_Click(object sender, System.EventArgs e)
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
            this.previewHeader.Text = string.Format(this.fileSizeTextFormat, computing);
            this.fileSizeTimer.Change(timerDelayTime, 0);
            OnProgress(0);
        }

        private volatile bool callbackBusy = false;
        private ManualResetEvent callbackDoneEvent = new ManualResetEvent(true);

        private void UpdateFileSizeAndPreview(string tempFileName)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (tempFileName == null)
            {
                string error = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Error");
                this.previewHeader.Text = string.Format(this.fileSizeTextFormat, error);
            }
            else
            {
                FileInfo fi = new FileInfo(tempFileName);
                long fileSize = fi.Length;
                this.previewHeader.Text = string.Format(fileSizeTextFormat, Utility.SizeStringFromBytes(fileSize));
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
            this.previewHeader.Text = string.Format(this.fileSizeTextFormat, computing);
            int newPercent = Utility.Clamp(percent, 0, 100);
            OnProgress(newPercent);
        }

        private void FileSizeProgressEventHandler(object state, ProgressEventArgs e)
        {
            this.BeginInvoke(new Procedure<int>(SetFileSizeProgress), new object[] { (int)e.Percent });
        }

        private void FileSizeTimerCallback(object state)
        {
            try
            {
                if (!this.IsHandleCreated)
                {
                    return;
                }

                if (callbackBusy)
                {
                    this.Invoke(new Procedure(QueueFileSizeTextUpdate));
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

            catch
            {
                // Handle rare race condition where this method just fails because the form is gone
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

                    this.FileType.Save(
                        this.Document, 
                        stream, 
                        this.SaveConfigToken, 
                        this.scratchSurface,
                        new ProgressEventHandler(FileSizeProgressEventHandler), 
                        true);

                    stream.Flush();
                    stream.Close();

                    this.BeginInvoke(new Procedure<string>(UpdateFileSizeAndPreview), new object[] { tempName });
                }
#if !DEBUG
            }

            catch
            {
                this.BeginInvoke(new Procedure<string>(UpdateFileSizeAndPreview), new object[] { null } );
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

        private void BaseOkButton_Click(object sender, System.EventArgs e)
        {
            // TODO: if this takes too long, put up a dialog box saying "waiting for background task to finish ..."
            //       and with progress if ISaveWithProgress!
            using (new WaitCursorChanger(this))
            {
                this.callbackDoneEvent.WaitOne();
            }

            CleanupTimer();
        }

        private void BaseCancelButton_Click(object sender, EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                callbackDoneEvent.WaitOne();
            }

            CleanupTimer();
        }

        private bool documentMouseDown = false;
        private Point lastMouseXY;
        private void DocumentView_DocumentMouseDown(object sender, MouseEventArgs e)
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

        private void DocumentView_DocumentMouseMove(object sender, MouseEventArgs e)
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
                    PointF scrollPos = documentView.DocumentScrollPositionF;
                    PointF newScrollPos = new PointF(scrollPos.X - delta.Width, scrollPos.Y - delta.Height);
                    
                    documentView.DocumentScrollPositionF = newScrollPos;
                    documentView.Update();

                    lastMouseXY = mouseXY;
                    lastMouseXY.X -= delta.Width;
                    lastMouseXY.Y -= delta.Height;
                }
            }        
        }

        private void DocumentView_DocumentMouseUp(object sender, MouseEventArgs e)
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
