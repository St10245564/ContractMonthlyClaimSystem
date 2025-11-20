Contract Monthly Claim System (CMCS)
📋 Project Overview
The Contract Monthly Claim System is a comprehensive ASP.NET Core web application designed for educational institutions to manage monthly claims submitted by contract lecturers. The system streamlines the claim submission, review, approval, and reporting processes with robust role-based access control.

🎯 Features
🔐 Authentication & Authorization
Role-based access control with three user types:

Lecturers: Submit and manage their claims

Coordinators: Review and approve pending claims

Managers: Full system access with audit capabilities

Secure session-based authentication

📝 Claims Management
Claim Submission with automatic total calculation

Supporting document uploads (PDF, DOCX, XLSX, JPG, PNG)

Real-time claim tracking (Pending, Approved, Rejected, Revision Requested)

Edit/Delete functionality for pending claims

📊 Reporting & Analytics
Comprehensive Reports with date range and status filters

Multiple Export Formats (CSV, PDF)

Dashboard Analytics with visual charts

Financial summaries and performance metrics

🔍 Audit & Security
Complete Audit Trail tracking all system activities

IP Address Tracking for security monitoring

Role-based Permissions ensuring data security

🛠️ Technology Stack
Backend
ASP.NET Core 6.0 - Web framework

Entity Framework Core - ORM and data access

SQL Server - Database management

Frontend
HTML5 & CSS3 - Page structure and styling

Bootstrap 5.3 - Responsive UI framework

JavaScript/jQuery - Client-side interactivity

Chart.js - Data visualization

🗄️ Database Schema
Core Entities
Users - System users (Lecturers, Coordinators, Managers)

Modules - Course modules with hourly rates

Claims - Monthly claim submissions

SupportingDocuments - File attachments

AuditLogs - System activity tracking

🚀 Installation & Setup
Prerequisites
.NET 6.0 SDK

SQL Server (LocalDB or Express)

Visual Studio 2022 or VS Code

Quick Start
Clone the repository

Update connection string in appsettings.json

Run database migrations:

bash
dotnet ef database update
Run the application:

bash
dotnet run
👥 Default User Accounts
Role	Username	Password	Access
Lecturer	lecturer1	password123	Submit & view own claims
Coordinator	coordinator1	password123	Review & approve claims
Manager	manager1	password123	Full system access + reports
💡 Usage Guide
For Lecturers
Login with lecturer credentials

Submit New Claim: Enter module details, hours worked, and upload documents

Track Claims: View status of submitted claims

For Coordinators
Login with coordinator credentials

Review Pending Claims from dashboard

Approve/Reject/Request Revisions with notes

For Managers
Login with manager credentials

Generate Reports with custom filters

View Audit Logs for system monitoring

Export Data in CSV/PDF formats

📊 Key Functionalities
Automated Calculations: Hours × Rate = Total Amount

File Management: Secure document upload and download

Real-time Updates: Instant status changes and calculations

Advanced Filtering: Date ranges and status-based filtering

Comprehensive Reporting: Multiple report types with exports

🔒 Security Features
Role-based access control

Session management

Input validation and sanitization

Complete audit trail

File type and size validation

📁 Project Structure
text
Controllers/
├── AccountController.cs      # Authentication
├── ClaimsController.cs       # Claims management
├── DashboardController.cs    # Analytics
└── ReportsController.cs      # Reporting

Models/
├── User.cs, Claim.cs, Module.cs
├── SupportingDocument.cs, AuditLog.cs
└── ViewModels/              # Data transfer objects

Views/
├── Account/                 # Login & registration
├── Claims/                  # Claim management
├── Dashboard/               # Analytics
└── Reports/                 # Reporting interface
🐛 Troubleshooting
Common Issues
Database Connection: Verify SQL Server is running

File Uploads: Check wwwroot/uploads/documents directory exists

Session Timeouts: Default 30-minute timeout

Error Solutions
"Access Denied": User lacks required permissions

"Invalid Login": Verify username/password

"File Too Large": Uploads limited to 10MB

🔮 Future Enhancements
Email notifications

Bulk claim processing

Advanced analytics

Mobile-responsive improvements

Multi-language support

Developed with ASP.NET Core 6.0
Version: 1.0
Last Updated: December 2024
