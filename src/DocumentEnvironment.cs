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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace PaintDotNet
{
    /// <summary>
    /// Manages document-independent workspace configuration details, and provides
    /// notification events for every item that can change.
    /// </summary>
    public class DocumentEnvironment
        : IDisposable
    {
        #region Font stuff
        private TextAlignment textAlignment;
        public TextAlignment TextAlignment
        {
            get
            {
                return textAlignment;
            }

            set
            {
                if (value != textAlignment)
                {
                    OnTextAlignmentChanging();
                    textAlignment = value;
                    OnTextAlignmentChanged();
                }
            }
        }

        public event EventHandler TextAlignmentChanging;
        protected void OnTextAlignmentChanging()
        {
            if (TextAlignmentChanging != null)
            {
                TextAlignmentChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler TextAlignmentChanged;
        protected void OnTextAlignmentChanged()
        {
            if (TextAlignmentChanged != null)
            {
                TextAlignmentChanged(this, EventArgs.Empty);
            }
        }

        private FontInfo fontInfo;
        public FontInfo FontInfo
        {
            get
            {
                return fontInfo;
            }

            set
            {
                if (fontInfo != value)
                {
                    OnFontInfoChanging();
                    fontInfo = value;
                    OnFontInfoChanged();
                }
            }
        }

        // FontInfoChanging
        // This event is raised right before the 'fontInfo' is changed via the 'FontInfo' property.
        public event EventHandler FontInfoChanging;
        protected void OnFontInfoChanging()
        {
            if (FontInfoChanging != null)
            {
                FontInfoChanging(this, EventArgs.Empty);
            }
        }

        // FontInfoChanged
        // This event is raised right after the 'fontInfo' is changed via the 'FontInfo' property.
        public event EventHandler FontInfoChanged;

        protected void OnFontInfoChanged()
        {
            if (FontInfoChanged != null)
            {
                FontInfoChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Tool
        private Tool tool;
        public Tool Tool
        {
            get
            {
                return tool;
            }
        }

        public void SetTool(Type toolType, DocumentWorkspace workspace)
        {
            if (toolType == GetToolType())
            {
                return;
            }
            else if (toolType == null)
            {
                SetTool(null);
            }
            else
            {
                SetTool(Tool.CreateTool(toolType, workspace));
            }
        }

        public Type GetToolType()
        {
            if (Tool != null)
            {
                return Tool.GetType();
            }
            else
            {
                return null;
            }
        }

        public void SetTool(Tool copyMe)
        {
            OnToolChanging();

            if (tool != null)
            {
                tool.PerformDeactivate();
                tool.StatusChanged -= new EventHandler(tool_StatusChanged);
                tool.Dispose();
                tool = null;
            }

            if (copyMe != null)
            {
                tool = Tool.CreateTool(copyMe.GetType(), copyMe.Workspace);
                tool.StatusChanged += new EventHandler(tool_StatusChanged);
                tool.PerformActivate();
            }

            OnToolChanged();
        }

        // ToolChanging
        // This event is raised right before the 'activeTool' is changed via the 'ActiveTool' property.
        public event EventHandler ToolChanging;

        protected void OnToolChanging()
        {
            if (ToolChanging != null)
            {
                ToolChanging(this, EventArgs.Empty);
            }
        }

        // ToolChanged
        // This event is raised right after the 'activeTool' is changed via the 'ActiveTool' property.
        public event EventHandler ToolChanged;

        protected void OnToolChanged()
        {
            if (ToolChanged != null)
            {
                ToolChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region PenInfo
        private PenInfo penInfo;

        public Pen CreatePen(bool swapColors)
        {
            if (!swapColors)
            {
                return PenInfo.CreatePen(BrushInfo, ForeColor.ToColor(), BackColor.ToColor());
            }
            else
            {
                return PenInfo.CreatePen(BrushInfo, BackColor.ToColor(), ForeColor.ToColor());
            }
        }

        public PenInfo PenInfo
        {
            get
            {
                return penInfo;
            }

            set
            {
                OnPenInfoChanging();
                penInfo = value;
                OnPenInfoChanged();
            }
        }

        // PenInfoChanging
        // This event is raised right before the 'activePenInfo' is changed via the 'ActivePenInfo' property.
        public event EventHandler PenInfoChanging;
        protected void OnPenInfoChanging()
        {
            if (PenInfoChanging != null)
            {
                PenInfoChanging(this, EventArgs.Empty);
            }
        }

        // PenInfoChanged
        // This event is raised right after the 'activePenInfo' is changed via the 'ActivePenInfo' property.
        public event EventHandler PenInfoChanged;
        protected void OnPenInfoChanged()
        {
            if (PenInfoChanged != null)
            {
                PenInfoChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region BrushInfo
        private BrushInfo brushInfo;

        public Brush CreateBrush(bool swapColors)
        {
            if (!swapColors)
            {
                return BrushInfo.CreateBrush(ForeColor.ToColor(), BackColor.ToColor());
            }
            else
            {
                return BrushInfo.CreateBrush(BackColor.ToColor(), ForeColor.ToColor());
            }
        }

        public BrushInfo BrushInfo
        {
            get
            {
                return brushInfo;
            }

            set
            {
                OnBrushInfoChanging();
                brushInfo = value;
                OnBrushInfoChanged();
            }
        }

        // BrushInfoChanging
        // This event is raised right before the 'activeBrushInfo' is changed via the 'ActiveBrushInfo' property.
        public event EventHandler BrushInfoChanging;
        protected void OnBrushInfoChanging()
        {
            if (BrushInfoChanging != null)
            {
                BrushInfoChanging(this, EventArgs.Empty);
            }
        }

        // BrushInfoChanged
        // This event is raised right after the 'activeBrushInfo' is changed via the 'ActiveBrushInfo' property.
        public event EventHandler BrushInfoChanged;
        protected void OnBrushInfoChanged()
        {
            if (BrushInfoChanged != null)
            {
                BrushInfoChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region ForeColor
        private ColorBgra foreColor;

        public ColorBgra ForeColor
        {
            get
            {
                return foreColor;
            }

            set
            {
                OnForeColorChanging();
                foreColor = value;
                OnForeColorChanged();
            }
        }

        // ForeColorChanging
        // This event is raised right before the 'activeForeColor' is changed via the 'ActiveForeColor' property.
        public event EventHandler ForeColorChanging;
        protected void OnForeColorChanging()
        {
            if (ForeColorChanging != null)
            {
                ForeColorChanging(this, EventArgs.Empty);
            }
        }

        // ForeColorChanged
        // This event is raised right after the 'activeForeColor' is changed via the 'ActiveForeColor' property.
        public event EventHandler ForeColorChanged;
        protected void OnForeColorChanged()
        {
            if (ForeColorChanged != null)
            {
                ForeColorChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region BackColor
        private ColorBgra backColor;
        public ColorBgra BackColor
        {
            get
            {
                return backColor;
            }

            set
            {
                OnBackColorChanging();
                backColor = value;
                OnBackColorChanged();
            }
        }

        // BackColorChanging
        // This event is raised right beback the 'activeBackColor' is changed via the 'ActiveBackColor' property.
        public event EventHandler BackColorChanging;
        protected void OnBackColorChanging()
        {
            if (BackColorChanging != null)
            {
                BackColorChanging(this, EventArgs.Empty);
            }
        }

        // BackColorChanged
        // This event is raised right after the 'activeBackColor' is changed via the 'ActiveBackColor' property.
        public event EventHandler BackColorChanged;
        protected void OnBackColorChanged()
        {
            if (BackColorChanged != null)
            {
                BackColorChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region AlphaBlending

        public CompositingMode GetCompositingMode()
        {
            return alphaBlending ? CompositingMode.SourceOver : CompositingMode.SourceCopy;
        }
        
        private bool alphaBlending;
        public bool AlphaBlending
        {
            get
            {
                return alphaBlending;
            }

            set
            {
                if (value != alphaBlending)
                {
                    OnAlphaBlendingChanging();
                    alphaBlending = value;
                    OnAlphaBlendingChanged();
                }
            }
        }

        public event EventHandler AlphaBlendingChanging;
        protected void OnAlphaBlendingChanging()
        {
            if (AlphaBlendingChanging != null)
            {
                AlphaBlendingChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler AlphaBlendingChanged;
        protected void OnAlphaBlendingChanged()
        {
            if (AlphaBlendingChanged != null)
            {
                AlphaBlendingChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region ShapeDrawType
        private ShapeDrawType shapeDrawType;

        public ShapeDrawType ShapeDrawType
        {
            get
            {
                return shapeDrawType;
            }

            set
            {
                OnShapeDrawTypeChanging();
                shapeDrawType = value;
                OnShapeDrawTypeChanged();
            }
        }

        public event EventHandler ShapeDrawTypeChanging;
        protected void OnShapeDrawTypeChanging()
        {
            if (ShapeDrawTypeChanging != null)
            {
                ShapeDrawTypeChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler ShapeDrawTypeChanged;
        protected void OnShapeDrawTypeChanged()
        {
            if (ShapeDrawTypeChanged != null)
            {
                ShapeDrawTypeChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region SelectedPath and helper methods
        private Selection selection;
        //private PdnGraphicsPath selectedPath;

        public Selection Selection
        {
            get
            {
                return this.selection;
            }
        }

        /*
            /// <summary>
            /// Gets or sets the currently selected path.
            /// If you set this property, the DocumentEnvironment instance will take ownership
            /// of the PdnGraphicsPath and you should NOT call Dispose() on it.
            /// </summary>
            /// <remarks>
            /// If you are modifying the SelectedPath, you should call PerformSelectedPathChanging,
            /// then modify the path, then call PerformSelectedPathChanged.
            /// If you set this property, however, do not call those methods.
            /// </remarks>
            public PdnGraphicsPath SelectedPath
        {
            get
            {
                return selectedPath;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("SelectedPath can't be null");
                }

                OnSelectedPathChanging();

                if (!Object.ReferenceEquals(value, selectedPath))
                {
                    selectedPath.Dispose();
                }

                selectedPath = (PdnGraphicsPath)value;
                OnSelectedPathChanged();
            }
        }
        */

        /*
        public bool IsSelectionEmpty
        {
            get
            {
                return selectedPath.PointCount == 0;
            }
        }
        */

        /*
        public event EventHandler SelectedPathChanging;
        protected virtual void OnSelectedPathChanging()
        {
            if (SelectedPathChanging != null)
            {
                SelectedPathChanging(this, EventArgs.Empty);
            }
        }

        public void PerformSelectedPathChanging()
        {
            OnSelectedPathChanging();
        }

        public event EventHandler SelectedPathChanged;
        protected virtual void OnSelectedPathChanged()
        {
            if (SelectedPathChanged != null)
            {
                SelectedPathChanged(this, EventArgs.Empty);
            }
        }

        public void PerformSelectedPathChanged()
        {
            OnSelectedPathChanged();
        }
        */

        /*
        /// <summary>
        /// Returns a copy of the currently selected region. This is not necessarily intersected
        /// with the document's bounds, so if you need that property to be true you will have to
        /// call PdnRegion.Intersect yourself.
        /// </summary>
        /// <returns></returns>
        public PdnRegion CreateSelectedRegion()
        {
            return new PdnRegion(selectedPath);
        }
        */
        #endregion

        #region AntiAliasing
        public event EventHandler AntiAliasingChanging;
        protected void OnAntiAliasingChanging()
        {
            if (AntiAliasingChanging != null)
            {
                AntiAliasingChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler AntiAliasingChanged;
        protected void OnAntiAliasingChanged()
        {
            if (AntiAliasingChanged != null)
            {
                AntiAliasingChanged(this, EventArgs.Empty);
            }
        }

        private bool antiAliasing;
        public bool AntiAliasing
        {
            get
            {
                return antiAliasing;
            }

            set
            {
                if (antiAliasing != value)
                {
                    OnAntiAliasingChanging();
                    antiAliasing = value;
                    OnAntiAliasingChanged();
                }
            }
        }
        #endregion

        #region Units
        public event EventHandler UnitsChanging;
        protected void OnUnitsChanging()
        {
            if (UnitsChanging != null)
            {
                UnitsChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler UnitsChanged;
        protected void OnUnitsChanged()
        {
            if (UnitsChanged != null)
            {
                UnitsChanged(this, EventArgs.Empty);
            }
        }

        private MeasurementUnit units;
        public MeasurementUnit Units
        {
            get
            {
                return this.units;
            }

            set
            {
                if (this.units != value)
                {
                    OnUnitsChanging();
                    this.units = value;
                    OnUnitsChanged();
                }
            }
        }
        #endregion

        #region Tolerance
        public event EventHandler ToleranceChanged;
        protected void OnToleranceChanged()
        {
            if (ToleranceChanged != null)
            {
                ToleranceChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ToleranceChanging;
        protected void OnToleranceChanging()
        {
            if (ToleranceChanging != null)
            {
                ToleranceChanging(this, EventArgs.Empty);
            }
        }

        private float tolerance;
        public float Tolerance
        {
            get
            {
                return tolerance;
            }

            set
            {
                if (tolerance != value)
                {
                    tolerance = value;
                    OnToleranceChanged();
                }
            }
        }
        #endregion

        #region Static Tool Data Locker
        private Hashtable staticToolData = Hashtable.Synchronized(new Hashtable());

        public object GetStaticToolData(Type toolType)
        {
            return staticToolData[toolType];
        }

        public void SetStaticToolData(Type toolType, object data)
        {
            staticToolData[toolType] = data;
        }
        #endregion

        public void PerformAllChanged()
        {
            OnFontInfoChanged();
            OnTextAlignmentChanged();
            OnToolChanged();
            OnPenInfoChanged();
            OnBrushInfoChanged();
            OnForeColorChanged();
            OnBackColorChanged();
            OnShapeDrawTypeChanged();
            OnAlphaBlendingChanged();
            OnToleranceChanged();
        }

        private void ResetToDefaults()
        {
            antiAliasing = true;
            tool = null;
            foreColor = ColorBgra.FromBgra(0, 0, 0, 255);
            backColor = ColorBgra.FromBgra(255, 255, 255, 255);
            penInfo.Width = 1.0f;
            penInfo.DashStyle = DashStyle.Solid;
            brushInfo.BrushType = BrushType.Solid;
            brushInfo.HatchStyle = HatchStyle.BackwardDiagonal;
            fontInfo = new FontInfo(new FontFamily("Arial"), 12, 0);
            textAlignment = TextAlignment.Left;
            shapeDrawType = ShapeDrawType.Outline;
            alphaBlending = true;
            tolerance = 0.5f;
            this.selection.Reset();
        }

        public event EventHandler ToolStatusChanged;
        private void OnToolStatusChanged()
        {
            if (ToolStatusChanged != null)
            {
                ToolStatusChanged(this, EventArgs.Empty);
            }
        }

        private void tool_StatusChanged(object sender, EventArgs e)
        {
            OnToolStatusChanged();
        }
    

        public DocumentEnvironment()
        {    
            this.selection = new Selection();
            //this.selectedPath = new PdnGraphicsPath();
            this.units = MeasurementUnit.Pixel;
            ResetToDefaults();
        }

        ~DocumentEnvironment()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.tool != null)
                {
                    this.tool.Dispose();
                    this.tool = null;
                }
            }
        }
    }
}