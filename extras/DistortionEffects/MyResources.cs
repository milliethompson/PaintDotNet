using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;

namespace DistortionEffects
{
    class MyResources
    {
        public static ResourceManager MyResourceManager = new ResourceManager("DistortionEffects.MyResources", typeof(MyResources).Assembly);
    }
}
