namespace SqlWriter.Core;

public class SqlSchemaResolver
{
    private readonly Dictionary<string, string> tableLookup = new()
    {
       // User related
       { "Name", "User" },
       { "Password", "User" },
       { "Email", "User" },
       { "Address", "User" },

       // Order related
       { "OrderNumber", "Order" },
       { "OrderDate", "Order" },
       { "Status", "Order" },

       // Product related
       { "ProductName", "Product" },
       { "Price", "Product" },
       { "StockQuantity", "Product" },

       // OrderItem (bridge table between Order and Product)
       { "Quantity", "OrderItem" },
       { "UnitPrice", "OrderItem" },

       // Invoice related
       { "InvoiceNumber", "Invoice" },
       { "InvoiceDate", "Invoice" },

       // Payment related
       { "PaymentDate", "Payment" },
       { "Amount", "Payment" },
       { "PaymentMethod", "Payment" }
    };

    // Relations are *directed* here, but we'll make them bidirectional in the graph
    private readonly Dictionary<string, Dictionary<string, TableRelation>> relationLookup = new()
    {
       {
          "User", new Dictionary<string, TableRelation>
          {
             { "Order", new TableRelation("Id", "UserId") },
          }
       },
       {
          "Order", new Dictionary<string, TableRelation>
          {
             { "User", new TableRelation("UserId") },
             { "OrderItem", new TableRelation("Id", "OrderId") },
             { "Invoice", new TableRelation("Id", "OrderId") },
          }
       },
       {
          "OrderItem", new Dictionary<string, TableRelation>
          {
             { "Order", new TableRelation("OrderId") },
             { "Product", new TableRelation("ProductId") },
          }
       },
       {
          "Product", new Dictionary<string, TableRelation>
          {
             { "OrderItem", new TableRelation("Id", "ProductId") },
          }
       },
       {
          "Invoice", new Dictionary<string, TableRelation>
          {
             { "Order", new TableRelation("OrderId") },
             { "Payment", new TableRelation("Id", "InvoiceId") },
          }
       },
       {
          "Payment", new Dictionary<string, TableRelation>
          {
             { "Invoice", new TableRelation("InvoiceId") },
          }
       },
    };

    public HashSet<string> GetTables(IEnumerable<string> columnNames)
    {
        return columnNames
            .Select(x => this.tableLookup[x])
            .ToHashSet();
    }

    /// <summary>
    /// Build adjacency list for BFS
    /// </summary>
    private Dictionary<string, List<(string, TableRelation)>> BuildGraph()
    {
        var graph = new Dictionary<string, List<(string, TableRelation)>>();

        foreach (var (table, neighbors) in this.relationLookup)
        {
            if (!graph.ContainsKey(table))
            {
	            graph[table] = [];
            }

            foreach (var (neighbor, relation) in neighbors)
            {
                graph[table].Add((neighbor, relation));

                if (!graph.TryGetValue(neighbor, out var value))
                {
                    value = [];
                    graph[neighbor] = value;
                }

                if (value.Any(x => x.Item1 == table))
                {
	                continue;
                }

                var reverseRelation = new TableRelation(relation.ForeignColumn, relation.KeyColumn);
                value.Add((table, reverseRelation));
            }
        }

        return graph;
    }

    /// <summary>
    /// Find shortest path of relations between two tables
    /// </summary>
    public List<(string From, string To, TableRelation Rel)> FindPath(string start, string target)
    {
        var graph = this.BuildGraph();
        var queue = new Queue<string>();
        var visited = new HashSet<string>();
        var parent = new Dictionary<string, (string From, TableRelation Rel)>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target)
            {
	            break;
            }

            foreach (var (neighbor, relation) in graph[current])
            {
                if (!visited.Add(neighbor))
                {
	                continue;
                }

                parent[neighbor] = (current, relation);
                queue.Enqueue(neighbor);
            }
        }

        // reconstruct path
        var path = new List<(string From, string To, TableRelation Rel)>();
        var node = target;
        while (parent.ContainsKey(node))
        {
            var (from, rel) = parent[node];
            path.Add((from, node, rel));
            node = from;
        }

        path.Reverse(); // BFS gave us backtrack order
        return path;
    }
}