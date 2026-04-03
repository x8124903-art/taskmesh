USE taskmesh_projects;

INSERT INTO Project (Name, Description, Status, CreatedBy) VALUES
('Test Project 1', 'Integration test project', 1, 1);

INSERT INTO ProjectMember (ProjectId, UserId, Role, UserName, Email, JoinedAt) VALUES
(1, 1, 1, 'Test Owner', 'owner@test.com', NOW());
