using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace deliveryDomain.Model;

public partial class Courier : Entity
{
    [Display(Name = "Імя")]
    public string Name { get; set; } = null!;

    [Display(Name = "Телефон")]
    public string PhoneNumber { get; set; } = null!;

    [Display(Name = "Тип транспорту")]
    public string VehicleType { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
