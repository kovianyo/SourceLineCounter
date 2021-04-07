using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceLineCounter
{
    internal static class LineCountCalculator
    {
        public static int CalculateLineCount(Workspace workspace)
        {
            var documents = GetDocuments(workspace);

            int lineCount = GetLineCount(documents);

            return lineCount;
        }

        private static IEnumerable<Document> GetDocuments(Workspace workspace)
        {
            var documents = new List<Document>();

            foreach (var project in workspace.CurrentSolution.Projects)
            {
                string projectPath = Path.GetDirectoryName(project.FilePath) + @"\";
                string objPath = Path.Combine(projectPath, "obj") + @"\";
                var currentDocuments = project.Documents.ToArray();

                var projectDocuments = currentDocuments.Where(x => x.FilePath != null && x.FilePath.StartsWith(projectPath) && !x.FilePath.StartsWith(objPath)).ToArray();
                documents.AddRange(projectDocuments);
            }

            return documents;
        }

        private static int GetLineCount(IEnumerable<Document> documents)
        {
            int lineCount = 0;

            if (documents.Any())
            {
                foreach (var document in documents)
                {
                    var sourceText = document.GetTextAsync().Result;
                    int currentLineCount = sourceText.Lines.Count;
                    lineCount += currentLineCount;
                }
            }

            return lineCount;
        }
    }
}
