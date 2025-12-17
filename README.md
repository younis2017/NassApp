# NASSAD Advertising & Printing Platform

## Overview

NASSAD (nassad.ca) is a Canadian startup aiming to revolutionize advertising, design, and printing services by creating a central platform that connects clients with printing and design agencies across Canada.

This platform allows:

* Customers to submit orders online
* Automatic distribution of orders to registered agencies
* Real-time notifications via dashboard, email, and SMS
* First-come, first-served order acceptance

---

## Features

### Customer Features

* Order submission with detailed form:

  * Name, Phone, Email
  * Service Category
  * Attachment (direct file upload or URL)
  * Measurement and Description
* View order status

### Agency Features

* Receive order notifications (dashboard, email, SMS)
* Accept or decline orders
* Access order history

### Admin Features

* Manage categories, agencies, and customers
* Monitor transactions
* Generate reports

---

## Technology Stack

* **Backend:** C# (ASP.NET Core Web Application)
* **Frontend:** ASP.NET MVC / Razor Pages
* **Database:** Microsoft SQL Server
* **Notifications:** Email & SMS integration
* **API:** RESTful endpoints for third-party integrations

---

## Database Structure (Initial)

### Categories Table

* Category_id, Category_name, Category_status, Category_created

### Agency Table

* Agency_id, Agency_name, Agency_phone, Agency_email, Agency_website, Agency_address, Agency_location, Agency_tax_id, Agency_tenant, Agency_username, Agency_password, Agency_joined_date, Agency_logo, Agency_uid

### Customer Table

* Customer_id, Customer_name, Customer_phone, Customer_email, Customer_address, Customer_location, Customer_tax_id, Customer_tenant, Customer_username, Customer_password, Customer_joined_date, Customer_uid

### Transactions Table

* Trans_id, Trans_uid, Customer_id, Trans_date, Trans_blob_attachment, Trans_url_attachment, Trans_categories, Trans_description, Agency_id, Trans_received_date, Trans_max_agency, Trans_status, Agency_tenant

---

## Installation

1. Clone the repository

```
git clone https://github.com/yourusername/nassad.git
```

2. Open the solution in Visual Studio
3. Configure SQL Server connection string in `appsettings.json`
4. Run `Update-Database` using Package Manager Console to create the database
5. Build and run the application

---

## Usage

* Register agencies and customers via admin dashboard
* Customers submit orders
* Agencies receive notifications and accept/decline orders
* Admin monitors transactions and manages data

---

## Future Improvements

* Mobile app integration
* Internal order management for small businesses
* Advanced reporting and analytics
* Integration with shipping/logistics services

---

## License

This project is licensed under the MIT License.
