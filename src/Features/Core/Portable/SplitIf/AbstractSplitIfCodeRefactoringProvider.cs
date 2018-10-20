using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.SplitIf
{
    internal abstract class AbstractSplitIfCodeRefactoringProvider<TIfStatementSyntax, TExpressionSyntax>
        : CodeRefactoringProvider
        where TIfStatementSyntax : SyntaxNode
        where TExpressionSyntax : SyntaxNode
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
            {
                return;
            }

            var syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();
            var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var ifStatement = root.FindNode(context.Span).FirstAncestorOrSelf<TIfStatementSyntax>();
            if (ifStatement == null)
            {
                return;
            }

            // Nesting if clauses might change the logic if there's
            // an else clause present
            if (HasElseClause(ifStatement))
            {
                return;
            }

            if (!HasLogicalAndConditional(ifStatement))
            {
                return;
            }

            if (ifStatement.ContainsDiagnostics)
            {
                return;
            }

            // To prevent noisiness, only show this feature on the 'if' keyword of the if-statement.
            var token = ifStatement.GetFirstToken();
            if (!token.Span.Contains(context.Span))
            {
                return;
            }
            context.RegisterRefactoring(new MyCodeAction(Title, c => Task.FromResult(document)));
                    //UpdateDocumentAsync(root, document, ifStatement, switchSections)));
        }

        protected abstract string Title { get; }

        protected abstract bool HasElseClause(TIfStatementSyntax ifStatement);
        protected abstract bool HasLogicalAndConditional(TIfStatementSyntax ifStatement);

        protected sealed class MyCodeAction : CodeAction.DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }
        }
    }
}
