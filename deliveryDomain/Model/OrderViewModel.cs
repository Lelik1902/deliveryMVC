using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deliveryDomain.Model
{
    public class OrderViewModel
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public int CourierId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public string DeliveryAddress { get; set; } = null!;

        public string Status { get; set; } = "Нове";

        public List<int> SelectedGoods { get; set; } = new(); // Обрані товари

        //public List<SelectListItem> Clients { get; set; } = new();
        //public List<SelectListItem> Couriers { get; set; } = new();
        //public List<SelectListItem> Goods { get; set; } = new();
    }

}
