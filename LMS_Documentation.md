# LMS - Learning Management System Documentation

## Overview

This is a comprehensive Learning Management System (LMS) built with ASP.NET Core. The system allows users to create, manage, and participate in online classrooms with features like assignments, chat functionality, and payment integration.

## Technical Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: SQL Server (Entity Framework Core)
- **Authentication**: ASP.NET Identity
- **Real-time Communication**: SignalR
- **Email Service**: MailKit/MimeKit
- **Payment Integration**: VNPay
- **Testing**: xUnit, Moq, FluentAssertions
- **Containerization**: Docker

## Architecture

The application follows a typical MVC (Model-View-Controller) pattern with additional components:

### Core Components

1. **Models**: Data entities representing the application's domain
2. **Controllers**: Handle HTTP requests and orchestrate application flow
3. **Views**: Razor-based UI templates
4. **Services**: Business logic implementations
5. **Repositories**: Data access abstractions
6. **Hubs**: Real-time communication endpoints using SignalR

## Data Model

### Main Entities

1. **User**: Extends IdentityUser with additional properties for user profiles
   - Attributes: FullName, DateOfBirth, ImageUrl
   - Relationships: Has many Posts, ClassDetails
2. **ClassRoom**: Represents a course/class with properties like name, description, price

   - Attributes: Id (GUID), Name, Introduction, Description, ImageUrl, Code, Price, Students, Status (Pending/Approved), UserId (Creator)
   - Relationships: Belongs to a Topic, Has many Posts, ClassDetails, Assignments

3. **ClassDetail**: Represents user enrollment in a classroom (many-to-many relationship)

   - Attributes: ClassRoomId, UserId, CreateDate, LastModifiedDate, IsPaid
   - Relationships: Belongs to a ClassRoom and a User

4. **Assignment**: Tasks assigned to students within a classroom

   - Attributes: Id, Title, Description, FileUrl, DueDate, ClassRoomId, UserId (Creator)
   - Relationships: Belongs to a ClassRoom and a User, Has many Submissions

5. **Submission**: Student submissions for assignments

   - Attributes: Id, UserId (Submitter), AssignmentId, AnswerText, FileUrl, SubmitDate
   - Relationships: Belongs to an Assignment and a User

6. **Post**: Content shared within a classroom

   - Attributes: Id, UserId, Title, Message, ClassRoomId, CreateDate, LastModifiedDate
   - Relationships: Belongs to a ClassRoom and a User

7. **ChatRoom**: Virtual spaces for real-time communication

   - Attributes: Id, Name
   - Relationships: Has many Messages

8. **Message**: Individual chat messages within a chat room

   - Attributes: Id, UserId, Content, Timestamp, ChatRoomId
   - Relationships: Belongs to a ChatRoom

9. **Transaction**: Payment records for classroom enrollment

   - Attributes: Id, UserId, Amount, TransactionType, CreateDate, LastModifiedDate
   - Relationships: Belongs to a User

10. **Topic**: Categories for classrooms

    - Attributes: Id, Name, Alias, ImageUrl, ParentTopicId
    - Relationships: Has many ClassRooms

11. **Event**: Calendar events for scheduling
    - Attributes: Id, Title, Start, End
    - Relationships: Independent entity

## Detailed Database Schema

### Table Relationships

- **User-ClassRoom** (Many-to-Many): A user can create and enroll in multiple classrooms, and a classroom can have multiple users. This is implemented through the ClassDetail junction table.

- **User-Post** (One-to-Many): A user can create multiple posts, and each post belongs to one user.

- **ClassRoom-Post** (One-to-Many): A classroom can have multiple posts, and each post belongs to one classroom.

- **ClassRoom-Assignment** (One-to-Many): A classroom can have multiple assignments, and each assignment belongs to one classroom.

- **Assignment-Submission** (One-to-Many): An assignment can have multiple submissions, and each submission belongs to one assignment.

- **User-Submission** (One-to-Many): A user can have multiple submissions, and each submission belongs to one user.

- **ChatRoom-Message** (One-to-Many): A chat room can have multiple messages, and each message belongs to one chat room.

