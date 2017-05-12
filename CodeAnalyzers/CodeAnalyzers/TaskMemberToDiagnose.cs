using Microsoft.CodeAnalysis;

namespace CodeAnalyzers
{
    public class TaskMemberToDiagnose
    {
        public string Name { get; set; }

        public SymbolKind SymbolKind { get; set; }
    }
}
