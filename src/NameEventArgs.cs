using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for NameEventArgs.
	/// </summary>
	public class NameEventArgs
        : System.EventArgs
	{
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
        }

		public NameEventArgs(string name)
		{
            this.name = name;
		}
	}
}
