using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.SplitIf;

namespace Microsoft.CodeAnalysis.CSharp.SplitIf
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CSharpSplitIfCodeRefactoringProvider)), Shared]
    internal sealed class CSharpSplitIfCodeRefactoringProvider : AbstractSplitIfCodeRefactoringProvider<IfStatementSyntax, ExpressionStatementSyntax>
    {
        protected override string Title => CSharpFeaturesResources.Split_if;

        protected override bool HasElseClause(IfStatementSyntax ifStatement)
        {
            return ifStatement.Else != null;
        }

        protected override bool HasLogicalAndConditional(IfStatementSyntax ifStatement)
        {
            return ifStatement.Condition.IsKind(SyntaxKind.LogicalAndExpression);
        }
    }
}
