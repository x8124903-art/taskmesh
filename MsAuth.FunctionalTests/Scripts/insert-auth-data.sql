USE taskmesh_auth;

INSERT INTO User (Email, Name, PasswordHash, Role) VALUES
('testuser@taskmesh.com', 'Test User', '$2a$12$2anUlT3M1oUOo9Q8OdswlOVd7FHj9PCw.p.DFL5AE1OZCSpdhjfYS', 'User'),
('admin@taskmesh.com', 'Admin User', '$2a$12$2anUlT3M1oUOo9Q8OdswlOVd7FHj9PCw.p.DFL5AE1OZCSpdhjfYS', 'Admin'),
('inactive@taskmesh.com', 'Inactive User', '$2a$12$2anUlT3M1oUOo9Q8OdswlOVd7FHj9PCw.p.DFL5AE1OZCSpdhjfYS', 'User');
