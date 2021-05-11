//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from LGTemplateParser.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

#pragma warning disable 3021
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="LGTemplateParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
public interface ILGTemplateParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.context"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitContext([NotNull] LGTemplateParser.ContextContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>normalBody</c>
	/// labeled alternative in <see cref="LGTemplateParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNormalBody([NotNull] LGTemplateParser.NormalBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ifElseBody</c>
	/// labeled alternative in <see cref="LGTemplateParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfElseBody([NotNull] LGTemplateParser.IfElseBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>switchCaseBody</c>
	/// labeled alternative in <see cref="LGTemplateParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSwitchCaseBody([NotNull] LGTemplateParser.SwitchCaseBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>structuredBody</c>
	/// labeled alternative in <see cref="LGTemplateParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructuredBody([NotNull] LGTemplateParser.StructuredBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.structuredTemplateBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructuredTemplateBody([NotNull] LGTemplateParser.StructuredTemplateBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.structuredBodyNameLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructuredBodyNameLine([NotNull] LGTemplateParser.StructuredBodyNameLineContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.errorStructuredName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitErrorStructuredName([NotNull] LGTemplateParser.ErrorStructuredNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.structuredBodyContentLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructuredBodyContentLine([NotNull] LGTemplateParser.StructuredBodyContentLineContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.errorStructureLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitErrorStructureLine([NotNull] LGTemplateParser.ErrorStructureLineContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.keyValueStructureLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitKeyValueStructureLine([NotNull] LGTemplateParser.KeyValueStructureLineContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.keyValueStructureValue"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitKeyValueStructureValue([NotNull] LGTemplateParser.KeyValueStructureValueContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.structuredBodyEndLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructuredBodyEndLine([NotNull] LGTemplateParser.StructuredBodyEndLineContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.normalTemplateBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNormalTemplateBody([NotNull] LGTemplateParser.NormalTemplateBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.templateString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTemplateString([NotNull] LGTemplateParser.TemplateStringContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.normalTemplateString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNormalTemplateString([NotNull] LGTemplateParser.NormalTemplateStringContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.errorTemplateString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitErrorTemplateString([NotNull] LGTemplateParser.ErrorTemplateStringContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.ifElseTemplateBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfElseTemplateBody([NotNull] LGTemplateParser.IfElseTemplateBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.ifConditionRule"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfConditionRule([NotNull] LGTemplateParser.IfConditionRuleContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.ifCondition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfCondition([NotNull] LGTemplateParser.IfConditionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.switchCaseTemplateBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSwitchCaseTemplateBody([NotNull] LGTemplateParser.SwitchCaseTemplateBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.switchCaseRule"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSwitchCaseRule([NotNull] LGTemplateParser.SwitchCaseRuleContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.switchCaseStat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSwitchCaseStat([NotNull] LGTemplateParser.SwitchCaseStatContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression([NotNull] LGTemplateParser.ExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LGTemplateParser.expressionInStructure"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionInStructure([NotNull] LGTemplateParser.ExpressionInStructureContext context);
}
