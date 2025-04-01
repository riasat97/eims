# Electronic Inventory Management System (EIMS)

A full-stack application for managing electronic components inventory and projects.

## Project Structure

- `EIMS.Server`: .NET Core Web API backend
- `EIMS.Client`: Blazor WebAssembly frontend
- `EIMS.Shared`: Shared class library for common models and interfaces

## Prerequisites

- .NET 8.0 SDK
- Node.js and npm

## Setup Instructions

1. Clone the repository
2. Install Node.js dependencies for the client project:
   ```bash
   cd EIMS.Client
   npm install
   ```

3. Start the Tailwind CSS build process:
   ```bash
   npm run css:build
   ```

4. In a new terminal, run the backend API:
   ```bash
   cd EIMS.Server
   dotnet run
   ```

5. In another terminal, run the Blazor client:
   ```bash
   cd EIMS.Client
   dotnet run
   ```

## Features

- Parts management
- Project/BOM management
- Inventory tracking
- Barcode scanning support
- QR code support for item tracking
- Price calculation with support for:
  - Price breaks
  - Currency conversions
  - MOQ (Minimum Ordering Quantities)
  - Order multiples

## Development

The project uses:
- Tailwind CSS for styling
- DaisyUI for UI components
- Material Icons for icons
- Blazor WebAssembly for the frontend
- .NET Core Web API for the backend 