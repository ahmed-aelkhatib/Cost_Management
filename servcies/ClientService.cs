using Microsoft.AspNetCore.Mvc;

namespace ERPtask.servcies
{
    using System.Data.SqlClient;
    using System.Collections.Generic;
    using ERPtask.models;

    public class ClientService
    {
        private readonly string _connectionString;

        public ClientService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Client AddClient(string clientName, string contactDetails, string region)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO Clients (ClientName, ContactDetails, Region) OUTPUT INSERTED.ClientID VALUES (@ClientName, @ContactDetails, @Region)",
                    connection);
                command.Parameters.AddWithValue("@ClientName", clientName);
                command.Parameters.AddWithValue("@ContactDetails", contactDetails ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Region", region ?? (object)DBNull.Value);
                int clientId = (int)command.ExecuteScalar();
                return new Client { ClientID = clientId, ClientName = clientName, ContactDetails = contactDetails, Region = region };
            }
        }

        public bool UpdateClient(int clientId, string clientName, string contactDetails)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "UPDATE Clients SET ClientName = @ClientName, ContactDetails = @ContactDetails WHERE ClientID = @ClientID",
                    connection);
                command.Parameters.AddWithValue("@ClientID", clientId);
                command.Parameters.AddWithValue("@ClientName", clientName);
                command.Parameters.AddWithValue("@ContactDetails", contactDetails ?? (object)DBNull.Value);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }


        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT ClientID, ClientName, ContactDetails, Region FROM Clients", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            ClientID = reader.GetInt32(0),
                            ClientName = reader.GetString(1),
                            ContactDetails = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Region = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }
            return clients;
        }
        public Client GetClientById(int clientId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT ClientID, ClientName, ContactDetails, Region FROM Clients WHERE ClientID = @ClientID", connection);
                command.Parameters.AddWithValue("@ClientID", clientId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Client
                        {
                            ClientID = reader.GetInt32(0),
                            ClientName = reader.GetString(1),
                            ContactDetails = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Region = reader.IsDBNull(3) ? null : reader.GetString(3)
                        };
                    }
                    return null;
                }
            }
        }
    }
}
