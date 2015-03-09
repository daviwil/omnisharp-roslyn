using OmniSharp.Models;
using OmniSharp.Stdio.Protocol;
using OmniSharp.Stdio.Services;

namespace OmniSharp.Services
{
    public class StdioEventEmitter : IEventEmitter
    {
        private readonly ISharedTextWriter _writer;

        public StdioEventEmitter(ISharedTextWriter writer)
        {
            _writer = writer;
        }

        public void Emit(WorkspaceEvent evt)
        {
            _writer.WriteLineAsync(new EventPacket()
            {
                Event = evt.Kind,
                Body = evt
            });
        }
    }
}