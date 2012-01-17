using Irony.Parsing;

namespace Totem.Compiler
{
    [Language("Totem", "0.1", "A totem parser")]
    public partial class TotemGrammar : Irony.Parsing.Grammar
    {
        public TotemGrammar()
        {
            #region Lexical structure
            var strings = CreateStringLiteral("string");
            var number = CreateNumberLiteral("number");
            var identifier = CreateIdentifier("identifier");


            var singleLineComment = new CommentTerminal("singleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            var delimitedComment = new CommentTerminal("delimitedComment", "/*", "*/");
            this.NonGrammarTerminals.Add(singleLineComment);
            this.NonGrammarTerminals.Add(delimitedComment);

            //Temporarily, treat preprocessor instructions like comments
            CommentTerminal ppInstruction = new CommentTerminal("ppInstruction", "#", "\n");
            this.NonGrammarTerminals.Add(ppInstruction);

            // Symbols
            var colon = ToTerm(":", "colon");
            var semi = ToTerm(";", "semi");
            var semi_opt = new NonTerminal("semi_opt", Empty | semi);
            var dot = ToTerm(".", "dot");
            var comma = ToTerm(",", "comma");
            var comma_opt = new NonTerminal("comma_opt", Empty | comma);
            var commas_opt = new NonTerminal("commas_opt");
            commas_opt.Rule = MakeStarRule(commas_opt, null, comma);
            var qmark = ToTerm("?", "qmark");
            var qmark_opt = new NonTerminal("qmark_opt", Empty | qmark);
            var lbr = ToTerm("{", "lbr");
            var rbr = ToTerm("}", "rbr");
            var lpar = ToTerm("(", "lpar");
            var rpar = ToTerm(")", "rpar");
            var vark = ToTerm("var", "var");
            #endregion

            #region NonTerminals
            // Expressions
            var expression = new NonTerminal("expression", "expression");
            var primary_expression = new NonTerminal("primary_expression");
            var literal = new NonTerminal("literal");
            var member_access = new NonTerminal("member_access");
            var initializer_value = new NonTerminal("initializer_value");
            var assignment_operator = new NonTerminal("assignment_operator");
            var bin_op_expression = new NonTerminal("bin_op_expression");
            var bin_op = new NonTerminal("bin_op", "operator symbol");

            // Statements
            var statement = new NonTerminal("statement", "statement");
            var statement_list = new NonTerminal("statement_list");
            var statement_list_opt = new NonTerminal("statement_list_opt");
            var declaration_statement = new NonTerminal("declaration_statement");
            var embedded_statement = new NonTerminal("embedded_statement");
            var local_variable_declaration = new NonTerminal("local_variable_declaration");
            var local_variable_declarator = new NonTerminal("local_variable_declarator");
            var local_variable_declarators = new NonTerminal("local_variable_declarators");
            var block = new NonTerminal("block");
            var statement_expression = new NonTerminal("statement_expression");

            // Program
            var program = new NonTerminal("program");

            #endregion

            #region operators, punctuation and delimiters
            RegisterOperators(1, "||");
            RegisterOperators(2, "&&");
            RegisterOperators(3, "|");
            RegisterOperators(4, "^");
            RegisterOperators(5, "&");
            RegisterOperators(6, "==", "!=");
            RegisterOperators(7, "<", ">", "<=", ">=", "is", "as");
            RegisterOperators(8, "<<", ">>");
            RegisterOperators(9, "+", "-");
            RegisterOperators(10, "*", "/", "%");
            RegisterOperators(-2, "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            RegisterOperators(-1, "?");

            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=";
            this.MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":");
            this.MarkTransient(statement, embedded_statement, expression, literal, bin_op);

            this.AddTermsReportGroup("assignment", "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            this.AddTermsReportGroup("constant", number, strings);
            this.AddTermsReportGroup("constant", "true", "false", "null", "undefined");
            this.AddTermsReportGroup("unary operator", "+", "-", "!", "~");

            this.AddToNoReportGroup(comma, semi);
            this.AddToNoReportGroup("var", "new", "++", "--", "this", "base", "typeof", "{", "}", "[", "]");
            #endregion

            #region Rules
            // Expressions
            expression.Rule = bin_op_expression | primary_expression;
            bin_op_expression.Rule = expression + bin_op + expression;
            primary_expression.Rule = literal | member_access;
            literal.Rule = number | strings | "true" | "false" | "null" | "undefined";
            initializer_value.Rule = expression;
            member_access.Rule = identifier;

            bin_op.Rule = ToTerm("+") | "-";
            assignment_operator.Rule = ToTerm("=") | "+=" | "-=";

            // Statements
            statement.Rule = declaration_statement | embedded_statement;
            statement.ErrorRule = SyntaxError + semi; // Skip all until semicolon
            statement_list.Rule = MakePlusRule(statement_list, null, statement);
            statement_list_opt.Rule = Empty | statement_list;

            declaration_statement.Rule = local_variable_declaration + semi;
            local_variable_declaration.Rule = vark + local_variable_declarators;
            local_variable_declarator.Rule = identifier | identifier + "=" + initializer_value;
            local_variable_declarators.Rule = MakePlusRule(local_variable_declarators, comma, local_variable_declarator);

            embedded_statement.Rule = block | semi /* Empty statement */ | statement_expression + semi;

            block.Rule = lbr + statement_list_opt + rbr;

            statement_expression.Rule = member_access + assignment_operator + expression;

            // Program
            program.Rule = statement_list_opt;
            #endregion

            this.Root = program;
        }
    }
}
