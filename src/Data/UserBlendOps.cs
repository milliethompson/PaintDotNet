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
using System.Reflection;

namespace PaintDotNet
{
    /// <summary>
    /// This class contains all the render ops that can be used by the user
    /// to configure a layer's blending mode. It also contains helper
    /// functions to aid in enumerating and using these blend ops.
    /// 
    /// Each class should inherit from BinaryPixelOp, require no parameters
    /// for construction, and provide a static property called StaticName
    /// that provides a friendly name for UI purposes.
    /// 
    /// Each op should be marked with the [Serializable] attribute.
    /// 
    /// Also, all 3 overrides for Apply() should be implemented, for 
    /// performance reasons. All ops should also be sealed.
    /// 
    /// The default blend op for any layer is NormalBlendOp.
    /// 
    /// Credit for mathematical descriptions of many of the blend modes goes to
    /// a page on Pegtop Software's website called, "Blend Modes"
    /// http://www.pegtop.net/delphi/blendmodes/
    /// </summary>
    public sealed partial class UserBlendOps
    {
        private UserBlendOps()
        {
        }

        /// <summary>
        /// Returns an array of Type objects that lists all of the pixel ops contained
        /// within this class. You can then use Utility.GetStaticName to retrieve the
        /// value of the StaticName property.
        /// </summary>
        /// <returns></returns>
        public static Type[] GetBlendOps()
        {
            Type[] allTypes = typeof(UserBlendOps).GetNestedTypes();
            List<Type> types = new List<Type>(allTypes.Length);

            foreach (Type type in allTypes)
            {
                if (type.IsSubclassOf(typeof(UserBlendOp)) && !type.IsAbstract)
                {
                    types.Add(type);
                }
            }

            return types.ToArray();
        }

        public static UserBlendOp CreateBlendOp(Type opType)
        {
            ConstructorInfo ci = opType.GetConstructor(System.Type.EmptyTypes);
            UserBlendOp op = (UserBlendOp)ci.Invoke(null);
            return op;
        }

        public static UserBlendOp CreateDefaultBlendOp()
        {
            return new NormalBlendOp();
        }

        public static Type GetDefaultBlendOp()
        {
            return typeof(NormalBlendOp);
        }

    }
}
