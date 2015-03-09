using Microsoft.CodeAnalysis;
using OmniSharp.Models;
using OmniSharp.Services;

namespace OmniSharp.Roslyn
{
    public class WorkspaceEventForwarder
    {
        private readonly OmnisharpWorkspace _workspace;
        private readonly IEventEmitter _emitter;

        public WorkspaceEventForwarder(OmnisharpWorkspace workspace, IEventEmitter emitter)
        {
            _workspace = workspace;
            _emitter = emitter;
            _workspace.WorkspaceChanged += OnWorkspaceChanged;
        }

        private async void OnWorkspaceChanged(object source, WorkspaceChangeEventArgs args)
        {
            WorkspaceEvent payload = null;
            switch (args.Kind)
            {
                case WorkspaceChangeKind.DocumentAdded:
                case WorkspaceChangeKind.DocumentChanged:
                case WorkspaceChangeKind.DocumentReloaded:
                    payload = new WorkspaceEvent()
                    {
                        Kind = args.Kind.ToString(),
                        FileName = args.NewSolution.GetDocument(args.DocumentId).FilePath,
                        VersionId = (await args.NewSolution.GetDocument(args.DocumentId).GetTextVersionAsync()).ToString()
                    };
                    break;
                case WorkspaceChangeKind.DocumentRemoved:
                    payload = new WorkspaceEvent()
                    {
                        Kind = args.Kind.ToString(),
                        FileName = args.OldSolution.GetDocument(args.DocumentId).FilePath,
                        VersionId = (await args.OldSolution.GetDocument(args.DocumentId).GetTextVersionAsync()).ToString()
                    };
                    break;
                case WorkspaceChangeKind.ProjectAdded:
                case WorkspaceChangeKind.ProjectChanged:
                case WorkspaceChangeKind.ProjectReloaded:
                    payload = new WorkspaceEvent()
                    {
                        Kind = args.Kind.ToString(),
                        FileName = args.NewSolution.GetProject(args.ProjectId).FilePath,
                        VersionId = args.NewSolution.GetProject(args.ProjectId).Version.ToString()
                    };
                    break;
                case WorkspaceChangeKind.ProjectRemoved:
                    payload = new WorkspaceEvent()
                    {
                        Kind = args.Kind.ToString(),
                        FileName = args.OldSolution.GetProject(args.ProjectId).FilePath,
                        VersionId = args.NewSolution.GetProject(args.ProjectId).Version.ToString()
                    };
                    break;
            }

            if (payload != null)
            {
                _emitter.Emit(payload);
            }
        }
    }
}