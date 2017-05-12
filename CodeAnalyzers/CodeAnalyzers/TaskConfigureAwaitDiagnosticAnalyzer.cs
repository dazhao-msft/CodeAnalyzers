using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class TaskConfigureAwaitDiagnosticAnalyzer : TaskDiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(id: "CA1002",
                                                                                           title: "Avoid using System.Threading.Tasks.ConfigureAwait()",
                                                                                           messageFormat: "Incorrect usage of ConfigureAwait(false) in application code could cause issues. " +
                                                                                                          "For example, if the subsequent operations need to update UI, the calls very likely must happen on the UI thread. " +
                                                                                                          "ConfigureAwait(false) would cause those calls to run on a background thread and then introduce threading affinity issues.",
                                                                                           category: DiagnosticCategory.Usage,
                                                                                           defaultSeverity: DiagnosticSeverity.Warning,
                                                                                           isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        protected override TaskMemberToDiagnose TaskMemberToDiagnose => new TaskMemberToDiagnose() { Name = "ConfigureAwait", SymbolKind = SymbolKind.Method };
    }
}
