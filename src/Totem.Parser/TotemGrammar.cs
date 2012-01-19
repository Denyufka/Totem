using Irony.Parsing;

namespace Totem.Compiler
{
    [Language("Totem", "0.2", "A totem parser")]
    public partial class TotemGrammar : Irony.Parsing.Grammar
    {
        public TotemGrammar()
        {
            #region Lexical structure
            var @string = CreateStringLiteral("string");
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
            #region Terminals
            var @is = ToTerm("is", "is");
            var dot = ToTerm(".", "dot");
            var lt = ToTerm("<", "less");
            var gt = ToTerm(">", "greater");
            var lsbr = ToTerm("[", "left square bracket");
            var rsbr = ToTerm("]", "right square bracket");
            var lpr = ToTerm("(", "left parantesis");
            var rpr = ToTerm(")", "right parantesis");
            var lcbr = ToTerm("{", "left curly bracket");
            var rcbr = ToTerm("}", "right curly bracket");
            var comma = ToTerm(",", "comma");
            var semi = ToTerm(";", "semicolon");
            var colon = ToTerm(":", "colon");

            var @true = ToTerm("true", "true");
            var @false = ToTerm("false", "false");
            var @null = ToTerm("null", "null");
            var undefined = ToTerm("undefined", "undefined");
            var var = ToTerm("var", "var");
            var @new = ToTerm("new", "new");
            var @return = ToTerm("return", "return");
            var @throw = ToTerm("throw", "throw");
            var function = ToTerm("function", "function");
            #endregion

            #region 2. Non Terminals
            #region 2.0 Optional Terminals
            var identifierOpt = new NonTerminal("identifierOpt", Empty | identifier);
            #endregion
            #region 2.1 Expressions
            var Expr = new NonTerminal("Expr"/*, typeof(Expr)*/);
            var ParenExpr = new NonTerminal("ParenExpr");
            var MemberExpr = new NonTerminal("MemberExpr");
            var QualifiedName = new NonTerminal("QualifiedName");
            var ConstExpr = new NonTerminal("ConstExpr"/*, typeof(ConstExpr)*/);
            var BinExpr = new NonTerminal("BinExpr"/*, typeof(BinExpr)*/);
            var UnaryExpr = new NonTerminal("UnaryExpr"/*, typeof(UnaryExpr)*/);
            var AssignExpr = new NonTerminal("AssignExpr"/*, typeof(AssignExpr)*/);
            var FuncDefExpr = new NonTerminal("FuncDefExpr"/*, typeof(FuncDefExpr)*/);
            var VarExpr = new NonTerminal("VarExpr"/*, typeof(VarExpr)*/);
            var VarExprList = new NonTerminal("VarExprList"/*, typeof(VarExprList)*/);
            var Initializer = new NonTerminal("Initializer"/*, typeof(Initializer)*/);
            var InitializerOpt = new NonTerminal("InitializerOpt");

            var FunctionCall = new NonTerminal("FunctionCall"/*, typeof(FunctionCall)*/);
            var FunctionCallExpr = new NonTerminal("FunctionCallExpr");
            var Argument = new NonTerminal("Argument"/*, typeof(Argument)*/);
            var ArgumentList = new NonTerminal("ArgumentList"/*, typeof(ArgumentList)*/);
            var Parameter = new NonTerminal("Parameter"/*, typeof(Parameter)*/);
            var ParameterList = new NonTerminal("ParameterList"/* typeof(ParameterList)*/);

            var AssignOp = new NonTerminal("AssignOp");
            var PostOp = new NonTerminal("PostOp");
            var BinOp = new NonTerminal("BinOp");
            var LUnOp = new NonTerminal("LUnOp");
            var RUnOp = new NonTerminal("RUnOp");
            #endregion

            #region 2.2 Qualified Names
            var ExpressionList = new NonTerminal("ExpressionList"/*, typeof(ExpressionList)*/);

            var NewExpr = new NonTerminal("NewExpr"/*, typeof(NewExpr)*/);

            var ArrayResolution = new NonTerminal("ArrayResolution");
            #endregion

            #region 2.3 Statement
            var Condition = new NonTerminal("Condition"/*, typeof(Condition)*/);
            var Statement = new NonTerminal("Statement"/*, typeof(Statement)*/);

            var VarStmt = new NonTerminal("VarStmt");
            var ExprStmt = new NonTerminal("ExprStmt"/*, typeof(ExprStmt)*/);

            var Block = new NonTerminal("Block"/*, typeof(Block)*/);
            var StmtList = new NonTerminal("StmtList"/*, typeof(StmtList)*/);
            var FuncDefStmt = new NonTerminal("FuncDefStmt"/*, typeof(FuncDefStmt)*/);

            var FlowControlStmt = new NonTerminal("FlowControlStmt"/*, typeof(FlowControlStmt)*/);
            #endregion

            #region 2.4 Program and Functions
            var Prog = new NonTerminal("Prog");
            var Element = new NonTerminal("Element");
            var ElementList = new NonTerminal("ElementList");
            #endregion
            #endregion

            #region 3. BNF Rules
            #region 3.1 Expressions
            ConstExpr.Rule = @true | @false | undefined | @null | @string | number;

            BinExpr.Rule = Expr + BinOp + Expr;

            UnaryExpr.Rule = LUnOp + Expr;

            QualifiedName.Rule = MemberExpr | identifier;
            AssignExpr.Rule = QualifiedName + AssignOp + Expr
                | QualifiedName + PostOp;

            FuncDefExpr.Rule = function + identifierOpt + lpr + ParameterList + rpr + Block;
            ParameterList.Rule = MakeStarRule(ParameterList, comma, Parameter);
            Parameter.Rule = identifier + InitializerOpt;

            Expr.Rule = ConstExpr | BinExpr | UnaryExpr | identifier | AssignExpr | FuncDefExpr | ParenExpr | FunctionCallExpr | MemberExpr;// | NewExpr;
            ParenExpr.Rule = lpr + Expr + rpr;
            MemberExpr.Rule = Expr + dot + identifier;

            VarStmt.Rule = var + VarExprList + semi;
            VarExprList.Rule = MakePlusRule(VarExprList, comma, VarExpr);
            VarExpr.Rule = identifier + InitializerOpt;
            Initializer.Rule = ToTerm("=", "equals") + Expr;
            InitializerOpt.Rule = Empty | Initializer;

            NewExpr.Rule = @new + Expr + FunctionCall;

            BinOp.Rule = ToTerm("+") | "-";
            LUnOp.Rule = ToTerm("-") | "!" | @new;
            AssignOp.Rule = ToTerm("=") | "+=" | "-=";
            PostOp.Rule = ToTerm("--") | "++";

            FunctionCallExpr.Rule = Expr + FunctionCall;
            #endregion

            #region 3.2 Qualified Names
            FunctionCall.Rule = lpr + ArgumentList + rpr;
            ArgumentList.Rule = MakeStarRule(ArgumentList, comma, Argument);
            Argument.Rule = Expr | identifier + colon + Expr | @string + colon + Expr;

            MemberExpr.Rule = Expr + dot + identifier;

            ArrayResolution.Rule = lsbr + Expr + rsbr;
            #endregion

            #region 3.3 Statement
            Condition.Rule = lpr + Expr + rpr;
            ExprStmt.Rule = AssignExpr + semi
                | FunctionCallExpr + semi;
            FlowControlStmt.Rule = @return + semi
                | @return + Expr + semi
                | @throw + Expr + semi;

            Statement.Rule = semi // Empty statement
                | Block
                | ExprStmt
                | VarStmt
                | FlowControlStmt;

            StmtList.Rule = MakeStarRule(StmtList, null, Statement);

            FuncDefStmt.Rule = function + identifier + lpr + ParameterList + rpr + Block;

            Block.Rule = lcbr + StmtList + rcbr;
            #endregion

            #region 3.4 Program and Functions
            Element.Rule = Statement | FuncDefStmt;

            ElementList.Rule = MakeStarRule(ElementList, null, Element);

            Prog.Rule = ElementList + Eof;
            #endregion
            #endregion

            #region 4. Set root
            Root = Prog;
            #endregion

            #region 5. Operators precedence
            RegisterOperators(1, "||");
            RegisterOperators(2, "&&");
            RegisterOperators(3, "|");
            RegisterOperators(4, "^");
            RegisterOperators(5, "&");
            RegisterOperators(6, "==", "!=");
            RegisterOperators(7, "<", ">", "<=", ">=", "is");
            RegisterOperators(8, "<<", ">>");
            RegisterOperators(9, "+", "-");
            RegisterOperators(10, "*", "/", "%");
            RegisterOperators(-2, "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            RegisterOperators(-1, "?");
            #endregion

            #region 6. Punctuation Symbols
            Delimiters = "{}[](),:;+-*/%&|^!~<>=";
            MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":");
            #endregion

            MarkTransient(Element, Statement, InitializerOpt, Expr, ParenExpr, BinOp, LUnOp, AssignOp, PostOp, FunctionCall, QualifiedName);
            //LanguageFlags = Irony.Parsing.LanguageFlags.CreateAst;
            #endregion
        }
    }
}
