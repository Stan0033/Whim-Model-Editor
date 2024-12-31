using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{
    public static class Tasker
    {
        public static void AddElements(List<int> target, List<int> input)
        {
            foreach (int i in input) {
                if (!target.Contains(i)) { target.Add(i); }
            }
        }
    }
}
