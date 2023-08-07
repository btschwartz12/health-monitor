# ISHealthMonitor App Documentation

## Table of Contents

1. [Overview](#overview)
2. [Project Structure](#project-structure)
   - [ISHealthMonitor.UI](#ishealthmonitorui)
   - [ISHealthMonitor.Core](#ishealthmonitorcore)
3. [Site Pages](#site-pages)
   - [Public Pages](#public-pages)
   - [Admin Pages](#admin-pages)
4. [Admin Functions](#admin-functions)
5. [Models](#models)
6. [Testing](#testing)
7. [Build and Deployment](#build-and-deployment)
8. [Contact](#contact)

## Overview

The ISHealthMonitor app serves as an SSL certificate management system for Hyland's internal and external site. It enables users to keep track of the SSL certificate expiration date of the respective sites, set up reminders at their convenience, and enables them to view the status of all site certificates and directly create work orders.


## Project Structure

The application is organized into two main projects: ISHealthMonitor.UI and ISHealthMonitor.Core.

### ISHealthMonitor.UI

This acts as the User Interface (UI) of the application and follows the Model-View-Controller design pattern. This is the driver of the solution, as it contains `Program.cs` and `Startup.cs`, and is responsible for the GUI.

Structure of the directories are:

- `Controllers/` - Contains all the View controllers and API controllers used by the UI.
    - `API/` - Contains all the internal API endpoints used by the app. This includes all the CRUD operations for the models, all of the endpoints that supply data for the DataTables, and more. The 'API' internal endpoints require a SSO cookie.
    - `Rest/` - Contains the configuration for the JSON Web Token (JWT) system & the external endpoints that are called by daily-running processes. The 'Rest' endpoints require JWT authentication.
- `Views/` - It's divided into two major sections:
    - `Admin/` - Contains all the views that are available to only admins. This includes the CRUD tables for each model, the log viewer, and a couple of partial views.
    - `Home/` - Contains all the public views, including the configuration builder, the history page, the cert status page, and the work order builder.

- `Models/` - There is actually no folder for models in the ISHealthMonitor.UI project, since the models are quite complex and plentiful, they reside in the other project, ISHealthMonitor.Core.



### ISHealthMonitor.Core

This can be thought of as the 'back-end' of the solution, containing data models, DB access operations, and many other things that serve as the backbone of the application. It's the workhorse of the entire solution, and nearly everything that makes the app work is in the HealthModel. Here are the folders:

- `Common/` - Contains helpers for HTTP requests and Azure services.

- `Contracts/` - Contains Interfaces that are designed to be implemented in the `Implementations/` folder.

- `DataAccess/` - Contains libraries and utility functions for interacting with the database, including helpers for HTTP requests, Confluence, Azure, Unity (work orders), and JWT auth.

- `Data/` - Contains the models and DB access points, divided into several folders:
    - `Data/Contexts/` - Defines the actual DbContext objects used in the app.
    - `Data/DbSet/` - Defines all of the DbSet objects that are directly tied to the database.
    - `Data/DTO/` - Defines all of the data transfer objects that are used in the UI when it interacts with the internal API endpoints. There is a DTO for each of the DbSet objects, plus a few more.
    - `Data/Models/` - These models are everything that the app uses that is not directly tied to a Db object, mostly for the public DataTable rows, and models for custom pages.

- `Helpers/` - Contains helper libraries for various functionalities:
    - `Helpers/Auth/` - Contains classes that define a custom authorization handler, managing the visibility of admin views by accessing the Users database table to check if the logged-in user has admin privileges.
    - `Helpers/Cache/` - Contains the classes that allow the Log Viewer to cache previous logs.
    - `Helpers/Confluence/` - Contains the classes that define and render the confluence table.
    - `Helpers/Email/` - Contains the classes that define and render the automatically sent email.

NOTE: 'IHealthModel' is injected nearly everywhere in the project, abstracting all the database accesses and backend functionalities from the UI project. This means it's quite a large implementation, so please look at `Contracts/IHealthModel.cs` to understand what it contains, and you can jump to specific function implementations from there.



## Site Pages

### Public Pages

1. **Cert Status page** - This page consists of an AJAX DataTable that calls the internal API of the project. It queries the database of sites and reminders, with each row of the DataTable containing the site's information, expiration date, the number of users subscribed (with existing reminders for that site), detailed subscription information (for admins only), and a link to create a work order. The associated controller function can be found in `SitesController::SiteStatusViewer()`.

2. **Configuration Builder** - This page allows users to create reminders for specific sites or, in other words, create a configuration. It presents a multi-select form where users can choose any number of available sites and select reminder intervals for each. Upon submission, Reminder objects are created for the user in the database, representing the user's configuration. The associated controller function can be found in `RemindersController::ConfigurationBuilder()`.

3. **Configuration History Viewer** - Here, users can view, edit, or delete previously submitted configuration forms. The 'GroupSubmissionID' column in the Reminder Db object is utilized to keep track of reminders submitted together, enabling the functionality of this page. The associated controller function can be found in `RemindersController::ConfigurationHistory()`.

4. **Home Page** - Serving as the entry point of the site, this page introduces users to the platform. For those with existing configurations, it displays subscribed sites and provides options to delete or edit reminders. Links to the above sites are also included.

5. **Work Order Builder** - Users can initiate the renewal process for a site's certificate if it's nearing expiration directly from this page. By clicking on the site's row in the Cert Status page and filling out the provided form, a work order can be created. The system has some capability to detect duplicate work orders. The process is initiated from the Cert Status page, guiding users to fill out the necessary information.



### Admin Pages

Administrators have access to the following specialized pages:

1. **CRUD pages for each Db table** - A separate page is available for each table (Sites, Reminders, ReminderIntervals, Users, Settings) that permits administrators to perform Create, Read, Update, and Delete (CRUD) operations on each database model.

2. **Log viewer** - This page provides the ability to specify a date range for log viewing, and further filter logs by their type, offering comprehensive oversight over system activity.

3. **Admin Panel** - Accessible by clicking on the admin user's name in the top right, this panel serves as a gateway to all available 'Admin Functions', as detailed below.

## Admin Functions

1. **Refresh Certs** - Performs a thorough refresh of site SSL certificate information by pinging the DNS server for each existing site in the database. This operation can be time-consuming due to its exhaustive nature.

2. **Update Confluence** - Renders an HTML table with updated site certificates, akin to the one from the Cert Status page, and utilizes Confluence's REST API to post the data to the site's designated space.

3. **Fire Reminders** - Processes the currently existing configured reminders, compares the current date with certificate expiration dates, and if applicable, sends advisory emails to specified recipients. Reminders are triggered only if the difference between the current date and the expiration date is within 24 hours of the configured reminder interval, allowing this job to fire daily without duplicate reminders.

4. **Create Auto Work Orders** - Automatically scans sites for certificates on the brink of expiration (within a critical threshold defined in the settings DB) and generates work orders accordingly. This function can be configured or disabled within the settings DB.

Note: These four functions are executed in the order listed above when the external-facing endpoint is invoked by the daily-running process.



## Models

### ISHealthMonitorSiteDbSet
This represents an individual site whose cert is monitored by the HealthMonitor.
```csharp
public class ISHealthMonitorSiteDbSet
{
    public int ID { get; set; }
    public string URL { get; set; }
    public string DisplayName { get; set; }
    public DateTime SSLEffectiveDate { get; set; }
    public DateTime SSLExpirationDate { get; set; }
    public string SSLIssuer { get; set; }
    public string SSLSubject { get; set; }
    public string SSLCommonName { get; set; }
    public string SSLThumbprint { get; set; }
    public string SiteCategory { get; set; } // used to group when selecting
    public Guid CreatedBy { get; set; } // GUID of who added from the CRUD page
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdated { get; set; } // When the cert status was last checked in the DNS
    public bool Deleted { get; set; } // If true, not considered for anything by the app
    public bool AllowWorkOrderCreation { get; set; } // can the user create a work order for this site
    public DateTime? HSIDBWorkOrderLastSubmittedDate { get; set; } // used to determine if there is an active WO
    public int? HSIDBWorkOrderCurrentObjectID { get; set; } // used to determine if there is an active WO
    public bool Disabled { get; set; } // arbitrary, for now
    public bool Active { get; set; } // arbitrary, for now
} 
```

### ISHealthMonitorIntervalDbSet
This represents an interval of time that the user can select when configuring their remidners for a site's cert expiration.
```csharp
public class ISHealthMonitorIntervalDbSet
{
    public int ID { get; set; }
    public int DurationInMinutes { get; set; }
    public string Type { get; set; } // arbitrary
    public string DisplayName { get; set; } // what is presented to the user
    public Guid CreatedBy { get; set; } // GUID of who added from the CRUD page
    public DateTime CreatedDate { get; set; }
    public bool Deleted { get; set; } // If true, not considered for anything by the app
    public bool Disabled { get; set; } // arbitrary, for now
    public bool Active { get; set; } // arbitrary, for now
} 
```

### ISHealthMonitorUserReminderDbSet
This reprents an individual reminder for a specific user, interval, and site combination. This model has foreign keys to the Interval and Site tables.
```csharp
public class ISHealthMonitorUserReminderDbSet
{
    public int ID { get; set; }
    public string UserName { get; set; } // logged-in username of creator
    public int ISHealthMonitorSiteID { get; set; } // foreign key to site table
    public int ISHealthMonitorIntervalID { get; set; } // foreign key to interval table
    public int ISHealthMonitorGroupSubmissionID { get; set; } // used to group forms together for history viewer
    public Guid CreatedBy { get; set; } // GUID of who added from the CRUD page
    public DateTime CreatedDate { get; set; }
    public bool Deleted { get; set; } // If true, not considered for anything by the app
    public bool Active { get; set; } // arbitrary, for now

    public ISHealthMonitorSiteDbSet? ISHealthMonitorSite { get; set; }
    public ISHealthMonitorIntervalDbSet? ISHealthMonitorInterval { get; set; }
}
```

### ISHealthMonitorUserDbSet
This does NOT represent which users are able to access the site and create reminders; anyone can. This is just to keep track of which users have admin privileges.
```csharp
public class ISHealthMonitorUserDbSet
{
    public int ID { get; set; }
    public Guid Guid { get; set; }
    public bool IsAdmin { get; set; } // If they have access to all admin views and actions
    public string DisplayName { get; set; } // Used for displaying in CRUD table
    public bool Disabled { get; set; } // arbitrary, for now
    public bool Deleted { get; set; } // If true, not considered for anything by the app
    public DateTime DateCreated { get; set; }
} 
```




## Testing

[to be filled]

## Build and Deployment

[to be filled]

## Contact

[to be filled]
