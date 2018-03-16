using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tea_Launcher.Models
{
    public class GameFile
    {
        public string Path { get; set; }
        public string Hash { get; set; }

        public string WinPath()
        {
            return Path.Replace('/', '\\');
        }
    }
}
