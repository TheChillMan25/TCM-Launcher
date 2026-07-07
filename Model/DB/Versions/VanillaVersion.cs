using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TCM_Launcher.Model.DB.Versions
{
    public class VanillaVersion : VersionBase
    {
        public string Type { get; set; }
    }
}
