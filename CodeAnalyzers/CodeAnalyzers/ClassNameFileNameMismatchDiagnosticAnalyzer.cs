using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ClassNameFileNameMismatchDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(id: "CA1007",
                title: "Type name should match file name",
                messageFormat: "The file name '{0}' does not match type name",
                category: DiagnosticCategory.Naming,
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSyntaxTreeAction(OnSyntaxTreeVisit);
        }

        private void OnSyntaxTreeVisit(SyntaxTreeAnalysisContext context)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            CheckClassNameAndFileName((CSharpSyntaxTree)context.Tree, context);
        }

        private void CheckClassNameAndFileName(CSharpSyntaxTree syntaxTree, SyntaxTreeAnalysisContext compilationContext)
        {
            if (syntaxTree.IsGeneratedCode())
            {
                return;
            }

            var fileNameWithExtensions = Path.GetFileName(syntaxTree.FilePath);
            var fileName = fileNameWithExtensions.Split('.').First();

            var typeDecls = GetTopLevelType(syntaxTree.GetRoot());

            if (typeDecls.Count() == 1)
            {
                TypeDeclarationSyntax typeDecl = typeDecls.First();

                //skip partial class
                if (typeDecl is ClassDeclarationSyntax && ((ClassDeclarationSyntax)typeDecl).Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return;
                }

                if (typeDecl.Identifier.ValueText != fileName)
                {
                    compilationContext.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor,
                            typeDecl.Identifier.GetLocation(),
                            fileNameWithExtensions));
                }
            }
        }

        private IEnumerable<TypeDeclarationSyntax> GetTopLevelType(CSharpSyntaxNode node)
        {
            var typeDecls = node.ChildNodes().OfType<TypeDeclarationSyntax>();
            var namespaces = node.ChildNodes().OfType<NamespaceDeclarationSyntax>();
            return typeDecls.Concat(namespaces.SelectMany(GetTopLevelType));
        }
    }
}
