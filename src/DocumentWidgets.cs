/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

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

        private ViewConfigStrip viewConfigStrip;
        public ViewConfigStrip ViewConfigStrip
        {
            get
            {
                return viewConfigStrip;
            }

            set
            {
                viewConfigStrip = value;
            }
        }

        private DrawConfigStrip drawConfigStrip;
        public DrawConfigStrip DrawConfigStrip
        {
            get
            {
                return this.drawConfigStrip;
            }

            set
            {
                this.drawConfigStrip = value;
            }
        }

        private CommonActionsStrip commonActionsStrip;
        public CommonActionsStrip CommonActionsStrip
        {
            get
            {
                return commonActionsStrip;
            }

            set
            {
                commonActionsStrip = value;
            }
        }

        private TextConfigStrip textConfigStrip;
        public TextConfigStrip TextConfigStrip
        {
            get
            {
                return this.textConfigStrip;
            }

            set
            {
                this.textConfigStrip = value;
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
