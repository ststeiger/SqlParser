
using SqlParser.grammars;


namespace SqlParser
{


    public class SqlVisitor
        : TSqlParserBaseVisitor<string>
    {


        /*  
        // https://github.com/c0oil/Antlr
        public override string VisitSql_clauses(TSqlParser.Sql_clausesContext ctx)  
        {  
            System.Console.WriteLine("VisitSql_clauses");  
            return VisitChildren(ctx).ToString();  
        }  

        public override string VisitSql_clause(TSqlParser.Sql_clauseContext ctx)
        {
            System.Console.WriteLine("VisitSql_clause");
            try
            {
                return VisitDml_clause(ctx.dml_clause()).ToString();
            }
            catch (System.Exception e)
            {
                return "";
            }
        }

        public override string VisitDml_clause(TSqlParser.Dml_clauseContext ctx)
        {
            System.Console.WriteLine("VisitDml_clause");
            return VisitChildren(ctx).ToString();
        }
        */

        public override string Visit([Antlr4.Runtime.Misc.NotNull] Antlr4.Runtime.Tree.IParseTree tree)
        {
            return base.Visit(tree);

            // return tree.GetText();

            /*
            int n = tree.ChildCount;
            string s = "";
            for (int i = 0; i < n; i++)
            {
                Antlr4.Runtime.Tree.IParseTree c = tree.GetChild(i);

                TSqlParser.BatchContext bc = c as TSqlParser.BatchContext;
                if (bc != null)
                {
                    s += bc.start.Text + " " + bc.stop.Text;
                }
                
            }

            return s;
            */
        }


    }


}