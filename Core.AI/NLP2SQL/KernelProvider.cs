using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using SqlWriter.Core.AI.Kernel;

namespace SqlWriter.Core.AI.NLP2SQL;

public static class KernelProvider
{
	private static Microsoft.SemanticKernel.Kernel? kernel;

	public static async Task<Microsoft.SemanticKernel.Kernel> GetKernelAsync()
	{
		if (kernel is not null) return kernel;
		kernel = KernelBuilder.GetKernel();
		var store = kernel.Services.GetRequiredService<InMemoryVectorStore>();
		var collection = store.GetCollection<string, ColumnDecoration.ColumnMetadata>("ColumnCollection");
		await collection.EnsureCollectionExistsAsync();
		var generator = kernel.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
		foreach (var metadata in ColumnDecoration.Metadata)
		{
			var embedding = await generator.GenerateAsync(metadata.Description);

			metadata.DescriptionEmbedding = embedding.Vector;
			await collection.UpsertAsync(metadata);
		}
		
		return kernel;
	}
}