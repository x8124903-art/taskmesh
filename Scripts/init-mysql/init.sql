CREATE DATABASE IF NOT EXISTS taskmesh_auth;
CREATE DATABASE IF NOT EXISTS taskmesh_projects;
CREATE DATABASE IF NOT EXISTS taskmesh_tasks;

USE taskmesh_auth;

CREATE TABLE IF NOT EXISTS User (
    IdUser INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Name VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_email (Email),
    INDEX idx_role (Role)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS RefreshToken (
    IdRefreshToken INT AUTO_INCREMENT PRIMARY KEY,
    Token VARCHAR(500) NOT NULL UNIQUE,
    UserId INT NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    RevokedAt DATETIME NULL,
    INDEX idx_token (Token),
    INDEX idx_user_id (UserId),
    FOREIGN KEY (UserId) REFERENCES User(IdUser) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

USE taskmesh_projects;

CREATE TABLE IF NOT EXISTS ProjectRole (
    IdProjectRole INT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO ProjectRole (IdProjectRole, Name) VALUES 
    (1, 'Owner'), 
    (2, 'Admin'), 
    (3, 'Member'), 
    (4, 'Viewer')
ON DUPLICATE KEY UPDATE Name=Name;

CREATE TABLE IF NOT EXISTS ProjectStatus (
    IdProjectStatus INT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(200)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO ProjectStatus (IdProjectStatus, Name, Description) VALUES 
    (1, 'Active', 'Proyecto activo en desarrollo'),
    (2, 'Paused', 'Proyecto pausado temporalmente'),
    (3, 'Completed', 'Proyecto completado exitosamente'),
    (4, 'Archived', 'Proyecto archivado')
ON DUPLICATE KEY UPDATE Name=Name;

CREATE TABLE IF NOT EXISTS Project (
    IdProject INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    Status INT NOT NULL,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_created_by (CreatedBy),
    INDEX idx_status (Status),
    FOREIGN KEY (Status) REFERENCES ProjectStatus(IdProjectStatus)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProjectMember (
    IdProjectMember INT AUTO_INCREMENT PRIMARY KEY,
    ProjectId INT NOT NULL,
    UserId INT NOT NULL,
    Role INT NOT NULL,
    UserName VARCHAR(100) DEFAULT NULL,
    Email VARCHAR(255) DEFAULT NULL,
    InvitedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    JoinedAt DATETIME NULL,
    INDEX idx_project_id (ProjectId),
    INDEX idx_user_id (UserId),
    INDEX idx_project_user (ProjectId, UserId),
    INDEX idx_role (Role),
    UNIQUE KEY unique_project_user (ProjectId, UserId),
    FOREIGN KEY (ProjectId) REFERENCES Project(IdProject) ON DELETE CASCADE,
    FOREIGN KEY (Role) REFERENCES ProjectRole(IdProjectRole)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProjectInvitation (
    IdProjectInvitation INT AUTO_INCREMENT PRIMARY KEY,
    ProjectId INT NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Role INT NOT NULL,
    Token VARCHAR(500) NOT NULL UNIQUE,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    InvitedByUserId INT NOT NULL,
    InvitedByName VARCHAR(100) DEFAULT NULL,
    InvitedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NOT NULL,
    AcceptedAt DATETIME NULL,
    RejectedAt DATETIME NULL,
    INDEX idx_token (Token),
    INDEX idx_project (ProjectId),
    INDEX idx_email (Email),
    INDEX idx_status (Status),
    INDEX idx_role (Role),
    FOREIGN KEY (ProjectId) REFERENCES Project(IdProject) ON DELETE CASCADE,
    FOREIGN KEY (Role) REFERENCES ProjectRole(IdProjectRole)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ========================================
-- TASKMESH_TASKS DATABASE
-- ========================================

USE taskmesh_tasks;

CREATE TABLE IF NOT EXISTS Task (
    IdTask INT AUTO_INCREMENT PRIMARY KEY,
    ProjectId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    Status INT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 2,
    AssignedToUserId INT NULL,
    CreatedBy INT NOT NULL,
    DueDate DATETIME NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    RowVersion INT NOT NULL DEFAULT 1,
    IsDeleted BOOLEAN DEFAULT FALSE,
    DeletedAt DATETIME NULL,
    INDEX idx_project_id (ProjectId),
    INDEX idx_status (Status),
    INDEX idx_priority (Priority),
    INDEX idx_assigned_to (AssignedToUserId),
    INDEX idx_created_by (CreatedBy),
    INDEX idx_is_deleted (IsDeleted),
    INDEX idx_project_status (ProjectId, Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS TaskComment (
    IdTaskComment INT AUTO_INCREMENT PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    Comment TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_task_id (TaskId),
    INDEX idx_user_id (UserId),
    INDEX idx_created_at (CreatedAt),
    FOREIGN KEY (TaskId) REFERENCES Task(IdTask) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
