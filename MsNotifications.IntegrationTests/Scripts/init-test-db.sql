-- MsNotifications Integration Tests Database Schema

CREATE TABLE IF NOT EXISTS Notification (
    IdNotification INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Message VARCHAR(500) NOT NULL,
    RelatedEntityType VARCHAR(50) NULL,
    RelatedEntityId INT NULL,
    RelatedProjectId INT NULL,
    IsRead BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_user_id (UserId),
    INDEX idx_is_read (IsRead),
    INDEX idx_created_at (CreatedAt),
    INDEX idx_type (Type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS ProcessedEvent (
    EventId VARCHAR(100) PRIMARY KEY,
    EventType VARCHAR(100) NOT NULL,
    ProcessedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_event_type (EventType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
