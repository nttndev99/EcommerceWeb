using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Enums
{
    public enum ProductStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        OutOfStock = 3,
        Discontinued = 4
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public enum AdjustmentType
    {
        Add    = 1,   // nhập kho / bổ sung
        Remove = 2,   // xuất kho / hao hụt
        Set    = 3,   // kiểm kê – set thẳng số lượng
        Import = 4,   // nhập theo Purchase Order
        Sale   = 5,   // trừ khi có đơn hàng
        Return = 6    // trả hàng – cộng lại
    }

    public enum OrderStatus
    {
        Pending    = 0,
        Processing = 1,
        Shipped    = 2,
        Delivered  = 3,
        Cancelled  = 4,
    }
    
    public enum PaymentStatus
    {
        Unpaid   = 0,
        Paid     = 1,
        Refunded = 2,
        Failed   = 3,
    }
    
    public enum PaymentMethod
    {
        COD          = 0,
        BankTransfer = 1,
        CreditCard   = 2,
        Momo         = 3,
    }

}