using System;
using System.Collections.Generic;

namespace WebAPIWithEF.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public string? CustomerTypeId { get; set; }

    public string? StateCode { get; set; }
}
