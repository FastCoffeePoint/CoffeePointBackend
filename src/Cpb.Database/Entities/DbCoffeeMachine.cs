using System.ComponentModel.DataAnnotations;
using Cpb.Domain;

namespace Cpb.Database.Entities;

public class DbCoffeeMachine : DbEntity
{
    public Guid Id { get; set; }
    public CoffeeMachineState State { get; set; }
    [MaxLength(128)]
    public string Url { get; set; }

    public List<DbIngredient> Ingredients { get; set; }
}