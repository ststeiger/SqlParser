
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
            var ls = GetVariableNames();
            System.Console.WriteLine(ls);
            // SplitMultiTableInsertScript();
            // SubstituteVariablesTest();
            // CommentRemoverLexerTest();

            System.Console.WriteLine(" --- Press any key to continue --- ");
            System.Console.ReadKey();
        }



        public static System.Text.Encoding GetSystemEncoding()
        {
            // The OEM code page for use by legacy console applications
            // int oem = System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage;

            // The ANSI code page for use by legacy GUI applications
            // int ansi = System.Globalization.CultureInfo.InstalledUICulture.TextInfo.ANSICodePage; // Machine 
            int ansi = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage; // User 

            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                System.Text.Encoding enc = System.Text.Encoding.GetEncoding(ansi);
                return enc;
            }
            catch (System.Exception)
            { }


            try
            {

                foreach (System.Text.EncodingInfo ei in System.Text.Encoding.GetEncodings())
                {
                    System.Text.Encoding e = ei.GetEncoding();

                    // 20'127: US-ASCII 
                    if (e.WindowsCodePage == ansi && e.CodePage != 20127)
                    {
                        return e;
                    }

                }
            }
            catch (System.Exception)
            { }

            // return System.Text.Encoding.GetEncoding("iso-8859-1");
            return System.Text.Encoding.UTF8;
        }


        public static System.Collections.Generic.List<string> GetVariableNames()
        {
            System.Collections.Generic.List<string> ls = new System.Collections.Generic.List<string>();

            string text = @"
SELECT BE_Name FROM T_Benutzer WHERE Name =@username 
OR Name LIKE '%' + @foo + '%'
";
            System.IO.StringReader reader = new System.IO.StringReader(text);

            // Antlr4.Runtime.AntlrInputStream input = new Antlr4.Runtime.AntlrInputStream(reader);

            Antlr4.Runtime.ICharStream input1 = new Antlr4.Runtime.AntlrInputStream(reader);
            Antlr4.Runtime.CaseChangingCharStream input = new Antlr4.Runtime.CaseChangingCharStream(input1, true);


            TSqlLexer lexer = new TSqlLexer(input);

            Antlr4.Runtime.CommonTokenStream tokenStream = new Antlr4.Runtime.CommonTokenStream(lexer);
            tokenStream.Fill();

            int lastIndex = 0;

            foreach (Antlr4.Runtime.IToken token in tokenStream.GetTokens())
            {
                // System.Console.WriteLine(token.Text);
                string tokenTypeName = lexer.Vocabulary.GetSymbolicName(token.Type);
                Antlr4.Runtime.Misc.Interval ival = new Antlr4.Runtime.Misc.Interval(lastIndex, token.StopIndex);
                string extracted = token.InputStream.GetText(ival);
                
                // table_name, cte_name: ID, SQUARE_BRACKET_ID
                // Local variables: LOCAL_ID
                if (token.Type == TSqlLexer.LOCAL_ID)
                {
                    extracted = extracted.Trim(new char[] { ' ', '\t', '\v', '\r', '\n' });
                    ls.Add(extracted);
                } // End if (token.Type == TSqlLexer.LOCAL_ID) 

                lastIndex = token.StopIndex + 1;
            } // Next token 

            return ls;
        }


        static void SplitMultiTableInsertScript()
        {
            string fileName = @"D:\SQL\TESS\Anlage_Refdaten.txt";
            fileName = @"D:\SQL\TESS\Adressdaten.txt";
            fileName = @"D:\SQL\TESS\Anlagedaten.txt";
            fileName = @"D:\SQL\TESS\Anlagerechte.txt";
            fileName = @"D:\SQL\TESS\Kontaktdaten.txt";
            fileName = @"D:\SQL\TESS\Navigation.txt";
            fileName = @"D:\username\Desktop\Raumdaten\Raumdaten.sql";
            fileName = @"D:\username\Desktop\Raumdaten\Vertragsdaten.sql";




            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // https://github.com/antlr/grammars-v4/tree/master/tsql
            // https://github.com/antlr/grammars-v4/tree/master/plsql/CSharp


            System.Text.Encoding enc = GetSystemEncoding();


            string text = System.IO.File.ReadAllText(fileName, enc);
            System.IO.StringReader reader = new System.IO.StringReader(text);

            // Antlr4.Runtime.AntlrInputStream input = new Antlr4.Runtime.AntlrInputStream(reader);

            Antlr4.Runtime.ICharStream input1 = new Antlr4.Runtime.AntlrInputStream(reader);
            Antlr4.Runtime.CaseChangingCharStream input = new Antlr4.Runtime.CaseChangingCharStream(input1, true);


            TSqlLexer lexer = new TSqlLexer(input);

            Antlr4.Runtime.CommonTokenStream tokenStream = new Antlr4.Runtime.CommonTokenStream(lexer);

            tokenStream.Fill();


            int lastIndex = 0;


            System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> dict =
                new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>(System.StringComparer.InvariantCultureIgnoreCase);


            System.Collections.Generic.List<string> lsTableName = new System.Collections.Generic.List<string>();


            bool ignoreThis = true;
            bool partOfTableName = false;

            int lastTokenType = -1;
            int secondLastTokenType = -1;


            foreach (Antlr4.Runtime.IToken token in tokenStream.GetTokens())
            {
                // System.Console.WriteLine(token.Text);
                string tokenTypeName = lexer.Vocabulary.GetSymbolicName(token.Type);
                Antlr4.Runtime.Misc.Interval ival = new Antlr4.Runtime.Misc.Interval(lastIndex, token.StopIndex);
                string extracted = token.InputStream.GetText(ival);
                extracted = extracted.Trim(new char[] { '\t', '\v', '\r', '\n' });

                if (token.Type == TSqlLexer.INSERT)
                {
                    if (sb.Length > 0)
                    {
                        string tn = string.Join(".", lsTableName.ToArray()).Replace("[", "").Replace("]", "").Trim();
                        lsTableName.Clear();
                        System.Console.WriteLine(tn);

                        if (!dict.ContainsKey(tn))
                            dict[tn] = new System.Collections.Generic.List<string>();


                        sb.Append(";");
                        dict[tn].Add(sb.ToString());
                    }


                    sb.Clear();
                    ignoreThis = false;
                    partOfTableName = true;
                }
                else if (token.Type == TSqlLexer.GO)
                {
                    ignoreThis = true;
                    partOfTableName = false;
                }
                else if (token.Type == TSqlLexer.USE)
                {
                    ignoreThis = true;
                    partOfTableName = false;
                }
                else if (token.Type == TSqlLexer.SEMI)
                {
                    ignoreThis = true;
                    partOfTableName = false;
                }
                else if (token.Type == TSqlLexer.Eof)
                { }
                else if (token.Type == TSqlLexer.LR_BRACKET)
                {
                    partOfTableName = false;
                }
                else if (token.Type == TSqlLexer.RR_BRACKET)
                { }
                else if (token.Type == TSqlLexer.COMMA)
                { }
                else if (token.Type == TSqlLexer.INTO)
                { }
                else if (token.Type == TSqlLexer.VALUES || token.Type == TSqlLexer.SELECT)
                {

                }
                else if (token.Type == TSqlLexer.ID || token.Type == TSqlLexer.SQUARE_BRACKET_ID)
                {
                    if (partOfTableName)
                        lsTableName.Add(extracted);
                }
                else if (token.Type == TSqlLexer.DOT)
                { }
                else if (token.Type == TSqlLexer.STRING)
                { }
                else if (token.Type == TSqlLexer.DECIMAL)
                { }
                else if (token.Type == TSqlLexer.FLOAT)
                { }
                else if (token.Type == TSqlLexer.NULL)
                { }
                else if (token.Type == TSqlLexer.CAST)
                { }
                else if (token.Type == TSqlLexer.AS)
                {
                    // CAST(xxx AS datetime) 
                }
                else if (token.Type == TSqlLexer.MINUS)
                {
                    // Negative Number
                }
                else
                {
                    System.Console.WriteLine(tokenTypeName);
                }




                // System.Console.WriteLine((extracted));
                if (!ignoreThis && token.Type != TSqlLexer.SEMI)
                {
                    sb.Append(extracted);
                }





                // System.Console.WriteLine(token.Text);
                // System.Console.WriteLine(token.Type);
                // System.Console.WriteLine(tokenTypeName);

                lastIndex = token.StopIndex + 1;


                secondLastTokenType = lastTokenType;
                lastTokenType = token.Type;
            } // Next token 

            if (sb.Length > 0)
            {
                string tn = string.Join(".", lsTableName.ToArray()).Replace("[", "").Replace("]", "").Trim();
                lsTableName.Clear();
                System.Console.WriteLine(tn);

                if (!dict.ContainsKey(tn))
                    dict[tn] = new System.Collections.Generic.List<string>();

                sb.Append(";");
                dict[tn].Add(sb.ToString());
            } // End if (sb.Length > 0) 


            sb.Clear();
            sb = null;


            string baseDir = System.IO.Path.GetFileNameWithoutExtension(fileName);

            string outputDirectory = System.IO.Path.GetDirectoryName(fileName);

            baseDir = System.IO.Path.Combine(outputDirectory, baseDir);
            if (!System.IO.Directory.Exists(baseDir))
                System.IO.Directory.CreateDirectory(baseDir);

            foreach (System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.List<string>> kvp in dict)
            {
                string dir = kvp.Key;
                string content = string.Join("\r\n\r\n", kvp.Value.ToArray());
                System.Console.WriteLine(content);

                string fn = System.IO.Path.Combine(baseDir, kvp.Key + ".sql");
                System.IO.File.WriteAllText(fn, content, System.Text.Encoding.UTF8);
            } // Next kvp 


            System.Console.WriteLine(dict);

        } // End Sub SplitMultiTableInsertScript 


        static string SubstituteVariablesTest()
        {
            // https://github.com/antlr/grammars-v4/tree/master/tsql
            // https://github.com/antlr/grammars-v4/tree/master/plsql/CSharp

            string text = @"
DECLARE @legalEntity int 
-- SET @legalEntity = 1


;WITH CTE AS 
(
	      SELECT 1 AS id, 123 AS abc 
	UNION SELECT 2 AS id, 456 AS abc 
	UNION SELECT 3 AS id, 789 AS abc 
	UNION SELECT 4 AS id, 012 AS abc 
	UNION SELECT 5 AS id, 345 AS abc 
	UNION SELECT 6 AS id, 678 AS abc 
)

SELECT 
	 *
	,'@legalEntity' AS abcdef -- strings do not get substituted 
	,987 AS [@legalEntity] -- identifiers do not get substituted 
FROM CTE 
WHERE (1=1) 
AND 
(
	'0' IN (@legalEntity,
@legalEntity )
	OR 
	CTE.id IN (@legalEntity) 
	-- CTE.id IN (@legalEntity /* @legalEntity */) 
	
) 
/*
==>
AND 
(
	'0' IN (1,2,6)
	OR 
	CTE.id IN (1,2,6) 
	-- OR CTE.id IN (1,2,3,4,5,6 /* 1,2,3,4,5,6 */) 
)
*/

