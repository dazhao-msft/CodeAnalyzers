using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class TaskContinueWithDiagnosticAnalyzer : TaskDiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(id: "CA1003",
                                                                                           title: "Avoid using System.Threading.Tasks.ContinueWith()",
                                                                                           messageFormat: "Avoid using System.Threading.Tasks.ContinueWith(). Use async/await instead.",
                                                                                           category: DiagnosticCategory.Usage,
                                                                                           defaultSeverity: DiagnosticSeverity.Warning,
                                                                                           isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        protected override TaskMemberToDiagnose TaskMemberToDiagnose => new TaskMemberToDiagnose() { Name = "ContinueWith", SymbolKind = SymbolKind.Method };
    }
}

