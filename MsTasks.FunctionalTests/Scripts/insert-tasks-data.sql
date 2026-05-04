-- Insert sample tasks for functional tests
-- Project 1, multiple statuses and priorities
-- Status: Todo=1, InProgress=2, Review=3, Testing=4, Done=5, Blocked=6
-- Priority: Low=1, Medium=2, High=3

-- Todo tasks
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, RowVersion, IsDeleted)
VALUES 
(1, 'Setup project infrastructure', 'Configure Docker and CI/CD pipeline', 1, 10, 3, 1, '2025-02-01 12:00:00', 10, 1, 0),
(5, 'Implement task board', 'Create Kanban board component', 1, 20, 3, 1, '2025-02-05 12:00:00', 20, 1, 0),
(6, 'Setup monitoring', 'Configure Prometheus and Grafana', 1, NULL, 1, 1, '2025-02-10 12:00:00', 10, 1, 0),
(7, 'Performance optimization', 'Optimize database queries', 1, NULL, 2, 1, NULL, 10, 1, 0);

-- InProgress tasks
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, RowVersion, IsDeleted)
VALUES 
(2, 'Implement authentication', 'Add JWT authentication', 1, 10, 3, 2, '2025-01-25 18:00:00', 10, 1, 0),
(4, 'Create unit tests', 'Write comprehensive unit tests', 1, 20, 2, 2, '2025-01-30 12:00:00', 10, 1, 0);

-- Review tasks
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, RowVersion, IsDeleted)
VALUES 
(8, 'Code review task', 'Review PR #123', 1, 10, 2, 3, '2025-01-22 12:00:00', 10, 1, 0);

-- Testing tasks
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, RowVersion, IsDeleted)
VALUES 
(9, 'Testing task', 'Test new features', 1, 20, 2, 4, '2025-01-23 12:00:00', 10, 1, 0);

-- Done tasks
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, RowVersion, IsDeleted)
VALUES 
(3, 'Write API documentation', 'Document all endpoints with OpenAPI', 1, 10, 2, 5, '2025-01-20 16:00:00', 10, 1, 0);

-- Blocked tasks
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, RowVersion, IsDeleted)
VALUES 
(10, 'Blocked task', 'Waiting for external dependency', 1, 10, 3, 6, '2025-01-28 12:00:00', 10, 1, 0);

-- Insert sample comments
INSERT INTO TaskComment (IdTaskComment, TaskId, UserId, Comment, CreatedAt) VALUES
(1, 2, 10, 'Started working on JWT implementation', '2025-01-15 10:00:00'),
(2, 2, 20, 'Need clarification on token refresh logic', '2025-01-15 14:00:00'),
(3, 2, 10, 'Tokens expire after 15 minutes, refresh tokens last 7 days', '2025-01-15 14:30:00'),
(4, 4, 20, 'Unit tests completed for Task service', '2025-01-16 11:00:00'),
(5, 8, 10, 'Approved, looks good to merge', '2025-01-18 12:00:00');
