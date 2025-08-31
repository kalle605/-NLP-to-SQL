using SqlWriter.Core.AI.NLP2SQL;

namespace SqlWriter.Tests;

public class NLP2SQLTests
{
	private readonly SqlTransformer sut = new();
	
	[Theory]
	[InlineData("What is the order quantity of order '25121'",
		"""
		SELECT Quantity
		FROM Order
		LEFT OUTER JOIN OrderItem on OrderItem.OrderId = Order.Id
		WHERE OrderNumber = '25121'
		""")]

	[InlineData("What is the email address of the user who placed order '25121'",
		"""
		SELECT Email
		FROM [User]
		LEFT OUTER JOIN [Order] ON [Order].UserId = [User].Id
		WHERE OrderNumber = '25121'
		""")]

	[InlineData("What is the product name and price for order '25121'",
		"""
		SELECT ProductName, Price
		FROM [Order]
		LEFT OUTER JOIN OrderItem ON OrderItem.OrderId = [Order].Id
		LEFT OUTER JOIN Product ON Product.Id = OrderItem.ProductId
		WHERE OrderNumber = '25121'
		""")]

	[InlineData("What is the invoice number for order '25121'",
		"""
		SELECT InvoiceNumber
		FROM [Order]
		LEFT OUTER JOIN Invoice ON Invoice.OrderId = [Order].Id
		WHERE OrderNumber = '25121'
		""")]

	[InlineData("What is the payment method for invoice 'INV-1001'",
		"""
		SELECT PaymentMethod
		FROM Invoice
		LEFT OUTER JOIN Payment ON Payment.InvoiceId = Invoice.Id
		WHERE InvoiceNumber = 'INV-1001'
		""")]

	[InlineData("What is the stock quantity of product 'Widget'",
		"""
		SELECT StockQuantity
		FROM Product
		WHERE ProductName = 'Widget'
		""")]

	[InlineData("What is the address of the user with email 'john@example.com'",
		"""
		SELECT Address
		FROM [User]
		WHERE Email = 'john@example.com'
		""")]

	[InlineData("What is the order date for invoice 'INV-1001'",
		"""
		SELECT OrderDate
		FROM Invoice
		LEFT OUTER JOIN [Order] ON [Order].Id = Invoice.OrderId
		WHERE InvoiceNumber = 'INV-1001'
		""")]

	[InlineData("What is the total amount paid for invoice 'INV-1001'",
		"""
		SELECT Amount
		FROM Invoice
		LEFT OUTER JOIN Payment ON Payment.InvoiceId = Invoice.Id
		WHERE InvoiceNumber = 'INV-1001'
		""")]

	[InlineData("What is the status of order '25121'",
		"""
		SELECT Status
		FROM [Order]
		WHERE OrderNumber = '25121'
		""")]

	[InlineData("What is the quantity and unit price for product 'Widget' in order '25121'",
		"""
		SELECT Quantity, UnitPrice
		FROM [Order]
		LEFT OUTER JOIN OrderItem ON OrderItem.OrderId = [Order].Id
		LEFT OUTER JOIN Product ON Product.Id = OrderItem.ProductId
		WHERE OrderNumber = '25121' AND ProductName = 'Widget'
		""")]
	public async Task Should_Transform_Text_To_SQL(string question, string expected)
	{
		var result = await this.sut.Transform(question);

		Assert.Equal(expected, result);
	}
}