";

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
#if NO_COMMENTS
                if (token.Type == TSqlLexer.LINE_COMMENT || token.Type == TSqlLexer.COMMENT ||
                    token.Type == TSqlLexer.Eof)
                {
                    Antlr4.Runtime.Misc.Interval blankInterval = new Antlr4.Runtime.Misc.Interval(lastIndex, token.StartIndex - 1);
                    string extractedBlank = token.InputStream.GetText(blankInterval);
                    if (string.IsNullOrEmpty(extractedBlank))
                        sb.Append(" ");
                    else
                        sb.Append(extractedBlank);

                    lastIndex = token.StopIndex + 1;
                    continue;
                } // End if comment 
#endif 


                // sql += token.Text + " ";
                Antlr4.Runtime.Misc.Interval ival = new Antlr4.Runtime.Misc.Interval(lastIndex, token.StopIndex);
                string extracted = token.InputStream.GetText(ival);

                // table_name, cte_name: ID, SQUARE_BRACKET_ID
                // Local variables: LOCAL_ID
                if (token.Type == TSqlLexer.LOCAL_ID)
                {
                    extracted = extracted.Trim(new char[] { ' ', '\t', '\v', '\r', '\n' });

                    System.Console.WriteLine(extracted);
                } // End if (token.Type == TSqlLexer.LOCAL_ID) 

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

            return sql;
        } // End Sub SubstituteVariablesTest 



        public static void CommentRemoverLexerTest()
        {

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

        } // End Sub CommentRemoverLexerTest 




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
                        Antlr4.Runtime.Misc.Interval blankInterval = new Antlr4.Runtime.Misc.Interval(lastIndex, token.StartIndex - 1);
                        string extractedBlank = token.InputStream.GetText(blankInterval);
                        if (string.IsNullOrEmpty(extractedBlank))
                            sb.Append(" ");
                        else
                            sb.Append(extractedBlank);

                        lastIndex = token.StopIndex + 1;
                        continue;
                    } // End if comment 

                    // sql += token.Text + " ";
                    Antlr4.Runtime.Misc.Interval ival = new Antlr4.Runtime.Misc.Interval(lastIndex, token.StopIndex);
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
