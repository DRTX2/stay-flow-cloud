# StayFlow Cloud — Use Case Diagrams

## System Actors

| Actor | Description |
|-------|-------------|
| **Guest** | Hotel guest with access to the self-service guest portal |
| **Front Desk Staff** | Hotel staff managing day-to-day operations |
| **Housekeeping Staff** | Staff managing room cleaning tasks |
| **Tenant Admin** | Hotel owner/manager configuring the system |
| **Platform Owner** | StayFlow Cloud operator managing all tenants |

---

## 1. Reservation Management

```mermaid
graph LR
    Guest["🧑 Guest"]
    Staff["👤 Front Desk Staff"]
    Admin["🏨 Tenant Admin"]

    UC1(("Browse\nAvailability"))
    UC2(("Create\nReservation"))
    UC3(("Modify\nReservation"))
    UC4(("Cancel\nReservation"))
    UC5(("Check In\nGuest"))
    UC6(("Check Out\nGuest"))
    UC7(("Assign\nRoom"))
    UC8(("View\nReservation List"))
    UC9(("Add Extra\nServices"))

    Guest --> UC1
    Guest --> UC2
    Staff --> UC2
    Staff --> UC3
    Staff --> UC4
    Staff --> UC5
    Staff --> UC6
    Staff --> UC7
    Staff --> UC8
    Staff --> UC9
    Admin --> UC8
    Admin --> UC4

    UC5 -.->|includes| UC7
    UC6 -.->|includes| UC9
```

---

## 2. Billing & Invoicing

```mermaid
graph LR
    Staff["👤 Front Desk Staff"]
    Admin["🏨 Tenant Admin"]
    Guest["🧑 Guest"]

    UC1(("Generate\nInvoice"))
    UC2(("View Invoice\nDetails"))
    UC3(("Mark Invoice\nPaid"))
    UC4(("Apply\nDiscount"))
    UC5(("Export\nInvoice PDF"))
    UC6(("View Billing\nHistory"))

    Staff --> UC1
    Staff --> UC2
    Staff --> UC3
    Staff --> UC4
    Staff --> UC5
    Admin --> UC6
    Admin --> UC2
    Guest --> UC2
    Guest --> UC5

    UC1 -.->|includes| UC2
    UC3 -.->|extends| UC1
```

---

## 3. Housekeeping & Maintenance

```mermaid
graph LR
    Housekeeper["🧹 Housekeeping Staff"]
    Maintenance["🔧 Maintenance Staff"]
    Admin["🏨 Tenant Admin"]
    Staff["👤 Front Desk Staff"]

    UC1(("View Assigned\nCleaning Tasks"))
    UC2(("Mark Room\nCleaned"))
    UC3(("Report\nMaintenance Issue"))
    UC4(("View Maintenance\nRequests"))
    UC5(("Resolve\nIssue"))
    UC6(("Override Room\nStatus"))
    UC7(("Assign Task\nManually"))

    Housekeeper --> UC1
    Housekeeper --> UC2
    Housekeeper --> UC3
    Maintenance --> UC4
    Maintenance --> UC5
    Admin --> UC7
    Admin --> UC6
    Staff --> UC6
    Staff --> UC7
```

---

## 4. Guest Management

```mermaid
graph LR
    Staff["👤 Front Desk Staff"]
    Admin["🏨 Tenant Admin"]
    Guest["🧑 Guest"]

    UC1(("Create Guest\nProfile"))
    UC2(("Search\nGuest"))
    UC3(("View Guest\nHistory"))
    UC4(("Edit Contact\nInfo"))
    UC5(("View Guest\n360 Profile"))
    UC6(("Upload Guest\nDocument"))

    Staff --> UC1
    Staff --> UC2
    Staff --> UC3
    Staff --> UC5
    Staff --> UC6
    Admin --> UC3
    Guest --> UC4
    Guest --> UC6

    UC5 -.->|includes| UC3
```

---

## 5. Tenant Administration

```mermaid
graph LR
    Admin["🏨 Tenant Admin"]
    Platform["⚙️ Platform Owner"]

    UC1(("Configure Hotel\nSettings"))
    UC2(("Manage Staff\nAccounts"))
    UC3(("Define Room\nTypes & Rates"))
    UC4(("View Analytics\nDashboard"))
    UC5(("Enable/Disable\nFeature Flags"))
    UC6(("Create\nTenant"))
    UC7(("Manage All\nTenants"))
    UC8(("Access Audit\nLogs"))

    Admin --> UC1
    Admin --> UC2
    Admin --> UC3
    Admin --> UC4
    Admin --> UC5
    Admin --> UC8
    Platform --> UC6
    Platform --> UC7
    Platform --> UC8

    UC2 -.->|includes| UC5
```

---

## 6. Authentication & Access Control

```mermaid
graph LR
    AnyUser["👥 Any User"]
    Staff["👤 Staff / Admin"]
    Platform["⚙️ Platform Owner"]

    UC1(("Sign In with\nEmail + Password"))
    UC2(("Sign Out"))
    UC3(("Refresh\nSession"))
    UC4(("Sign In with\nGoogle/Microsoft"))
    UC5(("Manage\nPermissions"))
    UC6(("Assign\nRoles"))

    AnyUser --> UC1
    AnyUser --> UC2
    AnyUser --> UC3
    AnyUser --> UC4
    Staff --> UC6
    Platform --> UC5
    Platform --> UC6

    UC1 -.->|extends| UC4
```
