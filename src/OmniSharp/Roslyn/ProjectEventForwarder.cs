using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using OmniSharp.AspNet5;
using OmniSharp.Models;
using OmniSharp.MSBuild;
using OmniSharp.Services;

namespace OmniSharp.Roslyn
{
    public class ProjectEventForwarder
    {
        private readonly AspNet5Context _aspnet5Context;
        private readonly MSBuildContext _msbuildContext;
        private readonly OmnisharpWorkspace _workspace;
        private readonly IEventEmitter _emitter;
        private readonly ISet<Tuple<string, WorkspaceChangeKind>> _queue = new HashSet<Tuple<string, WorkspaceChangeKind>>();
        private readonly object _lock = new object();

        public ProjectEventForwarder(AspNet5Context aspnet5Context, MSBuildContext msbuildContext, OmnisharpWorkspace workspace, IEventEmitter emitter)
        {
            _aspnet5Context = aspnet5Context;
            _msbuildContext = msbuildContext;
            _workspace = workspace;
            _emitter = emitter;
            _workspace.WorkspaceChanged += OnWorkspaceChanged;
        }

        private void OnWorkspaceChanged(object source, WorkspaceChangeEventArgs args)
        {

            Tuple<string, WorkspaceChangeKind> fileNameAndEventKind = null;

            switch (args.Kind)
            {
                case WorkspaceChangeKind.ProjectAdded:
                case WorkspaceChangeKind.ProjectChanged:
                case WorkspaceChangeKind.ProjectReloaded:
                    fileNameAndEventKind = Tuple.Create(args.NewSolution.GetProject(args.ProjectId).FilePath, args.Kind);
                    break;
                case WorkspaceChangeKind.ProjectRemoved:
                    fileNameAndEventKind = Tuple.Create(args.OldSolution.GetProject(args.ProjectId).FilePath, args.Kind);
                    break;
            }

            if (fileNameAndEventKind != null)
            {
                lock (_lock)
                {
                    var removed = _queue.Remove(fileNameAndEventKind);
                    _queue.Add(fileNameAndEventKind);
                    if (!removed)
                    {
                        Task.Factory.StartNew(async () =>
                        {
                            await Task.Delay(500);

                            lock (_lock)
                            {
                                _queue.Remove(fileNameAndEventKind);

                                object payload = null;
                                if (fileNameAndEventKind.Item2 != WorkspaceChangeKind.ProjectRemoved)
                                {
                                    payload = GetProjectInformation(fileNameAndEventKind.Item1);
                                }
                                _emitter.Emit(fileNameAndEventKind.Item2.ToString(), payload);
                            }
                        });
                    }
                }
            }
        }

        private ProjectInformationResponse GetProjectInformation(string fileName)
        {
            var msBuildContextProject = _msbuildContext.GetProject(fileName);
            var aspNet5ContextProject = _aspnet5Context.GetProject(fileName);

            return new ProjectInformationResponse
            {
                MsBuildProject = msBuildContextProject != null ? new MSBuildProject(msBuildContextProject) : null,
                AspNet5Project = aspNet5ContextProject != null ? new AspNet5Project(aspNet5ContextProject) : null
            };
        }
    }
}
