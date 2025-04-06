using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace deliveryDomain.Model;

public partial class Good : Entity
{
    public string Name { get; set; } = null!;

    [Range(0.01, double.MaxValue, ErrorMessage = "Ціна не може бути менше за 0.")]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderGood> OrderGoods { get; set; } = new List<OrderGood>();
}