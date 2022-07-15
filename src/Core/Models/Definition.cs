using System.Collections.Generic;

namespace Core.Models
{
    public class Definition
    {
        public string Name { get; set; }
        public string InstallDir { get; set; }
        //public List<Depot> InstalledDepots { get; set; }
        public override string ToString()
        {
            return $"{Name} ({InstallDir})";
        }
    }
}
