//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace CodeAnalyzers
{
    internal abstract class TaskDiagnosticAnalyzer : DiagnosticAnalyzer
    {
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

                compilationStartAnalysisContext.RegisterCodeBlockStartAction<SyntaxKind>(codeBlockStartAnalysisContext =>
                {
                    if (codeBlockStartAnalysisContext.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (codeBlockStartAnalysisContext.CodeBlock.SyntaxTree.IsGeneratedCode())
                    {
                        return;
                    }

                    codeBlockStartAnalysisContext.RegisterSyntaxNodeAction(syntaxNodeAnalysisContext =>
                    {
                        if (syntaxNodeAnalysisContext.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        var taskApiToDiagnose = GetTaskApiToDiagnose();

                        var identifierNameNode = (IdentifierNameSyntax)syntaxNodeAnalysisContext.Node;

                        if (identifierNameNode.Identifier.Text != taskApiToDiagnose.Name)
                        {
                            return;
                        }

                        var symbolInfo = syntaxNodeAnalysisContext.SemanticModel.GetSymbolInfo(identifierNameNode, syntaxNodeAnalysisContext.CancellationToken);

                        if (symbolInfo.Symbol == null || symbolInfo.Symbol.Kind != taskApiToDiagnose.SymbolKind)
                        {
                            return;
                        }

                        if (symbolInfo.Symbol.ContainingType.OriginalDefinition != taskType &&
                            symbolInfo.Symbol.ContainingType.OriginalDefinition != taskOfTType)
                        {
                            return;
                        }

                        syntaxNodeAnalysisContext.ReportDiagnostic(Diagnostic.Create(SupportedDiagnostics.First(), syntaxNodeAnalysisContext.Node.GetLocation()));

                    }, SyntaxKind.IdentifierName);
                });
            });
        }

        protected abstract TaskApiToDiagnose GetTaskApiToDiagnose();

        protected struct TaskApiToDiagnose
        {
            public string Name { get; set; }

            public SymbolKind SymbolKind { get; set; }
        }
    }
}
