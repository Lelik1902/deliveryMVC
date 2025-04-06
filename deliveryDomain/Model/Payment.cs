using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace deliveryDomain.Model;

public partial class Payment : Entity
{
    [Display(Name = "Номер замовлення")]

    public int OrderId { get; set; }

    [Display(Name = "Дата оплати")]
    public DateTime PaymentDate { get; set; }

    [Display(Name = "Сума")]
    public decimal Amount { get; set; }

    [Display(Name = "Метод оплати")]
    public string PaymentMethod { get; set; } = null!;


    public virtual Order? Order { get; set; } = null!;
}
