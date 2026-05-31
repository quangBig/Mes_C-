# 🏭 Smart Factory MES System

## Giới thiệu

Smart Factory MES System là hệ thống quản lý sản xuất mô phỏng theo mô hình MES (Manufacturing Execution System) trong nhà máy thông minh.

Hệ thống hỗ trợ:

* Quản lý nhà máy (Factory)
* Quản lý xưởng sản xuất (Workshop)
* Quản lý dây chuyền sản xuất (Production Line)
* Quản lý máy móc (Machine)
* Quản lý sản phẩm (Product)
* Quản lý lệnh sản xuất (Work Order)
* Theo dõi sản xuất theo Serial Number
* Truy xuất nguồn gốc (Traceability)
* Quản lý lỗi sản xuất (Defect Management)
* Dashboard và báo cáo sản xuất
* Phân quyền người dùng theo vai trò

---

# Công nghệ sử dụng

## Backend

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* JWT Authentication
* Swagger

## Frontend

* ReactJS
* Tailwind CSS
* Axios
* React Router

## Database

* SQL Server

---

# Cấu trúc nhà máy

Factory
│
├── Workshop
│
├── Production Line
│
├── Machine
│
├── Work Order
│
├── Serial Number
│
├── Production Tracking
│
└── Defect Management

---

# Database Structure

## Users & Roles

Roles

* RoleId
* RoleName

Users

* UserId
* UserName
* Email
* PasswordHash
* RoleId

---

## Factory Structure

Factories

* FactoryId
* FactoryCode
* FactoryName

Workshops

* WorkshopId
* FactoryId
* WorkshopCode
* WorkshopName

ProductionLines

* LineId
* WorkshopId
* LineCode
* LineName

Machines

* MachineId
* LineId
* MachineCode
* MachineName
* Status

---

## Production

Products

* ProductId
* ProductCode
* ProductName

WorkOrders

* WorkOrderId
* WorkOrderCode
* ProductId
* Quantity
* PlannedStartTime
* PlannedEndTime
* StartTime
* EndTime
* Status

SerialNumbers

* SerialId
* SerialNumber
* WorkOrderId
* ProductId
* CurrentStatus

ProductionTracking

* TrackingId
* SerialId
* MachineId
* OperatorId
* ProcessName
* Result
* TrackingTime

---

## Quality

Defects

* DefectId
* DefectCode
* DefectName

DefectLogs

* DefectLogId
* TrackingId
* DefectId
* Remark

---

# Chức năng hệ thống

## 📊 Dashboard

Hiển thị:

* OEE
* Yield
* Production Today
* Running Orders
* Machine Status
* Defect Analysis
* Alarm Monitoring

---

## 🏭 Factory Management

Quản lý:

* Thêm Factory
* Sửa Factory
* Xóa Factory
* Xem danh sách Factory

API:

GET /api/Factory

GET /api/Factory/{id}

POST /api/Factory

PUT /api/Factory/{id}

DELETE /api/Factory/{id}

---

## 🏗 Workshop Management

Quản lý xưởng sản xuất.

Ví dụ:

* SMT Workshop
* Assembly Workshop
* Packaging Workshop

---

## ⚙ Production Line Management

Ví dụ:

* SMT Line 1
* SMT Line 2
* SMT Line 3

---

## 🖥 Machine Management

Ví dụ:

* Printer-01
* SPI-01
* Mounter-01
* Reflow-01
* AOI-01

Trạng thái:

* Running
* Alarm
* Maintenance
* Offline

---

## 📦 Product Management

Ví dụ:

* PCB-A01
* PCB-A02
* PCB-A03

---

## 📋 Work Order Management

Ví dụ:

WO2026001

Product: PCB-A01

Quantity: 1000

Status: Running

---

## 🔍 Traceability

Tìm kiếm theo Serial Number.

Ví dụ:

SN000001

Kết quả:

Printer PASS

SPI PASS

Mounter PASS

AOI FAIL

Defect:

Missing Component

---

## ❌ Defect Management

Các loại lỗi:

* Missing Component
* Wrong Part
* Solder Bridge
* Open Circuit

---

# Phân quyền hệ thống

## 👑 Admin

Toàn quyền hệ thống.

Menu:

* Dashboard
* Factory
* Workshops
* Production Lines
* Machines
* Products
* Work Orders
* Traceability
* Defects
* Users
* Reports

Quyền:

* Create
* Read
* Update
* Delete
* User Management
* Role Management

---

## 🏭 Manager

Quản lý sản xuất.

Menu:

* Dashboard
* Machines
* Products
* Work Orders
* Traceability
* Defects
* Reports

Không được truy cập:

* Users
* Factory
* Workshops

Chức năng:

* Theo dõi sản lượng
* Theo dõi tiến độ sản xuất
* Xem báo cáo

---

## 🔧 Engineer

Kỹ sư sản xuất.

Menu:

* Dashboard
* Machines
* Traceability
* Defects
* Reports

Chức năng:

* Theo dõi máy
* Xử lý Alarm
* Phân tích lỗi

Ví dụ:

AOI-01 Alarm

↓

Engineer kiểm tra

↓

Khắc phục sự cố

---

## 👷 Operator

Nhân viên vận hành.

Menu:

* Work Orders
* My Work Orders
* Traceability

Chức năng:

* Quét Serial Number
* Thực hiện công đoạn
* Ghi nhận PASS / FAIL

Ví dụ:

SN000001

[PASS]

[FAIL]

---

## 🧪 QC

Nhân viên chất lượng.

Menu:

* Dashboard
* Traceability
* Defects
* Reports

Chức năng:

* Kiểm tra lỗi
* Xác nhận lỗi
* Phân tích lỗi

Ví dụ:

SN000001

AOI FAIL

Defect:

Missing Component

[Confirm]

[Reject]

---

# Cài đặt dự án

## Tạo Migration

```powershell
& "$env:USERPROFILE\.dotnet\tools\dotnet-ef.exe" migrations add InitialCreate
```

## Update Database

```powershell
& "$env:USERPROFILE\.dotnet\tools\dotnet-ef.exe" database update
```

## Chạy Backend

```powershell
dotnet run
```

## Swagger

[https://localhost:xxxx/swagger](https://localhost:xxxx/swagger)

---

# Tác giả

Smart Factory MES System

Graduation Project – ASP.NET Core + ReactJS + SQL Server
