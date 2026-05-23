/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Provides methods and properties related to scanning and printing.
	/// </summary>
	/// <remarks>
    /// Originally adapted from http://www.codeproject.com/dotnet/wiascriptingdotnet.asp
    /// </remarks>
	public sealed class ScanningAndPrinting
	{
		private ScanningAndPrinting()
		{
		}

        /// <summary>
        /// Gets whether or not the scanning and printing features are available without
        /// taking into account whether a scanner or printer are actually connected.
        /// </summary>
        public static bool IsComponentAvailable
        {
            get
            {
                return IsWia2Available();
            }
        }

        /// <summary>
        /// Gets whether printing is possible.
        /// </summary>
        public static bool CanPrint
        {
            get
            {
                return IsWia2Available();
            }
        }

        /// <summary>
        /// Gets whether scanning is possible. The user must have a scanner connect for this to return true.
        /// </summary>
        public static bool CanScan
        {
            get
            {
                if (IsWia2Available())
                {
                    WIA.DeviceManagerClass dmc = new WIA.DeviceManagerClass();

                    if (dmc.DeviceInfos.Count > 0)
                    {
                        return true;
                    }
                }
               
                return false;
            }
        }

        /// <summary>
        /// Prints an image.
        /// </summary>
        /// <param name="fileName">The name of a file containing a bitmap to print.</param>
        public static void Print(Control owner, string fileName)
        {
            if (!CanPrint)
            {
                throw new InvalidOperationException("Printing is not available");
            }

            WIA.VectorClass vector = new WIA.VectorClass();
            object tempName_o = (object)fileName;
            vector.Add(ref tempName_o, 0);
            object vector_o = (object)vector;
            WIA.CommonDialogClass cdc = new WIA.CommonDialogClass();

            Form ownedForm = owner.FindForm();
            bool[] ownedFormsEnabled = null;

            // Disable the entire UI, otherwise it's possible to close PDN while the
            // print wizard is active! And then it crashes.
            if (ownedForm != null)
            {
                ownedFormsEnabled = new bool[ownedForm.OwnedForms.Length];

                for (int i = 0; i < ownedForm.OwnedForms.Length; ++i)
                {
                    ownedFormsEnabled[i] = ownedForm.OwnedForms[i].Enabled;
                    ownedForm.OwnedForms[i].Enabled = false;
                }

                owner.FindForm().Enabled = false;
            }

            cdc.ShowPhotoPrintingWizard(ref vector_o);

            if (ownedForm != null)
            {
                for (int i = 0; i < ownedForm.OwnedForms.Length; ++i)
                {
                    ownedForm.OwnedForms[i].Enabled = ownedFormsEnabled[i];
                }

                owner.FindForm().Enabled = true;
            }
        }

        /// <summary>
        /// Presents a user interface for scanning.
        /// </summary>
        /// <param name="fileName">A string to hold the value of the bitmap saved as a result of scanning.</param>
        /// <returns>The result of the scanning operation.</returns>
        public static ScanResult Scan(out string fileName)
        {
            if (!CanScan)
            {
                throw new InvalidOperationException("Scanning is not available");
            }

            ScanResult result;

            WIA.CommonDialogClass cdc = new WIA.CommonDialogClass();
            WIA.ImageFile imageFile = null;
            
            try
            {
                imageFile = cdc.ShowAcquireImage(WIA.WiaDeviceType.UnspecifiedDeviceType,
                                                 WIA.WiaImageIntent.UnspecifiedIntent,
                                                 WIA.WiaImageBias.MaximizeQuality,
                                                 "{00000000-0000-0000-0000-000000000000}",
                                                 true,
                                                 true,
                                                 false);
            }

            catch (System.Runtime.InteropServices.COMException)
            {
                result = ScanResult.DeviceBusy;
                imageFile = null;
            }

            if (imageFile != null)
            {
                string tempName = Path.GetTempFileName() + "." + imageFile.FileExtension;
                imageFile.SaveFile(tempName);
                fileName = tempName;
                result = ScanResult.Success;
            }
            else
            {
                fileName = null;
                result = ScanResult.UserCancelled;
            }

            return result;
        }

        private static bool IsWia2Available()
        {
            try
            {
                WIA.DeviceManagerClass dmc = new WIA.DeviceManagerClass();
                return true;
            }

            catch
            {
                return false;
            }
        }
    }
}
