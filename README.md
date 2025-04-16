# Nextech Coding Challenge
 
<details>
<summary>Hacker News API - Backend Challenge</summary>
===================================

Overview
--------

This project is a .NET 8.0 web API that provides an interface to fetch the newest stories from Hacker News. It acts as a wrapper around the official Hacker News API, providing pagination capabilities and caching to improve performance.

Features
--------

-   Fetch the newest stories from Hacker News with pagination support
-   Memory caching of story IDs to reduce API calls
-   Docker support for containerized deployment
-   Swagger UI for API documentation and testing
-   Comprehensive unit tests

Prerequisites
-------------

-   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Docker](https://www.docker.com/products/docker-desktop/) (optional, for containerized deployment)
-   Visual Studio 2022 or Visual Studio Code (optional, for development)

Getting Started
---------------

### Clone the Repository

```
git clone https://your-repository-url/Backend-Challenge.git
cd Backend-Challenge

```

### Build and Run Locally

#### Using .NET CLI

```
# Navigate to the project directory
cd "Backend Challenge"

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

```

The API will be available at `http://localhost:5037` by default.

#### Using Visual Studio

1.  Open the solution file `Backend Challenge.sln` in Visual Studio
2.  Press F5 or click the "Start" button to build and run the project


API Documentation
-----------------

Once the application is running, you can access the Swagger UI at:

-   Local Development: `http://localhost:5037/swagger`

### Available Endpoints

#### GET /api/Stories

Fetches a paginated list of newest stories from Hacker News.

**Parameters:**

-   `amount` (int): Number of stories per page
-   `page` (int): Page number (starting from 1)

**Example:**

```
GET /api/Stories?amount=10&page=1

```

Testing
-------

### Running Unit Tests

```
# Navigate to the project directory
cd "Backend Challenge"

# Run the tests
dotnet test

```

### Manual Testing

You can manually test the API using:

1.  **Swagger UI**: Navigate to the Swagger UI endpoint after starting the application
2.  **Curl**:

    ```
    curl -X GET "http://localhost:5037/api/Stories?amount=10&page=1" -H "accept: application/json"

    ```

3.  **Postman or similar API testing tool**

Implementation Details
----------------------

-   The application uses RestSharp to communicate with the Hacker News API
-   Stories are cached for 5 minutes to reduce the load on the Hacker News API
-   Error handling is implemented at multiple levels to ensure a robust application

Troubleshooting
---------------

If you encounter any issues:

1.  Ensure the Hacker News API is accessible (https://hacker-news.firebaseio.com/)
2.  Check application logs for detailed error information
3.  Verify your network connection if running in Docker


</details>
<details>
<summary>Hacker News API - Frontend Challenge</summary>
This is how you dropdown.
</details>