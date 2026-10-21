## Database

We have chosen to use the latest version of the MSSQL database and run it through Docker, as it is a technology we are all familiar with and is well supported by documentation.

It is also a database that is well supported by Microsoft, and we can easily pull images directly from them.

However, we use the latest database image in our Docker Compose configuration to ensure that we get all the newest features available for our database, allowing us to stay up to date with the latest developments in database technology.

This makes it easy to scale horizontally along the x-axis and allows us to quickly set up the same database on other computers and servers.