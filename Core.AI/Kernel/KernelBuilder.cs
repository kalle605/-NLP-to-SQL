using Microsoft.SemanticKernel;

namespace SqlWriter.Core.AI.Kernel;

public class KernelBuilder
{
	public static Microsoft.SemanticKernel.Kernel GetKernel()
	{
		var builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();
		var endpoint = new Uri("http://localhost:11434");
		builder.AddOllamaEmbeddingGenerator("bge-m3", endpoint);
		builder.AddOllamaChatClient("gpt-oss:20b", endpoint);
		builder.Services.AddInMemoryVectorStore();
		
		return builder.Build();
	}
}