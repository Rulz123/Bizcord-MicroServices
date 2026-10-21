## Entity framework

We have chosen to use Entity Framework because it provides automation through the Code First approach, meaning that we do not have to create the database manually. It is also a tool we are relatively familiar with, and it significantly reduces the amount of data-handling code we need to write, as it works seamlessly with LINQ.

Entity Framework also has extensive documentation, making it easy to implement for both new and experienced developers.

This choice was made because the database is relatively simple, making it easy to manage migration versioning and scale the solution as needed.

Since we use EF (Entity Framework), we do not need to write the database queries ourselves, as the DbContext and EF handle this for us. It also works seamlessly with asynchronous operations, making it quick and easy to extend the functionality and integrate with other services.

Entity Framework also works well with the LINQ approach and returns database query results as collections.

When a fresh instance of the microservice is created, a migration must be generated, followed by a database update, so that the database is created through Entity Framework.

This is done using the following commands:

```sh 
dotnet ef migrations add DatabaseV_0
dotnet ef database update
```