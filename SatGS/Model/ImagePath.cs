using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.Model
{
    internal struct ImagePath
    {
        public string FileName
        {
            get
            {
                return FullPath.Split('\\').Last();
            }
        }
        public string FullPath { get; }
        public ImagePath(string path)
        {
            FullPath = path;
        }
    }
}
