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
    /// <summary>
    /// Summary description for MainForm.
    /// </summary>
    public class MainForm 
        : PaintDotNet.PdnBaseForm
    {
        // This class is used to reduce flickering in the status bar when the width changes
        // We limit redrawing to 10 times per second.
        private class PdnStatusBar
            : StatusBar
        {
            private int oldWidth = -1;
            private DateTime lastTime = DateTime.MinValue;
            private System.Windows.Forms.Timer timer = null;

            protected override void OnResize(EventArgs e)
            {
                if (this.oldWidth != this.Width)
                {
                    UI.SetControlRedraw(this, false);
                }

                base.OnResize (e);

                if (this.oldWidth != this.Width)
                {
                    UI.SetControlRedraw(this, true);

                    if (timer == null)
                    {
                        timer = new System.Windows.Forms.Timer();
                        timer.Tick += new EventHandler(timer_Tick);
                    }

                    if (DateTime.Now - this.lastTime > new TimeSpan(0, 0, 0, 0, 100))
                    {
                        this.lastTime = DateTime.Now;
                        Invalidate();
                        Update();
                    }
                    else if (!timer.Enabled)
                    {
                        timer.Interval = 100;
                        timer.Enabled = true;
                    }
                }

                this.oldWidth = this.Width;
            }

            private void timer_Tick(object sender, EventArgs e)
            {
                Invalidate();
                Update();
                timer.Enabled = false;
            }
        }

        private const int effectRefreshInterval = 100;
        private const int tilesPerCpu = 25;
        private int renderingThreadCount = Math.Max(2, Processor.LogicalCpuCount);

        private System.Windows.Forms.MainMenu mainMenu;

        private System.Windows.Forms.MenuItem menuFile;
        private System.Windows.Forms.MenuItem menuFileExit;
        private System.Windows.Forms.MenuItem menuFileOpen;
        private System.Windows.Forms.MenuItem menuFileNew;
        private System.Windows.Forms.MenuItem menuFileSave;
        private System.Windows.Forms.MenuItem menuFileSaveAs;
        private System.Windows.Forms.MenuItem menuFileAcquire;
        private System.Windows.Forms.MenuItem menuFileAcquireFromScannerOrCamera;
        private System.Windows.Forms.MenuItem menuFileAcquireFromClipboard;
        private System.Windows.Forms.MenuItem menuFileOpenInNewWindow;
        private System.Windows.Forms.MenuItem menuFileNewWindow;
        private System.Windows.Forms.MenuItem menuFileOpenRecent;
        private System.Windows.Forms.MenuItem menuFileLanguage;
        private System.Windows.Forms.MenuItem menuFileLanguageSentinel;
        private System.Windows.Forms.MenuItem menuFileUpdates;
        private System.Windows.Forms.MenuItem menuFileUpdatesCheckNow;
        private System.Windows.Forms.MenuItem menuFileUpdatesAutoCheckEnabled;
        private System.Windows.Forms.MenuItem menuFileUpdatesCheckForBetas;
        private System.Windows.Forms.MenuItem menuFilePrint;

        private System.Windows.Forms.MenuItem menuEdit;
        private System.Windows.Forms.MenuItem menuEditUndo;
        private System.Windows.Forms.MenuItem menuEditRedo;
        private System.Windows.Forms.MenuItem menuEditCopy;
        private System.Windows.Forms.MenuItem menuEditPaste;
        private System.Windows.Forms.MenuItem menuEditCut;
        private System.Windows.Forms.MenuItem menuEditInvertSelection;
        private System.Windows.Forms.MenuItem menuEditSelectAll;
        private System.Windows.Forms.MenuItem menuEditDeselect;
        private System.Windows.Forms.MenuItem menuEditEraseSelection;
        private System.Windows.Forms.MenuItem menuEditPasteInToNewLayer;

        private System.Windows.Forms.MenuItem menuView;
        private System.Windows.Forms.MenuItem menuViewZoomIn;
        private System.Windows.Forms.MenuItem menuViewZoomOut;
        private System.Windows.Forms.MenuItem menuViewZoomToWindow;
        private System.Windows.Forms.MenuItem menuViewZoomToSelection;
        private System.Windows.Forms.MenuItem menuViewActualSize;
        private System.Windows.Forms.MenuItem menuViewSeperator;
        private System.Windows.Forms.MenuItem menuViewGrid;
        private System.Windows.Forms.MenuItem menuViewRulers;
        private System.Windows.Forms.MenuItem menuViewUnits;
        private System.Windows.Forms.MenuItem menuViewUnitsPixels;
        private System.Windows.Forms.MenuItem menuViewUnitsInches;
        private System.Windows.Forms.MenuItem menuViewUnitsCentimeters;

        private System.Windows.Forms.MenuItem menuImage;
        private System.Windows.Forms.MenuItem menuImageCrop;
        private System.Windows.Forms.MenuItem menuImageResize;
        private System.Windows.Forms.MenuItem menuImageFlip;
        private System.Windows.Forms.MenuItem menuImageFlipHorizontal;
        private System.Windows.Forms.MenuItem menuImageFlipVertical;
        private System.Windows.Forms.MenuItem menuImageFlatten;
        private System.Windows.Forms.MenuItem menuImageCanvasSize;
        private System.Windows.Forms.MenuItem menuImageRotate;
        private System.Windows.Forms.MenuItem menuImageRotate90CW;
        private System.Windows.Forms.MenuItem menuImageRotate180CW;
        private System.Windows.Forms.MenuItem menuImageRotate270CW;
        private System.Windows.Forms.MenuItem menuImageRotate90CCW;
        private System.Windows.Forms.MenuItem menuImageRotate180CCW;
        private System.Windows.Forms.MenuItem menuImageRotate270CCW;

        private System.Windows.Forms.MenuItem menuLayers;
        private System.Windows.Forms.MenuItem menuLayersAddNewLayer;
        private System.Windows.Forms.MenuItem menuLayersDeleteLayer;
        private System.Windows.Forms.MenuItem menuLayersFlip;
        private System.Windows.Forms.MenuItem menuLayersFlipHorizontal;
        private System.Windows.Forms.MenuItem menuLayersFlipVertical;
        private System.Windows.Forms.MenuItem menuLayersRotateZoom;
        private System.Windows.Forms.MenuItem menuLayersDuplicateLayer;
        private System.Windows.Forms.MenuItem menuLayersLayerProperties;
        private System.Windows.Forms.MenuItem menuLayersAdjustments;
        private System.Windows.Forms.MenuItem menuLayersImportFromFile;

        private System.Windows.Forms.MenuItem menuEffects;
        private System.Windows.Forms.MenuItem menuEffectsSentinel;

        private System.Windows.Forms.MenuItem menuTools;
        private System.Windows.Forms.MenuItem menuToolsAntiAliasing;
        private System.Windows.Forms.MenuItem menuToolsAlphaBlending;
        private System.Windows.Forms.MenuItem menuToolsSeperator;

        private System.Windows.Forms.MenuItem menuWindow;
        private System.Windows.Forms.MenuItem menuWindowResetWindowLocations;
        private System.Windows.Forms.MenuItem menuWindowTools;
        private System.Windows.Forms.MenuItem menuWindowHistory;
        private System.Windows.Forms.MenuItem menuWindowLayers;
        private System.Windows.Forms.MenuItem menuWindowColors;
        private System.Windows.Forms.MenuItem menuWindowTranslucent;

        private System.Windows.Forms.MenuItem menuHelp;
        private System.Windows.Forms.MenuItem menuHelpHelpTopics;
        private System.Windows.Forms.MenuItem menuHelpSendFeedback;
        private System.Windows.Forms.MenuItem menuHelpAbout;

        private System.Windows.Forms.MenuItem menuDebug;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem4;

        private System.Windows.Forms.MenuItem menuSeparator1;
        private System.Windows.Forms.MenuItem menuSeparator2;
        private System.Windows.Forms.MenuItem menuSeparator4;
        private System.Windows.Forms.MenuItem menuSeparator5;
        private System.Windows.Forms.MenuItem menuSeparator6;
        private System.Windows.Forms.MenuItem menuSeparator9;
        private System.Windows.Forms.MenuItem menuSeparator10;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem10;
        private System.Windows.Forms.MenuItem menuItem11;
        private System.Windows.Forms.MenuItem menuItem8;
        private System.Windows.Forms.MenuItem menuItem14;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.MenuItem menuItem9;
        private System.Windows.Forms.MenuItem menuItem13;
        private System.Windows.Forms.MenuItem menuItem16;
        private System.Windows.Forms.MenuItem menuItem17;
        private System.Windows.Forms.MenuItem menuItem12;
        private System.Windows.Forms.MenuItem menuItem18;

        private System.ComponentModel.IContainer components;
        private PaintDotNet.DocumentWorkspace workspace;

        private System.Windows.Forms.ImageList menuImages;
        private EventHandler menuEffectsClickDelegate;
        private EventHandler menuToolsClickDelegate;
        private CancelEventHandler hideInsteadOfCloseDelegate;

        // NOTE: This is done as an object and not EffectConfigToken so that we can delay loading
        //       the PaintDotNet.Effects.dll until after we start up
        private object lastEffectToken = null;

        // NOTE: This is done as an object and not Effect so that we can delay loading the 
        //       PaintDotNet.Effects.dll until after we start up
        private object lastEffect = null;

        private System.Windows.Forms.StatusBar statusBar;
        private System.Windows.Forms.StatusBarPanel progressStatusBar;
        private System.Windows.Forms.StatusBarPanel imageInfoStatusBar;
        private System.Windows.Forms.StatusBarPanel cursorInfoStatusBar;
        private System.Windows.Forms.StatusBarPanel contextStatusBar;

        private System.Windows.Forms.MenuItem menuSeparator7;

        // We keep track of each configurable effect's last token
        // This way it keeps its values in between user invocations
        private Hashtable effectTokenHash = new Hashtable();
        private System.Windows.Forms.MenuItem menuSeparator8;
        private System.Windows.Forms.Timer floaterOpacityTimer;
        private FloatingToolForm[] floaters;
        private System.Windows.Forms.Timer populateEffectsTimer;

        private MostRecentFiles mostRecentFiles = null;
        private const int defaultMostRecentFilesMax = 8;
        private const int mruIconSize = 40;
        private System.Windows.Forms.ImageList mruImageList;
        private DotNetWidgets.DotNetMenuProvider mruDotNetMenuProvider = null;
        private DotNetWidgets.DotNetMenuProvider dotNetMenuProvider = null;
        private System.Windows.Forms.Timer invalidateTimer;

        private Icon paintDotNetIcon;
        private Icon stopWatchIcon;
        private Icon selectionIcon;
        private Icon helpIcon;
        private Icon cursorXYIcon;
        private Icon imageSizeIcon;
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

        private Icon HelpIcon
        {
            get
            {
                if (this.helpIcon == null)
                {
                    this.helpIcon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuHelpHelpTopicsIcon.bmp"), true);
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
                    this.layersImportFromFileIcon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuLayersImportFromFileIcon.bmp"), true);
                }

                return this.layersImportFromFileIcon;
            }
        }

        private Icon StopWatchIcon
        {
            get
            {
                if (this.stopWatchIcon == null)
                {
                    this.stopWatchIcon = PdnResources.GetIcon("Icons.StopWatchIcon.ico");
                }

                return this.stopWatchIcon;
            }
        }

        private Icon SelectionIcon
        {
            get
            {
                if (this.selectionIcon == null)
                {
                    this.selectionIcon = PdnResources.GetIcon("Icons.SelectionIcon.ico");
                }

                return this.selectionIcon;
            }
        }

        private Icon CursorXYIcon
        {
            get
            {
                if (this.cursorXYIcon == null)
                {
                    this.cursorXYIcon = PdnResources.GetIcon("Icons.CursorXYIcon.ico");
                }

                return this.cursorXYIcon;
            }
        }

        private Icon ImageSizeIcon
        {
            get
            {
                if (this.imageSizeIcon == null)
                {
                    this.imageSizeIcon = PdnResources.GetIcon("Icons.ImageSizeIcon.ico");
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
                    this.addNewLayerIcon = PdnResources.GetImage("Icons.MenuLayersAddNewLayerIcon.bmp");
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
                    this.fileNewIcon = PdnResources.GetImage("Icons.MenuFileNewIcon.bmp");
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
                    this.editCutIcon = PdnResources.GetImage("Icons.MenuEditCutIcon.bmp");
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
                    this.imageFromDiskIcon = PdnResources.GetImage("Icons.ImageFromDiskIcon.bmp");
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

            this.dotNetMenuProvider = new DotNetWidgets.DotNetMenuProvider();
            this.components.Add(dotNetMenuProvider);
            dotNetMenuProvider.ImageList = this.menuImages;
            TurnOnSpecialDrawing();

            workspace.DocumentView.ScaleFactorChanged += new EventHandler(DocumentView_ScaleFactorChanged);
            this.mruImageList.ImageSize = new System.Drawing.Size(2 + MainForm.mruIconSize, 2 + MainForm.mruIconSize);

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

            // WHen the user changes the display resolution, we need to do some fixing of our UI
            // like making sure our floaters are actually on screen
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);

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
            UserSessions.SessionChanged += new EventHandler(UserSessions_SessionChanged);

            populateEffectsTimer.Enabled = true;

            workspace.Environment.UnitsChanged += new EventHandler(Environment_UnitsChanged);

            // they want to print? ok, queue it up
            if (doPrint && !doKill)
            {
                this.BeginInvoke(new VoidVoidDelegate(PrintOnStartup));
            }

            workspace.Environment.Tolerance = tolerance;
            Application.Idle += new EventHandler(Application_Idle);
        }

        protected override void OnEnableStyles()
        {
            // do nothing initially
            //base.OnEnableStyles();
        }

        private void SetToolHelpText()
        {
            if (workspace.Environment.Tool != null)
            {
                string toolName = workspace.Environment.Tool.Name;
                string helpText = workspace.Environment.Tool.HelpText;

                string contextFormat = PdnResources.GetString("MainForm.StatusBar.Context.Help.Text.Format");
                contextStatusBar.Text = string.Format(contextFormat, toolName, helpText);
                contextStatusBar.Icon = this.HelpIcon;
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

        private void TurnOnSpecialDrawing()
        {
            foreach (MenuItem mi in this.mainMenu.MenuItems)
            {
                TurnOnSpecialDrawing(mi, true);
            }
        }

        private void TurnOnSpecialDrawing(MenuItem menuItem, bool skipFirst)
        {
            if (!skipFirst)
            {
                dotNetMenuProvider.SetDrawSpecial(menuItem, true);
            }

            foreach (MenuItem mi in menuItem.MenuItems)
            {
                TurnOnSpecialDrawing(mi, false);
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
                this.workspace.DocumentView.RulersEnabled = Settings.CurrentUser.GetBoolean(PdnSettings.Rulers, false);
                this.workspace.DocumentView.DrawGrid = Settings.CurrentUser.GetBoolean(PdnSettings.DrawGrid, false);
                this.workspace.Environment.Units = (MeasurementUnit)Enum.Parse(typeof(MeasurementUnit), Settings.CurrentUser.GetString(PdnSettings.Units, MeasurementUnit.Pixel.ToString()), true);
                PdnBaseForm.EnableOpacity = Settings.CurrentUser.GetBoolean(PdnSettings.TranslucentWindows, true);
            }

            catch (Exception ex)
            {
                Tracing.Ping("Exception in MainForm.LoadSettings:" + ex.ToString());

                Settings.CurrentUser.Delete(new string[] { 
                                                             PdnSettings.Rulers, 
                                                             PdnSettings.DrawGrid, 
                                                             PdnSettings.TranslucentWindows,
                                                             PdnSettings.Tolerance,
                                                             PdnSettings.Units
                                                         });
            }
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

            Settings.CurrentUser.SetBoolean(PdnSettings.Rulers, this.workspace.DocumentView.RulersEnabled);
            Settings.CurrentUser.SetBoolean(PdnSettings.DrawGrid, this.workspace.DocumentView.DrawGrid);
            Settings.CurrentUser.SetString(PdnSettings.Units, this.workspace.Environment.Units.ToString(CultureInfo.InvariantCulture));
            Settings.CurrentUser.SetBoolean(PdnSettings.TranslucentWindows, PdnBaseForm.EnableOpacity);

            if (this.WindowState != FormWindowState.Minimized)
            {
                Settings.CurrentUser.SetBoolean(PdnSettings.ToolsFormVisible, this.workspace.Widgets.MainToolBarForm.Visible);
                Settings.CurrentUser.SetBoolean(PdnSettings.ColorsFormVisible, this.workspace.Widgets.ColorsForm.Visible);
                Settings.CurrentUser.SetBoolean(PdnSettings.HistoryFormVisible, this.workspace.Widgets.HistoryForm.Visible);
                Settings.CurrentUser.SetBoolean(PdnSettings.LayersFormVisible, this.workspace.Widgets.LayerForm.Visible);
            }

            SaveMruList();
        }

        private void SaveMruList()
        {
            if (mostRecentFiles == null)
            {
                return;
            }

            Settings.CurrentUser.SetInt32("MRUMax", this.mostRecentFiles.MaxCount);
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
            this.cursorInfoStatusBar.Icon = this.CursorXYIcon;
            this.cursorInfoStatusBar.Text = string.Empty;
            this.workspace.DocumentView.DocumentMouseMove += new MouseEventHandler(DocumentView_DocumentMouseMove);
            
            // imageInfo (width,height info)
            this.imageInfoStatusBar.Icon = this.ImageSizeIcon;
            this.workspace.DocumentChanged += new EventHandler(workspace_DocumentChanged);

            // progress
            this.progressStatusBar.Text = string.Empty;
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

        // ImageList functions
        private Hashtable imageListImages = new Hashtable(); // maps image->int which is used as index into this.menuImages
        private int AddImageToMenuImages(Image newImage)
        {
            object result = imageListImages[newImage];

            if (result == null)
            {
                int index = menuImages.Images.Add(newImage, menuImages.TransparentColor);
                imageListImages.Add(newImage, index);
                return index;
            }
            else
            {
                return (int)result;
            }
        }

        private void SetMenuIcon(MenuItem menuItem, string imageName)
        {
            int index = AddImageToMenuImages(PdnResources.GetImage(imageName));
            this.dotNetMenuProvider.SetDrawSpecial(menuItem, true);
            this.dotNetMenuProvider.SetImageIndex(menuItem, index);
        }

        private void SetMenuIcon(MenuItem menuItem, Image image)
        {
            int index = AddImageToMenuImages(image);
            this.dotNetMenuProvider.SetDrawSpecial(menuItem, true);
            this.dotNetMenuProvider.SetImageIndex(menuItem, index);
        }

        private void ClickOnMenuItem(MenuItem menuItem)
        {
            menuItem.PerformClick();
        }

        private delegate void VoidMenuItemDelegate(MenuItem menuItem);

        private void ClickOnMenuItemAsync(MenuItem menuItem)
        {
            this.BeginInvoke(new VoidMenuItemDelegate(ClickOnMenuItem), new object[] { menuItem });
        }

        private void ClearMenuItem(MenuItem menuItem)
        {
            menuItem.MenuItems.Clear();
        }

        private void AddToMenuItem(MenuItem addToMe, MenuItem addMe)
        {
            addToMe.MenuItems.Add(addMe);
            this.dotNetMenuProvider.SetDrawSpecial(addMe, true);
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
            this.mainMenu = new System.Windows.Forms.MainMenu();
            this.menuFile = new System.Windows.Forms.MenuItem();
            this.menuFileNew = new System.Windows.Forms.MenuItem();
            this.menuFileOpen = new System.Windows.Forms.MenuItem();
            this.menuFileOpenRecent = new System.Windows.Forms.MenuItem();
            this.menuItem16 = new System.Windows.Forms.MenuItem();
            this.menuFileAcquire = new System.Windows.Forms.MenuItem();
            this.menuFileAcquireFromClipboard = new System.Windows.Forms.MenuItem();
            this.menuFileAcquireFromScannerOrCamera = new System.Windows.Forms.MenuItem();
            this.menuItem11 = new System.Windows.Forms.MenuItem();
            this.menuFileNewWindow = new System.Windows.Forms.MenuItem();
            this.menuFileOpenInNewWindow = new System.Windows.Forms.MenuItem();
            this.menuSeparator1 = new System.Windows.Forms.MenuItem();
            this.menuFileSave = new System.Windows.Forms.MenuItem();
            this.menuFileSaveAs = new System.Windows.Forms.MenuItem();
            this.menuItem10 = new System.Windows.Forms.MenuItem();
            this.menuFilePrint = new System.Windows.Forms.MenuItem();
            this.menuSeparator2 = new System.Windows.Forms.MenuItem();
            this.menuFileExit = new System.Windows.Forms.MenuItem();
            this.menuEdit = new System.Windows.Forms.MenuItem();
            this.menuEditUndo = new System.Windows.Forms.MenuItem();
            this.menuEditRedo = new System.Windows.Forms.MenuItem();
            this.menuSeparator4 = new System.Windows.Forms.MenuItem();
            this.menuEditCut = new System.Windows.Forms.MenuItem();
            this.menuEditCopy = new System.Windows.Forms.MenuItem();
            this.menuEditPaste = new System.Windows.Forms.MenuItem();
            this.menuEditPasteInToNewLayer = new System.Windows.Forms.MenuItem();
            this.menuEditEraseSelection = new System.Windows.Forms.MenuItem();
            this.menuSeparator6 = new System.Windows.Forms.MenuItem();
            this.menuEditInvertSelection = new System.Windows.Forms.MenuItem();
            this.menuEditSelectAll = new System.Windows.Forms.MenuItem();
            this.menuEditDeselect = new System.Windows.Forms.MenuItem();
            this.menuView = new System.Windows.Forms.MenuItem();
            this.menuViewZoomIn = new System.Windows.Forms.MenuItem();
            this.menuViewZoomOut = new System.Windows.Forms.MenuItem();
            this.menuViewZoomToWindow = new System.Windows.Forms.MenuItem();
            this.menuViewZoomToSelection = new System.Windows.Forms.MenuItem();
            this.menuViewActualSize = new System.Windows.Forms.MenuItem();
            this.menuViewSeperator = new System.Windows.Forms.MenuItem();
            this.menuViewGrid = new System.Windows.Forms.MenuItem();
            this.menuViewRulers = new System.Windows.Forms.MenuItem();
            this.menuViewUnits = new System.Windows.Forms.MenuItem();
            this.menuViewUnitsPixels = new System.Windows.Forms.MenuItem();
            this.menuViewUnitsInches = new System.Windows.Forms.MenuItem();
            this.menuViewUnitsCentimeters = new System.Windows.Forms.MenuItem();
            this.menuImage = new System.Windows.Forms.MenuItem();
            this.menuImageCrop = new System.Windows.Forms.MenuItem();
            this.menuImageResize = new System.Windows.Forms.MenuItem();
            this.menuImageCanvasSize = new System.Windows.Forms.MenuItem();
            this.menuSeparator8 = new System.Windows.Forms.MenuItem();
            this.menuImageFlip = new System.Windows.Forms.MenuItem();
            this.menuImageFlipHorizontal = new System.Windows.Forms.MenuItem();
            this.menuImageFlipVertical = new System.Windows.Forms.MenuItem();
            this.menuImageRotate = new System.Windows.Forms.MenuItem();
            this.menuImageRotate90CW = new System.Windows.Forms.MenuItem();
            this.menuImageRotate180CW = new System.Windows.Forms.MenuItem();
            this.menuImageRotate270CW = new System.Windows.Forms.MenuItem();
            this.menuItem13 = new System.Windows.Forms.MenuItem();
            this.menuImageRotate90CCW = new System.Windows.Forms.MenuItem();
            this.menuImageRotate180CCW = new System.Windows.Forms.MenuItem();
            this.menuImageRotate270CCW = new System.Windows.Forms.MenuItem();
            this.menuLayers = new System.Windows.Forms.MenuItem();
            this.menuLayersAddNewLayer = new System.Windows.Forms.MenuItem();
            this.menuLayersDeleteLayer = new System.Windows.Forms.MenuItem();
            this.menuLayersDuplicateLayer = new System.Windows.Forms.MenuItem();
            this.menuLayersImportFromFile = new System.Windows.Forms.MenuItem();
            this.menuSeparator5 = new System.Windows.Forms.MenuItem();
            this.menuLayersAdjustments = new System.Windows.Forms.MenuItem();
            this.menuItem17 = new System.Windows.Forms.MenuItem();
            this.menuImageFlatten = new System.Windows.Forms.MenuItem();
            this.menuItem18 = new System.Windows.Forms.MenuItem();
            this.menuLayersFlip = new System.Windows.Forms.MenuItem();
            this.menuLayersFlipHorizontal = new System.Windows.Forms.MenuItem();
            this.menuLayersFlipVertical = new System.Windows.Forms.MenuItem();
            this.menuLayersRotateZoom = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuLayersLayerProperties = new System.Windows.Forms.MenuItem();
            this.menuEffects = new System.Windows.Forms.MenuItem();
            this.menuEffectsSentinel = new System.Windows.Forms.MenuItem();
            this.menuTools = new System.Windows.Forms.MenuItem();
            this.menuToolsAntiAliasing = new System.Windows.Forms.MenuItem();
            this.menuToolsAlphaBlending = new System.Windows.Forms.MenuItem();
            this.menuToolsSeperator = new System.Windows.Forms.MenuItem();
            this.menuWindow = new System.Windows.Forms.MenuItem();
            this.menuWindowResetWindowLocations = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuWindowTranslucent = new System.Windows.Forms.MenuItem();
            this.menuItem12 = new System.Windows.Forms.MenuItem();
            this.menuWindowTools = new System.Windows.Forms.MenuItem();
            this.menuWindowHistory = new System.Windows.Forms.MenuItem();
            this.menuWindowLayers = new System.Windows.Forms.MenuItem();
            this.menuWindowColors = new System.Windows.Forms.MenuItem();
            this.menuDebug = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.menuItem14 = new System.Windows.Forms.MenuItem();
            this.menuHelp = new System.Windows.Forms.MenuItem();
            this.menuHelpHelpTopics = new System.Windows.Forms.MenuItem();
            this.menuSeparator7 = new System.Windows.Forms.MenuItem();
            this.menuHelpAbout = new System.Windows.Forms.MenuItem();
            this.menuFileLanguage = new System.Windows.Forms.MenuItem();
            this.menuFileLanguageSentinel = new System.Windows.Forms.MenuItem();
            this.menuFileUpdates = new System.Windows.Forms.MenuItem();
            this.menuFileUpdatesCheckNow = new System.Windows.Forms.MenuItem();
            this.menuSeparator9 = new System.Windows.Forms.MenuItem();
            this.menuFileUpdatesAutoCheckEnabled = new System.Windows.Forms.MenuItem();
            this.menuFileUpdatesCheckForBetas = new System.Windows.Forms.MenuItem();
            this.menuHelpSendFeedback = new System.Windows.Forms.MenuItem();
            this.menuSeparator10 = new System.Windows.Forms.MenuItem();
            this.defaultButton = new System.Windows.Forms.Button();

            this.statusBar = new PdnStatusBar(); //new System.Windows.Forms.StatusBar();
            this.contextStatusBar = new System.Windows.Forms.StatusBarPanel();
            this.progressStatusBar = new System.Windows.Forms.StatusBarPanel();
            this.imageInfoStatusBar = new System.Windows.Forms.StatusBarPanel();
            this.cursorInfoStatusBar = new System.Windows.Forms.StatusBarPanel();
            this.workspace = new PaintDotNet.DocumentWorkspace();
            this.menuImages = new System.Windows.Forms.ImageList(this.components);
            this.floaterOpacityTimer = new System.Windows.Forms.Timer(this.components);
            this.invalidateTimer = new System.Windows.Forms.Timer(this.components);
            this.populateEffectsTimer = new System.Windows.Forms.Timer(this.components);
            this.mruImageList = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.contextStatusBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressStatusBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageInfoStatusBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cursorInfoStatusBar)).BeginInit();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
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
            // 
            // menuFile
            // 
            this.menuFile.Index = 0;
            this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
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
            this.menuFileNew.Index = 0;
            this.menuFileNew.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.menuFileNew.Click += new System.EventHandler(this.menuFileNew_Click);
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.Index = 1;
            this.menuFileOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
            // 
            // menuFileOpenRecent
            // 
            this.menuFileOpenRecent.Index = 2;
            this.menuFileOpenRecent.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                               this.menuItem16});
            this.menuFileOpenRecent.Popup += new System.EventHandler(this.menuFileOpenRecent_Popup);
            // 
            // menuItem16
            // 
            this.menuItem16.Index = 0;
            this.menuItem16.Text = "sentinel";
            // 
            // menuFileAcquire
            // 
            this.menuFileAcquire.Index = 3;
            this.menuFileAcquire.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                            this.menuFileAcquireFromClipboard,
                                                                                            this.menuFileAcquireFromScannerOrCamera});
            this.menuFileAcquire.Popup += new System.EventHandler(this.menuFileAcquire_Popup);
            // 
            // menuFileAcquireFromClipboard
            // 
            this.menuFileAcquireFromClipboard.Index = 0;
            this.menuFileAcquireFromClipboard.Click += new System.EventHandler(this.menuFileAcquireFromClipboard_Click);
            // 
            // menuFileAcquireFromScannerOrCamera
            // 
            this.menuFileAcquireFromScannerOrCamera.Index = 1;
            this.menuFileAcquireFromScannerOrCamera.Click += new System.EventHandler(this.menuFileAcquireFromScannerOrCamera_Click);
            // 
            // menuItem11
            // 
            this.menuItem11.Index = 4;
            this.menuItem11.Text = "-";
            // 
            // menuFileNewWindow
            // 
            this.menuFileNewWindow.Index = 5;
            this.menuFileNewWindow.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftW;
            this.menuFileNewWindow.Click += new System.EventHandler(this.menuFileNewWindow_Click);
            // 
            // menuFileOpenInNewWindow
            // 
            this.menuFileOpenInNewWindow.Index = 6;
            this.menuFileOpenInNewWindow.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftO;
            this.menuFileOpenInNewWindow.Click += new System.EventHandler(this.menuFileOpenInNewWindow_Click);
            // 
            // menuSeparator1
            // 
            this.menuSeparator1.Index = 7;
            this.menuSeparator1.Text = "-";
            // 
            // menuFileSave
            // 
            this.menuFileSave.Index = 8;
            this.menuFileSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.menuFileSave.Click += new System.EventHandler(this.menuFileSave_Click);
            // 
            // menuFileSaveAs
            // 
            this.menuFileSaveAs.Index = 9;
            this.menuFileSaveAs.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftS;
            this.menuFileSaveAs.Click += new System.EventHandler(this.menuFileSaveAs_Click);
            // 
            // menuItem10
            // 
            this.menuItem10.Index = 10;
            this.menuItem10.Text = "-";
            // 
            // menuFilePrint
            // 
            this.menuFilePrint.Index = 11;
            this.menuFilePrint.Shortcut = System.Windows.Forms.Shortcut.CtrlP;
            this.menuFilePrint.Click += new System.EventHandler(this.menuFilePrint_Click);
            // 
            // menuSeparator2
            // 
            this.menuSeparator2.Index = 12;
            this.menuSeparator2.Text = "-";
            //
            // menuFileLanguage
            //
            this.menuFileLanguage.Index = 13;
            this.menuFileLanguage.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                             this.menuFileLanguageSentinel});
            this.menuFileLanguage.Popup += new EventHandler(menuFileLanguage_Popup);
            // 
            // menuFileLanguageSentinel
            //
            this.menuFileLanguageSentinel.Index = 0;
            this.menuFileLanguageSentinel.Text = "(sentinel)";
            //
            // menuFileUpdates
            //
            this.menuFileUpdates.Index = 14;
            this.menuFileUpdates.Popup += new EventHandler(menuFileUpdates_Popup);
            this.menuFileUpdates.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                            this.menuFileUpdatesCheckNow,
                                                                                            this.menuSeparator9,
                                                                                            this.menuFileUpdatesAutoCheckEnabled,
                                                                                            this.menuFileUpdatesCheckForBetas});
            //
            // menuFileUpdatesCheckNow
            //
            this.menuFileUpdatesCheckNow.Index = 0;
            this.menuFileUpdatesCheckNow.Click += new EventHandler(menuFileUpdatesCheckNow_Click);
            //
            // menuSeparator9
            //
            this.menuSeparator9.Index = 1;
            this.menuSeparator9.Text = "-";
            //
            // menuFileUpdatesAutoCheckEnabled
            //
            this.menuFileUpdatesAutoCheckEnabled.Index = 2;
            this.menuFileUpdatesAutoCheckEnabled.Click += new EventHandler(menuFileUpdatesAutoCheckEnabled_Click);
            //
            // menuFileUpdatesCheckForBetas
            //
            this.menuFileUpdatesCheckForBetas.Index = 3;
            this.menuFileUpdatesCheckForBetas.Click += new EventHandler(menuFileUpdatesCheckForBetas_Click);
            //
            // menuSeparator10
            //
            this.menuSeparator10.Index = 15;
            this.menuSeparator10.Text = "-";
            // 
            // menuFileExit
            // 
            this.menuFileExit.Index = 16;
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuEdit
            // 
            this.menuEdit.Index = 1;
            this.menuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
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
            this.menuEdit.Popup += new System.EventHandler(this.menuEdit_Popup);
            // 
            // menuEditUndo
            // 
            this.menuEditUndo.Index = 0;
            this.menuEditUndo.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
            this.menuEditUndo.Click += new System.EventHandler(this.menuEditUndo_Click);
            // 
            // menuEditRedo
            // 
            this.menuEditRedo.Index = 1;
            this.menuEditRedo.Shortcut = System.Windows.Forms.Shortcut.CtrlY;
            this.menuEditRedo.Click += new System.EventHandler(this.menuEditRedo_Click);
            // 
            // menuSeparator4
            // 
            this.menuSeparator4.Index = 2;
            this.menuSeparator4.Text = "-";
            // 
            // menuEditCut
            // 
            this.menuEditCut.Index = 3;
            this.menuEditCut.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
            this.menuEditCut.Click += new System.EventHandler(this.menuEditCut_Click);
            // 
            // menuEditCopy
            // 
            this.menuEditCopy.Index = 4;
            this.menuEditCopy.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.menuEditCopy.Click += new System.EventHandler(this.menuEditCopy_Click);
            // 
            // menuEditPaste
            // 
            this.menuEditPaste.Index = 5;
            this.menuEditPaste.Shortcut = System.Windows.Forms.Shortcut.CtrlV;
            this.menuEditPaste.Click += new System.EventHandler(this.menuEditPaste_Click);
            // 
            // menuEditPasteInToNewLayer
            // 
            this.menuEditPasteInToNewLayer.Index = 6;
            this.menuEditPasteInToNewLayer.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftV;
            this.menuEditPasteInToNewLayer.Click += new System.EventHandler(this.menuEditPasteInToNewLayer_Click);
            // 
            // menuEditEraseSelection
            // 
            this.menuEditEraseSelection.Index = 7;
            this.menuEditEraseSelection.Shortcut = System.Windows.Forms.Shortcut.Del;
            this.menuEditEraseSelection.Click += new System.EventHandler(this.menuEditClearSelection_Click);
            // 
            // menuSeparator6
            // 
            this.menuSeparator6.Index = 8;
            this.menuSeparator6.Text = "-";
            // 
            // menuEditInvertSelection
            // 
            this.menuEditInvertSelection.Index = 9;
            this.menuEditInvertSelection.Click += new System.EventHandler(this.menuEditInvertSelection_Click);
            this.menuEditInvertSelection.Shortcut = System.Windows.Forms.Shortcut.CtrlI;
            // 
            // menuEditSelectAll
            // 
            this.menuEditSelectAll.Index = 10;
            this.menuEditSelectAll.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
            this.menuEditSelectAll.Click += new System.EventHandler(this.menuEditSelectAll_Click);
            // 
            // menuEditDeselect
            // 
            this.menuEditDeselect.Index = 11;
            this.menuEditDeselect.Shortcut = System.Windows.Forms.Shortcut.CtrlD;
            this.menuEditDeselect.Click += new System.EventHandler(this.menuEditDeselect_Click);
            // 
            // menuView
            // 
            this.menuView.Index = 2;
            this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                     this.menuViewZoomIn,
                                                                                     this.menuViewZoomOut,
                                                                                     this.menuViewZoomToWindow,
                                                                                     this.menuViewZoomToSelection,
                                                                                     this.menuViewActualSize,
                                                                                     this.menuViewSeperator,
                                                                                     this.menuViewGrid,
                                                                                     this.menuViewRulers,
                                                                                     this.menuViewUnits});
            this.menuView.Popup += new System.EventHandler(this.menuView_Popup);
            // 
            // menuViewZoomIn
            // 
            this.menuViewZoomIn.Index = 0;
            this.menuViewZoomIn.Shortcut = System.Windows.Forms.Shortcut.CtrlK;
            this.menuViewZoomIn.Click += new System.EventHandler(this.menuViewZoomIn_Click);
            // 
            // menuViewZoomOut
            // 
            this.menuViewZoomOut.Index = 1;
            this.menuViewZoomOut.Shortcut = System.Windows.Forms.Shortcut.CtrlJ;
            this.menuViewZoomOut.Click += new System.EventHandler(this.menuViewZoomOut_Click);
            // 
            // menuViewZoomToWindow
            // 
            this.menuViewZoomToWindow.Index = 2;
            this.menuViewZoomToWindow.Shortcut = System.Windows.Forms.Shortcut.CtrlB;
            this.menuViewZoomToWindow.Click += new System.EventHandler(this.menuViewZoomToWindow_Click);
            // 
            // menuViewZoomToSelection
            // 
            this.menuViewZoomToSelection.Index = 3;
            this.menuViewZoomToSelection.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftB;
            this.menuViewZoomToSelection.Click += new System.EventHandler(this.menuViewZoomToSelection_Click);
            // 
            // menuViewActualSize
            // 
            this.menuViewActualSize.Index = 4;
            this.menuViewActualSize.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftA;
            this.menuViewActualSize.Click += new System.EventHandler(this.menuViewActualSize_Click);
            // 
            // menuViewSeperator
            // 
            this.menuViewSeperator.Index = 5;
            this.menuViewSeperator.Text = "-";
            // 
            // menuViewGrid
            // 
            this.menuViewGrid.Index = 6;
            this.menuViewGrid.Click += new System.EventHandler(this.menuViewGrid_Click);
            // 
            // menuViewRulers
            // 
            this.menuViewRulers.Index = 7;
            this.menuViewRulers.Click += new System.EventHandler(this.menuViewRulers_Click);
            //
            // menuViewUnits
            //
            this.menuViewUnits.Index = 8;
            this.menuViewUnits.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                          this.menuViewUnitsPixels,
                                                                                          this.menuViewUnitsInches,
                                                                                          this.menuViewUnitsCentimeters});
            this.menuViewUnits.Popup += new EventHandler(menuViewUnits_Popup);
            //
            // menuViewUnitsPixels
            //
            this.menuViewUnitsPixels.Index = 0;
            this.menuViewUnitsPixels.Click += new EventHandler(menuViewUnitsPixels_Click);
            //
            // menuViewUnitsInches
            //
            this.menuViewUnitsInches.Index = 1;
            this.menuViewUnitsInches.Click += new EventHandler(menuViewUnitsInches_Click);
            //
            // menuViewUnitsCentimeters
            //
            this.menuViewUnitsCentimeters.Index = 2;
            this.menuViewUnitsCentimeters.Click += new EventHandler(menuViewUnitsCentimeters_Click);
            // 
            // menuImage
            // 
            this.menuImage.Index = 3;
            this.menuImage.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuImageCrop,
                                                                                      this.menuImageResize,
                                                                                      this.menuImageCanvasSize,
                                                                                      this.menuSeparator8,
                                                                                      this.menuImageFlip,
                                                                                      this.menuImageRotate,
                                                                                      this.menuItem18,
                                                                                      this.menuImageFlatten });
            this.menuImage.Popup += new System.EventHandler(this.menuImage_Popup);
            // 
            // menuImageCrop
            // 
            this.menuImageCrop.Index = 0;
            this.menuImageCrop.Click += new System.EventHandler(this.menuImageCrop_Click);
            // 
            // menuImageResize
            // 
            this.menuImageResize.Index = 1;
            this.menuImageResize.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
            this.menuImageResize.Click += new System.EventHandler(this.menuImageResize_Click);
            // 
            // menuImageCanvasSize
            // 
            this.menuImageCanvasSize.Index = 2;
            this.menuImageCanvasSize.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftR;
            this.menuImageCanvasSize.Click += new System.EventHandler(this.menuImageCanvasSize_Click);
            // 
            // menuSeparator8
            // 
            this.menuSeparator8.Index = 3;
            this.menuSeparator8.Text = "-";
            // 
            // menuImageFlip
            // 
            this.menuImageFlip.Index = 4;
            this.menuImageFlip.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                          this.menuImageFlipHorizontal,
                                                                                          this.menuImageFlipVertical});
            // 
            // menuImageFlipHorizontal
            // 
            this.menuImageFlipHorizontal.Index = 0;
            this.menuImageFlipHorizontal.Click += new System.EventHandler(this.menuImageFlipHorizontal_Click);
            // 
            // menuImageFlipVertical
            // 
            this.menuImageFlipVertical.Index = 1;
            this.menuImageFlipVertical.Click += new System.EventHandler(this.menuImageFlipVertical_Click);
            // 
            // menuImageRotate
            // 
            this.menuImageRotate.Index = 5;
            this.menuImageRotate.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
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
            this.menuImageRotate90CW.Index = 0;
            this.menuImageRotate90CW.Shortcut = System.Windows.Forms.Shortcut.CtrlH;
            this.menuImageRotate90CW.Click += new System.EventHandler(this.menuImageRotate90CW_Click);
            // 
            // menuImageRotate180CW
            // 
            this.menuImageRotate180CW.Index = 1;
            this.menuImageRotate180CW.Click += new System.EventHandler(this.menuImageRotate180CW_Click);
            // 
            // menuImageRotate270CW
            // 
            this.menuImageRotate270CW.Index = 2;
            this.menuImageRotate270CW.Click += new System.EventHandler(this.menuImageRotate270CW_Click);
            // 
            // menuItem13
            // 
            this.menuItem13.Index = 3;
            this.menuItem13.Text = "-";
            // 
            // menuImageRotate90CCW
            // 
            this.menuImageRotate90CCW.Index = 4;
            this.menuImageRotate90CCW.Shortcut = System.Windows.Forms.Shortcut.CtrlG;
            this.menuImageRotate90CCW.Click += new System.EventHandler(this.menuImageRotate90CCW_Click);
            // 
            // menuImageRotate180CCW
            // 
            this.menuImageRotate180CCW.Index = 5;
            this.menuImageRotate180CCW.Click += new System.EventHandler(this.menuImageRotate180CCW_Click);
            // 
            // menuImageRotate270CCW
            // 
            this.menuImageRotate270CCW.Index = 6;
            this.menuImageRotate270CCW.Click += new System.EventHandler(this.menuImageRotate270CCW_Click);
            //
            // menuItem18
            //
            this.menuItem18.Index = 6;
            this.menuItem18.Text = "-";
            // 
            // menuImageFlatten
            // 
            this.menuImageFlatten.Index = 7;
            this.menuImageFlatten.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftF;
            this.menuImageFlatten.Click += new System.EventHandler(this.menuImageFlatten_Click);
            // 
            // menuLayers
            // 
            this.menuLayers.Index = 4;
            this.menuLayers.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
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
            this.menuLayers.Popup += new System.EventHandler(this.menuLayers_Popup);
            // 
            // menuLayersAddNewLayer
            // 
            this.menuLayersAddNewLayer.Index = 0;
            this.menuLayersAddNewLayer.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftN;
            this.menuLayersAddNewLayer.Click += new System.EventHandler(this.menuLayersAddNewLayer_Click);
            // 
            // menuLayersDeleteLayer
            // 
            this.menuLayersDeleteLayer.Index = 1;
            this.menuLayersDeleteLayer.Shortcut = System.Windows.Forms.Shortcut.ShiftDel;
            this.menuLayersDeleteLayer.Click += new System.EventHandler(this.menuLayersDeleteLayer_Click);
            // 
            // menuLayersDuplicateLayer
            // 
            this.menuLayersDuplicateLayer.Index = 2;
            this.menuLayersDuplicateLayer.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftD;
            this.menuLayersDuplicateLayer.Click += new System.EventHandler(this.menuLayersDuplicateLayer_Click);
            // 
            // menuLayersImportFromFile
            // 
            this.menuLayersImportFromFile.Index = 3;
            this.menuLayersImportFromFile.Click += new System.EventHandler(this.menuLayersImportFromFile_click);
            // 
            // menuSeparator5
            // 
            this.menuSeparator5.Index = 4;
            this.menuSeparator5.Text = "-";
            // 
            // menuLayersAdjustments
            // 
            this.menuLayersAdjustments.Index = 5;
            this.menuLayersAdjustments.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                                  this.menuItem17});
            this.menuLayersAdjustments.Popup += new System.EventHandler(this.menuLayersAdjustments_Popup);
            // 
            // menuItem17
            // 
            this.menuItem17.Index = 0;
            this.menuItem17.Text = "(sentinel)";
            // 
            // menuLayersFlip
            // 
            this.menuLayersFlip.Index = 6;
            this.menuLayersFlip.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                           this.menuLayersFlipHorizontal,
                                                                                           this.menuLayersFlipVertical});
            // 
            // menuLayersFlipHorizontal
            // 
            this.menuLayersFlipHorizontal.Index = 0;
            this.menuLayersFlipHorizontal.Click += new System.EventHandler(this.menuLayersFlipHorizontal_Click);
            // 
            // menuLayersFlipVertical
            // 
            this.menuLayersFlipVertical.Index = 1;
            this.menuLayersFlipVertical.Click += new System.EventHandler(this.menuLayersFlipVertical_Click);
            //
            // menuLayersRotateZoom
            //
            this.menuLayersRotateZoom.Index = 7;
            this.menuLayersRotateZoom.Click += new EventHandler(menuLayersRotateZoom_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 8;
            this.menuItem9.Text = "-";
            // 
            // menuLayersLayerProperties
            // 
            this.menuLayersLayerProperties.Index = 9;
            this.menuLayersLayerProperties.Shortcut = System.Windows.Forms.Shortcut.F4;
            this.menuLayersLayerProperties.Click += new System.EventHandler(this.menuLayersLayerProperties_Click);
            // 
            // menuEffects
            // 
            this.menuEffects.Index = 5;
            this.menuEffects.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                        this.menuEffectsSentinel});
            this.menuEffects.Popup += new System.EventHandler(this.menuEffects_Popup);
            // 
            // menuEffectsSentinel
            // 
            this.menuEffectsSentinel.Index = 0;
            this.menuEffectsSentinel.Text = "sentinel";
            // 
            // menuTools
            // 
            this.menuTools.Index = 6;
            this.menuTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuToolsAntiAliasing,
                                                                                      this.menuToolsAlphaBlending,
                                                                                      this.menuToolsSeperator});
            this.menuTools.Popup += new System.EventHandler(this.menuTools_Popup);
            // 
            // menuToolsAntiAliasing
            // 
            this.menuToolsAntiAliasing.Index = 0;
            this.menuToolsAntiAliasing.Click += new System.EventHandler(this.menuToolsAntiAliasing_Click);
            //
            // menuToolsAlphaBlending
            //
            this.menuToolsAlphaBlending.Index = 1;
            this.menuToolsAlphaBlending.Click += new EventHandler(menuToolsAlphaBlending_Click);
            // 
            // menuToolsSeperator
            // 
            this.menuToolsSeperator.Index = 2;
            this.menuToolsSeperator.Text = "-";
            // 
            // menuWindow
            // 
            this.menuWindow.Index = 7;
            this.menuWindow.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                       this.menuWindowResetWindowLocations,
                                                                                       this.menuItem7,
                                                                                       this.menuWindowTranslucent,
                                                                                       this.menuItem12,
                                                                                       this.menuWindowTools,
                                                                                       this.menuWindowHistory,
                                                                                       this.menuWindowLayers,
                                                                                       this.menuWindowColors});
            this.menuWindow.Popup += new System.EventHandler(this.menuWindow_Popup);
            // 
            // menuWindowResetWindowLocations
            // 
            this.menuWindowResetWindowLocations.Index = 0;
            this.menuWindowResetWindowLocations.Click += new System.EventHandler(this.menuWindowResetWindowLocations_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 1;
            this.menuItem7.Text = "-";
            // 
            // menuWindowTranslucent
            // 
            this.menuWindowTranslucent.Index = 2;
            this.menuWindowTranslucent.Click += new System.EventHandler(this.menuWindowTranslucent_Click);
            // 
            // menuItem12
            // 
            this.menuItem12.Index = 3;
            this.menuItem12.Text = "-";
            // 
            // menuWindowTools
            // 
            this.menuWindowTools.Index = 4;
            this.menuWindowTools.Shortcut = System.Windows.Forms.Shortcut.F5;
            this.menuWindowTools.Click += new System.EventHandler(this.menuWindowTools_Click);
            // 
            // menuWindowHistory
            // 
            this.menuWindowHistory.Index = 5;
            this.menuWindowHistory.Shortcut = System.Windows.Forms.Shortcut.F6;
            this.menuWindowHistory.Click += new System.EventHandler(this.menuWindowHistory_Click);
            // 
            // menuWindowLayers
            // 
            this.menuWindowLayers.Index = 6;
            this.menuWindowLayers.Shortcut = System.Windows.Forms.Shortcut.F7;
            this.menuWindowLayers.Click += new System.EventHandler(this.menuWindowLayers_Click);
            // 
            // menuWindowColors
            // 
            this.menuWindowColors.Index = 7;
            this.menuWindowColors.Shortcut = System.Windows.Forms.Shortcut.F8;
            this.menuWindowColors.Click += new System.EventHandler(this.menuWindowColors_Click);
            // 
            // menuDebug
            // 
            this.menuDebug.Index = 8;
            this.menuDebug.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem1,
                                                                                      this.menuItem4,
                                                                                      this.menuItem5,
                                                                                      this.menuItem6,
                                                                                      this.menuItem8});
            this.menuDebug.Text = "Debug";
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "Invalidate Document";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 1;
            this.menuItem4.Text = "GC.Collect";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 2;
            this.menuItem5.Text = "Breakpoint";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 3;
            this.menuItem6.Text = "Resposition floaters";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 4;
            this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem14});
            this.menuItem8.Text = "Stress";
            // 
            // menuItem14
            // 
            this.menuItem14.Index = 0;
            this.menuItem14.Text = "Open All Files On C:, D:";
            this.menuItem14.Click += new System.EventHandler(this.menuItem14_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.Index = 9;
            this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                     this.menuHelpHelpTopics,
                                                                                     this.menuHelpSendFeedback,
                                                                                     this.menuSeparator7,
                                                                                     this.menuHelpAbout});
            // 
            // menuHelpHelpTopics
            // 
            this.menuHelpHelpTopics.Index = 0;
            this.menuHelpHelpTopics.Shortcut = System.Windows.Forms.Shortcut.F1;
            this.menuHelpHelpTopics.Click += new System.EventHandler(this.menuHelpHelpTopics_Click);
            //
            // menuHelpSendFeedback
            //
            this.menuHelpSendFeedback.Index = 1;
            this.menuHelpSendFeedback.Click += new EventHandler(menuHelpSendFeedback_Click);
            // 
            // menuSeparator7
            // 
            this.menuSeparator7.Index = 2;
            this.menuSeparator7.Text = "-";
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Index = 3;
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            // 
            // statusBar
            // 
            this.statusBar.Location = new System.Drawing.Point(0, 648);
            this.statusBar.Name = "statusBar";
            this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
                                                                                         this.contextStatusBar,
                                                                                         this.progressStatusBar,
                                                                                         this.imageInfoStatusBar,
                                                                                         this.cursorInfoStatusBar});
            this.statusBar.ShowPanels = true;
            this.statusBar.Size = new System.Drawing.Size(752, 22);
            this.statusBar.TabIndex = 1;
            this.statusBar.Text = "Status Bar";
            // 
            // contextStatusBar
            // 
            this.contextStatusBar.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.contextStatusBar.Width = 436;
            //
            // imageInfoStatusBar
            //
            this.imageInfoStatusBar.Width = 130;
            //
            // cursorInfoStatusBar
            //
            this.cursorInfoStatusBar.Width = 130;
            // 
            // workspace
            // 
            this.workspace.ActiveLayer = null;
            this.workspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workspace.Location = new System.Drawing.Point(0, 0);
            this.workspace.Name = "workspace";
            this.workspace.Size = new System.Drawing.Size(752, 648);
            this.workspace.TabIndex = 2;
            this.workspace.Scroll += new System.EventHandler(this.workspace_Scroll);
            this.workspace.DocumentChanged += new System.EventHandler(this.workspace_DocumentChanged);
            this.workspace.ToolStatusChanged += new EventHandler(workspace_ToolStatusChanged);
            // 
            // menuImages
            // 
            this.menuImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.menuImages.ImageSize = new System.Drawing.Size(16, 16);
            this.menuImages.TransparentColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(192)), ((System.Byte)(192)));
            // 
            // floaterOpacityTimer
            // 
            this.floaterOpacityTimer.Enabled = false;
            this.floaterOpacityTimer.Interval = 25;
            this.floaterOpacityTimer.Tick += new System.EventHandler(this.floaterOpacityTimer_Tick);
            // 
            // invalidateTimer
            // 
            this.invalidateTimer.Interval = 25;
            this.invalidateTimer.Tick += new System.EventHandler(this.invalidateTimer_Tick);
            //
            // populateEffectsTimer
            //
            this.populateEffectsTimer.Interval = 250;
            this.populateEffectsTimer.Tick += new EventHandler(populateEffectsTimer_Tick);
            // 
            // mruImageList
            // 
            this.mruImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.mruImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.mruImageList.TransparentColor = System.Drawing.Color.Transparent;
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
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(950, 738);
            this.Controls.Add(this.workspace);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.defaultButton);
            this.AcceptButton = this.defaultButton;
            this.Menu = this.mainMenu;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
            this.BackColor = SystemColors.ControlDark;
            this.ForceActiveTitleBar = true;
            this.KeyPreview = true;
            this.Controls.SetChildIndex(this.statusBar, 0);
            this.Controls.SetChildIndex(this.workspace, 0);
            ((System.ComponentModel.ISupportInitialize)(this.contextStatusBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressStatusBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageInfoStatusBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cursorInfoStatusBar)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

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

            workspace.Widgets.CommonActionsWidget.ButtonClick += new EnumValueEventHandler(CommonActionsWidget_ButtonClick);
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

            if (SystemLayer.Security.IsAdministrator)
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

            base.OnLoad (e);
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
            if (!this.workspace.Widgets.CommonActionsWidget.GetButtonEnabled(CommonAction.CheckForUpdates))
            {
                this.workspace.Widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.CheckForUpdates, true);
                this.workspace.Widgets.CommonActionsWidget.SetButtonVisible(CommonAction.CheckForUpdates, true);
                this.workspace.Widgets.CommonActionsWidget.Width += 32;
                this.workspace.Widgets.CommonActionsWidget.BlinkButton(CommonAction.CheckForUpdates, 5, 350);
            }
        }

        private void DisableUpdatesButton()
        {
            if (this.workspace.Widgets.CommonActionsWidget.GetButtonEnabled(CommonAction.CheckForUpdates))
            {
                this.workspace.Widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.CheckForUpdates, false);
                this.workspace.Widgets.CommonActionsWidget.SetButtonVisible(CommonAction.CheckForUpdates, false);
                this.workspace.Widgets.CommonActionsWidget.Width -= 32;
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
                DownloadAndUnzipUpdateDialog dialog = new DownloadAndUnzipUpdateDialog(this, manifest.VersionInfos[versionIndex]);
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

            workspace.Focus();
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
            foreach (MenuItem mi in mainMenu.MenuItems)
            {
                mi.Popup += new EventHandler(MenuPopupFirstTimeHandler);
            }
        }

        private void UnregisterMenuPopupFirstTimeDelegates()
        {
            foreach (MenuItem mi in mainMenu.MenuItems)
            {
                mi.Popup -= new EventHandler(MenuPopupFirstTimeHandler);
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
            string appDir = Path.GetDirectoryName(Application.ExecutablePath);
            string rzPath = Path.Combine(appDir, "Effects/RotateZoom.dll");
            Assembly rzAssembly = Assembly.LoadFrom(rzPath);
            Type rzType = rzAssembly.GetType("PaintDotNet.Effects.RotateZoom.RotateZoomEffect", true);
            PropertyInfo rzNamePI = rzType.GetProperty("StaticName", BindingFlags.Public | BindingFlags.Static);
            string rzName = (string)rzNamePI.GetValue(null, null);
            PropertyInfo rzShortcutPI = rzType.GetProperty("StaticShortcut", BindingFlags.Public | BindingFlags.Static);
            Shortcut rzShortcut = (Shortcut)rzShortcutPI.GetValue(null, null);
            PropertyInfo rzImagePI = rzType.GetProperty("StaticImage", BindingFlags.Public | BindingFlags.Static);
            Image rzImage = (Image)rzImagePI.GetValue(null, null);
            string rzNameFormatString = PdnResources.GetString("MainForm.Effects.Name.Format.Configurable");
            string rzMenuName = string.Format(rzNameFormatString, rzName);
            this.menuLayersRotateZoom.Text = rzMenuName;
            this.SetMenuIcon(this.menuLayersRotateZoom, rzImage);
            this.menuLayersRotateZoom.Shortcut = rzShortcut;

            this.menuLayersLayerProperties.Text = PdnResources.GetString("MainForm.Menu.Layers.LayerProperties.Text");

            this.menuToolsAntiAliasing.Text = PdnResources.GetString("MainForm.Menu.Tools.AntiAliasing.Text");
            this.menuToolsAlphaBlending.Text = PdnResources.GetString("MainForm.Menu.Tools.AlphaBlending.Text");

            this.menuWindowResetWindowLocations.Text = PdnResources.GetString("MainForm.Menu.Window.ResetWindowLocations.Text");
            this.menuWindowTranslucent.Text = PdnResources.GetString("MainForm.Menu.Window.Translucent.Text");
            this.menuWindowTools.Text = PdnResources.GetString("MainForm.Menu.Window.Tools.Text");
            this.menuWindowHistory.Text = PdnResources.GetString("MainForm.Menu.Window.History.Text");
            this.menuWindowLayers.Text = PdnResources.GetString("MainForm.Menu.Window.Layers.Text");
            this.menuWindowColors.Text = PdnResources.GetString("MainForm.Menu.Window.Colors.Text");

            this.menuHelpHelpTopics.Text = PdnResources.GetString("MainForm.Menu.Help.HelpTopics.Text");
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
                if (fi.FieldType == typeof(MenuItem))
                {
                    string iconFileName = "Icons." + fi.Name[0].ToString().ToUpper() + fi.Name.Substring(1) + "Icon.bmp";
                    MenuItem mi = (MenuItem)fi.GetValue(this);
                    Stream iconStream = PdnResources.GetResourceStream(iconFileName);

                    if (iconStream != null)
                    {
                        Image iconImage = Image.FromStream(iconStream);
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

            string tempName;
            ScanResult result = ScanningAndPrinting.Scan(this, out tempName);

            if (tempName != null)
            {
                using (new WaitCursorChanger(this))
                {
                    using (Image image = Image.FromFile(tempName))
                    {
                        workspace.SetDocument(Document.FromImage(image));
                        workspace.SetDocumentSaveOptions(null, null, null);
                        //workspace.DocumentView.ScaleFactor = new ScaleFactor(1, 1);
                    }
                }

                workspace.History.ClearAll();
                workspace.History.PushNewAction(new NullHistoryAction(PdnResources.GetString("AcquireImageAction.Name"), this.AddNewLayerIcon));

                // Try to delete the temp file but don't worry if we can't
                try
                {
                    File.Delete(tempName);
                }

                catch
                {
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

                this.imageInfoStatusBar.Text = string.Format(
                    CultureInfo.InvariantCulture, 
                    this.imageInfoStatusBarFormat, 
                    widthString, 
                    units, 
                    heightString, 
                    units);
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
            workspace.Widgets.HistoryForm.PerformUndoClick();
        }

        private void menuEditRedo_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.HistoryForm.PerformRedoClick();
        }

        private void menuFileNew_Click(object sender, System.EventArgs e)
        {
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

            using (NewFileDialog nfd = new NewFileDialog())
            {
                Size newDocSize = GetNewDocumentSize();
                
                if (IsClipboardImageAvailable())
                {
                    try
                    {
                        Utility.GCFullCollect();
                        IDataObject clipData = Clipboard.GetDataObject();

                        using (Image clipImage = (Image)clipData.GetData(DataFormats.Bitmap))
                        {
                            newDocSize.Width = clipImage.Width;
                            newDocSize.Height = clipImage.Height;
                        }
                    }

                    catch (OutOfMemoryException)
                    {
                    }

                    catch (ExternalException)
                    {
                    }
                }

                nfd.OriginalSize = new Size(newDocSize.Width, newDocSize.Height);
                nfd.OriginalDpuUnit = PdnSettings.GetLastNonPixelUnits();
                nfd.OriginalDpu = Document.GetDefaultDpu(nfd.OriginalDpuUnit);
                nfd.Units = nfd.OriginalDpuUnit;
                nfd.Resolution = nfd.OriginalDpu;
                nfd.ConstrainToAspect = Settings.CurrentUser.GetBoolean(PdnSettings.LastMaintainAspectRatioNF, false);

                if (Utility.ShowDialog(nfd, this) == DialogResult.OK)
                {
                    CreateBlankDocument(new Size(nfd.ImageWidth, nfd.ImageHeight), nfd.Units, nfd.Resolution);
                    Settings.CurrentUser.SetBoolean(PdnSettings.LastMaintainAspectRatioNF, nfd.ConstrainToAspect);

                    if (nfd.Units != MeasurementUnit.Pixel)
                    {
                        Settings.CurrentUser.SetString(PdnSettings.LastNonPixelUnits, nfd.Units.ToString());
                    }

                    if (workspace.Environment.Units != MeasurementUnit.Pixel)
                    {
                        workspace.Environment.Units = nfd.Units;
                    }
                }
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
                workspace.DocumentView.SuspendRefresh();

                try
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
                }

                finally
                {                
                    workspace.DocumentView.ResumeRefresh();
                }

                Invalidate(true);

                // add to MRU list
                AddMru(fileName);

                // warn about version?
                // 2.1 Build 1897 signifies when the file format changed and broke backwards compatibility (for saving)
                // 2.1 Build 1921 signifies when MemoryBlock was upgraded to support 64-bits, which broke it again
                // 2.1 Build 1924 upgraded to "unimportant ordering" for MemoryBlock serialization so we can to faster multiproc saves
                //                (in v2.5 we always save in order, although that doesn't change the file format's laxness)
                // 2.5 Build 2105 signifies when we introduced strong naming, and changed the way PropertyItems are serialized
                if (workspace.Document.SavedWithVersion < new Version(2, 5, 2105))
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

                    Utility.InfoBox(this, text);
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
                        //if (fileType.SavesWithProgress)
                        {
                            SaveProgressDialog sd = new SaveProgressDialog(this);
                            sd.Save(stream, workspace.Document, fileType, saveConfigToken);
                        }
                        /*
                        else
                        {
                            fileType.Save(workspace.Document, stream, saveConfigToken, null);
                        }
                        */

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

                if (!success)
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

        private void menuEdit_Popup(object sender, System.EventArgs e)
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

                    Surface copySurface = new Surface((int)selectionBounds.Width, (int)selectionBounds.Height);
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

                    selectionRegion.Dispose();
                    selectionOutline.Dispose();
                    renderArgs.Dispose();
                    maskedSurface.Dispose();
                    copySurface.Dispose();
                    copyBitmap.Dispose();
                    copyOpaqueBitmap.Dispose();                
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
            workspace.EnableOutlineAnimation = true;
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
                        aed.Icon = Utility.ImageToIcon(effect.Image, Color.FromArgb(192, 192, 192));
                    }
                    else
                    {
                        aed.Icon = this.StopWatchIcon;
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

                    if (effect is IConfigurableEffect)
                    {
                        string configurableFormat = PdnResources.GetString("MainForm.Effects.Name.Format.Configurable");
                        name = string.Format(configurableFormat, name);
                    }
                
                    if (repeatName == ((MenuItem)sender).Text)
                    {
                        Surface copy = workspace.ScratchSurface;

                        using (new WaitCursorChanger(this))
                        {
                            copy.CopySurface(((BitmapLayer)workspace.ActiveLayer).Surface);
                        }

                        ((Effect)lastEffect).EnvironmentParameters = eep;
                        DoEffect((Effect)lastEffect, (EffectConfigToken)lastEffectToken, selectedRegion, selectedRegion, copy);
                    }
                    else if (name == ((MenuItem)sender).Text)
                    {
                        EffectConfigToken newLastToken = null;
                        effect.EnvironmentParameters = eep;

                        if (!(effect is IConfigurableEffect))
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
                            previewRegion.Intersect(Rectangle.Inflate(workspace.VisibleDocumentRectangle, 1, 1));

                            Surface originalSurface = workspace.ScratchSurface;

                            using (new WaitCursorChanger(this))
                            {
                                originalSurface.CopySurface(((BitmapLayer)workspace.ActiveLayer).Surface);
                            }
                            
                            //
                            workspace.Widgets.LayerControl.SuspendLayerPreviewUpdates();
                            //

                            IConfigurableEffect config = (IConfigurableEffect)effect;
                            using (EffectConfigDialog configDialog = config.CreateConfigDialog())
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
            foreach (MenuItem mi in menuEffects.MenuItems)
            {
                mi.Click -= menuEffectsClickDelegate;
            }

            menuEffects.MenuItems.Clear();

            // If we have a repeatable effect, add it with "Repeat ___ (Ctrl+F)" along with a separator
            if (this.lastEffect != null)
            {
                string repeatFormat = PdnResources.GetString("MainForm.Effects.RepeatMenuItem.Format");
                string menuName = string.Format(repeatFormat, ((Effect)lastEffect).Name);
                MenuItem mi = new MenuItem(menuName, menuEffectsClickDelegate, Shortcut.CtrlF);
                this.dotNetMenuProvider.SetDrawSpecial(mi, true);

                if (((Effect)lastEffect).Image != null)
                {
                    SetMenuIcon(mi, ((Effect)lastEffect).Image);
                }

                mi.Shortcut = Shortcut.CtrlF;

                menuEffects.MenuItems.Add(mi);

                // add separator
                MenuItem separator = new MenuItem("-");
                this.dotNetMenuProvider.SetDrawSpecial(separator, true);
                menuEffects.MenuItems.Add(separator);
                repeatEffectKeyboardHackEnabled = true;
                effectsKeyboardHackEnabled = true;
            }

            // Fill the menu with the effect names, and "..." if it is configurable
            AddEffectsToMenu(menuEffects, new BoolObjectDelegate(EffectsMenuPredicate), false);

            effectsPopulated = true;
        }

        private void PopulateAdjustmentsMenu()
        {
            menuLayersAdjustments.MenuItems.Clear();
            AddEffectsToMenu(menuLayersAdjustments, new BoolObjectDelegate(AdjustmentsMenuPredicate), true);
            adjustmentsPopulated = true;
        }

        private void PopulateEffectsAndAdjustmentsMenus()
        {
            PopulateEffectsMenu();
            PopulateAdjustmentsMenu();
        }

        private void menuEffects_Popup(object sender, System.EventArgs e)
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

        private void menuLayersAdjustments_Popup(object sender, System.EventArgs e)
        {
            PopulateAdjustmentsMenu();
            menuLayersAdjustments.Popup -= new EventHandler(menuLayersAdjustments_Popup);
        }

        private void AddEffectsToMenu(MenuItem topMenu, BoolObjectDelegate predicate, bool withShortcuts)
        {
            // Fill the menu with the effect names, and "..." if it is configurable
            foreach (Type type in workspace.Effects)
            {
                ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
                Effect effect = (Effect)ci.Invoke(null);

                if (!predicate(effect))
                {
                    continue;
                }

                string name = effect.Name;

                if (effect is IConfigurableEffect)
                {
                    string configurableFormat = PdnResources.GetString("MainForm.Effects.Name.Format.Configurable");
                    name = string.Format(configurableFormat, name);
                }

                MenuItem mi;
                
                if (withShortcuts)
                {
                    mi = new MenuItem(name, menuEffectsClickDelegate, effect.Shortcut);
                }
                else
                {
                    mi = new MenuItem(name, menuEffectsClickDelegate);
                }

                this.dotNetMenuProvider.SetDrawSpecial(mi, true);

                if (effect.Image != null)
                {
                    this.SetMenuIcon(mi, effect.Image);
                }

                MenuItem addEffectHere = topMenu;

                if (effect.SubMenuName != null)
                {
                    MenuItem subMenu = null;

                    // search for this subMenu
                    foreach (MenuItem sub in menuEffects.MenuItems)
                    {
                        if (sub.Text == effect.SubMenuName)
                        {
                            subMenu = sub;
                        }
                    }

                    if (subMenu == null)
                    {
                        subMenu = new MenuItem(effect.SubMenuName);
                        this.dotNetMenuProvider.SetDrawSpecial(subMenu, true);
                        topMenu.MenuItems.Add(subMenu);
                    }

                    addEffectHere = subMenu;
                }

                addEffectHere.MenuItems.Add(mi);
            }         
        }

        private void menuFileAcquire_Popup(object sender, System.EventArgs e)
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
            workspace.PerformAction(typeof(InvertSelectionAction));
        }

        private void menuEditClearSelection_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(EraseSelectionAction));
        }

        private void menuEditSelectAll_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(SelectAllAction));
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

                        Document newDoc = CanvasSizeAction.ResizeDocument(this, workspace.Document, newSize, 
                            AnchorEdge.TopLeft, workspace.Environment.BackColor, false, false);

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
            Rectangle visibleDocRect = workspace.DocumentView.VisibleDocumentRectangle;
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

                Point docScrollPos = workspace.DocumentView.DocumentScrollPosition;

                Point newDocScrollPos = new Point(docScrollPos.X + delta.Width,
                                                  docScrollPos.Y + delta.Height);

                workspace.DocumentView.DocumentScrollPosition = newDocScrollPos;
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

        private void menuLayers_Popup(object sender, System.EventArgs e)
        {
        }

        private void menuLayersDeleteLayer_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.PerformDeleteLayerClick();
        }

        private void menuWindow_Popup(object sender, System.EventArgs e)
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
                workspace.Focus();
            }
        }

        private void menuWindowHistory_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.HistoryForm.Visible = !workspace.Widgets.HistoryForm.Visible;
            if (!workspace.Widgets.HistoryForm.Visible) 
            {
                //if we don't do this, hiding the last floating window can cause PDN to lose focus
                workspace.Focus();
            }
        }

        private void menuWindowLayers_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.Visible = !workspace.Widgets.LayerForm.Visible; 
            if (!workspace.Widgets.LayerForm.Visible) 
            {
                //if we don't do this, hiding the last floating window can cause PDN to lose focus
                workspace.Focus();
            }
        }

        private void menuWindowColors_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.ColorsForm.Visible = !workspace.Widgets.ColorsForm.Visible;
            if (!workspace.Widgets.ColorsForm.Visible) 
            {
                //if we don't do this, hiding the last floating window can cause PDN to lose focus
                workspace.Focus();
            }
        }

        private void menuImage_Popup(object sender, System.EventArgs e)
        {
            menuImageCrop.Enabled = !workspace.Environment.Selection.IsEmpty;
            menuImageFlatten.Enabled = (workspace.Document.Layers.Count > 1);
        }

        private void menuImageCrop_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(CropAction));
        }

        private void menuImageResize_Click(object sender, System.EventArgs e)
        {
            workspace.PerformAction(typeof(ResizeAction));
        }

        private void menuTools_Popup(object sender, System.EventArgs e)
        {
            menuTools.MenuItems.Clear();
            menuTools.MenuItems.Add(menuToolsAntiAliasing);
            menuTools.MenuItems.Add(menuToolsAlphaBlending);
            menuTools.MenuItems.Add(menuToolsSeperator);

            foreach (ToolInfo toolInfo in workspace.ToolInfos)
            {
                MenuItem mi = new MenuItem(toolInfo.Name, menuToolsClickDelegate);
                SetMenuIcon(mi, (Image)toolInfo.Image.Clone());
                menuTools.MenuItems.Add(mi);
            }
            
            menuTools.Popup -= new EventHandler(menuTools_Popup);
            menuTools.Popup += new EventHandler(menuTools_Popup2);
            menuTools_Popup2(sender, e);
        }

        private void menuTools_Popup2(object sender, System.EventArgs e)
        {
            Tool currentTool = workspace.Environment.Tool;

            foreach (MenuItem mi in menuTools.MenuItems)
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

            menuToolsAntiAliasing.Checked = workspace.Environment.AntiAliasing;
            menuToolsAlphaBlending.Checked = workspace.Environment.AlphaBlending;
        }

        private void menuTools_ClickHandler(object sender, System.EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;

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

            this.cursorInfoStatusBar.Text = string.Format(
                CultureInfo.InvariantCulture, 
                this.cursorInfoStatusBarFormat, 
                xString, 
                units, 
                yString, 
                units);
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
            this.progressStatusBar.Text = string.Empty;
            this.progressStatusBar.Icon = null;
        }

        private string progressTextFormat = PdnResources.GetString("MainForm.StatusBar.Progress.Percentage.Format");
        private void ResetProgressStatusBar()
        {
            this.progressStatusBar.Text = string.Format(this.progressTextFormat, 0);

            if (this.progressStatusBar.Icon == null)
            {
                this.progressStatusBar.Icon = this.StopWatchIcon;
            }
        }

        private double GetProgressStatusBarValue()
        {
            lock (progressStatusBar)
            {
                try
                {
                    if (progressStatusBar.Text == string.Empty)
                    {
                        return -1.0;
                    }
                    else
                    {
                        return double.Parse(progressStatusBar.Text.Substring(0, progressStatusBar.Text.Length - 1));
                    }
                }

                catch (FormatException)
                {
                    return -1.0;
                }
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
                if (GetProgressStatusBarValue() < percent)
                {
                    this.progressStatusBar.Text = string.Format(this.progressTextFormat, (int)percent);

                    if (this.progressStatusBar.Icon == null)
                    {
                        this.progressStatusBar.Icon = this.StopWatchIcon;
                    }
                }
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

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
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
        /// There seems to be a problem with the way WinForms (or DotNetWidgets?) handles additions to the 
        /// menu system that have shortcuts. It seems that when you add a new menu item w/ a shortcut, it
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
                    if (repeatEffectKeyboardHackEnabled && e.Control && e.KeyCode == Keys.F)
                    {
                        if (menuEffects.MenuItems.Count > 0 &&
                            menuEffects.MenuItems[0].Shortcut == Shortcut.CtrlF)
                        {
                            menuEffects.MenuItems[0].PerformClick();
                            repeatEffectKeyboardHackEnabled = false;
                        }
                    }
                    else if (effectsKeyboardHackEnabled && IsDynamicAcceleratorHACK(e))
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
        /// Used to convert a Keys into a string that can be used for direct
        /// comparison to a Shortcut enumeration member. This method is used
        /// by DoAcceleratorHACK().
        /// </summary>
        /// <param name="key">The "Keys" that were pressed.</param>
        /// <returns>A string that can be compared to a Shortcut.ToString()</returns>
        private static string KeysToString(KeyEventArgs key)
        {
            string ret = string.Empty;

            if (key.Alt)
            {
                ret += "Alt";
            }

            if (key.Control)
            {
                ret += "Ctrl";
            }

            if (key.Shift)
            {
                ret += "Shift";
            }

            if (key.KeyCode == Keys.Back)
            {
                ret += "Bksp";
            }
            else if (key.KeyCode == Keys.Insert)
            {
                ret += "Ins";
            }
            else if (key.KeyCode == Keys.Delete)
            {
                ret += "Del";
            }
            else
            {
                ret += key.KeyCode.ToString();
            }

            return ret;
        }

        /// <summary>
        /// Similar to DoAcceleratorHACK, but only tests whether a key is an accelerator or not.
        /// Does not actually perform the "menu item click."
        /// </summary>
        /// <param name="keyInfo"></param>
        /// <returns></returns>
        private bool IsAcceleratorHACK(KeyEventArgs keyInfo)
        {
            string keyName = KeysToString(keyInfo);
            
            Type ourType = this.GetType();

            FieldInfo[] fields = ourType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType == typeof(MenuItem))
                {
                    MenuItem mi = (MenuItem)fi.GetValue(this);
                    Shortcut shortcut = mi.Shortcut;
                    
                    if (keyName == shortcut.ToString())
                    {
                        return true;
                    }
                }
            }

            return IsDynamicAcceleratorHACK(keyInfo);
        }

        private bool IsDynamicAcceleratorHACK(KeyEventArgs keyInfo)
        {
            string keyName = KeysToString(keyInfo);

            // Populate the Layers->Adjustments list if it isn't populated.
            if (menuLayersAdjustments.MenuItems.Count <= 1)
            {
                this.menuLayersAdjustments_Popup(this, EventArgs.Empty);
            }

            foreach (MenuItem mi in menuLayersAdjustments.MenuItems)
            {
                if (keyName == mi.Shortcut.ToString())
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
            string keyName = KeysToString(keyInfo);
            Type ourType = this.GetType();
            FieldInfo[] fields = ourType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType == typeof(MenuItem))
                {
                    MenuItem mi = (MenuItem)fi.GetValue(this);

                    // If they want to paste and the ColorsForm is active, don't handle it
                    if (mi == menuEditPaste && Form.ActiveForm == workspace.Widgets.ColorsForm)
                    {
                        continue;
                    }

                    Shortcut shortcut = mi.Shortcut;
                    
                    if (keyName == shortcut.ToString())
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
            string keyName = KeysToString(keyInfo);

            if (keyName.Length < 2)
            {
                return;
            }

            if (menuLayersAdjustments.MenuItems.Count <= 1)
            {
                PopulateAdjustmentsMenu();
            }

            foreach (MenuItem mi in menuLayersAdjustments.MenuItems)
            {
                if (keyName == mi.Shortcut.ToString())
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

        private void CommonActionsWidget_ButtonClick(object sender, EnumValueEventArgs e)
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
        }

        private readonly string contextStatusBarFormat = PdnResources.GetString("MainForm.StatusBar.Context.SelectedArea.Text.Format");
        private readonly string contextStatusBarWithAngleFormat = PdnResources.GetString("MainForm.StatusBar.Context.SelectedArea.Text.WithAngle.Format");

        private void Environment_SelectedPathChanged(object sender, EventArgs e)
        {
            if (workspace.Environment.Selection.IsEmpty)
            {
                this.contextStatusBar.Text = string.Empty;
                this.contextStatusBar.Icon = null;
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
                
                string pluralUnits = PdnResources.GetString("MeasurementUnit." + workspace.Environment.Units.ToString(CultureInfo.InvariantCulture) + ".Plural");

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

                    this.contextStatusBar.Text = string.Format(
                        contextStatusBarWithAngleFormat, 
                        widthString, 
                        unitsAbbreviation, 
                        heightString, 
                        unitsAbbreviation,
                        areaString,
                        pluralUnits.ToLower(),
                        moveTool.HostAngle.ToString("N", nfi2));
                }
                else
                {
                    this.contextStatusBar.Text = string.Format(
                        contextStatusBarFormat, 
                        widthString, 
                        unitsAbbreviation, 
                        heightString, 
                        unitsAbbreviation,
                        areaString,
                        pluralUnits.ToLower());
                }

                if (!object.ReferenceEquals(this.contextStatusBar.Icon, this.SelectionIcon))
                {
                    this.contextStatusBar.Icon = this.SelectionIcon;
                }
            }
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
            // Otherwise we tell them they need XP SP1.
            if (Environment.OSVersion.Version >= new Version(5, 2))
            {
                Utility.ErrorBox(this, PdnResources.GetString("WIA.Error.EnableMe"));
            }
            else
            {
                Utility.ErrorBox(this, PdnResources.GetString("WIA.Error.RequiresXPSP1"));
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
                workspace.Document.RenderFlat(ra);
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
            ArrayList result = new ArrayList();

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

            return (string[])result.ToArray(typeof(string));
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
                    Image image = Image.FromFile(fileName);
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

        private void menuFileOpenRecent_Popup(object sender, System.EventArgs e)
        {
            LoadMruList();
            MostRecentFile[] filesReverse = mostRecentFiles.GetFileList();
            MostRecentFile[] files = new MostRecentFile[filesReverse.Length];
            int i;

            for (i = 0; i < filesReverse.Length; ++i)
            {
                files[files.Length - i - 1] = filesReverse[i];
            }

            foreach (MenuItem mi in menuFileOpenRecent.MenuItems)
            {
                mi.Click -= new EventHandler(menuFileOpenRecentFile_Click);
            }

            if (mruDotNetMenuProvider != null)
            {
                mruDotNetMenuProvider.OwnerForm = null;
                mruDotNetMenuProvider.Dispose();
                mruDotNetMenuProvider = null;
            }

            mruDotNetMenuProvider = new DotNetWidgets.DotNetMenuProvider();
            mruDotNetMenuProvider.OwnerForm = this;
            mruDotNetMenuProvider.ImageList = mruImageList;

            menuFileOpenRecent.MenuItems.Clear();

            foreach (Image image in mruImageList.Images)
            {
                image.Dispose();
            }

            mruImageList.Images.Clear();
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
                MenuItem mi = new MenuItem(menuName);
                mi.Click += new EventHandler(menuFileOpenRecentFile_Click);
                int thumbIndex = mruImageList.Images.Add(mrf.Thumb, mruImageList.TransparentColor);
                mruDotNetMenuProvider.SetDrawSpecial(mi, true);
                mruDotNetMenuProvider.SetImageIndex(mi, thumbIndex);
                menuFileOpenRecent.MenuItems.Add(mi);
                ++i;
            }

            if (menuFileOpenRecent.MenuItems.Count == 0)
            {
                MenuItem none = new MenuItem(PdnResources.GetString("MainForm.Menu.File.OpenRecent.None"));
                none.Enabled = false;
                mruDotNetMenuProvider.SetDrawSpecial(none, true);
                menuFileOpenRecent.MenuItems.Add(none);
            }
            else
            {
                MenuItem separator = new MenuItem("-");
                mruDotNetMenuProvider.SetDrawSpecial(separator, true);
                menuFileOpenRecent.MenuItems.Add(separator);

                MenuItem clearList = new MenuItem();
                clearList.Text = PdnResources.GetString("MainForm.Menu.File.OpenRecent.ClearThisList");
                mruDotNetMenuProvider.SetDrawSpecial(clearList, true);
                menuFileOpenRecent.MenuItems.Add(clearList);
                Image deleteIcon = PdnResources.GetImage("Icons.MenuEditEraseSelectionIcon.bmp");
                Bitmap bitmap = new Bitmap(mruDotNetMenuProvider.ImageList.ImageSize.Width, mruDotNetMenuProvider.ImageList.ImageSize.Height);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(mruDotNetMenuProvider.ImageList.TransparentColor);
                    Point offset = new Point((bitmap.Width - deleteIcon.Width) / 2, (bitmap.Height - deleteIcon.Height) / 2);
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(deleteIcon, offset.X, offset.Y, deleteIcon.Width, deleteIcon.Height);
                }

                for (int y = 0; y < bitmap.Height; ++y)
                {
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        if (bitmap.GetPixel(x, y) == Color.FromArgb(192, 192, 192))
                        {
                            bitmap.SetPixel(x, y, mruImageList.TransparentColor);
                        }
                    }
                }
             
                int index = mruImageList.Images.Add(bitmap, mruImageList.TransparentColor);
                mruDotNetMenuProvider.SetImageIndex(clearList, index);
                clearList.Click += new EventHandler(clearList_Click);
            }
        }

        private void menuFileOpenRecentFile_Click(object sender, System.EventArgs e)
        {
            try
            {
                MenuItem mi = (MenuItem)sender;
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

        private void menuView_Popup(object sender, System.EventArgs e)
        {
            menuViewZoomToSelection.Enabled = !workspace.Environment.Selection.IsEmpty;
            menuViewZoomToWindow.Checked = (workspace.ZoomBasis == ZoomBasis.Window);
            menuViewGrid.Checked = workspace.DocumentView.DrawGrid;
            menuViewRulers.Checked = workspace.DocumentView.RulersEnabled;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // HACK: Doing this fixes a really messed up bug that we run into in the following circumstances:
                //       Sometimes a StackOverflowException is raised if you do the following:
                //       1. Start up Paint.NET and then have the window in a normal (non-maximized, non-mimized) state.
                //       2. Select the Lasso tool and scribble with it in a medium-sized area for maybe 5-10 seconds.
                //       3. Find another program and maximize it so it overlaps Paint.NET (like IE, FF, VS, whatever)
                //       4. Wait 1 minute.
                //       5. Click on Paint.NET.
                //       6. Crash!
                // So to work around this. we just redraw the entire document on activation (user clicks on us).
                // (bug #907)
                // Note: We use WM_ACTIVATEAPP instead of override OnActivated because we don't want to do this
                //       when switching between forms inside of *our* application
                case NativeMethods.WmConstants.WM_ACTIVATEAPP:
                    if (m.WParam == new IntPtr(1))
                    {
                        if (workspace != null)
                        {
                            if (!workspace.Environment.Selection.IsEmpty)
                            {
                                this.workspace.Document.Invalidate();
                            }
                        }
                    }

                    goto default;

                default:
                    base.WndProc (ref m);
                    break;
            }
        }

        private void populateEffectsTimer_Tick(object sender, EventArgs e)
        {
            populateEffectsTimer.Enabled = false;
            populateEffectsTimer.Tick -= new EventHandler(populateEffectsTimer_Tick);
            populateEffectsTimer.Dispose();
            populateEffectsTimer = null;

            PopulateEffectsAndAdjustmentsMenus();
        }

        private void ftf_VisibleChanged(object sender, EventArgs e)
        {
            workspace.Focus();
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
                this.Icon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuFileUpdatesIcon.bmp"), true);
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
                this.Icon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuFileUpdatesIcon.bmp"), true);
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

        private void menuFileUpdates_Popup(object sender, EventArgs e)
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
            this.cursorInfoStatusBar.Text = string.Empty;
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

        private class MenuTitleAndLocaleComparer
            : IComparer
        {
            public int Compare(object x, object y)
            {
                return string.Compare(((MenuTitleAndLocale)x).title, ((MenuTitleAndLocale)y).title);
            }
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
                return ci.DisplayName;
            }
            else
            {
                return GetCultureInfoName(ci.Parent);
            }
        }

        private void menuFileLanguage_Popup(object sender, EventArgs e)
        {
            this.menuFileLanguage.MenuItems.Clear();
            const string left = "PaintDotNet.Strings";
            const string right = ".resources";
            string ourDir = Path.GetDirectoryName(Application.ExecutablePath);
            string fileSpec = left + "*" + right;
            string[] pathNames = Directory.GetFiles(ourDir, fileSpec);
            MenuTitleAndLocale[] mtals = new MenuTitleAndLocale[pathNames.Length];

            for (int i = 0; i < pathNames.Length; ++i)
            {
                string pathName = pathNames[i];
                string dirName = Path.GetDirectoryName(pathName);
                string fileName = Path.GetFileName(pathName);
                string sansRight = fileName.Substring(0, fileName.Length - right.Length);
                string sansLeft = sansRight.Substring(left.Length);

                string locale;
                if (sansLeft.Length > 0 && sansLeft[0] == '.')
                {
                    locale = sansLeft.Substring(1);
                }
                else if (sansLeft.Length == 0)
                {
                    locale = "en-US";
                }
                else
                {
                    locale = sansLeft;
                }

                CultureInfo cultureInfo = new CultureInfo(locale, true);
                mtals[i] = new MenuTitleAndLocale(cultureInfo.DisplayName, locale);
            }

            Array.Sort(mtals, new MenuTitleAndLocaleComparer());

            foreach (MenuTitleAndLocale mtal in mtals)
            {
                MenuItemWithTag menuItem = new MenuItemWithTag();
                menuItem.Text = GetCultureInfoName(new CultureInfo(mtal.locale)); // mtal.title;
                menuItem.Tag = mtal.locale;
                menuItem.Click += new EventHandler(LanguageMenuItem_Click);

                if (mtal.locale == CultureInfo.CurrentUICulture.Name)
                {
                    menuItem.Checked = true;
                }

                this.dotNetMenuProvider.SetDrawSpecial(menuItem, true);
                this.menuFileLanguage.MenuItems.Add(menuItem);
            }
        }
       
        private void LanguageMenuItem_Click(object sender, EventArgs e)
        {
            MenuItemWithTag miwt = (MenuItemWithTag)sender;
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
            contextStatusBar.Text = workspace.Environment.Tool.StatusText;
            contextStatusBar.Icon = workspace.Environment.Tool.StatusIcon;
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

        private void menuViewUnits_Popup(object sender, EventArgs e)
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
