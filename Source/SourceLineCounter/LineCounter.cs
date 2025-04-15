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

        public async Task InitializeAsync(AsyncPackage package, CancellationToken cancellationToken)
        {
            var componentModel = (IComponentModel)await package.GetServiceAsync(typeof(SComponentModel));

            if (componentModel != null)
            {
                _workspace = componentModel.GetService<VisualStudioWorkspace>();
                _workspace.WorkspaceChanged += OnWorkspaceChanged;

                CreatePresenter();

                await RegisterSolutionEvents(package, cancellationToken);
            }
        }

        private void CreatePresenter()
        {
            _presenter = new Presenter();
            _presenter.CreateControls();
            _presenter.OnRecalculate += UpdateLineCount;
        }

        private async Task RegisterSolutionEvents(AsyncPackage package, CancellationToken cancellationToken)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var dte = (DTE)await package.GetServiceAsync(typeof(DTE));

            if (dte != null)
            {
                dte.Events.SolutionEvents.AfterClosing += SolutionEventsOnAfterClosing;
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
