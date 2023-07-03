﻿namespace BisUtils.Param.Models.Literals;

using Core.IO;
using Enumerations;
using FResults;
using Options;
using Stubs;

public interface IParamString : IParamLiteral<string>
{
    ParamStringType StringType { get; }


    public bool ToInt(out IParamInt? paramInt);
    public bool ToFloat(out IParamFloat? paramFloat);
}

public struct ParamString : IParamString
{
    public Result? LastResult { get; private set; } = null;
    public IParamFile? ParamFile { get; set; }

    public ParamStringType StringType { get; set; } = ParamStringType.Unquoted;

    public string ParamValue { get => paramValue; set => paramValue = value; }
    private string paramValue = "";

    public ParamString(string value, ParamStringType type = ParamStringType.Quoted)
    {
        paramValue = value;
        StringType = type;
    }

    public Result Binarize(BisBinaryWriter writer, ParamOptions options)
    {
        writer.Write(options.LiteralIdFoster(GetType()));
        writer.WriteAsciiZ(ParamValue, options);
        return LastResult = Result.ImmutableOk();
    }

    public Result Debinarize(BisBinaryReader reader, ParamOptions options) =>
        LastResult = reader.ReadAsciiZ(out paramValue, options);

    public Result ToParam(out string str, ParamOptions options) =>
        LastResult = Stringify(out str, ParamValue, StringType, options);

    public Result Validate(ParamOptions options) =>
        LastResult = Result.ImmutableOk();

    public static Result Stringify(out string stringified, string str, ParamStringType stringType, ParamOptions options)
    {
        switch (stringType)
        {
            case ParamStringType.Quoted:
            case ParamStringType.Unquoted:
                stringified = ""; //TODO:
                return Result.ImmutableOk();
            default:
                stringified = "";
                return Result.Fail("");
        }
    }


#pragma warning disable CA1305 //TODO: Options with locale
    public bool ToInt(out IParamInt paramInt)
    {
        var response = int.TryParse(ParamValue, out var parsedInt);
        if (!response)
        {
            paramInt = null;
            return response;
        }
        paramInt = new ParamInt() { ParamValue = parsedInt, ParamFile = ParamFile };
        return true;
    }

    public bool ToFloat(out IParamFloat paramFloat)
    {
        var response = float.TryParse(ParamValue, out var parsedInt);
        if (!response)
        {
            paramFloat = null;
            return response;
        }
        paramFloat = new ParamFloat { ParamValue = parsedInt, ParamFile = ParamFile };
        return true;
    }

    public IParamFloat? ToFloat()
    {
        ToFloat(out var paramFloat);
        return paramFloat;
    }

    public IParamInt? ToInt()
    {
        ToInt(out var paramInt);
        return paramInt;
    }
#pragma warning restore CA1305

}
