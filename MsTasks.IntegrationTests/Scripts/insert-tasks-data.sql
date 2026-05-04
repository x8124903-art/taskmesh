-- Insert test tasks for Project 1 (≥20 tasks for Board testing per SC-002)
-- Distribution across all 6 statuses
-- Status: Todo=1, InProgress=2, Review=3, Testing=4, Done=5, Blocked=6
-- Priority: Low=1, Medium=2, High=3

-- Todo (5 tasks)
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, CreatedAt, RowVersion, IsDeleted)
VALUES 
(1, 'Setup development environment', 'Configure IDE and tools', 1, 20, 3, 1, DATE_ADD(NOW(), INTERVAL 7 DAY), 10, NOW(), 1, 0),
(2, 'Design database schema', 'Create ERD and table definitions', 1, NULL, 3, 1, DATE_ADD(NOW(), INTERVAL 5 DAY), 10, NOW(), 1, 0),
(3, 'Write API documentation', 'Document all REST endpoints', 1, 20, 2, 1, DATE_ADD(NOW(), INTERVAL 10 DAY), 10, NOW(), 1, 0),
(4, 'Create user stories', 'Define acceptance criteria', 1, NULL, 1, 1, DATE_ADD(NOW(), INTERVAL 14 DAY), 10, NOW(), 1, 0),
(5, 'Setup CI/CD pipeline', 'Configure GitHub Actions', 1, 20, 2, 1, DATE_ADD(NOW(), INTERVAL 3 DAY), 10, NOW(), 1, 0);

-- InProgress (4 tasks)
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, CreatedAt, RowVersion, IsDeleted)
VALUES 
(6, 'Implement authentication', 'Add JWT token validation', 1, 20, 3, 2, DATE_ADD(NOW(), INTERVAL 2 DAY), 10, DATE_SUB(NOW(), INTERVAL 2 DAY), 1, 0),
(7, 'Create user registration', 'Build signup form and API', 1, 20, 3, 2, DATE_ADD(NOW(), INTERVAL 1 DAY), 10, DATE_SUB(NOW(), INTERVAL 3 DAY), 1, 0),
(8, 'Develop project CRUD', 'Implement project management', 1, 10, 2, 2, NOW(), 10, DATE_SUB(NOW(), INTERVAL 1 DAY), 1, 0),
(9, 'Build task board UI', 'Create Kanban board component', 1, 20, 2, 2, DATE_ADD(NOW(), INTERVAL 4 DAY), 10, DATE_SUB(NOW(), INTERVAL 5 DAY), 1, 0);

-- Review (4 tasks)
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, CreatedAt, RowVersion, IsDeleted)
VALUES 
(10, 'Code review authentication', 'Review JWT implementation', 1, 10, 3, 3, NOW(), 20, DATE_SUB(NOW(), INTERVAL 1 HOUR), 1, 0),
(11, 'Review database migrations', 'Check migration scripts', 1, 10, 2, 3, DATE_ADD(NOW(), INTERVAL 1 DAY), 20, DATE_SUB(NOW(), INTERVAL 3 HOUR), 1, 0),
(12, 'Review API endpoints', 'Validate REST design', 1, NULL, 2, 3, DATE_ADD(NOW(), INTERVAL 2 DAY), 20, DATE_SUB(NOW(), INTERVAL 6 HOUR), 1, 0),
(13, 'Review UI components', 'Check component structure', 1, 10, 1, 3, DATE_ADD(NOW(), INTERVAL 3 DAY), 20, DATE_SUB(NOW(), INTERVAL 12 HOUR), 1, 0);

