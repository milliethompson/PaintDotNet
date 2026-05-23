/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FileTypes.
    /// </summary>
    public sealed class FileTypes
    {
        private FileTypes()
        {
        }

        public static FileTypeCollection Collection
        {
            get
            {
                return new PdnFileTypes().GetFileTypeCollection();
            }
        }

        // Note: Since we only have one source to look at for file type factories, we hard
        //       code it. In the future, who knows. Don't remove this commented-out code.

        /*
        private static FileTypeCollection collection;
       
        public static FileTypeCollection Collection
        {
            get
            {
                if (collection == null)
                {
                    collection = GetFileTypes();
                }

                return collection;
            }
        }

        private static bool DoesTypeImplementInterface(Type derivedType, Type interfaceType)
        {
            Type[] interfaces = derivedType.GetInterfaces();

            foreach (Type type in interfaces)
            {
                if (type == interfaceType)
                {
                    return true;
                }
            }

            return false;
        }

        private static Type[] GetFileTypeFactoriesFromAssembly(Assembly assembly)
        {
            ArrayList fileTypeFactories = new ArrayList();

            foreach (Type type in assembly.GetTypes())
            {
                if (DoesTypeImplementInterface(type, typeof(IFileTypeFactory)) && !type.IsAbstract)
                {
                    fileTypeFactories.Add(type);
                }
            }

            return (Type[])fileTypeFactories.ToArray(typeof(Type));
        }

        private static FileTypeCollection GetFileTypes()
        {
            // For now we only look at the main assembly, but in the future maybe we'll search
            // external assemblies as well ...
            Type[] fileTypeFactories = GetFileTypeFactoriesFromAssembly(Assembly.GetExecutingAssembly());
            ArrayList allFileTypes = new ArrayList(10);
            
            foreach (Type type in fileTypeFactories)
            {
                ConstructorInfo ci = type.GetConstructor(System.Type.EmptyTypes);
                IFileTypeFactory factory = (IFileTypeFactory)ci.Invoke(null);
                FileType[] fileTypes = factory.GetFileTypeInstances();

                foreach (FileType fileType in fileTypes)
                {
                    allFileTypes.Add(fileType);
                }
            }

            return new FileTypeCollection(allFileTypes);
        }
        */

    }
}
