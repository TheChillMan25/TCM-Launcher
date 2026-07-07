using System;
using System.Collections.Generic;
using System.Text;

namespace TCM_Launcher.Model.DB.Versions
{
    public class ForgeVersion : VersionBase
    {
        public string MCVersion { get; set; }
        public bool Recommended { get; set; }
    }
}
