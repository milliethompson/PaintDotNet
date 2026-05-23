/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Specialized;

namespace PaintDotNet
{
	/// <summary>
	/// This class exposes two types of metadata: system, and user.
	/// It is provided mostly for batching operations: loading all the data, modifying the copy,
	/// and then saving back all the data.
	/// "User" metadata is internally stored such that the keys are prefixed with a caret (^).
	/// </summary>
	public class MetaData
	{
        /// <summary>
        /// This is the name of the section where EXIF tags are stored. 
        /// </summary>
        /// <remarks>
        /// All entries in here are expected to be PropertyItem objects which were serialized 
        /// using PdnGraphics.SerializePropertyItem. The name of each entry in this section is
        /// irrelevant, as some EXIF tags are allowed to occur more than once. Thus, if you
        /// want to search for EXIF tags of a certain ID you will have to deserialize each
        /// one and compare the Id property.
        /// It is the responsibility of the FileType implementation to load and save these.
        /// </remarks>
        public const string ExifSectionName = "$exif";

        /// <summary>
        /// This is the name of the section where user-defined metadata may go.
        /// </summary>
        public const string UserSectionName = "$user";

        private NameValueCollection userMetaData;
        private const string sectionSeparator = ".";

        public string[] GetKeys(string section)
        {
            string sectionName = section + sectionSeparator;
            ArrayList keys = new ArrayList();

            foreach (string key in userMetaData.Keys)
            {
                if (key.StartsWith(sectionName))
                {
                    keys.Add(key.Substring(sectionName.Length));
                }
            }

            return (string[])keys.ToArray(typeof(string));
        }

        public string[] GetSections()
        {
            Set sections = new Set();
            
            foreach (string key in userMetaData.Keys)
            {
                int dotIndex = key.IndexOf(sectionSeparator);

                if (dotIndex != -1)
                {
                    string sectionName = key.Substring(0, dotIndex);                    

                    if (!sections.Contains(sectionName))
                    {
                        sections.Add(sectionName);
                    }
                }
            }

            return (string[])sections.ToArray(typeof(string));
        }

        /// <summary>
        /// Gets a value from the metadata collection.
        /// </summary>
        /// <param name="section">The logical section to retrieve from.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        /// <returns>A string containing the value, or null if the value wasn't present.</returns>
        public string GetValue(string section, string name)
        {
            return userMetaData.Get(section + sectionSeparator + name);
        }

        /// <summary>
        /// Removes a value from the metadata collection.
        /// </summary>
        /// <param name="section">The logical section to remove from.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        public void RemoveValue(string section, string name)
        {
            userMetaData.Remove(section + sectionSeparator + name);
        }

        public string GetUserValue(string name)
        {
            return GetValue(UserSectionName, name);
        }

        /// <summary>
        /// Sets a value in the metadata collection.
        /// </summary>
        /// <param name="section">The logical section to add or update date in.</param>
        /// <param name="name">The name of the value to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue(string section, string name, string value)
        {
            userMetaData.Set(section + sectionSeparator + name, value);
        }

        public void SetUserValue(string name, string value)
        {
            SetValue(MetaData.UserSectionName, name, value);
        }

		internal MetaData(NameValueCollection userMetaData)
		{
            this.userMetaData = userMetaData;
		}
	}
}
