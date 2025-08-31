using SqlWriter.Core;

namespace SqlWriter.Tests;

public class SqlWriterTests
{
	private readonly SqlCompiler sut = new();

	[Theory]
	[InlineData("Name", """
	                    SELECT Name
	                    FROM User
	                    """
	)]
	[InlineData("Password", """
	                        SELECT Password
	                        FROM User
	                        """
	)]
	[InlineData("OrderNumber", """
	                           SELECT OrderNumber
	                           FROM Order
	                           """
	)]
	public void Should_Include_Table_With_Single_Column(string columnName, string expected)
	{
		var result = this.sut.Compile(columnName);

		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("""
	            SELECT Name, Password
	            FROM User
	            """, "Name", "Password"
	)]
	public void Should_Include_Table_With_MultipleSingle(string expected, params string[] columnNames)
	{
		var result = this.sut.Compile(columnNames);

		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("""
	            SELECT Name, Password, OrderNumber
	            FROM User
	            LEFT OUTER JOIN Order on Order.UserId = User.Id
	            """, "Name", "Password", "OrderNumber"
	)]
	[InlineData("""
	            SELECT OrderNumber, Name, Password
	            FROM Order
	            LEFT OUTER JOIN User on User.Id = Order.UserId
	            """, "OrderNumber", "Name", "Password"
	)]

// User → Order → OrderItem
	[InlineData("""
	            SELECT Name, OrderNumber, Quantity
	            FROM User
	            LEFT OUTER JOIN Order on Order.UserId = User.Id
	            LEFT OUTER JOIN OrderItem on OrderItem.OrderId = Order.Id
	            """, "Name", "OrderNumber", "Quantity"
	)]

// Order → OrderItem → Product
	[InlineData("""
	            SELECT OrderNumber, ProductName, Price, Quantity
	            FROM Order
	            LEFT OUTER JOIN OrderItem on OrderItem.OrderId = Order.Id
	            LEFT OUTER JOIN Product on Product.Id = OrderItem.ProductId
	            """, "OrderNumber", "ProductName", "Price", "Quantity"
	)]

// User → Order → Invoice
	[InlineData("""
	            SELECT Name, OrderNumber, InvoiceNumber, InvoiceDate
	            FROM User
	            LEFT OUTER JOIN Order on Order.UserId = User.Id
	            LEFT OUTER JOIN Invoice on Invoice.OrderId = Order.Id
	            """, "Name", "OrderNumber", "InvoiceNumber", "InvoiceDate"
	)]

// Order → Invoice → Payment
	[InlineData("""
	            SELECT OrderNumber, InvoiceNumber, Amount, PaymentDate
	            FROM Order
	            LEFT OUTER JOIN Invoice on Invoice.OrderId = Order.Id
	            LEFT OUTER JOIN Payment on Payment.InvoiceId = Invoice.Id
	            """, "OrderNumber", "InvoiceNumber", "Amount", "PaymentDate"
	)]

// User → Order → OrderItem → Product → Invoice → Payment
	[InlineData("""
	            SELECT Name, OrderNumber, ProductName, Quantity, InvoiceNumber, Amount
	            FROM User
	            LEFT OUTER JOIN Order on Order.UserId = User.Id
	            LEFT OUTER JOIN OrderItem on OrderItem.OrderId = Order.Id
	            LEFT OUTER JOIN Product on Product.Id = OrderItem.ProductId
	            LEFT OUTER JOIN Invoice on Invoice.OrderId = Order.Id
	            LEFT OUTER JOIN Payment on Payment.InvoiceId = Invoice.Id
	            """, "Name", "OrderNumber", "ProductName", "Quantity", "InvoiceNumber"
		, "Amount"
	)]
	public void Should_Handle_Columns_From_Multiple_Tables(string expected, params string[] columnNames)
	{
		var result = this.sut.Compile(columnNames);

		Assert.Equal(expected, result);
	}
}