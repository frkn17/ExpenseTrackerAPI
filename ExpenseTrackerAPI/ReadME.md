# Expense Tracker API

This is a secure and RESTful Expense Tracker API built with **ASP.NET CORE** and **Entity Framework Core**. It allows users to manage their expenses, providing features such as user authentication, expense tracking.

## 🚀 Features

	- JWT Authentication (Login and Register)
	- Create, Read, Update, delete expenses
	- Filter expenses by date range
	- Expense statistics
	- Admin endpoint to manage users and view their expenses

## 🛠️ Tech Stack
	- ASP.NET Core Web API
	- Entity Framework Core
	- SQL Server
	- JWT for authentication

## 📂 Project Structure
	- **Controllers**: Contains API controllers for handling requests.
	- **Models**: Contains data models and DTOs.
	- **Data**: Contains the database context and migrations.
	- **Services**: Contains business logic and service classes.

## 🔧 Setup Instructions
	1. Clone the repository:
		``` bash
		git clone git clone https://github.com/frkn17/ExpenseTrackerAPI.git
		cd ExpenseTrackerAPI

	2. Create a appsettings.json
		{
		"ConnectionStrings": {
			"DefaultConnection": "your-sql-connection-string"
			},
		"Jwt": {
			"Key": "your-secret-key",
			"Issuer": "your-app",
			"Audience": "your-app-users"
			}
		}

	3. Run migrations to set up the database:
		```bash
		dotnet ef migrations add InitialCreate
		dotnet ef database update
		```

	4. Run the application:
		```bash
		dotnet run
		```
	5. Use a tool like  Swagger to test the API endpoints.

	6. License
	- This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
