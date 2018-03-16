using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tea_Launcher.Models
{
    public class GameFile
    {
        private string _winPath;
        private string _unixPath;
        public string WinPath { get { return _winPath; } set { _winPath = value; _unixPath = value.Replace('\\', '/'); } }
        public string UnixPath { get { return _unixPath; } set { _unixPath = value; _winPath = value.Replace('/', '\\'); } }
        public string Hash { get; set; }

    }
}
