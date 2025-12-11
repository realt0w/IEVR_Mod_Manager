using System;
using System.Collections.Generic;

namespace IEVRModManager.Models
{
    public class ModProfile
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public List<ModData> Mods { get; set; } = new List<ModData>();
        public string SelectedCpkName { get; set; } = string.Empty;
    }
}

