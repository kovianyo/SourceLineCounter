using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Task = System.Threading.Tasks.Task;

namespace SourceLineCounter
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuidString)]
    public sealed class SourceLineCounterPackage : AsyncPackage
    {
        /// <summary>
        /// SourceLineCounterPackage GUID string.
        /// </summary>
        private const string PackageGuidString = "4b8763d8-fdf1-4fa6-84ec-92689dc51878";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var lineCounter = new LineCounter();

            await lineCounter.InitializeAsync(this, cancellationToken);
        }
    }
}
