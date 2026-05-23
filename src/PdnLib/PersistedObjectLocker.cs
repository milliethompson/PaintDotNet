/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace PaintDotNet
{
    /// <summary>
    /// Privates a Guid->WeakReference mapping for PersistedObject instances.
    /// </summary>
    public sealed class PersistedObjectLocker
    {
        private static Hashtable guidToPO = new Hashtable();

        public static Guid Add(PersistedObject po)
        {
            Guid retGuid = Guid.NewGuid();
            WeakReference wr = new WeakReference(po);
            guidToPO.Add(retGuid, wr);
            return retGuid;
        }

        public static PersistedObject Get(Guid guid)
        {
            WeakReference wr = (WeakReference)guidToPO[guid];
            PersistedObject po;

            if (wr == null)
            {
                po = null;
            }
            else
            {
                object weakObj = wr.Target;

                if (weakObj == null)
                {
                    po = null;
                    guidToPO.Remove(guid);
                }
                else
                {
                    po = (PersistedObject)weakObj;
                }
            }

            return po;
        }

        public static void Remove(Guid guid)
        {
            guidToPO.Remove(guid);
        }

        private PersistedObjectLocker()
        {
        }
    }
}
