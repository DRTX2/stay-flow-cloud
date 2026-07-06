# StayFlow Cloud — Activity / Process Diagrams

## 1. Guest Check-In Process

```mermaid
flowchart TD
    Start([Guest Arrives at Front Desk])
    SearchGuest[Search Guest Profile\nor Create New Guest]
    FindReservation[Locate Reservation\nby Confirmation Code]
    VerifyID[Verify Guest Identity\n& Document]
    RoomReady{Is Assigned\nRoom Ready?}
    AssignDifferentRoom[Assign Alternative\nAvailable Room]
    CheckRoomStatus{Room Status}
    ConfirmDetails[Confirm Stay Details:\nDates, Rate, Services]
    ProcessPayment[Capture Payment\nMethod / Pre-auth]
    IssueKeycard[Issue Room Key\n/ Access Code]
    UpdateReservation[Update Reservation\nStatus → CheckedIn]
    UpdateRoom[Update Room\nStatus → Occupied]
    ConfirmGuest[Provide Guest\nWelcome Info]
    End([Check-In Complete])

    Start --> SearchGuest
    SearchGuest --> FindReservation
    FindReservation --> VerifyID
    VerifyID --> RoomReady
    RoomReady -->|No| CheckRoomStatus
    CheckRoomStatus -->|Being Cleaned| AssignDifferentRoom
    CheckRoomStatus -->|Maintenance Issue| AssignDifferentRoom
    AssignDifferentRoom --> ConfirmDetails
    RoomReady -->|Yes| ConfirmDetails
    ConfirmDetails --> ProcessPayment
    ProcessPayment --> IssueKeycard
    IssueKeycard --> UpdateReservation
    UpdateReservation --> UpdateRoom
    UpdateRoom --> ConfirmGuest
    ConfirmGuest --> End
```

---

## 2. Reservation Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending : Guest creates booking
    Pending --> Confirmed : Staff confirms / auto-confirm
    Pending --> Cancelled : Guest cancels / timeout
    Confirmed --> CheckedIn : Guest arrives, check-in processed
    Confirmed --> NoShow : Guest doesn't arrive by deadline
    Confirmed --> Cancelled : Guest cancels (with policy)
    CheckedIn --> CheckedOut : Guest checks out
    CheckedOut --> [*]
    NoShow --> [*]
    Cancelled --> [*]

    Pending : 🟡 Pending\nAwaiting confirmation
    Confirmed : 🟢 Confirmed\nReservation secured
    CheckedIn : 🔵 Checked In\nGuest in room
    CheckedOut : ✅ Checked Out\nStay complete
    NoShow : 🔴 No Show\nGuest didn't arrive
    Cancelled : ⛔ Cancelled\nBooking cancelled
```

---

## 3. Invoice Generation & Payment

```mermaid
flowchart TD
    Start([Trigger: Check-Out\nor Manual Request])
    LoadReservation[Load Reservation\n& all Services]
    CalculateLines[Calculate Line Items:\n- Room nights × rate\n- Extra services\n- Late check-out fees]
    ApplyTaxes[Apply Tax Rules\n& Discounts]
    CreateInvoice[Create Invoice\nStatus: Draft]
    ReviewInvoice{Staff Reviews\nInvoice}
    ApplyAdjustments[Apply Manual\nAdjustments/Discounts]
    FinalizeInvoice[Finalize Invoice\nStatus: Issued]
    CollectPayment[Collect Payment:\n- Cash\n- Card\n- Bank Transfer]
    MarkPaid[Mark Invoice Paid\n+ Payment Method]
    GeneratePDF[Generate PDF\nInvoice]
    EmailGuest[Email Invoice PDF\nto Guest]
    End([Invoice Complete])

    Start --> LoadReservation
    LoadReservation --> CalculateLines
    CalculateLines --> ApplyTaxes
    ApplyTaxes --> CreateInvoice
    CreateInvoice --> ReviewInvoice
    ReviewInvoice -->|Adjustments Needed| ApplyAdjustments
    ApplyAdjustments --> ReviewInvoice
    ReviewInvoice -->|Approved| FinalizeInvoice
    FinalizeInvoice --> CollectPayment
    CollectPayment --> MarkPaid
    MarkPaid --> GeneratePDF
    GeneratePDF --> EmailGuest
    EmailGuest --> End
```

---

## 4. Housekeeping Workflow

```mermaid
flowchart TD
    Trigger([Trigger: Guest\nChecks Out])
    UpdateRoomDirty[Update Room Status\n→ Dirty]
    CreateTask[Create Housekeeping\nTask Automatically]
    AssignTask{Auto-assign\nto Staff?}
    ManualAssign[Supervisor Assigns\nTask Manually]
    AutoAssign[Auto-assign to\nAvailable Staff]
    NotifyStaff[Notify Staff\nof New Task]
    StaffStartsCleaning[Staff Marks Task\nIn Progress]
    CleanRoom[Clean & Prepare\nRoom]
    IssueFound{Issue\nFound?}
    ReportIssue[Report Maintenance\nRequest]
    MarkComplete[Mark Task\nComplete]
    UpdateRoomClean[Update Room Status\n→ Clean / Available]
    NotifyFrontDesk[Notify Front Desk:\nRoom Available]
    End([Room Ready\nfor Next Guest])

    Trigger --> UpdateRoomDirty
    UpdateRoomDirty --> CreateTask
    CreateTask --> AssignTask
    AssignTask -->|Auto| AutoAssign
    AssignTask -->|Manual| ManualAssign
    AutoAssign --> NotifyStaff
    ManualAssign --> NotifyStaff
    NotifyStaff --> StaffStartsCleaning
    StaffStartsCleaning --> CleanRoom
    CleanRoom --> IssueFound
    IssueFound -->|Yes| ReportIssue
    ReportIssue --> MarkComplete
    IssueFound -->|No| MarkComplete
    MarkComplete --> UpdateRoomClean
    UpdateRoomClean --> NotifyFrontDesk
    NotifyFrontDesk --> End
```

---

## 5. CI/CD Deployment Pipeline

```mermaid
flowchart LR
    Dev[Developer\nPushes Code]
    GH[GitHub\nRepository]

    subgraph "GitHub Actions"
        Lint[Lint &\nFormat Check]
        Build[Build &\nCompile]
        Test[Run Tests]
        DockerBuild[Docker Build\n(API + Web)]
        Push[Push to\nAzure ACR]
        Deploy[Deploy to\nAzure Container Apps]
    end

    Neon["DB Migrations\n(Migration Host)"]
    Live[Live Production\nEnvironment]

    Dev -->|git push| GH
    GH --> Lint
    Lint -->|pass| Build
    Build -->|pass| Test
    Test -->|pass| DockerBuild
    DockerBuild -->|images| Push
    Push --> Deploy
    Deploy --> Neon
    Deploy --> Live

    Lint -->|fail| Dev
    Build -->|fail| Dev
    Test -->|fail| Dev
```
