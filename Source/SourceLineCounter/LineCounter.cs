using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
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
                
                _presenter = new Presenter();
                _presenter.CreateControls();
                _presenter.OnRecalculate += UpdateLineCount;

                await package.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                _dte = (DTE) await package.GetServiceAsync(typeof(DTE));
                
                if (_dte != null)
                {
                    _events = _dte.Events;
                    _solutionEvents = _events.SolutionEvents;
                    _solutionEvents.Opened += SolutionEventsOnOpened;
                    _solutionEvents.AfterClosing += SolutionEventsOnAfterClosing;
                }

                UpdateLineCount();
            }
        }

        private void SolutionEventsOnAfterClosing()
        {
            _presenter.SetLineCount(0);
        }

        private void SolutionEventsOnOpened()
        {
            UpdateLineCount();
        }

        private void UpdateLineCount()
        {
            int lineCount = LineCountCalculator.CalculateLineCount(_workspace);
            _presenter.SetLineCount(lineCount);
        }
    }
}
