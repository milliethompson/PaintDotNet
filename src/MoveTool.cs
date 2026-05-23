/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

// Leave uncommented to always use bilinear rendering. Otherwise nearest neighbor
// is used while interacting with the selection via the mouse, for better performance.
//#define ALWAYSHIGHQUALITY

using PaintDotNet.Threading;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for MoveTool.
    /// </summary>
    public class MoveTool
        : MoveToolBase
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("MoveTool.Name");
            }
        }

        private bool highQuality = false;
        private BitmapLayer activeLayer;
        private RenderArgs renderArgs;
        private bool didPaste = false;

        private MoveToolContext ourContext
        {
            get
            {
                return (MoveToolContext)this.context;
            }
        }

        [Serializable]
        private sealed class MoveToolContext
            : MoveToolBase.Context
        {
            [NonSerialized]
            private MaskedSurface liftedPixels;

            [NonSerialized]
            public PersistedObject<MaskedSurface> poLiftedPixels;

            public Guid poLiftedPixelsGuid;

            public MaskedSurface LiftedPixels
            {
                get
                {
                    if (this.liftedPixels == null)
                    {
                        if (this.poLiftedPixels != null)
                        {
                            this.liftedPixels = (MaskedSurface)poLiftedPixels.Object;
                        }
                    }

                    return this.liftedPixels;
                }

                set
                {
                    if (value == null)
                    {
                        this.poLiftedPixels = null;
                        this.liftedPixels = null;
                    }
                    else
                    {
                        this.poLiftedPixels = new PersistedObject<MaskedSurface>(value, true);
                        this.poLiftedPixelsGuid = PersistedObjectLocker.Add(this.poLiftedPixels);
                        this.liftedPixels = null;
                    }
                }
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("poLiftedPixelsGuid", this.poLiftedPixelsGuid);
            }

            public MoveToolContext(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                this.poLiftedPixelsGuid = (Guid)info.GetValue("poLiftedPixelsGuid", typeof(Guid));
                this.poLiftedPixels = PersistedObjectLocker.Get<MaskedSurface>(this.poLiftedPixelsGuid);
            }

            public MoveToolContext(MoveToolContext cloneMe)
                : base(cloneMe)
            {
                this.poLiftedPixelsGuid = cloneMe.poLiftedPixelsGuid;
                this.poLiftedPixels = cloneMe.poLiftedPixels; // do not clone
                this.liftedPixels = cloneMe.liftedPixels; // do not clone
            }

            public MoveToolContext()
            {
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                }
            }

            public override object Clone()
            {
                return new MoveToolContext(this);
            }
        }

        private class ContextHistoryAction
            : ToolHistoryAction
        {
            private int layerIndex;
            private object liftedPixelsRef; // prevent this from being GC'd
            
            [Serializable]
            private class OurHistoryActionData
                : HistoryActionData
            {
                public MoveToolContext context;

                public OurHistoryActionData(Context context)
                {
                    this.context = (MoveToolContext)context.Clone();
                }
            }

            protected override HistoryAction OnToolUndo()
            {
                MoveTool moveTool = Workspace.Environment.Tool as MoveTool;

                if (moveTool == null)
                {
                    throw new InvalidOperationException("Current Tool is not the MoveTool");
                }

                ContextHistoryAction cha = new ContextHistoryAction(Workspace, moveTool.ourContext, this.Name, this.Image);
                OurHistoryActionData ohad = (OurHistoryActionData)this.Data;
                Context newContext = ohad.context;

                if (moveTool.Workspace.ActiveLayerIndex != this.layerIndex)
                {
                    bool oldDOLC = moveTool.deactivateOnLayerChange;
                    moveTool.deactivateOnLayerChange = false;
                    moveTool.Workspace.ActiveLayerIndex = this.layerIndex;
                    moveTool.deactivateOnLayerChange = oldDOLC;
                    moveTool.activeLayer = (BitmapLayer)moveTool.Workspace.ActiveLayer;
                    moveTool.renderArgs = new RenderArgs(moveTool.activeLayer.Surface);
                    moveTool.ClearSavedMemory();
                }

                moveTool.context.Dispose();
                moveTool.context = newContext;

                moveTool.DestroyNubs();

                if (moveTool.context.lifted)
                {
                    moveTool.PositionNubs(moveTool.context.currentMode);
                }

                return cha;
            }

            public ContextHistoryAction(DocumentWorkspace workspace, MoveToolContext context, string name, Image image)
                : base(workspace, name, image)
            {
                this.Data = new OurHistoryActionData(context);
                this.layerIndex = workspace.ActiveLayerIndex;
                this.liftedPixelsRef = context.poLiftedPixels;
            }
        }

        protected override void OnActivate()
        {
            this.moveToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.MoveToolCursor.cur"));
            this.Cursor = this.moveToolCursor;

            this.context.lifted = false;
            this.ourContext.LiftedPixels = null;
            this.context.offset = new Point(0, 0);
            this.context.liftedBounds = Workspace.Environment.Selection.GetBoundsF();
            this.activeLayer = (BitmapLayer)Workspace.ActiveLayer;

            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }

            this.renderArgs = new RenderArgs(activeLayer.Surface);

            this.tracking = false;
            PositionNubs(this.context.currentMode);

