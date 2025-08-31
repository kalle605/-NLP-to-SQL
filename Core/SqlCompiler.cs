using System.Text;

namespace SqlWriter.Core;

public record TableRelation(string KeyColumn, string ForeignColumn = "Id");

public class SqlCompiler
{
	private readonly SqlSchemaResolver resolver = new();

	public string Compile(params string[] columnNames)
	{
		var tables = this.resolver.GetTables(columnNames);

		if (tables.Count == 0)
		{
			throw new ArgumentException("No tables resolved from columns.");
		}

		var root = tables.First();
		var edges = new HashSet<(string From, string To, TableRelation Rel)>();

		foreach (var table in tables.Where(t => t != root))
		{
			var path = this.resolver.FindPath(root, table);
			foreach (var edge in path)
			{
				edges.Add(edge);
			}
		}

		var fromClauseBuilder = new StringBuilder();
		fromClauseBuilder.AppendLine(root);

		foreach (var (from, to, rel) in edges)
		{
			fromClauseBuilder.AppendLine(
				$"LEFT OUTER JOIN {to} on {to}.{rel.ForeignColumn} = {from}.{rel.KeyColumn}"
			);
		}

		return $"""
		        SELECT {string.Join(", ", columnNames)}
		        FROM {fromClauseBuilder}
		        """.Trim();
	}
}