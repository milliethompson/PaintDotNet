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
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Provides static method and properties for obtaining all the FileType objects
    /// responsible for loading and saving Document instances. Loads FileType plugins
    /// too.
    /// </summary>
    public sealed class FileTypes
    {
        private FileTypes()
        {
        }

        private static FileTypeCollection collection;
       
        public static FileTypeCollection GetFileTypes()
        {
            if (collection == null)
            {
                collection = LoadFileTypes();
            }

            return collection;
        }

        private static bool IsInterfaceImplemented(Type derivedType, Type interfaceType)
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
                if (IsInterfaceImplemented(type, typeof(IFileTypeFactory)) && !type.IsAbstract)
                {
                    fileTypeFactories.Add(type);
                }
            }

            return (Type[])fileTypeFactories.ToArray(typeof(Type));
        }

        private static Type[] GetFileTypeFactoriesFromAssemblies(ICollection assemblies)
        {
            ArrayList allFactories = new ArrayList();

            foreach (Assembly assembly in assemblies)
            {
                Type[] factories = GetFileTypeFactoriesFromAssembly(assembly);

                foreach (Type type in factories)
                {
                    allFactories.Add(type);
                }
            }

            return (Type[])allFactories.ToArray(typeof(Type));
        }

        private static FileTypeCollection LoadFileTypes()
        {
            ArrayList assemblies = new ArrayList();

            // add the built-in IFileTypeFactory house
            assemblies.Add(typeof(FileType).Assembly);

            // enumerate the assemblies inside the FileTypes directory
            string homeDir = Path.GetDirectoryName(Application.ExecutablePath);
            string fileTypesDir = Path.Combine(homeDir, "FileTypes");
            bool dirExists;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(fileTypesDir);
                dirExists = dirInfo.Exists;
            }

            catch
            {
                dirExists = false;
            }

            if (dirExists)
            {
                foreach (string fileName in Directory.GetFiles(fileTypesDir, "*.dll"))
                {
                    bool success;
                    Assembly pluginAssembly = null;

                    try
                    {
                        pluginAssembly = Assembly.LoadFrom(fileName);
                        success = true;
                    }

                    catch (Exception)
                    {
                        success = false;
                    }

                    if (success)
                    {
                        assemblies.Add(pluginAssembly);
                    }

                }
            }

            // Get all the IFileTypeFactory implementations
            Type[] fileTypeFactories = GetFileTypeFactoriesFromAssemblies(assemblies);
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
    }
}
