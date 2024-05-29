using System.ComponentModel.DataAnnotations;
using Cpb.Domain;

namespace Cpb.Database.Entities;

public class DbCoffeeMachine : DbEntity
{
    public const int NameMaxLength = 128;
    public const int UrlLength = 128;
    
    public Guid Id { get; set; }
    
    [MaxLength(NameMaxLength)]
    public string Name { get; set; }
    public CoffeeMachineStates State { get; set; }
    
    [MaxLength(UrlLength)]
    public string MachineHealthCheckEndpointUrl { get; set; }

    public List<DbIngredient> Ingredients { get; set; }
    
    public List<DbCoffeeMachineIngredient> Links { get; set; }
}