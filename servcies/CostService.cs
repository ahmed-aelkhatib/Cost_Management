using ERPtask.models;
using System.Data.SqlClient;
namespace ERPtask.servcies
{
    public class CostService
    {
        private readonly string _connectionString;

        public CostService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CostEntry AddCostEntry(string category, decimal amount, DateTime date, string description)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO Costs (Category, Amount, Date, Description) OUTPUT INSERTED.CostID VALUES (@Category, @Amount, @Date, @Description)",
                    connection);
                command.Parameters.AddWithValue("@Category", category);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@Date", date);
                command.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                int costId = (int)command.ExecuteScalar();
                return new CostEntry { CostID = costId, Category = category, Amount = amount, Date = date, Description = description };
            }
        }

        public List<CostEntry> GetAllCosts()
        {
            var costs = new List<CostEntry>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT CostID, Category, Amount, Date, Description FROM Costs", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        costs.Add(new CostEntry
                        {
                            CostID = reader.GetInt32(0),
                            Category = reader.GetString(1),
                            Amount = reader.GetDecimal(2),
                            Date = reader.GetDateTime(3),
                            Description = reader.IsDBNull(4) ? null : reader.GetString(4)
                        });
                    }
                }
            }
            return costs;
        }

        public CostEntry GetCostById(int costId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT CostID, Category, Amount, Date, Description FROM Costs WHERE CostID = @CostID", connection);
                command.Parameters.AddWithValue("@CostID", costId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new CostEntry
                        {
                            CostID = reader.GetInt32(0),
                            Category = reader.GetString(1),
                            Amount = reader.GetDecimal(2),
                            Date = reader.GetDateTime(3),
                            Description = reader.IsDBNull(4) ? null : reader.GetString(4)
                        };
                    }
                    return null;
                }
            }
        }
    }
}
