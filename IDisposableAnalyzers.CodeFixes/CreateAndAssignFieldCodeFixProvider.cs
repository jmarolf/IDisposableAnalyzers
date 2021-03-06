namespace IDisposableAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Simplification;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CreateAndAssignFieldCodeFixProvider))]
    [Shared]
    internal class CreateAndAssignFieldCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            IDISP001DisposeCreated.DiagnosticId,
            IDISP004DontIgnoreReturnValueOfTypeIDisposable.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText) ||
                    token.IsMissing)
                {
                    continue;
                }

                var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                if (diagnostic.Id == IDISP001DisposeCreated.DiagnosticId)
                {
                    var statement = node.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
                    if (statement?.FirstAncestor<ConstructorDeclarationSyntax>() != null &&
                        statement.Declaration.Variables.Count == 1 &&
                        statement.Declaration.Variables[0].Initializer != null)
                    {
                        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                                  .ConfigureAwait(false);
                        if (semanticModel.GetSymbolInfo(statement.Declaration.Type).Symbol is ITypeSymbol type)
                        {
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    "Create and assign field.",
                                    cancellationToken => ApplyAddUsingFixAsync(context, statement, type, cancellationToken),
                                    nameof(CreateAndAssignFieldCodeFixProvider)),
                                diagnostic);
                        }
                    }
                }

                if (diagnostic.Id == IDISP004DontIgnoreReturnValueOfTypeIDisposable.DiagnosticId)
                {
                    var statement = node.FirstAncestorOrSelf<ExpressionStatementSyntax>();
                    if (statement?.FirstAncestor<ConstructorDeclarationSyntax>() != null)
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Create and assign field.",
                                _ => ApplyAddUsingFixAsync(context, statement),
                                nameof(CreateAndAssignFieldCodeFixProvider)),
                            diagnostic);
                    }
                }
            }
        }

        private static async Task<Document> ApplyAddUsingFixAsync(CodeFixContext context, LocalDeclarationStatementSyntax statement, ITypeSymbol type, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
            var containingType = statement.FirstAncestor<TypeDeclarationSyntax>();
            var usesUnderscoreNames = containingType.UsesUnderscore(editor.SemanticModel, cancellationToken);
            var variableDeclarator = statement.Declaration.Variables[0];
            var identifier = variableDeclarator.Identifier;
            var field = editor.AddField(
                containingType,
                usesUnderscoreNames
                    ? "_" + identifier.ValueText
                    : identifier.ValueText,
                Accessibility.Private,
                DeclarationModifiers.ReadOnly,
                type,
                CancellationToken.None);

            var fieldAccess = usesUnderscoreNames
                ? SyntaxFactory.IdentifierName(field.Name())
                : SyntaxFactory.ParseExpression($"this.{field.Name()}");
            editor.ReplaceNode(
                statement,
                SyntaxFactory.ExpressionStatement(
                                 (ExpressionSyntax)editor.Generator.AssignmentStatement(
                                     fieldAccess,
                                     variableDeclarator.Initializer.Value))
                             .WithLeadingTrivia(statement.GetLeadingTrivia())
                             .WithTrailingTrivia(statement.GetTrailingTrivia()));

            return editor.GetChangedDocument();
        }

        private static async Task<Document> ApplyAddUsingFixAsync(CodeFixContext context, ExpressionStatementSyntax statement)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document).ConfigureAwait(false);
            var usesUnderscoreNames = editor.SemanticModel.SyntaxTree.GetRoot().UsesUnderscore(editor.SemanticModel, CancellationToken.None);
            var containingType = statement.FirstAncestor<TypeDeclarationSyntax>();

            var field = editor.AddField(
                containingType,
                usesUnderscoreNames
                    ? "_disposable"
                    : "disposable",
                Accessibility.Private,
                DeclarationModifiers.ReadOnly,
                SyntaxFactory.ParseTypeName("System.IDisposable").WithAdditionalAnnotations(Simplifier.Annotation),
                CancellationToken.None);

            var fieldAccess = usesUnderscoreNames
                ? SyntaxFactory.IdentifierName(field.Name())
                : SyntaxFactory.ParseExpression($"this.{field.Name()}");
            editor.ReplaceNode(
                statement,
                SyntaxFactory.ExpressionStatement(
                                 (ExpressionSyntax)editor.Generator.AssignmentStatement(
                                     fieldAccess,
                                     statement.Expression))
                             .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                             .WithTrailingTrivia(SyntaxFactory.ElasticMarker));
            return editor.GetChangedDocument();
        }
    }
}