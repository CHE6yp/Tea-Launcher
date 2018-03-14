using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tea_Launcher.Models
{
    public class Game
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Screenshot { get; set; }
        public string Launcher { get; set; }
        public double FileCount { get; set; }
        public double DownloadingFile { get; set; }
    }
}
