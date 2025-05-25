# ERPtask - Enterprise Resource Planning API

## Overview
ERPtask is a .NET Core-based API for managing invoices, clients, costs, and notifications in an enterprise resource planning system. Built with ASP.NET Core, it provides RESTful endpoints to handle invoicing, client management, cost tracking, and notification services.

## Features
- **Invoice Management**: Create, edit, retrieve, and update invoices, including discounts and due dates.
- **Client Management**: Add, update, and retrieve client information, including region-specific details.
- **Cost Tracking**: Record and retrieve cost entries with categories, amounts, and descriptions.
- **Notification System**: Send reminders for invoice due dates and retrieve notification details.

## Project Structure
The project is organized into controllers, services, and models under the `ERPtask` namespace:
- **Controllers**:
  - `InvoiceController.cs`: Handles invoice creation, editing, and retrieval.
  - `ClientController.cs`: Manages client data, including adding and updating clients with region support.
  - `CostController.cs`: Manages cost entries for financial tracking.
  - `ReminderController.cs`: Handles sending and retrieving notifications for invoice due dates.
- **Services**: Contains business logic (e.g., `InvoiceService`, `ClientService`, `CostService`, `NotificationService`).
- **Models**: Defines data structures like `InvoiceItem`, `Client`, `CostEntry`, and request DTOs.

## API Endpoints
### InvoiceController
- **POST** `/api/Invoice`: Create a new invoice.
- **PUT** `/api/Invoice/{id}`: Edit an existing invoice's items and discount.
- **PUT** `/api/Invoice/editInvoice/{id}`: Update an invoice's discount.
- **GET** `/api/Invoice/allInvoices`: Retrieve all invoices.
- **GET** `/api/Invoice/{id}`: Retrieve an invoice by ID.
- **PUT** `/api/Invoice/updateDueDate/{id}`: Update an invoice's due date.

### ClientController
- **POST** `/api/Client`: Add a new client.
- **PUT** `/api/Client/editClient/{id}`: Update client details.
- **GET** `/api/Client`: Retrieve all clients.
- **GET** `/api/Client/{id}`: Retrieve a client by ID.

### CostController
- **POST** `/api/Cost`: Add a new cost entry.
- **GET** `/api/Cost`: Retrieve all cost entries.
- **GET** `/api/Cost/{id}`: Retrieve a cost entry by ID.

### ReminderController
- **POST** `/api/Reminder`: Send a due date reminder (default: email).
- **GET** `/api/Reminder`: Retrieve all notifications.
- **GET** `/api/Reminder/{id}`: Retrieve a notification by ID.

## Prerequisites
- .NET Core SDK (version 6.0 or later)
- A compatible IDE (e.g., Visual Studio, VS Code)
- A database (configured via services, not shown in the provided code)
- Dependency injection setup for services (`InvoiceService`, `ClientService`, `CostService`, `NotificationService`)

## Setup Instructions
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/your-username/ERPtask.git
   cd ERPtask
   ```

2. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure Services**:
   - Ensure the database connection is configured (e.g., in `appsettings.json`).
   - Register services in `Startup.cs` or `Program.cs`:
     ```csharp
     services.AddScoped<InvoiceService>();
     services.AddScoped<ClientService>();
     services.AddScoped<CostService>();
     services.AddScoped<NotificationService>();
     ```

4. **Run the Application**:
   ```bash
   dotnet run
   ```
   The API will be available at `https://localhost:5001` (or the configured port).

5. **Test the API**:
   Use tools like Postman or cURL to test endpoints. Example:
   ```bash
   curl -X POST https://localhost:5001/api/Invoice -H "Content-Type: application/json" -d '{"ClientID": 1, "Items": [{"Description": "Item1", "Quantity": 2, "UnitPrice": 10}], "Discount": 5}'
   ```

## Dependencies
- **Microsoft.AspNetCore.Mvc**: For building RESTful APIs.
- **.NET Core**: Framework for the application.
- (Additional dependencies may be required based on service implementations.)

## Contribution Guidelines
1. Fork the repository and create a feature branch (`git checkout -b feature/your-feature`).
2. Follow C# coding standards and add unit tests for new functionality.
3. Submit a pull request with a clear description of changes.

## License
This project is licensed under the MIT License.

## Contact
For issues or questions, open an issue on GitHub or contact the repository maintainer.
