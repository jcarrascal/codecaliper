namespace DotNetBytes.CodeCaliper.Core.RoslynAnalysis
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class CSharpCodeMetricsWalker : CSharpSyntaxWalker
    {
        private readonly ProcessContext mContext;
        private readonly Stack<dynamic> mScopesStack = new Stack<dynamic>();
        private dynamic mCurrentScope;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CSharpCodeMetricsWalker" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CSharpCodeMetricsWalker(ProcessContext context)
        {
            this.mContext = context;
        }

        /// <summary>
        ///     Visits the specified file to calculate cyclomatic complexity.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="syntaxTree">The syntax tree.</param>
        public void Visit(dynamic file, SyntaxTree syntaxTree)
        {
            this.mCurrentScope = file;
            this.mCurrentScope.CyclomaticComplexity = 0;
            this.mCurrentScope.SourceLinesOfCode = 0;
            this.Visit(syntaxTree.GetRoot());
        }

        /// <inheritdoc />
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            dynamic type = new ExpandoObject();
            type.Identifier = this.mCurrentScope.Identifier + ":" + node.Identifier +
                              node.TypeParameterList;
            type.CyclomaticComplexity = 0;
            type.SourceLinesOfCode = 0;

            this.mScopesStack.Push(this.mCurrentScope);
            this.mCurrentScope = type;
            base.VisitClassDeclaration(node);
            this.mCurrentScope = this.mScopesStack.Pop();
            this.mCurrentScope.CyclomaticComplexity += type.CyclomaticComplexity;
            this.mCurrentScope.SourceLinesOfCode += type.SourceLinesOfCode;
            this.mContext[type.Identifier] = type;
        }

        /// <inheritdoc />
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            dynamic function = new ExpandoObject();
            function.Identifier = this.mCurrentScope.Identifier + ":" + node.Identifier +
                                  node.TypeParameterList + "(" + StringifyParameterList(node.ParameterList) + ")" +
                                  node.ReturnType;
            function.CyclomaticComplexity = 1;
            function.SourceLinesOfCode = 0;

            this.mScopesStack.Push(this.mCurrentScope);
            this.mCurrentScope = function;
            base.VisitMethodDeclaration(node);
            this.mCurrentScope = this.mScopesStack.Pop();
            this.mCurrentScope.CyclomaticComplexity += function.CyclomaticComplexity;
            this.mCurrentScope.SourceLinesOfCode += function.SourceLinesOfCode;
            this.mContext[function.Identifier] = function;
        }

        public override void VisitBlock(BlockSyntax node)
        {
            this.mCurrentScope.SourceLinesOfCode += node.Statements.Count;
            base.VisitBlock(node);
        }

        /// <inheritdoc />
        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            string parentIdentifier = null;
            if (node.Parent.Parent is PropertyDeclarationSyntax property)
                parentIdentifier = property.Identifier.ToString();
            else if (node.Parent.Parent is IndexerDeclarationSyntax indexer)
                parentIdentifier = "[" + StringifyParameterList(indexer.ParameterList) + "]" + indexer.Type;

            dynamic function = new ExpandoObject();
            function.Identifier = this.mCurrentScope.Identifier + ":" + parentIdentifier + ":" + node.Keyword;
            function.CyclomaticComplexity = 1;
            function.SourceLinesOfCode = 0;

            this.mScopesStack.Push(this.mCurrentScope);
            this.mCurrentScope = function;
            base.VisitAccessorDeclaration(node);
            this.mCurrentScope = this.mScopesStack.Pop();
            this.mCurrentScope.CyclomaticComplexity += function.CyclomaticComplexity;
            this.mCurrentScope.SourceLinesOfCode += function.SourceLinesOfCode;
            this.mContext[function.Identifier] = function;
        }

        private static string StringifyParameterList(BaseParameterListSyntax parameterList)
        {
            return string.Join(", ", parameterList.Parameters.Select(p => p.Type));
        }

        /// <inheritdoc />
        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            dynamic function = new ExpandoObject();
            function.Identifier = this.mCurrentScope.Identifier + ":" + node.Identifier +
                                  node.ParameterList;
            function.CyclomaticComplexity = 1;
            function.SourceLinesOfCode = 0;

            this.mScopesStack.Push(this.mCurrentScope);
            this.mCurrentScope = function;
            base.VisitConstructorDeclaration(node);
            this.mCurrentScope = this.mScopesStack.Pop();
            this.mCurrentScope.CyclomaticComplexity += function.CyclomaticComplexity;
            this.mCurrentScope.SourceLinesOfCode += function.SourceLinesOfCode;
            this.mContext[function.Identifier] = function;
        }

        /// <inheritdoc />
        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            dynamic function = new ExpandoObject();
            function.Identifier = this.mCurrentScope.Identifier + ":~" + node.Identifier +
                                  node.ParameterList;
            function.CyclomaticComplexity = 1;
            function.SourceLinesOfCode = 0;

            this.mScopesStack.Push(this.mCurrentScope);
            this.mCurrentScope = function;
            base.VisitDestructorDeclaration(node);
            this.mCurrentScope = this.mScopesStack.Pop();
            this.mCurrentScope.CyclomaticComplexity += function.CyclomaticComplexity;
            this.mCurrentScope.SourceLinesOfCode += function.SourceLinesOfCode;
            this.mContext[function.Identifier] = function;
        }

        /// <inheritdoc />
        public override void VisitIfStatement(IfStatementSyntax node)
        {
            var complexity = CountLogicalOperators(node.Condition) + 1;
            this.mCurrentScope.CyclomaticComplexity += complexity;
            base.VisitIfStatement(node);
        }

        private static int CountLogicalOperators(ExpressionSyntax node)
        {
            if (node is BinaryExpressionSyntax condition)
            {
                var op = condition.OperatorToken.ValueText;
                return CountLogicalOperators(condition.Left) + CountLogicalOperators(condition.Right)
                       + (op == "&&" || op == "||" ? 1 : 0);
            }

            return 0;
        }

        /// <inheritdoc />
        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            var complexity = CountLogicalOperators(node.Condition) + 1;
            this.mCurrentScope.CyclomaticComplexity += complexity;
            base.VisitWhileStatement(node);
        }

        /// <inheritdoc />
        public override void VisitDoStatement(DoStatementSyntax node)
        {
            var complexity = CountLogicalOperators(node.Condition) + 1;
            this.mCurrentScope.CyclomaticComplexity += complexity;
            base.VisitDoStatement(node);
        }

        /// <inheritdoc />
        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            ++this.mCurrentScope.CyclomaticComplexity;
            base.VisitForEachStatement(node);
        }

        /// <inheritdoc />
        public override void VisitForStatement(ForStatementSyntax node)
        {
            var complexity = CountLogicalOperators(node.Condition) + 1;
            this.mCurrentScope.CyclomaticComplexity += complexity;
            base.VisitForStatement(node);
        }

        public override void VisitSwitchSection(SwitchSectionSyntax node)
        {
            if (node.Labels.All(l => l.Keyword.Text != "default"))
                ++this.mCurrentScope.CyclomaticComplexity;

            this.mCurrentScope.SourceLinesOfCode += node.Statements.Count;
            base.VisitSwitchSection(node);
        }

        /// <inheritdoc />
        public override void VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            ++this.mCurrentScope.CyclomaticComplexity;
            base.VisitCatchDeclaration(node);
        }

        /// <inheritdoc />
        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            ++this.mCurrentScope.CyclomaticComplexity;
            ++this.mCurrentScope.SourceLinesOfCode;
            base.VisitSimpleLambdaExpression(node);
        }

        /// <inheritdoc />
        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            ++this.mCurrentScope.CyclomaticComplexity;
            base.VisitAnonymousMethodExpression(node);
        }
    }
}