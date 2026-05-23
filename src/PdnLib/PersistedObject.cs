/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for PersistedObject.
	/// </summary>
	public sealed class PersistedObject
        : IDisposable
	{
        private static ArrayList fileNames = ArrayList.Synchronized(new ArrayList());

        public static string[] FileNames
        {
            get
            {
                return (string[])fileNames.ToArray(typeof(string));
            }
        }

        // NOTE: We use a BSTR to hold the filename because we still need to be able to
        //       delete the file in our finalizer. However, the rules of finalizers say
        //       that you may not reference another object. Hence, we can not use a
        //       normal .NET System.String.
        private IntPtr bstrTempFileName = IntPtr.Zero;
        private WeakReference objectRef;
        private bool disposed = false;

        /// <summary>
        /// Gets the data stored in this instance of PersistedObject.
        /// </summary>
        /// <remarks>
        /// If the object has already been finalized and freed from memory, then this
        /// property will deserialize the object from disk before returning a new
        /// reference to it.
        /// </remarks>
        public object Object
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }

                object o;
                
                if (objectRef == null)
                {
                    o = null;
                }
                else
                {
                    o = objectRef.Target;
                }

                if (o == null)
                {
                    string tempFileName = Marshal.PtrToStringBSTR(this.bstrTempFileName);
                    FileStream stream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    BinaryFormatter formatter = new BinaryFormatter();
                    DeferredFormatter deferred = new DeferredFormatter();
                    StreamingContext context = new StreamingContext(formatter.Context.State, deferred);
                    formatter.Context = context;
                    object theObject = formatter.Deserialize(stream);
                    deferred.FinishDeserialization(stream);
                    this.objectRef = new WeakReference(theObject);
                    stream.Close();
                    return theObject;
                }
                else
                {
                    return o;
                }
            }
        }

        /// <summary>
        /// Gets the data stored in this instance of PersistedObject.
        /// </summary>
        /// <remarks>
        /// If the object has already been finalized and freed from memory, then
        /// this property will return null.
        /// </remarks>
        public object WeakObject
        {
            get
            {
                if (objectRef == null)
                {
                    return null;
                }
                else
                {
                    return objectRef.Target;
                }
            }
        }

        static PersistedObject()
        {
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        /// <summary>
        /// Creates a new instance of the PersistedObject class.
        /// </summary>
        /// <param name="theObject">
        /// The object to persist. It must be serializable.
        /// </param>
        /// <remarks>
        /// Deferred serialization via IDeferredSerializable and DeferredFormatter are supported,
        /// and the compression level will be set to none (zero).
        /// </remarks>
		public PersistedObject(object theObject)
		{
            this.objectRef = new WeakReference(theObject);
            string tempFileName = Path.GetTempFileName();
            fileNames.Add(tempFileName);
            this.bstrTempFileName = Marshal.StringToBSTR(tempFileName);

            FileStream stream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            BinaryFormatter formatter = new BinaryFormatter();
            DeferredFormatter deferred = new DeferredFormatter(0, null);
            StreamingContext context = new StreamingContext(formatter.Context.State, deferred);
            formatter.Context = context;
            formatter.Serialize(stream, theObject);
            deferred.FinishSerialization(stream);
            stream.Flush();
            stream.Close();
        }

        ~PersistedObject()
        {
            Dispose(false);
        }

        /// <summary>
        /// Ensures that the object held by this instance of PersistedObject is flushed to disk
        /// and freed from memory.
        /// </summary>
        public void Flush()
        {
            // At this point we assume the object has already been serialized to disk.
            object obj = this.WeakObject;
            IDisposable disposable = obj as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
            }

            this.objectRef = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                string tempFileName = Marshal.PtrToStringBSTR(this.bstrTempFileName);

                FileInfo fi = new FileInfo(tempFileName);

                if (fi.Exists)
                {
                    try
                    {
                        fi.Delete();
                    }

                    catch
                    {
                    }

                    try
                    {
                        fileNames.Remove(tempFileName);
                    }

                    catch
                    {
                    }
                }

                Marshal.FreeBSTR(this.bstrTempFileName);
                disposed = true;
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // Clean-up leftover persisted objects
            string[] fileNames = PersistedObject.FileNames;
            if (fileNames.Length != 0)
            {
                foreach (string fileName in fileNames)
                {
                    FileInfo fi = new FileInfo(fileName);

                    if (fi.Exists)
                    {
                        try
                        {
                            fi.Delete();
                        }
                        
                        catch
                        {
                        }                        
                    }
                    else
                    {
                        // File didn't exist
                    }
                }
            }
        }
    }
}
