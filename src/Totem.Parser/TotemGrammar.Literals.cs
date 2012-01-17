using System;
using System.Globalization;
using Irony.Parsing;

namespace Totem.Compiler
{
    public partial class TotemGrammar
    {
        private StringLiteral CreateStringLiteral(string name)
        {
            var term = new StringLiteral(name);
            term.AddStartEnd("\"", StringOptions.AllowsAllEscapes);
            term.AddStartEnd("'", StringOptions.AllowsDoubledQuote | StringOptions.AllowsOctalEscapes | StringOptions.AllowsUEscapes | StringOptions.AllowsXEscapes);
            return term;
        }

        private NumberLiteral CreateNumberLiteral(string name)
        {
            var term = new NumberLiteral(name);
            term.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.Int64 };
            term.DefaultFloatType = TypeCode.Double;
            term.AddPrefix("0x", NumberOptions.Hex);
            term.AddSuffix("l", TypeCode.Int64, TypeCode.UInt64);
            term.AddSuffix("f", TypeCode.Single);
            term.AddSuffix("d", TypeCode.Double);
            return term;
        }

        private IdentifierTerminal CreateIdentifier(string name)
        {
            var term = new IdentifierTerminal(name);
            term.AddPrefix("@", IdOptions.IsNotKeyword);

            term.StartCharCategories.AddRange(new UnicodeCategory[] {
                UnicodeCategory.LowercaseLetter,
                UnicodeCategory.UppercaseLetter,
                UnicodeCategory.TitlecaseLetter,
                UnicodeCategory.ModifierLetter,
                UnicodeCategory.OtherLetter,
                UnicodeCategory.LetterNumber
            });

            term.CharCategories.AddRange(term.StartCharCategories);
            term.CharCategories.AddRange(new UnicodeCategory[] {
                UnicodeCategory.DecimalDigitNumber
            });

            term.CharsToRemoveCategories.Add(UnicodeCategory.Format);
            return term;
        }
    }
}
