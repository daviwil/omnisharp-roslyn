using OmniSharp.Models;

namespace OmniSharp.Services
{
    public class NullEventEmitter : IEventEmitter
    {
        public void Emit(WorkspaceEvent e)
        {
            // nothing
        }
    }
}