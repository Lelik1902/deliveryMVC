using System;
using System.Collections.Generic;

namespace deliveryDomain.Model;

public partial class Order : Entity
{
    public int ClientId { get; set; }

    public int CourierId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? DeliveryAddress { get; set; } = null!;

    public string? Status { get; set; } = null!;

    public virtual Courier? Courier { get; set; }

    public virtual Client? Client { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<OrderGood> OrderGoods { get; set; } = new List<OrderGood>();
}