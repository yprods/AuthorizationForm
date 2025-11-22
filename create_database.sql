-- SQLite Database Schema for Authorization Form Application
-- This script creates all tables and initial data

-- Enable foreign keys
PRAGMA foreign_keys = ON;

-- ============================================
-- ASP.NET Identity Tables
-- ============================================

-- AspNetUsers table (extends IdentityUser)
CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id TEXT NOT NULL PRIMARY KEY,
    UserName TEXT,
    NormalizedUserName TEXT,
    Email TEXT,
    NormalizedEmail TEXT,
    EmailConfirmed INTEGER NOT NULL DEFAULT 0,
    PasswordHash TEXT,
    SecurityStamp TEXT,
    ConcurrencyStamp TEXT,
    PhoneNumber TEXT,
    PhoneNumberConfirmed INTEGER NOT NULL DEFAULT 0,
    TwoFactorEnabled INTEGER NOT NULL DEFAULT 0,
    LockoutEnd TEXT,
    LockoutEnabled INTEGER NOT NULL DEFAULT 0,
    AccessFailedCount INTEGER NOT NULL DEFAULT 0,
    -- Extended fields for ApplicationUser
    FullName TEXT,
    Department TEXT,
    IsManager INTEGER NOT NULL DEFAULT 0,
    IsAdmin INTEGER NOT NULL DEFAULT 0,
    ManagerId TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ManagerId) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT
);

-- AspNetRoles table
CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT,
    NormalizedName TEXT,
    ConcurrencyStamp TEXT
);