- **Topic-ClassRoom** (One-to-Many): A topic can have multiple classrooms, and each classroom belongs to one topic.

### Complete Database Table Fields

#### Users Table (ASP.NET Identity)

| Field                | Data Type      | Description                      | Constraints |
| -------------------- | -------------- | -------------------------------- | ----------- |
| Id                   | varchar(50)    | Primary key                      | PK          |
| UserName             | nvarchar(256)  | Login username                   | Unique      |
| NormalizedUserName   | nvarchar(256)  | Normalized username for searches | Unique      |
| Email                | nvarchar(256)  | User email address               |             |
| NormalizedEmail      | nvarchar(256)  | Normalized email for searches    |             |
| EmailConfirmed       | bit            | Whether email is confirmed       |             |
| PasswordHash         | nvarchar(max)  | Hashed password                  |             |
| SecurityStamp        | nvarchar(max)  | Security stamp for validation    |             |
| ConcurrencyStamp     | nvarchar(max)  | Concurrency token                |             |
| PhoneNumber          | nvarchar(max)  | User phone number                |             |
| PhoneNumberConfirmed | bit            | Whether phone is confirmed       |             |
| TwoFactorEnabled     | bit            | Whether 2FA is enabled           |             |
| LockoutEnd           | datetimeoffset | When lockout ends                |             |
| LockoutEnabled       | bit            | Whether account can be locked    |             |
| AccessFailedCount    | int            | Failed access attempts           |             |
| FullName             | nvarchar(max)  | User's full name                 |             |
| DateOfBirth          | datetime2      | User's birth date                |             |
| ImageUrl             | nvarchar(max)  | Profile image URL                |             |

#### ClassRooms Table

| Field            | Data Type        | Description                    | Constraints          |
| ---------------- | ---------------- | ------------------------------ | -------------------- |
| Id               | uniqueidentifier | Primary key                    | PK, Default: NEWID() |
| Name             | nvarchar(max)    | Classroom name                 |                      |
| TopicId          | int              | Related topic ID               | FK -> Topics.Id      |
| Introduction     | nvarchar(max)    | Short introduction             |                      |
| Description      | nvarchar(max)    | Detailed description           |                      |
| ImageUrl         | nvarchar(max)    | Cover image URL                |                      |
| Code             | nvarchar(max)    | Join code for students         |                      |
| Price            | decimal(18,0)    | Enrollment price               |                      |
| Students         | int              | Number of enrolled students    |                      |
| UserId           | nvarchar(max)    | Creator's user ID              |                      |
| CreateDate       | datetime2        | Creation timestamp             |                      |
| LastModifiedDate | datetime2        | Last update timestamp          | Nullable             |
| Status           | int              | Status (0=Pending, 1=Approved) |                      |

#### ClassDetails Table (Junction table for User-ClassRoom)

| Field            | Data Type        | Description                | Constraints             |
| ---------------- | ---------------- | -------------------------- | ----------------------- |
| ClassRoomId      | uniqueidentifier | Classroom ID               | PK, FK -> ClassRooms.Id |
| UserId           | varchar(50)      | User ID                    | PK, FK -> Users.Id      |
| CreateDate       | datetime2        | Enrollment date            |                         |
| LastModifiedDate | datetime2        | Last update timestamp      | Nullable                |
| IsPaid           | bit              | Whether enrollment is paid |                         |

#### Assignments Table

| Field            | Data Type        | Description           | Constraints         |
| ---------------- | ---------------- | --------------------- | ------------------- |
| Id               | int              | Primary key           | PK, Identity        |
| UserId           | varchar(50)      | Creator's user ID     | FK -> Users.Id      |
| Title            | nvarchar(max)    | Assignment title      |                     |
| Description      | nvarchar(max)    | Assignment details    |                     |
| FileUrl          | nvarchar(max)    | Attached file URL     |                     |
| DueDate          | datetime2        | Deadline              |                     |
| ClassRoomId      | uniqueidentifier | Related classroom     | FK -> ClassRooms.Id |
| CreateDate       | datetime2        | Creation timestamp    |                     |
| LastModifiedDate | datetime2        | Last update timestamp | Nullable            |

