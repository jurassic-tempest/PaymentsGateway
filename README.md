# Payment Gateway Api
This is a Web API project that acts as a payments gateway between a merchant and a bank.

## How to run
To start the bank simulator, run `docker-compose up` in the root directory of the project.

Start the PaymentGatewayApi project in Visual Studio (or use the command `dotnet run` in the PaymentGatewayApi directory from the command line).`)

## Key design decisions and considerations
- Extensibility of the code
- Maintainability of the code and how easy it is to understand
- Security

## Assumptions I have made
- We don't want to return the validation errors to the client for security reasons (otherwise I was considering using a custom exception filter to catch the validation errors and return them to the client in a more user-friendly way).
- Reject payment requests with an amount of 0.
- We want to store both the declined and successful transactions in the database.
- We don't need to store the authorization code returned by the bank anywhere.
- The bank simulator should be running while the PaymentGatewayApi is running.
- Authorization of the requests to this API is not required in this iteration but will need to be implemented before deployment.

## Potential improvements
- Develop integration tests further so that they can running them spins up the docker container for the bank simulator.
- Add more logging for observability: I was weery of adding logging that exposes sensitive information
- Deployments: to a cloud provider like AWS or Azure in containers

