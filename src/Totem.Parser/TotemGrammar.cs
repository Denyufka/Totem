using Irony.Parsing;

namespace Totem.Compiler
{
    [Language("Totem", "0.2", "A totem parser")]
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

            var parameter = new NonTerminal("parameter");
            var parameter_list = new NonTerminal("parameter_list");
            var parameter_list_opt = new NonTerminal("parameter_list_opt");

            var argument = new NonTerminal("argument");
            var argument_list = new NonTerminal("argument_list");
            var argument_list_opt = new NonTerminal("argument_list_opt");
            var argument_list_par = new NonTerminal("argument_list_par");

            var function_call = new NonTerminal("function_call");

            var return_expression = new NonTerminal("return_expression");

            var member_access_segment = new NonTerminal("member_access_segment");
            var member_access_segment_list = new NonTerminal("member_access_segment_list");
            var member_access_segment_list_opt = new NonTerminal("member_access_segment_list_opt");

            // Statements
            var statement = new NonTerminal("statement", "statement");
            var statement_list = new NonTerminal("statement_list");
            var statement_list_opt = new NonTerminal("statement_list_opt");
            var declaration_statement = new NonTerminal("declaration_statement");
            var embedded_statement = new NonTerminal("embedded_statement");
            var local_variable_declaration = new NonTerminal("local_variable_declaration");
            var local_variable_declarator = new NonTerminal("local_variable_declarator");
            var local_variable_declarators = new NonTerminal("local_variable_declarators");
            var local_function_declaration = new NonTerminal("local_function_declaration");
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
            member_access.Rule = identifier + member_access_segment_list_opt;
            return_expression.Rule = ToTerm("return") + expression;

            member_access_segment.Rule = dot + identifier | argument_list_par;
            member_access_segment_list.Rule = MakePlusRule(member_access_segment_list, null, member_access_segment);
            member_access_segment_list_opt.Rule = Empty | member_access_segment_list;

            argument.Rule = expression | identifier + ":" + expression;
            argument_list.Rule = MakePlusRule(argument_list, comma, argument);
            argument_list_opt.Rule = Empty | argument_list;
            argument_list_par.Rule = lpar + argument_list_opt + rpar;

            bin_op.Rule = ToTerm("+") | "-";
            assignment_operator.Rule = ToTerm("=") | "+=" | "-=";

            // Statements
            statement.Rule = declaration_statement | embedded_statement;
            statement.ErrorRule = SyntaxError + semi; // Skip all until semicolon
            statement_list.Rule = MakePlusRule(statement_list, null, statement);
            statement_list_opt.Rule = Empty | statement_list;

            parameter.Rule = identifier | identifier + "=" + expression;
            parameter_list.Rule = MakePlusRule(parameter_list, comma, parameter);
            parameter_list_opt.Rule = Empty | parameter_list;

            declaration_statement.Rule = local_variable_declaration + semi | local_function_declaration;
            local_variable_declaration.Rule = vark + local_variable_declarators;
            local_variable_declarator.Rule = identifier | identifier + "=" + initializer_value;
            local_variable_declarators.Rule = MakePlusRule(local_variable_declarators, comma, local_variable_declarator);
            local_function_declaration.Rule = ToTerm("function") + identifier + "(" + parameter_list_opt + ")" + block;

            embedded_statement.Rule = block | semi /* Empty statement */ | statement_expression + semi | return_expression + semi | function_call + semi;
            function_call.Rule = member_access;

            block.Rule = lbr + statement_list_opt + rbr;

            statement_expression.Rule = member_access + assignment_operator + expression;

            // Program
            program.Rule = statement_list_opt;
            #endregion

            this.Root = program;
        }
    }
}
