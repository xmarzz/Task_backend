namespace SoftwareDashboardAPI.Models
{
    public class Software
    {
        public int Id { get; set; }
        public string AppName { get; set; } = null!;
        public string Version { get; set; } = null!;
        public string Status { get; set; } = "Running";
        public int OpenIssues { get; set; } = 0;
        public int ResolvedTickets { get; set; } = 0;
    }
}

