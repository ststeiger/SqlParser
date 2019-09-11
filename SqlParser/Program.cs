
using Antlr4.Runtime.Misc;
using SqlParser.grammars;


namespace SqlParser
{
    
    
    internal class Program
    {
        // https://www.nuget.org/packages/Microsoft.SqlServer.TransactSql.ScriptDom/15.0.4200.1
        
        
        // https://fullboarllc.com/getting-started-with-visual-studio-2015-antlr4-c-and-a-sql-grammar/
        // https://fullboarllc.com/using-antlr-4-with-net-core-2-1-and-c-getting-started/
        // https://fullboarllc.com/antlr4-dotnet-core-visitor/
        
        // https://programming-pages.com/2013/12/14/antlr-4-with-c-and-visual-studio-2012/
        // https://weekly-geekly.github.io/articles/346038/index.html
        // https://tomassetti.me/antlr-mega-tutorial/
        // https://tomassetti.me/getting-started-with-antlr-in-csharp/
        
        
        // https://github.com/elshev/DataGenerator
        // https://github.com/ajaxx/tsql2pgsql
        // https://github.com/azraelrabbit/SqlSchemer
        static void Main(string[] args)
        {
            // https://github.com/antlr/grammars-v4/tree/master/tsql
            // https://github.com/antlr/grammars-v4/tree/master/plsql/CSharp
            
            // string text = System.IO.File.ReadAllText(@"D:\username\Desktop\sysflang.sql");
            string text = @"
SELECT 123 AS /*some crap*/aaa, 'test' as test 
    , 'Hello foo /*bar*/ my --world ' AS xyz -- Hello
--ciao
-- bye bye
";

            text = @"SELECT 123 as/* */abc
";
            text = "SELECT 123[abc]";
            // text = "SELECT 123/* test */[abc]";
            text = @"SELECT 123[abc]
/*test*/";


            // Dom.Test();

            LexerTest(text);
            // WalkerTest(fileName);
            // VisitorTest(fileName);
            
            System.Console.WriteLine(" --- Press any key to continue --- ");
            System.Console.ReadKey();
        } // End Sub Main 


        // https://github.com/dotjpg3141/Strings
        static void LexerTest(string text)
        {
            try
            {
                System.IO.StringReader reader = new System.IO.StringReader(text);

                // Antlr4.Runtime.AntlrInputStream input = new Antlr4.Runtime.AntlrInputStream(reader);

                Antlr4.Runtime.ICharStream input1 = new Antlr4.Runtime.AntlrInputStream(reader);
                Antlr4.Runtime.CaseChangingCharStream input = new Antlr4.Runtime.CaseChangingCharStream(input1, true);


                TSqlLexer lexer = new TSqlLexer(input);

                Antlr4.Runtime.CommonTokenStream tokenStream = new Antlr4.Runtime.CommonTokenStream(lexer);
                
                tokenStream.Fill();
                
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                int lastIndex = 0;
                
                foreach (Antlr4.Runtime.IToken token in tokenStream.GetTokens())
                {
                    // System.Console.WriteLine(token.Text);
                    string tokenTypeName = lexer.Vocabulary.GetSymbolicName(token.Type);
                    
                    if (token.Type == TSqlLexer.LINE_COMMENT || token.Type == TSqlLexer.COMMENT ||
                        token.Type == TSqlLexer.Eof)
                    {
                        Interval blankInterval = new Interval(lastIndex, token.StartIndex - 1);
                        string extractedBlank = token.InputStream.GetText(blankInterval);
                        if(string.IsNullOrEmpty(extractedBlank))
                            sb.Append(" ");
                        else
                            sb.Append(extractedBlank);

                        lastIndex = token.StopIndex + 1;
                        continue;
                    } // End if comment 
                    
                    // sql += token.Text + " ";
                    Interval ival = new Interval(lastIndex, token.StopIndex);
                    string extracted = token.InputStream.GetText(ival);
                    // System.Console.WriteLine((extracted));
                    sb.Append(extracted);
                    
                    
                    // System.Console.WriteLine(token.Text);
                    // System.Console.WriteLine(token.Type);
                    // System.Console.WriteLine(tokenTypeName);

                    lastIndex = token.StopIndex + 1;
                } // Next token 
                
                string sql = sb.ToString();
                sb.Clear();
                sb = null;
                System.Console.WriteLine(sql);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        } // End Sub LexerTest 


        static void WalkerTest(string text)
        {
            try
            {
                System.IO.StringReader reader = new System.IO.StringReader(text);
                Antlr4.Runtime.AntlrInputStream input = new Antlr4.Runtime.AntlrInputStream(reader);
                TSqlLexer lexer = new TSqlLexer(input);
                Antlr4.Runtime.CommonTokenStream tokens = new Antlr4.Runtime.CommonTokenStream(lexer);
                TSqlParser parser = new TSqlParser(tokens);
                // Specify our entry point

                // TSqlParser.Query_specificationContext 
                TSqlParser.Tsql_fileContext fileContext = parser.tsql_file();
                // Antlr4.Runtime.Tree.IParseTree root = (Antlr4.Runtime.Tree.IParseTree)fileContext;
                
                
                // TSqlParser.Query_specificationContext tsqlParser.Tsql_fileContext fileContext = parser.tsql_file();
                System.Console.WriteLine("fileContext.ChildCount = " + fileContext.ChildCount.ToString());
                // Walk it and attach our listener 
                Antlr4.Runtime.Tree.ParseTreeWalker walker = new Antlr4.Runtime.Tree.ParseTreeWalker();
                // AntlrTsqListener listener = new AntlrTsqListener();
                EverythingListener listener = new EverythingListener();

                walker.Walk(listener, fileContext);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        } // End Sub WalkerTest 
        
        
        static void VisitorTest(string text)
        {
            try
            {
                System.IO.StringReader reader = new System.IO.StringReader(text);
                Antlr4.Runtime.AntlrInputStream input = new Antlr4.Runtime.AntlrInputStream(reader);
                TSqlLexer lexer = new TSqlLexer(input);
                Antlr4.Runtime.CommonTokenStream tokens = new Antlr4.Runtime.CommonTokenStream(lexer);
                TSqlParser parser = new TSqlParser(tokens);
                //Specify our entry point
                
                // TSqlParser.Query_specificationContext 
                TSqlParser.Tsql_fileContext fileContext = parser.tsql_file();
                
                System.Console.WriteLine("fileContext.ChildCount = " + fileContext.ChildCount.ToString());
                
                SqlVisitor vis = new SqlVisitor();
                string s = vis.Visit(fileContext);
                System.Console.WriteLine(s);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            
        } // End Sub VisitorTest
        
        
    } // End Class Program 
    
    
} // End Namespace SqlParser 
