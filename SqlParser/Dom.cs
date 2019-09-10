
// using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;


namespace SqlParser
{


    class Dom
    {

        // https://stackoverflow.com/questions/9842991/regex-to-remove-single-line-sql-comments
        public static string RemoveCstyleComments(string strInput)
        {
            string strPattern = @"/[*][\w\d\s]+[*]/";
            //strPattern = @"/\*.*?\*/"; // Doesn't work 
            //strPattern = "/\\*.*?\\*/"; // Doesn't work 
            //strPattern = @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/ "; // Doesn't work 
            //strPattern = @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/ "; // Doesn't work 

            // http://stackoverflow.com/questions/462843/improving-fixing-a-regex-for-c-style-block-comments 
            strPattern = @"/\*(?>(?:(?>[^*]+)|\*(?!/))*)\*/";  // Works ! 

            string strOutput = System.Text.RegularExpressions.Regex.Replace(strInput, strPattern, string.Empty, System.Text.RegularExpressions.RegexOptions.Multiline);
            return strOutput;
        } // End Function RemoveCstyleComments 

        // string sql = "--this is a test\r\nselect stuff where substaff like '--this comment should stay' --this should be removed\r\n";
        public static string RemoveSingleLineComment(string sql)
        {
            char[] quotes = { '\'', '"' };

            int newCommentLiteral, lastCommentLiteral = 0;
            while ((newCommentLiteral = sql.IndexOf("--", lastCommentLiteral)) != -1)
            {
                int countQuotes = sql.Substring(lastCommentLiteral, newCommentLiteral - lastCommentLiteral).Split(quotes).Length - 1;
                if (countQuotes % 2 == 0) //this is a comment, since there's an even number of quotes preceding
                {
                    int eol = sql.IndexOf("\r\n") + 2;
                    if (eol == -1)
                        eol = sql.Length; //no more newline, meaning end of the string
                    sql = sql.Remove(newCommentLiteral, eol - newCommentLiteral);
                    lastCommentLiteral = newCommentLiteral;
                }
                else //this is within a string, find string ending and moving to it
                {
                    int singleQuote = sql.IndexOf("'", newCommentLiteral);
                    if (singleQuote == -1)
                        singleQuote = sql.Length;
                    int doubleQuote = sql.IndexOf('"', newCommentLiteral);
                    if (doubleQuote == -1)
                        doubleQuote = sql.Length;

                    lastCommentLiteral = System.Math.Min(singleQuote, doubleQuote) + 1;

                    //instead of finding the end of the string you could simply do += 2 but the program will become slightly slower
                }
            }

            System.Console.WriteLine(sql);
            return sql;
        }


        // https://michaeljswart.com/2014/04/removing-comments-from-sql/
        // http://web.archive.org/web/*/https://michaeljswart.com/2014/04/removing-comments-from-sql/
        public static string StripCommentsFromSQL(string SQL)
        {
            Microsoft.SqlServer.TransactSql.ScriptDom.TSql150Parser parser = 
                new Microsoft.SqlServer.TransactSql.ScriptDom.TSql150Parser(true);

            System.Collections.Generic.IList<Microsoft.SqlServer.TransactSql.ScriptDom.ParseError> errors;


            Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragment fragments = 
                parser.Parse(new System.IO.StringReader(SQL), out errors);

            // clear comments
            string result = string.Join(
              string.Empty,
              fragments.ScriptTokenStream
                  .Where(x => x.TokenType != Microsoft.SqlServer.TransactSql.ScriptDom.TSqlTokenType.MultilineComment)
                  .Where(x => x.TokenType != Microsoft.SqlServer.TransactSql.ScriptDom.TSqlTokenType.SingleLineComment)
                  .Select(x => x.Text));

            return result;

        }

        private static System.Text.RegularExpressions.Regex everythingExceptNewLines = 
            new System.Text.RegularExpressions.Regex("[^\r\n]");


        // http://drizin.io/Removing-comments-from-SQL-scripts/
        // http://web.archive.org/web/*/http://drizin.io/Removing-comments-from-SQL-scripts/
        public static string RemoveComments(string input, bool preservePositions, bool removeLiterals = false)
        {
            //based on http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
            var lineComments = @"--(.*?)\r?\n";
            var lineCommentsOnLastLine = @"--(.*?)$"; // because it's possible that there's no \r\n after the last line comment
                                                      // literals ('literals'), bracketedIdentifiers ([object]) and quotedIdentifiers ("object"), they follow the same structure:
                                                      // there's the start character, any consecutive pairs of closing characters are considered part of the literal/identifier, and then comes the closing character
            var literals = @"('(('')|[^'])*')"; // 'John', 'O''malley''s', etc
            var bracketedIdentifiers = @"\[((\]\])|[^\]])* \]"; // [object], [ % object]] ], etc
            var quotedIdentifiers = @"(\""((\""\"")|[^""])*\"")"; // "object", "object[]", etc - when QUOTED_IDENTIFIER is set to ON, they are identifiers, else they are literals
                                                                  //var blockComments = @"/\*(.*?)\*/";  //the original code was for C#, but Microsoft SQL allows a nested block comments // //https://msdn.microsoft.com/en-us/library/ms178623.aspx

            //so we should use balancing groups // http://weblogs.asp.net/whaggard/377025
            var nestedBlockComments = @"/\*
                                 (?>
                                 /\*  (?<LEVEL>)      # On opening push level
                                 | 
                                 \*/ (?<-LEVEL>)     # On closing pop level
                                 |
                                 (?! /\* | \*/ ) . # Match any char unless the opening and closing strings   
                                 )+                         # /* or */ in the lookahead string
                                 (?(LEVEL)(?!))             # If level exists then fail
                                 \*/";

            string noComments = System.Text.RegularExpressions.Regex.Replace(input,
                nestedBlockComments + "|" + lineComments + "|" + lineCommentsOnLastLine + "|" + literals + "|" + bracketedIdentifiers + "|" + quotedIdentifiers,
                me => {
                    if (me.Value.StartsWith("/*") && preservePositions)
                        return everythingExceptNewLines.Replace(me.Value, " "); // preserve positions and keep line-breaks // return new string(' ', me.Value.Length);
             else if (me.Value.StartsWith("/*") && !preservePositions)
                        return "";
                    else if (me.Value.StartsWith("--") && preservePositions)
                        return everythingExceptNewLines.Replace(me.Value, " "); // preserve positions and keep line-breaks
             else if (me.Value.StartsWith("--") && !preservePositions)
                        return everythingExceptNewLines.Replace(me.Value, ""); // preserve only line-breaks // Environment.NewLine;
             else if (me.Value.StartsWith("[") || me.Value.StartsWith("\""))
                        return me.Value; // do not remove object identifiers ever
             else if (!removeLiterals) // Keep the literal strings
                 return me.Value;
                    else if (removeLiterals && preservePositions) // remove literals, but preserving positions and line-breaks
             {
                        var literalWithLineBreaks = everythingExceptNewLines.Replace(me.Value, " ");
                        return "'" + literalWithLineBreaks.Substring(1, literalWithLineBreaks.Length - 2) + "'";
                    }
                    else if (removeLiterals && !preservePositions) // wrap completely all literals
                 return "''";
                    else
                        throw new System.NotImplementedException();
                },
                System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);
            return noComments;
        }


    }
}
