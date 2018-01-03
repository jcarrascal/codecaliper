namespace DotNetBytes.CodeCaliper.Core.RoslynAnalysis
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class CSharpCyclomaticComplexityWalker : CSharpSyntaxWalker
    {
        private readonly ProcessContext mContext;
        private readonly Stack<dynamic> mObjects = new Stack<dynamic>();
        private dynamic mCurrentObject;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CSharpCyclomaticComplexityWalker" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CSharpCyclomaticComplexityWalker(ProcessContext context)
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
            this.mCurrentObject = file;
            this.mCurrentObject.CyclomaticComplexity = 0;
            this.Visit(syntaxTree.GetRoot());
        }

        /// <inheritdoc />
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            dynamic type = new ExpandoObject();
            type.Identifier = this.mCurrentObject.Identifier + ":" + node.Identifier +
                              node.TypeParameterList;
            type.CyclomaticComplexity = 0;

            this.mObjects.Push(this.mCurrentObject);
            this.mCurrentObject = type;
            base.VisitClassDeclaration(node);
            this.mCurrentObject = this.mObjects.Pop();
            this.mCurrentObject.CyclomaticComplexity += type.CyclomaticComplexity;
            this.mContext[type.Identifier] = type;
        }

        /// <inheritdoc />
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            dynamic function = new ExpandoObject();
            function.Identifier = this.mCurrentObject.Identifier + ":" + node.Identifier +
                                  node.TypeParameterList + "(" + StringifyParameterList(node.ParameterList) + ")" +
                                  node.ReturnType;
            function.CyclomaticComplexity = 1;

            this.mObjects.Push(this.mCurrentObject);
            this.mCurrentObject = function;
            base.VisitMethodDeclaration(node);
            this.mCurrentObject = this.mObjects.Pop();
            this.mCurrentObject.CyclomaticComplexity += function.CyclomaticComplexity;
            this.mContext[function.Identifier] = function;
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
            function.Identifier = this.mCurrentObject.Identifier + ":" + parentIdentifier + ":" + node.Keyword;
            function.CyclomaticComplexity = 1;

            this.mObjects.Push(this.mCurrentObject);
            this.mCurrentObject = function;
            base.VisitAccessorDeclaration(node);
            this.mCurrentObject = this.mObjects.Pop();
            this.mCurrentObject.CyclomaticComplexity += function.CyclomaticComplexity;
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
            function.Identifier = this.mCurrentObject.Identifier + ":" + node.Identifier +
                                  node.ParameterList;
            function.CyclomaticComplexity = 1;

            this.mObjects.Push(this.mCurrentObject);
            this.mCurrentObject = function;
            base.VisitConstructorDeclaration(node);
            this.mCurrentObject = this.mObjects.Pop();
            this.mCurrentObject.CyclomaticComplexity += function.CyclomaticComplexity;
            this.mContext[function.Identifier] = function;
        }

        /// <inheritdoc />
        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            dynamic function = new ExpandoObject();
            function.Identifier = this.mCurrentObject.Identifier + ":~" + node.Identifier +
                                  node.ParameterList;
            function.CyclomaticComplexity = 1;

            this.mObjects.Push(this.mCurrentObject);
            this.mCurrentObject = function;
            base.VisitDestructorDeclaration(node);
            this.mCurrentObject = this.mObjects.Pop();
            this.mCurrentObject.CyclomaticComplexity += function.CyclomaticComplexity;
            this.mContext[function.Identifier] = function;
        }

        /// <inheritdoc />
        public override void VisitIfStatement(IfStatementSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitIfStatement(node);
        }

        /// <inheritdoc />
        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitWhileStatement(node);
        }

        /// <inheritdoc />
        public override void VisitDoStatement(DoStatementSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitDoStatement(node);
        }

        /// <inheritdoc />
        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitForEachStatement(node);
        }

        /// <inheritdoc />
        public override void VisitForStatement(ForStatementSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitForStatement(node);
        }

        /// <inheritdoc />
        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitCaseSwitchLabel(node);
        }

        /// <inheritdoc />
        public override void VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitCatchDeclaration(node);
        }

        /// <inheritdoc />
        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitSimpleLambdaExpression(node);
        }

        /// <inheritdoc />
        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            ++this.mCurrentObject.CyclomaticComplexity;
            base.VisitAnonymousMethodExpression(node);
        }
    }
}