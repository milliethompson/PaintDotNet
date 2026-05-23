using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Ink;
using Microsoft.StylusInput;
using Microsoft.StylusInput.PluginData;

namespace PaintDotNet
{
	/// <summary>
	/// 
	/// </summary>
	public class StylusReader : IStylusAsyncPlugin
	{
		private PointF ratio = PointF.Empty;
		private DocumentView subject;
		private Control attachedControl;
		public StylusReader(DocumentView subject, Control attached)
		{
			Graphics g = subject.CreateGraphics();
			attachedControl = attached;
			this.ratio = new PointF(g.DpiX / 2540.0f, g.DpiY / 2540.0f);
			this.subject = subject;
		}
	
		private PointF Himetric2PointF(int x, int y) 
		{
			return new PointF(x * ratio.X, y * ratio.Y);
		}

		private MouseButtons Status2MouseButtons(int status) 
		{
			if ((status & 0x1) != 0) 
			{
				if ((status & 0x8) != 0) 
				{
					return MouseButtons.Right;
				} 
				else
				{
					return MouseButtons.Left;
				}
			} 
			else
			{
				return MouseButtons.None;
			}
		}

		MouseButtons lastbutton = MouseButtons.None;
		private void Interpret(StylusDataBase data, int index)
		{
			Point offset = attachedControl.PointToScreen(new Point(0, 0));
			PointF relativePosition = Himetric2PointF(data[index], data[index + 1]);
			PointF position = subject.ScreenToDocument(new PointF(relativePosition.X + offset.X, relativePosition.Y + offset.Y));
			float pressure = (data.PacketPropertyCount > 3 ? data[index + 2] : 255.0f) / 255.0f;
			int status = data[index + data.PacketPropertyCount - 1];
			MouseButtons button = Status2MouseButtons(status);

			if (lastbutton != button) 
			{
				//if a button was previously down, MouseUp it.
				if (lastbutton != MouseButtons.None) 
				{
					subject.PerformDocumentMouseUp(new StylusEventArgs(lastbutton, 1, position.X, position.Y, 0, pressure));
				}
				//if a new button was pushed, MouseDown it.
				if (button != MouseButtons.None) 
				{
					subject.PerformDocumentMouseDown(new StylusEventArgs(button, 1, position.X, position.Y, 0, pressure));
				}
			}
			//regardless of the button states, send a new MouseMove
			subject.PerformDocumentMouseMove(new StylusEventArgs(button, 1, position.X, position.Y, 0, pressure));
			lastbutton = button;
		}

		#region IStylusAsyncPlugin Members

		public Microsoft.StylusInput.DataInterestMask DataInterest
		{
			get
			{
				return DataInterestMask.AllStylusData;
			}
		}

		public void Packets(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.PacketsData data)
		{
			for (int i = 0; i < data.Count; i += data.PacketPropertyCount) 
			{
				Interpret(data, i);
			}
		}

		public void InAirPackets(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.InAirPacketsData data)
		{
			for (int i = 0; i < data.Count; i += data.PacketPropertyCount) 
			{
				Interpret(data, i);
			}
		}

		public void StylusDown(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusDownData data)
		{
			Interpret(data, 0);
		}

		public void StylusUp(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusUpData data)
		{
			Interpret(data, 0);
		}

		public void StylusButtonDown(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusButtonDownData data)
		{
		}

		public void StylusButtonUp(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusButtonUpData data)
		{
		}

		public void StylusInRange(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusInRangeData data)
		{
		}

		public void StylusOutOfRange(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusOutOfRangeData data)
		{
		}

		public void RealTimeStylusEnabled(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.RealTimeStylusEnabledData data)
		{
		}

		public void RealTimeStylusDisabled(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.RealTimeStylusDisabledData data)
		{
		}

		public void Error(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.ErrorData data)
		{
		}

		public void SystemGesture(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.SystemGestureData data)
		{
		}

		public void TabletAdded(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.TabletAddedData data)
		{
		}

		public void TabletRemoved(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.TabletRemovedData data)
		{
		}

		public void CustomStylusDataAdded(Microsoft.StylusInput.RealTimeStylus sender, Microsoft.StylusInput.PluginData.CustomStylusData data)
		{
		}

		#endregion
	}
}
