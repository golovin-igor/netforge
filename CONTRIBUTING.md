# Contributing to NetSim

Thank you for your interest in contributing to NetSim! This guide explains how to set up your development environment, the preferred coding style, how to run the test suite, and the pull request process.

## Development Setup

1. Install the latest [.NET SDK](https://dotnet.microsoft.com/en-us/download) (version 9.0 or later).
2. Clone the repository:
   ```bash
   git clone https://github.com/your-org/NetSim.git
   cd NetSim
   ```
3. Restore dependencies and build the solution:
   ```bash
   dotnet build NetSim.sln
   ```
4. Optional: run the debug console during development:
   ```bash
   dotnet run --project DebugConsole
   ```

## Coding Style

- Follow standard C# coding conventions.
- Keep code self-documenting and use meaningful names.
- Include XML documentation comments for public APIs when practical.
- Format code using the built-in `dotnet format` or your IDE's formatter.

## Testing

- Add or update unit tests for all changes.
- Run the full test suite before submitting:
  ```bash
  dotnet test
  ```
- Ensure new tests cover edge cases and error conditions.

## Pull Request Process

1. Create a descriptive branch name for your work.
2. Make focused commits with clear messages.
3. Verify all tests pass and the code builds.
4. Submit a pull request targeting the `main` branch and provide a concise summary of your changes.
5. Address review feedback promptly and keep the branch up to date with `main` as needed.

We appreciate your contributions and look forward to collaborating!
