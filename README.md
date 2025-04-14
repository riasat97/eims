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

## Implemented Features

- **Parts Management**
  - Add, edit, and view component details
  - Tag components with categories
  - Add technical specifications

- **Inventory Tracking**
  - Location management with storage hierarchies
  - Batch location generation for warehouse organization
  - Track quantity on hand
  - Reserve quantities for projects

- **Project/BOM Management**
  - Create and manage bills of materials
  - Associate parts with projects
  - Track project progress

- **QR and Barcode Support (Upcomming)**
  - Generate QR codes for locations and items
  - Barcode scanning for quick lookups

- **UI Features**
  - Responsive dark-themed interface
  - Mobile-friendly design
  - Interactive data tables with sorting and filtering

## Upcoming Features

The following features are planned but not yet implemented:

### Short-term Roadmap (Next 3 months)

- **Advanced Search and Filtering**
  - Full-text search across all inventory
  - Advanced filtering options by multiple parameters
  - Saved searches and filter presets

- **Supplier Integration**
  - Direct API connections to major component suppliers
  - Automatic price and availability updates
  - Order placement through the system

- **Import/Export Functionality**
  - Excel/CSV import and export
  - BOM import from Eagle/KiCad/Altium
  - Data migration tools

- **Inventory Reports**
  - Customizable inventory reports
  - Stock level alerts and notifications
  - Consumption trends and forecasting

### Long-term Roadmap (4-12 months)

- **Multi-tenant Support**
  - User roles and permissions
  - Team collaboration features
  - Access control for different inventory areas

- **Manufacturing Integration**
  - PCB assembly tracking
  - Component placement visualization
  - Integration with manufacturing equipment

- **API Expansion**
  - Public API documentation
  - Webhook support for inventory events
  - Custom integration capabilities

## Technical Implementation Details

- **Frontend**: Blazor WebAssembly with:
  - Tailwind CSS for styling
  - DaisyUI for UI components
  - Material Icons for iconography

- **Backend**: .NET Core Web API with:
  - Entity Framework Core for data access
  - SQL Server database
  - Identity for authentication and authorization

- **CI/CD**:
  - GitHub Actions for continuous integration
  - Automated testing on pull requests
  - Docker containerization for deployment

## Contributing

We welcome contributions to EIMS! Please check our contribution guidelines before submitting pull requests.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

For questions or support, please open an issue in the GitHub repository or contact the maintainers directly. 