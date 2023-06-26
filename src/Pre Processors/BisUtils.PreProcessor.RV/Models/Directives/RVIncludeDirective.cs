﻿namespace BisUtils.PreProcessor.RV.Models.Directives;

using Elements;
using FResults;
using Lexer;
using Stubs;

public interface IRVIncludeDirective : IRVDirective
{
    IRVIncludeString IncludeTarget { get; }
}


public class RVIncludeDirective : RVDirective, IRVIncludeDirective
{
    public IRVIncludeString IncludeTarget { get; set; }

    public RVIncludeDirective(IRVPreProcessor processor, IRVIncludeString includeTarget) : base(processor, "include") =>
        IncludeTarget = includeTarget;

    public override Result ToText(out string str) => throw new NotImplementedException();

    public static Result ParseDirective(IRVPreProcessor processor, RVLexerOld lexerOld, out IRVIncludeDirective include)
    {
        var result = RVIncludeString.ParseString(processor, lexerOld, out var includeString);
        include = new RVIncludeDirective(processor, includeString);
        return result;
    }

    public override Result Process(RVLexerOld lexerOld, int startPosition) => throw new NotImplementedException();
}
