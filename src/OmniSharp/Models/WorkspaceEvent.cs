namespace OmniSharp.Models
{
    public class WorkspaceEvent
    {
        public string Kind { get; set; }
        public string FileName { get; set; }
        public string VersionId { get; set; }
    }
}