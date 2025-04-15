using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace SourceLineCounter
{
    internal class LineCounter
    {
        private VisualStudioWorkspace _workspace;

        private Presenter _presenter;

        private DTE _dte;
        private Events _events;
        private SolutionEvents _solutionEvents;


        public async Task InitializeAsync(AsyncPackage package, CancellationToken cancellationToken)
        {
            var componentModel = (IComponentModel)await package.GetServiceAsync(typeof(SComponentModel));

            if (componentModel != null)
            {
                _workspace = componentModel.GetService<VisualStudioWorkspace>();
                _workspace.WorkspaceChanged += OnWorkspaceChanged;

                _presenter = new Presenter();
                _presenter.CreateControls();
                _presenter.OnRecalculate += UpdateLineCount;

                await package.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

                _dte = (DTE)await package.GetServiceAsync(typeof(DTE));

                if (_dte != null)
                {
                    _events = _dte.Events;
                    _solutionEvents = _events.SolutionEvents;
                    _solutionEvents.AfterClosing += SolutionEventsOnAfterClosing;
                }
            }
        }

        private void SolutionEventsOnAfterClosing()
        {
            _presenter.SetLineCount(0);
        }

        private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.Kind == WorkspaceChangeKind.SolutionChanged)
            {
                UpdateLineCount();
            }
        }

        private void UpdateLineCount()
        {
            int lineCount = LineCountCalculator.CalculateLineCount(_workspace);

            _presenter.SetLineCount(lineCount);
        }
    }
}
