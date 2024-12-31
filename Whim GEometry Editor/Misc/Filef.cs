using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{
    public static class Filef
    {
        public static void CreateFileAndDirectory(string filePath)
        {
            string folder = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(folder);
            File.Create(filePath);
        }

    }

}
