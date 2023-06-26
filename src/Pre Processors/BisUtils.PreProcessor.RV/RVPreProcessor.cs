﻿namespace BisUtils.PreProcessor.RV;

using Core.Parsing;
using Core.Parsing.Lexer;
using Enumerations;
using Models.Directives;
using Utils;

public interface IRVPreProcessor : IBisPreProcessor<RvTypes>
{
    List<IRVDefineDirective> MacroDefinitions { get; }

    RVIncludeFinder IncludeLocator { get; }
}

public class RVPreProcessor : BisPreProcessor<RvTypes>, IRVPreProcessor
{
    public List<IRVDefineDirective> MacroDefinitions { get; } = new ();
    public required RVIncludeFinder IncludeLocator { get; init; }

    private static readonly IBisLexer<RvTypes>.TokenDefinition EOFDefinition =
        CreateTokenDefinition("rv.eof", RvTypes.EOF, 200);

    private static readonly IBisLexer<RvTypes>.TokenDefinition TextDefinition =
        CreateTokenDefinition("rv.text", RvTypes.Text, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition DHashDefinition =
        CreateTokenDefinition("rv.hash.double", RvTypes.DoubleHash, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition HashDefinition =
        CreateTokenDefinition("rv.hash.single", RvTypes.Hash, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition CommaDefinition =
        CreateTokenDefinition("rv.comma", RvTypes.Comma, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition DefineDefinition =
        CreateTokenDefinition("rv.directive.define", RvTypes.Define, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition IncludeDefinition =
        CreateTokenDefinition("rv.directive.include", RvTypes.Include, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition NewLineDefinition =
        CreateTokenDefinition("rv.newLine", RvTypes.NewLine, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition DirectiveNewLineDefinition =
        CreateTokenDefinition("rv.newLine.directive", RvTypes.DirectiveNewLine, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition LineCommentDefinition =
        CreateTokenDefinition("rv.comment.line", RvTypes.LineComment, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition BlockCommentDefinition =
        CreateTokenDefinition("rv.comment.block", RvTypes.BlockComment, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition ElseDefinition =
        CreateTokenDefinition("rv.directive.else", RvTypes.Else, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition IfDefDefinition =
        CreateTokenDefinition("rv.directive.ifdef", RvTypes.IfDef, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition IfNDefDefinition =
        CreateTokenDefinition("rv.directive.ifdef.not", RvTypes.IfNDef, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition EndifDefinition =
        CreateTokenDefinition("rv.directive.endif", RvTypes.EndIf, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition LeftParenthesisDefinition =
        CreateTokenDefinition("rv.parenthesis.left", RvTypes.LeftParenthesis, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition RightParenthesisDefinition =
        CreateTokenDefinition("rv.parenthesis.right", RvTypes.RightParenthesis, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition LeftAngleDefinition =
        CreateTokenDefinition("rv.angle.left", RvTypes.LeftAngle, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition RightAngleDefinition =
        CreateTokenDefinition("rv.angle.right", RvTypes.RightAngle, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition DoubleQuoteDefinition =
        CreateTokenDefinition("rv.quote.double", RvTypes.DoubleQuote, 1);

    private static readonly IBisLexer<RvTypes>.TokenDefinition WhitespaceDefinition =
        CreateTokenDefinition("rv.whitespace", RvTypes.Whitespace, 1);

    private static readonly IEnumerable<IBisLexer<RvTypes>.TokenDefinition> TokenDefinitions = new[]
    {
        EOFDefinition, TextDefinition, DHashDefinition, HashDefinition, CommaDefinition, LeftParenthesisDefinition,
        RightParenthesisDefinition, LeftAngleDefinition, RightAngleDefinition, DoubleQuoteDefinition,
        LineCommentDefinition, BlockCommentDefinition, DirectiveNewLineDefinition, NewLineDefinition,
        IfDefDefinition, IfNDefDefinition, EndifDefinition, IncludeDefinition, ElseDefinition

    };

    public override IEnumerable<IBisLexer<RvTypes>.TokenDefinition> TokenTypes => TokenDefinitions;
    public override IBisLexer<RvTypes>.TokenDefinition EOFToken => EOFDefinition;

    public RVPreProcessor(string content, List<IRVDefineDirective> macroDefinitions) : base(content)
    {

    }

    protected override IBisLexer<RvTypes>.TokenMatch GetNextToken()
    {
        MoveForward();

        var start = Position;
        if (IsEOF() || CurrentChar is null)
        {
        }
        if (IsWhitespace())
        {
            while (IsWhitespace(PeekForward()))
            {
                MoveForward();
            }

            return CreateTokenMatch(start..Position, WhitespaceDefinition);
        }

        switch (CurrentChar)
        {
            case null:
                return CreateTokenMatch(..0, "", EOFDefinition);
            case ',':
                return CreateTokenMatch(start..Position, ",", CommaDefinition);
            case '(':
                return CreateTokenMatch(start..Position, "(", LeftParenthesisDefinition);
            case ')':
                return CreateTokenMatch(start..Position, ")", RightParenthesisDefinition);
            case '<':
                return CreateTokenMatch(start..Position, "<", LeftAngleDefinition);
            case '>':
                return CreateTokenMatch(start..Position, ">", RightAngleDefinition);
            case '"':
                return CreateTokenMatch(start..Position, "\"", DoubleQuoteDefinition);
            case '\n':
                return CreateTokenMatch(start..Position, "\n", NewLineDefinition);
            case '\r':
            {
                if (PeekForward() == '\n')
                {
                    MoveForward();
                }

                return CreateTokenMatch(start..Position, "\r\n", NewLineDefinition);
            }
            case '/':
            {
                switch (PeekForward())
                {
                    case '/':
                        return CreateTokenMatch(start..(Position + TraverseLine()), LineCommentDefinition);
                    case '*':
                        while (!(PreviousChar == '*' && CurrentChar == '/') && CurrentChar != null)
                        {
                        }

                        return CreateTokenMatch(start..(Position + TraverseLine()), BlockCommentDefinition);
                }

                break;
            }
            case '#':
            {
                if (PeekForward() != '#')
                {
                    return CreateTokenMatch(start..Position, "#", HashDefinition);
                }

                MoveForward();
                return CreateTokenMatch(start..Position, "##", DHashDefinition);
            }
            case '\\':
            {
                if (PeekForward() == '\r')
                {
                    MoveForward();
                }

                if (PeekForward() == '\n')
                {
                    MoveForward();
                    return CreateTokenMatch(start..Position,"\\\n", DirectiveNewLineDefinition);
                }
                break;
            }
            case 'd':
            {
                if (PeekForwardMulti(6) == "define")
                {
                    MoveForward(5);
                    return CreateTokenMatch(start..Position, DefineDefinition);
                }

                break;
            }
            case 'e':
            {
                switch (PeekForward())
                {
                    case 'l':
                    {
                        if (PeekForwardMulti(4) == "else")
                        {
                            MoveForward(3);
                            return CreateTokenMatch(start..Position, "else", ElseDefinition);
                        }

                        break;
                    }
                    case 'n':
                    {
                        if (PeekForwardMulti(5) == "endif")
                        {
                            MoveForward(4);
                            return CreateTokenMatch(start..Position, "endif", ElseDefinition);
                        }

                        break;
                    }

                }

                break;
            }
            case 'i':
            {
                switch (PeekForward())
                {
                    case 'f':
                    {
                        MoveForward();
                        switch (MoveForward())
                        {
                            case 'n':
                            {
                                if (PeekForwardMulti(4) == "ndef")
                                {
                                    MoveForward(3);
                                    return CreateTokenMatch(start..Position, "ifndef", IfNDefDefinition);
                                }
                                break;
                            }
                            case 'd':
                            {
                                if (PeekForwardMulti(3) == "def")
                                {
                                    MoveForward(2);
                                    return CreateTokenMatch(start..Position, "ifdef", IfDefDefinition);
                                }

                                break;
                            }
                        }
                        break;
                    }
                    case 'n':
                    {
                        if (PeekForwardMulti(7) == "include")
                        {
                            MoveForward(6);
                            return CreateTokenMatch(start..Position, "include", IncludeDefinition);
                        }
                        break;
                    }
                }
                break;
            }

        }
        throw new IOException();
    }


    private bool IsWhitespace(char? c = null) => (c ?? CurrentChar) switch
    {
        '\t' or '\u000B' or '\u000C' or ' ' or '\r' or '\n' => true,
        _ => false
    };

    public int TraverseLine()
    {
        var charCount = 0;
        while (true)
        {
            switch (CurrentChar)
            {
                case null:
                    return charCount;
                case '\n':
                    MoveForward();
                    return ++charCount;
                default:
                    charCount++;
                    MoveForward();
                    break;
            }
        }
    }

}