#### Submissions Table

| Field        | Data Type     | Description          | Constraints          |
| ------------ | ------------- | -------------------- | -------------------- |
| Id           | int           | Primary key          | PK, Identity         |
| UserId       | varchar(50)   | Submitter's user ID  | FK -> Users.Id       |
| AssignmentId | int           | Related assignment   | FK -> Assignments.Id |
| AnswerText   | nvarchar(max) | Text answer          |                      |
| FileUrl      | nvarchar(max) | Submitted file URL   |                      |
| SubmitDate   | datetime2     | Submission timestamp |                      |

#### Posts Table

| Field            | Data Type        | Description           | Constraints         |
| ---------------- | ---------------- | --------------------- | ------------------- |
| Id               | int              | Primary key           | PK, Identity        |
| UserId           | varchar(50)      | Author's user ID      | FK -> Users.Id      |
| Title            | nvarchar(max)    | Post title            |                     |
| Message          | nvarchar(max)    | Post content          |                     |
| ClassRoomId      | uniqueidentifier | Related classroom     | FK -> ClassRooms.Id |
| CreateDate       | datetime2        | Creation timestamp    |                     |
| LastModifiedDate | datetime2        | Last update timestamp | Nullable            |

#### ChatRooms Table

| Field | Data Type     | Description    | Constraints  |
| ----- | ------------- | -------------- | ------------ |
| Id    | int           | Primary key    | PK, Identity |
| Name  | nvarchar(max) | Chat room name | Required     |

#### Messages Table

| Field      | Data Type     | Description       | Constraints           |
| ---------- | ------------- | ----------------- | --------------------- |
| Id         | int           | Primary key       | PK, Identity          |
| UserId     | nvarchar(max) | Sender's user ID  | Required              |
| Content    | nvarchar(max) | Message content   | Required              |
| Timestamp  | datetime2     | Message timestamp | Default: DateTime.Now |
| ChatRoomId | int           | Related chat room | FK -> ChatRooms.Id    |

#### Transactions Table

| Field            | Data Type     | Description           | Constraints    |
| ---------------- | ------------- | --------------------- | -------------- |
| Id               | int           | Primary key           | PK, Identity   |
| UserId           | varchar(50)   | User ID               | FK -> Users.Id |
| Amount           | decimal(18,0) | Transaction amount    |                |
| TransactionType  | int           | Type of transaction   | Enum value     |
| CreateDate       | datetime2     | Creation timestamp    |                |
| LastModifiedDate | datetime2     | Last update timestamp | Nullable       |

#### Topics Table

| Field         | Data Type     | Description       | Constraints      |
| ------------- | ------------- | ----------------- | ---------------- |
| Id            | int           | Primary key       | PK, Identity     |
| Name          | nvarchar(max) | Topic name        |                  |
| Alias         | nvarchar(max) | URL-friendly name |                  |
| ImageUrl      | nvarchar(max) | Topic image URL   |                  |
| ParentTopicId | int           | Parent topic ID   | Self-referencing |

#### Events Table

| Field | Data Type     | Description | Constraints  |
| ----- | ------------- | ----------- | ------------ |
| Id    | int           | Primary key | PK, Identity |
| Title | nvarchar(max) | Event title |              |
| Start | datetime2     | Start time  |              |
| End   | datetime2     | End time    |              |

#### Roles Table (ASP.NET Identity)

| Field            | Data Type     | Description                  | Constraints |
| ---------------- | ------------- | ---------------------------- | ----------- |
| Id               | nvarchar(450) | Primary key                  | PK          |
| Name             | nvarchar(256) | Role name                    |             |
| NormalizedName   | nvarchar(256) | Normalized name for searches | Unique      |
| ConcurrencyStamp | nvarchar(max) | Concurrency token            |             |

#### UserRoles Table (ASP.NET Identity)

| Field  | Data Type     | Description | Constraints        |
| ------ | ------------- | ----------- | ------------------ |
| UserId | nvarchar(450) | User ID     | PK, FK -> Users.Id |
| RoleId | nvarchar(450) | Role ID     | PK, FK -> Roles.Id |

