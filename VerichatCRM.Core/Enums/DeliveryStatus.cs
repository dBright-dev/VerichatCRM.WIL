using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Enums
{
    public static class DeliveryStatuses
    {
        public const string Processing = "Processing";
        public const string Dispatched = "Dispatched";
        public const string InTransit = "In Transit";
        public const string Delivered = "Delivered";
        public const string Failed = "Failed";
        public const string Returned = "Returned";

        public static readonly string[] All =
        {
        Processing, Dispatched, InTransit, Delivered, Failed, Returned
    };
    }

}
