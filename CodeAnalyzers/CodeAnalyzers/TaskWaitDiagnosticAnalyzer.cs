//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class TaskWaitDiagnosticAnalyzer : TaskDiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(id: "CA1004",
                                                                                           title: "Avoid calling System.Threading.Tasks.Task.Wait()",
                                                                                           messageFormat: "Avoid calling System.Threading.Tasks.Task.Wait(), which blocks callers.",
                                                                                           category: DiagnosticCategory.Usage,
                                                                                           defaultSeverity: DiagnosticSeverity.Warning,
                                                                                           isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Descriptor); }
        }

        protected override TaskApiToDiagnose GetTaskApiToDiagnose()
        {
            return new TaskApiToDiagnose() { Name = "Wait", SymbolKind = SymbolKind.Method };
        }
    }
}

