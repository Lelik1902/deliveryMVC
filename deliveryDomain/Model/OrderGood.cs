using System;
using System.Collections.Generic;

namespace deliveryDomain.Model;

public partial class OrderGood
{
    public int OrderId { get; set; }
    public int GoodId { get; set; }
    public int Quantity { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual Good Good { get; set; } = null!;
}