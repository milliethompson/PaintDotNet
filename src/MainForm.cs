/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Effects;
using PaintDotNet.SystemLayer;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class MainForm 
        : PaintDotNet.PdnBaseForm
    {
        private const int effectRefreshInterval = 16;
        private const int tilesPerCpu = 200;
        private int renderingThreadCount = Math.Max(2, Processor.LogicalCpuCount);

        private PaintDotNet.SystemLayer.MenuStripEx mainMenu;

        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuFileExit;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpen;
        private System.Windows.Forms.ToolStripMenuItem menuFileNew;
        private System.Windows.Forms.ToolStripMenuItem menuFileSave;
        private System.Windows.Forms.ToolStripMenuItem menuFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem menuFileAcquire;
        private System.Windows.Forms.ToolStripMenuItem menuFileAcquireFromScannerOrCamera;
        private System.Windows.Forms.ToolStripMenuItem menuFileAcquireFromClipboard;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpenInNewWindow;
        private System.Windows.Forms.ToolStripMenuItem menuFileNewWindow;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpenRecent;
        private System.Windows.Forms.ToolStripMenuItem menuFileLanguage;
        private System.Windows.Forms.ToolStripMenuItem menuFileLanguageSentinel;
        private System.Windows.Forms.ToolStripMenuItem menuFileUpdates;
        private System.Windows.Forms.ToolStripMenuItem menuFileUpdatesCheckNow;
        private System.Windows.Forms.ToolStripMenuItem menuFileUpdatesAutoCheckEnabled;
        private System.Windows.Forms.ToolStripMenuItem menuFileUpdatesCheckForBetas;
        private System.Windows.Forms.ToolStripMenuItem menuFilePrint;

        private System.Windows.Forms.ToolStripMenuItem menuEdit;
        private System.Windows.Forms.ToolStripMenuItem menuEditUndo;
        private System.Windows.Forms.ToolStripMenuItem menuEditRedo;
        private System.Windows.Forms.ToolStripMenuItem menuEditCopy;
        private System.Windows.Forms.ToolStripMenuItem menuEditPaste;
        private System.Windows.Forms.ToolStripMenuItem menuEditCut;
        private System.Windows.Forms.ToolStripMenuItem menuEditInvertSelection;
        private System.Windows.Forms.ToolStripMenuItem menuEditSelectAll;
        private System.Windows.Forms.ToolStripMenuItem menuEditDeselect;
        private System.Windows.Forms.ToolStripMenuItem menuEditEraseSelection;
        private System.Windows.Forms.ToolStripMenuItem menuEditPasteInToNewLayer;

        private System.Windows.Forms.ToolStripMenuItem menuView;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomIn;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomOut;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomToWindow;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomToSelection;
        private System.Windows.Forms.ToolStripMenuItem menuViewActualSize;
        private System.Windows.Forms.ToolStripSeparator menuViewSeperator;
        private System.Windows.Forms.ToolStripMenuItem menuViewGrid;
        private System.Windows.Forms.ToolStripMenuItem menuViewRulers;
        private System.Windows.Forms.ToolStripMenuItem menuViewUnits;
        private System.Windows.Forms.ToolStripMenuItem menuViewUnitsPixels;
        private System.Windows.Forms.ToolStripMenuItem menuViewUnitsInches;
        private System.Windows.Forms.ToolStripMenuItem menuViewUnitsCentimeters;

        private System.Windows.Forms.ToolStripMenuItem menuImage;
        private System.Windows.Forms.ToolStripMenuItem menuImageCrop;
        private System.Windows.Forms.ToolStripMenuItem menuImageResize;
        private System.Windows.Forms.ToolStripMenuItem menuImageFlip;
        private System.Windows.Forms.ToolStripMenuItem menuImageFlipHorizontal;
        private System.Windows.Forms.ToolStripMenuItem menuImageFlipVertical;
        private System.Windows.Forms.ToolStripMenuItem menuImageFlatten;
        private System.Windows.Forms.ToolStripMenuItem menuImageCanvasSize;
        private System.Windows.Forms.ToolStripMenuItem menuImageRotate;
        private System.Windows.Forms.ToolStripMenuItem menuImageRotate90CW;
        private System.Windows.Forms.ToolStripMenuItem menuImageRotate180CW;
        private System.Windows.Forms.ToolStripMenuItem menuImageRotate270CW;
        private System.Windows.Forms.ToolStripMenuItem menuImageRotate90CCW;
        private System.Windows.Forms.ToolStripMenuItem menuImageRotate180CCW;
        private System.Windows.Forms.ToolStripMenuItem menuImageRotate270CCW;

        private System.Windows.Forms.ToolStripMenuItem menuLayers;
        private System.Windows.Forms.ToolStripMenuItem menuLayersAddNewLayer;
        private System.Windows.Forms.ToolStripMenuItem menuLayersDeleteLayer;
        private System.Windows.Forms.ToolStripMenuItem menuLayersFlip;
        private System.Windows.Forms.ToolStripMenuItem menuLayersFlipHorizontal;
        private System.Windows.Forms.ToolStripMenuItem menuLayersFlipVertical;
        private System.Windows.Forms.ToolStripMenuItem menuLayersRotateZoom;
        private System.Windows.Forms.ToolStripMenuItem menuLayersDuplicateLayer;
        private System.Windows.Forms.ToolStripMenuItem menuLayersLayerProperties;
        private System.Windows.Forms.ToolStripMenuItem menuLayersAdjustments;
        private System.Windows.Forms.ToolStripMenuItem menuLayersImportFromFile;

        private System.Windows.Forms.ToolStripMenuItem menuEffects;
        private System.Windows.Forms.ToolStripMenuItem menuEffectsSentinel;

        private System.Windows.Forms.ToolStripMenuItem menuTools;
        private System.Windows.Forms.ToolStripMenuItem menuToolsAntialiasing;
        private System.Windows.Forms.ToolStripMenuItem menuToolsAlphaBlending;
        private System.Windows.Forms.ToolStripSeparator menuToolsSeperator;

        private System.Windows.Forms.ToolStripMenuItem menuWindow;
        private System.Windows.Forms.ToolStripMenuItem menuWindowResetWindowLocations;
        private System.Windows.Forms.ToolStripMenuItem menuWindowTools;
        private System.Windows.Forms.ToolStripMenuItem menuWindowHistory;
        private System.Windows.Forms.ToolStripMenuItem menuWindowLayers;
        private System.Windows.Forms.ToolStripMenuItem menuWindowColors;
        private System.Windows.Forms.ToolStripMenuItem menuWindowTranslucent;

        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuHelpHelpTopics;
        private System.Windows.Forms.ToolStripMenuItem menuHelpDonate;
        private System.Windows.Forms.ToolStripMenuItem menuHelpSendFeedback;
        private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;

        private System.Windows.Forms.ToolStripMenuItem menuDebug;
        private System.Windows.Forms.ToolStripMenuItem menuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuItem4;

        private System.Windows.Forms.ToolStripMenuItem menuItem8;

        private System.Windows.Forms.ToolStripSeparator menuSeparator1;
        private System.Windows.Forms.ToolStripSeparator menuSeparator2;
        private System.Windows.Forms.ToolStripSeparator menuSeparator4;
        private System.Windows.Forms.ToolStripSeparator menuSeparator5;
        private System.Windows.Forms.ToolStripSeparator menuSeparator6;
        private System.Windows.Forms.ToolStripSeparator menuSeparator9;
        private System.Windows.Forms.ToolStripSeparator menuSeparator10;
        private System.Windows.Forms.ToolStripSeparator menuItem5;
        private System.Windows.Forms.ToolStripSeparator menuItem10;
        private System.Windows.Forms.ToolStripSeparator menuItem11;
        private System.Windows.Forms.ToolStripSeparator menuItem14;
        private System.Windows.Forms.ToolStripSeparator menuItem6;
        private System.Windows.Forms.ToolStripSeparator menuItem7;
        private System.Windows.Forms.ToolStripSeparator menuItem9;
        private System.Windows.Forms.ToolStripSeparator menuItem13;
        private System.Windows.Forms.ToolStripSeparator menuItem16;
        private System.Windows.Forms.ToolStripSeparator menuItem17;
        private System.Windows.Forms.ToolStripSeparator menuItem12;
        private System.Windows.Forms.ToolStripSeparator menuItem18;
        private System.Windows.Forms.ToolStripSeparator menuSeparator7;
        private System.Windows.Forms.ToolStripSeparator menuSeparator8;

        private System.ComponentModel.IContainer components;
        private PaintDotNet.DocumentWorkspace workspace;

        private EventHandler menuEffectsClickDelegate;
        private EventHandler menuToolsClickDelegate;
        private CancelEventHandler hideInsteadOfCloseDelegate;

        // NOTE: This is done as an object and not EffectConfigToken so that we can delay loading
        //       the PaintDotNet.Effects.dll until after we start up
        private object lastEffectToken = null;

        // NOTE: This is done as an object and not Effect so that we can delay loading the 
        //       PaintDotNet.Effects.dll until after we start up
        private object lastEffect = null;

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripSeparator progressStatusSeparator;
        private System.Windows.Forms.ToolStripProgressBar progressStatusBar;
        private System.Windows.Forms.ToolStripStatusLabel imageInfoStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel cursorInfoStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel contextStatusLabel;

        // We keep track of each configurable effect's last token
        // This way it keeps its values in between user invocations
        private Hashtable effectTokenHash = new Hashtable();
        private System.Windows.Forms.Timer floaterOpacityTimer;
        private FloatingToolForm[] floaters;
        private System.Windows.Forms.Timer deferredInitializationTimer;

        private MostRecentFiles mostRecentFiles = null;
        private const int defaultMostRecentFilesMax = 8;
        private const int mruIconSize = 40;
        private System.Windows.Forms.Timer invalidateTimer;

        private Icon paintDotNetIcon;
        private Image selectionIcon;
        private Image helpIcon;
        private Image cursorXYIcon;
        private Image imageSizeIcon;
        private Image addNewLayerIcon;
        private Image fileNewIcon;
        private Image editCutIcon;
        private Image imageFromDiskIcon;
        private Icon layersImportFromFileIcon;

        private System.Windows.Forms.Button defaultButton;

        private bool doKill = false;
        private bool adjustmentsPopulated = false;
        private bool effectsPopulated = false;

        private SplashForm splash = null;

        private string upgradeMsiFileName = null;

        // During startup, a background thread MAY check for updates.
        // If it finds out that an update is available, it will enable the updates button in the
        // common actions toolbar, and fill in these two variables. When the user clicks on the
        // button, the values will be read from these two variables instead of being re-downloaded
        // from the server.
        private PdnVersionManifest versionManifest = null;
        private Exception versionCheckException;
        private int versionManifestIndex = -1;

        private Image HelpIcon
        {
            get
            {
                if (this.helpIcon == null)
                {
                    this.helpIcon = PdnResources.GetImage("Icons.MenuHelpHelpTopicsIcon.png");
                }

                return this.helpIcon;
            }
        }
        
        private Icon LayersImportFromFileIcon
        {
            get
            {
                if (this.layersImportFromFileIcon == null)
                {
                    this.layersImportFromFileIcon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuLayersImportFromFileIcon.png"), true);
                }

                return this.layersImportFromFileIcon;
            }
        }

        private Image SelectionIcon
        {
            get
            {
                if (this.selectionIcon == null)
                {
                    this.selectionIcon = PdnResources.GetImage("Icons.SelectionIcon.png");
                }

                return this.selectionIcon;
            }
        }

        private Image CursorXYIcon
        {
            get
            {
                if (this.cursorXYIcon == null)
                {
                    this.cursorXYIcon = PdnResources.GetImage("Icons.CursorXYIcon.png");
                }

                return this.cursorXYIcon;
            }
        }

        private Image ImageSizeIcon
        {
            get
            {
                if (this.imageSizeIcon == null)
                {
                    this.imageSizeIcon = PdnResources.GetImage("Icons.ImageSizeIcon.png");
                }

                return this.imageSizeIcon;
            }
        }

        private Image AddNewLayerIcon
        {
            get
            {
                if (this.addNewLayerIcon == null)
                {
                    this.addNewLayerIcon = PdnResources.GetImage("Icons.MenuLayersAddNewLayerIcon.png");
                }

                return this.addNewLayerIcon;
            }
        }

        private Image FileNewIcon
        {
            get
            {
                if (this.fileNewIcon == null)
                {
                    this.fileNewIcon = PdnResources.GetImage("Icons.MenuFileNewIcon.png");
                }

                return this.fileNewIcon;
            }
        }

        private Image EditCutIcon
        {
            get
            {
                if (this.editCutIcon == null)
                {
                    this.editCutIcon = PdnResources.GetImage("Icons.MenuEditCutIcon.png");
                }

                return this.editCutIcon;
            }
        }

        private Image ImageFromDiskIcon
        {
            get
            {
                if (this.imageFromDiskIcon == null)
                {
                    this.imageFromDiskIcon = PdnResources.GetImage("Icons.ImageFromDiskIcon.png");
                }

                return this.imageFromDiskIcon;
            }
        }

        public MainForm()
            : this(new string[0])
        {
        }

        private bool fileOpened = false;
        public MainForm(string[] args)
        {
            LayerElement.SetAllowRefreshPreview(false);
            paintDotNetIcon = PdnResources.GetIcon("Icons.PaintDotNet.ico");

            this.StartPosition = FormStartPosition.WindowsDefaultLocation;

            bool noSplash = true; 
            bool doPrint = false;
            string fileName = null;

            // Parse command line arguments
            foreach (string argument in args)
            {
                if (0 == string.Compare(argument, "/splash", true))
                {
                    noSplash = false;
                }
                else if (0 == string.Compare(argument, "/print", true))
                {
                    doPrint = true;
                }
                else if (0 == string.Compare(argument, "/test", true))
                {
                    // This lets us use an alternate update manifest on the web server so that
                    // we can test manifests on a small scale before "deploying" them to everybody
                    PdnInfo.IsTestMode = true;
                }
                else if (0 == string.Compare(argument, "/profileStartupTimed", true))
                {
                    // profileStartupTimed and profileStartupWorkingSet compete, which
                    // ever is last in the args list wins.
                    PdnInfo.StartupTest = PdnInfo.StartupTestType.Timed;
                }
                else if (0 == string.Compare(argument, "/profileStartupWorkingSet", true))
                {
                    // profileStartupTimed and profileStartupWorkingSet compete, which
                    // ever is last in the args list wins.
                    PdnInfo.StartupTest = PdnInfo.StartupTestType.WorkingSet;
                }
                else
                {
                    fileName = argument;
                    noSplash = false;
                }
            }

            // make splash, if warranted
            if (!noSplash)
            {
                splash = new SplashForm();
                splash.Show();
                splash.Update();

                if (fileName != null)
                {
                    splash.TopMost = false; // this is so any error dialogs don't get hidden
                }
            }

            float tolerance;
            
            try
            {
                tolerance = Settings.CurrentUser.GetSingle(PdnSettings.Tolerance, 0.5f);
                tolerance = Math.Min(1.0f, Math.Max(0.0f, tolerance));
            }

            catch
            {
                tolerance = 0.5f;
            }

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.menuFile.Text = PdnResources.GetString("MainForm.Menu.File.Text");
            this.menuEdit.Text = PdnResources.GetString("MainForm.Menu.Edit.Text");
            this.menuView.Text = PdnResources.GetString("MainForm.Menu.View.Text"); 
            this.menuImage.Text = PdnResources.GetString("MainForm.Menu.Image.Text"); 
            this.menuLayers.Text = PdnResources.GetString("MainForm.Menu.Layers.Text");
            this.menuEffects.Text = PdnResources.GetString("MainForm.Menu.Effects.Text");
            this.menuTools.Text = PdnResources.GetString("MainForm.Menu.Tools.Text");
            this.menuWindow.Text = PdnResources.GetString("MainForm.Menu.Window.Text");
            this.menuHelp.Text = PdnResources.GetString("MainForm.Menu.Help.Text");

            workspace.DocumentView.ScaleFactorChanged += new EventHandler(DocumentView_ScaleFactorChanged);

            components = null;

            this.Icon = this.paintDotNetIcon;

            menuEffectsClickDelegate = new EventHandler(menuEffects_ClickHandler);
            menuToolsClickDelegate = new EventHandler(menuTools_ClickHandler);
            hideInsteadOfCloseDelegate = new CancelEventHandler(HideInsteadOfCloseHandler);

            // open any file if they want it

            // Does not load window location/state
            LoadSettings();

            if (fileName != null)
            {
                fileOpened = DoOpenFile(fileName);

                if (!fileOpened) // some error while opening the file
                {
                    doKill = true;
                }
            }

            if (!fileOpened)
            {
                MeasurementUnit units = Document.GetDefaultDpuUnit();
                double dpu = Document.GetDefaultDpu(units);
                Size newSize = GetNewDocumentSize();
                CreateBlankDocument(newSize, units, dpu);
                workspace.DocumentView.IncrementJustPaintWhite();
            }

            workspace.Document.Dirty = false;

#if !DEBUG
            menuDebug.Visible = false;
#endif
            //menuFileLanguage.Visible = PdnInfo.IsDebugBuild || PdnInfo.IsTestMode;

            // NOTE: Since we ngen as part of setup now, this is no longer necessary.
            //
            // HACK: On many systems we get this annoying delay when we first start out drawing.
            // Apparently there is some initialization that only occurs the first time we start
            // using a tool. So we do some stuff here to get around it by simulating drawing
            // a little bit of stuff and then backing out of its side effects.
            // Note that this does not fix a "bug" per se, but an annoyance.
            // At this point it's assumed we only have one item on the history stack.
            // I think this is a JIT issue actually. Not really an "issue" so much as the way it works.
            /*
            HistoryAction ha = (HistoryAction)workspace.History.UndoStack.ToArray()[0];
            workspace.Environment.Tool.PerformMouseDown(new StylusEventArgs(new MouseEventArgs(MouseButtons.Left, 0, 1, 1, 0)));
            workspace.Environment.Tool.PerformMouseMove(new StylusEventArgs(new MouseEventArgs(MouseButtons.Left, 0, 2, 2, 0)));
            workspace.Environment.Tool.PerformMouseMove(new StylusEventArgs(new MouseEventArgs(MouseButtons.Left, 0, 2, 2, 0)));
            workspace.Environment.Tool.PerformMouseUp(new StylusEventArgs(new MouseEventArgs(MouseButtons.Left, 0, 3, 3, 0)));
            workspace.History.StepBackward();
            workspace.History.ClearAll();
            workspace.History.PushNewAction(ha);
            workspace.Document.Dirty = false;
            */

            SetupStatusBars();
            LoadWindowState();

            deferredInitializationTimer.Enabled = true;

            workspace.Environment.UnitsChanged += new EventHandler(Environment_UnitsChanged);

            // they want to print? ok, queue it up
            if (doPrint && !doKill)
            {
                this.BeginInvoke(new VoidVoidDelegate(PrintOnStartup));
            }

            workspace.Environment.Tolerance = tolerance;
            Application.Idle += new EventHandler(Application_Idle);
        }

        private void SetToolHelpText()
        {
            if (workspace.Environment.Tool != null)
            {
                string toolName = workspace.Environment.Tool.Name;
                string helpText = workspace.Environment.Tool.HelpText;

                string contextFormat = PdnResources.GetString("MainForm.StatusBar.Context.Help.Text.Format");

                contextStatusLabel.Text = string.Format(contextFormat, toolName, helpText);
                contextStatusLabel.Image = this.HelpIcon;
            }
        }

        private void PrintOnStartup()
        {
            menuFilePrint.PerformClick();
            menuFileExit.PerformClick();
        }

        /// <summary>
        /// Computes what the size of a new document should be. If the screen is in a normal,
        /// wider-than-tall (landscape) mode then it returns 800x600. If the screen is in a
        /// taller-than-wide (portrait) mode then it retusn 600x800. If the screen is square
        /// then it returns 800x600.
        /// </summary>
        private Size GetNewDocumentSize()
        {
            if (ScreenAspect < 1.0)
            {
                return new Size(600, 800);
            }
            else
            {
                return new Size(800, 600);
            }
        }

        private void LoadWindowState()
        {
            try
            {
                FormWindowState fws = (FormWindowState)Enum.Parse(typeof(FormWindowState), 
                    Settings.CurrentUser.GetString(PdnSettings.WindowState, WindowState.ToString()), true);

                if (fws != FormWindowState.Minimized)
                {
                    if (fws != FormWindowState.Maximized)
                    {
                        Rectangle newBounds = Rectangle.Empty;

                        // Load the registry values into a rectangle so that we
                        // can update the settings all at once, instead of one
                        // at a time. This will make loading the size an all or
                        // none operation, with no rollback necessary
                        newBounds.Width = Settings.CurrentUser.GetInt32(PdnSettings.Width, this.Width);
                        newBounds.Height = Settings.CurrentUser.GetInt32(PdnSettings.Height, this.Height);

                        int top = Settings.CurrentUser.GetInt32(PdnSettings.Top, this.Top);
                        int left = Settings.CurrentUser.GetInt32(PdnSettings.Left, this.Left);
                        newBounds.Location = new Point(top, left);

                        this.Bounds = newBounds;
                    }

                    this.WindowState = fws;
                }
            }

            catch
            {
                Settings.CurrentUser.Delete(new string[] { 
                                                             PdnSettings.Width,
                                                             PdnSettings.Height,
                                                             PdnSettings.WindowState,
                                                             PdnSettings.Top,
                                                             PdnSettings.Left 
                                                         });
            }
        }

        private void LoadSettings()
        {
            try
            {
                PdnBaseForm.EnableOpacity = Settings.CurrentUser.GetBoolean(PdnSettings.TranslucentWindows, true);
            }

            catch (Exception ex)
            {
                Tracing.Ping("Exception in MainForm.LoadSettings:" + ex.ToString());

                try
                {
                    Settings.CurrentUser.Delete(new string[] 
                                                { 
                                                    PdnSettings.TranslucentWindows
                                                });
                }

                catch
                {
                }
            }

            this.workspace.LoadSettings();
        }

        private void LoadMruList()
        {
            // Load the most recent files
            try
            {
                int max = Settings.CurrentUser.GetInt32(PdnSettings.MruMax, MainForm.defaultMostRecentFilesMax);
                this.mostRecentFiles = new MostRecentFiles(max);

                for (int i = 0; i < this.mostRecentFiles.MaxCount; ++i)
                {
                    try
                    {
                        string mruName = "MRU" + i.ToString();
                        string fileName = (string)Settings.CurrentUser.GetString(mruName);

                        if (fileName != null)
                        {
                            Image thumb = Settings.CurrentUser.GetImage(mruName + "Thumb");

                            if (fileName != null && thumb != null)
                            {
                                MostRecentFile mrf = new MostRecentFile(fileName, thumb);
                                mostRecentFiles.Add(mrf);
                            }
                        }
                    }

                    catch
                    {
                        break;
                    }
                }
            }

            catch
            {
                this.mostRecentFiles = new MostRecentFiles(MainForm.defaultMostRecentFilesMax);
            }
        }

        private void SaveSettings()
        {
            Settings.CurrentUser.SetInt32(PdnSettings.Width, this.Width);
            Settings.CurrentUser.SetInt32(PdnSettings.Height, this.Height);
            Settings.CurrentUser.SetInt32(PdnSettings.Top, this.Top);
            Settings.CurrentUser.SetInt32(PdnSettings.Left, this.Left);
            Settings.CurrentUser.SetString(PdnSettings.WindowState, this.WindowState.ToString());

            Settings.CurrentUser.SetBoolean(PdnSettings.TranslucentWindows, PdnBaseForm.EnableOpacity);

            if (this.WindowState != FormWindowState.Minimized)
            {
                Settings.CurrentUser.SetBoolean(PdnSettings.ToolsFormVisible, this.workspace.Widgets.MainToolBarForm.Visible);
                Settings.CurrentUser.SetBoolean(PdnSettings.ColorsFormVisible, this.workspace.Widgets.ColorsForm.Visible);
                Settings.CurrentUser.SetBoolean(PdnSettings.HistoryFormVisible, this.workspace.Widgets.HistoryForm.Visible);
                Settings.CurrentUser.SetBoolean(PdnSettings.LayersFormVisible, this.workspace.Widgets.LayerForm.Visible);
            }

            this.workspace.SaveSettings();

            SaveMruList();
        }

        private void SaveMruList()
        {
            if (mostRecentFiles == null)
            {
                return;
            }

            Settings.CurrentUser.SetInt32(PdnSettings.MruMax, this.mostRecentFiles.MaxCount);
            MostRecentFile[] mrfArray = mostRecentFiles.GetFileList();

            for (int i = 0; i < mostRecentFiles.MaxCount; ++i)
            {
                string mruName = "MRU" + i.ToString();
                string mruThumbName = mruName + "Thumb";

                if (i >= mrfArray.Length)
                {
                    Settings.CurrentUser.Delete(mruName);
                    Settings.CurrentUser.Delete(mruThumbName);
                }
                else
                {
                    MostRecentFile mrf = mrfArray[i];
                    Settings.CurrentUser.SetString(mruName, mrf.FileName);
                    Settings.CurrentUser.SetImage(mruThumbName, mrf.Thumb);
                }
            }
        }

        private void SetupStatusBars()
        {
            // context
            SetToolHelpText();
            this.workspace.Environment.ToolChanged += new EventHandler(Environment_ToolChanged);

            // cursorInfo (x,y info)
            this.cursorInfoStatusLabel.Image = this.CursorXYIcon;
            this.cursorInfoStatusLabel.Text = string.Empty;

            this.workspace.DocumentView.DocumentMouseMove += new MouseEventHandler(DocumentView_DocumentMouseMove);
            
            // imageInfo (width,height info)
            this.imageInfoStatusLabel.Image = this.ImageSizeIcon;
            this.workspace.DocumentChanged += new EventHandler(workspace_DocumentChanged);

            // progress
            this.progressStatusBar.Visible = false;
            this.progressStatusSeparator.Visible = false;
            this.progressStatusBar.Height -= 4;
            this.progressStatusBar.ProgressBar.Style = ProgressBarStyle.Continuous;
        }

        protected override void OnQueryEndSession(CancelEventArgs e)
        {
            OnClosing(e);
            base.OnQueryEndSession(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (workspace.Document.Dirty)
            {
                switch (AskForSave())
                {
                    case DialogResult.Yes:
                        if (!DoSave())
                        {
                            e.Cancel = true;
                        }

                        break;

                    case DialogResult.No:
                        break;

                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }

            if (e.Cancel)
            {
                if (this.upgradeMsiFileName != null)
                {
                    try
                    {
                        File.Delete(this.upgradeMsiFileName);
                    }

                    catch
                    {
                    }

                    this.upgradeMsiFileName = null;
                }
            }
            else
            {
                SaveSettings();

                foreach (Form hideMe in this.floaters)
                {
                    hideMe.Hide();
                }

                this.Hide();

                if (this.upgradeMsiFileName != null && 
                    (string.Compare(".msi", Path.GetExtension(this.upgradeMsiFileName), true) == 0 ||
                     string.Compare(".exe", Path.GetExtension(this.upgradeMsiFileName), true) == 0))
                {
                    // Save the %TEMP% filename to the settings repository so that it will
                    // be deleted the next time Paint.NET run
                    string fileName = Path.GetFileName(this.upgradeMsiFileName);

                    // Verify the update's signature
                    bool verified = Security.VerifySignedFile(this, this.upgradeMsiFileName, true, false);

                    if (verified)
                    {
                        // Launch the update!
                        Settings.CurrentUser.SetString(PdnSettings.UpdateMsiFileName, fileName);

                        if (0 == string.Compare(Path.GetExtension(this.upgradeMsiFileName), ".exe", true))
                        {
                            Process.Start(this.upgradeMsiFileName, "/skipConfig /restartPdnOnExit");
                        }
                        else
                        {
                            Process.Start(this.upgradeMsiFileName);
                        }
                    }
                    else
                    {
                        // negative UI already shown -- do not show, just delete the MSI
                        try
                        {
                            File.Delete(this.upgradeMsiFileName);
                        }

                        catch
                        {
                        }

                        this.upgradeMsiFileName = null;
                    }
                }
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            workspace.Environment.SetTool(null);
            base.OnClosed (e);
        }

        private void SetMenuIcon(ToolStripMenuItem menuItem, string imageName)
        {
            menuItem.ImageTransparentColor = Utility.TransparentKey;
            menuItem.Image = PdnResources.GetImage(imageName);
        }

        private void SetMenuIcon(ToolStripMenuItem menuItem, Image image)
        {
            menuItem.ImageTransparentColor = Utility.TransparentKey;
            menuItem.Image = image;
        }

        private void ClickOnMenuItem(ToolStripMenuItem menuItem)
        {
            menuItem.PerformClick();
        }

        private delegate void VoidMenuItemDelegate(ToolStripMenuItem menuItem);

        private void ClickOnMenuItemAsync(ToolStripMenuItem menuItem)
        {
            this.BeginInvoke(new VoidMenuItemDelegate(ClickOnMenuItem), new object[] { menuItem });
        }

        private void ClearMenuItem(ToolStripMenuItem menuItem)
        {
            menuItem.DropDownItems.Clear();
        }

        private void AddToMenuItem(ToolStripMenuItem addToMe, ToolStripMenuItem addMe)
        {
            addToMe.DropDownItems.Add(addMe);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.floaterOpacityTimer != null)
                {
                    this.floaterOpacityTimer.Tick -= new System.EventHandler(this.floaterOpacityTimer_Tick);
                    this.floaterOpacityTimer.Dispose();
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
            this.components = new System.ComponentModel.Container();
            this.mainMenu = new PaintDotNet.SystemLayer.MenuStripEx();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpenRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem16 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileAcquire = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileAcquireFromClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileAcquireFromScannerOrCamera = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem11 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileNewWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpenInNewWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem10 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFilePrint = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.menuEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditPasteInToNewLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditEraseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.menuEditInvertSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditDeselect = new System.Windows.Forms.ToolStripMenuItem();
            this.menuView = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewZoomToWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewZoomToSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewActualSize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewSeperator = new System.Windows.Forms.ToolStripSeparator();
            this.menuViewGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewRulers = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewUnits = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewUnitsPixels = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewUnitsInches = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewUnitsCentimeters = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageCrop = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageResize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageCanvasSize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.menuImageFlip = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageFlipHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageFlipVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageRotate = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageRotate90CW = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageRotate180CW = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageRotate270CW = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem13 = new System.Windows.Forms.ToolStripSeparator();
            this.menuImageRotate90CCW = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageRotate180CCW = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImageRotate270CCW = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLayers = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLayersAddNewLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLayersDeleteLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLayersDuplicateLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLayersImportFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.menuLayersAdjustments = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem17 = new System.Windows.Forms.ToolStripSeparator();
            this.menuImageFlatten = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem18 = new System.Windows.Forms.ToolStripSeparator();
            this.menuLayersFlip = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLayersFlipHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLayersFlipVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLayersRotateZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem9 = new System.Windows.Forms.ToolStripSeparator();
            this.menuLayersLayerProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEffects = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEffectsSentinel = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsAntialiasing = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsAlphaBlending = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsSeperator = new System.Windows.Forms.ToolStripSeparator();
            this.menuWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindowResetWindowLocations = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.menuWindowTranslucent = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem12 = new System.Windows.Forms.ToolStripSeparator();
            this.menuWindowTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindowHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindowLayers = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindowColors = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem14 = new System.Windows.Forms.ToolStripSeparator();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpHelpTopics = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpDonate = new ToolStripMenuItem();
            this.menuSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileLanguage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileLanguageSentinel = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileUpdates = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileUpdatesCheckNow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileUpdatesAutoCheckEnabled = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileUpdatesCheckForBetas = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpSendFeedback = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.defaultButton = new System.Windows.Forms.Button();

            this.statusStrip = new StatusStrip();
            this.contextStatusLabel = new ToolStripStatusLabel();
            this.progressStatusSeparator = new ToolStripSeparator();
            this.progressStatusBar = new ToolStripProgressBar();
            this.imageInfoStatusLabel = new ToolStripStatusLabel();
            this.cursorInfoStatusLabel = new ToolStripStatusLabel();
            this.workspace = new PaintDotNet.DocumentWorkspace();
            this.floaterOpacityTimer = new System.Windows.Forms.Timer(this.components);
            this.invalidateTimer = new System.Windows.Forms.Timer(this.components);
            this.deferredInitializationTimer = new System.Windows.Forms.Timer(this.components);
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.ItemAdded += new ToolStripItemEventHandler(OnMenuItemAdded);
            this.mainMenu.ItemRemoved += new ToolStripItemEventHandler(OnMenuItemRemoved);
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                     this.menuFile,
                                                                                     this.menuEdit,
                                                                                     this.menuView,
                                                                                     this.menuImage,
                                                                                     this.menuLayers,
                                                                                     this.menuEffects,
                                                                                     this.menuTools,
                                                                                     this.menuWindow,
                                                                                     this.menuDebug,
                                                                                     this.menuHelp});
            this.mainMenu.LayoutStyle = ToolStripLayoutStyle.Flow;
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                     this.menuFileNew,
                                                                                     this.menuFileOpen,
                                                                                     this.menuFileOpenRecent,
                                                                                     this.menuFileAcquire,
                                                                                     this.menuItem11,
                                                                                     this.menuFileNewWindow,
                                                                                     this.menuFileOpenInNewWindow,
                                                                                     this.menuSeparator1,
                                                                                     this.menuFileSave,
                                                                                     this.menuFileSaveAs,
                                                                                     this.menuItem10,
                                                                                     this.menuFilePrint,
                                                                                     this.menuSeparator2,
                                                                                     this.menuFileLanguage,
                                                                                     this.menuFileUpdates,
                                                                                     this.menuSeparator10,
                                                                                     this.menuFileExit});
            // 
            // menuFileNew
            // 
            this.menuFileNew.ShortcutKeys = Keys.Control | Keys.N;
            this.menuFileNew.Click += new System.EventHandler(this.menuFileNew_Click);
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.ShortcutKeys = Keys.Control | Keys.O;
            this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
            // 
            // menuFileOpenRecent
            // 
            this.menuFileOpenRecent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                               this.menuItem16});
            this.menuFileOpenRecent.DropDownOpening += new System.EventHandler(this.menuFileOpenRecent_DropDownOpening);
            this.menuFileOpenRecent.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            // 
            // menuItem16
            // 
            this.menuItem16.Text = "sentinel";
            // 
            // menuFileAcquire
            // 
            this.menuFileAcquire.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                            this.menuFileAcquireFromClipboard,
                                                                                            this.menuFileAcquireFromScannerOrCamera});
            this.menuFileAcquire.DropDownOpening += new System.EventHandler(this.menuFileAcquire_DropDownOpening);
            this.menuFileAcquire.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            // 
            // menuFileAcquireFromClipboard
            // 
            this.menuFileAcquireFromClipboard.Click += new System.EventHandler(this.menuFileAcquireFromClipboard_Click);
            // 
            // menuFileAcquireFromScannerOrCamera
            // 
            this.menuFileAcquireFromScannerOrCamera.Click += new System.EventHandler(this.menuFileAcquireFromScannerOrCamera_Click);
            // 
            // menuFileNewWindow
            // 
            this.menuFileNewWindow.ShortcutKeys = Keys.Control | Keys.Shift | Keys.W;
            this.menuFileNewWindow.Click += new System.EventHandler(this.menuFileNewWindow_Click);
            // 
            // menuFileOpenInNewWindow
            // 
            this.menuFileOpenInNewWindow.ShortcutKeys = Keys.Control | Keys.Shift | Keys.O;
            this.menuFileOpenInNewWindow.Click += new System.EventHandler(this.menuFileOpenInNewWindow_Click);
            // 
            // menuFileSave
            // 
            this.menuFileSave.ShortcutKeys = Keys.Control | Keys.S;
            this.menuFileSave.Click += new System.EventHandler(this.menuFileSave_Click);
            // 
            // menuFileSaveAs
            // 
            this.menuFileSaveAs.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            this.menuFileSaveAs.Click += new System.EventHandler(this.menuFileSaveAs_Click);
            // 
            // menuFilePrint
            // 
            this.menuFilePrint.ShortcutKeys = Keys.Control | Keys.P;
            this.menuFilePrint.Click += new System.EventHandler(this.menuFilePrint_Click);
            //
            // menuFileLanguage
            //
            this.menuFileLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                             this.menuFileLanguageSentinel});
            this.menuFileLanguage.DropDownOpening += new EventHandler(menuFileLanguage_DropDownOpening);
            this.menuFileLanguage.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            // 
            // menuFileLanguageSentinel
            //
            this.menuFileLanguageSentinel.Text = "(sentinel)";
            //
            // menuFileUpdates
            //
            this.menuFileUpdates.DropDownOpening += new EventHandler(menuFileUpdates_DropDownOpening);
            this.menuFileUpdates.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            this.menuFileUpdates.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                            this.menuFileUpdatesCheckNow,
                                                                                            this.menuSeparator9,
                                                                                            this.menuFileUpdatesAutoCheckEnabled,
                                                                                            this.menuFileUpdatesCheckForBetas});
            //
            // menuFileUpdatesCheckNow
            //
            this.menuFileUpdatesCheckNow.Click += new EventHandler(menuFileUpdatesCheckNow_Click);
            //
            // menuFileUpdatesAutoCheckEnabled
            //
            this.menuFileUpdatesAutoCheckEnabled.Click += new EventHandler(menuFileUpdatesAutoCheckEnabled_Click);
            //
            // menuFileUpdatesCheckForBetas
            //
            this.menuFileUpdatesCheckForBetas.Click += new EventHandler(menuFileUpdatesCheckForBetas_Click);
            // 
            // menuFileExit
            // 
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuEdit
            // 
            this.menuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                     this.menuEditUndo,
                                                                                     this.menuEditRedo,
                                                                                     this.menuSeparator4,
                                                                                     this.menuEditCut,
                                                                                     this.menuEditCopy,
                                                                                     this.menuEditPaste,
                                                                                     this.menuEditPasteInToNewLayer,
                                                                                     this.menuEditEraseSelection,
                                                                                     this.menuSeparator6,
                                                                                     this.menuEditInvertSelection,
                                                                                     this.menuEditSelectAll,
                                                                                     this.menuEditDeselect});
            this.menuEdit.DropDownOpening += new System.EventHandler(this.menuEdit_DropDownOpening);
            this.menuEdit.DropDownClosed += new EventHandler(this.OnMenuDropDownClosed);
            // 
            // menuEditUndo
            // 
            this.menuEditUndo.ShortcutKeys = Keys.Control | Keys.Z;
            this.menuEditUndo.Click += new System.EventHandler(this.menuEditUndo_Click);
            // 
            // menuEditRedo
            // 
            this.menuEditRedo.ShortcutKeys = Keys.Control | Keys.Y; 
            this.menuEditRedo.Click += new System.EventHandler(this.menuEditRedo_Click);
            // 
            // menuEditCut
            // 
            this.menuEditCut.ShortcutKeys = Keys.Control | Keys.X;
            this.menuEditCut.Click += new System.EventHandler(this.menuEditCut_Click);
            // 
            // menuEditCopy
            // 
            this.menuEditCopy.ShortcutKeys = Keys.Control | Keys.C;
            this.menuEditCopy.Click += new System.EventHandler(this.menuEditCopy_Click);
            // 
            // menuEditPaste
            // 
            this.menuEditPaste.ShortcutKeys = Keys.Control | Keys.V;
            this.menuEditPaste.Click += new System.EventHandler(this.menuEditPaste_Click);
            // 
            // menuEditPasteInToNewLayer
            // 
            this.menuEditPasteInToNewLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;
            this.menuEditPasteInToNewLayer.Click += new System.EventHandler(this.menuEditPasteInToNewLayer_Click);
            // 
            // menuEditEraseSelection
            // 
            this.menuEditEraseSelection.ShortcutKeys = Keys.Delete;
            this.menuEditEraseSelection.Click += new System.EventHandler(this.menuEditClearSelection_Click);
            // 
            // menuEditInvertSelection
            // 
            this.menuEditInvertSelection.Click += new System.EventHandler(this.menuEditInvertSelection_Click);
            this.menuEditInvertSelection.ShortcutKeys = Keys.Control | Keys.I;
            // 
            // menuEditSelectAll
            // 
            this.menuEditSelectAll.ShortcutKeys = Keys.Control | Keys.A;
            this.menuEditSelectAll.Click += new System.EventHandler(this.menuEditSelectAll_Click);
            // 
            // menuEditDeselect
            // 
            this.menuEditDeselect.ShortcutKeys = Keys.Control | Keys.D;
            this.menuEditDeselect.Click += new System.EventHandler(this.menuEditDeselect_Click);
            // 
            // menuView
            // 
            this.menuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                     this.menuViewZoomIn,
                                                                                     this.menuViewZoomOut,
                                                                                     this.menuViewZoomToWindow,
                                                                                     this.menuViewZoomToSelection,
                                                                                     this.menuViewActualSize,
                                                                                     this.menuViewSeperator,
                                                                                     this.menuViewGrid,
                                                                                     this.menuViewRulers,
                                                                                     this.menuViewUnits});
            this.menuView.DropDownOpening += new System.EventHandler(this.menuView_DropDownOpening);
            this.menuView.DropDownClosed += new EventHandler(this.OnMenuDropDownClosed);
            // 
            // menuViewZoomIn
            // 
            this.menuViewZoomIn.ShortcutKeys = Keys.Control | Keys.K;
            this.menuViewZoomIn.Click += new System.EventHandler(this.menuViewZoomIn_Click);
            // 
            // menuViewZoomOut
            // 
            this.menuViewZoomOut.ShortcutKeys = Keys.Control | Keys.J;
            this.menuViewZoomOut.Click += new System.EventHandler(this.menuViewZoomOut_Click);
            // 
            // menuViewZoomToWindow
            // 
            this.menuViewZoomToWindow.ShortcutKeys = Keys.Control | Keys.B;
            this.menuViewZoomToWindow.Click += new System.EventHandler(this.menuViewZoomToWindow_Click);
            // 
            // menuViewZoomToSelection
            // 
            this.menuViewZoomToSelection.ShortcutKeys = Keys.Control | Keys.Shift | Keys.B;
            this.menuViewZoomToSelection.Click += new System.EventHandler(this.menuViewZoomToSelection_Click);
            // 
            // menuViewActualSize
            // 
            this.menuViewActualSize.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;
            this.menuViewActualSize.Click += new System.EventHandler(this.menuViewActualSize_Click);
            // 
            // menuViewGrid
            // 
            this.menuViewGrid.Click += new System.EventHandler(this.menuViewGrid_Click);
            // 
            // menuViewRulers
            // 
            this.menuViewRulers.Click += new System.EventHandler(this.menuViewRulers_Click);
            //
            // menuViewUnits
            //
            this.menuViewUnits.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                          this.menuViewUnitsPixels,
                                                                                          this.menuViewUnitsInches,
                                                                                          this.menuViewUnitsCentimeters});
            this.menuViewUnits.DropDownOpening += new EventHandler(menuViewUnits_DropDownOpening);
            this.menuViewUnits.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            //
            // menuViewUnitsPixels
            //
            this.menuViewUnitsPixels.Click += new EventHandler(menuViewUnitsPixels_Click);
            //
            // menuViewUnitsInches
            //
            this.menuViewUnitsInches.Click += new EventHandler(menuViewUnitsInches_Click);
            //
            // menuViewUnitsCentimeters
            //
            this.menuViewUnitsCentimeters.Click += new EventHandler(menuViewUnitsCentimeters_Click);
            // 
            // menuImage
            // 
            this.menuImage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                      this.menuImageCrop,
                                                                                      this.menuImageResize,
                                                                                      this.menuImageCanvasSize,
                                                                                      this.menuSeparator8,
                                                                                      this.menuImageFlip,
                                                                                      this.menuImageRotate,
                                                                                      this.menuItem18,
                                                                                      this.menuImageFlatten });
            this.menuImage.DropDownOpening += new System.EventHandler(this.menuImage_DropDownOpening);
            this.menuImage.DropDownClosed += new EventHandler(this.OnMenuDropDownClosed);
            // 
            // menuImageCrop
            // 
            this.menuImageCrop.Click += new System.EventHandler(this.menuImageCrop_Click);
            this.menuImageCrop.ShortcutKeys = Keys.Control | Keys.Shift | Keys.X;
            // 
            // menuImageResize
            // 
            this.menuImageResize.ShortcutKeys = Keys.Control | Keys.R;
            this.menuImageResize.Click += new System.EventHandler(this.menuImageResize_Click);
            // 
            // menuImageCanvasSize
            // 
            this.menuImageCanvasSize.ShortcutKeys = Keys.Control | Keys.Shift | Keys.R;
            this.menuImageCanvasSize.Click += new System.EventHandler(this.menuImageCanvasSize_Click);
            // 
            // menuImageFlip
            // 
            this.menuImageFlip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                          this.menuImageFlipHorizontal,
                                                                                          this.menuImageFlipVertical});
            // 
            // menuImageFlipHorizontal
            // 
            this.menuImageFlipHorizontal.Click += new System.EventHandler(this.menuImageFlipHorizontal_Click);
            // 
            // menuImageFlipVertical
            // 
            this.menuImageFlipVertical.Click += new System.EventHandler(this.menuImageFlipVertical_Click);
            // 
            // menuImageRotate
            // 
            this.menuImageRotate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                            this.menuImageRotate90CW,
                                                                                            this.menuImageRotate180CW,
                                                                                            this.menuImageRotate270CW,
                                                                                            this.menuItem13,
                                                                                            this.menuImageRotate90CCW,
                                                                                            this.menuImageRotate180CCW,
                                                                                            this.menuImageRotate270CCW});
            // 
            // menuImageRotate90CW
            // 
            this.menuImageRotate90CW.ShortcutKeys = Keys.Control | Keys.H;
            this.menuImageRotate90CW.Click += new System.EventHandler(this.menuImageRotate90CW_Click);
            // 
            // menuImageRotate180CW
            // 
            this.menuImageRotate180CW.Click += new System.EventHandler(this.menuImageRotate180CW_Click);
            // 
            // menuImageRotate270CW
            // 
            this.menuImageRotate270CW.Click += new System.EventHandler(this.menuImageRotate270CW_Click);
            // 
            // menuImageRotate90CCW
            // 
            this.menuImageRotate90CCW.ShortcutKeys = Keys.Control | Keys.G;
            this.menuImageRotate90CCW.Click += new System.EventHandler(this.menuImageRotate90CCW_Click);
            // 
            // menuImageRotate180CCW
            // 
            this.menuImageRotate180CCW.Click += new System.EventHandler(this.menuImageRotate180CCW_Click);
            // 
            // menuImageRotate270CCW
            // 
            this.menuImageRotate270CCW.Click += new System.EventHandler(this.menuImageRotate270CCW_Click);
            // 
            // menuImageFlatten
            // 
            this.menuImageFlatten.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F;
            this.menuImageFlatten.Click += new System.EventHandler(this.menuImageFlatten_Click);
            // 
            // menuLayers
            // 
            this.menuLayers.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                       this.menuLayersAddNewLayer,
                                                                                       this.menuLayersDeleteLayer,
                                                                                       this.menuLayersDuplicateLayer,
                                                                                       this.menuLayersImportFromFile,
                                                                                       this.menuSeparator5,
                                                                                       this.menuLayersAdjustments,
                                                                                       this.menuLayersFlip,
                                                                                       this.menuLayersRotateZoom,
                                                                                       this.menuItem9,
                                                                                       this.menuLayersLayerProperties});
            this.menuLayers.DropDownOpening += new System.EventHandler(this.menuLayers_DropDownOpening);
            this.menuLayers.DropDownClosed += new EventHandler(this.OnMenuDropDownClosed);
            // 
            // menuLayersAddNewLayer
            // 
            this.menuLayersAddNewLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
            this.menuLayersAddNewLayer.Click += new System.EventHandler(this.menuLayersAddNewLayer_Click);
            // 
            // menuLayersDeleteLayer
            // 
            this.menuLayersDeleteLayer.ShortcutKeys = Keys.Shift | Keys.Delete;
            this.menuLayersDeleteLayer.Click += new System.EventHandler(this.menuLayersDeleteLayer_Click);
            // 
            // menuLayersDuplicateLayer
            // 
            this.menuLayersDuplicateLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
            this.menuLayersDuplicateLayer.Click += new System.EventHandler(this.menuLayersDuplicateLayer_Click);
            // 
            // menuLayersImportFromFile
            // 
            this.menuLayersImportFromFile.Click += new System.EventHandler(this.menuLayersImportFromFile_click);
            // 
            // menuLayersAdjustments
            // 
            this.menuLayersAdjustments.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                                  this.menuItem17});
            this.menuLayersAdjustments.DropDownOpening += new System.EventHandler(this.menuLayersAdjustments_DropDownOpening);
            this.menuLayersAdjustments.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            // 
            // menuItem17
            // 
            this.menuItem17.Text = "(sentinel)";
            // 
            // menuLayersFlip
            // 
            this.menuLayersFlip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                           this.menuLayersFlipHorizontal,
                                                                                           this.menuLayersFlipVertical});
            // 
            // menuLayersFlipHorizontal
            // 
            this.menuLayersFlipHorizontal.Click += new System.EventHandler(this.menuLayersFlipHorizontal_Click);
            // 
            // menuLayersFlipVertical
            // 
            this.menuLayersFlipVertical.Click += new System.EventHandler(this.menuLayersFlipVertical_Click);
            //
            // menuLayersRotateZoom
            //
            this.menuLayersRotateZoom.Click += new EventHandler(menuLayersRotateZoom_Click);
            // 
            // menuLayersLayerProperties
            // 
            this.menuLayersLayerProperties.ShortcutKeys = Keys.F4;
            this.menuLayersLayerProperties.Click += new System.EventHandler(this.menuLayersLayerProperties_Click);
            // 
            // menuEffects
            // 
            this.menuEffects.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                        this.menuEffectsSentinel});
            this.menuEffects.DropDownOpening += new System.EventHandler(this.menuEffects_DropDownOpening);
            this.menuEffects.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            // 
            // menuEffectsSentinel
            // 
            this.menuEffectsSentinel.Text = "sentinel";
            // 
            // menuTools
            // 
            this.menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                      this.menuToolsAntialiasing,
                                                                                      this.menuToolsAlphaBlending,
                                                                                      this.menuToolsSeperator});
            this.menuTools.DropDownOpening += new System.EventHandler(this.menuTools_DropDownOpening);
            this.menuTools.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            // 
            // menuToolsAntiAliasing
            // 
            this.menuToolsAntialiasing.Click += new System.EventHandler(this.menuToolsAntiAliasing_Click);
            //
            // menuToolsAlphaBlending
            //
            this.menuToolsAlphaBlending.Click += new EventHandler(menuToolsAlphaBlending_Click);
            // 
            // menuWindow
            // 
            this.menuWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                       this.menuWindowResetWindowLocations,
                                                                                       this.menuItem7,
                                                                                       this.menuWindowTranslucent,
                                                                                       this.menuItem12,
                                                                                       this.menuWindowTools,
                                                                                       this.menuWindowHistory,
                                                                                       this.menuWindowLayers,
                                                                                       this.menuWindowColors});
            this.menuWindow.DropDownOpening += new System.EventHandler(this.menuWindow_DropDownOpening);
            this.menuWindow.DropDownClosed += new EventHandler(OnMenuDropDownClosed);
            // 
            // menuWindowResetWindowLocations
            // 
            this.menuWindowResetWindowLocations.Click += new System.EventHandler(this.menuWindowResetWindowLocations_Click);
            // 
            // menuWindowTranslucent
            // 
            this.menuWindowTranslucent.Click += new System.EventHandler(this.menuWindowTranslucent_Click);
            // 
            // menuWindowTools
            // 
            this.menuWindowTools.ShortcutKeys = Keys.F5;
            this.menuWindowTools.Click += new System.EventHandler(this.menuWindowTools_Click);
            // 
            // menuWindowHistory
            // 
            this.menuWindowHistory.ShortcutKeys = Keys.F6;
            this.menuWindowHistory.Click += new System.EventHandler(this.menuWindowHistory_Click);
            // 
            // menuWindowLayers
            // 
            this.menuWindowLayers.ShortcutKeys = Keys.F7;
            this.menuWindowLayers.Click += new System.EventHandler(this.menuWindowLayers_Click);
            // 
            // menuWindowColors
            // 
            this.menuWindowColors.ShortcutKeys = Keys.F8;
            this.menuWindowColors.Click += new System.EventHandler(this.menuWindowColors_Click);
            // 
            // menuDebug
            // 
            this.menuDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                      this.menuItem1,
                                                                                      this.menuItem4,
                                                                                      this.menuItem5,
                                                                                      this.menuItem6,
                                                                                      this.menuItem8});
            this.menuDebug.Text = "Debug";
            // 
            // menuItem1
            // 
            this.menuItem1.Text = "Invalidate Document";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Text = "GC.Collect";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Text = "Breakpoint";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Text = "Resposition floaters";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem8
            // 
            this.menuItem8.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                      this.menuItem14});
            this.menuItem8.Text = "Stress";
            // 
            // menuItem14
            // 
            this.menuItem14.Text = "Open All Files On C:, D:";
            this.menuItem14.Click += new System.EventHandler(this.menuItem14_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                     this.menuHelpHelpTopics,
                                                                                     this.menuHelpDonate,
                                                                                     this.menuHelpSendFeedback,
                                                                                     this.menuSeparator7,
                                                                                     this.menuHelpAbout});
            // 
            // menuHelpHelpTopics
            // 
            this.menuHelpHelpTopics.ShortcutKeys = Keys.F1;
            this.menuHelpHelpTopics.Click += new System.EventHandler(this.menuHelpHelpTopics_Click);
            //
            // menuHelpDonate
            //
            this.menuHelpDonate.Click += new EventHandler(menuHelpDonate_Click);
            //
            // menuHelpSendFeedback
            //
            this.menuHelpSendFeedback.Click += new EventHandler(menuHelpSendFeedback_Click);
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            //
            // statusStrip
            //
            this.statusStrip.Items.Add(this.contextStatusLabel);
            this.statusStrip.Items.Add(this.progressStatusSeparator);
            this.statusStrip.Items.Add(this.progressStatusBar);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.imageInfoStatusLabel);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.cursorInfoStatusLabel);
            this.statusStrip.Name = "statusStrip";
            //
            // contextStatusLabel
            //
            this.contextStatusLabel.Name = "contextStatusLabel";
            this.contextStatusLabel.Width = UI.ScaleWidth(436);
            this.contextStatusLabel.Spring = true;
            this.contextStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.contextStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            //
            // progressStatusBar
            //
            this.progressStatusBar.Name = "progressStatusBar";
            this.progressStatusBar.Width = 130;
            this.progressStatusBar.AutoSize = false;
            //
            // imageInfoStatusLabel
            //
            this.imageInfoStatusLabel.Name = "imageInfoStatusLabel";
            this.imageInfoStatusLabel.Width = UI.ScaleWidth(130);
            this.imageInfoStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.imageInfoStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.imageInfoStatusLabel.AutoSize = false;
            //
            // cursorInfoStatusLabel
            //
            this.cursorInfoStatusLabel.Name = "cursorInfoStatusLabel";
            this.cursorInfoStatusLabel.Width = UI.ScaleWidth(130);
            this.cursorInfoStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.cursorInfoStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.cursorInfoStatusLabel.AutoSize = false;
            // 
            // workspace
            // 
            this.workspace.ActiveLayer = null;
            this.workspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workspace.Location = new System.Drawing.Point(0, 0);
            this.workspace.Name = "workspace";
            this.workspace.Size = new System.Drawing.Size(752, 648);
            this.workspace.TabIndex = 2;
            this.workspace.Scroll += new System.Windows.Forms.ScrollEventHandler(this.workspace_Scroll);
            this.workspace.DocumentChanged += new System.EventHandler(this.workspace_DocumentChanged);
            this.workspace.ToolStatusChanged += new EventHandler(workspace_ToolStatusChanged);
            this.workspace.ProcessCmdKeyEvent += new CmdKeysEventHandler(workspace_ProcessCmdKeyEvent);
            // 
            // floaterOpacityTimer
            // 
            this.floaterOpacityTimer.Enabled = false;
            this.floaterOpacityTimer.Interval = 25;
            this.floaterOpacityTimer.Tick += new System.EventHandler(this.floaterOpacityTimer_Tick);
            // 
            // invalidateTimer
            // 
            this.invalidateTimer.Interval = effectRefreshInterval;
            this.invalidateTimer.Tick += new System.EventHandler(this.invalidateTimer_Tick);
            //
            // populateEffectsTimer
            //
            this.deferredInitializationTimer.Interval = 250;
            this.deferredInitializationTimer.Tick += new EventHandler(DeferredInitialization);
            //
            // defaultButton
            //
            this.defaultButton.Size = new System.Drawing.Size(1, 1);
            this.defaultButton.Text = "";
            this.defaultButton.Location = new Point(-100, -100);
            this.defaultButton.TabStop = false;
            this.defaultButton.Click += new EventHandler(defaultButton_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(950, 738);
            this.Controls.Add(this.workspace);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.defaultButton);
            this.Controls.Add(this.mainMenu);
            this.AcceptButton = this.defaultButton;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
            this.ForceActiveTitleBar = true;
            this.KeyPreview = true;
            this.Controls.SetChildIndex(this.statusStrip, 0);
            this.Controls.SetChildIndex(this.workspace, 0);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private void menuHelpDonate_Click(object sender, EventArgs e)
        {
            Shell.OpenUrl(this, InvariantStrings.DonateUrl);
        }

        void OnMenuItemAdded(object sender, ToolStripItemEventArgs e)
        {
            ToolStripMenuItem mi = e.Item as ToolStripMenuItem;

            if (mi != null)
            {
                mi.DropDown.ItemAdded += OnMenuItemRemoved;
                mi.Click += OnMenuItemClicked;
            }
        }

        void OnMenuItemRemoved(object sender, ToolStripItemEventArgs e)
        {
            ToolStripMenuItem mi = e.Item as ToolStripMenuItem;

            if (mi != null)
            {
                mi.DropDown.ItemAdded -= OnMenuItemRemoved;
                mi.Click -= OnMenuItemClicked;
            }
        }

        void OnMenuItemClicked(object sender, EventArgs e)
        {
            this.workspace.DocumentView.Focus();
        }

        private Keys CharToKeys(char c)
        {
            Keys keys = Keys.None;
            c = Char.ToLower(c);

            if (c >= 'a' && c <= 'z')
            {
                keys = (Keys)((int)Keys.A + (int)c - (int)'a');
            }

            return keys;
        }

        private Keys GetMenuCmdKey(string text)
        {
            Keys keys = Keys.None;

            for (int i = 0; i < text.Length - 1; ++i)
            {
                if (text[i] == '&')
                {
                    keys = Keys.Alt | CharToKeys(text[i + 1]);
                    break;
                }
            }

            return keys;
        }

        bool workspace_ProcessCmdKeyEvent(object sender, ref Message msg, Keys keyData)
        {
            bool result = false;

            foreach (ToolStripMenuItem mi in this.mainMenu.Items)
            {
                Keys keys = GetMenuCmdKey(mi.Text);

                if (keyData == keys)
                {
                    mi.ShowDropDown();
                    result = true;
                }
            }

            return result;
        }

        private void DeleteUpdateMsi()
        {
            // If we just installed an update, then delete it! Save some hard drive space.
            string msiDeleteMeFull = Settings.CurrentUser.GetString(PdnSettings.UpdateMsiFileName, null);
            string msiDeleteMe = Path.GetFileName(msiDeleteMeFull); // make sure someone can't put "..\..\..\..\windows\system32\cmd.exe" or something
            string msiDeleteMeExt = Path.GetExtension(msiDeleteMe);

            if (msiDeleteMe != null &&
                (string.Compare(".msi", msiDeleteMeExt, true, CultureInfo.InvariantCulture) == 0 ||
                 string.Compare(".exe", msiDeleteMeExt, true, CultureInfo.InvariantCulture) == 0))
            {
                string tempDir = Environment.ExpandEnvironmentVariables("%TEMP%");
                string msiPath = Path.Combine(tempDir, msiDeleteMe);
                int retryCount = 3;

                while (retryCount > 0)
                {
                    try
                    {
                        File.Delete(msiPath);
                        retryCount = 0;
                    }

                    catch
                    {
                        Thread.Sleep(1000);
                    }

                    --retryCount;
                }

                try
                {
                    Settings.CurrentUser.Delete(PdnSettings.UpdateMsiFileName);
                }

                catch
                {
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            DeleteUpdateMsi();

            if (doKill)
            {
                Application.Exit();
            }

            this.floaters = new FloatingToolForm[] { 
                                                       workspace.Widgets.MainToolBarForm,
                                                       workspace.Widgets.ColorsForm,
                                                       workspace.Widgets.HistoryForm,
                                                       workspace.Widgets.LayerForm
                                                   };

            foreach (FloatingToolForm ftf in floaters)
            {
                ftf.Closing += this.hideInsteadOfCloseDelegate;
                ftf.KeyDown += new KeyEventHandler(FloatingForm_KeyDown);
                ftf.KeyUp += new KeyEventHandler(FloatingForm_KeyUp);
                ftf.MouseWheel += new MouseEventHandler(FloatingForm_MouseWheel);
                ftf.VisibleChanged += new EventHandler(ftf_VisibleChanged);
                this.Move += new EventHandler(ftf.RepositionForm); // TODO: eliminate OO misnomer?
            }

            workspace.Widgets.CommonActionsStrip.ButtonClick += new EnumValueEventHandler(CommonActionsStrip_ButtonClick);
            workspace.Environment.Selection.Changed += new EventHandler(Environment_SelectedPathChanged);
            workspace.DocumentView.Layout += new LayoutEventHandler(DocumentView_Layout);
            workspace.History.Changed += new EventHandler(History_Changed);

            //BeginInvoke(new VoidVoidDelegate(PositionFloatingForms), null);
            PositionFloatingForms();

            // Set up icons -- defer this until they actually click on a menu though
            RegisterMenuPopupFirstTimeDelegates();

            if (splash != null)
            {
                splash.Close();
                splash.Dispose();
                splash = null;
            }

            if (SystemLayer.Security.IsAdministrator &&
                PdnInfo.StartupTest == PdnInfo.StartupTestType.None)
            {
                // Should we check for updates? Only enable this stuff if the user is an admin.

                // If we previously determined an update was available, but if the
                // user hasn't yet pushed the green+arrow button, then show the button again!
                // This will allow us to keep the button in the UI w/o pinging the website
                // again until the user clicks the button, OR until several more days have
                // elapsed.
                if (Settings.SystemWide.GetBoolean(PdnSettings.UpdateIsAvailable, false))
                {
                    EnableUpdatesButton();
                }

                // Check for updates anyway
                if (Updates.ShouldCheckForUpdates())
                {
                    // Otherwise, let the Updates class determine if we should check for updates!
                    // And if it says yes, spawn the update checking in to a background thread.
                    Updates.PingLastUpdateCheckTime();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.BackgroundCheckForUpdates));
                }
            }

            base.OnLoad(e);

            switch (PdnInfo.StartupTest)
            {
                case PdnInfo.StartupTestType.Timed:
                    Application.DoEvents();
                    Application.Exit();
                    break;

                case PdnInfo.StartupTestType.WorkingSet:
                    const int waitPeriodForVadumpSnapshot = 20000;
                    Application.DoEvents();
                    Thread.Sleep(waitPeriodForVadumpSnapshot);
                    Application.Exit();
                    break;
            }
        }

        private void BackgroundCheckForUpdates()
        {
            BackgroundCheckForUpdates((object)(int)3000);
        }

        private void BackgroundCheckForUpdates(object context)
        {
            if (context != null)
            {
                Thread.Sleep((int)context);
            }

            try
            {
                CheckForUpdates(out this.versionManifest, out this.versionManifestIndex, out this.versionCheckException);

                if (context == null && this.versionManifest != null && this.versionManifestIndex != -1)
                {
                    this.BeginInvoke(new VoidVoidDelegate(this.EnableUpdatesButton));
                    this.BeginInvoke(new VoidVoidDelegate(this.ShowUpdateDialog));
                }
                else
                {
                    this.BeginInvoke(new VoidVoidDelegate(this.DisableUpdatesButton));
                    Settings.SystemWide.SetBoolean(PdnSettings.UpdateIsAvailable, false);
                }
            }

            catch
            {
            }
        }

        private void EnableUpdatesButton()
        {
            if (!this.workspace.Widgets.CommonActionsStrip.GetButtonEnabled(CommonAction.CheckForUpdates))
            {
                this.workspace.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CheckForUpdates, true);
                this.workspace.Widgets.CommonActionsStrip.SetButtonVisible(CommonAction.CheckForUpdates, true);
            }
        }

        private void DisableUpdatesButton()
        {
            if (this.workspace.Widgets.CommonActionsStrip.GetButtonEnabled(CommonAction.CheckForUpdates))
            {
                this.workspace.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CheckForUpdates, false);
                this.workspace.Widgets.CommonActionsStrip.SetButtonVisible(CommonAction.CheckForUpdates, false);
            }
        }

        private void CheckForUpdates(out PdnVersionManifest manifestResult, out int latestVersionIndexResult, 
            out Exception exception)
        {
            exception = null;
            PdnVersionManifest manifest = null;
            manifestResult = null;
            latestVersionIndexResult = -1;

            int retries = 2;

            while (retries > 0)
            {
                try
                {
                    manifest = Updates.GetManifest(out exception);
                    retries = 0;
                }

                catch (Exception ex)
                {
                    exception = ex;
                    --retries;

                    if (retries == 0)
                    {
                        manifest = null;
                    }
                }
            }

            if (manifest != null)
            {
                int stableIndex = manifest.GetLatestStableVersionIndex();
                int betaIndex = manifest.GetLatestBetaVersionIndex();

                // Check for betas as well?
                bool checkForBetas = ("1" == Settings.SystemWide.GetString(PdnSettings.AlsoCheckForBetas, "0"));

                // Figure out which version we want to compare against the current version
                int latestIndex = stableIndex;

                if (checkForBetas)
                {
                    // If they like betas, and if the beta is newer than the latest stable release,
                    // then offer it to them.
                    if (betaIndex != -1 && 
                        (stableIndex == -1 || manifest.VersionInfos[betaIndex].Version >= manifest.VersionInfos[stableIndex].Version))
                    {
                        latestIndex = betaIndex;
                    }
                }

                // Now compare that version against the current version
                if (latestIndex != -1)
                {
                    if (manifest.VersionInfos[latestIndex].Version > PdnInfo.GetVersion())
                    {
                        manifestResult = manifest;
                        latestVersionIndexResult = latestIndex;
                        Settings.SystemWide.SetBoolean(PdnSettings.UpdateIsAvailable, true);
                    }
                }
            }
        }

        private void ShowUpdateDialog()
        {
            if (this.versionManifest != null && this.versionManifestIndex != -1)
            {
                ShowUpdateDialog(this.versionManifest, this.versionManifestIndex);
            }
        }

        private delegate void ShowUpdateDialogDelegate(PdnVersionManifest manifest, int versionIndex);
        private void ShowUpdateDialog(PdnVersionManifest manifest, int versionIndex)
        {
            Settings.SystemWide.SetBoolean(PdnSettings.UpdateIsAvailable, false);
            UpdatesDialog updates = new UpdatesDialog();
            updates.EnableInstanceOpacity = false;
            updates.PdnVersionManifest = manifest;
            updates.PdnVersionManifestIndex = versionIndex;
            DialogResult result = updates.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                DownloadAndUnzipUpdateDialog dialog = new DownloadAndUnzipUpdateDialog(this, 
                    manifest.VersionInfos[versionIndex]);

                dialog.ShowDialog();

                if (dialog.UpgradeMsiFileName != null)
                {
                    string message = PdnResources.GetString("CheckForUpdates.MustClosePdn");
                    DialogResult result2 = Utility.InfoBoxOKCancel(this, message);

                    if (result2 == DialogResult.OK)
                    {
                        this.upgradeMsiFileName = dialog.UpgradeMsiFileName;
                        ClickOnMenuItem(this.menuFileExit);
                    }
                    else
                    {
                        this.upgradeMsiFileName = null;

                        try
                        {
                            File.Delete(dialog.UpgradeMsiFileName);
                        }

                        catch
                        {
                        }
                    }
                }
            }
        }

        private void CheckForUpdatesClickHandler()
        {
            DisableUpdatesButton();
            Settings.SystemWide.SetBoolean(PdnSettings.UpdateIsAvailable, false);

            if (this.versionManifest != null && this.versionManifestIndex != -1)
            {
                ShowUpdateDialog(this.versionManifest, this.versionManifestIndex);
            }
            else
            {
                this.ClickOnMenuItem(this.menuFileUpdatesCheckNow);
            }

            this.versionManifest = null;
            this.versionManifestIndex = -1;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(Application_Idle);
            LayerElement.SetAllowRefreshPreview(true);

            if (this.fileOpened)
            {
                this.workspace.Widgets.LayerControl.RefreshPreviews();
            }
        }

        private void PositionFloatingForms()
        {
            workspace.Widgets.MainToolBarForm.Reposition = WhichEdge.TopLeft;
            workspace.Widgets.ColorsForm.Reposition = WhichEdge.BottomLeft;
            workspace.Widgets.HistoryForm.Reposition = WhichEdge.TopRight;
            workspace.Widgets.LayerForm.Reposition = WhichEdge.BottomRight;

            foreach (FloatingToolForm ftf in floaters)
            {
                this.AddOwnedForm(ftf);
                ftf.Opacity = 1.0;
            }

            if (Settings.CurrentUser.GetBoolean(PdnSettings.ToolsFormVisible, true))
            {
                workspace.Widgets.MainToolBarForm.Show();
            }

            if (Settings.CurrentUser.GetBoolean(PdnSettings.ColorsFormVisible, true))
            {
                workspace.Widgets.ColorsForm.Show();
            }

            if (Settings.CurrentUser.GetBoolean(PdnSettings.HistoryFormVisible, true))
            {
                workspace.Widgets.HistoryForm.Show();
            }

            if (Settings.CurrentUser.GetBoolean(PdnSettings.LayersFormVisible, true))
            {
                workspace.Widgets.LayerForm.Show();
            }

            floaterOpacityTimer.Enabled = true;

            workspace.DocumentView.Focus();
        }

        protected override void OnResize(EventArgs e)
        {
            if (floaterOpacityTimer != null)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    if (this.floaterOpacityTimer.Enabled)
                    {
                        this.floaterOpacityTimer.Enabled = false;
                    }
                }
                else
                {
                    if (!this.floaterOpacityTimer.Enabled)
                    {
                        this.floaterOpacityTimer.Enabled = true;
                    }

                    this.floaterOpacityTimer_Tick(this, EventArgs.Empty);
                }
            }

            base.OnResize (e);
        }

        private void RegisterMenuPopupFirstTimeDelegates()
        {
            foreach (ToolStripMenuItem mi in mainMenu.Items)
            {
                mi.DropDownOpening += new EventHandler(MenuPopupFirstTimeHandler);
            }
        }

        private void UnregisterMenuPopupFirstTimeDelegates()
        {
            foreach (ToolStripMenuItem mi in mainMenu.Items)
            {
                mi.DropDownOpening -= new EventHandler(MenuPopupFirstTimeHandler);
            }
        }

        private bool menuItemNamesDone = false;
        private void InitMenuItemNames()
        {
            if (menuItemNamesDone)
            {
                return;
            }

            menuItemNamesDone = true;

            this.menuFileNew.Text = PdnResources.GetString("MainForm.Menu.File.New.Text");
            this.menuFileOpen.Text = PdnResources.GetString("MainForm.Menu.File.Open.Text");
            this.menuFileOpenRecent.Text = PdnResources.GetString("MainForm.Menu.File.OpenRecent.Text");
            this.menuFileAcquire.Text = PdnResources.GetString("MainForm.Menu.File.Acquire.Text");
            this.menuFileAcquireFromClipboard.Text = PdnResources.GetString("MainForm.Menu.File.Acquire.FromClipboard.Text");
            this.menuFileAcquireFromScannerOrCamera.Text = PdnResources.GetString("MainForm.Menu.File.Acquire.FromScannerOrCamera.Text");
            this.menuFileNewWindow.Text = PdnResources.GetString("MainForm.Menu.File.NewWindow.Text");
            this.menuFileOpenInNewWindow.Text = PdnResources.GetString("MainForm.Menu.File.OpenInNewWindow.Text");
            this.menuFileSave.Text = PdnResources.GetString("MainForm.Menu.File.Save.Text");
            this.menuFileSaveAs.Text = PdnResources.GetString("MainForm.Menu.File.SaveAs.Text");
            this.menuFilePrint.Text = PdnResources.GetString("MainForm.Menu.File.Print.Text");
            this.menuFileUpdates.Text = PdnResources.GetString("MainForm.Menu.File.Updates.Text");
            this.menuFileUpdatesAutoCheckEnabled.Text = PdnResources.GetString("MainForm.Menu.File.Updates.AutoCheckEnabled.Text");
            this.menuFileUpdatesCheckNow.Text = PdnResources.GetString("MainForm.Menu.File.Updates.CheckNow.Text");
            this.menuFileUpdatesCheckForBetas.Text = PdnResources.GetString("MainForm.Menu.File.Updates.CheckForBetas.Text");
            this.menuFileLanguage.Text = PdnResources.GetString("MainForm.Menu.File.Language.Text");
            this.menuFileExit.Text = PdnResources.GetString("MainForm.Menu.File.Exit.Text");

            this.menuEditUndo.Text = PdnResources.GetString("MainForm.Menu.Edit.Undo.Text");
            this.menuEditRedo.Text = PdnResources.GetString("MainForm.Menu.Edit.Redo.Text");
            this.menuEditCut.Text = PdnResources.GetString("MainForm.Menu.Edit.Cut.Text");
            this.menuEditCopy.Text = PdnResources.GetString("MainForm.Menu.Edit.Copy.Text");
            this.menuEditPaste.Text = PdnResources.GetString("MainForm.Menu.Edit.Paste.Text");
            this.menuEditPasteInToNewLayer.Text = PdnResources.GetString("MainForm.Menu.Edit.PasteInToNewLayer.Text");
            this.menuEditEraseSelection.Text = PdnResources.GetString("MainForm.Menu.Edit.EraseSelection.Text");
            this.menuEditInvertSelection.Text = PdnResources.GetString("MainForm.Menu.Edit.InvertSelection.Text");
            this.menuEditSelectAll.Text = PdnResources.GetString("MainForm.Menu.Edit.SelectAll.Text");
            this.menuEditDeselect.Text = PdnResources.GetString("MainForm.Menu.Edit.Deselect.Text");

            this.menuViewZoomIn.Text = PdnResources.GetString("MainForm.Menu.View.ZoomIn.Text");
            this.menuViewZoomOut.Text = PdnResources.GetString("MainForm.Menu.View.ZoomOut.Text");
            this.menuViewZoomToWindow.Text = PdnResources.GetString("MainForm.Menu.View.ZoomToWindow.Text");
            this.menuViewZoomToSelection.Text = PdnResources.GetString("MainForm.Menu.View.ZoomToSelection.Text");
            this.menuViewActualSize.Text = PdnResources.GetString("MainForm.Menu.View.ActualSize.Text");
            this.menuViewGrid.Text = PdnResources.GetString("MainForm.Menu.View.Grid.Text");
            this.menuViewRulers.Text = PdnResources.GetString("MainForm.Menu.View.Rulers.Text");
            this.menuViewUnits.Text = PdnResources.GetString("MainForm.Menu.View.Units.Text");
            this.menuViewUnitsPixels.Text = PdnResources.GetString("MeasurementUnit.Pixel.Plural");
            this.menuViewUnitsInches.Text = PdnResources.GetString("MeasurementUnit.Inch.Plural");
            this.menuViewUnitsCentimeters.Text = PdnResources.GetString("MeasurementUnit.Centimeter.Plural");

            this.menuImageCrop.Text = PdnResources.GetString("MainForm.Menu.Image.Crop.Text");
            this.menuImageResize.Text = PdnResources.GetString("MainForm.Menu.Image.Resize.Text");
            this.menuImageCanvasSize.Text = PdnResources.GetString("MainForm.Menu.Image.CanvasSize.Text");
            this.menuImageFlip.Text = PdnResources.GetString("MainForm.Menu.Image.Flip.Text");
            this.menuImageFlipHorizontal.Text = PdnResources.GetString("MainForm.Menu.Image.Flip.Horizontal.Text");
            this.menuImageFlipVertical.Text = PdnResources.GetString("MainForm.Menu.Image.Flip.Vertical.Text");
            this.menuImageRotate.Text = PdnResources.GetString("MainForm.Menu.Image.Rotate.Text");
            this.menuImageRotate90CW.Text = PdnResources.GetString("MainForm.Menu.Image.Rotate.90CW.Text");
            this.menuImageRotate180CW.Text = PdnResources.GetString("MainForm.Menu.Image.Rotate.180CW.Text");
            this.menuImageRotate270CW.Text = PdnResources.GetString("MainForm.Menu.Image.Rotate.270CW.Text");
            this.menuImageRotate90CCW.Text = PdnResources.GetString("MainForm.Menu.Image.Rotate.90CCW.Text");
            this.menuImageRotate180CCW.Text = PdnResources.GetString("MainForm.Menu.Image.Rotate.180CCW.Text");
            this.menuImageRotate270CCW.Text = PdnResources.GetString("MainForm.Menu.Image.Rotate.270CCW.Text");
            this.menuImageFlatten.Text = PdnResources.GetString("MainForm.Menu.Image.Flatten.Text");

            this.menuLayersAddNewLayer.Text = PdnResources.GetString("MainForm.Menu.Layers.AddNewLayer.Text");
            this.menuLayersDeleteLayer.Text = PdnResources.GetString("MainForm.Menu.Layers.DeleteLayer.Text");
            this.menuLayersDuplicateLayer.Text = PdnResources.GetString("MainForm.Menu.Layers.DuplicateLayer.Text");
            this.menuLayersImportFromFile.Text = PdnResources.GetString("MainForm.Menu.Layers.ImportFromFile.Text");
            this.menuLayersAdjustments.Text = PdnResources.GetString("MainForm.Menu.Layers.Adjustments.Text");
            this.menuLayersFlip.Text = PdnResources.GetString("MainForm.Menu.Layers.Flip.Text");
            this.menuLayersFlipHorizontal.Text = PdnResources.GetString("MainForm.Menu.Layers.Flip.Horizontal.Text");
            this.menuLayersFlipVertical.Text = PdnResources.GetString("MainForm.Menu.Layers.Flip.Vertical.Text");

            // Fill in Rotate/Zoom menu item
            string rzName = RotateZoomEffect.StaticName;
            Keys rzShortcut = RotateZoomEffect.StaticShortcutKeys;
            Image rzImage = RotateZoomEffect.StaticImage;
            string rzNameFormatString = PdnResources.GetString("MainForm.Effects.Name.Format.Configurable");
            string rzMenuName = string.Format(rzNameFormatString, rzName);
            this.menuLayersRotateZoom.Text = rzMenuName;
            this.SetMenuIcon(this.menuLayersRotateZoom, rzImage);
            this.menuLayersRotateZoom.ShortcutKeys = rzShortcut;

            this.menuLayersLayerProperties.Text = PdnResources.GetString("MainForm.Menu.Layers.LayerProperties.Text");

            this.menuToolsAntialiasing.Text = PdnResources.GetString("MainForm.Menu.Tools.AntiAliasing.Text");
            this.menuToolsAlphaBlending.Text = PdnResources.GetString("MainForm.Menu.Tools.AlphaBlending.Text");

            this.menuWindowResetWindowLocations.Text = PdnResources.GetString("MainForm.Menu.Window.ResetWindowLocations.Text");
            this.menuWindowTranslucent.Text = PdnResources.GetString("MainForm.Menu.Window.Translucent.Text");
            this.menuWindowTools.Text = PdnResources.GetString("MainForm.Menu.Window.Tools.Text");
            this.menuWindowHistory.Text = PdnResources.GetString("MainForm.Menu.Window.History.Text");
            this.menuWindowLayers.Text = PdnResources.GetString("MainForm.Menu.Window.Layers.Text");
            this.menuWindowColors.Text = PdnResources.GetString("MainForm.Menu.Window.Colors.Text");

            this.menuHelpHelpTopics.Text = PdnResources.GetString("MainForm.Menu.Help.HelpTopics.Text");
            this.menuHelpDonate.Text = PdnResources.GetString("MainForm.Menu.Help.Donate.Text");
            this.menuHelpAbout.Text = PdnResources.GetString("MainForm.Menu.Help.About.Text");
            this.menuHelpSendFeedback.Text = PdnResources.GetString("MainForm.Menu.Help.SendFeedback.Text");
        }

        private void MenuPopupFirstTimeHandler(object sender, EventArgs e)
        {
            InitMenuItemNames();
            InitMenuItemIcons();
            UnregisterMenuPopupFirstTimeDelegates();
        }

        private void InitMenuItemIcons()
        {
            Type ourType = this.GetType();

            FieldInfo[] fields = ourType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType == typeof(ToolStripMenuItem))
                {
                    string iconFileName = "Icons." + fi.Name[0].ToString().ToUpper() + fi.Name.Substring(1) + "Icon.png";
                    ToolStripMenuItem mi = (ToolStripMenuItem)fi.GetValue(this);
                    Stream iconStream = PdnResources.GetResourceStream(iconFileName);

                    if (iconStream != null)
                    {
                        Image iconImage = PdnResources.LoadImage(iconStream); //Image.FromStream(iconStream);
                        this.SetMenuIcon(mi, iconImage);
                    }
                }
            }
        }

        private void menuFileExit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private DialogResult AskForSave()
        {
            string format = PdnResources.GetString("MainForm.AskForSave");
            string text = string.Format(format, GetFriendlyName());
            return MessageBox.Show(this, text, PdnInfo.GetProductName(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Shows our "About" dialog box.
        /// </summary>
        private void menuHelpAbout_Click(object sender, System.EventArgs e)
        {
            using (AboutDialog af = new AboutDialog())
            {
                af.ShowDialog(this);
            }
        }

        private void menuFileAcquireFromScannerOrCamera_Click(object sender, System.EventArgs e)
        {
            if (!ScanningAndPrinting.CanScan)
            {
                ShowWiaError();
                return;
            }

            // first ...
            if (workspace.Document.Dirty)
            {
                switch (AskForSave())
                {
                    case DialogResult.Yes:
                        if (!DoSave())
                        {
                            return;
                        }

                        break;

                    case DialogResult.No:
                        break;

                    case DialogResult.Cancel:
                        return;
                }
            }

            string tempName = Path.ChangeExtension(SystemLayer.FileSystem.GetTempFileName(), ".bmp");
            ScanResult result = ScanningAndPrinting.Scan(this, tempName);

            if (result == ScanResult.Success)
            {
                string errorText = null;

                try
                {
                    Image image;

                    try
                    {
                        image = PdnResources.LoadImage(tempName);
                    }

                    catch (FileNotFoundException)
                    {
                        errorText = PdnResources.GetString("MainForm.LoadDocument.Error.FileNotFoundException");
                        throw;
                    }

                    catch (OutOfMemoryException)
                    {
                        errorText = PdnResources.GetString("MainForm.LoadDocument.Error.OutOfMemoryException");
                        throw;
                    }

                    Document document;

                    try
                    {
                        document = Document.FromImage(image);
                    }

                    catch (OutOfMemoryException)
                    {
                        errorText = PdnResources.GetString("MainForm.LoadDocument.Error.OutOfMemoryException");
                        throw;
                    }

                    finally
                    {
                        image.Dispose();
                        image = null;
                    }

                    try
                    {
                        workspace.SetDocument(document);
                    }

                    catch (OutOfMemoryException)
                    {
                        errorText = PdnResources.GetString("MainForm.LoadDocument.Error.OutOfMemoryException");
                        throw;
                    }

                    document = null;
                    workspace.SetDocumentSaveOptions(null, null, null);
                    workspace.History.ClearAll();
                    HistoryAction newHA = new NullHistoryAction(PdnResources.GetString("AcquireImageAction.Name"), this.AddNewLayerIcon);
                    workspace.History.PushNewAction(newHA);

                    // Try to delete the temp file but don't worry if we can't
                    try
                    {
                        File.Delete(tempName);
                    }

                    catch
                    {
                    }
                }

                catch (Exception)
                {
                    if (errorText != null)
                    {
                        Utility.ErrorBox(this, errorText);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private void menuFileAcquireFromClipboard_Click(object sender, System.EventArgs e)
        {
            if (workspace.Document.Dirty)
            {
                switch (AskForSave())
                {
                    case DialogResult.Yes:
                        ClickOnMenuItem(this.menuFileSave);
                        break;

                    case DialogResult.No:
                        break;

                    case DialogResult.Cancel:
                        return;
                }
            }

            try
            {
                IDataObject pasted;
                Image image;

                using (new WaitCursorChanger(this))
                {
                    Utility.GCFullCollect();
                    pasted = Clipboard.GetDataObject();
                    image = (Image)pasted.GetData(DataFormats.Bitmap);
                }

                if (image == null)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("AcquireImageAction.Error.Clipboard.NoAcquirableImage"));
                }
                else
                {
                    Document document = null;

                    try
                    {
                        using (new WaitCursorChanger(this))
                        {
                            Utility.GCFullCollect();
                            document = Document.FromImage(image);
                            workspace.SetDocument(document);
                            workspace.SetDocumentSaveOptions(null, null, null);
                            workspace.History.ClearAll();

                            workspace.History.PushNewAction(new NullHistoryAction(PdnResources.GetString("AcquireImageAction.Name"), 
                                this.AddNewLayerIcon));

                            Invalidate();
                        }
                    }

                    catch
                    {
                        Utility.ErrorBox(this, PdnResources.GetString("AcquireImageAction.Error.Clipboard.TransferError"));
                    }
                }
            }

            catch (ExternalException)
            {
                Utility.ErrorBox(this, PdnResources.GetString("AcquireImageAction.Error.Clipboard.TransferError"));
                return;
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(this, PdnResources.GetString("AcquireImageAction.Error.Clipboard.OutOfMemory"));
                return;
            }

            catch (ThreadStateException)
            {
                // The ApartmentState property of the application is not set to ApartmentState.STA
                // I don't think this one will ever happen, seeing as how Main is tagged with the
                // STA attribute.
                return;
            }
        }

        private string imageInfoStatusBarFormat = PdnResources.GetString("MainForm.StatusBar.Size.Format");
        private void workspace_DocumentChanged(object sender, System.EventArgs e)
        {
            SetTitleText();

            if (workspace.Document != null)
            {
                string widthString;
                string heightString;
                string units;

                CoordinatesToStrings(workspace.Document.Width, workspace.Document.Height, 
                    out widthString, out heightString, out units);

                string imageText = string.Format(
                    CultureInfo.InvariantCulture, 
                    this.imageInfoStatusBarFormat, 
                    widthString, 
                    units, 
                    heightString, 
                    units);

                this.imageInfoStatusLabel.Text = imageText;
            }

            OnResize(EventArgs.Empty);
        }

        private string GetFriendlyName()
        {
            string title;

            if (workspace.DocumentFileName != null)
            {
                title = Path.GetFileName(workspace.DocumentFileName);
            }
            else
            {
                title = PdnResources.GetString("MainForm.Untitled");
            }

            return title;
        }

        private void SetTitleText()
        {
            if (workspace == null) 
            {
                return;
            }

            string appTitle = PdnInfo.GetAppName();
            string ratio = string.Empty;
            string title = string.Empty;
            string friendlyName = GetFriendlyName();
            string text;

            if (this.WindowState != FormWindowState.Minimized) 
            {
                string format = PdnResources.GetString("MainForm.Text.Format.Normal");
                text = string.Format(format, friendlyName, workspace.DocumentView.ScaleFactor, appTitle);
            }
            else
            {
                string format = PdnResources.GetString("MainForm.Text.Format.Minimized");
                text = string.Format(format, friendlyName, appTitle);
            }

            if (workspace.Document != null)
            {
                title = text;
            }

            if (!PdnInfo.IsFinalBuild)
            {
                string titleWithExpiryFormat = PdnResources.GetString("MainForm.Text.Format.ExpiryIncluded");
                title = string.Format(titleWithExpiryFormat, title, PdnInfo.ExpirationDate.ToShortDateString());
            }

            Text = title;
        }

        private void menuEditUndo_Click(object sender, System.EventArgs e)
        {
            if (!workspace.DocumentView.IsMouseCaptured())
            {
                workspace.Widgets.HistoryForm.PerformUndoClick();
            }
        }

        private void menuEditRedo_Click(object sender, System.EventArgs e)
        {
            if (!workspace.DocumentView.IsMouseCaptured())
            {
                workspace.Widgets.HistoryForm.PerformRedoClick();
            }
        }

        private void menuFileNew_Click(object sender, System.EventArgs e)
        {
            StringBuilder sbTrace = new StringBuilder();

            try
            {
                sbTrace.Append("1 ");
                if (workspace.Document.Dirty)
                {
                    sbTrace.Append("2 ");
                    switch (AskForSave())
                    {
                        case DialogResult.Yes:
                            sbTrace.Append("3 ");
                            if (!DoSave())
                            {
                                sbTrace.Append("4 ");
                                return;
                            }

                            sbTrace.Append("5 ");
                            break;

                        case DialogResult.No:
                            sbTrace.Append("6 ");
                            break;

                        case DialogResult.Cancel:
                            sbTrace.Append("7 ");
                            return;
                    }
                }

                using (NewFileDialog nfd = new NewFileDialog())
                {
                    sbTrace.Append("8 ");
                    Size newDocSize = GetNewDocumentSize();
                    sbTrace.Append("9 ");

                    if (IsClipboardImageAvailable())
                    {
                        sbTrace.Append("10 ");
                        try
                        {
                            sbTrace.Append("11 ");
                            Utility.GCFullCollect();
                            sbTrace.Append("12 ");
                            IDataObject clipData = Clipboard.GetDataObject();
                            sbTrace.Append("13 ");

                            using (Image clipImage = (Image)clipData.GetData(DataFormats.Bitmap))
                            {
                                sbTrace.Append("14 ");
                                int width2 = clipImage.Width;
                                sbTrace.Append("15 ");
                                int height2 = clipImage.Height;
                                sbTrace.Append("16 ");
                                newDocSize = new Size(width2, height2);
                            }
                        }

                        catch (Exception ex)
                        {
                            if (ex is OutOfMemoryException)
                            {
                                sbTrace.Append("17 ");
                            }
                            else if (ex is ExternalException)
                            {
                                sbTrace.Append("18 ");
                            }
                            else if (ex is NullReferenceException)
                            {
                                sbTrace.Append("18_2 ");
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    nfd.OriginalSize = new Size(newDocSize.Width, newDocSize.Height);
                    sbTrace.Append("19 ");
                    nfd.OriginalDpuUnit = PdnSettings.GetLastNonPixelUnits();
                    sbTrace.Append("20 ");
                    nfd.OriginalDpu = Document.GetDefaultDpu(nfd.OriginalDpuUnit);
                    sbTrace.Append("21 ");
                    nfd.Units = nfd.OriginalDpuUnit;
                    sbTrace.Append("22 ");
                    nfd.Resolution = nfd.OriginalDpu;
                    sbTrace.Append("23 ");
                    nfd.ConstrainToAspect = Settings.CurrentUser.GetBoolean(PdnSettings.LastMaintainAspectRatioNF, false);
                    sbTrace.Append("24 ");

                    if (Utility.ShowDialog(nfd, this) == DialogResult.OK)
                    {
                        sbTrace.Append("25 ");
                        CreateBlankDocument(new Size(nfd.ImageWidth, nfd.ImageHeight), nfd.Units, nfd.Resolution);

                        sbTrace.Append("25a ");
                        workspace.ZoomBasis = ZoomBasis.Window;

                        sbTrace.Append("26 ");
                        Settings.CurrentUser.SetBoolean(PdnSettings.LastMaintainAspectRatioNF, nfd.ConstrainToAspect);
                        sbTrace.Append("27 ");

                        if (nfd.Units != MeasurementUnit.Pixel)
                        {
                            sbTrace.Append("28 ");
                            Settings.CurrentUser.SetString(PdnSettings.LastNonPixelUnits, nfd.Units.ToString());
                        }
                        sbTrace.Append("29 ");

                        if (workspace.Environment.Units != MeasurementUnit.Pixel)
                        {
                            sbTrace.Append("30 ");
                            workspace.Environment.Units = nfd.Units;
                        }
                    }
                }
            }

            // We're getting crash reports in this function. I have an idea of the cause
            // but nothing concrete. This should help.          
            catch (Exception ex)
            {
                throw new ApplicationException("Traced code path: " + sbTrace.ToString(), ex);
            }
        }

        private void CreateBlankDocument(Size size, MeasurementUnit dpu, double dpuUnit)
        {
            workspace.DocumentView.SuspendRefresh();

            try
            {
                Document untitled = new Document(size.Width, size.Height); 
                BitmapLayer bitmapLayer;
            
                try
                {
                    using (new WaitCursorChanger(this))
                    {
                        bitmapLayer = Layer.CreateBackgroundLayer(size.Width, size.Height);
                    }
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("NewImageAction.Error.OutOfMemory"));
                    return;
                }

                using (new WaitCursorChanger(this))
                {
                    untitled.Layers.Add(bitmapLayer);
                    workspace.SetDocument(untitled);
                    workspace.SetDocumentSaveOptions(null, null, null);
                    workspace.History.ClearAll();
                    workspace.History.PushNewAction(new NullHistoryAction(PdnResources.GetString("NewImageAction.Name"), this.FileNewIcon));
                    workspace.Document.Dirty = false;
                }
            }

            finally
            {
                workspace.DocumentView.ResumeRefresh();
            }

            SetTitleText();
        }

        public static Document LoadDocument(Control parent, string fileName, out FileType fileTypeResult)
        {
            return LoadDocument(parent, fileName, Point.Empty, false, out fileTypeResult);
        }

        public static Document LoadDocument(Control parent, string fileName, Point progressDialogStartPos, bool useStartPos, out FileType fileTypeResult)
        {
            FileTypeCollection fileTypes = FileTypes.GetFileTypes();
            int ftIndex = fileTypes.IndexOfExtension(Path.GetExtension(fileName));
            fileTypeResult = null;

            if (ftIndex == -1)
            {
                Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.ImageTypeNotRecognized"));
                return null;
            }

            FileType fileType = fileTypes[ftIndex];
            fileTypeResult = fileType;

            Document document = null;

            using (new WaitCursorChanger(parent))
            {
                Utility.GCFullCollect();
                Stream stream = null;

                try
                {
                    LoadProgressDialog ld = null;

                    try
                    {
                        stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                        ld = new LoadProgressDialog(parent, stream, fileType);

                        if (useStartPos)
                        {
                            document = ld.Load(progressDialogStartPos);
                        }
                        else
                        {
                            document = ld.Load();
                        }
                    }

                    catch (WorkerThreadException ex)
                    {
                        if (ld != null && ld.Cancelled)
                        {
                            if (document != null)
                            {
                                document.Dispose();
                                document = null;
                            }
                        }
                        else
                        {
                            Type innerExType = ex.InnerException.GetType();
                            ConstructorInfo ci = innerExType.GetConstructor(new Type[] { typeof(string), typeof(Exception) });

                            if (ci == null)
                            {
                                throw;
                            }
                            else
                            {
                                Exception ex2 = (Exception)ci.Invoke(new object[] { "Worker thread threw an exception of this type", ex.InnerException });
                                throw ex2;
                            }
                        }
                    }
                }

                catch (ArgumentException)
                {
                    if (fileName.Length == 0)
                    {
                        Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.BlankFileName"));
                    }
                    else
                    {
                        Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.ArgumentException"));
                    }
                }

                catch (UnauthorizedAccessException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.UnauthorizedAccessException"));
                }
    
                catch (SecurityException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.SecurityException"));
                }

                catch (FileNotFoundException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.FileNotFoundException"));
                }

                catch (DirectoryNotFoundException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.DirectoryNotFoundException"));
                }

                catch (PathTooLongException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.PathTooLongException"));
                }

                catch (IOException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.IOException"));
                }

                catch (SerializationException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.SerializationException"));
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.OutOfMemoryException"));
                }

                catch (Exception)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("MainForm.LoadDocument.Error.Exception"));
                }

                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }
                }
            }

            return document;
        }

        private bool DoOpenFile(string fileName)
        {
            return DoOpenFile(fileName, true);
        }

        private bool DoOpenFile(string fileName, bool askToSaveChanges)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (askToSaveChanges)
            {
                if (workspace.Document != null && workspace.Document.Dirty)
                {
                    switch (AskForSave())
                    {
                        case DialogResult.Yes:
                            if (!DoSave())
                            {
                                return false;
                            }

                            break;

                        case DialogResult.No:
                            break;

                        case DialogResult.Cancel:
                            return false;
                    }
                }
            }

            // Keep the old document around in case an error occurs so we can revert back to it
            Document oldDocument = workspace.Document;

            // If the splash form is still open, that means we're opening a file at startup
            // So position the "Loading" dialog box so it does not overlap the splash form
            Point startPos = Point.Empty;
            bool useStartPos = false;

            if (splash != null)
            {
                startPos = splash.Location;
                        
                startPos.X += splash.Width / 2;
                startPos.Y += splash.Height;
                startPos.Y += 6;

                useStartPos = true;
            }

            FileType fileType;
            Document document;
            document = LoadDocument(this, fileName, startPos, useStartPos, out fileType);

            if (document == null)
            {
                if (workspace.Document != oldDocument)
                {
                    using (new WaitCursorChanger(this))
                    {
                        workspace.SetDocument(oldDocument);
                    }
                }

                this.Cursor = Cursors.Default;
            }
            else
            {
                using (new WaitCursorChanger(this))
                {
                    workspace.SetDocumentSaveOptions(fileName, fileType, null);
                    workspace.SetDocument(document);
                    workspace.History.ClearAll();

                    workspace.History.PushNewAction(new NullHistoryAction(PdnResources.GetString("OpenImageAction.Name"), 
                        this.ImageFromDiskIcon));

                    workspace.Document.Dirty = false;
                }

                SetTitleText();
            
                if (document != null)
                {
                    workspace.ZoomBasis = ZoomBasis.Window;
                } 

                // add to MRU list
                AddMru(fileName);

                // warn about version?
                // 2.1 Build 1897 signifies when the file format changed and broke backwards compatibility (for saving)
                // 2.1 Build 1921 signifies when MemoryBlock was upgraded to support 64-bits, which broke it again
                // 2.1 Build 1924 upgraded to "unimportant ordering" for MemoryBlock serialization so we can to faster multiproc saves
                //                (in v2.5 we always save in order, although that doesn't change the file format's laxness)
                // 2.5 Build 2105 changed the way PropertyItems are serialized
                // 2.6 Build      upgrade to .NET 2.0, does not appear to be compatible with 2.5 and earlier files as a result
                if (workspace.Document.SavedWithVersion < new Version(2, 6, 0))
                {
                    Version ourVersion = PdnInfo.GetVersion();
                    Version ourVersion2 = new Version(ourVersion.Major, ourVersion.Minor);
                    Version ourVersion3 = new Version(ourVersion.Major, ourVersion.Minor, ourVersion.Build);

                    int fields;
                    if (workspace.Document.SavedWithVersion < ourVersion2)
                    {
                        fields = 2;
                    }
                    else 
                    {
                        fields = 3;
                    }

                    string format = PdnResources.GetString("MainForm.LoadDocument.Warning.SavedWithOlderVersion.Format");
                    string text = string.Format(format, workspace.Document.SavedWithVersion.ToString(fields),
                        new Version(Application.ProductVersion).ToString(fields));

                    // TODO: should we even bother to inform them? It is probably more annoying than not,
                    //       especially since older versions will say "Hey this file is corrupt OR saved with a newer version"
                    //Utility.InfoBox(this, text);
                }
            }

            return document != null;
        }

        private static bool NullGetThumbnailImageAbort()
        {
            return false;
        }

        /// <summary>
        /// Takes the current Document and ass it to the MRU list with the given absolute filename.
        /// </summary>
        /// <param name="fileName"></param>
        private void AddMru(string fileName)
        {
            Type oldToolType = workspace.Environment.GetToolType();
            workspace.Environment.SetTool(null);

            try
            {
                Surface renderSurface = workspace.ScratchSurface;
                string fullFileName = Path.GetFullPath(fileName);

                // Figure out size to use
                Rectangle bounds = workspace.Document.Bounds;
                int edgeLength = MainForm.mruIconSize;
                Size thumbSize;

                if (bounds.Width > bounds.Height)
                {
                    thumbSize = new Size(edgeLength, Math.Max(1, (bounds.Height * edgeLength) / bounds.Width));
                }
                else if (bounds.Height > bounds.Width)
                {
                    thumbSize = new Size(Math.Max(1, (bounds.Width * edgeLength) / bounds.Height), edgeLength);
                }
                else // if (bounds.Height == bounds.Width)
                {
                    thumbSize = new Size(edgeLength, edgeLength);
                }

                // Render the thumbnail
                using (RenderArgs ra = new RenderArgs(renderSurface))
                {
                    workspace.Document.Render(ra, workspace.Document.Bounds);
                }

                // Resize
                Surface thumbInset = new Surface(thumbSize);
                thumbInset.FitSurface(ResamplingAlgorithm.SuperSampling, renderSurface);

                // Sharpen it
                using (RenderArgs ra = new RenderArgs(thumbInset))
                {
                    new SharpenEffect().RenderInPlace(ra, ra.Bounds);
                }

                // Put it inside a square bitmap
                Surface thumb = new Surface(2 + edgeLength, 2 + edgeLength);

                using (RenderArgs ra = new RenderArgs(thumb))
                {
                    ra.Graphics.Clear(Color.FromArgb(0, 0, 0, 0));

                    Rectangle dstRect = new Rectangle((thumb.Width - thumbSize.Width) / 2, 
                        (thumb.Height - thumbSize.Height) / 2, thumbSize.Width, thumbSize.Height);

                    thumb.CopySurface(thumbInset, dstRect.Location);
                    ra.Graphics.DrawLines(Pens.Black, new Point[] { 
                                                                      new Point(dstRect.Left, dstRect.Top),
                                                                      new Point(dstRect.Right, dstRect.Top),
                                                                      new Point(dstRect.Right, dstRect.Bottom),
                                                                      new Point(dstRect.Left, dstRect.Bottom),
                                                                      new Point(dstRect.Left, dstRect.Top)
                                                                  });

                    thumbInset.Dispose();

                    renderSurface = null;

                    MostRecentFile mrf = new MostRecentFile(fullFileName, Utility.FullCloneBitmap(ra.Bitmap));

                    if (mostRecentFiles == null)
                    {
                        LoadMruList();
                    }

                    if (mostRecentFiles.Contains(fullFileName))
                    {
                        mostRecentFiles.Remove(fullFileName);
                    }

                    mostRecentFiles.Add(mrf);
                    SaveMruList();
                }
            }

            finally
            {
                workspace.Environment.SetTool(oldToolType, workspace);
            }
        }

        public static DialogResult ChooseFile(Control parent, out string fileName)
        {
            return ChooseFile(parent, out fileName, null);
        }

        public static DialogResult ChooseFile(Control parent, out string fileName, string startingDir)
        {
            string[] fileNames;
            DialogResult result = ChooseFiles(parent, out fileNames, false, startingDir);

            if (result == DialogResult.OK)
            {
                fileName = fileNames[0];
            }
            else
            {
                fileName = null;
            }

            return result;
        }

        public static DialogResult ChooseFiles(Control parent, out string[] fileNames, bool multiselect)
        {
            return ChooseFiles(parent, out fileNames, multiselect, null);
        }

        public static DialogResult ChooseFiles(Control parent, out string[] fileNames, bool multiselect, string startingDir)
        {
            FileTypeCollection fileTypes = FileTypes.GetFileTypes();

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (startingDir != null)
                {
                    ofd.InitialDirectory = startingDir;
                }
                else
                {
                    ofd.InitialDirectory = GetDefaultSavePath();
                }

                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Multiselect = multiselect;
                ofd.RestoreDirectory = true;

                ofd.Filter = fileTypes.ToString(true, PdnResources.GetString("MainForm.ChooseFiles.AllImages"), false, true);
                ofd.FilterIndex = 0;

                DialogResult result = ShowFileDialog(parent, ofd);
                fileNames = ofd.FileNames;

                return result;
            }
        }

        private void menuFileOpen_Click(object sender, System.EventArgs e)
        {
            if (workspace.Document != null && workspace.Document.Dirty)
            {
                switch (AskForSave())
                {
                    case DialogResult.Yes:
                        if (!DoSave())
                        {
                            return;
                        }

                        break;

                    case DialogResult.No:
                        break;

                    case DialogResult.Cancel:
                        return;
                }
            }
            
            string fileName;
            FileType fileType;
            SaveConfigToken saveConfigToken;
            workspace.GetDocumentSaveOptions(out fileName, out fileType, out saveConfigToken);
            string filePath = Path.GetDirectoryName(fileName);

            string newFileName;
            DialogResult result = ChooseFile(this, out newFileName, filePath);

            if (result == DialogResult.OK)
            {
                DoOpenFile(newFileName, false);
            }
        }

        private static string GetDefaultSaveName()
        {
            return PdnResources.GetString("MainForm.Untitled");
        }

        private static string GetDefaultSavePath()
        {
            string myPics = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string dir = Settings.CurrentUser.GetString(PdnSettings.LastFileDialogDirectory, null);

            if (dir == null)
            {
                dir = myPics;
            }
            else
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);

                    if (!dirInfo.Exists)
                    {
                        dir = myPics;
                    }
                }

                catch
                {
                    dir = myPics;
                }
            }

            return dir;
        }

        /// <summary>
        /// Shows an OpenFileDialog or SaveFileDialog and populates the InitialDirectory from the global
        /// settings repository if possible.
        /// </summary>
        /// <param name="fd">The FileDialog to show.</param>
        /// <remarks>
        /// The FileDialog should already have its InitialDirectory populated as a suggestion of where to start.
        /// </remarks>
        private static DialogResult ShowFileDialog(Control owner, FileDialog fd)
        {
            string initialDirectory;
            initialDirectory = Settings.CurrentUser.GetString(PdnSettings.LastFileDialogDirectory, fd.InitialDirectory);

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(initialDirectory);

                using (new WaitCursorChanger(owner))
                {
                    if (!dirInfo.Exists)
                    {
                        initialDirectory = fd.InitialDirectory;
                    }
                }
            }

            catch
            {
                initialDirectory = null;
            }

            fd.InitialDirectory = initialDirectory;
            DialogResult result = UI.ShowFileDialogWithThumbnailView(owner, fd);
            
            if (result == DialogResult.OK)
            {
                string newDir = Path.GetDirectoryName(fd.FileNames[0]);
                Settings.CurrentUser.SetString(PdnSettings.LastFileDialogDirectory, newDir);
            }

            return result;
        }

        /// <summary>
        /// Use this to get a save config token. You should already know the filename and file type.
        /// An existing save config token is optional and will be used to pre-populate the config dialog.
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="saveConfigToken"></param>
        /// <param name="newSaveConfigToken"></param>
        /// <returns>false if the user cancelled, otherwise true</returns>
        /// <remarks>Assumes that no Tool is active, as it uses the ScratchSurface.</remarks>
        private bool GetSaveConfigToken(FileType fileType, SaveConfigToken saveConfigToken, out SaveConfigToken newSaveConfigToken)
        {
            if (fileType.SupportsConfiguration)
            {
                using (SaveConfigDialog scd = new SaveConfigDialog())
                {
                    ProgressEventHandler peh = delegate(object sender, ProgressEventArgs e)
                    {
                        if (e.Percent < 0 || e.Percent >= 100)
                        {
                            ResetProgressStatusBar();
                            EraseProgressStatusBar();
                        }
                        else
                        {
                            SetProgressStatusBar(e.Percent);
                        }
                    };

                    if (fileType.SavesWithProgress)
                    {
                        scd.Progress += peh;
                    }

                    scd.Document = workspace.Document;
                    scd.FileType = fileType;

                    SaveConfigToken token = fileType.GetLastSaveConfigToken();
                    if (saveConfigToken != null &&
                        token.GetType() == saveConfigToken.GetType())
                    {
                        scd.SaveConfigToken = saveConfigToken;
                    }

                    scd.RenderSurface = workspace.ScratchSurface;
                    scd.EnableInstanceOpacity = false;

                    // show configuration/preview dialog
                    DialogResult dr = scd.ShowDialog(this);

                    if (fileType.SavesWithProgress)
                    {
                        scd.Progress -= peh;
                        ResetProgressStatusBar();
                        EraseProgressStatusBar();
                    }

                    if (dr == DialogResult.OK)
                    {
                        newSaveConfigToken = scd.SaveConfigToken;
                        return true;
                    }
                    else
                    {
                        newSaveConfigToken = null;
                        return false;
                    }
                }
            }
            else
            {
                newSaveConfigToken = fileType.GetLastSaveConfigToken();
                return true;
            }
        }

        /// <summary>
        /// Used to set the file name, file type, and save config token
        /// </summary>
        /// <param name="newFileName"></param>
        /// <param name="newFileType"></param>
        /// <param name="newSaveConfigToken"></param>
        /// <returns>true if the user clicked through and accepted, or false if they cancelled at any point</returns>
        private bool DoSaveAsDialog(out string newFileName, out FileType newFileType, out SaveConfigToken newSaveConfigToken)
        {
            FileTypeCollection fileTypes = FileTypes.GetFileTypes();

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.AddExtension = true;
                sfd.CheckPathExists = true;
                sfd.DefaultExt = string.Empty;
                sfd.OverwritePrompt = true;
                sfd.Filter = fileTypes.ToString(false, null, true, false);

                string fileName;
                FileType fileType;
                SaveConfigToken saveConfigToken;
                workspace.GetDocumentSaveOptions(out fileName, out fileType, out saveConfigToken);

                if (workspace.Document.Layers.Count > 1 && fileType != null && !fileType.SupportsLayers)
                {
                    fileType = null;
                }

                if (fileType == null)
                {
                    if (workspace.Document.Layers.Count == 1)
                    {
                        fileType = PdnFileTypes.Png;
                    }
                    else
                    {
                        fileType = PdnFileTypes.Pdn;
                    }

                    fileName = Path.ChangeExtension(fileName, fileType.DefaultExtension);
                }

                if (fileName == null)
                {
                    string name = GetDefaultSaveName();
                    fileName = Path.Combine(GetDefaultSavePath(), Path.ChangeExtension(name, fileType.DefaultExtension));
                }

                sfd.FileName = Path.ChangeExtension(fileName, null);
                sfd.FilterIndex = 1 + fileTypes.IndexOfFileType(fileType);
                sfd.InitialDirectory = Path.GetDirectoryName(fileName);
                sfd.RestoreDirectory = true;
                sfd.ShowHelp = false;
                sfd.Title = PdnResources.GetString("MainForm.SaveAsDialog.Title");
                sfd.ValidateNames = true;

                DialogResult dr1 = ShowFileDialog(this, sfd);
                bool result;

                if (dr1 != DialogResult.OK)
                {
                    result = false;
                }
                else
                {
                    fileName = sfd.FileName;
                    FileType fileType2 = fileTypes[sfd.FilterIndex - 1];
                    result = GetSaveConfigToken(fileType2, saveConfigToken, out saveConfigToken);
                    fileType = fileType2;
                }

                if (result)
                {
                    newFileName = fileName;
                    newFileType = fileType;
                    newSaveConfigToken = saveConfigToken;
                }
                else
                {
                    newFileName = null;
                    newFileType = null;
                    newSaveConfigToken = null;
                }

                return result;
            }
        }

        /// <summary>
        /// Does the grunt work to do a File->Save As operation.
        /// </summary>
        /// <returns><b>true</b> if the file was saved correctly, <b>false</b> if the user cancelled</returns>
        private bool DoSaveAs()
        {
            Type oldToolType = workspace.Environment.GetToolType();
            workspace.Environment.SetTool(null);

            try
            {
                string fileName;
                FileType fileType;
                SaveConfigToken saveConfigToken;

                bool result = DoSaveAsDialog(out fileName, out fileType, out saveConfigToken);

                if (result)
                {
                    string oldFileName;
                    FileType oldFileType;
                    SaveConfigToken oldSaveConfigToken;
                    workspace.GetDocumentSaveOptions(out oldFileName, out oldFileType, out oldSaveConfigToken);

                    workspace.SetDocumentSaveOptions(fileName, fileType, saveConfigToken);
                    bool result2 = DoSave(true);

                    if (!result2)
                    {
                        workspace.SetDocumentSaveOptions(oldFileName, oldFileType, oldSaveConfigToken);
                    }

                    return result2;
                }
                else
                {
                    return false;
                }
            }

            finally
            {
                workspace.Environment.SetTool(oldToolType, this.workspace);
            }
        }

        private void menuFileSaveAs_Click(object sender, System.EventArgs e)
        {
            DoSaveAs();
        }

        /// <summary>
        /// Warns the user that we need to flatten the image.
        /// </summary>
        /// <returns>Returns DialogResult.Yes if they want to proceed or DialogResult.No if they don't.</returns>
        private DialogResult WarnAboutFlattening()
        {
            return MessageBox.Show(this, PdnResources.GetString("MainForm.WarnAboutFlattening.Text"),
                PdnResources.GetString("MainForm.WarnAboutFlattening.Title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private bool DoSave()
        {
            return DoSave(false);
        }

        /// <summary>
        /// Does the dirty work for a File->Save operation. If any of the "Save Options" in the
        /// DocumentWorkspace are null, this will call DoSaveAs(). If the image has more than 1
        /// layer but the file type they want to save with does not support layers, then it will
        /// ask the user about flattening the image.
        /// </summary>
        /// <param name="tryToFlatten">
        /// If true, will ask the user about flattening if the workspace's saveFileType does not 
        /// support layers and the image has more than 1 layer.
        /// If false, then DoSaveAs will be called and the fileType will be prepopulated with
        /// the .PDN type.
        /// </param>
        /// <returns><b>true</b> if the file was saved, <b>false</b> if the user cancelled</returns>
        private bool DoSave(bool tryToFlatten)
        {
            Type oldToolType = workspace.Environment.GetToolType();
            workspace.Environment.SetTool(null);

            try
            {
                string fileName;
                FileType fileType;
                SaveConfigToken saveConfigToken;

                workspace.GetDocumentSaveOptions(out fileName, out fileType, out saveConfigToken);

                // if they haven't specified a filename, then revert to "Save As" behavior
                if (fileName == null)
                {
                    return DoSaveAs();
                }

                // if we have a filename but no file type, try to infer the file type
                if (fileType == null)
                {
                    FileTypeCollection fileTypes = FileTypes.GetFileTypes();
                    string ext = Path.GetExtension(fileName);
                    int index = fileTypes.IndexOfExtension(ext);
                    FileType inferredFileType = fileTypes[index];
                    fileType = inferredFileType;
                }

                // if the image has more than 1 layer but is saving with a file type that
                // does not support layers, then we must ask them if we may flatten the
                // image first
                if (workspace.Document.Layers.Count > 1 && !fileType.SupportsLayers)
                {
                    if (!tryToFlatten)
                    {
                        return DoSaveAs();
                    }
                    else
                    {
                        DialogResult dr = WarnAboutFlattening();

                        if (dr == DialogResult.Yes)
                        {
                            if (!workspace.Environment.Selection.IsEmpty)
                            {
                                workspace.PerformAction(typeof(DeselectAction));
                            }

                            workspace.PerformAction(typeof(FlattenAction));
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                // get the configuration!
                if (saveConfigToken == null)
                {
                    bool result = GetSaveConfigToken(fileType, saveConfigToken, out saveConfigToken);

                    if (!result)
                    {
                        return false;
                    }
                }

                // At this point fileName, fileType, and saveConfigToken must all be non-null

                // if the document supports custom headers, embed a thumbnail in there
                if (fileType.SupportsCustomHeaders)
                {
                    using (new WaitCursorChanger(this))
                    {
                        Utility.GCFullCollect();
                        const int maxDim = 96; // 96x96 is the size that Windows asks for in Thumbnail view

                        Surface flattened = workspace.ScratchSurface;
                        workspace.Document.Flatten(flattened);
                    
                        Surface thumb;

                        if (workspace.Document.Width > maxDim || workspace.Document.Height > maxDim)
                        {
                            int width;
                            int height;

                            if (workspace.Document.Width > workspace.Document.Height)
                            {
                                width = maxDim;
                                height = (workspace.Document.Height * maxDim) / workspace.Document.Width;
                            }
                            else
                            {
                                height = maxDim;
                                width = (workspace.Document.Width * maxDim) / workspace.Document.Height;
                            }

                            int thumbWidth = Math.Max(1, width);
                            int thumbHeight = Math.Max(1, height);

                            thumb = new Surface(thumbWidth, thumbHeight);
                            thumb.SuperSamplingFitSurface(flattened);
                        }
                        else
                        {
                            thumb = new Surface(flattened.Size);
                            thumb.CopySurface(flattened);
                        }

                        Document thumbDoc = new Document(thumb.Width, thumb.Height);
                        BitmapLayer thumbLayer = new BitmapLayer(thumb);
                        thumb.Dispose();
                        thumbDoc.Layers.Add(thumbLayer);
                        MemoryStream thumbGif = new MemoryStream();
                        GifSaveConfigToken token = new GifSaveConfigToken(128, true, 4);
                        PdnFileTypes.Gif.Save(thumbDoc, thumbGif, token, null, false);
                        byte[] thumbBytes = thumbGif.ToArray();
                        string thumbString = Convert.ToBase64String(thumbBytes);
                        thumbDoc.Dispose();

                        string thumbXml = "<thumb gif=\"" + thumbString + "\" />";
                        workspace.Document.CustomHeaders = thumbXml;
                    }
                }

                // save!
                bool success = false;
                Stream stream = null;

                try 
                {
                    stream = (Stream)new FileStream(fileName, FileMode.Create, FileAccess.Write);

                    using (new WaitCursorChanger(this))
                    {
                        Utility.GCFullCollect();
                        SaveProgressDialog sd = new SaveProgressDialog(this);
                        sd.Save(stream, workspace.Document, fileType, saveConfigToken);
                        success = true;
                    }
                }

                catch (UnauthorizedAccessException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("MainForm.SaveDocument.Error.UnauthorizedAccessException"));
                }
    
                catch (SecurityException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("MainForm.SaveDocument.Error.SecurityException"));
                }

                catch (DirectoryNotFoundException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("MainForm.SaveDocument.Error.DirectoryNotFoundException"));
                }

                catch (IOException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("MainForm.SaveDocument.Error.IOException"));
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("MainForm.SaveDocument.Error.OutOfMemoryException"));
                }

#if !DEBUG
                catch
                {
                    Utility.ErrorBox(this, PdnResources.GetString("MainForm.SaveDocument.Error.Exception"));
                }
#endif

                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }
                }

                if (success)
                {
                    Shell.AddToRecentDocumentsList(fileName);
                }
                else
                {
                    return false;
                }

                // reset the dirty bit so they won't be asked to save on quitting
                workspace.Document.Dirty = false;

                // some misc. book keeping ...
                AddMru(fileName);
                SetTitleText();

                // and finally, shout happiness by way of ...
                return true;
            }

            finally
            {
                workspace.Environment.SetTool(oldToolType, this.workspace);
            }
        }

        private void menuFileSave_Click(object sender, System.EventArgs e)
        {
            DoSave();              
        }

        private bool IsClipboardImageAvailable()
        {
            try
            {
                // find out if there's anything on the clipboard that we can use
                Utility.GCFullCollect();
                IDataObject pasted = Clipboard.GetDataObject();

                if (pasted.GetDataPresent(DataFormats.Bitmap))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (ExternalException)
            {
                return false;
            }

            catch (OutOfMemoryException)
            {
                Utility.GCFullCollect();
                return false;
            }
        }

        private void menuEdit_DropDownOpening(object sender, System.EventArgs e)
        {
            bool selection = !workspace.Environment.Selection.IsEmpty;

            menuEditCopy.Enabled = selection;
            menuEditCut.Enabled = selection && (workspace.ActiveLayer is BitmapLayer);
            menuEditEraseSelection.Enabled = selection;
            menuEditInvertSelection.Enabled = selection;
            menuEditDeselect.Enabled = selection;
            
            // find out if there's anything on the clipboard that we can use
            menuEditPaste.Enabled = IsClipboardImageAvailable();

            if (!menuEditPaste.Enabled)
            {
                if (workspace.Environment.Tool != null)
                {
                    bool canHandle;

                    try
                    {
                        IDataObject pasted = Clipboard.GetDataObject();
                        workspace.Environment.Tool.PerformPasteQuery(pasted, out canHandle);
                    }

                    catch (ExternalException)
                    {
                        canHandle = false;
                    }

                    if (canHandle)
                    {
                        menuEditPaste.Enabled = true;
                    }
                }
            }

            menuEditPasteInToNewLayer.Enabled = IsClipboardImageAvailable();

            //
            menuEditUndo.Enabled = (workspace.History.UndoStack.Count > 1); // top of stack is always assumed to be a "NullHistoryAction," which is not undoable! thus we don't count it
            menuEditRedo.Enabled = (workspace.History.RedoStack.Count > 0);
        }

        // For the menus where we dynamically enable menu items (e.g. Copy only enabled when there's a selection),
        // we have to make sure to re-enable all the items when the menu goes way.
        // This is important for cases where, for example: Edit menu is opened, "Deselect" is disabled because
        // there is no selection. User then clicks on Select All. The menu then goes away. However, since Deselect
        // was disabled, the Ctrl+D shortcut will not be honored even though there is a selection.
        // So the disabling of menu items should only be temporary for the duration of the menu's visibility.
        private void OnMenuDropDownClosed(object sender, System.EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            foreach (ToolStripItem tsi in menu.DropDownItems)
            {
                tsi.Enabled = true;
            }
        }

        private bool CopySelectionToClipboard()
        {
            bool success = true;

            if (workspace.Environment.Selection.IsEmpty)
            {
                return false;
            }

            try
            {
                using (new WaitCursorChanger(this))
                {
                    Utility.GCFullCollect();
                    PdnRegion selectionRegion = workspace.Environment.Selection.CreateRegion();
                    PdnGraphicsPath selectionOutline = workspace.Environment.Selection.CreatePath();
                    BitmapLayer activeLayer = (BitmapLayer)workspace.ActiveLayer;
                    RenderArgs renderArgs = new RenderArgs(activeLayer.Surface);
                    MaskedSurface maskedSurface = new MaskedSurface(renderArgs.Surface, selectionOutline);
                    SurfaceForClipboard surfaceForClipboard = new SurfaceForClipboard(maskedSurface);
                    Rectangle selectionBounds = Utility.GetRegionBounds(selectionRegion);

                    if (selectionBounds.Width > 0 && selectionBounds.Height > 0)
                    {
                        Surface copySurface = new Surface(selectionBounds.Width, selectionBounds.Height);
                        Bitmap copyBitmap = copySurface.CreateAliasedBitmap();
                        Bitmap copyOpaqueBitmap = new Bitmap(copySurface.Width, copySurface.Height, PixelFormat.Format24bppRgb);

                        using (Graphics copyBitmapGraphics = Graphics.FromImage(copyBitmap))
                        {
                            copyBitmapGraphics.Clear(Color.White);
                        }

                        maskedSurface.Draw(copySurface, -selectionBounds.X, -selectionBounds.Y);

                        using (Graphics copyOpaqueBitmapGraphics = Graphics.FromImage(copyOpaqueBitmap))
                        {
                            copyOpaqueBitmapGraphics.Clear(Color.White);
                            copyOpaqueBitmapGraphics.DrawImage(copyBitmap, 0, 0);
                        }

                        DataObject dataObject = new DataObject();

                        dataObject.SetData(DataFormats.Bitmap, copyOpaqueBitmap);
                        dataObject.SetData(surfaceForClipboard);

                        int retryCount = 2;

                        while (retryCount >= 0)
                        {
                            try
                            {
                                using (new WaitCursorChanger(this))
                                {
                                    Clipboard.SetDataObject(dataObject, true);
                                }

                                break;
                            }

                            catch
                            {
                                if (retryCount == 0)
                                {
                                    success = false;
                                    Utility.ErrorBox(this, PdnResources.GetString("CopyAction.Error.TransferToClipboard"));
                                }
                                else
                                {
                                    Thread.Sleep(200);
                                }
                            }

                            finally
                            {
                                --retryCount;
                            }
                        }

                        copySurface.Dispose();
                        copyBitmap.Dispose();
                        copyOpaqueBitmap.Dispose();
                    }

                    selectionRegion.Dispose();
                    selectionOutline.Dispose();
                    renderArgs.Dispose();
                    maskedSurface.Dispose();
                }
            }

            catch (OutOfMemoryException)
            {
                success = false;
                Utility.ErrorBox(this, PdnResources.GetString("CopyAction.Error.OutOfMemory"));
            }

            Utility.GCFullCollect();
            return success;
        }

        private void menuEditCopy_Click(object sender, System.EventArgs e)
        {
            CopySelectionToClipboard();
        }

        private void menuEditCut_Click(object sender, System.EventArgs e)
        {
            if (!workspace.Environment.Selection.IsEmpty)
            {
                if (CopySelectionToClipboard()) 
                {
                    workspace.PerformAction(typeof(EraseSelectionAction), PdnResources.GetString("CutAction.Name"), 
                        this.EditCutIcon);
                }
            }
        }

        #region effect background/progress rendering
        private PdnRegion[] progressRegions;
        private int progressRegionsStartIndex;

        private void RenderedTileHandler(object sender, RenderedTileEventArgs e)
        {
            if (this.progressRegions[e.TileNumber] == null)
            {
                this.progressRegions[e.TileNumber] = e.RenderedRegion;
            }
        }

        private void invalidateTimer_Tick(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            lock (this.progressRegions)
            {
                int min = this.progressRegionsStartIndex;
                int max;

                for (max = min; max < progressRegions.Length; ++max)
                {
                    if (this.progressRegions[max] == null)
                    {
                        break;
                    }
                }

                if (min != max)
                {
                    using (PdnRegion updateRegion = PdnRegion.CreateEmpty())
                    {
                        for (int i = min; i < max; ++i)
                        {
                            updateRegion.Union(this.progressRegions[i]);
                        }

                        using (PdnRegion simplified = Utility.SimplifyAndInflateRegion(updateRegion))
                        {
                            workspace.ActiveLayer.Invalidate(simplified);
                        }

                        this.progressRegionsStartIndex = max;
                    }
                }

                double progress = 100.0 * (double)max / (double)progressRegions.Length;
                SetProgressStatusBar(progress);
            }
        }

        private void EffectConfigTokenChangedHandler(object sender, System.EventArgs e)
        {
            EffectConfigDialog ecf = (EffectConfigDialog)sender;
            BackgroundEffectRenderer ber = (BackgroundEffectRenderer)ecf.Tag;

            if (ber != null)
            {
                this.SyncResetProgressStatusBar();
                ber.Start();
            }
        }

        private void FinishedRenderingHandler(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(FinishedRenderingHandler), new object[] { sender, e });
            }
            else
            {
                workspace.EnableOutlineAnimation = true;
            }
        }

        private void StartingRenderingHandler(object sender, EventArgs e)
        {
            this.InvokeResetProgressStatusBar();
            workspace.EnableOutlineAnimation = false;

            if (this.progressRegions == null)
            {
                this.progressRegions = new PdnRegion[tilesPerCpu * renderingThreadCount];
            }

            lock (this.progressRegions)
            {
                for (int i = 0; i < progressRegions.Length; ++i)
                {
                    progressRegions[i] = null;
                }
            
                this.progressRegionsStartIndex = 0;
            }
        }

        private bool DoEffect(Effect effect, EffectConfigToken token, PdnRegion selectedRegion, 
            PdnRegion regionToRender, Surface originalSurface)
        {
            bool oldDirtyValue = workspace.Document.Dirty;
            bool resetDirtyValue = false;

            bool returnVal = false;
            workspace.EnableOutlineAnimation = false;

            try
            {
                using (ProgressDialog aed = new ProgressDialog())
                {
                    if (effect.Image != null)
                    {
                        aed.Icon = Utility.ImageToIcon(effect.Image, Utility.TransparentKey);
                    }

                    aed.Opacity = 0.9;
                    aed.Value = 0;
                    aed.Text = effect.Name;
                    aed.Description = string.Format(PdnResources.GetString("MainForm.Effects.ApplyingDialog.Description"), effect.Name);

                    invalidateTimer.Enabled = true;

                    using (new WaitCursorChanger(this))
                    {
                        HistoryAction ha = null;
                        DialogResult result = DialogResult.None;

                        this.InvokeResetProgressStatusBar();
                        workspace.Widgets.LayerControl.SuspendLayerPreviewUpdates();

                        try
                        {
                            using (new WaitCursorChanger(this))
                            {
                                ha = new BitmapHistoryAction(effect.Name, effect.Image, this.workspace,
                                    this.workspace.ActiveLayerIndex, selectedRegion, originalSurface);
                            }

                            BackgroundEffectRenderer ber = new BackgroundEffectRenderer(
                                effect,
                                token,
                                new RenderArgs(((BitmapLayer)workspace.ActiveLayer).Surface),
                                new RenderArgs(originalSurface),
                                regionToRender,
                                tilesPerCpu * renderingThreadCount,
                                renderingThreadCount);

                            aed.Tag = ber;
                            ber.RenderedTile += new RenderedTileEventHandler(aed.RenderedTileHandler);
                            ber.RenderedTile += new RenderedTileEventHandler(RenderedTileHandler);
                            ber.StartingRendering += new EventHandler(StartingRenderingHandler);
                            ber.FinishedRendering += new EventHandler(aed.FinishedRenderingHandler);
                            ber.FinishedRendering += new EventHandler(FinishedRenderingHandler);
                            ber.Start();

                            result = Utility.ShowDialog(aed, this);

                            if (result == DialogResult.Cancel)
                            {
                                resetDirtyValue = true;

                                using (new WaitCursorChanger(this))
                                {
                                    ber.Abort();
                                    ber.Join();
                                    ((BitmapLayer)workspace.ActiveLayer).Surface.CopySurface(originalSurface);
                                }
                            }

                            invalidateTimer.Enabled = false;

                            ber.Join();
                            ber.Dispose();
                        }

                        catch
                        {
                            using (new WaitCursorChanger(this))
                            {
                                ((BitmapLayer)workspace.ActiveLayer).Surface.CopySurface(originalSurface);
                            }
                        }

                        finally
                        {
                            workspace.Widgets.LayerControl.ResumeLayerPreviewUpdates();
                        }

                        using (PdnRegion simplifiedRenderRegion = Utility.SimplifyAndInflateRegion(selectedRegion))
                        {
                            using (new WaitCursorChanger(this))
                            {
                                workspace.ActiveLayer.Invalidate(simplifiedRenderRegion);
                            }
                        }

                        using (new WaitCursorChanger(this))
                        {
                            if (result == DialogResult.OK)
                            {
                                if (ha != null)
                                {
                                    workspace.History.PushNewAction(ha);
                                }

                                workspace.Update();
                                returnVal = true;
                            }
                            else
                            {
                                Utility.GCFullCollect();
                            }
                        }
                    } // using
                } // using
            }

            finally
            {
                workspace.EnableOutlineAnimation = true;

                if (resetDirtyValue)
                {
                    workspace.Document.Dirty = oldDirtyValue;
                }
            }

            return returnVal;
        }
        #endregion

        private void menuEffects_ClickHandler(object sender, System.EventArgs e)
        {
            bool oldDirtyValue = workspace.Document.Dirty;
            bool resetDirtyValue = false;

            effectsKeyboardHackEnabled = false;

            this.Update(); // make sure the window is done 'closing'
            this.InvokeResetProgressStatusBar();

            PdnRegion selectedRegion;

            if (workspace.Environment.Selection.IsEmpty)
            {
                selectedRegion = new PdnRegion(workspace.Document.Bounds);
            }
            else
            {
                selectedRegion = workspace.Environment.Selection.CreateRegion();
            }

            Type oldTool = workspace.Environment.GetToolType();
            workspace.Environment.SetTool(null);

            try
            {
                foreach (Type effectType in workspace.Effects)
                {   
                    ConstructorInfo ci = effectType.GetConstructor(Type.EmptyTypes);
                    Effect effect = (Effect)ci.Invoke(null);

                    EffectEnvironmentParameters eep = new EffectEnvironmentParameters(
                        workspace.Environment.ForeColor,
                        workspace.Environment.BackColor,
                        workspace.Environment.PenInfo.Width,
                        selectedRegion);

                    string name = effect.Name;

                    string repeatFormat = PdnResources.GetString("MainForm.Effects.RepeatMenuItem.Format");
                    string repeatName = string.Format(repeatFormat, effect.Name);

                    if (effect.IsConfigurable)
                    {
                        string configurableFormat = PdnResources.GetString("MainForm.Effects.Name.Format.Configurable");
                        name = string.Format(configurableFormat, name);
                    }
                
                    if (repeatName == ((ToolStripMenuItem)sender).Text)
                    {
                        Surface copy = workspace.ScratchSurface;

                        using (new WaitCursorChanger(this))
                        {
                            copy.CopySurface(((BitmapLayer)workspace.ActiveLayer).Surface);
                        }

                        ((Effect)lastEffect).EnvironmentParameters = eep;
                        DoEffect((Effect)lastEffect, (EffectConfigToken)lastEffectToken, selectedRegion, selectedRegion, copy);
                    }
                    else if (name == ((ToolStripMenuItem)sender).Text)
                    {
                        EffectConfigToken newLastToken = null;
                        effect.EnvironmentParameters = eep;

                        if (!(effect.IsConfigurable))
                        {
                            Surface copy = workspace.ScratchSurface;

                            using (new WaitCursorChanger(this))
                            {
                                copy.CopySurface(((BitmapLayer)workspace.ActiveLayer).Surface);
                            }

                            DoEffect(effect, null, selectedRegion, selectedRegion, copy);
                        }
                        else
                        {
                            PdnRegion previewRegion = (PdnRegion)selectedRegion.Clone();
                            previewRegion.Intersect(RectangleF.Inflate(workspace.VisibleDocumentRectangleF, 1, 1));

                            Surface originalSurface = workspace.ScratchSurface;

                            using (new WaitCursorChanger(this))
                            {
                                originalSurface.CopySurface(((BitmapLayer)workspace.ActiveLayer).Surface);
                            }
                            
                            //
                            workspace.Widgets.LayerControl.SuspendLayerPreviewUpdates();
                            //

                            using (EffectConfigDialog configDialog = effect.CreateConfigDialog())
                            {
                                configDialog.Opacity = 0.9;

                                configDialog.Effect = effect;
                                configDialog.EffectSourceSurface = originalSurface;
                                configDialog.Selection = selectedRegion;

                                EventHandler eh = new EventHandler(EffectConfigTokenChangedHandler);
                                configDialog.EffectTokenChanged += eh;

                                if (effectTokenHash[effectType] != null)
                                {
                                    EffectConfigToken oldToken = (EffectConfigToken)((EffectConfigToken)effectTokenHash[effectType]).Clone();
                                    configDialog.EffectToken = oldToken;
                                }

                                BackgroundEffectRenderer ber = new BackgroundEffectRenderer(
                                    effect,
                                    configDialog.EffectToken, 
                                    new RenderArgs(((BitmapLayer)workspace.ActiveLayer).Surface), 
                                    new RenderArgs(originalSurface), 
                                    previewRegion, 
                                    tilesPerCpu * renderingThreadCount,
                                    renderingThreadCount);

                                ber.RenderedTile += new RenderedTileEventHandler(RenderedTileHandler);
                                ber.StartingRendering += new EventHandler(StartingRenderingHandler);
                                ber.FinishedRendering += new EventHandler(FinishedRenderingHandler);
                                configDialog.Tag = ber;

                                invalidateTimer.Enabled = true;
                                DialogResult dr = Utility.ShowDialog(configDialog, this);
                                invalidateTimer.Enabled = false;
                                this.invalidateTimer_Tick(invalidateTimer, EventArgs.Empty);

                                if (dr == DialogResult.OK)
                                {
                                    effectTokenHash[effectType] = configDialog.EffectToken;
                                }

                                using (new WaitCursorChanger(this))
                                {
                                    ber.Abort();
                                    ber.Join();
                                    ber.Dispose();
                                    ber = null;

                                    if (dr != DialogResult.OK)
                                    {
                                        ((BitmapLayer)workspace.ActiveLayer).Surface.CopySurface(originalSurface);
                                        workspace.ActiveLayer.Invalidate();
                                    }

                                    configDialog.EffectTokenChanged -= eh;
                                    configDialog.Hide();
                                    this.Update();
                                    previewRegion.Dispose();
                                }

                                //
                                workspace.Widgets.LayerControl.ResumeLayerPreviewUpdates();
                                //

                                if (dr == DialogResult.OK)
                                {
                                    PdnRegion remainingToRender = selectedRegion.Clone();
                                    PdnRegion alreadyRendered = PdnRegion.CreateEmpty();

                                    for (int i = 0; i < this.progressRegions.Length; ++i)
                                    {
                                        if (this.progressRegions[i] == null)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            remainingToRender.Exclude(this.progressRegions[i]);
                                            alreadyRendered.Union(this.progressRegions[i]);
                                        }
                                    }

                                    workspace.ActiveLayer.Invalidate(alreadyRendered);
                                    newLastToken = (EffectConfigToken)configDialog.EffectToken.Clone();
                                    this.ResetProgressStatusBar();
                                    DoEffect(effect, newLastToken, selectedRegion, remainingToRender, originalSurface);
                                }
                                else // if (dr == DialogResult.Cancel)
                                {
                                    using (new WaitCursorChanger(this))
                                    {
                                        workspace.ActiveLayer.Invalidate();
                                        Utility.GCFullCollect();
                                    }
                                    
                                    resetDirtyValue = true;
                                    return;
                                }
                            }
                        }

                        // if it was from the Effects menu, save it as the "Repeat ...." item
                        if (effect.Category == EffectCategory.Effect)
                        {
                            lastEffect = effect;
                            lastEffectToken = newLastToken;
                            ResetEffectsMenu();
                            PopulateEffectsMenu();
                        }
                    }
                }
            }

            finally
            {
                selectedRegion.Dispose();
                this.InvokeEraseProgressStatusBar();
                workspace.EnableOutlineAnimation = true;
                workspace.Environment.SetTool(oldTool, workspace);

                for (int i = 0; i < this.progressRegions.Length; ++i)
                {
                    if (this.progressRegions[i] != null)
                    {
                        this.progressRegions[i].Dispose();
                        this.progressRegions[i] = null;
                    }
                }

                if (resetDirtyValue)
                {
                    workspace.Document.Dirty = oldDirtyValue;
                }
            }
        }

        private bool reinitEffectsMenu = true;
        private void ResetEffectsMenu()
        {
            reinitEffectsMenu = true;
        }

        private void PopulateEffectsMenu()
        {
            // Clear out the menu items!
            foreach (ToolStripItem mi in menuEffects.DropDownItems)
            {
                if (mi is ToolStripMenuItem)
                {
                    mi.Click -= menuEffectsClickDelegate;
                }
            }

            menuEffects.DropDownItems.Clear();

            // If we have a repeatable effect, add it with "Repeat ___ (Ctrl+F)" along with a separator
            if (this.lastEffect != null)
            {
                string repeatFormat = PdnResources.GetString("MainForm.Effects.RepeatMenuItem.Format");
                string menuName = string.Format(repeatFormat, ((Effect)lastEffect).Name);
                ToolStripMenuItem mi = new ToolStripMenuItem(menuName, null, menuEffectsClickDelegate, Keys.Control | Keys.F);

                if (((Effect)lastEffect).Image != null)
                {
                    SetMenuIcon(mi, ((Effect)lastEffect).Image);
                }

                mi.ShortcutKeys = Keys.Control | Keys.F;

                menuEffects.DropDownItems.Add(mi);

                // add separator
                ToolStripSeparator separator = new ToolStripSeparator();
                menuEffects.DropDownItems.Add(separator);
                repeatEffectKeyboardHackEnabled = true;
                effectsKeyboardHackEnabled = true;
            }

            // Fill the menu with the effect names, and "..." if it is configurable
            AddEffectsToMenu(menuEffects, new BoolObjectDelegate(EffectsMenuPredicate), false);

            effectsPopulated = true;
        }

        private void PopulateAdjustmentsMenu()
        {
            menuLayersAdjustments.DropDownItems.Clear();
            AddEffectsToMenu(menuLayersAdjustments, new BoolObjectDelegate(AdjustmentsMenuPredicate), true);
            adjustmentsPopulated = true;
        }

        private void PopulateEffectsAndAdjustmentsMenus()
        {
            PopulateEffectsMenu();
            PopulateAdjustmentsMenu();
        }

        private void menuEffects_DropDownOpening(object sender, System.EventArgs e)
        {
            if (!reinitEffectsMenu)
            {
                return;
            }

            PopulateEffectsMenu();        
            reinitEffectsMenu = false;
        }

        private bool AdjustmentsMenuPredicate(object effect)
        {
            return ((Effect)effect).Category == EffectCategory.Adjustment;
        }

        private bool EffectsMenuPredicate(object effect)
        {
            return ((Effect)effect).Category == EffectCategory.Effect;
        }

        private void menuLayersAdjustments_DropDownOpening(object sender, System.EventArgs e)
        {
            PopulateAdjustmentsMenu();
            menuLayersAdjustments.DropDownOpening -= new EventHandler(menuLayersAdjustments_DropDownOpening);
        }

        private void AddEffectsToMenu(ToolStripMenuItem topMenu, BoolObjectDelegate predicate, bool withShortcuts)
        {
            // Fill the menu with the effect names, and "..." if it is configurable
            foreach (Type type in workspace.Effects)
            {
                Effect effect;

                try
                {
                    ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
                    effect = (Effect)ci.Invoke(null);
                }

                catch
                {
                    // We don't want a DLL that can't be figured out to cause the app to crash
                    // But should we show an error message?
                    continue;
                }

                if (!predicate(effect))
                {
                    continue;
                }

                string name = effect.Name;

                if (effect.IsConfigurable)
                {
                    string configurableFormat = PdnResources.GetString("MainForm.Effects.Name.Format.Configurable");
                    name = string.Format(configurableFormat, name);
                }

                ToolStripMenuItem mi = new ToolStripMenuItem(name, null, menuEffectsClickDelegate, 
                    withShortcuts ? effect.ShortcutKeys : Keys.None);

                if (effect.Image != null)
                {
                    this.SetMenuIcon(mi, effect.Image);
                }

                ToolStripMenuItem addEffectHere = topMenu;

                if (effect.SubMenuName != null)
                {
                    ToolStripMenuItem subMenu = null;

                    // search for this subMenu
                    foreach (ToolStripItem sub in menuEffects.DropDownItems)
                    {
                        ToolStripMenuItem submi = sub as ToolStripMenuItem;

                        if (submi != null)
                        {
                            if (submi.Text == effect.SubMenuName)
                            {
                                subMenu = submi;
                                break;
                            }
                        }
                    }

                    if (subMenu == null)
                    {
                        subMenu = new ToolStripMenuItem(effect.SubMenuName);
                        topMenu.DropDownItems.Add(subMenu);
                    }

                    addEffectHere = subMenu;
                }

                addEffectHere.DropDownItems.Add(mi);
            }         
        }

        private void menuFileAcquire_DropDownOpening(object sender, System.EventArgs e)
        {
            menuFileAcquireFromClipboard.Enabled = this.IsClipboardImageAvailable();        

            // We only disable the scanner menu item if we know for sure a scanner is not available
            // If WIA isn't available we leave the menu item enabled. That way we can give an
            // informative error message when the user clicks on it and say "scanning requires XP SP1"
            // Otherwise the user is confused and will make scathing posts on our forum.
            bool scannerEnabled = true;

            if (ScanningAndPrinting.IsComponentAvailable)
            {
                if (!ScanningAndPrinting.CanScan)
                {
                    scannerEnabled = false;
                }
            }

            menuFileAcquireFromScannerOrCamera.Enabled = scannerEnabled;
        }

        private void menuEditInvertSelection_Click(object sender, System.EventArgs e)
        {
            if (!workspace.Environment.Selection.IsEmpty)
            {
                workspace.PerformAction(typeof(InvertSelectionAction));

                // Make sure that the selection info shows up in the status bar, and not the tool's help text
                workspace.Environment.Selection.PerformChanging();
                workspace.Environment.Selection.PerformChanged();
            }
        }

        private void menuEditClearSelection_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(EraseSelectionAction));
        }

        private void menuEditSelectAll_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(SelectAllAction));

            // Make sure that the selection info shows up in the status bar, and not the tool's help text
            workspace.Environment.Selection.PerformChanging();
            workspace.Environment.Selection.PerformChanged();
        }

        private void menuEditDeselect_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(DeselectAction));
        }

        private void menuEditPaste_Click(object sender, System.EventArgs e)
        {
            DoPaste();
        }

        private bool DoPaste()
        {
            SurfaceForClipboard surfaceForClipboard = null;
            IDataObject clipData = null;

            try
            {
                Utility.GCFullCollect();
                clipData = Clipboard.GetDataObject();
            }

            catch (ExternalException)
            {
                Utility.ErrorBox(this, PdnResources.GetString("PasteAction.Error.TransferFromClipboard"));
                return false;
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(this, PdnResources.GetString("PasteAction.Error.OutOfMemory"));
                return false;
            }

            // First "ask" the current tool if it wants to handle it
            bool handledByTool = false;
            if (workspace.Environment.Tool != null)
            {
                workspace.Environment.Tool.PerformPaste(clipData, out handledByTool);
            }

            if (handledByTool)
            {
                return true;
            }

            if (clipData.GetDataPresent(typeof(SurfaceForClipboard)))
            {
                try
                {
                    Utility.GCFullCollect();
                    surfaceForClipboard = (SurfaceForClipboard)clipData.GetData(typeof(SurfaceForClipboard));
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("PasteAction.Error.OutOfMemory"));
                    return false;
                }
            }

            if (surfaceForClipboard == null && clipData.GetDataPresent(DataFormats.Bitmap))
            {
                Image image;
                
                try
                {
                    Utility.GCFullCollect();
                    image = (Image)clipData.GetData(DataFormats.Bitmap);
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("PasteAction.Error.OutOfMemory"));
                    return false;
                }

                // Sometimes we get weird errors if we're in, say, 16-bit mode but the image was copied
                // to the clipboard in 32-bit mode
                if (image == null)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("PasteAction.Error.NotRecognized"));
                    return false;
                }

                Surface surface = null;
                MaskedSurface maskedSurface = null;

                try
                {
                    Utility.GCFullCollect();
                    Bitmap bitmap = new Bitmap(image);
                    image.Dispose();
                    surface = Surface.CopyFromBitmap(bitmap);
                    bitmap.Dispose();
                    maskedSurface = new MaskedSurface(surface, new PdnRegion(surface.Bounds));
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("PasteAction.Error.OutOfMemory"));
                    return false;
                }

                surfaceForClipboard = new SurfaceForClipboard(maskedSurface);
            }

            if (surfaceForClipboard == null || surfaceForClipboard.MaskedSurface == null)
            {   
                // silently fail: like what if a program overwrote the clipboard in between the time
                // we enabled the "Paste" menu item and the user actually clicked paste?
                // it could happen!
                Utility.ErrorBox(this, PdnResources.GetString("PasteAction.Error.NoImage"));
                return false;
            }

            // If the image is larger than the document, ask them if they'd like to make the image larger first
            Rectangle bounds = surfaceForClipboard.Bounds;

            if (bounds.Width > workspace.Document.Width ||
                bounds.Height > workspace.Document.Height)
            {
                DialogResult dr = MessageBox.Show(this, PdnResources.GetString("PasteAction.Question.ExpandCanvas"),
                    PdnInfo.GetAppName(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                int layerIndex = workspace.Document.Layers.IndexOf(workspace.ActiveLayer);

                switch (dr)
                {
                    case DialogResult.Yes:
                        Size newSize = new Size(Math.Max(bounds.Width, workspace.Document.Width),
                                                Math.Max(bounds.Height, workspace.Document.Height));

                        Document newDoc = CanvasSizeAction.ResizeDocument(workspace.Document, newSize,
                            AnchorEdge.TopLeft, workspace.Environment.BackColor);

                        if (newDoc == null)
                        {
                            return false; // user clicked cancel!
                        }
                        else
                        {
                            HistoryAction rdha = new ReplaceDocumentHistoryAction(CanvasSizeAction.StaticName, CanvasSizeAction.StaticImage, workspace);
                            workspace.SetDocument(newDoc);
                            workspace.History.PushNewAction(rdha);
                            workspace.ActiveLayer = (Layer)workspace.Document.Layers[layerIndex];
                        }

                        break;

                    case DialogResult.No:
                        break;

                    case DialogResult.Cancel:
                        return false;

                    default:
                        throw new InvalidEnumArgumentException("Internal error: DialogResult was neither Yes, No, nor Cancel");
                }
            }

            // Decide where to paste to: If the paste is within bounds of the document, do as normal
            // Otherwise, center it.
            Rectangle docBounds = workspace.Document.Bounds;
            Rectangle intersect1 = Rectangle.Intersect(docBounds, bounds);
            bool doMove = intersect1.IsEmpty;

            Point pasteOffset;
            
            if (doMove)
            {
                pasteOffset = new Point(-bounds.X + (docBounds.Width / 2) - (bounds.Width / 2),
                                        -bounds.Y + (docBounds.Height / 2) - (bounds.Height / 2));
            }
            else
            {
                pasteOffset = new Point(0, 0);
            }

            // Paste to the place it was originally copied from (for PDN-to-PDN transfers)
            // and then if its not pasted within the viewable rectangle we pan to that location
            RectangleF visibleDocRectF = workspace.DocumentView.VisibleDocumentRectangleF;
            Rectangle visibleDocRect = Utility.RoundRectangle(visibleDocRectF);
            Rectangle bounds2 = new Rectangle(new Point(bounds.X + pasteOffset.X, bounds.Y + pasteOffset.Y), bounds.Size);
            Rectangle intersect2 = Rectangle.Intersect(bounds2, visibleDocRect);
            bool doPan = intersect2.IsEmpty;

            workspace.Environment.SetTool(null);
            workspace.Environment.SetTool(typeof(MoveTool), workspace);

            ((MoveTool)workspace.Environment.Tool).PasteMouseDown(surfaceForClipboard, pasteOffset);

            if (doPan)
            {
                Point centerPtView = new Point(visibleDocRect.Left + (visibleDocRect.Width / 2),
                                               visibleDocRect.Top + (visibleDocRect.Height / 2));

                Point centerPtPasted = new Point(bounds2.Left + (bounds2.Width / 2),
                                                 bounds2.Top + (bounds2.Height / 2));

                Size delta = new Size(centerPtPasted.X - centerPtView.X,
                                      centerPtPasted.Y - centerPtView.Y);

                PointF docScrollPos = workspace.DocumentView.DocumentScrollPositionF;

                PointF newDocScrollPos = new PointF(docScrollPos.X + delta.Width,
                                                    docScrollPos.Y + delta.Height);

                workspace.DocumentView.DocumentScrollPositionF = newDocScrollPos;
            }

            return true;
        }

        private void menuLayersAddNewLayer_Click(object sender, System.EventArgs e)
        {
            try
            {
                Utility.GCFullCollect();
                NewLayerHistoryAction nlha = workspace.AddNewLayerToDocument();
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(this, PdnResources.GetString("NewLayerAction.Error.OutOfMemory"));
            }
        }

        private void menuLayersDuplicateLayer_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.PerformDuplicateLayerClick();
        }

        private void menuImageFlatten_Click(object sender, System.EventArgs e)
        {
            if (workspace.Document.Layers.Count <= 1)
            {
                return;
            }

            bool foundHidden = false;

            foreach (Layer layer in workspace.Document.Layers)
            {
                if (!layer.Visible)
                {
                    foundHidden = true;
                    break;
                }
            }

            if (foundHidden)
            {
                DialogResult result = MessageBox.Show(this, PdnResources.GetString("FlattenAction.Question.DiscardHiddenLayers"), 
                    PdnInfo.GetProductName(), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

                this.Focus();

                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            if (!workspace.Environment.Selection.IsEmpty)
            {
                workspace.PerformAction(typeof(DeselectAction));
            }

            workspace.PerformAction(typeof(FlattenAction));
        }

        private void menuLayers_DropDownOpening(object sender, System.EventArgs e)
        {
        }

        private void menuLayersDeleteLayer_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.PerformDeleteLayerClick();
        }

        private void menuWindow_DropDownOpening(object sender, System.EventArgs e)
        {
            menuWindowTranslucent.Checked = PdnBaseForm.EnableOpacity;
            menuWindowTools.Checked = workspace.Widgets.MainToolBarForm.Visible;
            menuWindowHistory.Checked = workspace.Widgets.HistoryForm.Visible;
            menuWindowLayers.Checked = workspace.Widgets.LayerForm.Visible;
            menuWindowColors.Checked = workspace.Widgets.ColorsForm.Visible;
        }

        private void menuWindowTools_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.MainToolBarForm.Visible = !workspace.Widgets.MainToolBarForm.Visible;
            if (!workspace.Widgets.MainToolBarForm.Visible) 
            {
                //if we don't do this, hiding the last floating window can cause PDN to lose focus
                workspace.DocumentView.Focus();
            }
        }

        private void menuWindowHistory_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.HistoryForm.Visible = !workspace.Widgets.HistoryForm.Visible;
            if (!workspace.Widgets.HistoryForm.Visible) 
            {
                //if we don't do this, hiding the last floating window can cause PDN to lose focus
                workspace.DocumentView.Focus();
            }
        }

        private void menuWindowLayers_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.Visible = !workspace.Widgets.LayerForm.Visible; 
            if (!workspace.Widgets.LayerForm.Visible) 
            {
                //if we don't do this, hiding the last floating window can cause PDN to lose focus
                workspace.DocumentView.Focus();
            }
        }

        private void menuWindowColors_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.ColorsForm.Visible = !workspace.Widgets.ColorsForm.Visible;
            if (!workspace.Widgets.ColorsForm.Visible) 
            {
                //if we don't do this, hiding the last floating window can cause PDN to lose focus
                workspace.DocumentView.Focus();
            }
        }

        private void menuImage_DropDownOpening(object sender, System.EventArgs e)
        {
            menuImageCrop.Enabled = !workspace.Environment.Selection.IsEmpty;
            menuImageFlatten.Enabled = (workspace.Document.Layers.Count > 1);
        }

        private void menuImageCrop_Click(object sender, System.EventArgs e)
        {
            if (!workspace.Environment.Selection.IsEmpty)
            {
                workspace.PerformAction(typeof(CropAction));
            }
        }

        private void menuImageResize_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(ResizeAction));
        }

        private void menuTools_DropDownOpening(object sender, System.EventArgs e)
        {
            menuTools.DropDownItems.Clear();
            menuTools.DropDownItems.Add(menuToolsAntialiasing);
            menuTools.DropDownItems.Add(menuToolsAlphaBlending);
            menuTools.DropDownItems.Add(menuToolsSeperator);

            foreach (ToolInfo toolInfo in workspace.ToolInfos)
            {
                ToolStripMenuItem mi = new ToolStripMenuItem(toolInfo.Name, null, menuToolsClickDelegate);
                SetMenuIcon(mi, (Image)toolInfo.Image.Clone());
                menuTools.DropDownItems.Add(mi);
            }
            
            menuTools.DropDownOpening -= new EventHandler(menuTools_DropDownOpening);
            menuTools.DropDownOpening += new EventHandler(menuTools_DropDownOpening2);
            menuTools_DropDownOpening2(sender, e);
        }

        private void menuTools_DropDownOpening2(object sender, System.EventArgs e)
        {
            Tool currentTool = workspace.Environment.Tool;

            foreach (ToolStripItem tsi in menuTools.DropDownItems)
            {
                ToolStripMenuItem mi = tsi as ToolStripMenuItem;

                if (mi != null)
                {
                    string name = null;

                    if (currentTool != null)
                    {
                        name = workspace.Environment.Tool.Name;
                    }

                    if (name != null && mi.Text == workspace.Environment.Tool.Name)
                    {
                        mi.Checked = true;
                    }
                    else
                    {
                        mi.Checked = false;
                    }
                }
            }

            menuToolsAntialiasing.Checked = workspace.Environment.AntiAliasing;
            menuToolsAlphaBlending.Checked = workspace.Environment.AlphaBlending;
        }

        private void menuTools_ClickHandler(object sender, System.EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            foreach (ToolInfo toolInfo in workspace.ToolInfos)
            {
                if (toolInfo.Name == mi.Text)
                {
                    workspace.Widgets.MainToolBar.SelectTool(toolInfo.ToolType);
                }
            }
        }

        private void HideInsteadOfCloseHandler(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            ((Form)sender).Hide();
        }

        private void menuItem1_Click(object sender, System.EventArgs e)
        {
            workspace.Document.Invalidate();
            Update();
        }

        private void menuItem4_Click(object sender, System.EventArgs e)
        {
            Utility.GCFullCollect();        
        }

        private void menuLayersFlipHorizontal_Click(object sender, System.EventArgs e)
        {
            DeselectIfSelected();
            workspace.PerformAction(typeof(FlipLayerHorizontalAction));
        }

        private void menuLayersFlipVertical_Click(object sender, System.EventArgs e)
        {
            DeselectIfSelected();
            workspace.PerformAction(typeof(FlipLayerVerticalAction));
        }

        private void menuImageFlipHorizontal_Click(object sender, System.EventArgs e)
        {
            DeselectIfSelected();
            workspace.PerformAction(typeof(FlipDocumentHorizontalAction));
        }

        private void menuImageFlipVertical_Click(object sender, System.EventArgs e)
        {
            DeselectIfSelected();
            workspace.PerformAction(typeof(FlipDocumentVerticalAction));
        }

        private void DeselectIfSelected()
        {
            if (!workspace.Environment.Selection.IsEmpty)
            {
                workspace.PerformAction(typeof(DeselectAction));
            }
        }

        private void menuItem5_Click(object sender, System.EventArgs e)
        {
            int x = 0;
            ++x;
        }

        private static string GetUnitsAbbreviation(MeasurementUnit units)
        {
            string result;

            switch (units)
            {
                case MeasurementUnit.Pixel:
                    result = string.Empty;
                    break;

                case MeasurementUnit.Centimeter:
                    result = PdnResources.GetString("MeasurementUnit.Centimeter.Abbreviation");
                    break;

                case MeasurementUnit.Inch:
                    result = PdnResources.GetString("MeasurementUnit.Inch.Abbreviation");
                    break;

                default:
                    throw new InvalidEnumArgumentException("MeasurementUnit was invalid");
            }
            
            return result;
        }

        private void CoordinatesToStrings(int x, int y, out string xString, out string yString, out string units)
        {
            string unitsAbbreviation = GetUnitsAbbreviation(workspace.Environment.Units);

            units = GetUnitsAbbreviation(workspace.Environment.Units);

            if (workspace.Environment.Units == MeasurementUnit.Pixel)
            {
                xString = x.ToString();
                yString = y.ToString();
            }
            else
            {
                double physicalX = workspace.Document.PixelToPhysicalX(x, workspace.Environment.Units);
                xString = physicalX.ToString("F2");

                double physicalY = workspace.Document.PixelToPhysicalY(y, workspace.Environment.Units);
                yString = physicalY.ToString("F2");
            }
        }

        private string cursorInfoStatusBarFormat = PdnResources.GetString("MainForm.StatusBar.CursorInfo.Format");
        private void DocumentView_DocumentMouseMove(object sender, MouseEventArgs e)
        {
            string xString;
            string yString;
            string units;

            CoordinatesToStrings(e.X, e.Y, out xString, out yString, out units);

            string cursorText = string.Format(
                CultureInfo.InvariantCulture, 
                this.cursorInfoStatusBarFormat, 
                xString, 
                units, 
                yString, 
                units);

            this.cursorInfoStatusLabel.Text = cursorText;
            this.statusStrip.Update();
        }

        private void InvokeResetProgressStatusBar()
        {
            this.BeginInvoke(new VoidVoidDelegate(ResetProgressStatusBar));
        }

        private void InvokeEraseProgressStatusBar()
        {
            this.BeginInvoke(new VoidVoidDelegate(EraseProgressStatusBar));
        }

        private void SyncResetProgressStatusBar()
        {
            IAsyncResult result = this.BeginInvoke(new VoidVoidDelegate(ResetProgressStatusBar));
            object ignore = this.EndInvoke(result);
        }

        private void EraseProgressStatusBar()
        {
            this.progressStatusSeparator.Visible = false;
            this.progressStatusBar.Visible = false;
            this.progressStatusBar.Value = 0;
        }

        private string progressTextFormat = PdnResources.GetString("MainForm.StatusBar.Progress.Percentage.Format");
        private void ResetProgressStatusBar()
        {
            this.progressStatusBar.Value = 0;
            this.progressStatusSeparator.Visible = true;
            this.progressStatusBar.Visible = true;
        }

        private double GetProgressStatusBarValue()
        {
            lock (progressStatusBar)
            {
                return progressStatusBar.Value;
            }
        }

        private void SetProgressStatusBarCallback(object context)
        {
            SetProgressStatusBar((double)context);
        }

        private void SetProgressStatusBar(double percent)
        {
            lock (progressStatusBar)
            {
                progressStatusBar.Value = (int)percent;
                bool visible = (percent != 100);
                progressStatusBar.Visible = visible;
                progressStatusSeparator.Visible = visible;
            }
        }

        private void menuImageCanvasSize_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(CanvasSizeAction));
        }

        private void menuItem6_Click(object sender, System.EventArgs e)
        {
            this.PositionFloatingForms();
        }

        private void menuWindowResetWindowLocations_Click(object sender, System.EventArgs e)
        {
            this.PositionFloatingForms();
        }

        private void menuLayersLayerProperties_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.PerformPropertiesClick();
        }

        private void menuViewZoomIn_Click(object sender, System.EventArgs e)
        {
            workspace.ZoomIn();
        }

        private void menuViewZoomOut_Click(object sender, System.EventArgs e)
        {
            workspace.ZoomOut();
        }

        private void menuViewZoomToWindow_Click(object sender, EventArgs e)
        {
            if (workspace.ZoomBasis == ZoomBasis.Window) 
            {
                workspace.ZoomBasis = ZoomBasis.Factor;
            } 
            else 
            {
                workspace.ZoomBasis = ZoomBasis.Window;
            }
        }

        private void menuViewZoomToSelection_Click(object sender, EventArgs e)
        {
            workspace.ZoomBasis = ZoomBasis.Selection;
            workspace.ZoomBasis = ZoomBasis.Selection;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!effectsPopulated)
            {
                PopulateEffectsMenu();
            }

            if (!adjustmentsPopulated)
            {
                PopulateAdjustmentsMenu();
            }

            InitMenuItemNames();

            base.OnKeyDown(e);
        }

        /// <summary>
        /// There seems to be a problem with the way WinForms handles additions to the menu system that 
        /// have shortcuts. It seems that when you add a new menu item w/ a shortcut, it
        /// is not recognize the first time the user tries to press any accelerator key. The next time, it
        /// does work.
        /// So what we do is enable our "accelerator hack" until the user has run an effect for the first
        /// time.
        /// </summary>
        private bool effectsKeyboardHackEnabled = true;

        /// <summary>
        /// Similar to effectsKeyboardHackEnabled, this handles the case where Ctrl+F is pressed after
        /// "Repeat ..." has been added to the effects menu for the first time.
        /// </summary>
        private bool repeatEffectKeyboardHackEnabled = true;

