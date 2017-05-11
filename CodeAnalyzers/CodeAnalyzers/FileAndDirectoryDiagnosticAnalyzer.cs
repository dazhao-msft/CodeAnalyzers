//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class FileAndDirectoryDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(id: "CA1006",
                                                                                           title: "Avoid using System.IO.File and System.IO.Directory",
                                                                                           messageFormat: "Avoid using System.IO.File and System.IO.Directory. Use FileSystem.Current instead.",
                                                                                           category: DiagnosticCategory.Design,
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

                var fileType = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName("System.IO.File");
                var directoryType = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName("System.IO.Directory");

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

                        if (identifierNameNode.Identifier.Text != "File" && identifierNameNode.Identifier.Text != "Directory")
                        {
                            return;
                        }

                        var symbolInfo = syntaxNodeAnalysisContext.SemanticModel.GetSymbolInfo(identifierNameNode, syntaxNodeAnalysisContext.CancellationToken);

                        if (symbolInfo.Symbol == null || symbolInfo.Symbol.Kind != SymbolKind.NamedType)
                        {
                            return;
                        }

                        var typeSymbol = (INamedTypeSymbol)symbolInfo.Symbol;

                        if (typeSymbol != fileType && typeSymbol != directoryType)
                        {
                            return;
                        }

                        syntaxNodeAnalysisContext.ReportDiagnostic(Diagnostic.Create(Descriptor, syntaxNodeAnalysisContext.Node.GetLocation()));

                    }, SyntaxKind.IdentifierName);
                });
            });
        }
    }
}
