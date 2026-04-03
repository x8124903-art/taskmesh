USE taskmesh_projects;

INSERT INTO Project (Name, Description, Status, CreatedBy) VALUES
('Functional Test Project 1', 'First test project', 1, 1),
('Functional Test Project 2', 'Second test project', 1, 1),
('Paused Project', 'A paused project', 2, 2);

INSERT INTO ProjectMember (ProjectId, UserId, Role, JoinedAt) VALUES
(1, 1, 1, NOW()),  -- User 1 is Owner of Project 1
(1, 2, 3, NOW()),  -- User 2 is Member of Project 1
(2, 1, 1, NOW()),  -- User 1 is Owner of Project 2
(3, 2, 1, NOW());  -- User 2 is Owner of Project 3

INSERT INTO ProjectInvitation (ProjectId, Email, Role, Token, Status, InvitedByUserId, ExpiresAt) VALUES
(1, 'invited@taskmesh.com', 3, 'test-token-001', 'Pending', 1, DATE_ADD(NOW(), INTERVAL 7 DAY)),
(2, 'expired@taskmesh.com', 3, 'test-token-002', 'Expired', 1, DATE_SUB(NOW(), INTERVAL 1 DAY));
