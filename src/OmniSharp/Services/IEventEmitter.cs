using OmniSharp.Models;

namespace OmniSharp.Services
{
    public interface IEventEmitter
    {
        void Emit(WorkspaceEvent evt);
    }
}