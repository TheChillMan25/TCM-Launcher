using System;
using System.Collections.Generic;
using System.Text;

namespace TCM_Launcher.Model.DB
{
    public class ProfileSettings
    {
        public int Id { get; set; }
        public string GameProfileId { get; set; }
        public GameProfile GameProfile { get; set; }
        public int? Ram { get; set; }
        public string? JVMArgs { get; set; }
    }
}
