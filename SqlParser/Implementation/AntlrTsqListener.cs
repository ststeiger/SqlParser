
using SqlParser.grammars;


namespace SqlParser
{

    // https://github.com/flipworldit/SqlFormatter/tree/master/SqlFormatter
    // https://github.com/sergbas/SqlParserAntlr
    // https://github.com/Kostka12/BP_DBMasking/blob/fd8de0318c2397fad2e3aed3f23eb5ea122bcb6b/TestingConsoleApp/Program.cs
    // https://github.com/Kostka12/BP_DBMasking/blob/master/TestingConsoleApp/Program.cs


    public class EverythingListener 
    : TSqlParserBaseListener
    {

        public override void EnterEveryRule([Antlr4.Runtime.Misc.NotNull] Antlr4.Runtime.ParserRuleContext context)
        {
            string  s = context.GetText();
            System.Console.WriteLine(s);

        }
    }

    public class AntlrTsqListener
        : TSqlParserBaseListener
    {
        private enum JoinMode
        {
            Undefined,
            Where,
            Join
        };
        private JoinMode mode;
        private enum BranchType
        {
            Select,
            Table_sources,
            Search_condition,
            Join
        };
        private BranchType branch;
        private string alias = "";
        public override void EnterQuery_specification(TSqlParser.Query_specificationContext ctx)
        {
            mode = JoinMode.Undefined;
        }
        public override void EnterTable_sources(TSqlParser.Table_sourcesContext ctx)
        {
            if (ctx.ChildCount > 1) mode = JoinMode.Where;
            branch = BranchType.Table_sources;
        }
        public override void EnterTable_source_item_joined([Antlr4.Runtime.Misc.NotNull] TSqlParser.Table_source_item_joinedContext ctx)
        {
            if ((mode == JoinMode.Undefined & ctx.ChildCount == 1) || (mode == JoinMode.Where)) return;
            mode = JoinMode.Join;
            branch = BranchType.Table_sources;
        }
        public override void EnterTable_name_with_hint([Antlr4.Runtime.Misc.NotNull] TSqlParser.Table_name_with_hintContext ctx)
        {
            if (mode == JoinMode.Undefined) return;
            if (branch == BranchType.Table_sources)
                System.Console.WriteLine(branch.ToString());
            alias = "";
        }
        public override void EnterTable_name([Antlr4.Runtime.Misc.NotNull] TSqlParser.Table_nameContext ctx)
        {
            if (branch == BranchType.Search_condition || branch == BranchType.Select || mode == JoinMode.Undefined) return;
            System.Console.WriteLine(ctx.GetText());
        }
        public override void EnterTable_alias([Antlr4.Runtime.Misc.NotNull] TSqlParser.Table_aliasContext ctx)
        {
            if (branch == BranchType.Search_condition || branch == BranchType.Select | mode == JoinMode.Undefined) return;
            alias = ctx.GetChild(0).GetText();
            System.Console.WriteLine("alias=" + alias);
        }
        public override void EnterSearch_condition([Antlr4.Runtime.Misc.NotNull] TSqlParser.Search_conditionContext ctx)
        {
            if (mode == JoinMode.Undefined) return;
            branch = BranchType.Search_condition;
            System.Console.WriteLine("Search_condition");
            System.Console.WriteLine(ctx.GetText());
            return;
        }
        public override void EnterSelect_statement([Antlr4.Runtime.Misc.NotNull] TSqlParser.Select_statementContext ctx)
        {
            System.Console.WriteLine("Select_statement");
            branch = BranchType.Select;
            return;
        }
    }


}
