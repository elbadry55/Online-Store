using System;
using System.Collections.Generic;
using System.Text;

namespace Online_Store.Models
{
    public enum OrderStatus
    {
        Pending, 
        Shipped,
        Delivered,
        Cancelled
    }
}
