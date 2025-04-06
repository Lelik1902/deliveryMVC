using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace deliveryDomain.Model;

public partial class Category : Entity
{
    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Категорія")]
    public string CategoryName { get; set; } = null!;

    [Display(Name = "Інформація про категорію")]
    public string Description { get; set; } = null!;

    public virtual ICollection<Good> Goods { get; set; } = new List<Good>();
}
