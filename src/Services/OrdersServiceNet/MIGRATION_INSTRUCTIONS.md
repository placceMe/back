# To create and apply migrations, run these commands in the OrdersServiceNet directory:

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# For Docker environment, the migrations will be automatically applied via MigrationExtensions.ApplyMigrations<OrdersDbContext>() in Program.cs