### Key Constraints

- **ClassDetail**: Has a composite primary key (ClassRoomId, UserId)
- **ClassRoom**: Uses GUID as primary key, generated with NEWID() in SQL Server
- **Transaction**: Uses the decimal(18,0) type for Amount to handle currency values

## Feature Workflow and Relationships

### Authentication & Authorization Flow

1. User registers with email and creates an account
2. Admin assigns roles (Administrator, Manager, User) through UserRolesController
3. Role-based permissions control access to different parts of the application

### Classroom Management Flow

1. Teacher creates a classroom (ClassRoomsController.Create)
2. Classroom is set to Pending status by default
3. Admin/Manager approves classroom (ClassRoomStatus changes to Approved)
4. Approved classrooms appear in listings (HomeController.Index)
5. Students can:
   - Join free classrooms using a code
   - Enroll in paid classrooms through payment process
6. Enrollment creates a ClassDetail record linking User to ClassRoom

### Content Management Flow

1. Teachers create posts within classrooms (PostsController.Create)
2. Posts appear in classroom details view (ClassRoomsController.Details)
3. Teachers create assignments with deadlines (AssignmentsController.Create)
4. Assignments appear in classroom details and can be viewed by enrolled students
5. Students submit work before the deadline (SubmissionsController.Create)
6. Teachers review submissions

### Communication Flow

1. Users join chat rooms (ChatController.Room)
2. ChatHub manages real-time message exchange using SignalR
3. Messages are stored in the database and retrieved for new users joining the room

### Calendar and Events Flow

1. Users create events (CalendarController.SaveEvent)
2. Events are stored in the Events table
3. Calendar view displays events for the selected time period

### Payment Integration Flow

1. Student initiates enrollment in a paid classroom
2. System redirects to payment page (PaysController.Pay)
3. VnPayService creates payment URL and redirects to VNPay
4. VNPay processes payment and redirects back to callback URL
5. PaysController.PaymentCallBack handles the response
6. On successful payment:
   - Transaction record is created
   - ClassDetail record is created with IsPaid=true
   - Student gains access to the classroom

## Performance Considerations

### Indexes

- ClassRoomId and UserId fields for efficient joins
- ClassRoom Status for filtering approved classrooms
- DueDate on Assignments for deadline queries

### Tracking

- IDateTracking interface implemented by key entities to automatically track creation and modification dates

### Data Loading

- Eager loading through Entity Framework Include() to reduce multiple database calls
- Pagination for large data sets (Posts, Assignments)

## Key Features

### Authentication & Authorization

- Identity-based authentication with roles (Admin, Manager, User)
- Role-based access control
- Account management

### Classroom Management

- Create and manage online classrooms
- Approval workflow for new classrooms
- Classroom enrollment (free and paid)
- Code-based classroom joining

### Content Management

- Post creation and management within classrooms
- Assignment creation and management
- File uploads and management
- Submission tracking and management

### Communication

- Real-time chat functionality using SignalR
- Classroom discussion boards

### Calendar and Events

- Event scheduling and management
- Calendar views for upcoming events and deadlines

### Payment Integration

- Integration with VNPay payment service
- Transaction management and tracking

## Architecture Patterns

- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Dependency Injection**: Service resolution
- **MVC**: Separation of concerns

## Key Files and Directories

### Configuration

- **Program.cs**: Application startup and service configuration
- **appsettings.json**: Application configuration

### Data

