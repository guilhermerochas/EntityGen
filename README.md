# EntityGen
EntityGenerator for creating  C# classes based on SQL Server Tables

## Usage 

To see the usage use the following command:
```bash
  .\EntityGen.exe --help
```

it will show all the possible options

## Connection File

To connect to the database, you will need a json connection file with the following body:
```json
{
  "database": "MyDatabase",
  "user_id": "user",
  "password": "1234",
  "server": "127.0.0.1,1499"
}
```

an example can be found on the project folder. if a path wasn't passed as an argument it will try to find a connection file at <br>
the current directory with the name `connection.json`.
