using System;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// This class is used to hold references to many of the UI elements
    /// that are privately encapsulated in the DocumentWorkspace class.
    /// This allows other program elements to access these objects without
    /// breaking OO best practices.
    /// </summary>
    public class DocumentWidgets
    {
        private DocumentWorkspace workspace;

        private FlowPanel topDock;
        public FlowPanel TopDock
        {
            get
            {
                return topDock;
            }

            set
            {
                topDock = value;
            }
        }

        private PenConfigWidget penConfigWidget;
        public PenConfigWidget PenConfigWidget
        {
            get
            {
                return penConfigWidget;
            }

            set
            {
                penConfigWidget = value;
            }
        }

        private BrushConfigWidget brushConfigWidget;
        public BrushConfigWidget BrushConfigWidget
        {
            get
            {
                return brushConfigWidget;
            }

            set
            {
                brushConfigWidget = value;
            }
        }

        private ShapeDrawTypeConfigWidget shapeDrawTypeConfigWidget;
        public ShapeDrawTypeConfigWidget ShapeDrawTypeConfigWidget 
        {
            get
            {
                return shapeDrawTypeConfigWidget;
            }

            set
            {
                shapeDrawTypeConfigWidget = value;
            }
        }

        private CommonActionsWidget commonActionsWidget;
        public CommonActionsWidget CommonActionsWidget
        {
            get
            {
                return commonActionsWidget;
            }

            set
            {
                commonActionsWidget = value;
            }
        }

        private TextConfigWidget textConfigWidget;
        public TextConfigWidget TextConfigWidget
        {
            get
            {
                return textConfigWidget;
            }

            set
            {
                textConfigWidget = value;
            }
        }

        private MainToolBarForm mainToolBarForm;
        public MainToolBarForm MainToolBarForm
        {
            get
            {
                return mainToolBarForm;
            }

            set
            {
                mainToolBarForm = value;
            }
        }

        public MainToolBar MainToolBar
        {
            get
            {
                return mainToolBarForm.MainToolBar;
            }
        }

        public ColorDisplayWidget ColorDisplayWidget
        {
            get
            {
                return mainToolBarForm.MainToolBar.ColorDisplay;
            }
        }

        private LayerForm layerForm;
        public LayerForm LayerForm
        {
            get
            {
                return layerForm;
            }

            set
            {
                layerForm = value;
            }
        }

        public LayerControl LayerControl
        {
            get
            {
                return layerForm.LayerControl;
            }
        }

        private HistoryForm historyForm;
        public HistoryForm HistoryForm
        {
            get
            {
                return historyForm;
            }

            set
            {
                historyForm = value;
            }
        }

        public HistoryControl HistoryControl
        {
            get
            {
                return HistoryForm.HistoryControl;
            }
        }

        private ColorsForm colorsForm;
        public ColorsForm ColorsForm
        {
            get
            {
                return colorsForm;
            }

            set
            {
                colorsForm = value;
            }
        }


        public DocumentWidgets(DocumentWorkspace workspace)
        {
            this.workspace = workspace;
        }
    }
}
