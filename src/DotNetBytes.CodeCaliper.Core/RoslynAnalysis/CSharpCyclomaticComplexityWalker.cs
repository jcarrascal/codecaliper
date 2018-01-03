namespace DotNetBytes.CodeCaliper.Core.RoslynAnalysis
{
    using System.Collections.Generic;
    using System.Dynamic;
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
                                  node.TypeParameterList + node.ParameterList + node.ReturnType;
            function.CyclomaticComplexity = 1;

            this.mObjects.Push(this.mCurrentObject);
            this.mCurrentObject = function;
            base.VisitMethodDeclaration(node);
            this.mCurrentObject = this.mObjects.Pop();
            this.mCurrentObject.CyclomaticComplexity += function.CyclomaticComplexity;
            this.mContext[function.Identifier] = function;
        }
    }
}