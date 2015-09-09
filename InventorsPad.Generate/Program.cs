using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorsPad.Generate
{
    class Program
    {
        static void Main(string[] args)
        {
            var gen = new InventorsPad();
            gen.Save();
        }
    }
}
