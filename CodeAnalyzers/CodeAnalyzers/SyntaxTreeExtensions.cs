using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace CodeAnalyzers
{
    internal static class SyntaxTreeExtensions
    {
        public static bool IsGeneratedCode(this SyntaxTree syntaxTree)
        {
            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            string directoryName = Path.GetDirectoryName(syntaxTree.FilePath);

            if (directoryName.IndexOf(@"obj\", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            string fileName = Path.GetFileName(syntaxTree.FilePath);

            if (fileName.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".i.cs", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
