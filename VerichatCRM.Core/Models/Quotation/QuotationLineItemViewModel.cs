using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Quotation
{
    public class QuotationLineItemViewModel
    {
        public int RowId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
