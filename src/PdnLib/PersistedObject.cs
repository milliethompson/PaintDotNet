/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class PersistedObject<T> 
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
        private string tempFileName;
        private WeakReference objectRef;
        private volatile object theObject; // only non-null until the object is saved to disk, after with objectRef is our only reference
        private bool disposed = false;

        /// <summary>
        /// Gets the data stored in this instance of PersistedObject.
        /// </summary>
        /// <remarks>
        /// If the object has already been finalized and freed from memory, then this
        /// property will deserialize the object from disk before returning a new
        /// reference to it.
        /// </remarks>
        public T Object
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }

                T o;
                
                if (objectRef == null)
                {
                    o = default(T);
                }
                else
                {
                    o = (T)objectRef.Target;
                }

                if (o == null)
                {
                    string tempFileName = Marshal.PtrToStringBSTR(this.bstrTempFileName);
                    FileStream stream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    BinaryFormatter formatter = new BinaryFormatter();
                    DeferredFormatter deferred = new DeferredFormatter();
                    StreamingContext context = new StreamingContext(formatter.Context.State, deferred);
                    formatter.Context = context;
                    T theObject = (T)formatter.Deserialize(stream);
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
        public T WeakObject
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }

                if (objectRef == null)
                {
                    return default(T);
                }
                else
                {
                    return (T)objectRef.Target;
                }
            }
        }

        /// <summary>
        /// Ensures that the object held by this instance of PersistedObject is flushed to disk
        /// and freed from memory.
        /// </summary>
        public void Flush()
        {
            while (this.theObject != null)
            {
                Thread.Sleep(10);
            }

            // At this point we assume the object has already been serialized to disk.
            object obj = this.WeakObject;
            IDisposable disposable = obj as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }

            this.objectRef = null;
        }

        /// <summary>
        /// Creates a new instance of the PersistedObject class.
        /// </summary>
        /// <param name="theObject">
        /// The object to persist. It must be serializable.
        /// </param>
        /// <param name="background">
        /// Whether to serialize to disk in the background. If you specify true, then you must make
        /// sure not to mutate or dispose theObject.</param>
        /// <remarks>
        /// Deferred serialization via IDeferredSerializable and DeferredFormatter are supported,
        /// and the compression level will be set to none (zero) if background is false. The
        /// compression level will be one if background is true.
        /// </remarks>
        public PersistedObject(T theObject, bool background)
        {
            this.objectRef = new WeakReference(theObject);
            this.tempFileName = FileSystem.GetTempFileName();
            fileNames.Add(tempFileName);
            this.bstrTempFileName = Marshal.StringToBSTR(tempFileName);
            this.theObject = theObject;

            if (background)
            {
                Thread thread = new Thread(new ThreadStart(PersistToDisk));
                thread.Priority = ThreadPriority.BelowNormal;
                thread.IsBackground = false;
                thread.Start();
            }
            else
            {
                PersistToDisk();
            }
        }

        private void PersistToDisk(object notUsed)
        {
            PersistToDisk();
        }

        private void PersistToDisk()
        {
            FileStream stream = new FileStream(this.tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            BinaryFormatter formatter = new BinaryFormatter();
            DeferredFormatter deferred = new DeferredFormatter(false, null);
            StreamingContext context = new StreamingContext(formatter.Context.State, deferred);

            formatter.Context = context;
            formatter.Serialize(stream, this.theObject);
            deferred.FinishSerialization(stream);
            stream.Flush();
            stream.Close();

            this.theObject = null;
        }

        ~PersistedObject()
        {
            Dispose(false);
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
                if (disposing)
                {
                    while (this.theObject != null)
                    {
                        Thread.Sleep(10);
                    }
                }

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

        static PersistedObject()
        {
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // Clean-up leftover persisted objects
            string[] fileNames = PersistedObject<T>.FileNames;

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
