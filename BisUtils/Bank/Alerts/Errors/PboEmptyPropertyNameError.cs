﻿namespace BisUtils.Bank.Alerts.Errors;

using BisUtils.Bank.Model;
using FResults.Reasoning;

public class PboEmptyPropertyNameError : ErrorBase
{
    public static readonly PboEmptyPropertyNameError Instance = new();

    public static PboEmptyPropertyNameError CreateInstanceWithData(Dictionary<string, object> metadata) =>
        new() { Metadata = metadata };

    private PboEmptyPropertyNameError()
    {
    }

    public override string? AlertName
    {
        get => "EmptyPboProperty";
        init => throw new NotSupportedException();
    }

    public override Type? AlertScope
    {
        get => typeof(PboFile);
        init => throw new NotSupportedException();
    }

    public override string? Message
    {
        get => "Pbo Properties cannot be empty as this is what signifies the last property";
        set => throw new NotSupportedException();
    }

}