#if ALWAYSHIGHQUALITY
            this.highQuality = true;
#endif

            base.OnActivate ();
        }

        protected override void OnDeactivate()
        {
            if (this.moveToolCursor != null)
            {
                this.moveToolCursor.Dispose();
                this.moveToolCursor = null;
            }

            if (context.lifted)
            {   
                Drop();
            }

            this.activeLayer = null;

            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }

            this.tracking = false;
            DestroyNubs();
            base.OnDeactivate();
        }

        protected override void Drop()
        {
            RestoreSavedRegion();

            PdnRegion regionCopy = Workspace.Environment.Selection.CreateRegion();

            using (PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(regionCopy, 
                       Utility.DefaultSimplificationFactor, 2))
            {
                HistoryAction bitmapAction2 = new BitmapHistoryAction(Name, Image, Workspace, 
                    Workspace.ActiveLayerIndex, simplifiedRegion);

                bool oldHQ = this.highQuality;
                this.highQuality = true;
                Render(this.context.offset, true);
                this.highQuality = oldHQ;
                this.currentHistoryActions.Add(bitmapAction2);

                activeLayer.Invalidate(simplifiedRegion);
                Update();
            }

            regionCopy.Dispose();
            regionCopy = null;

            ContextHistoryAction cha = new ContextHistoryAction(this.Workspace, this.ourContext, this.Name, this.Image);
            this.currentHistoryActions.Add(cha);

            string name;
            Image image;

            if (didPaste)
            {
                name = EnumWrapper.EnumValueToLocalizedName(typeof(CommonAction), CommonAction.Paste);
                image = PdnResources.GetImage("Icons.MenuEditPasteIcon.png");
            }
            else
            {
                name = this.Name;
                image = this.Image;
            }

            didPaste = false;

            SelectionHistoryAction sha = new SelectionHistoryAction(this.Name, this.Image, this.Workspace);
            this.currentHistoryActions.Add(sha);

            this.context.Dispose();
            this.context = new MoveToolContext();

            this.FlushHistoryActions(PdnResources.GetString("MoveTool.HistoryAction.DropPixels"));
        }

        protected override void OnSelectionChanging()
        {
            base.OnSelectionChanging();

            if (!dontDrop)
            {
                if (context.lifted)
                {
                    Drop();
                }

                if (tracking)
                {
                    tracking = false;
                }
            }
        }

        protected override void OnSelectionChanged()
        {
            if (!context.lifted)
            {
                DestroyNubs();
                PositionNubs(this.context.currentMode);
            }

            base.OnSelectionChanged();
        }

        /// <summary>
        /// Provided as a special entry point so that Paste can work well.
        /// </summary>
        /// <param name="surface">What you want to paste.</param>
        /// <param name="offset">Where you want to paste it.</param>
        public void PasteMouseDown(SurfaceForClipboard sfc, Point offset)
        {
            if (this.context.lifted)
            {
                Drop();
            }

            MaskedSurface pixels = sfc.MaskedSurface;
            PdnGraphicsPath pastePath = pixels.CreatePath();

            PdnRegion pasteRegion = new PdnRegion(pastePath);

            PdnRegion simplifiedPasteRegion = Utility.SimplifyAndInflateRegion(pasteRegion);
            HistoryAction bitmapAction = new BitmapHistoryAction(Name, Image, Workspace, Workspace.ActiveLayerIndex, simplifiedPasteRegion);
            this.currentHistoryActions.Add(bitmapAction);

            PushContextHistoryAction();

            this.context.seriesGuid = Guid.NewGuid();
            this.context.currentMode = Mode.Translate;
            this.context.startEdge = Edge.None;
            this.context.startAngle = 0.0f;

            this.ourContext.LiftedPixels = pixels;
            this.context.lifted = true;
            this.context.liftTransform = new Matrix();
            this.context.liftTransform.Reset();
            this.context.deltaTransform = new Matrix();
            this.context.deltaTransform.Reset();
            this.context.offset = new Point(0, 0);

            bool oldDD = this.dontDrop;
            this.dontDrop = true;

            SelectionHistoryAction sha = new SelectionHistoryAction(null, null, Workspace);
            this.currentHistoryActions.Add(sha);

            Workspace.Environment.Selection.PerformChanging();
            Workspace.Environment.Selection.Reset();
            Workspace.Environment.Selection.SetContinuation(pastePath, CombineMode.Replace, true);
            pastePath = null;
            Workspace.Environment.Selection.CommitContinuation();
            Workspace.Environment.Selection.PerformChanged();

            //ContextHistoryAction cha2 = new ContextHistoryAction(Workspace, this.ourContext, null, null);
            //this.currentHistoryActions.Add(cha2);
            PushContextHistoryAction();

            this.context.liftedBounds = Workspace.Environment.Selection.GetBoundsF(false);
            this.context.startBounds = this.context.liftedBounds;
            this.context.baseTransform = new Matrix(); //Workspace.Environment.Selection.GetInterimTransformCopy();
            this.context.baseTransform.Reset();
            this.tracking = true;

            this.dontDrop = oldDD;
            this.didPaste = true;

            this.tracking = true;

            DestroyNubs();
            PositionNubs(this.context.currentMode);

            // we use the value 70,000 to simulate mouse input because that's guaranteed to be out of bounds of where
            // the mouse can actually be -- PDN is limited to 65536 x 65536 images by design
            MouseEventArgs mea1 = new MouseEventArgs(MouseButtons.Left, 0, 70000, 70000, 0);
            MouseEventArgs mea2 = new MouseEventArgs(MouseButtons.Left, 0, 70000 + offset.X, 70000 + offset.Y, 0);
            this.context.startMouseXY = new Point(70000, 70000);

            OnMouseDown(mea1);
            OnMouseMove(mea2);
            OnMouseUp(mea2);
        }

        protected override void OnLift(MouseEventArgs e)
        {
            PdnGraphicsPath liftPath = Workspace.Environment.Selection.CreatePath();
            PdnRegion liftRegion = Workspace.Environment.Selection.CreateRegion();

            this.ourContext.LiftedPixels = new MaskedSurface(activeLayer.Surface, liftPath);
            HistoryAction bitmapAction = new BitmapHistoryAction(Name, Image, Workspace, Workspace.ActiveLayerIndex, this.ourContext.poLiftedPixelsGuid);

            this.currentHistoryActions.Add(bitmapAction);
            
            // If the user is holding down the control key, we want to *copy* the pixels
            // and not "lift and erase"
            if ((ModifierKeys & Keys.Control) == Keys.None)
            {
                ColorBgra fill = Workspace.Environment.BackColor;
                fill.A = 0;
                UnaryPixelOp op = new UnaryPixelOps.Constant(fill);
                op.Apply(renderArgs.Surface, liftRegion);
            }

            liftRegion.Dispose();
            liftRegion = null;

            liftPath.Dispose();
            liftPath = null;
        }

        protected override void PushContextHistoryAction()
        {
            ContextHistoryAction cha = new ContextHistoryAction(this.Workspace, this.ourContext, null, null);
            this.currentHistoryActions.Add(cha);
        }

        protected override void Render(Point newOffset, bool useNewOffset)
        {
            Render(newOffset, useNewOffset, true);
        }

        protected void Render(Point newOffset, bool useNewOffset, bool saveRegion)
        {
            Rectangle saveBounds = Workspace.Environment.Selection.GetBounds();
            PdnRegion selectedRegion = Workspace.Environment.Selection.CreateRegion();
            PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(selectedRegion);

            if (saveRegion)
            {
                SaveRegion(simplifiedRegion, saveBounds);
            }
            
            this.ourContext.LiftedPixels.Draw(
                renderArgs.Surface, 
                this.context.deltaTransform, 
                this.highQuality);
            
            activeLayer.Invalidate(simplifiedRegion);
            PositionNubs(this.context.currentMode);
            
            simplifiedRegion.Dispose();
            selectedRegion.Dispose();
        }

        protected override void PreRender()
        {
            RestoreSavedRegion();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!tracking)
            {
                return;
            }

            this.highQuality = true;
            OnMouseMove(e);

