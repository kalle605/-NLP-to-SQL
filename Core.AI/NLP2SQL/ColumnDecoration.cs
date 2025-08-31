using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace SqlWriter.Core.AI.NLP2SQL;

public class ColumnDecoration
{
	public class ColumnMetadata
	{
		[VectorStoreKey]
		[TextSearchResultName]
		public string Id { get; } = Guid.NewGuid().ToString();
		
		public string ColumnName { get; }
		public string TableName { get; }

		// [VectorStoreVector()]
		// [TextSearchResultValue]
		// public string Description { get; set; }
		
		[VectorStoreData(IsFullTextIndexed = true)]
		public string Description { get; set; }

		[VectorStoreVector(Dimensions: 4, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
		public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
		
		public ColumnMetadata(string columnName, string tableName, string description)
		{
			this.ColumnName = columnName;
			this.TableName = tableName;
			this.Description = description;
		}

		public ColumnMetadata()
		{
			
		}
	};

	public static readonly List<ColumnMetadata> Metadata =
	[
		new ColumnMetadata("Name", "User", "The full name of the user or customer"),
		new ColumnMetadata("Password", "User", "The user's account password (usually stored as a hash)"),
		new ColumnMetadata("Email", "User", "The user's email address used for login or communication"),
		new ColumnMetadata("Address", "User", "The user's home or mailing address"),
		new ColumnMetadata("OrderNumber", "Order", "A unique identifier assigned to each order"),
		new ColumnMetadata("OrderDate", "Order", "The date when the order was placed"),
		new ColumnMetadata("Status", "Order", "The current status of the order (e.g., pending, shipped, completed, cancelled)"),
		new ColumnMetadata("ProductName", "Product", "The name or title of the product"),
		new ColumnMetadata("Price", "Product", "The price of a single unit of the product"),
		new ColumnMetadata("StockQuantity", "Product", "The current number of product units available in inventory"),
		new ColumnMetadata("Quantity", "OrderItem", "The number of units of the product included in the order"),
		new ColumnMetadata("UnitPrice", "OrderItem", "The price per unit of the product at the time of the order"),
		new ColumnMetadata("InvoiceNumber", "Invoice", "A unique identifier for the invoice"),
		new ColumnMetadata("InvoiceDate", "Invoice", "The date when the invoice was created or issued"),
		new ColumnMetadata("PaymentDate", "Payment", "The date when the payment was made"),
		new ColumnMetadata("Amount", "Payment", "The total monetary amount of the payment"),
		new ColumnMetadata("PaymentMethod", "Payment", "The method used to make the payment (e.g., credit card, PayPal, bank transfer)"),
	];
}