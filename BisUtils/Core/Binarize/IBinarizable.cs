﻿using BisUtils.Core.Binarize.Options;
using BisUtils.Core.Binarize.Utils;
using BisUtils.Core.IO;

namespace BisUtils.Core.Binarize;

public interface IBinarizable<in TBinarizationOptions> where TBinarizationOptions : IBinarizationOptions
{
    [MustBeValidated("Object is not currently in a valid state to be written.")]
    public BinarizationResult Binarize(BisBinaryWriter writer, TBinarizationOptions options);
}