-- Testing (3 tasks)
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, CreatedAt, RowVersion, IsDeleted)
VALUES 
(14, 'Test user registration flow', 'E2E testing of signup', 1, 20, 3, 4, NOW(), 10, DATE_SUB(NOW(), INTERVAL 2 HOUR), 1, 0),
(15, 'Test project creation', 'Validate project CRUD', 1, 20, 2, 4, DATE_ADD(NOW(), INTERVAL 1 DAY), 10, DATE_SUB(NOW(), INTERVAL 4 HOUR), 1, 0),
(16, 'Performance testing', 'Load test API endpoints', 1, NULL, 1, 4, DATE_ADD(NOW(), INTERVAL 5 DAY), 10, DATE_SUB(NOW(), INTERVAL 1 DAY), 1, 0);

-- Done (5 tasks)
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, CreatedAt, RowVersion, IsDeleted)
VALUES 
(17, 'Project initialization', 'Setup repository and structure', 1, 10, 3, 5, DATE_SUB(NOW(), INTERVAL 10 DAY), 10, DATE_SUB(NOW(), INTERVAL 15 DAY), 1, 0),
(18, 'Setup MySQL database', 'Configure database server', 1, 10, 3, 5, DATE_SUB(NOW(), INTERVAL 8 DAY), 10, DATE_SUB(NOW(), INTERVAL 12 DAY), 1, 0),
(19, 'Install dependencies', 'Setup project packages', 1, 20, 2, 5, DATE_SUB(NOW(), INTERVAL 7 DAY), 10, DATE_SUB(NOW(), INTERVAL 11 DAY), 1, 0),
(20, 'Create README documentation', 'Write installation guide', 1, 20, 1, 5, DATE_SUB(NOW(), INTERVAL 5 DAY), 10, DATE_SUB(NOW(), INTERVAL 9 DAY), 1, 0),
(21, 'Setup Docker Compose', 'Configure containers', 1, 10, 2, 5, DATE_SUB(NOW(), INTERVAL 4 DAY), 10, DATE_SUB(NOW(), INTERVAL 8 DAY), 1, 0);

-- Blocked (2 tasks)
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, CreatedAt, RowVersion, IsDeleted)
VALUES 
(22, 'Deploy to production', 'Setup production environment', 1, NULL, 3, 6, DATE_ADD(NOW(), INTERVAL 30 DAY), 10, DATE_SUB(NOW(), INTERVAL 2 DAY), 1, 0),
(23, 'Configure monitoring', 'Setup Prometheus and Grafana', 1, NULL, 2, 6, DATE_ADD(NOW(), INTERVAL 20 DAY), 10, DATE_SUB(NOW(), INTERVAL 3 DAY), 1, 0);

-- Additional tasks for Project 1 owned by Member (user 20) for FR-011 testing
INSERT INTO Task (IdTask, Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, CreatedAt, RowVersion, IsDeleted)
VALUES 
(24, 'Task owned by Member', 'Member can delete this', 1, 20, 1, 1, DATE_ADD(NOW(), INTERVAL 7 DAY), 20, NOW(), 1, 0),
(25, 'Task assigned to Member', 'Member can change status', 1, 20, 2, 2, DATE_ADD(NOW(), INTERVAL 3 DAY), 10, NOW(), 1, 0);

-- Insert test comments for tasks
INSERT INTO TaskComment (TaskId, UserId, Comment, CreatedAt)
VALUES 
(1, 10, 'Started working on this task', DATE_SUB(NOW(), INTERVAL 1 HOUR)),
(1, 20, 'IDE setup complete', DATE_SUB(NOW(), INTERVAL 30 MINUTE)),
(6, 20, 'JWT implementation in progress', DATE_SUB(NOW(), INTERVAL 2 HOUR)),
(6, 10, 'Please add unit tests', DATE_SUB(NOW(), INTERVAL 1 HOUR)),
(6, 20, 'Tests added, ready for review', DATE_SUB(NOW(), INTERVAL 30 MINUTE)),
(10, 10, 'Code looks good, minor changes needed', DATE_SUB(NOW(), INTERVAL 15 MINUTE)),
(17, 10, 'Repository structure approved', DATE_SUB(NOW(), INTERVAL 5 DAY));
