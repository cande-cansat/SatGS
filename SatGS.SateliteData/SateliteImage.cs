using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.SateliteData
{
    public class SateliteImage
    {
        public string FileName => Path.Split('\\').Last();
        public string Path { get; }
        public SateliteImage(string path)
        {
            Path = path;
        }
    }

    public class SateliteImageFactory
    {
        public static SateliteImage Create(byte[] payload)
        {
            var fileName = $"{DateTime.Now:MM.dd_HH.mm.ss}.png";
            var path = $"{Directory.GetCurrentDirectory()}\\Images\\{fileName}";
            
            using(var writer = new BinaryWriter(File.OpenWrite(path)))
            {
                writer.Write(payload, 0, payload.Length);
                writer.Flush();
            }

            return new SateliteImage(path);
        }
    }
}
