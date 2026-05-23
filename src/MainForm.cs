/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
using System.Reflection;
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
        private const int tilesPerCpu = 25;
        private int renderingThreadCount = Math.Max(2, Processor.LogicalCpuCount);

        private System.Windows.Forms.MenuItem menuFileExit;
        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem menuFileOpen;
        private System.Windows.Forms.MenuItem menuFileNew;
        private System.Windows.Forms.MenuItem menuFileSave;
        private System.Windows.Forms.MenuItem menuFileSaveAs;
        private System.Windows.Forms.MenuItem menuFileAcquire;
        private System.Windows.Forms.MenuItem menuFileAcquireFromScannerOrCamera;
        private System.Windows.Forms.MenuItem menuFileAcquireFromClipboard;
        private System.Windows.Forms.MenuItem menuHelpAbout;
        private System.Windows.Forms.MenuItem menuEditUndo;
        private System.Windows.Forms.MenuItem menuFile;
        private System.Windows.Forms.MenuItem menuEdit;
        private System.Windows.Forms.MenuItem menuHelp;
        private System.Windows.Forms.MenuItem menuSeparator1;
        private System.Windows.Forms.MenuItem menuSeparator2;
        private System.Windows.Forms.MenuItem menuEditCopy;
        private System.Windows.Forms.MenuItem menuEditPaste;
        private System.Windows.Forms.MenuItem menuEditCut;
        private System.Windows.Forms.MenuItem menuSeparator4;
        private System.Windows.Forms.StatusBar statusBar;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.MenuItem menuEffects;
        private PaintDotNet.DocumentWorkspace workspace;
        private readonly FileTypeCollection fileTypes = FileTypes.Collection;
        private System.Windows.Forms.MenuItem menuEffectsSentinel;
        private System.Windows.Forms.MenuItem menuEditInvertSelection;
        private System.Windows.Forms.MenuItem menuEditSelectAll;
        private System.Windows.Forms.MenuItem menuEditDeselect;
        private System.Windows.Forms.MenuItem menuLayers;
        private System.Windows.Forms.MenuItem menuLayersAddNewLayer;
        private System.Windows.Forms.MenuItem menuLayersDeleteLayer;
        private System.Windows.Forms.MenuItem menuSeparator5;
        private System.Windows.Forms.MenuItem menuSeparator6;
        private System.Windows.Forms.MenuItem menuWindow;
        private System.Windows.Forms.MenuItem menuWindowTools;
        private System.Windows.Forms.MenuItem menuWindowHistory;
        private System.Windows.Forms.MenuItem menuWindowLayers;
        private System.Windows.Forms.MenuItem menuWindowColors;
        private System.Windows.Forms.MenuItem menuImage;
        private System.Windows.Forms.MenuItem menuImageCrop;
        private System.Windows.Forms.MenuItem menuImageResize;
        private System.Windows.Forms.ImageList menuImages;
        private System.Windows.Forms.MenuItem menuTools;
        private EventHandler menuEffectsClickDelegate;
        private EventHandler menuToolsClickDelegate;
        private CancelEventHandler hideInsteadOfCloseDelegate;

        private System.Windows.Forms.MenuItem menuEditRedo;
        private System.Windows.Forms.MenuItem menuDebug;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuImageFlip;
        private System.Windows.Forms.MenuItem menuImageFlipHorizontal;
        private System.Windows.Forms.MenuItem menuImageFlipVertical;
        private System.Windows.Forms.MenuItem menuLayersFlip;
        private System.Windows.Forms.MenuItem menuLayersFlipHorizontal;
        private System.Windows.Forms.MenuItem menuLayersFlipVertical;
        private System.Windows.Forms.MenuItem menuImageFlatten;

        // NOTE: This is done as an object and not EffectConfigToken so that we can delay loading
        //       the PaintDotNet.Effects.dll until after we start up
        private object lastEffectToken = null;

        // NOTE: This is done as an object and not Effect so that we can delay loading the 
        //       PaintDotNet.Effects.dll until after we start up
        private object lastEffect = null;

        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.StatusBarPanel progressStatusBar;
        private System.Windows.Forms.StatusBarPanel imageInfoStatusBar;
        private System.Windows.Forms.StatusBarPanel cursorInfoStatusBar;
        private System.Windows.Forms.MenuItem menuImageCanvasSize;
        private System.Windows.Forms.MenuItem menuHelpHelpTopics;
        private System.Windows.Forms.MenuItem menuSeparator7;
        private System.Windows.Forms.MenuItem menuLayersDuplicateLayer;

        // We keep track of each configurable effect's last token
        // This way it keeps its values in between user invocations
        private Hashtable effectTokenHash = new Hashtable();
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.MenuItem menuWindowResetWindowLocations;
        private System.Windows.Forms.MenuItem menuItem9;
        private System.Windows.Forms.MenuItem menuLayersLayerProperties;
        private System.Windows.Forms.MenuItem menuItem13;
        private System.Windows.Forms.MenuItem menuImageRotate;
        private System.Windows.Forms.MenuItem menuImageRotate90CW;
        private System.Windows.Forms.MenuItem menuImageRotate180CW;
        private System.Windows.Forms.MenuItem menuImageRotate270CW;
        private System.Windows.Forms.MenuItem menuImageRotate90CCW;
        private System.Windows.Forms.MenuItem menuImageRotate180CCW;
        private System.Windows.Forms.MenuItem menuImageRotate270CCW;
        private System.Windows.Forms.MenuItem menuSeparator8;
        private System.Windows.Forms.StatusBarPanel contextStatusBar;
        private System.Windows.Forms.Timer floaterOpacityTimer;
        private FloatingToolForm[] floaters;
        private System.Windows.Forms.MenuItem menuEditEraseSelection;
        private System.Windows.Forms.MenuItem menuEditPasteInToNewLayer;
        private System.Windows.Forms.MenuItem menuFilePrint;
        private System.Windows.Forms.MenuItem menuItem10;
        private System.Windows.Forms.Timer invalidateTimer;
        private System.Windows.Forms.Timer populateEffectsTimer;
        private System.Windows.Forms.MenuItem menuFileOpenInNewWindow;
        private System.Windows.Forms.MenuItem menuFileNewWindow;
        private System.Windows.Forms.MenuItem menuItem11;
        private System.Windows.Forms.MenuItem menuItem8;
        private System.Windows.Forms.MenuItem menuItem14;

        private MostRecentFiles mostRecentFiles = null;
        private const int defaultMostRecentFilesMax = 8;
        private const int mruIconSize = 32;
        private System.Windows.Forms.MenuItem menuFileOpenRecent;
        private System.Windows.Forms.MenuItem menuItem16;
        private System.Windows.Forms.ImageList mruImageList;
        private DotNetWidgets.DotNetMenuProvider mruDotNetMenuProvider = null;
        private DotNetWidgets.DotNetMenuProvider dotNetMenuProvider = null;
        private System.Windows.Forms.MenuItem menuItem17;
        private System.Windows.Forms.MenuItem menuLayersAdjustments;
        private System.Windows.Forms.MenuItem menuLayersImportFromFile;
        private System.Windows.Forms.MenuItem menuWindowTranslucent;
        private System.Windows.Forms.MenuItem menuItem12;
        private System.Windows.Forms.MenuItem menuItem18;

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

        private System.Windows.Forms.MenuItem menuViewZoomIn;
        private System.Windows.Forms.MenuItem menuViewZoomOut;
        private System.Windows.Forms.MenuItem menuViewZoomToWindow;
        private System.Windows.Forms.MenuItem menuViewZoomToSelection;
        private System.Windows.Forms.MenuItem menuViewActualSize;
        private System.Windows.Forms.MenuItem menuToolsAntiAliasing;
        private System.Windows.Forms.MenuItem menuToolsSeperator;
        private System.Windows.Forms.MenuItem menuViewSeperator;
        private System.Windows.Forms.MenuItem menuViewRulers;
        private System.Windows.Forms.MenuItem menuViewGrid;
        private System.Windows.Forms.MenuItem menuView;

        private bool doKill = false;
        private bool adjustmentsPopulated = false;
        private bool effectsPopulated = false;

        private SplashForm splash = null;

        public MainForm()
            : this(new string[0])
        {
        }

        public MainForm(string[] args)
        {
            paintDotNetIcon = PdnGraphics.LoadApplicationIcon(); //new Icon(Utility.GetResourceStream("PaintDotNet.ico"));
            stopWatchIcon = new Icon(Utility.GetResourceStream("Icons.StopWatchIcon.ico"));
            selectionIcon = new Icon(Utility.GetResourceStream("Icons.SelectionIcon.ico"));
            helpIcon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuHelpHelpTopicsIcon.bmp"), true);
            cursorXYIcon = new Icon(Utility.GetResourceStream("Icons.CursorXYIcon.ico"));
            imageSizeIcon = new Icon(Utility.GetResourceStream("Icons.ImageSizeIcon.ico"));
            addNewLayerIcon = Utility.GetImageResource("Icons.MenuLayersAddNewLayerIcon.bmp");
            fileNewIcon = Utility.GetImageResource("Icons.MenuFileNewIcon.bmp");
            editCutIcon = Utility.GetImageResource("Icons.MenuEditCutIcon.bmp");
            imageFromDiskIcon = Utility.GetImageResource("Icons.ImageFromDiskIcon.bmp");
            layersImportFromFileIcon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuLayersImportFromFileIcon.bmp"), true);

            this.StartPosition = FormStartPosition.WindowsDefaultLocation;

            bool noSplash = true; 
            string fileName = null;

            // Parse command line arguments
            foreach (string argument in args)
            {
                if (0 == string.Compare(argument, "/splash", true))
                {
                    noSplash = false;
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

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.dotNetMenuProvider = new DotNetWidgets.DotNetMenuProvider();
            this.components.Add(dotNetMenuProvider);
            dotNetMenuProvider.ImageList = this.menuImages;
            TurnOnSpecialDrawing();

            workspace.DocumentView.ScaleFactorChanged += new EventHandler(DocumentView_ScaleFactorChanged);
            this.mruImageList.ImageSize = new System.Drawing.Size(2 + MainForm.mruIconSize, 2 + MainForm.mruIconSize);

            components = null;

            this.Icon = paintDotNetIcon;

            menuEffectsClickDelegate = new EventHandler(menuEffects_ClickHandler);
            menuToolsClickDelegate = new EventHandler(menuTools_ClickHandler);
            hideInsteadOfCloseDelegate = new CancelEventHandler(HideInsteadOfCloseHandler);

            // open any file if they want it

            // Does not load window location/state
            LoadSettings();

            bool fileOpened = false;

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
                CreateBlankDocument(GetNewDocumentSize());
            }

            workspace.Document.Dirty = false;


            // WHen the user changes the display resolution, we need to do some fixing of our UI
            // like making sure our floaters are actually on screen
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);

#if !DEBUG
            menuDebug.Visible = false;
#endif
            // NOTE: Since we ngen as part of setup now, this is no longer necessary.
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
        }

        private void SetToolHelpText()
        {
            if (workspace.Environment.Tool != null)
            {
                string toolName = workspace.Environment.Tool.Name;
                string helpText = workspace.Environment.Tool.HelpText;

                contextStatusBar.Text = toolName + ": " + helpText;
                contextStatusBar.Icon = helpIcon;
            }
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
                TurnOnSpecialDrawing(mi);
            }
        }

        private void TurnOnSpecialDrawing(MenuItem menuItem)
        {
            dotNetMenuProvider.SetDrawSpecial(menuItem, true);

            foreach (MenuItem mi in menuItem.MenuItems)
            {
                TurnOnSpecialDrawing(mi);
            }
        }

        private void LoadWindowState()
        {
            try
            {
                FormWindowState fws = (FormWindowState)Enum.Parse(typeof(FormWindowState), Settings.GetString(PdnSettings.WindowState, WindowState.ToString()), true);

                if (fws != FormWindowState.Minimized)
                {
                    if (fws != FormWindowState.Maximized)
                    {
                        Rectangle newBounds = Rectangle.Empty;

                        /* load the registry values into a rectangle so that we
                         * can update the settings all at once, instead of one
                         * at a time. This will make loading the size an all or
                         * none operation, with no rollback necessary */
                        newBounds.Width = Settings.GetInt32(PdnSettings.Width, this.Width);
                        newBounds.Height = Settings.GetInt32(PdnSettings.Height, this.Height);

                        int top = Settings.GetInt32(PdnSettings.Top, this.Top);
                        int left = Settings.GetInt32(PdnSettings.Left, this.Left);
                        newBounds.Location = new Point(top, left);

                        this.Bounds = newBounds;
                    }

                    this.WindowState = fws;
                }
            }

            catch
            {
                Settings.Delete(new string[] { 
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
                this.workspace.Environment.AntiAliasing = Settings.GetBoolean(PdnSettings.AntiAliasing);
                this.workspace.DocumentView.RulersEnabled = Settings.GetBoolean(PdnSettings.Rulers);
                this.workspace.DocumentView.DrawGrid = Settings.GetBoolean(PdnSettings.DrawGrid);
                PdnBaseForm.EnableOpacity = Settings.GetBoolean(PdnSettings.TranslucentWindows);
                this.workspace.History.Limit = Settings.GetInt32(PdnSettings.HistoryLimit, -1);
            }

            catch
            {
                Settings.Delete(new string[] { PdnSettings.AntiAliasing, 
                                               PdnSettings.Rulers, 
                                               PdnSettings.DrawGrid, 
                                               PdnSettings.TranslucentWindows, 
                                               PdnSettings.HistoryLimit });
            }
        }

        private void LoadMruList()
        {
            // Load the most recent files
            try
            {
                int max = Settings.GetInt32(PdnSettings.MruMax, MainForm.defaultMostRecentFilesMax);
                this.mostRecentFiles = new MostRecentFiles(max);

                for (int i = 0; i < this.mostRecentFiles.MaxCount; ++i)
                {
                    try
                    {
                        string mruName = "MRU" + i.ToString();
                        string fileName = (string)Settings.GetString(mruName);

                        if (fileName != null)
                        {
                            Image thumb = Settings.GetImage(mruName + "Thumb");

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
            Settings.SetInt32(PdnSettings.Width, this.Width);
            Settings.SetInt32(PdnSettings.Height, this.Height);
            Settings.SetInt32(PdnSettings.Top, this.Top);
            Settings.SetInt32(PdnSettings.Left, this.Left);
            Settings.SetString(PdnSettings.WindowState, this.WindowState.ToString());

            Settings.SetBoolean(PdnSettings.AntiAliasing, this.workspace.Environment.AntiAliasing);
            Settings.SetBoolean(PdnSettings.Rulers, this.workspace.DocumentView.RulersEnabled);
            Settings.SetBoolean(PdnSettings.DrawGrid, this.workspace.DocumentView.DrawGrid);
            Settings.SetBoolean(PdnSettings.TranslucentWindows, PdnBaseForm.EnableOpacity);
            Settings.SetInt32(PdnSettings.HistoryLimit, this.workspace.History.Limit);

            if (this.WindowState != FormWindowState.Minimized)
            {
                Settings.SetBoolean(PdnSettings.ToolsFormVisible, this.workspace.Widgets.MainToolBarForm.Visible);
                Settings.SetBoolean(PdnSettings.ColorsFormVisible, this.workspace.Widgets.ColorsForm.Visible);
                Settings.SetBoolean(PdnSettings.HistoryFormVisible, this.workspace.Widgets.HistoryForm.Visible);
                Settings.SetBoolean(PdnSettings.LayersFormVisible, this.workspace.Widgets.LayerForm.Visible);
            }

            SaveMruList();
        }

        private void SaveMruList()
        {
            if (mostRecentFiles == null)
            {
                return;
            }

            Settings.SetInt32("MRUMax", this.mostRecentFiles.MaxCount);
            MostRecentFile[] mrfArray = mostRecentFiles.GetFileList();

            for (int i = 0; i < mrfArray.Length; ++i)
            {
                MostRecentFile mrf = mrfArray[i];
                string mruName = "MRU" + i.ToString();

                Settings.SetString(mruName, mrf.FileName);
                Settings.SetImage(mruName + "Thumb", mrf.Thumb);
            }
        }

        private void SetupStatusBars()
        {
            // context
            SetToolHelpText();
            this.workspace.Environment.ToolChanged += new EventHandler(Environment_ToolChanged);

            // cursorInfo (x,y info)
            this.cursorInfoStatusBar.Icon = cursorXYIcon;
            this.cursorInfoStatusBar.Text = string.Empty;
            this.workspace.DocumentView.DocumentMouseMove += new MouseEventHandler(DocumentView_DocumentMouseMove);
            
            // imageInfo (width,height info)
            this.imageInfoStatusBar.Icon = imageSizeIcon;
            //this.imageInfoStatusBar.Text = string.Empty;
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

            if (!e.Cancel)
            {
                SaveSettings();
                this.Hide();
            }

            base.OnClosing (e);
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
            int index = AddImageToMenuImages(Utility.GetImageResource(imageName));
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
            this.menuViewRulers = new System.Windows.Forms.MenuItem();
            this.menuViewGrid = new System.Windows.Forms.MenuItem();
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
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuLayersLayerProperties = new System.Windows.Forms.MenuItem();
            this.menuEffects = new System.Windows.Forms.MenuItem();
            this.menuEffectsSentinel = new System.Windows.Forms.MenuItem();
            this.menuTools = new System.Windows.Forms.MenuItem();
            this.menuToolsAntiAliasing = new System.Windows.Forms.MenuItem();
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
            this.statusBar = new System.Windows.Forms.StatusBar();
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
                                                                                     this.menuFileExit});
            this.menuFile.Text = "&File";
            this.menuFile.Popup += new System.EventHandler(this.menuFile_Popup);
            // 
            // menuFileNew
            // 
            this.menuFileNew.Index = 0;
            this.menuFileNew.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.menuFileNew.Text = "&New ...";
            this.menuFileNew.Click += new System.EventHandler(this.menuFileNew_Click);
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.Index = 1;
            this.menuFileOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuFileOpen.Text = "&Open ...";
            this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
            // 
            // menuFileOpenRecent
            // 
            this.menuFileOpenRecent.Index = 2;
            this.menuFileOpenRecent.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                               this.menuItem16});
            this.menuFileOpenRecent.Text = "Open &Recent";
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
            this.menuFileAcquire.Text = "Ac&quire";
            this.menuFileAcquire.Popup += new System.EventHandler(this.menuFileAcquire_Popup);
            // 
            // menuFileAcquireFromClipboard
            // 
            this.menuFileAcquireFromClipboard.Index = 0;
            this.menuFileAcquireFromClipboard.Text = "From &Clipboard";
            this.menuFileAcquireFromClipboard.Click += new System.EventHandler(this.menuFileAcquireFromClipboard_Click);
            // 
            // menuFileAcquireFromScannerOrCamera
            // 
            this.menuFileAcquireFromScannerOrCamera.Index = 1;
            this.menuFileAcquireFromScannerOrCamera.Text = "From &Scanner or Camera ...";
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
            this.menuFileNewWindow.Text = "New Window";
            this.menuFileNewWindow.Click += new System.EventHandler(this.menuFileNewWindow_Click);
            // 
            // menuFileOpenInNewWindow
            // 
            this.menuFileOpenInNewWindow.Index = 6;
            this.menuFileOpenInNewWindow.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftO;
            this.menuFileOpenInNewWindow.Text = "Open in New Window ...";
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
            this.menuFileSave.Text = "&Save";
            this.menuFileSave.Click += new System.EventHandler(this.menuFileSave_Click);
            // 
            // menuFileSaveAs
            // 
            this.menuFileSaveAs.Index = 9;
            this.menuFileSaveAs.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftS;
            this.menuFileSaveAs.Text = "Save &As...";
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
            this.menuFilePrint.Text = "Print ...";
            this.menuFilePrint.Click += new System.EventHandler(this.menuFilePrint_Click);
            // 
            // menuSeparator2
            // 
            this.menuSeparator2.Index = 12;
            this.menuSeparator2.Text = "-";
            // 
            // menuFileExit
            // 
            this.menuFileExit.Index = 13;
            this.menuFileExit.Text = "E&xit";
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
            this.menuEdit.Text = "&Edit";
            this.menuEdit.Popup += new System.EventHandler(this.menuEdit_Popup);
            // 
            // menuEditUndo
            // 
            this.menuEditUndo.Index = 0;
            this.menuEditUndo.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
            this.menuEditUndo.Text = "&Undo";
            this.menuEditUndo.Click += new System.EventHandler(this.menuEditUndo_Click);
            // 
            // menuEditRedo
            // 
            this.menuEditRedo.Index = 1;
            this.menuEditRedo.Shortcut = System.Windows.Forms.Shortcut.CtrlY;
            this.menuEditRedo.Text = "&Redo";
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
            this.menuEditCut.Text = "Cu&t";
            this.menuEditCut.Click += new System.EventHandler(this.menuEditCut_Click);
            // 
            // menuEditCopy
            // 
            this.menuEditCopy.Index = 4;
            this.menuEditCopy.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.menuEditCopy.Text = "&Copy";
            this.menuEditCopy.Click += new System.EventHandler(this.menuEditCopy_Click);
            // 
            // menuEditPaste
            // 
            this.menuEditPaste.Index = 5;
            this.menuEditPaste.Shortcut = System.Windows.Forms.Shortcut.CtrlV;
            this.menuEditPaste.Text = "&Paste";
            this.menuEditPaste.Click += new System.EventHandler(this.menuEditPaste_Click);
            // 
            // menuEditPasteInToNewLayer
            // 
            this.menuEditPasteInToNewLayer.Index = 6;
            this.menuEditPasteInToNewLayer.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftV;
            this.menuEditPasteInToNewLayer.Text = "Paste in to New Layer";
            this.menuEditPasteInToNewLayer.Click += new System.EventHandler(this.menuEditPasteInToNewLayer_Click);
            // 
            // menuEditEraseSelection
            // 
            this.menuEditEraseSelection.Index = 7;
            this.menuEditEraseSelection.Shortcut = System.Windows.Forms.Shortcut.Del;
            this.menuEditEraseSelection.Text = "&Erase Selection";
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
            this.menuEditInvertSelection.Text = "&Invert Selection";
            this.menuEditInvertSelection.Click += new System.EventHandler(this.menuEditInvertSelection_Click);
            this.menuEditInvertSelection.Shortcut = System.Windows.Forms.Shortcut.CtrlI;
            // 
            // menuEditSelectAll
            // 
            this.menuEditSelectAll.Index = 10;
            this.menuEditSelectAll.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
            this.menuEditSelectAll.Text = "Select All";
            this.menuEditSelectAll.Click += new System.EventHandler(this.menuEditSelectAll_Click);
            // 
            // menuEditDeselect
            // 
            this.menuEditDeselect.Index = 11;
            this.menuEditDeselect.Shortcut = System.Windows.Forms.Shortcut.CtrlD;
            this.menuEditDeselect.Text = "&Deselect";
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
                                                                                     this.menuViewRulers,
                                                                                     this.menuViewGrid});
            this.menuView.Text = "&View";
            this.menuView.Popup += new System.EventHandler(this.menuView_Popup);
            // 
            // menuViewZoomIn
            // 
            this.menuViewZoomIn.Index = 0;
            this.menuViewZoomIn.Shortcut = System.Windows.Forms.Shortcut.CtrlJ;
            this.menuViewZoomIn.Text = "Zoom In";
            this.menuViewZoomIn.Click += new System.EventHandler(this.menuViewZoomIn_Click);
            // 
            // menuViewZoomOut
            // 
            this.menuViewZoomOut.Index = 1;
            this.menuViewZoomOut.Shortcut = System.Windows.Forms.Shortcut.CtrlK;
            this.menuViewZoomOut.Text = "Zoom Out";
            this.menuViewZoomOut.Click += new System.EventHandler(this.menuViewZoomOut_Click);
            // 
            // menuViewZoomToWindow
            // 
            this.menuViewZoomToWindow.Index = 2;
            this.menuViewZoomToWindow.Shortcut = System.Windows.Forms.Shortcut.CtrlB;
            this.menuViewZoomToWindow.Text = "Zoom To Window";
            this.menuViewZoomToWindow.Click += new System.EventHandler(this.menuViewZoomToWindow_Click);
            // 
            // menuViewZoomToSelection
            // 
            this.menuViewZoomToSelection.Index = 3;
            this.menuViewZoomToSelection.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftB;
            this.menuViewZoomToSelection.Text = "Zoom To Selection";
            this.menuViewZoomToSelection.Click += new System.EventHandler(this.menuViewZoomToSelection_Click);
            // 
            // menuViewActualSize
            // 
            this.menuViewActualSize.Index = 4;
            this.menuViewActualSize.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftA;
            this.menuViewActualSize.Text = "Actual Size";
            this.menuViewActualSize.Click += new System.EventHandler(this.menuViewActualSize_Click);
            // 
            // menuViewSeperator
            // 
            this.menuViewSeperator.Index = 5;
            this.menuViewSeperator.Text = "-";
            // 
            // menuViewRulers
            // 
            this.menuViewRulers.Index = 6;
            this.menuViewRulers.Text = "&Rulers";
            this.menuViewRulers.Click += new System.EventHandler(this.menuViewRulers_Click);
            // 
            // menuViewGrid
            // 
            this.menuViewGrid.Index = 7;
            this.menuViewGrid.Text = "&Grid";
            this.menuViewGrid.Click += new System.EventHandler(this.menuViewGrid_Click);
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
            this.menuImage.Text = "&Image";
            this.menuImage.Popup += new System.EventHandler(this.menuImage_Popup);
            // 
            // menuImageCrop
            // 
            this.menuImageCrop.Index = 0;
            this.menuImageCrop.Text = "Cro&p to Selection";
            this.menuImageCrop.Click += new System.EventHandler(this.menuImageCrop_Click);
            // 
            // menuImageResize
            // 
            this.menuImageResize.Index = 1;
            this.menuImageResize.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
            this.menuImageResize.Text = "&Resize ...";
            this.menuImageResize.Click += new System.EventHandler(this.menuImageResize_Click);
            // 
            // menuImageCanvasSize
            // 
            this.menuImageCanvasSize.Index = 2;
            this.menuImageCanvasSize.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftR;
            this.menuImageCanvasSize.Text = "Canvas &Size ...";
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
            this.menuImageFlip.Text = "&Flip";
            // 
            // menuImageFlipHorizontal
            // 
            this.menuImageFlipHorizontal.Index = 0;
            this.menuImageFlipHorizontal.Text = "&Horizontal";
            this.menuImageFlipHorizontal.Click += new System.EventHandler(this.menuImageFlipHorizontal_Click);
            // 
            // menuImageFlipVertical
            // 
            this.menuImageFlipVertical.Index = 1;
            this.menuImageFlipVertical.Text = "&Vertical";
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
            this.menuImageRotate.Text = "Rotate";
            // 
            // menuImageRotate90CW
            // 
            this.menuImageRotate90CW.Index = 0;
            this.menuImageRotate90CW.Shortcut = System.Windows.Forms.Shortcut.CtrlH;
            this.menuImageRotate90CW.Text = "90° CW";
            this.menuImageRotate90CW.Click += new System.EventHandler(this.menuImageRotate90CW_Click);
            // 
            // menuImageRotate180CW
            // 
            this.menuImageRotate180CW.Index = 1;
            this.menuImageRotate180CW.Text = "180° CW";
            this.menuImageRotate180CW.Click += new System.EventHandler(this.menuImageRotate180CW_Click);
            // 
            // menuImageRotate270CW
            // 
            this.menuImageRotate270CW.Index = 2;
            this.menuImageRotate270CW.Text = "270° CW";
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
            this.menuImageRotate90CCW.Text = "90° CCW";
            this.menuImageRotate90CCW.Click += new System.EventHandler(this.menuImageRotate90CCW_Click);
            // 
            // menuImageRotate180CCW
            // 
            this.menuImageRotate180CCW.Index = 5;
            this.menuImageRotate180CCW.Text = "180° CCW";
            this.menuImageRotate180CCW.Click += new System.EventHandler(this.menuImageRotate180CCW_Click);
            // 
            // menuImageRotate270CCW
            // 
            this.menuImageRotate270CCW.Index = 6;
            this.menuImageRotate270CCW.Text = "270° CCW";
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
            this.menuImageFlatten.Text = "&Flatten";
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
                                                                                       this.menuItem9,
                                                                                       this.menuLayersLayerProperties});
            this.menuLayers.Text = "&Layers";
            this.menuLayers.Popup += new System.EventHandler(this.menuLayers_Popup);
            // 
            // menuLayersAddNewLayer
            // 
            this.menuLayersAddNewLayer.Index = 0;
            this.menuLayersAddNewLayer.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftN;
            this.menuLayersAddNewLayer.Text = "&Add New Layer";
            this.menuLayersAddNewLayer.Click += new System.EventHandler(this.menuLayersAddNewLayer_Click);
            // 
            // menuLayersDeleteLayer
            // 
            this.menuLayersDeleteLayer.Index = 1;
            this.menuLayersDeleteLayer.Shortcut = System.Windows.Forms.Shortcut.ShiftDel;
            this.menuLayersDeleteLayer.Text = "De&lete Layer";
            this.menuLayersDeleteLayer.Click += new System.EventHandler(this.menuLayersDeleteLayer_Click);
            // 
            // menuLayersDuplicateLayer
            // 
            this.menuLayersDuplicateLayer.Index = 2;
            this.menuLayersDuplicateLayer.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftD;
            this.menuLayersDuplicateLayer.Text = "&Duplicate Layer";
            this.menuLayersDuplicateLayer.Click += new System.EventHandler(this.menuLayersDuplicateLayer_Click);
            // 
            // menuLayersImportFromFile
            // 
            this.menuLayersImportFromFile.Index = 3;
            this.menuLayersImportFromFile.Text = "Import From File";
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
            this.menuLayersAdjustments.Text = "Adjustments";
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
            this.menuLayersFlip.Text = "F&lip";
            // 
            // menuLayersFlipHorizontal
            // 
            this.menuLayersFlipHorizontal.Index = 0;
            this.menuLayersFlipHorizontal.Text = "Horizontal";
            this.menuLayersFlipHorizontal.Click += new System.EventHandler(this.menuLayersFlipHorizontal_Click);
            // 
            // menuLayersFlipVertical
            // 
            this.menuLayersFlipVertical.Index = 1;
            this.menuLayersFlipVertical.Text = "Vertical";
            this.menuLayersFlipVertical.Click += new System.EventHandler(this.menuLayersFlipVertical_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 7;
            this.menuItem9.Text = "-";
            // 
            // menuLayersLayerProperties
            // 
            this.menuLayersLayerProperties.Index = 8;
            this.menuLayersLayerProperties.Shortcut = System.Windows.Forms.Shortcut.F4;
            this.menuLayersLayerProperties.Text = "Layer &Properties...";
            this.menuLayersLayerProperties.Click += new System.EventHandler(this.menuLayersLayerProperties_Click);
            // 
            // menuEffects
            // 
            this.menuEffects.Index = 5;
            this.menuEffects.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                        this.menuEffectsSentinel});
            this.menuEffects.Text = "Effe&cts";
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
                                                                                      this.menuToolsSeperator});
            this.menuTools.Text = "&Tools";
            this.menuTools.Popup += new System.EventHandler(this.menuTools_Popup);
            // 
            // menuToolsAntiAliasing
            // 
            this.menuToolsAntiAliasing.Index = 0;
            this.menuToolsAntiAliasing.Text = "&Anti-Aliasing";
            this.menuToolsAntiAliasing.Click += new System.EventHandler(this.menuToolsAntiAliasing_Click);
            // 
            // menuToolsSeperator
            // 
            this.menuToolsSeperator.Index = 1;
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
            this.menuWindow.Text = "&Window";
            this.menuWindow.Popup += new System.EventHandler(this.menuWindow_Popup);
            // 
            // menuWindowResetWindowLocations
            // 
            this.menuWindowResetWindowLocations.Index = 0;
            this.menuWindowResetWindowLocations.Text = "&Reset Window Locations";
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
            this.menuWindowTranslucent.Text = "Translucent";
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
            this.menuWindowTools.Text = "&Tools";
            this.menuWindowTools.Click += new System.EventHandler(this.menuWindowTools_Click);
            // 
            // menuWindowHistory
            // 
            this.menuWindowHistory.Index = 5;
            this.menuWindowHistory.Shortcut = System.Windows.Forms.Shortcut.F6;
            this.menuWindowHistory.Text = "&History";
            this.menuWindowHistory.Click += new System.EventHandler(this.menuWindowHistory_Click);
            // 
            // menuWindowLayers
            // 
            this.menuWindowLayers.Index = 6;
            this.menuWindowLayers.Shortcut = System.Windows.Forms.Shortcut.F7;
            this.menuWindowLayers.Text = "&Layers";
            this.menuWindowLayers.Click += new System.EventHandler(this.menuWindowLayers_Click);
            // 
            // menuWindowColors
            // 
            this.menuWindowColors.Index = 7;
            this.menuWindowColors.Shortcut = System.Windows.Forms.Shortcut.F8;
            this.menuWindowColors.Text = "&Colors";
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
                                                                                     this.menuSeparator7,
                                                                                     this.menuHelpAbout});
            this.menuHelp.Text = "&Help";
            // 
            // menuHelpHelpTopics
            // 
            this.menuHelpHelpTopics.Index = 0;
            this.menuHelpHelpTopics.Shortcut = System.Windows.Forms.Shortcut.F1;
            this.menuHelpHelpTopics.Text = "Help Topics";
            this.menuHelpHelpTopics.Click += new System.EventHandler(this.menuHelpHelpTopics_Click);
            // 
            // menuSeparator7
            // 
            this.menuSeparator7.Index = 1;
            this.menuSeparator7.Text = "-";
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Index = 2;
            this.menuHelpAbout.Text = "&About ...";
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
            this.populateEffectsTimer.Interval = 100;
            this.populateEffectsTimer.Tick += new EventHandler(populateEffectsTimer_Tick);
            // 
            // mruImageList
            // 
            this.mruImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.mruImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.mruImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(752, 670);
            this.Controls.Add(this.workspace);
            this.Controls.Add(this.statusBar);
            this.KeyPreview = true;
            this.Menu = this.mainMenu;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
            this.Text = "Paint.NET";
            this.Controls.SetChildIndex(this.statusBar, 0);
            this.Controls.SetChildIndex(this.workspace, 0);
            ((System.ComponentModel.ISupportInitialize)(this.contextStatusBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressStatusBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageInfoStatusBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cursorInfoStatusBar)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
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
            workspace.Environment.SelectedPathChanged += new EventHandler(Environment_SelectedPathChanged);
            workspace.DocumentView.Layout += new LayoutEventHandler(DocumentView_Layout);

            //BeginInvoke(new VoidVoidDelegate(PositionFloatingForms), null);
            //Application.Idle += new EventHandler(Application_Idle);
            PositionFloatingForms();

            // Set up icons -- defer this until they actually click on a menu though
            RegisterMenuPopupFirstTimeDelegates();

            if (splash != null)
            {
                splash.Close();
                splash.Dispose();
                splash = null;
            }

            base.OnLoad (e);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(Application_Idle);
            PositionFloatingForms();
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

            if (Settings.GetBoolean(PdnSettings.ToolsFormVisible, true))
            {
                workspace.Widgets.MainToolBarForm.Show();
            }

            if (Settings.GetBoolean(PdnSettings.ColorsFormVisible, true))
            {
                workspace.Widgets.ColorsForm.Show();
            }

            if (Settings.GetBoolean(PdnSettings.HistoryFormVisible, true))
            {
                workspace.Widgets.HistoryForm.Show();
            }

            if (Settings.GetBoolean(PdnSettings.LayersFormVisible, true))
            {
                workspace.Widgets.LayerForm.Show();
            }

            floaterOpacityTimer.Enabled = true;
            this.Focus();
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

        private void MenuPopupFirstTimeHandler(object sender, EventArgs e)
        {
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
                    Stream iconStream = Utility.GetResourceStream(iconFileName);

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
            return MessageBox.Show(this, "Do you want to save changes to " + GetFriendlyName() + "?", 
                Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
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
            ScanResult result = ScanningAndPrinting.Scan(out tempName);

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
                workspace.History.PushNewAction(new NullHistoryAction("Acquire Image", addNewLayerIcon));

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
                    pasted = Clipboard.GetDataObject();
                    image = (Image)pasted.GetData(DataFormats.Bitmap);
                }

                if (image == null)
                {
                    Utility.ErrorBox(this, "There is no acquirable image in the clipboard.");
                }
                else
                {
                    Document document = null;

                    try
                    {
                        using (new WaitCursorChanger(this))
                        {
                            document = Document.FromImage(image);
                            workspace.SetDocument(document);
                            workspace.SetDocumentSaveOptions(null, null, null);
                            //workspace.DocumentView.ScaleFactor = new ScaleFactor(1, 1);
                            workspace.History.ClearAll();
                            workspace.History.PushNewAction(new NullHistoryAction("Acquire Image", addNewLayerIcon));
                            Invalidate();
                        }
                    }

                    catch
                    {
                        Utility.ErrorBox(this, "There was an error transferring the image from the clipboard.");
                    }
                }
            }

            catch (ExternalException)
            {
                Utility.ErrorBox(this, "There was an error transferring the image from the clipboard.");
                return;
            }

            catch (OutOfMemoryException)
            {
                Utility.GCFullCollect();
                Utility.ErrorBox(this, "Not enough memory to acquire the image from the clipboard.");
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

        private void workspace_DocumentChanged(object sender, System.EventArgs e)
        {
            SetTitleText();

            if (workspace.Document != null)
            {
                this.imageInfoStatusBar.Text = workspace.Document.Width.ToString() + " x " + workspace.Document.Height.ToString();
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
                title = "Untitled";
            }

            return title;
        }

        private void SetTitleText()
        {
            string appTitle = PdnInfo.GetAppName();
            string ratio = string.Empty;
            string title = string.Empty;

            if (workspace == null) 
            {
                return;
            }

            if (this.WindowState != FormWindowState.Minimized) 
            {
                ratio = " (" + workspace.DocumentView.ScaleFactor + ")";
            }

            if (workspace.Document != null)
            {
                title = GetFriendlyName() + ratio + " - " + appTitle;
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
                nfd.NewWidth = newDocSize.Width;
                nfd.NewHeight = newDocSize.Height;
                
                if (IsClipboardImageAvailable())
                {
                    try
                    {
                        IDataObject clipData = Clipboard.GetDataObject();

                        using (Image clipImage = (Image)clipData.GetData(DataFormats.Bitmap))
                        {
                            nfd.NewHeight = clipImage.Height;
                            nfd.NewWidth = clipImage.Width;
                        }
                    }

                    catch (OutOfMemoryException)
                    {
                        Utility.GCFullCollect();
                    }

                    catch (ExternalException)
                    {
                    }
                }

                if (Utility.ShowDialog(nfd, this) == DialogResult.OK)
                {
                    CreateBlankDocument(new Size(nfd.NewWidth, nfd.NewHeight));
                }
            }
        }

        private void CreateBlankDocument(Size size)
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
                    Utility.GCFullCollect();
                    Utility.ErrorBox(this, "Not enough memory to create new image.");
                    return;
                }

                using (new WaitCursorChanger(this))
                {
                    untitled.Layers.Add(bitmapLayer);
                    workspace.SetDocument(untitled);
                    workspace.SetDocumentSaveOptions(null, null, null);
                    workspace.History.ClearAll();
                    workspace.History.PushNewAction(new NullHistoryAction("New Image", fileNewIcon));
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
            FileTypeCollection fileTypes = FileTypes.Collection;
            int ftIndex = fileTypes.IndexOfExtension(Path.GetExtension(fileName));
            fileTypeResult = null;

            if (ftIndex == -1)
            {
                Utility.ErrorBox(parent, "The image type is not recognized, and can not be opened.");
                return null;
            }

            FileType fileType = fileTypes[ftIndex];
            fileTypeResult = fileType;

            Document document = null;

            using (new WaitCursorChanger(parent))
            {
                Stream stream = null;

                try
                {
                    stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                    LoadProgressDialog ld = new LoadProgressDialog(parent, stream, fileType);

                    if (useStartPos)
                    {
                        document = ld.Load(progressDialogStartPos);
                    }
                    else
                    {
                        document = ld.Load();
                    }
                }

                catch (ArgumentException)
                {
                    if (fileName.Length == 0)
                    {
                        Utility.ErrorBox(parent, "The requested filename is blank.");
                    }
                    else
                    {
                        Utility.ErrorBox(parent, "There was an error opening the file.");
                    }
                }

                catch (UnauthorizedAccessException)
                {
                    Utility.ErrorBox(parent, "Access was denied (unauthorized access) to the requested file.");
                }
    
                catch (SecurityException)
                {
                    Utility.ErrorBox(parent, "Access was denied (security exception) to the requested file.");
                }

                catch (FileNotFoundException)
                {
                    Utility.ErrorBox(parent, "The file could not be found.");
                }

                catch (DirectoryNotFoundException)
                {
                    Utility.ErrorBox(parent, "The directory could not be found.");
                }

                catch (PathTooLongException)
                {
                    Utility.ErrorBox(parent, "The filename is too long.");
                }

                catch (IOException)
                {
                    Utility.ErrorBox(parent, "There was an error reading the file from the media.");
                }

                catch (SerializationException)
                {
                    Utility.ErrorBox(parent, "The file is corrupt or was saved with a newer version of Paint.NET.");
                }

                catch (OutOfMemoryException)
                {
                    Utility.GCFullCollect();
                    Utility.ErrorBox(parent, "Ran out of memory while trying to open the image.");
                }

                catch (Exception)
                {
                    Utility.ErrorBox(parent, "There was an unspecified error while opening the file.");
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
                        workspace.History.PushNewAction(new NullHistoryAction("Open Image", imageFromDiskIcon));
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
                if (workspace.Document.SavedWithVersion < new Version(2, 1, 1924))
                {
                    int fields = 2;

                    if (workspace.Document.SavedWithVersion >= new Version (2, 1, 0))
                    {
                        fields = 3;
                    }

                    Utility.InfoBox(this, "This image was saved with an older version of Paint.NET. " + 
                        "If you save it, it will not be readable by that older version.\r\n\r\n" + 
                        "Saved with: Paint.NET v" + workspace.Document.SavedWithVersion.ToString(fields) + "\r\n" +
                        "Current version: Paint.NET v" + new Version(Application.ProductVersion).ToString(fields)
                        );
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
                Image thumbInset = null;

                using (RenderArgs ra = new RenderArgs(renderSurface))
                {
                    workspace.Document.Render(ra, workspace.Document.Bounds);
                    thumbInset = ra.Bitmap.GetThumbnailImage(thumbSize.Width, thumbSize.Height, new Image.GetThumbnailImageAbort(MainForm.NullGetThumbnailImageAbort), IntPtr.Zero);
                }

                // Put it inside a square bitmap
                Bitmap thumb = new Bitmap(2 + edgeLength, 2 + edgeLength);

                using (Graphics thumbG = Graphics.FromImage(thumb))
                {
                    thumbG.Clear(Color.FromArgb(0, 0, 0, 0));

                    Rectangle dstRect = new Rectangle((thumb.Width - thumbSize.Width) / 2, 
                        (thumb.Height - thumbSize.Height) / 2, thumbSize.Width, thumbSize.Height);

                    thumbG.DrawImage(thumbInset, dstRect, 0, 0, thumbSize.Width, thumbSize.Height, GraphicsUnit.Pixel);
                    thumbG.DrawLines(Pens.Black, new Point[] { 
                                                                 new Point(dstRect.Left, dstRect.Top),
                                                                 new Point(dstRect.Right, dstRect.Top),
                                                                 new Point(dstRect.Right, dstRect.Bottom),
                                                                 new Point(dstRect.Left, dstRect.Bottom),
                                                                 new Point(dstRect.Left, dstRect.Top)
                                                             });

                    thumbInset.Dispose();
                }

                // Sharpen it
                Surface thumbSurface = Surface.CopyFromBitmap(thumb);

                using (RenderArgs ra = new RenderArgs(thumbSurface))
                {
                    new SharpenEffect().RenderInPlace(ra, thumbSurface.Bounds);
                }

                using (Bitmap bitmap = thumbSurface.CreateAliasedBitmap(thumbSurface.Bounds, true))
                {
                    using (Graphics thumbG = Graphics.FromImage(thumb))
                    {
                        thumbG.DrawImage(bitmap, new Point(0, 0));
                    }
                }

                thumbSurface.Dispose();

                renderSurface = null;
                MostRecentFile mrf = new MostRecentFile(fullFileName, thumb);

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
            FileTypeCollection fileTypes = FileTypes.Collection;

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

                ofd.Filter = fileTypes.ToString(true, "All images");
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
            return "Untitled";
        }

        private static string GetDefaultSavePath()
        {
            string myPics = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string dir = Settings.GetString(PdnSettings.LastFileDialogDirectory, null);

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
            initialDirectory = Settings.GetString(PdnSettings.LastFileDialogDirectory, fd.InitialDirectory);

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
                Settings.SetString(PdnSettings.LastFileDialogDirectory, newDir);
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

                    SaveConfigToken token = fileType.CreateDefaultSaveConfigToken();
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
                newSaveConfigToken = fileType.CreateDefaultSaveConfigToken();
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
            FileTypeCollection fileTypes = FileTypes.Collection;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.AddExtension = true;
                sfd.CheckPathExists = true;
                sfd.DefaultExt = string.Empty;
                sfd.OverwritePrompt = true;
                sfd.Filter = fileTypes.ToString(false, null);

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
                sfd.Title = "Save As ...";
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
            return MessageBox.Show(this,
                "Saving in this file format will \"flatten\" the image, discarding all layering information and hidden layers." + Environment.NewLine +
                "Proceed?",
                "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                    FileTypeCollection fileTypes = FileTypes.Collection;
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
                            if (!workspace.Environment.IsSelectionEmpty)
                            {
                                workspace.PerformAction(typeof(DeselectAction));
                            }

                            workspace.PerformAction(typeof(FlattenAction)); // TODO: shouldn't FlattenAction do the deselect for us?
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
                    PdnFileTypes.Gif.Save(thumbDoc, thumbGif, PdnFileTypes.Gif.CreateDefaultSaveConfigToken());
                    byte[] thumbBytes = thumbGif.ToArray();
                    string thumbString = Convert.ToBase64String(thumbBytes);
                    thumbDoc.Dispose();

                    string thumbXml = "<thumb gif=\"" + thumbString + "\" />";
                    workspace.Document.CustomHeaders = thumbXml;
                }

                // save!
                bool success = false;
                Stream stream = null;

                try 
                {
                    stream = (Stream)new FileStream(fileName, FileMode.Create, FileAccess.Write);

                    using (new WaitCursorChanger(this))
                    {
                        if (fileType is ISaveWithProgress)
                        {
                            SaveProgressDialog sd = new SaveProgressDialog(this);
                            sd.Save(stream, workspace.Document, fileType, saveConfigToken);
                        }
                        else
                        {
                            fileType.Save(workspace.Document, stream, saveConfigToken);
                        }

                        success = true;
                    }
                }

                catch (UnauthorizedAccessException)
                {
                    Utility.ErrorBox(this, "Access is denied. Use File->Save As to save under a different location or name.");
                }
    
                catch (SecurityException)
                {
                    Utility.ErrorBox(this, "Access is denied. Use File->Save As to save under a different location or name.");
                }

                catch (DirectoryNotFoundException)
                {
                    Utility.ErrorBox(this, "The directory could not be found. Use File->Save As to save to another location.");
                }

                catch (IOException)
                {
                    Utility.ErrorBox(this, "An I/O error occurred when writing to '" + fileName + "'.");
                }

                catch (OutOfMemoryException)
                {
                    Utility.GCFullCollect();
                    Utility.ErrorBox(this, "Ran out of memory while trying to save the image.");
                }

#if !DEBUG
                catch
                {
                    Utility.ErrorBox(this, "There was an unspecified error while saving the file.");
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
            bool selection = !workspace.Environment.IsSelectionEmpty;

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
            if (workspace.Environment.IsSelectionEmpty)
            {
                return false;
            }

            try
            {
                PdnRegion selectionRegion = workspace.Environment.CreateSelectedRegion();
                PdnGraphicsPath selectionOutline = (PdnGraphicsPath)workspace.Environment.SelectedPath.Clone();
                BitmapLayer activeLayer = (BitmapLayer)workspace.ActiveLayer;
                RenderArgs renderArgs = new RenderArgs(activeLayer.Surface);
                IrregularSurface copyIrregularSurface = new IrregularSurface(renderArgs.Surface, selectionRegion);
                SurfaceForClipboard surfaceForClipboard = new SurfaceForClipboard(copyIrregularSurface, new GraphicsPathWrapper(selectionOutline));
                Rectangle selectionBounds = Utility.GetRegionBounds(selectionRegion);

                Surface copySurface = new Surface((int)selectionBounds.Width, (int)selectionBounds.Height);
                Bitmap copyBitmap = copySurface.CreateAliasedBitmap();
                Bitmap copyOpaqueBitmap = new Bitmap(copySurface.Width, copySurface.Height, PixelFormat.Format24bppRgb);

                using (Graphics copyBitmapGraphics = Graphics.FromImage(copyBitmap))
                {
                    copyBitmapGraphics.Clear(Color.White);
                }

                copyIrregularSurface.Draw(copySurface, -selectionBounds.X, -selectionBounds.Y);

                using (Graphics copyOpaqueBitmapGraphics = Graphics.FromImage(copyOpaqueBitmap)) 
                {
                    copyOpaqueBitmapGraphics.Clear(Color.White);
                    copyOpaqueBitmapGraphics.DrawImage(copyBitmap, 0, 0);
                }

                DataObject dataObject = new DataObject();

                using (new WaitCursorChanger(this))
                {
                    dataObject.SetData(DataFormats.Bitmap, copyOpaqueBitmap);
                    dataObject.SetData(surfaceForClipboard);
                }

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
                            Utility.ErrorBox(this, "There was an error copying the image to the clipboard.");
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
                copyIrregularSurface.Dispose();
                copySurface.Dispose();
                copyBitmap.Dispose();
                copyOpaqueBitmap.Dispose();
            }

            catch (OutOfMemoryException)
            {
                success = false;
                Utility.GCFullCollect();
                Utility.ErrorBox(this, "Not enough memory to complete the clipboard operation.");
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
            if (!workspace.Environment.IsSelectionEmpty)
            {
                if (CopySelectionToClipboard()) 
                {
                    workspace.PerformAction(typeof(EraseSelectionAction), "Cut", editCutIcon);
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
            workspace.DocumentView.EnableOutlineAnimation = true;
        }

        private void StartingRenderingHandler(object sender, EventArgs e)
        {
            this.InvokeResetProgressStatusBar();
            workspace.DocumentView.EnableOutlineAnimation = false;

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

        private void DoEffect(Effect effect, EffectConfigToken token, PdnRegion renderRegion, Surface originalSurface)
        {
            workspace.DocumentView.EnableOutlineAnimation = false;

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
                        aed.Icon = stopWatchIcon;
                    }

                    aed.Opacity = 0.9;
                    aed.Value = 0;
                    aed.Text = "Applying " + effect.Name;
                    aed.Description = "Applying " + effect.Name + ":";

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
                                ha = new BitmapHistoryAction(effect.Name, effect.Image, this.workspace, this.workspace.ActiveLayerIndex, renderRegion);
                            }

                            BackgroundEffectRenderer ber = new BackgroundEffectRenderer(
                                effect,
                                token,
                                new RenderArgs(((BitmapLayer)workspace.ActiveLayer).Surface),
                                new RenderArgs(originalSurface),
                                renderRegion,
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

                        using (PdnRegion simplifiedRenderRegion = Utility.SimplifyAndInflateRegion(renderRegion))
                        {
                            using (new WaitCursorChanger(this))
                            {
                                workspace.ActiveLayer.Invalidate(simplifiedRenderRegion);
                            }
                        }

                        using (new WaitCursorChanger(this))
                        {
                            if (result == DialogResult.OK && ha != null)
                            {
                                workspace.History.PushNewAction(ha);
                                workspace.Update();
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
                workspace.DocumentView.EnableOutlineAnimation = true;
            }
        }
        #endregion

        private void menuEffects_ClickHandler(object sender, System.EventArgs e)
        {
            effectsKeyboardHackEnabled = false;

            this.Update(); // make sure the window is done 'closing'
            this.InvokeResetProgressStatusBar();

            PdnRegion selectedRegion;

            if (workspace.Environment.IsSelectionEmpty)
            {
                selectedRegion = new PdnRegion(workspace.Document.Bounds);
            }
            else
            {
                selectedRegion = workspace.Environment.CreateSelectedRegion();
                selectedRegion.Intersect(workspace.Document.Bounds);
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
                    string repeatName = "Repeat " + effect.Name;

                    if (effect is IConfigurableEffect)
                    {
                        name += "...";
                    }
                
                    if (repeatName == ((MenuItem)sender).Text)
                    {
                        Surface copy = workspace.ScratchSurface;

                        using (new WaitCursorChanger(this))
                        {
                            copy.CopySurface(((BitmapLayer)workspace.ActiveLayer).Surface);
                        }

                        ((Effect)lastEffect).EnvironmentParameters = eep;
                        DoEffect((Effect)lastEffect, (EffectConfigToken)lastEffectToken, selectedRegion, copy);
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

                            DoEffect(effect, null, selectedRegion, copy);
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

                                    ((BitmapLayer)workspace.ActiveLayer).Surface.CopySurface(originalSurface);
                                    workspace.ActiveLayer.Invalidate();
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
                                    newLastToken = (EffectConfigToken)configDialog.EffectToken.Clone();
                                    this.ResetProgressStatusBar();
                                    DoEffect(effect, newLastToken, selectedRegion, originalSurface);
                                }
                                else
                                {

                                    using (new WaitCursorChanger(this))
                                    {
                                        workspace.ActiveLayer.Invalidate();
                                        Utility.GCFullCollect();
                                    }

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
                workspace.DocumentView.EnableOutlineAnimation = true;
                workspace.Environment.SetTool(oldTool, workspace);
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
                string menuName = "Repeat " + ((Effect)lastEffect).Name;
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
                    name += "...";
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
                clipData = Clipboard.GetDataObject();
            }

            catch (ExternalException)
            {
                Utility.ErrorBox(this, "There was an error transferring the image from the clipboard.");
                return false;
            }

            catch (OutOfMemoryException)
            {
                Utility.GCFullCollect();
                Utility.ErrorBox(this, "Not enough memory to perform Paste.");
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
                    surfaceForClipboard = (SurfaceForClipboard)clipData.GetData(typeof(SurfaceForClipboard));
                }

                catch (OutOfMemoryException)
                {
                    Utility.GCFullCollect();
                    Utility.ErrorBox(this, "Not enough memory to perform Paste.");
                    return false;
                }
            }

            if (surfaceForClipboard == null && clipData.GetDataPresent(DataFormats.Bitmap))
            {
                Image image;
                
                try
                {
                    image = (Image)clipData.GetData(DataFormats.Bitmap);
                }

                catch (OutOfMemoryException)
                {
                    Utility.GCFullCollect();
                    Utility.ErrorBox(this, "Not enough memory to perform Paste.");
                    return false;
                }

                // Sometimes we get weird errors if we're in, say, 16-bit mode but the image was copied
                // to the clipboard in 32-bit mode
                if (image == null)
                {
                    Utility.ErrorBox(this, "The image in the clipboard couldn't be recognized. Try re-copying it with the original application that was used to acquire it.");
                    return false;
                }

                Surface surface = null;
                IrregularSurface irregularSurface = null;

                try
                {
                    Bitmap bitmap = new Bitmap(image);
                    image.Dispose();
                    surface = Surface.CopyFromBitmap(bitmap);
                    bitmap.Dispose();
                    irregularSurface = new IrregularSurface(surface, surface.Bounds);
                }

                catch (OutOfMemoryException)
                {
                    Utility.GCFullCollect();
                    Utility.ErrorBox(this, "Not enough memory to perform Paste.");
                    return false;
                }

                PdnGraphicsPath path = new PdnGraphicsPath();
                path.AddRectangle(new Rectangle(0, 0, surface.Width, surface.Height));
                path.CloseFigure();

                GraphicsPathWrapper gpw = new GraphicsPathWrapper(path);
                surfaceForClipboard = new SurfaceForClipboard(irregularSurface, gpw);
            }

            if (surfaceForClipboard == null || surfaceForClipboard.Surface == null)
            {   // silently fail: like what if a program overwrote the clipboard in between the time
                // we enabled the "Paste" menu item and the user actually clicked paste?
                // it could happen!
                Utility.ErrorBox(this, "The clipboard doesn't contain an image.");
                return false;
            }

            // If the image is larger than the document, ask them if they'd like to make the image larger first
            Rectangle bounds = Utility.GetRegionBounds(surfaceForClipboard.Surface.Region);

            if (bounds.Width > workspace.Document.Width ||
                bounds.Height > workspace.Document.Height)
            {
                DialogResult dr = MessageBox.Show(this, "The image being pasted is larger than the image canvas.\nExpand canvas to fit pasted image?", PdnInfo.GetAppName(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                int layerIndex = workspace.Document.Layers.IndexOf(workspace.ActiveLayer);

                switch (dr)
                {
                    case DialogResult.Yes:
                        Size newSize = new Size(Math.Max(bounds.Width, workspace.Document.Width),
                                                Math.Max(bounds.Height, workspace.Document.Height));

                        Document newDoc = CanvasSizeAction.ResizeDocument(this, workspace.Document, newSize, AnchorEdge.TopLeft, workspace.Environment.BackColor);

                        if (newDoc == null)
                        {
                            return false; // user clicked cancel!
                        }
                        else
                        {
                            HistoryAction rdha = new ReplaceDocumentHistoryAction("Canvas Size", null, workspace);
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
                        throw new InvalidEnumArgumentException("Internal error: DialogResult was no Yes, No, or Cancel");
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

            workspace.Widgets.MainToolBar.SelectTool(typeof(MoveTool));

            ((MoveTool)workspace.Environment.Tool).PasteMouseDown(surfaceForClipboard, pasteOffset);

            if (doPan)
            {
                Point centerPtView = new Point(visibleDocRect.Left + (visibleDocRect.Width / 2),
                                               visibleDocRect.Top + (visibleDocRect.Height / 2));

                Point centerPtPasted = new Point(bounds2.Left + (bounds2.Width / 2),
                                                 bounds2.Top + (bounds2.Height / 2));

                Size delta = new Size(centerPtPasted.X - centerPtView.X,
                                      centerPtPasted.Y - centerPtView.Y);

                PointF docScrollPos = workspace.DocumentView.DocumentScrollPosition;

                PointF newDocScrollPos = new PointF(docScrollPos.X + (float)delta.Width,
                                                    docScrollPos.Y + (float)delta.Height);

                workspace.DocumentView.DocumentScrollPosition = newDocScrollPos;
            }

            return true;
        }

        private void menuLayersAddNewLayer_Click(object sender, System.EventArgs e)
        {
            try
            {
                NewLayerHistoryAction nlha = workspace.AddNewLayerToDocument();
            }

            catch (OutOfMemoryException)
            {
                Utility.GCFullCollect();
                Utility.ErrorBox(this, "Not enough memory to create a new layer.");
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
                DialogResult result = MessageBox.Show(this, "Discard hidden layers?", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

                this.Focus();

                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            if (!workspace.Environment.IsSelectionEmpty)
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
            menuImageCrop.Enabled = !workspace.Environment.IsSelectionEmpty;
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
            menuToolsAntiAliasing.Checked = workspace.Environment.AntiAliasing;
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
            if (!workspace.Environment.IsSelectionEmpty)
            {
                workspace.PerformAction(typeof(DeselectAction));
            }
        }

        private void menuItem5_Click(object sender, System.EventArgs e)
        {
            int x = 0;
            ++x;
        }

        private void DocumentView_DocumentMouseMove(object sender, MouseEventArgs e)
        {
            this.cursorInfoStatusBar.Text = e.X.ToString() + " , " + e.Y.ToString();
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

        private void ResetProgressStatusBar()
        {
            this.progressStatusBar.Text = "0%";

            if (this.progressStatusBar.Icon == null)
            {
                this.progressStatusBar.Icon = stopWatchIcon;
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
            if (percent > 100)
            {
                int x = 5; ++x;
            }

            lock (progressStatusBar)
            {
                if (GetProgressStatusBarValue() < percent)
                {
                    this.progressStatusBar.Text = ((int)percent).ToString() + "%";

                    if (this.progressStatusBar.Icon == null)
                    {
                        this.progressStatusBar.Icon = stopWatchIcon;
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

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!e.Handled)
            {
                if (workspace.DocumentView.ContainsFocus && 
                    workspace.Environment.Tool != null)
                {
                    workspace.Environment.Tool.PerformKeyPress(e);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown (e);

            if (!effectsPopulated)
            {
                PopulateEffectsMenu();
            }

            if (!adjustmentsPopulated)
            {
                PopulateAdjustmentsMenu();
            }

            if (!e.Handled)
            {
                if (workspace.DocumentView.ContainsFocus && 
                    workspace.Environment.Tool != null)
                {
                    workspace.Environment.Tool.PerformKeyDown(e);
                }
            }
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

                if (!e.Handled)
                {
                    if (workspace.DocumentView.ContainsFocus && 
                        workspace.Environment.Tool != null)
                    {
                        workspace.Environment.Tool.PerformKeyUp(e);
                    }
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (Utility.IsArrowKey(keyData) && 
                (keyData & Keys.Modifiers) == 0)
            {
                KeyEventArgs kea = new KeyEventArgs(keyData);

                // We only need to intercept WM_KEYDOWN because WM_KEYUP is already handled
                // Also, don't do it if there's a modifier key
                switch (msg.Msg)
                {
                    case NativeMethods.WmConstants.WM_KEYDOWN:
                        if (workspace.DocumentView.ContainsFocus && 
                            workspace.Environment.Tool != null)
                        {
                            workspace.Environment.Tool.PerformKeyDown(kea);
                        }
                        return kea.Handled;

                    //case NativeMethods.WmConstants.WM_KEYUP:
                        //this.OnKeyUp(kea);
                        //return kea.Handled;
                }
            }
            return base.ProcessCmdKey (ref msg, keyData);
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

        private void Environment_SelectedPathChanged(object sender, EventArgs e)
        {
            if (workspace.Environment.IsSelectionEmpty)
            {
                this.contextStatusBar.Text = string.Empty;
                this.contextStatusBar.Icon = null;
            }
            else
            {
                int area = 0;
                Rectangle bounds;

                using (PdnRegion selection = workspace.Environment.CreateSelectedRegion())
                {
                    bounds = Utility.GetRegionBounds(selection);
                    area = selection.GetArea();
                }

                NumberFormatInfo nfi = new CultureInfo(CultureInfo.CurrentCulture.LCID).NumberFormat;

                nfi.NumberDecimalDigits = 0;

                this.contextStatusBar.Text = 
                    "Selected area: " + 
                    bounds.Width.ToString() +
                    " x " +
                    bounds.Height.ToString() +
                    " (" +
                    area.ToString("N", nfi) + 
                    " pixels)";

                if (this.contextStatusBar.Icon == null)
                {
                    this.contextStatusBar.Icon = selectionIcon;
                }
            }
        }

        private void floaterOpacityTimer_Tick(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
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

            if (floaters == null)
            {
                return;
            }

            if (!PdnBaseForm.EnableOpacity)
            {
                return;
            }

            for (int i = 0; i < floaters.Length; ++i)
            {
                FloatingToolForm ftf = floaters[i];

                Rectangle intersect = Rectangle.Intersect(screenDocRect, ftf.Bounds);
                double opacity = -1.0;

                try
                {
                    if (intersect.IsEmpty ||
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
                Utility.ErrorBox(this, "Not enough memory to create a new layer.");
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
                Utility.ErrorBox(this, "To use this feature you must enable the Windows Image Acquisition system service.");
            }
            else
            {
                Utility.ErrorBox(this, "This feature requires Windows XP SP1 or later.");
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

        private void menuFile_Popup(object sender, System.EventArgs e)
        {
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
                    string[] choices = new string[] { "Open it as a new image", "Import it as new layer(s) into the current image" };

                    if (DialogResult.OK != PdnMessageBox.Show("What would you like to do with this file?", 
                        Application.ProductName, choices, ref result, this, layersImportFromFileIcon))
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
                    DialogResult result = Utility.AskOKCancel(this, "Would you like to import these files as new layers into the current image?");

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
                this.invalidateTimer.Interval = 25;
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
                MenuItem none = new MenuItem("(none)");
                none.Enabled = false;
                mruDotNetMenuProvider.SetDrawSpecial(none, true);
                menuFileOpenRecent.MenuItems.Add(none);
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

        private DialogResult AskForResize(Size fitToMe)
        {
            DialogResult dr = MessageBox.Show(workspace, "The image being imported is larger than the image canvas." + Environment.NewLine + 
                                                         "Expand canvas to fit imported image?", PdnInfo.GetAppName(), 
                                                         MessageBoxButtons.YesNoCancel, 
                                                         MessageBoxIcon.Question);

            int layerIndex = workspace.Document.Layers.IndexOf(workspace.ActiveLayer);

            switch (dr)
            {
                case DialogResult.Yes:
                    Size newSize = new Size(Math.Max(fitToMe.Width, workspace.Document.Width),
                        Math.Max(fitToMe.Height, workspace.Document.Height));

                    Document newDoc = CanvasSizeAction.ResizeDocument(workspace, workspace.Document, newSize, AnchorEdge.TopLeft, workspace.Environment.BackColor);

                    if (newDoc == null)
                    {
                        return DialogResult.Cancel; // user clicked cancel!
                    }
                    else
                    {
                        HistoryAction rdha = new ReplaceDocumentHistoryAction("Canvas Size", null, workspace);
                        workspace.SetDocument(newDoc);
                        workspace.History.PushNewAction(rdha);
                        workspace.ActiveLayer = (Layer)workspace.Document.Layers[layerIndex];
                    }

                    return DialogResult.Yes;

                case DialogResult.No:
                    return DialogResult.No;

                case DialogResult.Cancel:
                    return DialogResult.Cancel;

                default:
                    throw new InvalidEnumArgumentException("Internal error: DialogResult was not Yes, No, or Cancel");
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
            menuViewZoomToSelection.Enabled = !workspace.Environment.IsSelectionEmpty;
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
                            if (!workspace.Environment.IsSelectionEmpty)
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
            PopulateEffectsAndAdjustmentsMenus();
            populateEffectsTimer.Tick -= new EventHandler(populateEffectsTimer_Tick);
            populateEffectsTimer.Dispose();
            populateEffectsTimer = null;
        }

        private void ftf_VisibleChanged(object sender, EventArgs e)
        {
            workspace.Focus();
        }
    }
}