-- AspNetUserRoles table
CREATE TABLE IF NOT EXISTS AspNetUserRoles (
    UserId TEXT NOT NULL,
    RoleId TEXT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

-- AspNetUserClaims table
CREATE TABLE IF NOT EXISTS AspNetUserClaims (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    ClaimType TEXT,
    ClaimValue TEXT,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- AspNetRoleClaims table
CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    RoleId TEXT NOT NULL,
    ClaimType TEXT,
    ClaimValue TEXT,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

-- AspNetUserLogins table
CREATE TABLE IF NOT EXISTS AspNetUserLogins (
    LoginProvider TEXT NOT NULL,
    ProviderKey TEXT NOT NULL,
    ProviderDisplayName TEXT,
    UserId TEXT NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- AspNetUserTokens table
CREATE TABLE IF NOT EXISTS AspNetUserTokens (
    UserId TEXT NOT NULL,
    LoginProvider TEXT NOT NULL,
    Name TEXT NOT NULL,
    Value TEXT,
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- ============================================
-- Application Tables
-- ============================================

-- Employees table
CREATE TABLE IF NOT EXISTS Employees (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    EmployeeId TEXT NOT NULL,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Department TEXT,
    Position TEXT,
    Email TEXT,
    Phone TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsActive INTEGER NOT NULL DEFAULT 1
);

-- ApplicationSystems table
CREATE TABLE IF NOT EXISTS Systems (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    Category TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- AuthorizationRequests table
CREATE TABLE IF NOT EXISTS AuthorizationRequests (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    ServiceLevel INTEGER NOT NULL,
    SelectedEmployees TEXT NOT NULL,
    SelectedSystems TEXT NOT NULL,
    Comments TEXT,
    ManagerId TEXT NOT NULL,
    FinalApproverId TEXT,
    Status INTEGER NOT NULL DEFAULT 0,
    ManagerApprovedAt TEXT,
    ManagerApprovalSignature TEXT,
    FinalApprovedAt TEXT,
    FinalApprovalDecision TEXT,
    FinalApprovalComments TEXT,
    DisclosureAcknowledged INTEGER NOT NULL DEFAULT 0,
    DisclosureAcknowledgedAt TEXT,
    RejectionReason TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ChangedByAdminId TEXT,
    PreviousManagerId TEXT,
    ManagerChangedAt TEXT,
    PdfPath TEXT,
    LastReminderSentAt TEXT,
    ReminderCount INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ManagerId) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT,
    FOREIGN KEY (FinalApproverId) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ChangedByAdminId) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT
);

-- RequestHistories table
CREATE TABLE IF NOT EXISTS RequestHistories (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    RequestId INTEGER NOT NULL,
    PreviousStatus INTEGER NOT NULL,
    NewStatus INTEGER NOT NULL,
    ActionPerformedBy TEXT,
    ActionPerformedById TEXT,
    Comments TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RequestId) REFERENCES AuthorizationRequests(Id) ON DELETE CASCADE
);

-- FormTemplates table
CREATE TABLE IF NOT EXISTS FormTemplates (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    TemplateContent TEXT NOT NULL,
    PdfTemplatePath TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById TEXT NOT NULL,
    FOREIGN KEY (CreatedById) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT
);

-- EmailTemplates table
CREATE TABLE IF NOT EXISTS EmailTemplates (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    TriggerType INTEGER NOT NULL,
    Subject TEXT NOT NULL,
    Body TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById TEXT NOT NULL,
    RecipientType TEXT NOT NULL DEFAULT 'User',
    CustomRecipients TEXT,
    FOREIGN KEY (CreatedById) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT
);

-- ============================================
-- Indexes for Performance
-- ============================================

CREATE INDEX IF NOT EXISTS IX_AspNetUsers_ManagerId ON AspNetUsers(ManagerId);
CREATE INDEX IF NOT EXISTS IX_AspNetUsers_NormalizedUserName ON AspNetUsers(NormalizedUserName);
CREATE INDEX IF NOT EXISTS IX_AspNetUsers_NormalizedEmail ON AspNetUsers(NormalizedEmail);
CREATE INDEX IF NOT EXISTS IX_AspNetUserRoles_UserId ON AspNetUserRoles(UserId);
CREATE INDEX IF NOT EXISTS IX_AspNetUserRoles_RoleId ON AspNetUserRoles(RoleId);
CREATE INDEX IF NOT EXISTS IX_AspNetUserClaims_UserId ON AspNetUserClaims(UserId);
CREATE INDEX IF NOT EXISTS IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims(RoleId);
CREATE INDEX IF NOT EXISTS IX_AspNetUserLogins_UserId ON AspNetUserLogins(UserId);
CREATE INDEX IF NOT EXISTS IX_AuthorizationRequests_UserId ON AuthorizationRequests(UserId);
CREATE INDEX IF NOT EXISTS IX_AuthorizationRequests_ManagerId ON AuthorizationRequests(ManagerId);
CREATE INDEX IF NOT EXISTS IX_AuthorizationRequests_FinalApproverId ON AuthorizationRequests(FinalApproverId);
CREATE INDEX IF NOT EXISTS IX_AuthorizationRequests_Status ON AuthorizationRequests(Status);
CREATE INDEX IF NOT EXISTS IX_RequestHistories_RequestId ON RequestHistories(RequestId);
CREATE INDEX IF NOT EXISTS IX_FormTemplates_CreatedById ON FormTemplates(CreatedById);
CREATE INDEX IF NOT EXISTS IX_EmailTemplates_CreatedById ON EmailTemplates(CreatedById);
CREATE INDEX IF NOT EXISTS IX_Employees_EmployeeId ON Employees(EmployeeId);

-- ============================================
-- Seed Data
-- ============================================

-- Insert default roles
-- Note: ConcurrencyStamp will be generated by ASP.NET Identity when roles are created
-- These are placeholder values - the application will update them
INSERT OR IGNORE INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
    ('1', 'Admin', 'ADMIN', hex(randomblob(16))),
    ('2', 'Manager', 'MANAGER', hex(randomblob(16))),
    ('3', 'User', 'USER', hex(randomblob(16)));

-- Insert default systems
INSERT OR IGNORE INTO Systems (Id, Name, Description, Category, IsActive, CreatedAt, UpdatedAt) VALUES
    (1, 'מערכת HR', 'מערכת ניהול משאבי אנוש', 'ניהול', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    (2, 'מערכת כספים', 'מערכת ניהול כספים', 'כספים', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    (3, 'מערכת מכירות', 'מערכת ניהול מכירות', 'מכירות', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    (4, 'מערכת לוגיסטיקה', 'מערכת ניהול לוגיסטיקה', 'לוגיסטיקה', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

-- Insert sample employees
INSERT OR IGNORE INTO Employees (EmployeeId, FirstName, LastName, Department, Email, CreatedAt, UpdatedAt, IsActive) VALUES
    ('EMP001', 'יוסי', 'כהן', 'IT', 'yossi@example.com', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1),
    ('EMP002', 'שרה', 'לוי', 'HR', 'sara@example.com', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1),
    ('EMP003', 'דוד', 'ישראלי', 'כספים', 'david@example.com', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1);

-- ============================================
-- End of Script
-- ============================================

