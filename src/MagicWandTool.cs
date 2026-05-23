using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for MagicWandTool.
	/// </summary>
	public class MagicWandTool 
		: Tool
	{
		//Variable declaration
		private ArrayList tracePoints = null;
		//private PdnGraphicsPath originalCopy;
		private PdnRegion currentRegion;
		private Surface surface;
		private int minX, maxX, minY, maxY;
		private ColorBgra currentColor;
		private bool[,] pixelsChecked;
		private Cursor cursorMouseDown, cursorMouseUp;

		public override char HotKey
		{
			get
			{
				return 'w';
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
		}

		#region Find extreme left and right
		private int FindLeft(int x, int y, bool[,] pixelsChecked)
		{
			int lowX = x;
			
			while (lowX >= 0 && 
				CheckColorBgra(surface[lowX, y], currentColor) && 
				currentRegion.IsVisible(lowX,y) && 
				!pixelsChecked[lowX, y])
			{
				pixelsChecked[lowX, y] = true;
				lowX--;
			}
			if(lowX < 0 || pixelsChecked[lowX,y] == false)
			{
				tracePoints.Add(new Point(lowX+1,y));	
			}
			
			if (lowX < minX)
			{
				minX = lowX;
			}

			return lowX+1;
		}

		private int FindRight(int x, int y, bool[,] pixelsChecked)
		{
			int highX = x;

			while ((highX < Workspace.Document.Width) && 
				currentRegion.IsVisible(highX, y) &&
				CheckColorBgra(surface[highX, y], currentColor) && 
				!pixelsChecked[highX, y])
			{
				pixelsChecked[highX, y] = true;
				highX++;
			}
			if(highX >= Workspace.Document.Width || pixelsChecked[highX,y] == false)
			{
				tracePoints.Add(new Point(highX,y));
			}
			if (highX >= maxX)
			{
				maxX = highX;
			}

			return highX;
		}

		private Point CheckPixel(int x, int y, bool [,] pixelsChecked)
		{
			if(!pixelsChecked[x,y])
			{
				if(currentRegion.IsVisible(x,y) && CheckColorBgra(surface[x,y],currentColor) )
				{
					pixelsChecked[x,y] = true;
					return new Point(x,y);
				}
			}
			return new Point(-1,-1);
		}

		private void FindExtremePoints(int x, int y)
		{

			int lowX = FindLeft(x, y, pixelsChecked);
			int highX = FindRight(x + 1, y, pixelsChecked);
			int i;
		
			// For Creating the Bounding Box///
			if (y >= maxY)
			{
				maxY = y+1;
			}
			else if (y < minY)
			{
				minY = y;
			}

			// Vertical Scan
			for (i = lowX+1; i < highX; i++)
			{
				if (y > 0 && 
					!pixelsChecked[i, y - 1] && 
					CheckColorBgra(surface[i, y - 1], currentColor))
				{
					FindExtremePoints(i,y-1);
				}
				else
					if(y > 0 && 
					!pixelsChecked[i,y-1] &&
					!CheckColorBgra(surface[i,y-1],currentColor))
				{
						pixelsChecked[i, y-1] = true;	
						tracePoints.Add(new Point(i,y-1));	
				}

				if (y < Workspace.Document.Size.Height - 1 &&
					!pixelsChecked[i, y + 1] && 
					CheckColorBgra(surface[i, y + 1], currentColor))
				{
					FindExtremePoints(i, y + 1);
				}
				else
					if(y < Workspace.Document.Size.Height -1 && 
					!pixelsChecked[i,y+1] &&
					!CheckColorBgra(surface[i,y+1],currentColor))
				{
					pixelsChecked[i, y+1] = true;
					tracePoints.Add(new Point(i,y+1));	
				}
			}

		}
		#endregion

		#region Tolerence Checking
		private bool CheckColorBgra(ColorBgra checkMe, ColorBgra source)
		{
			int ds = 0, t;
			t = checkMe.R - source.R;
			ds += t * t;
			t = checkMe.G - source.G;
			ds += t * t;
			t = checkMe.B - source.B;
			ds += t * t;
			return (ds < Workspace.Environment.Tolerance * Workspace.Environment.Tolerance);
		}
		#endregion

		private class ComparePointsByYThenX : IComparer
		{
			public int Compare(object a, object b)
			{
				Point point1 = (Point)a;
				Point point2 = (Point)b;

				if(point1.Y < point2.Y)
					return -1;
				else if(point1.Y > point2.Y)
					return 1;				
				if(point1.X < point2.X)
					return -1;
				else if(point1.X > point2.X)
					return 1;
				else
					return 0;
			}
		}

		protected PdnGraphicsPath MakePathFromBoundryPoints(ArrayList pts) 
		{
			PdnGraphicsPath retVal = new PdnGraphicsPath();
			IComparer pointCompare = new ComparePointsByYThenX();

			pts.Sort(pointCompare);

			while (pts.Count > 0) 
			{
				ArrayList figurePts = new ArrayList();
				int currIndex = 0;//for each figure, start with the first point in the pts array

				while (currIndex >= 0) 
				{
					Point currPt = (Point)pts[currIndex];
					pts.RemoveAt(currIndex);

					//find the bounds within the array of the surrounding 3 lines of points
					int start, end, count;
					start = ~pts.BinarySearch(0, currIndex, new Point(-1, currPt.Y - 1), pointCompare);
					end = ~pts.BinarySearch(currIndex, pts.Count - currIndex, new Point(-1, currPt.Y + 2), pointCompare);
					count = end - start;
					
					ArrayList candidates = new ArrayList(6);
					for (int y = -1; y <= 1; y++) 
					{
						//find the candidate points such that {pt.Y - 1 <= y <= pt.Y + 1}
						int ptIndex = pts.BinarySearch(start, count, new Point(currPt.X, currPt.Y + y), pointCompare);

						if (ptIndex >= 0) 
						{
							candidates.Add(ptIndex);
						} 
						else 
						{
							if (~ptIndex < pts.Count) 
							{
								candidates.Add(~ptIndex);
							}
							if (~ptIndex > 0) 
							{
								candidates.Add(~ptIndex - 1);
							}
						}
					}

					//look for closest point out of candidates
					//int bestDistanceSquared = int.MaxValue, bestIndex = -1;
					int bestDistanceSquared = 40, bestIndex = -1;
					foreach (object obj in candidates) 
					{
						int possIndex = (int)obj;
						Point possPt = (Point)pts[possIndex];

						int distanceSquared, linearDistance;

						linearDistance = possPt.Y - currPt.Y;
						distanceSquared = linearDistance * linearDistance;
						if (linearDistance > 1) 
						{
							continue;
						}
						linearDistance = possPt.X - currPt.X;
						distanceSquared += linearDistance * linearDistance;

						if (distanceSquared < bestDistanceSquared) 
						{
							bestIndex = possIndex;
							bestDistanceSquared = distanceSquared;
						}
					}

					//Add the point to the figure. If we just ran out of points,
					//then this will cause the while-condition to break
					figurePts.Add(currPt);
					currIndex = bestIndex;
				}
				//we now have a figure stored in 'figurePts'
				if (figurePts.Count > 2) 
				{
					retVal.StartFigure();
					retVal.AddPolygon((Point[])figurePts.ToArray(typeof(Point)));
					retVal.CloseFigure();
				}
			}
			return retVal;
		}
		private void MakeSelection()
		{
			if (tracePoints.Count > 2)
			{
				SelectionHistoryAction undoAction = new SelectionHistoryAction("sentinel", toolBarImage, Workspace);
				undoAction.Name = "Magic Wand Select";
				Workspace.History.PushNewAction(undoAction);

				Workspace.Environment.SelectedPath = MakePathFromBoundryPoints(tracePoints);
				Workspace.Environment.PerformSelectedPathChanging();
				Workspace.Environment.PerformSelectedPathChanged();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown (e);

			Cursor = cursorMouseDown;
			if (Utility.IsPointInRectangle(new Point(e.X, e.Y), Workspace.Document.Bounds))
			{

				Workspace.Environment.PerformSelectedPathChanging();
				Workspace.Environment.SelectedPath.Reset();


				Workspace.Environment.PerformSelectedPathChanged();


				// Create the Current Region
				if (!Workspace.Environment.IsSelectionEmpty)
				{
					currentRegion = Workspace.Environment.CreateSelectedRegion();
				}
				else
				{
					currentRegion = new PdnRegion(Workspace.Document.Bounds);
				}

				// See if the Mouseclick is valid fo schizzle
				if (!currentRegion.IsVisible(new Point(e.X,e.Y)))
				{
					return;
				}		

				surface = ((BitmapLayer)Workspace.ActiveLayer).Surface;
				pixelsChecked = new bool[Workspace.Document.Width + 1, Workspace.Document.Height + 1];
				currentColor = surface[e.X, e.Y];

				// These four variable help create the bounding box;
				minX = e.X;
				maxX = e.X; 
				minY = e.Y;
				maxY = e.Y;

				tracePoints = new ArrayList();

				// Bounding Box Pass
				FindExtremePoints(e.X, e.Y);

				Rectangle boundingBox = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
					
				pixelsChecked = new bool[Workspace.Document.Width + 1, Workspace.Document.Height + 1];

				// Draw Pass
				FindExtremePoints(e.X, e.Y);

				MakeSelection();
				pixelsChecked = null;
			}
			Cursor = cursorMouseUp;
		}

		public MagicWandTool(DocumentWorkspace parent) : base(parent)
		{
			name = "Magic Wand";
			toolBarImage = Utility.GetImageResource("Icons.MagicWandToolIcon.bmp");
			description = "Selects a Homogenous Color Region";
			helpText = "Click to select a region of similar color";
			cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.MagicWandToolCursor.cur"));
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.MagicWandToolCursorMouseDown.cur"));
			Cursor = cursorMouseUp;
		}
	}
}
