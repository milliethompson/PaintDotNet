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
using System.Collections.Generic;

namespace PaintDotNet
{
    /// <summary>
    /// Privates a Guid->WeakReference mapping for PersistedObject instances.
    /// This is used by the Move Selected Pixels tool so that it can have many
    /// instances of its history and context data that refer to the same
    /// MaskedSurface, but only have it serializing and deserializing exactly
    /// once (to and from the same file).
    /// </summary>
    public sealed class PersistedObjectLocker
    {
        private static Dictionary<Guid, WeakReference> guidToPO = new Dictionary<Guid, WeakReference>();

        public static Guid Add<T>(PersistedObject<T> po)
        {
            Guid retGuid = Guid.NewGuid();
            WeakReference wr = new WeakReference(po);
            guidToPO.Add(retGuid, wr);
            return retGuid;
        }

        public static PersistedObject<T> Get<T>(Guid guid)
        {
            WeakReference wr;
            guidToPO.TryGetValue(guid, out wr);
            PersistedObject<T> po;

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
                    po = (PersistedObject<T>)weakObj;
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