#if !ALWAYSHIGHQUALITY
            this.highQuality = false;
#endif

            this.rotateNub.Visible = false;
            tracking = false;
            PositionNubs(this.context.currentMode);

            string resourceName;
            switch (this.context.currentMode)
            {
                default:
                    throw new InvalidEnumArgumentException();

                case Mode.Rotate:
                    resourceName = "MoveTool.HistoryAction.Rotate";
                    break;

                case Mode.Scale:
                    resourceName = "MoveTool.HistoryAction.Scale";
                    break;

                case Mode.Translate:
                    resourceName = "MoveTool.HistoryAction.Translate";
                    break;
            }

            this.context.startAngle += this.angleDelta;

            if (this.context.liftTransform == null)
            {
                this.context.liftTransform = new Matrix();
            }

            this.context.liftTransform.Reset();
            this.context.liftTransform.Multiply(this.context.deltaTransform, MatrixOrder.Append);
            
            string actionName = PdnResources.GetString(resourceName);
            FlushHistoryActions(actionName);
        }

        private void FlushHistoryActions(string name)
        {
            if (this.currentHistoryActions.Count > 0)
            {
                CompoundHistoryAction cha = new CompoundHistoryAction(null, null,
                    this.currentHistoryActions.ToArray());

                string haName;
                Image image;

                if (this.didPaste)
                {
                    haName = PdnResources.GetString("CommonAction.Paste");
                    image = PdnResources.GetImage("Icons.MenuEditPasteIcon.png");
                    this.didPaste = false;
                }
                else
                {
                    if (name == null)
                    {
                        haName = this.Name;
                    }
                    else
                    {
                        haName = name;
                    }

                    image = this.Image;
                }

                CompoundToolHistoryAction ctha = new CompoundToolHistoryAction(cha, this.Workspace, haName, image);

                ctha.SeriesGuid = context.seriesGuid;
                Workspace.History.PushNewAction(ctha);

                this.currentHistoryActions.Clear();
            }
        }

        public MoveTool(DocumentWorkspace workspace)
            : base(workspace,
                   PdnResources.GetImage("Icons.MoveToolIcon.png"),
                   MoveTool.StaticName,
                   PdnResources.GetString("MoveTool.HelpText"), // "Click and drag to move a selected region",
                   'm')
        {
            this.context = new MoveToolContext();
            this.enableOutline = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                DisposeImage();
                DestroyNubs();

                if (this.renderArgs != null)
                {
                    this.renderArgs.Dispose();
                    this.renderArgs = null;
                }

                if (this.context != null)
                {
                    this.context.Dispose();
                    this.context = null;
                }
            }
        }

        protected override void OnExecutingHistoryAction(ExecutingHistoryActionEventArgs e)
        {
            this.dontDrop = true;

            RestoreSavedRegion();
            ClearSavedMemory();

            if (e.MayAlterSuspendTool)
            {
                e.SuspendTool = false;
            }
        }

        protected override void OnExecutedHistoryAction(ExecutedHistoryActionEventArgs e)
        {
            if (context.lifted)
            {
                bool oldHQ = this.highQuality;
                this.highQuality = false;
                Render(context.offset, true);
                ClearSavedMemory();
                this.highQuality = oldHQ;
            }
            else
            {
                DestroyNubs();
                PositionNubs(this.context.currentMode);
            }

            this.dontDrop = false;
        }

        protected override void OnFinishedHistoryStepGroup()
        {
            if (context.lifted)
            {
                bool oldHQ = this.highQuality;
                this.highQuality = true;
                Render(context.offset, true, false);
                this.highQuality = oldHQ;
            }

            base.OnFinishedHistoryStepGroup();
        }
    }
}