#if DEBUG
        static MainForm()
        {
            new Thread(FocusPrintThread).Start();
        }

        private static string GetControlName(Control control)
        {
            if (control == null)
            {
                return "null";
            }

            string name = control.Name + "(" + control.GetType().Name + ")";

            if (control.Parent != null)
            {
                name += " <- " + GetControlName(control.Parent);
            }

            return name;
        }

        private static void PrintFocus()
        {
            Control c = FindFocus();
            //Tracing.Ping("Focused: " + GetControlName(c));
        }

        private static void FocusPrintThread()
        {
            Thread.CurrentThread.IsBackground = true;

            while (true)
            {
                try
                {
                    Form form = Form.ActiveForm;

                    if (form != null)
                    {
                        form.BeginInvoke(new VoidVoidDelegate(PrintFocus));
                    }
                }

                catch
                {
                }

                Thread.Sleep(1000);
            }
        }
#endif

        // Useful for when you're debugging and want to figure out who in the world has focus
        public static Control FindFocus()
        {
            foreach (Form form in Application.OpenForms)
            {
                Control focused = FindFocus(form);

                if (focused != null)
                {
                    return focused;
                }
            }

            return null;
        }

        private static Control FindFocus(Control c)
        {
            if (c.Focused)
            {
                return c;
            }

            foreach (Control child in c.Controls)
            {
                Control f = FindFocus(child);

                if (f != null)
                {
                    return f;
                }
            }

            return null;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (!e.Handled)
            {
                // handle shortcuts which can't be expressed as a Shortcut enumeration
                if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
                {
                    if (e.Modifiers == Keys.Control)
                    {
                        ClickOnMenuItemAsync(this.menuViewZoomIn);
                        e.Handled = true;
                    }
                }
                else if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
                {
                    if (e.Modifiers == Keys.Control)
                    {
                        ClickOnMenuItemAsync(this.menuViewZoomOut);
                        e.Handled = true;
                    }
                } 
                else 
                {
                    if (repeatEffectKeyboardHackEnabled && e.Control && e.KeyCode == Keys.F && !e.Shift)
                    {
                        if (menuEffects.DropDownItems.Count > 0 &&
                            ((ToolStripMenuItem)menuEffects.DropDownItems[0]).ShortcutKeys == (Keys.Control | Keys.F))
                        {
                            menuEffects.DropDownItems[0].PerformClick();
                            repeatEffectKeyboardHackEnabled = false;
                        }
                    }
                    // HACK: For some reason, Ctrl+Shift+L only works once (for Auto-Levels). It doesn't seem to 
                    //       work unless our hack is specially cased enabled for it.
                    else if ((effectsKeyboardHackEnabled || e.KeyData == (Keys.Control | Keys.Shift | Keys.L)) && 
                              IsDynamicAcceleratorHACK(e))
                    {
                        DoDynamicAcceleratorHACK(e);
                        effectsKeyboardHackEnabled = false;
                    }
                }
            }
        }

        private void menuImageRotate90CW_Click(object sender, System.EventArgs e)
        {
            DocumentAction da = new RotateAction(this.workspace, RotateType.Clockwise90);
            DeselectIfSelected();
            workspace.PerformAction(da);
        }

        private void menuImageRotate180CW_Click(object sender, System.EventArgs e)
        {
            DocumentAction da = new RotateAction(this.workspace, RotateType.Clockwise180);
            DeselectIfSelected();
            workspace.PerformAction(da);
        }

        private void menuImageRotate270CW_Click(object sender, System.EventArgs e)
        {
            DocumentAction da = new RotateAction(this.workspace, RotateType.Clockwise270);
            DeselectIfSelected();
            workspace.PerformAction(da);
        }

        private void menuImageRotate90CCW_Click(object sender, System.EventArgs e)
        {
            DocumentAction da = new RotateAction(this.workspace, RotateType.CounterClockwise90);
            DeselectIfSelected();
            workspace.PerformAction(da);
        }

        private void menuImageRotate180CCW_Click(object sender, System.EventArgs e)
        {
            DocumentAction da = new RotateAction(this.workspace, RotateType.CounterClockwise180);
            DeselectIfSelected();
            workspace.PerformAction(da);
        }

        private void menuImageRotate270CCW_Click(object sender, System.EventArgs e)
        {
            DocumentAction da = new RotateAction(this.workspace, RotateType.CounterClockwise270);
            DeselectIfSelected();
            workspace.PerformAction(da);
        }

        private void FloatingForm_MouseWheel(object sender, MouseEventArgs e)
        {
            // route mouse wheel stuff to the documentView/panel
        }

        private Hashtable keysThatAreDown = new Hashtable();
        private void FloatingForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!keysThatAreDown.ContainsKey(e.KeyData)) 
            {
                keysThatAreDown.Add(e.KeyData, null);
            }

            if (!IsAcceleratorHACK(e))
            {
                this.OnKeyDown(e);
            }
        }

        private void FloatingForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (keysThatAreDown.ContainsKey(e.KeyData)) 
            {
                keysThatAreDown.Remove(e.KeyData);
            } 
            else
            {
                //If we weren't the form to get the KeyDown, we should ignore the KeyUp
                return;
            }
            if (e.Handled)
            {
                return;
            }

            // HACK: maybe the LayerForm should do this?
            if (e.KeyData == Keys.Delete && sender is LayerForm)
            {
                return;
            }

            if (IsAcceleratorHACK(e))
            {
                DoAcceleratorHACK(e);
            }
            else
            {
                this.OnKeyUp(e);
            }
        }

        /// <summary>
        /// Similar to DoAcceleratorHACK, but only tests whether a key is an accelerator or not.
        /// Does not actually perform the "menu item click."
        /// </summary>
        /// <param name="keyInfo"></param>
        /// <returns></returns>
        private bool IsAcceleratorHACK(KeyEventArgs keyInfo)
        {
            Type ourType = this.GetType();

            FieldInfo[] fields = ourType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType == typeof(ToolStripMenuItem))
                {
                    ToolStripMenuItem mi = (ToolStripMenuItem)fi.GetValue(this);
                    Keys shortcutKeys = mi.ShortcutKeys;
                    
                    if (keyInfo.KeyData == shortcutKeys)
                    {
                        return true;
                    }
                }
            }

            return IsDynamicAcceleratorHACK(keyInfo);
        }

        private bool IsDynamicAcceleratorHACK(KeyEventArgs keyInfo)
        {
            // Populate the Layers->Adjustments list if it isn't populated.
            if (menuLayersAdjustments.DropDownItems.Count <= 1)
            {
                this.menuLayersAdjustments_DropDownOpening(this, EventArgs.Empty);
            }

            foreach (ToolStripMenuItem mi in menuLayersAdjustments.DropDownItems)
            {
                if (keyInfo.KeyData == mi.ShortcutKeys)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// This function is sort of a hack that takes a KeyEventArgs and looks for a menu item
        /// that has the corresponding shortcut key. So if File/New has Shortcut == Shortcut.CtrlN
        /// and the keyInfo says that N was pressed with just Ctrl, then we execute FileNew.
        /// Uses reflection, so probably not too fast.
        /// 
        /// We need this because otherwise the floating forms/toolbars, when active, do not
        /// pass up keys which we would normally want to execute menu items, like Ctrl+N = File/New!
        /// </summary>
        /// <param name="keyInfo">A KeyEventArgs passed from a KeyUp event in a child form.</param>
        private void DoAcceleratorHACK(KeyEventArgs keyInfo)
        {
            Type ourType = this.GetType();
            FieldInfo[] fields = ourType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType == typeof(ToolStripMenuItem))
                {
                    ToolStripMenuItem mi = (ToolStripMenuItem)fi.GetValue(this);

                    // If they want to paste and the ColorsForm is active, don't handle it
                    if (mi == menuEditPaste && Form.ActiveForm == workspace.Widgets.ColorsForm)
                    {
                        continue;
                    }

                    Keys shortcutKeys = mi.ShortcutKeys; // TODO shortcut
                    
                    if (keyInfo.KeyData == shortcutKeys)
                    {
                        ClickOnMenuItem(mi);
                        keyInfo.Handled = true;
                        break;
                    }
                }
            }

            if (!keyInfo.Handled)
            {
                DoDynamicAcceleratorHACK(keyInfo);
            }
        }

        private void DoDynamicAcceleratorHACK(KeyEventArgs keyInfo) 
        {
            if (menuLayersAdjustments.DropDownItems.Count <= 1)
            {
                PopulateAdjustmentsMenu();
            }

            foreach (ToolStripMenuItem mi in menuLayersAdjustments.DropDownItems)
            {
                if (keyInfo.KeyData == mi.ShortcutKeys)
                {
                    ClickOnMenuItem(mi);
                    keyInfo.Handled = true;
                    break;
                }
            }
        }

        private void DocumentView_Layout(object sender, LayoutEventArgs e)
        {
            PerformLayout();
        }

        private void CommonActionsStrip_ButtonClick(object sender, EnumValueEventArgs e)
        {
            CommonAction ca = (CommonAction)e.EnumValue;

            switch (ca)
            {
                case CommonAction.CheckForUpdates:
                    CheckForUpdatesClickHandler();
                    break;

                case CommonAction.New:
                    ClickOnMenuItem(this.menuFileNew);
                    break;

                case CommonAction.Open:
                    ClickOnMenuItem(this.menuFileOpen);
                    break;

                case CommonAction.Save:
                    ClickOnMenuItem(this.menuFileSave);
                    break;

                case CommonAction.Print:
                    ClickOnMenuItem(this.menuFilePrint);
                    break;

                case CommonAction.Cut:
                    ClickOnMenuItem(this.menuEditCut);
                    break;

                case CommonAction.Copy:
                    ClickOnMenuItem(this.menuEditCopy);
                    break;

                case CommonAction.Paste:
                    ClickOnMenuItem(this.menuEditPaste);
                    break;

                case CommonAction.CropToSelection:
                    ClickOnMenuItem(this.menuImageCrop);
                    break;

                case CommonAction.Deselect:
                    ClickOnMenuItem(this.menuEditDeselect);
                    break;

                case CommonAction.Undo:
                    ClickOnMenuItem(this.menuEditUndo);
                    break;

                case CommonAction.Redo:
                    ClickOnMenuItem(this.menuEditRedo);
                    break;

                case CommonAction.ZoomIn:
                    workspace.ZoomIn();
                    break;

                case CommonAction.ZoomOut:
                    workspace.ZoomOut();
                    break;

                default:
                    throw new InvalidEnumArgumentException("e.EnumValue");
            }

            workspace.DocumentView.Focus();
        }

        private readonly string contextStatusBarFormat = PdnResources.GetString("MainForm.StatusBar.Context.SelectedArea.Text.Format");
        private readonly string contextStatusBarWithAngleFormat = PdnResources.GetString("MainForm.StatusBar.Context.SelectedArea.Text.WithAngle.Format");

        private void Environment_SelectedPathChanged(object sender, EventArgs e)
        {
            if (workspace.Environment.Selection.IsEmpty)
            {
                this.contextStatusLabel.Text = string.Empty;
                this.contextStatusLabel.Image = null;
            }
            else
            {
                int area = 0;
                Rectangle bounds;

                using (PdnRegion selection = workspace.Environment.Selection.CreateRegionRaw())
                {
                    selection.Intersect(workspace.Document.Bounds);
                    bounds = Utility.GetRegionBounds(selection);
                    area = selection.GetArea();
                }

                string unitsAbbreviation;
                string widthString;
                string heightString;
                this.CoordinatesToStrings(bounds.Width, bounds.Height, out widthString, out heightString, out unitsAbbreviation);

                NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                                                     
                string areaString;
                if (workspace.Environment.Units == MeasurementUnit.Pixel)
                {
                    nfi.NumberDecimalDigits = 0;
                    areaString = area.ToString("N", nfi);
                }
                else
                {
                    nfi.NumberDecimalDigits = 2;
                    double areaD = workspace.Document.PixelAreaToPhysicalArea(area, workspace.Environment.Units);
                    areaString = areaD.ToString("N", nfi);
                }
                
                string pluralUnits = PdnResources.GetString("MeasurementUnit." + workspace.Environment.Units.ToString() + ".Plural");

                MoveToolBase moveTool = workspace.Environment.Tool as MoveToolBase;
                if (moveTool != null && moveTool.HostShouldShowAngle)
                {
                    NumberFormatInfo nfi2 = (NumberFormatInfo)nfi.Clone();
                    nfi2.NumberDecimalDigits = 2;
                    float angle = moveTool.HostAngle;

                    while (angle > 180.0f)
                    {
                        angle -= 360.0f;
                    }

                    while (angle < -180.0f)
                    {
                        angle += 360.0f;
                    }

                    string contextText = string.Format(
                        contextStatusBarWithAngleFormat, 
                        widthString, 
                        unitsAbbreviation, 
                        heightString, 
                        unitsAbbreviation,
                        areaString,
                        pluralUnits.ToLower(),
                        moveTool.HostAngle.ToString("N", nfi2));

                    this.contextStatusLabel.Text = contextText;
                }
                else
                {
                    string contextText = string.Format(
                        contextStatusBarFormat, 
                        widthString, 
                        unitsAbbreviation, 
                        heightString, 
                        unitsAbbreviation,
                        areaString,
                        pluralUnits.ToLower());

                    this.contextStatusLabel.Text = contextText;
                }

                this.contextStatusLabel.Image = this.SelectionIcon;
            }

            this.statusStrip.Update();
        }

        private void floaterOpacityTimer_Tick(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            if (floaters == null)
            {
                return;
            }

            if (!PdnBaseForm.EnableOpacity)
            {
                return;
            }

            // Here's the behavior we want for our floaters:
            // 1. If the mouse is within a floaters rectangle, it should transition to fully opaque
            // 2. If the mouse is outside the floater's rectangle, it should transition to partially
            //    opaque
            // 3. However, if the floater is outside where the document is visible on screen, it
            //    should always be fully opaque.
            Rectangle screenDocRect;
                
            try
            {
                screenDocRect = workspace.DocumentView.VisibleDocumentBounds;
            }

            catch (ObjectDisposedException)
            {
                return; // do nothing, we are probably in the process of shutting down the app
            }

            for (int i = 0; i < floaters.Length; ++i)
            {
                FloatingToolForm ftf = floaters[i];

                Rectangle intersect = Rectangle.Intersect(screenDocRect, ftf.Bounds);
                double opacity = -1.0;

                try
                {
                    if (intersect.Width == 0 ||
                        intersect.Height == 0 ||
                        (Utility.IsPointInRectangle(Control.MousePosition, ftf.Bounds) &&
                        !workspace.DocumentView.IsMouseCaptured()) ||
                        Utility.DoesControlHaveMouseCaptured(ftf))
                    {
                        opacity = Math.Min(1.0, ftf.Opacity + 0.125);
                    }
                    else
                    {
                        opacity = Math.Max(0.75, ftf.Opacity - 0.0625);
                    }

                    if (opacity != ftf.Opacity)
                    {
                        ftf.Opacity = opacity;
                    }
                }

                catch (System.ComponentModel.Win32Exception)
                {
                    // We just eat the exception. Chris Strahl was having some problem where opacity was 0.7
                    // and we were trying to set it to 0.7 and it said "the parameter is incorrect"
                    // ... which is stupid. Bad NVIDIA drivers for his GeForce Go?
                    //throw new Exception(ftf.GetType().ToString() + " opacity is " + ftf.Opacity.ToString() + ", tried to set to " + opacity.ToString() + " and got this exception: " + ex.ToString());
                }
            }
        }

        private void menuViewActualSize_Click(object sender, System.EventArgs e)
        {
            workspace.DocumentView.ScaleFactor = ScaleFactor.OneToOne;
        }

        private void menuEditPasteInToNewLayer_Click(object sender, System.EventArgs e)
        {
            NewLayerHistoryAction nlha = null;

            try
            {
                nlha = workspace.AddNewLayerToDocument();
            }

            catch (OutOfMemoryException)
            {
                Utility.GCFullCollect();
                Utility.ErrorBox(this, PdnResources.GetString("NewLayerAction.Error.OutOfMemory"));
                return;
            }

            bool result = DoPaste();

            if (!result)
            {
                using (new WaitCursorChanger(this))
                {
                    workspace.History.StepBackward();
                }
            }
        }

        private void ShowWiaError()
        {
            // WIA requires Windows XP SP1 or later, or Windows Server 2003
            // So if we know they're on WS2k3, we tell them to enable WIA.
            // If they're on XP or later, tell them that WIA isn't available.
            // Otherwise we tell them they need XP SP1 (for the Win2K folks).
            if (OS.Type == OSType.Server)
            {
                Utility.ErrorBox(this, PdnResources.GetString("WIA.Error.EnableMe"));
            }
            else if (Environment.OSVersion.Version < OS.WindowsXP)
            {
                Utility.ErrorBox(this, PdnResources.GetString("WIA.Error.RequiresXPSP1"));
            }
            else
            {
                Utility.ErrorBox(this, PdnResources.GetString("WIA.Error.UnableToLoad"));
            }
        }

        private void menuFilePrint_Click(object sender, System.EventArgs e)
        {
            if (!ScanningAndPrinting.CanPrint)
            {
                ShowWiaError();
                return;
            }

            Type oldTool = workspace.Environment.GetToolType();
            workspace.Environment.SetTool(null);

            // render image to a bitmap, save it to disk
            Surface s = workspace.ScratchSurface;
            s.Clear();
            RenderArgs ra = new RenderArgs(s);

            this.Update();
            using (new WaitCursorChanger(this))
            {
                ra.Surface.Clear(ColorBgra.White);
                workspace.Document.Render(ra, false);
            }
            
            string tempName = Path.GetTempFileName();
            ra.Bitmap.Save(tempName, ImageFormat.Bmp);

            ScanningAndPrinting.Print(this, tempName);

            // Try to delete the temp file but don't worry if we can't
            try
            {
                File.Delete(tempName);
            }

            catch
            {
            }

            workspace.Environment.SetTool(oldTool, workspace);
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files) 
                {
                    try
                    {
                        FileAttributes fa = File.GetAttributes(file);

                        if ((fa & FileAttributes.Directory) == 0)
                        {
                            drgevent.Effect = DragDropEffects.Copy;
                        }
                    }

                    catch
                    {
                    }
                }
            }

            base.OnDragEnter (drgevent);
        }

        private string[] PruneDirectories(string[] fileNames)
        {
            List<string> result = new List<string>();

            foreach (string fileName in fileNames)
            {
                try
                {
                    FileAttributes fa = File.GetAttributes(fileName);

                    if ((fa & FileAttributes.Directory) == 0)
                    {
                        result.Add(fileName);
                    }
                }

                catch
                {
                }
            }

            return result.ToArray();
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] allFiles = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                if (allFiles == null)
                {
                    return;
                }

                string[] files = PruneDirectories(allFiles);

                bool importAsLayers = true;

                if (files.Length == 0)
                {
                    return;
                }
                else if (files.Length == 1)
                {
                    int result = 0; //importAsLayers ? 1 : 0;
                    string[] choices = new string[] { 
                                                        PdnResources.GetString("MainForm.DragDrop.Question.OpenOrImport.Open"),
                                                        PdnResources.GetString("MainForm.DragDrop.Question.OpenOrImport.Import")
                                                    };

                    if (DialogResult.OK != PdnMessageBox.Show(PdnResources.GetString("MainForm.DragDrop.Question.OpenOrImport.Title"), 
                        PdnInfo.GetProductName(), choices, ref result, this, this.LayersImportFromFileIcon))
                    {
                        return;
                    }
                    else 
                    {
                        importAsLayers = (result == 1);
                    }
                }
                else
                {
                    // Ask for confirmation
                    DialogResult result = Utility.AskOKCancel(this, PdnResources.GetString("MainForm.DragDrop.Question.Confirmation.Title"));

                    if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                if (!importAsLayers)
                {
                    DoOpenFile(files[0], true);
                }
                else
                {
                    ImportFromFileAction action = new ImportFromFileAction(this.workspace);
                    HistoryAction ha = action.ImportMultipleFiles(files);

                    if (ha != null)
                    {
                        workspace.History.PushNewAction(ha);
                    }
                }
            }
        }

        // When the user presses F1, two things happen: OnHelpRequested is called, AND
        // the Menu->Help menu item handler is called. Thus, if the user presses F1 the 
        // help file will be displayed twice.
        // So our implementation of OnHelpRequested only responds to the menu item
        // and to distinguish between the two requests we choose to use the .NET type system
        // instead of a magical value for the mouse position.
        private sealed class HelpEventArgs2 : HelpEventArgs
        {
            public HelpEventArgs2(Point mousePos)
                : base(mousePos)
            {
            }            
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            if (!hevent.Handled)
            {
                if (hevent is MainForm.HelpEventArgs2)
                {
                    Utility.ShowHelp(this);
                }

                hevent.Handled = true;
            }

            base.OnHelpRequested (hevent);
        }

        private void menuHelpHelpTopics_Click(object sender, System.EventArgs e)
        {
            OnHelpRequested(new HelpEventArgs2(Control.MousePosition));
        }

        private void menuFileOpenInNewWindow_Click(object sender, System.EventArgs e)
        {
            string fileName;
            string startingDir = Path.GetDirectoryName(workspace.DocumentFileName);
            DialogResult result = ChooseFile(this, out fileName, startingDir);

            if (result == DialogResult.OK)
            {
                Startup.StartNewInstance(fileName);
            }        
        }

        private void menuFileNewWindow_Click(object sender, System.EventArgs e)
        {
            Startup.StartNewInstance(null);
        }

        private void workspace_Scroll(object sender, System.EventArgs e)
        {
            // Commenting this out because it interferes with the one-liner help texts
            //this.contextStatusBar.Text = workspace.DocumentView.DocumentScrollPosition.ToString();
        }

        private void UserSessions_SessionChanged(object sender, EventArgs e)
        {
            if (UserSessions.IsRemote())
            {
                //this.contextStatusBar.Text = "Transparent windows have been disabled to improve remote session performance.";
                this.invalidateTimer.Interval = 200;
                this.floaterOpacityTimer.Enabled = false;
                this.menuWindowTranslucent.Checked = false;
                this.menuWindowTranslucent.Enabled = false;
            }
            else
            {
                //this.contextStatusBar.Text = string.Empty;
                this.invalidateTimer.Interval = effectRefreshInterval;
                this.floaterOpacityTimer.Enabled = true;
                this.menuWindowTranslucent.Enabled = true;
            }
        }

        private void StressOpen(string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                Update();
        
                try
                {
                    Image image = PdnResources.LoadImage(fileName); //Image.FromFile(fileName);
                    image.Dispose();
                    this.DoOpenFile(fileName);
                    Update();
                    Application.DoEvents();
                }

                catch
                {
                }
            }
        }

        private void menuItem14_Click(object sender, System.EventArgs e)
        {
            this.BeginInvoke(new VoidVoidDelegate(this.StressOpen), null);
        }

        private void StressOpen(string dir)
        {
            string[] dirs = Directory.GetDirectories(dir);

            StressOpen(Directory.GetFiles(dir, "*.jpg"));
            StressOpen(Directory.GetFiles(dir, "*.jpeg"));
            StressOpen(Directory.GetFiles(dir, "*.jpe"));
            StressOpen(Directory.GetFiles(dir, "*.png"));
            StressOpen(Directory.GetFiles(dir, "*.bmp"));
            StressOpen(Directory.GetFiles(dir, "*.pdn"));

            foreach (string theDir in dirs)
            {
                StressOpen(Path.Combine(dir, theDir));
            }
        }

        public void StressOpen()
        {
            StressOpen(@"C:\");
            StressOpen(@"D:\");
        }

        private void DocumentView_ScaleFactorChanged(object sender, EventArgs e)
        {
            SetTitleText();
        }

        private void menuFileOpenRecent_DropDownOpening(object sender, System.EventArgs e)
        {
            LoadMruList();
            MostRecentFile[] filesReverse = mostRecentFiles.GetFileList();
            MostRecentFile[] files = new MostRecentFile[filesReverse.Length];
            int i;

            for (i = 0; i < filesReverse.Length; ++i)
            {
                files[files.Length - i - 1] = filesReverse[i];
            }

            foreach (ToolStripItem mi in menuFileOpenRecent.DropDownItems)
            {
                mi.Click -= new EventHandler(menuFileOpenRecentFile_Click);
            }

            menuFileOpenRecent.DropDownItems.Clear();

            i = 0;

            foreach (MostRecentFile mrf in files)
            {
                string menuName;

                if (i < 9)
                {
                    menuName = "&";
                }
                else
                {
                    menuName = "";
                }

                menuName += (1 + i).ToString() + " " + Path.GetFileName(mrf.FileName);
                ToolStripMenuItem mi = new ToolStripMenuItem(menuName);
                mi.Click += new EventHandler(menuFileOpenRecentFile_Click);
                mi.ImageScaling = ToolStripItemImageScaling.None;
                mi.Image = (Image)mrf.Thumb.Clone();
                menuFileOpenRecent.DropDownItems.Add(mi);
                ++i;
            }

            if (menuFileOpenRecent.DropDownItems.Count == 0)
            {
                ToolStripMenuItem none = new ToolStripMenuItem(PdnResources.GetString("MainForm.Menu.File.OpenRecent.None"));
                none.Enabled = false;
                menuFileOpenRecent.DropDownItems.Add(none);
            }
            else
            {
                ToolStripSeparator separator = new ToolStripSeparator();
                menuFileOpenRecent.DropDownItems.Add(separator);

                ToolStripMenuItem clearList = new ToolStripMenuItem();
                clearList.Text = PdnResources.GetString("MainForm.Menu.File.OpenRecent.ClearThisList");
                menuFileOpenRecent.DropDownItems.Add(clearList);
                Image deleteIcon = PdnResources.GetImage("Icons.MenuEditEraseSelectionIcon.png");
                clearList.ImageTransparentColor = Utility.TransparentKey;
                clearList.ImageAlign = ContentAlignment.MiddleCenter;
                clearList.ImageScaling = ToolStripItemImageScaling.None;
                Bitmap bitmap = new Bitmap(mruIconSize + 2, mruIconSize + 2);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(clearList.ImageTransparentColor);

                    Point offset = new Point((bitmap.Width - deleteIcon.Width) / 2, 
                        (bitmap.Height - deleteIcon.Height) / 2);

                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(deleteIcon, offset.X, offset.Y, deleteIcon.Width, deleteIcon.Height);
                }

                clearList.Image = bitmap;
                clearList.Click += new EventHandler(clearList_Click);
            }
        }

        private void menuFileOpenRecentFile_Click(object sender, System.EventArgs e)
        {
            try
            {
                ToolStripMenuItem mi = (ToolStripMenuItem)sender;
                int spaceIndex = mi.Text.IndexOf(" ");
                string indexString = mi.Text.Substring(1, spaceIndex - 1);
                int index = int.Parse(indexString) - 1;
                MostRecentFile[] recentFiles = mostRecentFiles.GetFileList();
                string fileName = recentFiles[recentFiles.Length - index - 1].FileName;
                this.DoOpenFile(fileName);
            }

            catch
            {
            }
        }

        private void menuLayersImportFromFile_click(object sender, System.EventArgs e)
        {   
            workspace.PerformAction(typeof(ImportFromFileAction));
        }

        private void Environment_ToolChanged(object sender, EventArgs e)
        {
            this.SetToolHelpText();
        }

        private void menuWindowTranslucent_Click(object sender, System.EventArgs e)
        {
            PdnBaseForm.EnableOpacity = !PdnBaseForm.EnableOpacity;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged (e);
            SetTitleText();
        }

        private void menuViewRulers_Click(object sender, System.EventArgs e)
        {
            workspace.DocumentView.RulersEnabled = !workspace.DocumentView.RulersEnabled;
        }

        private void menuViewGrid_Click(object sender, System.EventArgs e)
        {
            workspace.DocumentView.DrawGrid = !workspace.DocumentView.DrawGrid;     
        }

        private void menuToolsAntiAliasing_Click(object sender, System.EventArgs e)
        {
            workspace.Environment.AntiAliasing = !workspace.Environment.AntiAliasing;
        }

        private void menuView_DropDownOpening(object sender, System.EventArgs e)
        {
            menuViewZoomToSelection.Enabled = !workspace.Environment.Selection.IsEmpty;
            menuViewZoomToWindow.Checked = (workspace.ZoomBasis == ZoomBasis.Window);
            menuViewGrid.Checked = workspace.DocumentView.DrawGrid;
            menuViewRulers.Checked = workspace.DocumentView.RulersEnabled;
        }

        private void DeferredInitialization(object sender, EventArgs e)
        {
            deferredInitializationTimer.Enabled = false;
            deferredInitializationTimer.Tick -= new EventHandler(DeferredInitialization);
            deferredInitializationTimer.Dispose();
            deferredInitializationTimer = null;

            PopulateEffectsAndAdjustmentsMenus();
            UserSessions.SessionChanged += new EventHandler(UserSessions_SessionChanged);
        }

        private void ftf_VisibleChanged(object sender, EventArgs e)
        {
            workspace.DocumentView.Focus();
        }

        private class CheckForUpdatesDialog
            : CallbackWithProgressDialog
        {
            private ThreadStart callback;

            public CheckForUpdatesDialog(Control owner, ThreadStart callback)
                : base(owner, 
                       PdnResources.GetString("CheckForUpdatesDialog.Text"),
                       PdnResources.GetString("CheckForUpdatesDialog.Description"))
            {
                this.callback = callback;
                this.Icon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuFileUpdatesIcon.png"), true);
            }

            public DialogResult ShowDialog()
            {
                return base.ShowDialog(false, true, callback);
            }
        }

        private class DownloadAndUnzipUpdateDialog
            : CallbackWithProgressDialog
        {
            private int maxBytes;
            private int bytesSoFar;
            private PdnVersionInfo downloadMe;
            private SiphonStream abortMe;
            private string upgradeMsiFileName;

            public string UpgradeMsiFileName
            {
                get
                {
                    return this.upgradeMsiFileName;
                }
            }

            protected override void OnCancelClick()
            {
                SiphonStream ss = this.abortMe;

                if (ss != null)
                {
                    ss.Abort(new ApplicationException("Cancelled"));
                }

                base.OnCancelClick ();
            }

            private void OnSiphonStreamIOFinished(object sender, IOEventArgs e)
            {
                this.bytesSoFar += e.Count;
                double percent = 100.0 * ((double)this.bytesSoFar / (double)this.maxBytes);
                this.Progress = (int)Math.Ceiling(percent);
            }

            private void DownloadAndUnzipUpdate()
            {
                // Download
                string zipTempName = Path.GetTempFileName() + ".zip";

                try
                {
                    bool getFull;

                    if (Utility.IsDotNetVersionInstalled(downloadMe.NetFxVersion.Major, downloadMe.NetFxVersion.Minor, 
                        downloadMe.NetFxVersion.Build))
                    {
                        getFull = false;
                    }
                    else
                    {
                        getFull = true;
                    }

                    this.bytesSoFar = 0;
                    
                    if (getFull)
                    {
                        this.maxBytes = downloadMe.FullDownloadSize;
                    }
                    else
                    {
                        this.maxBytes = downloadMe.DownloadSize;
                    }

                    this.Progress = 0;
                    FileStream zipFileWrite = new FileStream(zipTempName, FileMode.Create, FileAccess.Write, FileShare.Read);

                    try
                    {
                        SiphonStream siphonStream1 = new SiphonStream(zipFileWrite, 4096);
                        this.abortMe = siphonStream1;

                        if (this.maxBytes > 0)
                        {
                            siphonStream1.IOFinished += new IOEventHandler(OnSiphonStreamIOFinished);
                        }

                        string url;

                        if (getFull)
                        {
                            url = downloadMe.FullDownloadUrl;
                        }
                        else
                        {
                            url = downloadMe.DownloadUrl;
                        }

                        Updates.DownloadFile(new Uri(url), siphonStream1);

                        if (this.maxBytes > 0)
                        {
                            siphonStream1.IOFinished -= new IOEventHandler(OnSiphonStreamIOFinished);
                        }
                
                        this.abortMe = null;
                        siphonStream1 = null;
                    }

                    finally
                    {
                        if (zipFileWrite != null)
                        {
                            zipFileWrite.Close();
                            zipFileWrite = null;
                        }
                    }

                    // Extract if necessary

                    this.Description = PdnResources.GetString("DownloadAndUnzipUpdateDialog.Description.Extracting");
                    FileStream zipFileRead = new FileStream(zipTempName, FileMode.Open, FileAccess.Read, FileShare.Read);

                    try
                    {
                        ICSharpCode.SharpZipLib.Zip.ZipInputStream zipStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(zipFileRead);

                        // Search for the first .msi file in the zip, and extract it
                        ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry;
                        bool foundMsi = false;

                        while (true)
                        {
                            zipEntry = zipStream.GetNextEntry();

                            if (zipEntry == null)
                            {
                                break;
                            }

                            if (!zipEntry.IsDirectory &&
                                (string.Compare(".msi", Path.GetExtension(zipEntry.Name), true, CultureInfo.InvariantCulture) == 0 ||
                                 string.Compare(".exe", Path.GetExtension(zipEntry.Name), true, CultureInfo.InvariantCulture) == 0))
                            {
                                foundMsi = true;
                                break;
                            }
                        }

                        string msiFileName = null;

                        try
                        {
                            if (foundMsi)
                            {
                                this.maxBytes = (int)zipEntry.Size;
                                this.bytesSoFar = 0;
                    
                                msiFileName = Path.Combine(Path.GetDirectoryName(zipTempName), zipEntry.Name);
                                FileStream msiFileWrite = new FileStream(msiFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                                SiphonStream siphonStream2 = new SiphonStream(msiFileWrite, 4096);

                                this.Progress = 0;

                                if (this.maxBytes > 0)
                                {
                                    siphonStream2.IOFinished += new IOEventHandler(OnSiphonStreamIOFinished);
                                }

                                Utility.CopyStream(zipStream, siphonStream2);

                                if (this.maxBytes > 0)
                                {
                                    siphonStream2.IOFinished -= new IOEventHandler(OnSiphonStreamIOFinished);
                                }

                                siphonStream2 = null;
                                msiFileWrite.Close();
                                msiFileWrite = null;
                            }

                            if (foundMsi)
                            {
                                this.upgradeMsiFileName = msiFileName;
                                msiFileName = null;
                            }
                        }

                        finally
                        {
                            // If there was an error we will delete the MSI
                            if (msiFileName != null)
                            {   
                                try
                                {
                                    File.Delete(msiFileName);
                                }

                                catch
                                {
                                }

                                msiFileName = null;
                            }
                        }
                    }

                    finally
                    {
                        if (zipFileRead != null)
                        {
                            zipFileRead.Close();
                            zipFileRead = null;
                        }
                    }
                }

                finally
                {
                    // Done!
                    try
                    {
                        File.Delete(zipTempName);
                    }

                    catch
                    {
                    }
                }
            }

            //private string upgradeMsiFileName = null;

            public void ShowDialog()
            {
                try
                {
                    this.ShowDialog(true, false, new ThreadStart(this.DownloadAndUnzipUpdate));
                }

                catch (WorkerThreadException)
                {
                    if (!this.Cancelled)
                    {                        
                        string message = PdnResources.GetString("DownloadAndUnzipUpdateDialog.Error.Generic");
                        Utility.ErrorBox(this.Owner, message);
                    }
                }
            }

            public DownloadAndUnzipUpdateDialog(Control owner, PdnVersionInfo downloadMe)
                : base(owner,
                       PdnResources.GetString("DownloadAndUnzipUpdateDialog.Text"),
                       PdnResources.GetString("DownloadAndUnzipUpdateDialog.Description.Downloading"))
            {
                this.downloadMe = downloadMe;
                this.Icon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuFileUpdatesIcon.png"), true);
            }
        }

        private void menuFileUpdatesCheckNow_Click(object sender, EventArgs e)
        {
            if (!Security.IsAdministrator)
            {
                Utility.ShowNonAdminErrorBox(this);
            }
            else
            {
                DisableUpdatesButton();

                CheckForUpdatesDialog dialog = new CheckForUpdatesDialog(this, new ThreadStart(
                    this.BackgroundCheckForUpdates));

                Updates.PingLastUpdateCheckTime();
                DialogResult result = dialog.ShowDialog();

                if (result != DialogResult.Cancel)
                {
                    if (this.versionManifest != null && this.versionManifestIndex != -1)
                    {
                        this.ShowUpdateDialog(this.versionManifest, this.versionManifestIndex);
                        this.versionManifest = null;
                        this.versionManifestIndex = -1;
                    }
                    else
                    {
                        string errorMessage = PdnResources.GetString("CheckForUpdates.NoUpdatesFound");

                        WebException asWebException = this.versionCheckException as WebException;
                        if (asWebException != null)
                        {
                            switch (asWebException.Status)
                            {
                                case WebExceptionStatus.ProtocolError:
                                    string format = PdnResources.GetString("WebExceptionStatus.ProtocolError.Format");
                                    HttpStatusCode statusCode = ((HttpWebResponse)asWebException.Response).StatusCode;
                                    errorMessage = string.Format(format, statusCode.ToString(), (int)statusCode);
                                    break;

                                default:
                                    string stringName = "WebExceptionStatus." + asWebException.Status.ToString();
                                    errorMessage = PdnResources.GetString(stringName);
                                    break;
                            }
                        }

                        Tracing.Ping("Exception from updates checking: " + this.versionCheckException);
                        Utility.InfoBox(this, errorMessage);
                    }
                }
            }
        }

        private void menuFileUpdatesAutoCheckEnabled_Click(object sender, EventArgs e)
        {
            if (Security.IsAdministrator)
            {
                bool autoCheckForUpdates = ("1" == Settings.SystemWide.GetString(PdnSettings.AutoCheckForUpdates, "0"));
                Settings.SystemWide.SetString(PdnSettings.AutoCheckForUpdates, (!autoCheckForUpdates) ? "1" : "0");

                if (!autoCheckForUpdates)
                {
                    DisableUpdatesButton();
                    Settings.SystemWide.SetBoolean(PdnSettings.UpdateIsAvailable, false);
                }
            }
            else
            {
                Utility.ShowNonAdminErrorBox(this);
            }
        }

        private void menuFileUpdatesCheckForBetas_Click(object sender, EventArgs e)
        {
            if (Security.IsAdministrator)
            {
                bool alsoCheckForBetas = ("1" == Settings.SystemWide.GetString(PdnSettings.AlsoCheckForBetas, "0"));
                Settings.SystemWide.SetString(PdnSettings.AlsoCheckForBetas, (!alsoCheckForBetas) ? "1" : "0");
            }
            else
            {
                Utility.ShowNonAdminErrorBox(this);
            }
        }

        private void menuFileUpdates_DropDownOpening(object sender, EventArgs e)
        {
            this.menuFileUpdatesAutoCheckEnabled.Checked = ("1" == Settings.SystemWide.GetString(PdnSettings.AutoCheckForUpdates, "0"));
            this.menuFileUpdatesCheckForBetas.Checked = ("1" == Settings.SystemWide.GetString(PdnSettings.AlsoCheckForBetas, "0"));

            bool isAdmin = Security.IsAdministrator;
            this.menuFileUpdatesAutoCheckEnabled.Enabled = isAdmin;
            this.menuFileUpdatesCheckForBetas.Enabled = isAdmin;
        }

        private void defaultButton_Click(object sender, EventArgs e)
        {
            workspace.DocumentView.Focus();
            
            // Since defaultButton is the AcceptButton, hitting Enter will get 'eaten' but this button
            // So we have to give the Enter key to the Tool
            if (workspace.Environment.Tool != null)
            {
                workspace.Environment.Tool.PerformKeyPress(new KeyPressEventArgs('\r'));
                workspace.Environment.Tool.PerformKeyPress(Keys.Enter);
            }
        }

        private void Environment_UnitsChanged(object sender, EventArgs e)
        {
            workspace_DocumentChanged(sender, e);
            Environment_SelectedPathChanged(sender, e); 
            this.cursorInfoStatusLabel.Text = string.Empty;
        }

        private void History_Changed(object sender, EventArgs e)
        {
            // some actions change the document size: make sure we update our status bar panel
            this.workspace_DocumentChanged(sender, EventArgs.Empty);
        }

        private void menuLayersRotateZoom_Click(object sender, EventArgs e)
        {
            this.menuEffects_ClickHandler(this.menuLayersRotateZoom, EventArgs.Empty);
        }

        private class MenuTitleAndLocale
        {
            public string title;
            public string locale;

            public MenuTitleAndLocale(string title, string locale)
            {
                this.title = title;
                this.locale = locale;
            }
        }

        private string GetCultureInfoName(CultureInfo ci)
        {
            if (ci.Parent.Name == "")
            {
                return ci.NativeName;
            }
            else
            {
                return GetCultureInfoName(ci.Parent);
            }
        }

        private void menuFileLanguage_DropDownOpening(object sender, EventArgs e)
        {
            this.menuFileLanguage.DropDownItems.Clear();

            string[] locales = PdnResources.GetInstalledLocales();

            MenuTitleAndLocale[] mtals = new MenuTitleAndLocale[locales.Length];

            for (int i = 0; i < locales.Length; ++i)
            {
                string locale = locales[i];
                CultureInfo ci = new CultureInfo(locale);
                mtals[i] = new MenuTitleAndLocale(ci.DisplayName, locale);
            }

            Array.Sort(
                mtals,
                delegate(MenuTitleAndLocale x, MenuTitleAndLocale y)
                {
                    return string.Compare(x.title, y.title, StringComparison.InvariantCultureIgnoreCase);
                });

            foreach (MenuTitleAndLocale mtal in mtals)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem();
                menuItem.Text = GetCultureInfoName(new CultureInfo(mtal.locale));
                menuItem.Tag = mtal.locale;
                menuItem.Click += new EventHandler(LanguageMenuItem_Click);

                if (mtal.locale == CultureInfo.CurrentUICulture.Name)
                {
                    menuItem.Checked = true;
                }

                this.menuFileLanguage.DropDownItems.Add(menuItem);
            }
        }
       
        private void LanguageMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem miwt = (ToolStripMenuItem)sender;
            Settings.CurrentUser.SetString(PdnSettings.LanguageName, (string)miwt.Tag);
            string message = PdnResources.GetString("SetLanguage.PleaseRestartApplication");
            Utility.InfoBox(this, message);
        }

        private void menuToolsAlphaBlending_Click(object sender, EventArgs e)
        {
            workspace.Environment.AlphaBlending = !workspace.Environment.AlphaBlending;
        }

        private void clearList_Click(object sender, EventArgs e)
        {
            string question = PdnResources.GetString("ClearOpenRecentList.Dialog.Text");
            DialogResult result = Utility.AskYesNo(this, question);

            if (result == DialogResult.Yes)
            {
                this.mostRecentFiles.Clear();
                SaveMruList();
            }
        }

        private void workspace_ToolStatusChanged(object sender, EventArgs e)
        {
            this.contextStatusLabel.Text = workspace.Environment.Tool.StatusText;
            this.contextStatusLabel.Image = workspace.Environment.Tool.StatusIcon.ToBitmap();
        }

        private string GetEmailLaunchString(string email, string subject, string body)
        {
            const string emailFormat = "mailto:{0}?subject={1}&body={2}";
            string bodyUE = body.Replace("\r\n", "%0D%0A");
            string launchString = string.Format(emailFormat, email, subject, bodyUE);
            return launchString;
        }

        private void menuHelpSendFeedback_Click(object sender, EventArgs e)
        {
            string email = InvariantStrings.FeedbackEmail;
            string subjectFormat = PdnResources.GetString("SendFeedback.Email.Subject.Format");
            string subject = string.Format(subjectFormat, PdnInfo.GetFullAppName());
            string body = PdnResources.GetString("SendFeedback.Email.Body");
            string launchMe = GetEmailLaunchString(email, subject, body);
            launchMe = launchMe.Substring(0, Math.Min(1024, launchMe.Length));

            try
            {
                Process.Start(launchMe);
            }

            catch
            {
                string message = PdnResources.GetString("MainForm.LoadDocument.Error.FileNotFoundException");
                Utility.ErrorBox(this, message);
            }
        }

        private void menuViewUnits_DropDownOpening(object sender, EventArgs e)
        {
            menuViewUnitsPixels.Checked = false;
            menuViewUnitsInches.Checked = false;
            menuViewUnitsCentimeters.Checked = false;

            switch (workspace.Environment.Units)
            {
                case MeasurementUnit.Pixel:
                    menuViewUnitsPixels.Checked = true;
                    break;

                case MeasurementUnit.Inch:
                    menuViewUnitsInches.Checked = true;
                    break;

                case MeasurementUnit.Centimeter:
                    menuViewUnitsCentimeters.Checked = true;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        private void menuViewUnitsPixels_Click(object sender, EventArgs e)
        {
            workspace.Environment.Units = MeasurementUnit.Pixel;
        }

        private void menuViewUnitsInches_Click(object sender, EventArgs e)
        {
            workspace.Environment.Units = MeasurementUnit.Inch;
        }

        private void menuViewUnitsCentimeters_Click(object sender, EventArgs e)
        {
            workspace.Environment.Units = MeasurementUnit.Centimeter;
        }
    }
}
