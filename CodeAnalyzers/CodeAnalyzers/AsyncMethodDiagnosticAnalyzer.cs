//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class AsyncMethodDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(id: "CA1001",
                                                                                           title: "Async methods should have Async as suffix in the names",
                                                                                           messageFormat: "The methods returning Task or Task<T> should have Async as suffix in the names.",
                                                                                           category: DiagnosticCategory.Naming,
                                                                                           defaultSeverity: DiagnosticSeverity.Warning,
                                                                                           isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Descriptor); }
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                if (compilationStartAnalysisContext.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var taskType = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var taskOfTType = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

                compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    if (symbolAnalysisContext.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var methodSymbolOriginalDefinition = symbolAnalysisContext.Symbol.OriginalDefinition as IMethodSymbol;

                    if (methodSymbolOriginalDefinition.ReturnType.OriginalDefinition != taskType &&
                        methodSymbolOriginalDefinition.ReturnType.OriginalDefinition != taskOfTType)
                    {
                        return;
                    }

                    if (methodSymbolOriginalDefinition.MethodKind == MethodKind.AnonymousFunction ||
                        methodSymbolOriginalDefinition.MethodKind == MethodKind.LambdaMethod ||
                        methodSymbolOriginalDefinition.MethodKind == MethodKind.Conversion ||
                        methodSymbolOriginalDefinition.MethodKind == MethodKind.UserDefinedOperator ||
                        methodSymbolOriginalDefinition.MethodKind == MethodKind.PropertyGet ||
                        methodSymbolOriginalDefinition.MethodKind == MethodKind.PropertySet ||
                        methodSymbolOriginalDefinition.MethodKind == MethodKind.BuiltinOperator)
                    {
                        return;
                    }

                    if (methodSymbolOriginalDefinition.IsOverride)
                    {
                        return;
                    }

                    foreach (var interfaceType in methodSymbolOriginalDefinition.ContainingType.AllInterfaces)
                    {
                        foreach (var interfaceMember in interfaceType.GetMembers())
                        {
                            if (methodSymbolOriginalDefinition.ContainingType.FindImplementationForInterfaceMember(interfaceMember) == methodSymbolOriginalDefinition)
                            {
                                return;
                            }
                        }
                    }

                    if (methodSymbolOriginalDefinition.Name.EndsWith("Async", StringComparison.Ordinal))
                    {
                        return;
                    }

                    foreach (var location in methodSymbolOriginalDefinition.Locations)
                    {
                        if (location.IsInSource && !location.SourceTree.IsGeneratedCode())
                        {
                            symbolAnalysisContext.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                        }
                    }
                }, SymbolKind.Method);
            });
        }
    }
}
