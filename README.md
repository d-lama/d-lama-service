# D-LAMA Service


The *D-LAMA Service* is an independent API for other apps of the D-LAMA team. It uses the .NET Core 6.0.14 (LTS) framework, which allows deploying it on any platform. 

## Development
### Project setup
- **d-lama-service**: Domain logic
- **Data**: Database entity & Migrations
- **Test (to come)**: Unit test of domain logic

### Coding guidelines
- Use existing C# naming conventions
- General order:
    - 1. fields
    - 2. constructor
    - 3. public methods
    - 4. private methods
- Entity order:
    - 1. fields
    - 2. constructor
    - 3. navigation
    - 4. public methods
    - 5. private methods

### Database management
#### Initial setup
- Install SQL Express on your development machine

#### Deploy new database updates

- Open Package Manager Console in *Data* Project
- `Add-Migration $MigrationName` to create database changing code (e.g Add-Migration AddUser)
- When starting the application, it will then automatically deploy the migrations to your selected Database.

#### Seed your database with test data
Currently there is no automatic data creation method available (will come...).
In order to work with data now, just seed your db with custom data using MSSQL Studio.

### Branching workflow
Newly we directly merge changes from a **feature branch** into the **master** branch in order to be as fast as possible in development and receiving feedback from customers and other teams. 


### Developers
- [Stefanie Sigrist](https://github.com/sigrist3)
- [Gianmarco Güntert](https://github.com/guentgia)
- [Joel Grand](https://github.com/joelgrand)
