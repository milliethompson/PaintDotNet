using System;
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
            if (toolType == null)
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
                tool = null;
            }

            if (copyMe != null)
            {
                tool = Tool.CreateTool(copyMe.GetType(), copyMe.Workspace);
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
        private GraphicsPath selectedPath;
        public GraphicsPath SelectedPath
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
                selectedPath.Dispose();
                selectedPath = (GraphicsPath)value.Clone();
                OnSelectedPathChanged();
            }
        }

        public bool IsSelectionEmpty
        {
            get
            {
                return selectedPath.PointCount == 0;
            }
        }

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

        public Region CreateSelectedRegion()
        {
            using (GraphicsPath closed = (GraphicsPath)selectedPath.Clone())
            {
                closed.CloseAllFigures();
                return new Region(closed);
            }
        }
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

        #region HighlightSelection
        private bool highlightSelection;
        public bool HighlightSelection
        {
            get
            {
                return highlightSelection;
            }

            set
            {
                PerformSelectedPathChanging();
                highlightSelection = value;
                PerformSelectedPathChanged();
            }
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
        }

        public void ResetToDefaults()
        {
            antiAliasing = true;
            highlightSelection = true;
            tool = null;
            foreColor = ColorBgra.FromBgra(0, 0, 0, 255);
            backColor = ColorBgra.FromBgra(255, 255, 255, 255);
            penInfo.Width = 1.0f;
            penInfo.DashStyle = DashStyle.Solid;
            brushInfo.BrushType = BrushType.Solid;
            brushInfo.HatchStyle = HatchStyle.BackwardDiagonal;
            fontInfo = new FontInfo(new FontFamily("Arial"), 12, 0); // Arial size 12, no bold/italic/underline
            textAlignment = TextAlignment.Left;
            shapeDrawType = ShapeDrawType.Outline;

            OnSelectedPathChanging();
            SelectedPath.Reset();
            OnSelectedPathChanged();
        }

        public DocumentEnvironment()
		{    
            selectedPath = new GraphicsPath();
            ResetToDefaults();
		}
	}
}
