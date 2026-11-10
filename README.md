# GaaS Station

GaaS Station is an ASP.NET Core MVC web application that helps users find nearby fuel stations and compare fuel prices.

The application retrieves station locations and fuel pricing information through external APIs, allowing users to search and filter results based on location, station brand, and fuel type.

## Technologies

- ASP.NET Core MVC (.NET 10)
- C#
- Entity Framework Core 10
- Microsoft SQL Server
- ASP.NET Core Identity
- Razor Views
- HTTP Client

## Features

- Search for fuel stations by location.
- Find nearby stations using geolocation.
- Filter stations by brand.
- Filter results by fuel type.
- Compare fuel prices across stations.
- Sort results by lowest available price.
- User authentication and account management.

## Requirements

- .NET 10 SDK
- Microsoft SQL Server
- GaaS-API
- Geolocation API

## API Integration

GaaS Station communicates with two backend services:

- **GaaS-API:** Retrieves fuel pricing information.
- **Geolocation API:** Retrieves fuel station locations based on geographic coordinates.

These services allow the MVC application to combine station information with fuel pricing data and present the results to users.

## License

This project is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.
