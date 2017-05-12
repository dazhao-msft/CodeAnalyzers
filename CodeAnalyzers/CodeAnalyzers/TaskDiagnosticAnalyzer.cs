using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace CodeAnalyzers
{
    internal abstract class TaskDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        protected abstract TaskMemberToDiagnose TaskMemberToDiagnose { get; }

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

                        var identifierNameNode = (IdentifierNameSyntax)syntaxNodeAnalysisContext.Node;

                        if (identifierNameNode.Identifier.Text != TaskMemberToDiagnose.Name)
                        {
                            return;
                        }

                        var symbolInfo = syntaxNodeAnalysisContext.SemanticModel.GetSymbolInfo(identifierNameNode, syntaxNodeAnalysisContext.CancellationToken);

                        if (symbolInfo.Symbol == null || symbolInfo.Symbol.Kind != TaskMemberToDiagnose.SymbolKind)
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
    }
}
