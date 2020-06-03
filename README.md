# dotnet-csproj
This tool allows you to set and get values from elements within an csproj file.

## Installation
```
dotnet tool install csproj
```

## Usage

### Getting values
Retreiving values is as simple as running the following line. This will return the value of the element _Author_. If the element does not exist than the tool will exit with error code 1.

```
dotnet csproj --get Author
```

### Setting values
Setting values is similar as getting them. If the element does not exist it will be added to the csproj file.

```
dotnet csproj --set Author="Ruben Labruyere"
```

## Constribution
Feel free to open up an issue or pull request if you got a request.

## License
[MIT](LICENSE)