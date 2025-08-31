using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;

namespace SqlWriter.Core.AI.NLP2SQL;

public class SqlTransformer
{
	private readonly SqlCompiler compiler = new();
	
	public async Task<string> Transform(string text)
	{
		var kernel = await KernelProvider.GetKernelAsync();
		var store = kernel.Services.GetRequiredService<InMemoryVectorStore>();
		var collection = store.GetCollection<string, ColumnDecoration.ColumnMetadata>("ColumnCollection");
		var generator = kernel.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
		var embeddedQuestion = await generator.GenerateAsync(text);
		
		var columns = await collection
			.SearchAsync(embeddedQuestion
				, 5
				, new VectorSearchOptions<ColumnDecoration.ColumnMetadata>
				{
					VectorProperty = metadata => metadata.DescriptionEmbedding,
				}
			)
			.ToListAsync();

		var sql = this.compiler.Compile(columns.Select(x => x.Record.ColumnName).ToArray());

		var chatCompletion = kernel.GetRequiredService<IChatClient>();
		// 

		var initialMessage = new ChatMessage(ChatRole.System
			, "You are a database engineer tasked to refine SQL statements for the database 'SQL Anywhere'"
		);
		
		var userMessage = new ChatMessage(ChatRole.User,
			$"""
			 I have made a SQL and I would like you to refine it this is what I have
			 {sql}

			 Can you rewrite it so it solves the problem: '{text}'
			 """
		);

		var columnInformation = new ChatMessage(ChatRole.System,
			$"""
			 Here is information about the columns included in the SQL:
			 {string.Join(Environment.NewLine, columns.Select(x => x.Record.ColumnName + ":" + x.Record.Description))}

			 Now solve the problem by providing just the unwrapped SQL statement.
			 """
		);
		var result = await chatCompletion.GetResponseAsync([initialMessage, userMessage, columnInformation]);

		return result.Text;
	}
}