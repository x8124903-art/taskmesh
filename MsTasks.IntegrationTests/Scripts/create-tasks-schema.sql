-- Set connection collation to match table collation
SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create Task table
CREATE TABLE IF NOT EXISTS Task (
    IdTask INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description TEXT NULL,
    ProjectId INT NOT NULL,
    AssignedToUserId INT NULL,
    Priority INT NOT NULL DEFAULT 2,
    Status INT NOT NULL DEFAULT 1,
    DueDate DATETIME NULL,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    RowVersion INT NOT NULL DEFAULT 1,
    IsDeleted BOOLEAN DEFAULT FALSE,
    DeletedAt DATETIME NULL,
    INDEX idx_project_id (ProjectId),
    INDEX idx_assigned_to (AssignedToUserId),
    INDEX idx_status (Status),
    INDEX idx_priority (Priority),
    INDEX idx_is_deleted (IsDeleted),
    INDEX idx_created_by (CreatedBy)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Create TaskComment table
CREATE TABLE IF NOT EXISTS TaskComment (
    IdTaskComment INT AUTO_INCREMENT PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    Comment TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TaskId) REFERENCES Task(IdTask) ON DELETE CASCADE,
    INDEX idx_task_id (TaskId),
    INDEX idx_created_at (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
