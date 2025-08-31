using SqlWriter.Core.AI.NLP2SQL;

namespace SqlWriter.Tests;

public class NLP2SQLTests
{
	private readonly SqlTransformer sut = new();
	
	[Theory]
	[InlineData("What is the order quantity of order '25121'"
		, """
		       SELECT Quantity
		       FROM Order
		       LEFT OUTER JOIN OrderItem on OrderItem.OrderId = Order.Id
		       WHERE OrderNumber = '25121'
		       """
	)]
	public async Task Should_Transform_Text_To_SQL(string question, string expected)
	{
		var result = await this.sut.Transform(question);

		Assert.Equal(expected, result);
	}
}