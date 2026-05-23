/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Stores non-volatile name/value settings. These persist between sessions
	/// of the application.
	/// </summary>
	public sealed class Settings
	{
        private const string hkcuKey = @"SOFTWARE\Paint.NET";

        private Settings()
        {
        }

        private static RegistryKey CreateSettingsKey(bool writable)
        {
            RegistryKey rootKey = Microsoft.Win32.Registry.CurrentUser;
            RegistryKey softwareKey = null;

            try
            {
                softwareKey = rootKey.OpenSubKey(hkcuKey, writable);
            }

            catch (Exception)
            {
                softwareKey = null;
            }

            if (softwareKey == null)
            {
                try
                {
                    softwareKey = rootKey.CreateSubKey(hkcuKey);
                }

                catch (Exception)
                {
                    throw;
                }
            }

            return softwareKey;
        }   

        /// <summary>
        /// Deletes a settings key.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        public static void Delete(string key)
        {
            using (RegistryKey pdnKey = CreateSettingsKey(true))
            {
                pdnKey.DeleteValue(key, false);
            }
        }

        /// <summary>
        /// Deletes several settings keys.
        /// </summary>
        /// <param name="keys">The keys to delete.</param>
        public static void Delete(string[] keys)
        {
            using (RegistryKey pdnKey = CreateSettingsKey(true))
            {
                foreach (string key in keys)
                {
                    pdnKey.DeleteValue(key, false);
                }
            }
        }

        /// <summary>
        /// Retrieves the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <returns>The value of the key.</returns>
        public static object GetObject(string key)
        {
            using (RegistryKey pdnKey = CreateSettingsKey(false))
            {
                return pdnKey.GetValue(key);
            }
        }

        /// <summary>
        /// Retrieves the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <param name="defaultValue">The default value to use if the key doesn't exist.</param>
        /// <returns>The value of the key, or defaultValue if it didn't exist.</returns>
        public static object GetObject(string key, object defaultValue)
        {
            using (RegistryKey pdnKey = CreateSettingsKey(false))
            {
                return pdnKey.GetValue(key, defaultValue);
            }
        }

        /// <summary>
        /// Sets the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to set.</param>
        /// <param name="value">The new value of the key.</param>
        public static void SetObject(string key, object value)
        {
            using (RegistryKey pdnKey = CreateSettingsKey(true))
            {
                pdnKey.SetValue(key, value);
            }
        }

        /// <summary>
        /// Retrieves the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <returns>The value of the key.</returns>
        public static string GetString(string key)
        {
            return (string)GetObject(key);
        }

        /// <summary>
        /// Retrieves the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <param name="defaultValue">The default value to use if the key doesn't exist.</param>
        /// <returns>The value of the key, or defaultValue if it didn't exist.</returns>
        public static string GetString(string key, string defaultValue)
        {
            return (string)GetObject(key, defaultValue);
        }

        /// <summary>
        /// Sets the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to set.</param>
        /// <param name="value">The new value of the key.</param>
        public static void SetString(string key, string value)
        {
            SetObject(key, value);
        }

        /// <summary>
        /// Retrieves the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <returns>The value of the key.</returns>
        public static bool GetBoolean(string key)
        {
            return bool.Parse(GetString(key));
        }

        /// <summary>
        /// Retrieves the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <param name="defaultValue">The default value to use if the key doesn't exist.</param>
        /// <returns>The value of the key, or defaultValue if it didn't exist.</returns>
        public static bool GetBoolean(string key, bool defaultValue)
        {
            return bool.Parse(GetString(key, defaultValue.ToString()));
        }

        /// <summary>
        /// Sets the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to set.</param>
        /// <param name="value">The new value of the key.</param>
        public static void SetBoolean(string key, bool value)
        {
            SetString(key, value.ToString());
        }

        /// <summary>
        /// Retrieves the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <returns>The value of the key.</returns>
        public static Int32 GetInt32(string key)
        {
            return Int32.Parse(GetString(key));
        }

        /// <summary>
        /// Retrieves the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <param name="defaultValue">The default value to use if the key doesn't exist.</param>
        /// <returns>The value of the key, or defaultValue if it didn't exist.</returns>
        public static Int32 GetInt32(string key, Int32 defaultValue)
        {
            return Int32.Parse(GetString(key, defaultValue.ToString()));
        }

        /// <summary>
        /// Sets the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to set.</param>
        /// <param name="value">The new value of the key.</param>
        public static void SetInt32(string key, int value)
        {
            SetString(key, value.ToString());
        }

        /// <summary>
        /// Gets the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to retrieve.</param>
        /// <returns>The value of the key.</returns>
        /// <remarks>This method treats the key value as a stream of base64 encoded bytes that represent a PNG image.</remarks>
        public static Image GetImage(string key)
        {
            string imageB64 = GetString(key);
            byte[] pngBytes = Convert.FromBase64String(imageB64);
            MemoryStream ms = new MemoryStream(pngBytes);
            Image image = Image.FromStream(ms);
            ms.Close();
            return image;
        }

        /// <summary>
        /// Sets the value of a settings key.
        /// </summary>
        /// <param name="key">The name of the key to set.</param>
        /// <param name="value">The new value of the key.</param>
        /// <remarks>This method saves the key value as a stream of base64 encoded bytes that represent a PNG image.</remarks>
        public static void SetImage(string key, Image value)
        {
            MemoryStream ms = new MemoryStream();
            value.Save(ms, ImageFormat.Png);
            byte[] buffer = ms.GetBuffer();
            string base64 = Convert.ToBase64String(buffer);
            SetString(key, base64);
            ms.Close();
        }
	}
}
