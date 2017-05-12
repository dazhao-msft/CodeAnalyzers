using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class TaskResultDiagnosticAnalyzer : TaskDiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(id: "CA1005",
                                                                                           title: "Avoid calling System.Threading.Tasks.Task<TResult>.Result",
                                                                                           messageFormat: "Avoid calling System.Threading.Tasks.Task<TResult>.Result, which blocks callers.",
                                                                                           category: DiagnosticCategory.Usage,
                                                                                           defaultSeverity: DiagnosticSeverity.Warning,
                                                                                           isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        protected override TaskMemberToDiagnose TaskMemberToDiagnose => new TaskMemberToDiagnose() { Name = "Result", SymbolKind = SymbolKind.Property };
    }
}

