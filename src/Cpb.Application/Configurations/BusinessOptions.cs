using Cpb.Common;

namespace Cpb.Application.Configurations;

public class BusinessOptions : IOptions
{
    public static string Name => "BusinessOptions";

    public double IngredientMissingTolerancePercent { get; set; }
}