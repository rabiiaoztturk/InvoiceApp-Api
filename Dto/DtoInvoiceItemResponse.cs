﻿namespace InvoiceApp.Dto;

public class DtoInvoiceItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public int InvoiceId { get; set; }
}