- **ApplicationDbContext.cs**: Entity Framework database context
- **Entities/**: Domain model classes
- **Migrations/**: Database migration files

### Business Logic

- **Controllers/**: HTTP request handlers
- **Services/**: Business logic implementations
- **Repositories/**: Data access components

### UI

- **Views/**: Razor templates for rendering UI
- **wwwroot/**: Static assets (CSS, JS, images)

### Real-time

- **Hubs/ChatHub.cs**: SignalR hub for real-time chat

### Integration

- **VnPayService.cs**: Payment integration service

## Deployment

The application is containerized with Docker:

- **Dockerfile**: Contains instructions for building the container
- **docker-compose.yml**: Multi-container orchestration
- **.dockerignore**: Specifies files to exclude from the build context

## Special Features

1. **Classroom Approval Flow**: Administrators must approve classrooms before they become visible
2. **Payment Integration**: Support for paid courses with VNPay
3. **Real-time Chat**: Instant messaging between classroom members
4. **Assignment Management**: Complete workflow for creating, submitting, and grading assignments

## System Requirements

- .NET 9.0 runtime
- SQL Server
- Docker (optional for containerized deployment)

## Project Structure

### Root Directory

- **Program.cs**: Application entry point and configuration
- **LMS.csproj**: Project file with dependencies
- **Dockerfile** and **docker-compose.yml**: Containerization configuration
- **appsettings.json**: Application settings

### /Controllers

- **HomeController.cs**: Main entry point, handles homepage and classroom listing
- **ClassRoomsController.cs**: Manages classroom operations (create, join, details)
- **AssignmentsController.cs**: Handles assignment creation and submission
- **ChatController.cs**: Chat functionality
- **PaysController.cs**: Payment processing
- **AccountController.cs**: User account management
- **TopicsController.cs**: Classroom category management
- **CalendarController.cs**: Event and calendar management
- **PostsController.cs**: Manages posts within classrooms
- **FileController.cs**: File upload and management

### /Models

- **ErrorViewModel.cs**: Error handling model

### /Data

- **ApplicationDbContext.cs**: Database context
- **DbInitializer.cs**: Initial data seeding
- **/Entities**: Domain models
  - **User.cs**: User profile
  - **ClassRoom.cs**: Classroom entity
  - **ClassDetail.cs**: User enrollment
  - **Assignment.cs**: Assignment entity
  - **Submission.cs**: Assignment submission
  - **Post.cs**: User posts in classrooms
  - **Message.cs**: Chat messages
  - **ChatRoom.cs**: Chat room entity
  - **Transaction.cs**: Payment transactions
  - **Topic.cs**: Classroom categories
  - **Event.cs**: Calendar events

### /Services

- **IStorageService.cs/FileStorageService.cs**: File handling
- **EmailSenderService.cs**: Email notifications
- **VnPayService.cs**: Payment processing
- **PostService.cs**: Post management

### /ViewModels

- **ClassRoomViewModel.cs**: Classroom display model
- **PostCreateRequest.cs**: Post creation
- **AssignmentCreateRequest.cs**: Assignment creation
- **UserViewModel.cs**: User display
- **RegisteredClassViewModel.cs**: User's enrolled classes

### /Views

- **/Home**: Main application views
- **/ClassRooms**: Classroom-related views
- **/Assignments**: Assignment views
- **/Chat**: Chat interface
- **/Calendar**: Calendar views
- **/Posts**: Post-related views
- **/Pays**: Payment views
- **/Shared**: Layout and common UI components

### /Hubs

- **ChatHub.cs**: SignalR hub for real-time messaging

### /wwwroot

- Static assets (CSS, JavaScript, images)

## Database Schema

The application uses Entity Framework Code-First approach with the following key tables:

- **Users**: User accounts (ASP.NET Identity)
- **Roles**: User roles (ASP.NET Identity)
- **ClassRooms**: Online classrooms/courses
- **ClassDetails**: User enrollments in classrooms
- **Assignments**: Tasks assigned to students
- **Submissions**: Student assignment submissions
- **Posts**: Content shared in classrooms
- **ChatRooms**: Messaging rooms
- **Messages**: Individual chat messages
- **Transactions**: Payment records
- **Topics**: Classroom categories
- **Events**: Calendar events

## Authentication Flow

1. Users register with email verification
2. Admin assigns roles (Administrator, Manager, User)
3. Role-based access controls permissions to various parts of the application

## Classroom Workflow

1. Teachers create classrooms (pending approval)
2. Admins/Managers approve classrooms
3. Classrooms become visible in listings
4. Students join via code or payment
5. Teachers add content and assignments
6. Students participate and submit work
