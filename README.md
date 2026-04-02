# 🧾 POS System (Point of Sale)

## 📌 Overview

This project is a **Point of Sale (POS) System** designed to manage the sale of devices, accessories, and related products.
It provides full control over products, categories, clients, sales, and stock across multiple warehouses.

The system is built with a focus on **clean architecture**, **scalability**, and **maintainability**.

---

## 🚀 Features

### 🛍️ Product Management

* Add, edit, delete, and view products
* Assign products to categories and models
* Store product descriptions and pricing

### 🗂️ Category Management

* Manage categories with optional icons
* Organize products efficiently

### 👤 Client Management

* Store client information (name, phone)
* Link clients to sales

### 💰 Sales Management

* Create sales transactions
* Track sale details (quantity, price, items)
* Automatically calculate total price
* Support multiple items per sale

### 📦 Stock Management

* Track product quantities in warehouses
* Prevent negative stock (data validation)
* Update stock after sales
* Unique product per warehouse tracking

### 🏬 Warehouse Management

* Manage multiple warehouses
* Assign categories and models to warehouses
* Track inventory per warehouse

---

## 🧱 Database Structure

The system uses a relational database with the following main entities:

* **Categories** → Product grouping
* **CategoryIcons** → Icons for categories
* **Products** → Main items for sale
* **Models** → Product models/types
* **Clients** → Customer information
* **Sales** → Sales transactions
* **SalesDetails** → Items inside each sale
* **Stock** → Inventory tracking
* **Warehouses** → Storage locations
* **WarehouseCategories / WarehouseModels** → Warehouse relationships

---

## 🔗 Relationships Overview

* A **Product** belongs to a **Category** and a **Model**
* A **Sale** belongs to a **Client**
* A **Sale** contains multiple **SalesDetails**
* Each **SalesDetail** references a **Stock item**
* **Stock** links Products with Warehouses
* Warehouses can have specific Categories and Models

---

## 🛠️ Tech Stack

* **Backend:** C# (.NET)
* **Database:** SQL (SQLite / SQL Server)
* **Architecture:** Layered / Clean Architecture (SOLID principles)

---

## ⚙️ Core Functionalities (CRUD)

The system supports full CRUD operations for:

* Categories
* Products
* Models
* Clients
* Warehouses
* Stock
* Sales and Sales Details

---

## 🎯 Project Goals

* Build a real-world POS system
* Practice database design and relationships
* Apply clean architecture and SOLID principles
* Create a scalable and maintainable system

---

## 📸 Screenshots

### 🗂️ Categories
![Categories](https://github.com/benrisma133/POS_Sytem/blob/main/screenshots/categories.png?raw=true)

### 🧩 Models
![Models](https://github.com/benrisma133/POS_Sytem/blob/main/screenshots/models.png?raw=true)

### 🏷️ Brands
![Brands](https://github.com/benrisma133/POS_Sytem/blob/main/screenshots/brands.png?raw=true)

### 📦 Products
![Products](https://github.com/benrisma133/POS_Sytem/blob/main/screenshots/products.png?raw=true)

### 📊 Series
![Series](https://github.com/benrisma133/POS_Sytem/blob/main/screenshots/series.png?raw=true)

### 🏬 Warehouses
![Warehouses](https://github.com/benrisma133/POS_Sytem/blob/main/screenshots/warehouses.png?raw=true)

---

## 🎥 Videos

### 🍔 Responsive Menu & Main Content
![Menu and Main Content](https://raw.githubusercontent.com/benrisma133/POS_Sytem/main/videos/menu_and_main_content.gif)
---

## ▶️ How to Run

1. Clone the repository:

   ```bash
   git clone https://github.com/benrisma133/POS_Sytem.git
   ```

2. Open the project in Visual Studio

3. Configure the database connection

4. Run the application

---

## 📌 Future Improvements

* Authentication & user roles
* Reporting and analytics
* Payment integration
* UI/UX improvements

---

## 👨‍💻 Author

**Ismail Benrahhal**

* Programmer specialized in software and mobile development
* Passionate about building scalable systems

---
