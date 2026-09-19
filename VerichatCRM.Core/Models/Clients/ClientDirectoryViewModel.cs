using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Clients
{
    public class ClientDirectoryViewModel
    {
        public List<ClientViewModel> Clients { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? SectorFilter { get; set; }
        public List<string> AvailableSectors { get; set; } = new();
    }
}
