using PaintDotNet.Effects;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for MainForm.
    /// </summary>
    public class MainForm 
        : PaintDotNet.PdnBaseForm
    {
        private const int tilesPerCpu = 50;

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
        private readonly FileTypeCollection fileTypes = InitFileTypes();
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
        private System.Windows.Forms.MenuItem menuItem2;
        private EventHandler menuEffectsClickDelegate;
        private EventHandler menuToolsClickDelegate;
        private CancelEventHandler hideInsteadOfCloseDelegate;

        private Effect lastEffect = null;
        private System.Windows.Forms.MenuItem menuEditRedo;
        private System.Windows.Forms.MenuItem menuDebug;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuImageFlip;
        private System.Windows.Forms.MenuItem menuImageFlipHorizontal;
        private System.Windows.Forms.MenuItem menuImageFlipVertical;
        private System.Windows.Forms.MenuItem menuLayersFlip;
        private System.Windows.Forms.MenuItem menuLayersFlipHorizontal;
        private System.Windows.Forms.MenuItem menuLayersFlipVertical;
        private System.Windows.Forms.MenuItem menuLayersFlattenImage;
        private EffectConfigToken lastEffectToken = null;
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
        private System.Windows.Forms.MenuItem menuImageZoomIn;
        private System.Windows.Forms.MenuItem menuImageZoomOut;
        private System.Windows.Forms.MenuItem menuImageRotate;
        private System.Windows.Forms.MenuItem menuImageRotate90CW;
        private System.Windows.Forms.MenuItem menuImageRotate180CW;
        private System.Windows.Forms.MenuItem menuImageRotate270CW;
        private System.Windows.Forms.MenuItem menuImageRotate90CCW;
        private System.Windows.Forms.MenuItem menuImageRotate180CCW;
        private System.Windows.Forms.MenuItem menuImageRotate270CCW;
        private System.Windows.Forms.MenuItem menuSeparator8;
        private System.Windows.Forms.MenuItem menuSeparator9;
        private System.Windows.Forms.StatusBarPanel contextStatusBar;
        private System.Windows.Forms.Timer floaterOpacityTimer;
        private FloatingToolForm[] floaters;
        private System.Windows.Forms.MenuItem menuImageActualSize;
        private System.Windows.Forms.MenuItem menuEditEraseSelection;
        private System.Windows.Forms.MenuItem menuEditPasteInToNewLayer;
        private System.Windows.Forms.MenuItem menuFilePrint;
        private System.Windows.Forms.MenuItem menuItem10;
        private System.Windows.Forms.Timer invalidateTimer;
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

        private SplashForm splash = null;

        public MainForm()
            : this(new string[0])
        {
        }

        public MainForm(string[] args)
        {
            Utility.TraceMe();

            this.StartPosition = FormStartPosition.WindowsDefaultLocation;

            bool noSplash = false;
            string fileName = null;

            // Parse command line arguments
            foreach (string argument in args)
            {
                if (0 == string.Compare(argument, "/nosplash", true))
                {
                    noSplash = true;
                }
                else
                {
                    fileName = argument;
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
            Utility.TraceMe("shown splash form, now calling InitializeComponent");
            InitializeComponent();
            Utility.TraceMe("done with InitializeComponent()");

            Utility.TraceMe("turning on DNMP");
            this.dotNetMenuProvider = new DotNetWidgets.DotNetMenuProvider();
            this.components.Add(dotNetMenuProvider);
            dotNetMenuProvider.ImageList = this.menuImages;
            TurnOnSpecialDrawing();
            Utility.TraceMe("done");

            workspace.DocumentView.ScaleFactorChanged += new EventHandler(DocumentView_ScaleFactorChanged);
            this.mruImageList.ImageSize = new System.Drawing.Size(2 + MainForm.mruIconSize, 2 + MainForm.mruIconSize);

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            components = null;


            Utility.TraceMe("1");
            this.Icon = new Icon(Utility.GetResourceStream("PaintDotNet.ico"));

            Utility.TraceMe("2");
            menuEffectsClickDelegate = new EventHandler(menuEffects_ClickHandler);
            menuToolsClickDelegate = new EventHandler(menuTools_ClickHandler);
            hideInsteadOfCloseDelegate = new CancelEventHandler(HideInsteadOfCloseHandler);
            Utility.TraceMe("3");

            //

            //CreateBlankDocument(GetNewDocumentSize());
            // open any file if they want it
            if (fileName != null)
            {
                DoOpenFile(fileName);
            }
            else
            {
                CreateBlankDocument(GetNewDocumentSize());
            }

            Utility.TraceMe("4");
            workspace.Document.Dirty = false;

            Utility.TraceMe("5");

            // WHen the user changes the display resolution, we need to do some fixing of our UI
            // like making sure our floaters are actually on screen
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);

#if !DEBUG
            menuDebug.Visible = false;
#endif

            // HACK: On many systems we get this annoying delay when we first start out drawing.
            // Apparently there is some initialization that only occurs the first time we start
            // using a tool. So we do some stuff here to get around it by simulating drawing
            // a little bit of stuff and then backing out of its side effects.
            // Note that this does not fix a "bug" per se, but an annoyance.
            // At this point it's assumed we only have one item on the history stack.
            // I think this is a JIT issue actually. Not really an "issue" so much as the way it works.
            Utility.TraceMe("jit start");
            HistoryAction ha = (HistoryAction)workspace.History.UndoStack.ToArray()[0];
            workspace.Environment.Tool.PerformMouseDown(new MouseEventArgs(MouseButtons.Left, 0, 1, 1, 0));
            workspace.Environment.Tool.PerformMouseMove(new MouseEventArgs(MouseButtons.Left, 0, 2, 2, 0));
            workspace.Environment.Tool.PerformMouseMove(new MouseEventArgs(MouseButtons.Left, 0, 2, 2, 0));
            workspace.Environment.Tool.PerformMouseUp(new MouseEventArgs(MouseButtons.Left, 0, 3, 3, 0));
            workspace.History.StepBackward();
            workspace.History.ClearAll();
            workspace.History.PushNewAction(ha);
            workspace.Document.Dirty = false;
            Utility.TraceMe("jit done");

            LoadWindowStateFromRegistry();
            SetupStatusBars();

            Utility.TraceMe("done");
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

        private void LoadWindowStateFromRegistry()
        {
            Utility.TraceMe("start");

            Microsoft.Win32.RegistryKey key = CreateSettingsKey();

            // save the old values so we can restore them in the event of a parsing error
            int oldWidth = this.Width;
            int oldHeight = this.Height;
            int oldTop = this.Top;
            int oldLeft = this.Left;
            FormWindowState oldFws = this.WindowState;

            try
            {
                FormWindowState fws = (FormWindowState)Enum.Parse(typeof(FormWindowState), (string)key.GetValue("WindowState", WindowState.ToString()), true);

                if (fws != FormWindowState.Minimized)
                {
                    this.WindowState = fws;

                    if (fws != FormWindowState.Maximized)
                    {
                        this.Width = int.Parse((string)key.GetValue("Width", this.Width.ToString()));
                        this.Height = int.Parse((string)key.GetValue("Height", this.Height.ToString()));
                        this.Top = int.Parse((string)key.GetValue("Top", this.Top.ToString()));
                        this.Left = int.Parse((string)key.GetValue("Left", this.Left.ToString()));
                    }
                }
            }

            catch
            {
                key.DeleteValue("Width", false);
                key.DeleteValue("Height", false);
                key.DeleteValue("WindowState", false);
                key.DeleteValue("Top", false);
                key.DeleteValue("Left", false);

                this.Width = oldWidth;
                this.Height = oldHeight;
                this.WindowState = oldFws;
                this.Top = oldTop;
                this.Left = oldLeft;
            }

            Utility.TraceMe("leave");
        }

        private void LoadMruList()
        {
            LoadMruList(CreateSettingsKey());
        }

        private void LoadMruList(Microsoft.Win32.RegistryKey key)
        {
            // Load the most recent files
            try
            {
                int max = int.Parse((string)key.GetValue("MRUMax", MainForm.defaultMostRecentFilesMax.ToString()));
                this.mostRecentFiles = new MostRecentFiles(max);

                for (int i = 0; i < this.mostRecentFiles.MaxCount; ++i)
                {
                    try
                    {
                        string fileName = (string)key.GetValue("MRU" + i.ToString());

                        if (fileName != null)
                        {
                            string thumbB64 = (string)key.GetValue("MRU" + i.ToString() + "Thumb");

                            if (thumbB64 != null)
                            {
                                byte[] pngBytes = Convert.FromBase64String(thumbB64);
                                MemoryStream ms = new MemoryStream(pngBytes);
                                Image thumb = Image.FromStream(ms);

                                if (fileName != null && thumb != null)
                                {
                                    MostRecentFile mrf = new MostRecentFile(fileName, thumb);
                                    mostRecentFiles.Add(mrf);
                                }
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

        private void SaveWindowStateToRegistry()
        {
            Microsoft.Win32.RegistryKey key = CreateSettingsKey();

            key.SetValue("Width", this.Width.ToString());
            key.SetValue("Height", this.Height.ToString());
            key.SetValue("Top", this.Top.ToString());
            key.SetValue("Left", this.Left.ToString());
            key.SetValue("WindowState", this.WindowState.ToString());

            SaveMruList(key);
        }

        private void SaveMruList()
        {
            SaveMruList(CreateSettingsKey());
        }

        private void SaveMruList(Microsoft.Win32.RegistryKey key)
        {
            if (mostRecentFiles == null)
            {
                return;
            }

            key.SetValue("MRUMax", this.mostRecentFiles.MaxCount.ToString());
            MostRecentFile[] mrfArray = mostRecentFiles.GetFileList();

            for (int i = 0; i < mrfArray.Length; ++i)
            {
                MostRecentFile mrf = mrfArray[i];

                key.SetValue("MRU" + i.ToString(), (string)mrf.FileName);

                Bitmap thumb = (Bitmap)mrf.Thumb;
                MemoryStream ms = new MemoryStream();
                thumb.Save(ms, ImageFormat.Png);
                key.SetValue("MRU" + i.ToString() + "Thumb", System.Convert.ToBase64String(ms.GetBuffer()));
            }
        }

        private Microsoft.Win32.RegistryKey CreateSettingsKey()
        {
            Microsoft.Win32.RegistryKey rootKey = Microsoft.Win32.Registry.CurrentUser;
            Microsoft.Win32.RegistryKey softwareKey = rootKey.CreateSubKey("SOFTWARE");
            Microsoft.Win32.RegistryKey pdnKey = softwareKey.CreateSubKey("Paint.NET");
            return pdnKey;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged (e);
        }

        private void SetupStatusBars()
        {
            // cursorInfo (x,y info)
            this.cursorInfoStatusBar.Icon = new Icon(Utility.GetResourceStream("Icons.CursorXYIcon.ico"));
            this.cursorInfoStatusBar.Text = string.Empty;
            this.workspace.DocumentView.DocumentMouseMove += new MouseEventHandler(DocumentView_DocumentMouseMove);
            
            // imageInfo (width,height info)
            this.imageInfoStatusBar.Icon = new Icon(Utility.GetResourceStream("Icons.ImageSizeIcon.ico"));
            //this.imageInfoStatusBar.Text = string.Empty;
            this.workspace.DocumentChanged += new EventHandler(workspace_DocumentChanged);

            // progress
            this.progressStatusBar.Text = string.Empty;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing (e);

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

            SaveWindowStateToRegistry();
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
        //

        private static FileTypeCollection InitFileTypes()
        {
            ArrayList ft = new ArrayList();

            ft.Add(FileTypes.Bmp);
            ft.Add(FileTypes.Pdn);
            ft.Add(FileTypes.Jpeg);
            ft.Add(FileTypes.Png);
            ft.Add(FileTypes.Tiff);
            ft.Add(FileTypes.Gif);

            return new FileTypeCollection(ft);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
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
            this.menuSeparator9 = new System.Windows.Forms.MenuItem();
            this.menuImageZoomIn = new System.Windows.Forms.MenuItem();
            this.menuImageZoomOut = new System.Windows.Forms.MenuItem();
            this.menuImageActualSize = new System.Windows.Forms.MenuItem();
            this.menuLayers = new System.Windows.Forms.MenuItem();
            this.menuLayersAddNewLayer = new System.Windows.Forms.MenuItem();
            this.menuLayersDeleteLayer = new System.Windows.Forms.MenuItem();
            this.menuLayersDuplicateLayer = new System.Windows.Forms.MenuItem();
            this.menuSeparator5 = new System.Windows.Forms.MenuItem();
            this.menuLayersAdjustments = new System.Windows.Forms.MenuItem();
            this.menuItem17 = new System.Windows.Forms.MenuItem();
            this.menuLayersFlattenImage = new System.Windows.Forms.MenuItem();
            this.menuLayersFlip = new System.Windows.Forms.MenuItem();
            this.menuLayersFlipHorizontal = new System.Windows.Forms.MenuItem();
            this.menuLayersFlipVertical = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuLayersLayerProperties = new System.Windows.Forms.MenuItem();
            this.menuEffects = new System.Windows.Forms.MenuItem();
            this.menuEffectsSentinel = new System.Windows.Forms.MenuItem();
            this.menuTools = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuWindow = new System.Windows.Forms.MenuItem();
            this.menuWindowResetWindowLocations = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuWindowTools = new System.Windows.Forms.MenuItem();
            this.menuWindowHistory = new System.Windows.Forms.MenuItem();
            this.menuWindowLayers = new System.Windows.Forms.MenuItem();
            this.menuWindowColors = new System.Windows.Forms.MenuItem();
            this.menuDebug = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
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
            // menuImage
            // 
            this.menuImage.Index = 2;
            this.menuImage.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuImageCrop,
                                                                                      this.menuImageResize,
                                                                                      this.menuImageCanvasSize,
                                                                                      this.menuSeparator8,
                                                                                      this.menuImageFlip,
                                                                                      this.menuImageRotate,
                                                                                      this.menuSeparator9,
                                                                                      this.menuImageZoomIn,
                                                                                      this.menuImageZoomOut,
                                                                                      this.menuImageActualSize});
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
            this.menuImageResize.Text = "&Resize ...";
            this.menuImageResize.Click += new System.EventHandler(this.menuImageResize_Click);
            // 
            // menuImageCanvasSize
            // 
            this.menuImageCanvasSize.Index = 2;
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
            // menuSeparator9
            // 
            this.menuSeparator9.Index = 6;
            this.menuSeparator9.Text = "-";
            // 
            // menuImageZoomIn
            // 
            this.menuImageZoomIn.Index = 7;
            this.menuImageZoomIn.Shortcut = System.Windows.Forms.Shortcut.CtrlJ;
            this.menuImageZoomIn.Text = "Zoom In";
            this.menuImageZoomIn.Click += new System.EventHandler(this.menuImageZoomIn_Click);
            // 
            // menuImageZoomOut
            // 
            this.menuImageZoomOut.Index = 8;
            this.menuImageZoomOut.Shortcut = System.Windows.Forms.Shortcut.CtrlK;
            this.menuImageZoomOut.Text = "Zoom Out";
            this.menuImageZoomOut.Click += new System.EventHandler(this.menuImageZoomOut_Click);
            // 
            // menuImageActualSize
            // 
            this.menuImageActualSize.Index = 9;
            this.menuImageActualSize.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftA;
            this.menuImageActualSize.Text = "Actual Size";
            this.menuImageActualSize.Click += new System.EventHandler(this.menuImageActualSize_Click);
            // 
            // menuLayers
            // 
            this.menuLayers.Index = 3;
            this.menuLayers.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                       this.menuLayersAddNewLayer,
                                                                                       this.menuLayersDeleteLayer,
                                                                                       this.menuLayersDuplicateLayer,
                                                                                       this.menuSeparator5,
                                                                                       this.menuLayersAdjustments,
                                                                                       this.menuLayersFlattenImage,
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
            this.menuLayersDeleteLayer.Text = "De&lete Layer";
            this.menuLayersDeleteLayer.Click += new System.EventHandler(this.menuLayersDeleteLayer_Click);
            // 
            // menuLayersDuplicateLayer
            // 
            this.menuLayersDuplicateLayer.Index = 2;
            this.menuLayersDuplicateLayer.Text = "&Duplicate Layer";
            this.menuLayersDuplicateLayer.Click += new System.EventHandler(this.menuLayersDuplicateLayer_Click);
            // 
            // menuSeparator5
            // 
            this.menuSeparator5.Index = 3;
            this.menuSeparator5.Text = "-";
            // 
            // menuLayersAdjustments
            // 
            this.menuLayersAdjustments.Index = 4;
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
            // menuLayersFlattenImage
            // 
            this.menuLayersFlattenImage.Index = 5;
            this.menuLayersFlattenImage.Text = "&Flatten Image";
            this.menuLayersFlattenImage.Click += new System.EventHandler(this.menuLayersFlattenImage_Click);
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
            this.menuLayersLayerProperties.Text = "Layer &Properties ...";
            this.menuLayersLayerProperties.Click += new System.EventHandler(this.menuLayersLayerProperties_Click);
            // 
            // menuEffects
            // 
            this.menuEffects.Index = 4;
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
            this.menuTools.Index = 5;
            this.menuTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem2});
            this.menuTools.Text = "&Tools";
            this.menuTools.Popup += new System.EventHandler(this.menuTools_Popup);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "sentinel";
            // 
            // menuWindow
            // 
            this.menuWindow.Index = 6;
            this.menuWindow.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                       this.menuWindowResetWindowLocations,
                                                                                       this.menuItem7,
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
            // menuWindowTools
            // 
            this.menuWindowTools.Index = 2;
            this.menuWindowTools.Text = "&Tools";
            this.menuWindowTools.Click += new System.EventHandler(this.menuWindowTools_Click);
            // 
            // menuWindowHistory
            // 
            this.menuWindowHistory.Index = 3;
            this.menuWindowHistory.Text = "&History";
            this.menuWindowHistory.Click += new System.EventHandler(this.menuWindowHistory_Click);
            // 
            // menuWindowLayers
            // 
            this.menuWindowLayers.Index = 4;
            this.menuWindowLayers.Text = "&Layers";
            this.menuWindowLayers.Click += new System.EventHandler(this.menuWindowLayers_Click);
            // 
            // menuWindowColors
            // 
            this.menuWindowColors.Index = 5;
            this.menuWindowColors.Text = "&Colors";
            this.menuWindowColors.Click += new System.EventHandler(this.menuWindowColors_Click);
            // 
            // menuDebug
            // 
            this.menuDebug.Index = 7;
            this.menuDebug.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem1,
                                                                                      this.menuItem3,
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
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.Text = "Walk Object Graph to c:\\walk.txt";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 2;
            this.menuItem4.Text = "GC.Collect";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 3;
            this.menuItem5.Text = "Breakpoint";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 4;
            this.menuItem6.Text = "Resposition floaters";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 5;
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
            this.menuHelp.Index = 8;
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
            // progressStatusBar
            // 
            this.progressStatusBar.Text = "Status updates for long operations (Effect renderings in particular) should go he" +
                "re";
            // 
            // imageInfoStatusBar
            // 
            this.imageInfoStatusBar.Text = "Simple image info (width, height) goes here";
            // 
            // cursorInfoStatusBar
            // 
            this.cursorInfoStatusBar.Text = "Cursor info (x,y) goes here";
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
            this.floaterOpacityTimer.Enabled = true;
            this.floaterOpacityTimer.Interval = 25;
            this.floaterOpacityTimer.Tick += new System.EventHandler(this.floaterOpacityTimer_Tick);
            // 
            // invalidateTimer
            // 
            this.invalidateTimer.Interval = 25;
            this.invalidateTimer.Tick += new System.EventHandler(this.invalidateTimer_Tick);
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
            Utility.TraceMe();

            base.OnLoad (e);

            this.floaters = new FloatingToolForm[] { 
                                                       workspace.Widgets.LayerForm,
                                                       workspace.Widgets.HistoryForm,
                                                       workspace.Widgets.MainToolBarForm,
                                                       workspace.Widgets.ColorsForm
                                                   };

            foreach (FloatingToolForm ftf in floaters)
            {
                ftf.Closing += this.hideInsteadOfCloseDelegate;
                ftf.KeyDown += new KeyEventHandler(FloatingForm_KeyDown);
                ftf.KeyUp += new KeyEventHandler(FloatingForm_KeyUp);
                ftf.MouseWheel += new MouseEventHandler(FloatingForm_MouseWheel);
            }

            workspace.Widgets.ColorsForm.Resize += new EventHandler(ColorsForm_Resize);

            workspace.Widgets.CommonActionsWidget.ButtonClick += new EnumValueEventHandler(CommonActionsWidget_ButtonClick);
            workspace.Environment.SelectedPathChanged += new EventHandler(Environment_SelectedPathChanged);

            workspace.DocumentView.Layout += new LayoutEventHandler(DocumentView_Layout);

            BeginInvoke(new VoidVoidDelegate(PositionFloatingForms), null);
            BeginInvoke(new VoidVoidDelegate(workspace.OnLoad_ShowFloatingForms), null);
            //PositionFloatingForms();
            //workspace.OnLoad_ShowFloatingForms();

            // Set up icons
            BeginInvoke(new VoidVoidDelegate(InitMenuItemIcons), null);
            //InitMenuItemIcons();

            //
            if (splash != null)
            {
                splash.Close();
                splash.Dispose();
                splash = null;
            }

            Utility.TraceMe("done");

        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);
            PositionFloatingFormsIfNotMoved();
        }

        private Hashtable formLocations = new Hashtable();

        // Pass in null just to calculate the positions and store them into formLocations
        // this will causes the method to return -1,-1
        // TODO: move this kind of logic in to FloatingToolForm
        private Point CalcFormLocation(Form locateMe)
        {
            int rulerOffset = workspace.DocumentView.RulersEnabled ? 16 : 0; // HACK: should just get the ruler's size somehow

            formLocations[workspace.Widgets.MainToolBarForm] = PointToScreen(new Point(ClientRectangle.X + rulerOffset + 3, workspace.Widgets.TopDock.Bottom + rulerOffset + 3));

            formLocations[workspace.Widgets.LayerForm] = PointToScreen(new Point(workspace.DocumentView.ClientRectangle2.Right - 3 - workspace.Widgets.LayerForm.Width,
                this.RectangleToClient(workspace.DocumentView.RectangleToScreen(workspace.DocumentView.ClientRectangle2)).Bottom - 3 - workspace.Widgets.LayerForm.Height));

            formLocations[workspace.Widgets.HistoryForm] = PointToScreen(new Point(workspace.DocumentView.ClientRectangle2.Right - 3 - workspace.Widgets.HistoryForm.Width,
                workspace.Widgets.TopDock.Bottom + rulerOffset + 3));

            formLocations[workspace.Widgets.ColorsForm] = PointToScreen(new Point(ClientRectangle.X + rulerOffset + 3, 
                this.RectangleToClient(workspace.DocumentView.RectangleToScreen(workspace.DocumentView.ClientRectangle2)).Bottom - 3 - workspace.Widgets.ColorsForm.Height));

            if (locateMe == null)
            {
                return new Point(-1, -1);
            }

            return (Point)formLocations[locateMe];
        }

        private void PositionFloatingForms()
        {
            workspace.Widgets.MainToolBarForm.Location = CalcFormLocation(workspace.Widgets.MainToolBarForm);
            workspace.Widgets.LayerForm.Location = CalcFormLocation(workspace.Widgets.LayerForm);
            workspace.Widgets.HistoryForm.Location = CalcFormLocation(workspace.Widgets.HistoryForm);                
            workspace.Widgets.ColorsForm.Location = CalcFormLocation(workspace.Widgets.ColorsForm);
        }

        private void PositionFormIfNotMoved(Form form, Hashtable oldLocations, Hashtable newLocations)
        {
            if (oldLocations[form] == null || (Point)oldLocations[form] == form.Location)
            {
                form.Location = (Point)newLocations[form];
            }
        }

        private void PositionFloatingFormsIfNotMoved()
        {
            if (workspace != null)
            {
                Hashtable oldLocations = (Hashtable)formLocations.Clone();
                CalcFormLocation(null);

                PositionFormIfNotMoved(workspace.Widgets.MainToolBarForm, oldLocations, formLocations);
                PositionFormIfNotMoved(workspace.Widgets.LayerForm, oldLocations, formLocations);
                PositionFormIfNotMoved(workspace.Widgets.HistoryForm, oldLocations, formLocations);
                PositionFormIfNotMoved(workspace.Widgets.ColorsForm, oldLocations, formLocations);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);
            PositionFloatingFormsIfNotMoved();

            if (floaterOpacityTimer != null)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    this.floaterOpacityTimer.Enabled = false;
                }
                else
                {
                    this.floaterOpacityTimer.Enabled = true;
                }
            }
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove (e);
            PositionFloatingFormsIfNotMoved();
        }

        private void InitMenuItemIcons()
        {
            Utility.TraceMe("start");
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

            Utility.TraceMe("finish");
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
        /// Shows a generic Windows "About" dialog box.
        /// </summary>
        private void menuHelpAbout_Click(object sender, System.EventArgs e)
        {
            AboutDialog af = new AboutDialog();
            af.ShowDialog(this);
        }

        /// <summary>
        /// Adapted from http://www.codeproject.com/dotnet/wiascriptingdotnet.asp
        /// </summary>
        private void menuFileAcquireFromScannerOrCamera_Click(object sender, System.EventArgs e)
        {
            if (!IsWia2Available())
            {
                Utility.ErrorBox(this, "This feature is not available because the WIA 2.0 Automation Library (wiaaut.dll) is not installed.");
                return;
            }

            WIA.CommonDialogClass cdc = new WIA.CommonDialogClass();
            WIA.ImageFile imageFile = null;
            
            try
            {
                 imageFile = cdc.ShowAcquireImage(
                    WIA.WiaDeviceType.UnspecifiedDeviceType,
                    WIA.WiaImageIntent.UnspecifiedIntent,
                    WIA.WiaImageBias.MaximizeQuality,
                    "{00000000-0000-0000-0000-000000000000}",
                    true,
                    true,
                    false);
            }

            catch (System.Runtime.InteropServices.COMException)
            {
                Utility.ErrorBox(this, "Unable to retrieve image from device. It may be busy, or not connected properly.");
                return;
            }

            if (imageFile != null)
            {
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

                string tempName = Path.GetTempFileName() + "." + imageFile.FileExtension;
                imageFile.SaveFile(tempName);

                Image image = Image.FromFile(tempName);
                workspace.SetDocument(Document.FromImage(image));
                workspace.DocumentView.ScaleFactor = new ScaleFactor(1, 1);
                workspace.Document.Name = null;
                image.Dispose();
                workspace.History.ClearAll();
                workspace.History.PushNewAction(new NullHistoryAction("Acquire Image", Utility.GetImageResource("Icons.MenuLayersAddNewLayerIcon.bmp")));

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
                IDataObject pasted = Clipboard.GetDataObject();
                Image image = (Image)pasted.GetData(DataFormats.Bitmap);

                if (image == null)
                {
                    Utility.ErrorBox(this, "There is no acquirable image in the clipboard.");
                }
                else
                {
                    Document document = null;

                    try
                    {
                        document = Document.FromImage(image);
                        workspace.SetDocument(document);
                        workspace.DocumentView.ScaleFactor = new ScaleFactor(1, 1);
                        workspace.History.ClearAll();
                        workspace.History.PushNewAction(new NullHistoryAction("Acquire Image", Utility.GetImageResource("Icons.MenuLayersAddNewLayerIcon.bmp")));
                        Invalidate();
                    }

                    catch
                    {
                        Utility.ErrorBox(this, "There was an error transferring the image from the clipboard.");
                    }
                }
            }

            catch (ExternalException)
            {
                // Data could not be retrieved from the clipboard
            }

            catch (ThreadStateException)
            {
                // The ApartmentState property of the application is not set to ApartmentState.STA
                // I don't think this one will ever happen, seeing as how Main is tagged with the
                // STA attribute.
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

            if (workspace.Document.Name != null)
            {
                title = Path.GetFileName(workspace.Document.Name);
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

            ratio = " (" + (workspace.DocumentView.ScaleFactor.Ratio * 100.0).ToString() + "%)";

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

            NewFileDialog nfd = new NewFileDialog();
            nfd.NewWidth = GetNewDocumentSize().Width;
            nfd.NewHeight = GetNewDocumentSize().Height;

            if (nfd.ShowDialog(this) == DialogResult.OK)
            {
                CreateBlankDocument(new Size(nfd.NewWidth, nfd.NewHeight));
            }
        }

        private void CreateBlankDocument(Size size)
        {
            Document untitled = new Document(size.Width, size.Height); // TODO: reload the last document size from registry
            untitled.Name = null;

            BitmapLayer bitmapLayer;
            
            try
            {
                bitmapLayer = Layer.CreateBackgroundLayer(size.Width, size.Height);
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(this, "Not enough memory to create new image.");
                return;
            }

            untitled.Layers.Add(bitmapLayer);
            workspace.SetDocument(untitled);
            workspace.DocumentView.ScaleFactor = new ScaleFactor(1, 1);
            workspace.History.ClearAll();
            workspace.History.PushNewAction(new NullHistoryAction("New Image", Utility.GetImageResource("Icons.MenuFileNewIcon.bmp")));
            workspace.Document.Dirty = false;

            SetTitleText();
        }

        public void DoOpenFile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

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

            int ftIndex = fileTypes.IndexOfExtension(Path.GetExtension(fileName));

            if (ftIndex == -1)
            {
                Utility.ErrorBox(this, "The image type is not recognized, and can not be opened.");
                return;
            }

            // Save the old document in case an error occurs so we can revert to it
            Document oldDocument = workspace.Document;
            bool success = false;
            Document document = null;

            using (new WaitCursorChanger(this))
            {
                try
                {
                    FileType ft = fileTypes[ftIndex];
                    Stream stream = new FileStream(fileName, FileMode.Open);
                    LoadProgressDialog ld = new LoadProgressDialog(this, stream, ft);

                    if (splash != null)
                    {
                        Point startPos = splash.Location;
                        startPos.X += splash.Width / 2;
                        startPos.Y += splash.Height;
                        startPos.Y += 6;

                        document = ld.Load(startPos);

                    }
                    else
                    {
                        document = ld.Load();
                    }

                    stream.Close();

                    if (document != null)
                    {
                        success = true;
                    }
                }

                catch (ArgumentException)
                {
                    if (fileName.Length == 0)
                    {
                        Utility.ErrorBox(this, "The requested filename is blank.");
                    }
                    else
                    {
                        Utility.ErrorBox(this, "There was an error opening the file.");
                    }
                }

                catch (UnauthorizedAccessException)
                {
                    Utility.ErrorBox(this, "Access was denied to the requested file.");
                }
    
                catch (SecurityException)
                {
                    Utility.ErrorBox(this, "Access was denied to the requested file.");
                }

                catch (FileNotFoundException)
                {
                    Utility.ErrorBox(this, "The file could not be found.");
                }

                catch (DirectoryNotFoundException)
                {
                    Utility.ErrorBox(this, "The directory could not be found.");
                }

                catch (PathTooLongException)
                {
                    Utility.ErrorBox(this, "The filename is too long.");
                }

                catch (IOException)
                {
                    Utility.ErrorBox(this, "There was an error reading the file.");
                }

                catch
                {
                    Utility.ErrorBox(this, "There was an unknown error while opening the file.");
                }

                if (!success)
                {
                    if (workspace.Document != oldDocument)
                    {
                        workspace.SetDocument(oldDocument);
                    }

                    this.Cursor = Cursors.Default;
                    return;
                }

                document.Name = fileName;

                workspace.SetDocument(document);
                workspace.DocumentView.ScaleFactor = new ScaleFactor(1, 1);
                workspace.History.ClearAll();
                workspace.History.PushNewAction(new NullHistoryAction("Open Image", Utility.GetImageResource("Icons.ImageFromDiskIcon.bmp")));
                workspace.Document.Dirty = false;

                SetTitleText();
                Invalidate(true);

                success = true;

                // add to MRU list
                AddMru(fileName);
            }
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
            Surface renderSurface = workspace.DocumentView.BorrowRenderSurface();
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
                Rectangle dstRect = new Rectangle((thumb.Width - thumbSize.Width) / 2, (thumb.Height - thumbSize.Height) / 2, thumbSize.Width, thumbSize.Height);
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

        private DialogResult ChooseFile(out string fileName)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            ofd.Filter = this.fileTypes.ToString(true, "All images");
            ofd.FilterIndex = 0;

            DialogResult result = ofd.ShowDialog(this);
            fileName = ofd.FileName;

            return result;
        }

        private void menuFileOpen_Click(object sender, System.EventArgs e)
        {
            string fileName;
            DialogResult result = ChooseFile(out fileName);

            if (result == DialogResult.OK)
            {
                DoOpenFile(fileName);
            }
        }

        /// <summary>
        /// Does the grunt work to do a File->Save As operation.
        /// </summary>
        /// <returns><b>true</b> if the file was saved correctly, <b>false</b> if the user cancelled</returns>
        private bool DoSaveAs()
        {
            Utility.GCFullCollect();        

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.OverwritePrompt = true;
            sfd.Title = "Save As ...";
            sfd.Filter = fileTypes.ToString(false, null);
            sfd.OverwritePrompt = true;
            sfd.AddExtension = true;

            // Guess the file type from the extension of the document's name
            string ext = Path.GetExtension(workspace.Document.Name);
            int fIndex = fileTypes.IndexOfExtension(ext);

            if (fIndex == -1)
            {   // don't know the file type ... so we imply .bmp or .pdn
                if (workspace.Document.Layers.Count == 1)
                {   // default to .bmp for 1 layer
                    fIndex = fileTypes.IndexOfExtension(FileTypes.Bmp.DefaultExtension);
                }
                else
                {   // default to .pdn for >1 layer
                    fIndex = fileTypes.IndexOfExtension(FileTypes.Pdn.DefaultExtension);
                }
            }           else
            {
                // We found one!

                // however, if this filter does not support layers, and the document is multi-layered, revert to Layered Bitmap
                if (workspace.Document.Layers.Count != 1 && !fileTypes[fIndex].SupportsLayers)
                {
                    fIndex = fileTypes.IndexOfExtension(FileTypes.Pdn.DefaultExtension);
                }
            }

            if (workspace.Document.Name != null)
            {
                sfd.InitialDirectory = Path.GetDirectoryName(workspace.Document.Name);
                sfd.FileName = Path.GetFileName(workspace.Document.Name);

                if (fIndex != -1)
                {
                    sfd.FileName = Path.ChangeExtension(sfd.FileName, fileTypes[fIndex].DefaultExtension);
                }
            }

            sfd.FilterIndex = 1 + fIndex;
            sfd.RestoreDirectory = true;
            DialogResult dr = sfd.ShowDialog(this);

            if (dr == DialogResult.Cancel)
            {
                return false;
            }
            else
            //if (DialogResult.OK == sfd.ShowDialog(this))
            {
                FileType ft = fileTypes[sfd.FilterIndex - 1];

                // If the filename says, for example, ".jpg" but they chose to save as ".png" then
                // ask to change the file extension for them
                string ext2 = Path.GetExtension(sfd.FileName);
                if (!ft.SupportsExtension(ext2))
                {
                    string newName = Path.ChangeExtension(sfd.FileName, ft.DefaultExtension);

                    DialogResult yesNoDr = Utility.AskYesNoCancel(this, 
                        "To save correctly, the filename must be changed from '" + Path.GetFileName(sfd.FileName) + "' to '" + Path.GetFileName(newName) + "'.\n\nWould you like to allow this change?");

                    if (yesNoDr == DialogResult.Yes)
                    {
                        sfd.FileName = newName;
                    }
                    else if (yesNoDr == DialogResult.Cancel)
                    {
                        return false;
                    }
                }

                // If they are saving an image that has multiple layers, but the file type does not
                // support multiple layers, warn them that we are gonna FLATTEN the image
                if (workspace.Document.Layers.Count != 1 && !ft.SupportsLayers)
                {
                    if (DialogResult.Yes == WarnAboutFlattening())
                    {
                        if (!workspace.Environment.IsSelectionEmpty)
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

                Stream stream = sfd.OpenFile();
                AddMru(sfd.FileName);
                
                using (new WaitCursorChanger(this))
                {
                    if (ft is ISaveWithProgress)
                    {
                        SaveProgressDialog sd = new SaveProgressDialog(this);
                        sd.Save(stream, workspace.Document, ft);
                    }
                    else
                    {
                        ft.Save(workspace.Document, stream);
                    }

                    stream.Close();
                }

                workspace.Document.Dirty = false;
                workspace.Document.Name = sfd.FileName;
                SetTitleText();
            }

            Utility.GCFullCollect();        
            return true;
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

        /// <summary>
        /// Does the dirty work for a File->Save operation.
        /// </summary>
        /// <returns><b>true</b> if the file was saved, <b>false</b> if the user cancelled</returns>
        private bool DoSave()
        {
            Utility.GCFullCollect();        

            if (workspace.Document.Name == null)
            {
                return DoSaveAs();
            }

            string fileName = workspace.Document.Name;
            string extension = Path.GetExtension(fileName);

            // guess the file type based on the fileName's extension
            int ftIndex = fileTypes.IndexOfExtension(extension);

            // if we have no idea, default to Layered Bitmap
            if (ftIndex == -1)
            {
                ftIndex = fileTypes.IndexOfExtension(".pdn");
            }

            FileType ft = fileTypes[ftIndex];

            // if the user has a multilayer bitmap, and wants to save in a format that
            // doesn't support layers, bug them about it!
            if (workspace.Document.Layers.Count != 1 && !ft.SupportsLayers)
            {
                if (DialogResult.Yes == WarnAboutFlattening())
                {
                    if (!workspace.Environment.IsSelectionEmpty)
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

            // save!
            Stream stream = (Stream)new FileStream(fileName, FileMode.Create);
            AddMru(fileName);

            using (new WaitCursorChanger(this))
            {
                if (ft is ISaveWithProgress)
                {
                    SaveProgressDialog sd = new SaveProgressDialog(this);
                    sd.Save(stream, workspace.Document, ft);
                }
                else
                {
                    ft.Save(workspace.Document, stream);
                }

                stream.Close();
            }

            // reset the dirty bit so they won't be asked to save on quitting
            workspace.Document.Dirty = false;
            Utility.GCFullCollect();        
            return true;
        }

        private void menuFileSave_Click(object sender, System.EventArgs e)
        {
            DoSave();              
        }

        private bool IsClipboardImageAvailable()
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

            if (menuEditPaste.Enabled == false)
            {
                if (workspace.Environment.Tool != null)
                {
                    bool canHandle;
                    IDataObject pasted = Clipboard.GetDataObject();

                    workspace.Environment.Tool.PerformPasteQuery(pasted, out canHandle);

                    if (canHandle == true)
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

        private void menuEditCopy_Click(object sender, System.EventArgs e)
        {
            if (workspace.Environment.IsSelectionEmpty)
            {
                return;
            }

            try
            {
                PdnRegion selectionRegion = workspace.Environment.CreateSelectedRegion();
                PdnGraphicsPath selectionOutline = (PdnGraphicsPath)workspace.Environment.SelectedPath.Clone();
                BitmapLayer activeLayer = (BitmapLayer)workspace.ActiveLayer;
                RenderArgs renderArgs = new RenderArgs(activeLayer.Surface);
                IrregularSurface copySurface = new IrregularSurface(renderArgs.Surface, selectionRegion);
                SurfaceForClipboard surfaceForClipboard = new SurfaceForClipboard(copySurface, new GraphicsPathWrapper(selectionOutline));
                Rectangle selectionBounds = Utility.GetRegionBounds(selectionRegion);

                using (Surface copyBitmapSurface = new Surface((int)selectionBounds.Width, (int)selectionBounds.Height))
                {
                    using (Bitmap copyBitmap = copyBitmapSurface.CreateAliasedBitmap())
                    {
                        using (Graphics copyBitmapGraphics = Graphics.FromImage(copyBitmap))
                        {
                            copyBitmapGraphics.Clear(Color.White);
                        }

                        copySurface.Draw(copyBitmapSurface, -selectionBounds.X, -selectionBounds.Y);

                        DataObject dataObject = new DataObject();
                        dataObject.SetData(DataFormats.Bitmap, copyBitmap);
                        dataObject.SetData(surfaceForClipboard);

                        try
                        {
                            Clipboard.SetDataObject(dataObject, true);
                        }

                        catch
                        {
                            Utility.ErrorBox(this, "There was an error copying the image to the clipboard.");
                        }
                    }
                }

                copySurface.Dispose();
                renderArgs.Dispose();
                selectionOutline.Dispose();
                selectionRegion.Dispose();
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(this, "Not enough memory to complete the clipboard operation.");
            }
        }

        private void menuEditCut_Click(object sender, System.EventArgs e)
        {
            if (!workspace.Environment.IsSelectionEmpty)
            {   // TODO: if Copy fails (out of memory?) then don't erase!
                ClickOnMenuItem(menuEditCopy);
                workspace.PerformAction(typeof(EraseSelectionAction), "Cut", Utility.GetImageResource("Icons.MenuEditCutIcon.bmp"));
            }
        }

        #region effect background/progress rendering
        private void DoClearRegion(string undoName, PdnRegion region)
        {
            workspace.PerformAction(typeof(EraseSelectionAction));
        }

        private PdnRegion progressRegion = new PdnRegion();

        private void RenderedTileHandler(object sender, RenderedTileEventArgs e)
        {
            double progress = 100.0 * (double)(e.TileNumber + 1) / (double)e.TileCount;

            using (PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(e.RenderedRegion))
            {
                workspace.BeginInvoke(new WaitCallback(this.SetProgressStatusBarCallback), new object[] { progress });

                lock (progressRegion)
                {
                    progressRegion.Union(simplifiedRegion);
                }
            }
        }

        private void invalidateTimer_Tick(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            lock (progressRegion)
            {
                progressRegion.Intersect(workspace.VisibleDocumentRectangle);
                workspace.ActiveLayer.Invalidate(progressRegion);
                progressRegion.MakeEmpty();
                workspace.Update();
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
            using (PdnRegion blank = new PdnRegion())                                      
            {                                                                        
                blank.MakeEmpty();                                                   
                RenderedTileHandler(null, new RenderedTileEventArgs(blank, 1, 0));   
            }                         
                                               
            InvokeResetProgressStatusBar();                                          
            workspace.DocumentView.EnableOutlineAnimation = true;
        }

        private void StartingRenderingHandler(object sender, EventArgs e)
        {
            workspace.DocumentView.EnableOutlineAnimation = false;
        }

        private void DoEffect(Effect effect, EffectConfigToken token, PdnRegion renderRegion, Surface originalSurface)
        {
            workspace.DocumentView.EnableOutlineAnimation = false;

            try
            {
                ProgressDialog aed = new ProgressDialog();

                if (effect.Image != null)
                {
                    aed.Icon = Utility.ImageToIcon(effect.Image, Color.FromArgb(192, 192, 192));
                }
                else
                {
                    aed.Icon = new Icon(Utility.GetResourceStream("Icons.StopWatchIcon.ico"));
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
                    this.workspace.ActiveLayer.PushSuppressPreviewChanges();

                    try
                    {
                        ha = ((BitmapLayer)workspace.ActiveLayer).CreateHistoryAction(effect.Name, effect.Image, renderRegion);

                        BackgroundEffectRenderer ber = new BackgroundEffectRenderer(
                            effect,
                            token,
                            new RenderArgs(((BitmapLayer)workspace.ActiveLayer).Surface),
                            new RenderArgs(originalSurface),
                            renderRegion,
                            tilesPerCpu * CpuCount.Info.PhysicalCpuCount,
                            CpuCount.Info.PhysicalCpuCount);

                        aed.Tag = ber;
                        ber.RenderedTile += new RenderedTileEventHandler(aed.RenderedTileHandler);
                        ber.RenderedTile += new RenderedTileEventHandler(RenderedTileHandler);
                        ber.FinishedRendering += new EventHandler(aed.FinishedRenderingHandler);
                        ber.FinishedRendering += new EventHandler(FinishedRenderingHandler);
                        ber.Start();

                        result = aed.ShowDialog(this);

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
                        workspace.ActiveLayer.PopSuppressPreviewChanges();
                    }

                    using (PdnRegion simplifiedRenderRegion = Utility.SimplifyAndInflateRegion(renderRegion))
                    {
                        using (new WaitCursorChanger(this))
                        {
                            workspace.ActiveLayer.Invalidate(simplifiedRenderRegion);
                        }
                    }

                    if (result == DialogResult.OK && ha != null)
                    {
                        workspace.History.PushNewAction(ha);
                        workspace.Update();
                    }

                    this.InvokeResetProgressStatusBar();
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
                    // TODO: There needs to be a better way of converting from a Name to an instance of an effect
                    //       maybe use an attribute, and Reflection?
                    //       Note: Should not use an attribute, as then you can't require its presence.
                    ConstructorInfo ci = effectType.GetConstructor(Type.EmptyTypes);
                    Effect effect = (Effect)ci.Invoke(null);
                    string name = effect.Name;
                    string repeatName = "Repeat " + effect.Name;

                    if (effect is IConfigurableEffect)
                    {
                        name += " ...";
                    }
                
                    if (repeatName == ((MenuItem)sender).Text)
                    {
                        try
                        {
                            DoEffect(lastEffect, lastEffectToken, selectedRegion, ((BitmapLayer)workspace.ActiveLayer).Surface.CopyContents());
                        }

                        catch (OutOfMemoryException)
                        {
                            Utility.ErrorBox(this, "Not enough memory to apply effect.");
                            return;
                        }
                    }
                    else
                    if (name == ((MenuItem)sender).Text)
                    {
                        EffectConfigToken newLastToken = null;

                        if (!(effect is IConfigurableEffect))
                        {
                            try
                            {
                                using (Surface copy = ((BitmapLayer)workspace.ActiveLayer).Surface.CopyContents())
                                {
                                    DoEffect(effect, null, selectedRegion, copy);
                                }
                            }

                            catch (OutOfMemoryException)
                            {
                                Utility.ErrorBox(this, "Not enough memory to apply effect.");
                                return;
                            }
                        }
                        else
                        {
                            PdnRegion previewRegion = (PdnRegion)selectedRegion.Clone();
                            previewRegion.Intersect(Rectangle.Inflate(workspace.VisibleDocumentRectangle, 1, 1));

                            Surface originalSurface = null;
                            
                            try
                            {
                                originalSurface = ((BitmapLayer)workspace.ActiveLayer).Surface.CopyContents();
                            }

                            catch (OutOfMemoryException)
                            {
                                Utility.ErrorBox(this, "Not enough memory to use effects.");
                                return;
                            }

                            //
                            workspace.ActiveLayer.PushSuppressPreviewChanges();
                            //

                            IConfigurableEffect config = (IConfigurableEffect)effect;
                            EffectConfigDialog configDialog = config.CreateConfigDialog();

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
                                tilesPerCpu * CpuCount.Info.PhysicalCpuCount,
                                CpuCount.Info.PhysicalCpuCount);

                            ber.RenderedTile += new RenderedTileEventHandler(RenderedTileHandler);
                            ber.StartingRendering += new EventHandler(StartingRenderingHandler);
                            ber.FinishedRendering += new EventHandler(FinishedRenderingHandler);
                            configDialog.Tag = ber;

                            invalidateTimer.Enabled = true;
                            DialogResult dr = configDialog.ShowDialog(this);
                            invalidateTimer.Enabled = false;

                            if (dr == DialogResult.OK)
                            {
                                effectTokenHash[effectType] = configDialog.EffectToken;
                            }

                            ber.Abort();
                            ber.Join();
                            ber = null;

                            ((BitmapLayer)workspace.ActiveLayer).Surface.CopySurface(originalSurface);
                            workspace.ActiveLayer.Invalidate();
                            configDialog.EffectTokenChanged -= eh;
                            configDialog.Hide();
                            this.Update();
                            previewRegion.Dispose();

                            //
                            workspace.ActiveLayer.PopSuppressPreviewChanges();
                            //

                            if (dr == DialogResult.OK)
                            {
                                newLastToken = (EffectConfigToken)configDialog.EffectToken.Clone();
                                this.ResetProgressStatusBar();
                                DoEffect(effect, newLastToken, selectedRegion, originalSurface);
                            }
                            else
                            {
                                workspace.ActiveLayer.Invalidate();
                                return;
                            }
                        }

                        // if it was from the Effects menu, save it as the "Repeat ...." item
                        if (effect.Category == EffectCategory.Effect)
                        {
                            lastEffect = effect;
                            lastEffectToken = newLastToken;
                        }
                    }
                }
            }

            finally
            {
                selectedRegion.Dispose();
                this.InvokeResetProgressStatusBar();
                workspace.DocumentView.EnableOutlineAnimation = true;
                workspace.Environment.SetTool(oldTool, workspace);
            }
        }

        private void menuEffects_Popup(object sender, System.EventArgs e)
        {
            // Clear out the menu items!
            foreach (MenuItem mi in menuEffects.MenuItems)
            {
                mi.Click -= menuEffectsClickDelegate;
            }

            menuEffects.MenuItems.Clear();

            // If we have a repeatable effect, add it with "Repeat ___ ... (Ctrl+F)" along with a separator
            if (this.lastEffect != null)
            {
                string menuName = "Repeat " + lastEffect.Name;
                MenuItem mi = new MenuItem(menuName, menuEffectsClickDelegate);
                this.dotNetMenuProvider.SetDrawSpecial(mi, true);
                if (lastEffect.Image != null)
                {
                    SetMenuIcon(mi, lastEffect.Image);
                }

                // TODO: Ctrl+F wouldn't work ... maybe I'm doing something wrong? Sigh.
                //mi.Shortcut = Shortcut.CtrlF;

                menuEffects.MenuItems.Add(mi);

                // add separator
                MenuItem separator = new MenuItem("-");
                this.dotNetMenuProvider.SetDrawSpecial(separator, true);
                menuEffects.MenuItems.Add(separator);
            }

            // Fill the menu with the effect names, and "..." if it is configurable
            AddEffectsToMenu(menuEffects, new BoolObjectDelegate(EffectsMenuPredicate));
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
            menuLayersAdjustments.MenuItems.Clear();
            AddEffectsToMenu(menuLayersAdjustments, new BoolObjectDelegate(AdjustmentsMenuPredicate));
        }

        private void AddEffectsToMenu(MenuItem topMenu, BoolObjectDelegate predicate)
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
                    name += " ...";
                }

                MenuItem mi = new MenuItem(name, menuEffectsClickDelegate);
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

            bool scannerEnabled = false;

            if (IsWia2Available())
            {
                WIA.DeviceManagerClass dmc = new WIA.DeviceManagerClass();

                if (dmc.DeviceInfos.Count > 0)
                {
                    scannerEnabled = true;
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

            catch (OutOfMemoryException)
            {
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
                Utility.ErrorBox(this, "Not enough memory to create a new layer.");
            }
        }

        private void menuLayersDuplicateLayer_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.PerformDuplicateLayerClick();
        }

        private void menuLayersFlattenImage_Click(object sender, System.EventArgs e)
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
            menuLayersFlattenImage.Enabled = (workspace.Document.Layers.Count > 1);
        }

        private void menuLayersDeleteLayer_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.PerformDeleteLayerClick();
        }

        private void menuWindow_Popup(object sender, System.EventArgs e)
        {
            menuWindowTools.Checked = workspace.Widgets.MainToolBarForm.Visible;
            menuWindowHistory.Checked = workspace.Widgets.HistoryForm.Visible;
            menuWindowLayers.Checked = workspace.Widgets.LayerForm.Visible;
            menuWindowColors.Checked = workspace.Widgets.ColorsForm.Visible;
        }

        private void menuWindowTools_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.MainToolBarForm.Visible = !workspace.Widgets.MainToolBarForm.Visible;
        }

        private void menuWindowHistory_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.HistoryForm.Visible = !workspace.Widgets.HistoryForm.Visible;     
        }

        private void menuWindowLayers_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.LayerForm.Visible = !workspace.Widgets.LayerForm.Visible;     
        }

        private void menuWindowColors_Click(object sender, System.EventArgs e)
        {
            workspace.Widgets.ColorsForm.Visible = !workspace.Widgets.ColorsForm.Visible;
        }

        private void menuImage_Popup(object sender, System.EventArgs e)
        {
            menuImageCrop.Enabled = !workspace.Environment.IsSelectionEmpty;
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

            foreach (Type toolType in workspace.Tools)
            {
                Tool tool = Tool.CreateTool(toolType, workspace);
                MenuItem mi = new MenuItem(tool.Name, menuToolsClickDelegate);
                SetMenuIcon(mi, tool.Image);

                if (workspace.Environment.Tool != null && workspace.Environment.Tool.Name == tool.Name)
                {
                    mi.Checked = true;
                }

                menuTools.MenuItems.Add(mi);
            }
        }

        private void menuTools_ClickHandler(object sender, System.EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;

            foreach (Type toolType in workspace.Tools)
            {
                Tool tool = Tool.CreateTool(toolType, workspace);

                if (tool.Name == mi.Text)
                {
                    workspace.Widgets.MainToolBar.SelectTool(toolType);
                    break;
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

        private void menuItem3_Click(object sender, System.EventArgs e)
        {
#if DEBUG
            Utility.WalkGraph(this);
#endif
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

        private void SyncResetProgressStatusBar()
        {
            IAsyncResult result = this.BeginInvoke(new VoidVoidDelegate(ResetProgressStatusBar));
            object ignore = this.EndInvoke(result);
        }

        private void ResetProgressStatusBar()
        {
            this.progressStatusBar.Text = string.Empty;
            this.progressStatusBar.Icon = null;
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
                    this.progressStatusBar.Text = ((int)percent).ToString() + "%";

                    if (this.progressStatusBar.Icon == null)
                    {
                        this.progressStatusBar.Icon = new Icon(Utility.GetResourceStream("Icons.StopWatchIcon.ico"));
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

        private void menuImageZoomIn_Click(object sender, System.EventArgs e)
        {
            workspace.DocumentView.ZoomIn();
        }

        private void menuImageZoomOut_Click(object sender, System.EventArgs e)
        {
            workspace.DocumentView.ZoomOut();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!e.Handled)
            {
                if (workspace.ContainsFocus && 
                    workspace.Environment.Tool != null)
                {
                    workspace.Environment.Tool.PerformKeyPress(e);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown (e);

            if (!e.Handled)
            {
                if (workspace.ContainsFocus && 
                    workspace.Environment.Tool != null)
                {
                    workspace.Environment.Tool.PerformKeyDown(e);
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            // handle shortcuts which can't be expressed as a Shortcut enumeration
            if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
            {
                if (e.Modifiers == Keys.Control)
                {
                    ClickOnMenuItemAsync(this.menuImageZoomIn);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
            {
                if (e.Modifiers == Keys.Control)
                {
                    ClickOnMenuItemAsync(this.menuImageZoomOut);
                    e.Handled = true;
                }
            }

            if (!e.Handled)
            {
                if (workspace.ContainsFocus && 
                    workspace.Environment.Tool != null)
                {
                    workspace.Environment.Tool.PerformKeyUp(e);
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
                        if (workspace.ContainsFocus && 
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
        
        private void FloatingForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsAcceleratorHACK(e))
            {
                this.OnKeyDown(e);
            }
        }

        private void FloatingForm_KeyUp(object sender, KeyEventArgs e)
        {
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
                    Shortcut shortcut = mi.Shortcut;
                    
                    if (keyName == shortcut.ToString())
                    {
                        ClickOnMenuItem(mi);
                        keyInfo.Handled = true;
                        break;
                    }
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
                    workspace.DocumentView.ZoomIn();
                    break;

                case CommonAction.ZoomOut:
                    workspace.DocumentView.ZoomOut();
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
                    this.contextStatusBar.Icon = new Icon(Utility.GetResourceStream("Icons.SelectionIcon.ico"));
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
            Rectangle screenDocRect = workspace.DocumentView.VisibleDocumentBounds;

            if (floaters == null)
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

        private void menuImageActualSize_Click(object sender, System.EventArgs e)
        {
            workspace.DocumentView.ScaleFactor = ScaleFactor.OneToOne;
        }

        private void ColorsForm_Resize(object sender, EventArgs e)
        {
            this.PositionFloatingFormsIfNotMoved();
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
                Utility.ErrorBox(this, "Not enough memory to create a new layer.");
                return;
            }

            bool result = DoPaste();

            if (!result)
            {
                workspace.History.StepBackward();
            }
        }

        private void menuFilePrint_Click(object sender, System.EventArgs e)
        {
            if (!IsWia2Available())
            {
                Utility.ErrorBox(this, "This feature is not available because the WIA 2.0 Automation Library (wiaaut.dll) is not installed. Please uninstall and reinstall Paint.NET.");
                return;
            }

            WIA.CommonDialogClass cdc = new WIA.CommonDialogClass();

            // render image to a bitmap, save it to disk
            Surface s = new Surface(workspace.Document.Size);
            RenderArgs ra = new RenderArgs(s);

            this.Update();
            using (new WaitCursorChanger(this))
            {
                workspace.Document.Render(ra, ra.Bounds);
            }
            
            string tempName = Path.GetTempFileName();
            ra.Bitmap.Save(tempName, ImageFormat.Bmp);

            WIA.VectorClass vector = new WIA.VectorClass();
            object tempName_o = (object)tempName;
            vector.Add(ref tempName_o, 0);
            object vector_o = (object)vector;
            cdc.ShowPhotoPrintingWizard(ref vector_o);

            // Try to delete the temp file but don't worry if we can't
            try
            {
                File.Delete(tempName);
            }

            catch
            {
            }
        }

        private void menuFile_Popup(object sender, System.EventArgs e)
        {
            menuFilePrint.Enabled = IsWia2Available();        
        }

        private bool IsWia2Available()
        {
            try
            {
                WIA.DeviceManagerClass dmc = new WIA.DeviceManagerClass();
            }

            catch
            {
                return false;
            }

            return true;
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter (drgevent);
            
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                if (files.Length == 1)
                {
                    try
                    {
                        FileAttributes fa = File.GetAttributes(files[0]);

                        if ((fa | FileAttributes.Directory) != 0)
                        {
                            drgevent.Effect = DragDropEffects.Link;
                        }
                    }

                    catch
                    {
                    }
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop (drgevent);

            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);
                DoOpenFile(files[0]);
            }
        }

        private void menuHelpHelpTopics_Click(object sender, System.EventArgs e)
        {
            try
            {
                string fileName = "PaintDotNet.chm";
                string dirName = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string fullName = Path.Combine(dirName, fileName);

                // This produces very weird behavior, what with the help window popping *under*
                // the main window and doing other crazy stuff
                //Help.ShowHelp(this, fullName, "Overview.htm");

                // So we do this ... although this lets the user open multiple copies, it seems
                // to be the lesser of the two evils.
                System.Diagnostics.Process.Start(fullName);
            }

            catch
            {
                Utility.ErrorBox(this, "The help file 'PaintDotNet.chm' could not be found.");
            }
        }

        private void menuFileOpenInNewWindow_Click(object sender, System.EventArgs e)
        {
            string fileName;
            DialogResult result = ChooseFile(out fileName);

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
            //this.contextStatusBar.Text = workspace.DocumentView.DocumentScrollPosition.ToString();
        }

        protected override void OnRemoteSessionChange()
        {
            base.OnRemoteSessionChange ();

            if (PdnBaseForm.IsRemoteSession())
            {
                this.contextStatusBar.Text = "Transparent windows have been disabled to improve remote session performance.";
                this.invalidateTimer.Interval = 200;
                this.floaterOpacityTimer.Enabled = false;
            }
            else
            {
                this.contextStatusBar.Text = string.Empty;
                this.invalidateTimer.Interval = 25;
                this.floaterOpacityTimer.Enabled = true;
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
    }
}
