namespace MsProjects.Infrastructure.Repositories
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class QueriesMySql
    {
        internal const string GetAll = @"
            SELECT 
                p.IdProject, 
                p.Name, 
                p.Description, 
                p.Status,
                ps.Name AS StatusName,
                p.CreatedBy, 
                COALESCE(owner.UserName, CONCAT('User #', p.CreatedBy)) AS CreatedByName,
                p.IsDeleted, 
                p.CreatedAt
            FROM Project p
            LEFT JOIN ProjectStatus ps ON p.Status = ps.IdProjectStatus
            LEFT JOIN ProjectMember owner ON p.IdProject = owner.ProjectId AND owner.Role = 1
            WHERE p.IsDeleted = 0
            ORDER BY p.IdProject DESC;";

        internal const string GetById = @"
            SELECT 
                p.IdProject, 
                p.Name, 
                p.Description, 
                p.Status,
                ps.Name AS StatusName,
                p.CreatedBy, 
                COALESCE(owner.UserName, CONCAT('User #', p.CreatedBy)) AS CreatedByName,
                p.IsDeleted, 
                p.CreatedAt
            FROM Project p
            LEFT JOIN ProjectStatus ps ON p.Status = ps.IdProjectStatus
            LEFT JOIN ProjectMember owner ON p.IdProject = owner.ProjectId AND owner.Role = 1
            WHERE p.IdProject = @IdProject AND p.IsDeleted = 0;";

        internal const string Insert = @"
            INSERT INTO Project (Name, Description, Status, CreatedBy, IsDeleted, CreatedAt)
            VALUES (@Name, @Description, @Status, @CreatedBy, 0, @CreatedAt);
            SELECT LAST_INSERT_ID();";

        internal const string Update = @"
            UPDATE Project
            SET Name = @Name, Description = @Description, Status = @Status
            WHERE IdProject = @IdProject AND IsDeleted = 0;";

        internal const string Delete = @"
            UPDATE Project
            SET IsDeleted = 1
            WHERE IdProject = @IdProject;";

        internal const string GetProjectsByUserId = @"
            SELECT DISTINCT p.IdProject, p.Name, p.Description, p.Status, p.CreatedBy, p.IsDeleted, p.CreatedAt
            FROM Project p
            LEFT JOIN ProjectMember pm ON p.IdProject = pm.ProjectId
            WHERE p.IsDeleted = 0 AND (p.CreatedBy = @UserId OR pm.UserId = @UserId)
            ORDER BY p.IdProject DESC;";

        internal const string GetMembersByProjectId = @"
            SELECT 
                pm.IdProjectMember, 
                pm.ProjectId, 
                pm.UserId, 
                COALESCE(pm.UserName, CONCAT('User #', pm.UserId)) AS UserName,
                COALESCE(pm.Email, '') AS Email,
                pm.Role,
                pr.Name AS RoleName,
                pm.InvitedAt, 
                pm.JoinedAt
            FROM ProjectMember pm
            LEFT JOIN ProjectRole pr ON pm.Role = pr.IdProjectRole
            WHERE pm.ProjectId = @ProjectId
            ORDER BY pm.JoinedAt DESC;";

        internal const string GetMemberByProjectIdAndUserId = @"
            SELECT 
                pm.IdProjectMember, 
                pm.ProjectId, 
                pm.UserId, 
                COALESCE(pm.UserName, CONCAT('User #', pm.UserId)) AS UserName,
                COALESCE(pm.Email, '') AS Email,
                pm.Role,
                pr.Name AS RoleName,
                pm.InvitedAt, 
                pm.JoinedAt
            FROM ProjectMember pm
            LEFT JOIN ProjectRole pr ON pm.Role = pr.IdProjectRole
            WHERE pm.ProjectId = @ProjectId AND pm.UserId = @UserId
            LIMIT 1;";

        internal const string InsertMember = @"
            INSERT INTO ProjectMember (ProjectId, UserId, Role, InvitedAt, JoinedAt)
            VALUES (@ProjectId, @UserId, @Role, @InvitedAt, @JoinedAt);
            SELECT LAST_INSERT_ID();";

        internal const string UpdateMemberRole = @"
            UPDATE ProjectMember
            SET Role = @Role
            WHERE IdProjectMember = @IdProjectMember;";

        internal const string DeleteMember = @"
            DELETE FROM ProjectMember
            WHERE IdProjectMember = @IdProjectMember;";

        internal const string GetUserRoleInProject = @"
            SELECT Role 
            FROM ProjectMember 
            WHERE ProjectId = @ProjectId AND UserId = @UserId AND JoinedAt IS NOT NULL
            LIMIT 1;";

        internal const string GetProjectIdsByUserId = @"
            SELECT pm.ProjectId 
            FROM ProjectMember pm
            JOIN Project p ON pm.ProjectId = p.IdProject
            WHERE pm.UserId = @UserId AND p.IsDeleted = 0 AND pm.JoinedAt IS NOT NULL
            GROUP BY pm.ProjectId
            ORDER BY MAX(pm.JoinedAt) DESC;";

        internal const string ExistsByProjectIdAndUserId = @"
            SELECT COUNT(1) 
            FROM ProjectMember 
            WHERE ProjectId = @ProjectId AND UserId = @UserId AND JoinedAt IS NOT NULL;";

        internal const string ExistsMemberByProjectIdAndEmail = @"
            SELECT COUNT(1) 
            FROM ProjectMember 
            WHERE ProjectId = @ProjectId AND Email = @Email AND JoinedAt IS NOT NULL;";

        internal const string GetUserIdByEmail = @"
            SELECT UserId 
            FROM ProjectMember 
            WHERE Email = @Email AND JoinedAt IS NOT NULL
            LIMIT 1;";

        internal const string AddMember = @"
            INSERT INTO ProjectMember (ProjectId, UserId, Role, UserName, Email, InvitedAt, JoinedAt)
            VALUES (@ProjectId, @UserId, @Role, @UserName, @Email, @InvitedAt, @JoinedAt);
            SELECT LAST_INSERT_ID();";

        internal const string InsertInvitation = @"
            INSERT INTO ProjectInvitation 
                (ProjectId, Email, Role, Token, Status, InvitedByUserId, InvitedByName, InvitedAt, ExpiresAt)
            VALUES 
                (@ProjectId, @Email, @Role, @Token, 'Pending', @InvitedBy, @InvitedByName, @CreatedAt, @ExpiresAt);
            SELECT LAST_INSERT_ID();";

        internal const string GetInvitationById = @"
            SELECT 
                pi.IdProjectInvitation, 
                pi.ProjectId, 
                p.Name AS ProjectName,
                pi.Email, 
                pi.Role,
                pr.Name AS RoleName,
                pi.Token, 
                pi.Status, 
                pi.InvitedByUserId AS InvitedBy, 
                COALESCE(pi.InvitedByName, CONCAT('User #', pi.InvitedByUserId)) AS InvitedByName,
                pi.InvitedAt AS CreatedAt, 
                pi.ExpiresAt, 
                pi.AcceptedAt, 
                pi.RejectedAt
            FROM ProjectInvitation pi
            JOIN Project p ON pi.ProjectId = p.IdProject
            LEFT JOIN ProjectRole pr ON pi.Role = pr.IdProjectRole
            WHERE pi.IdProjectInvitation = @InvitationId;";

        internal const string GetInvitationByToken = @"
            SELECT 
                pi.IdProjectInvitation, 
                pi.ProjectId, 
                p.Name AS ProjectName,
                pi.Email, 
                pi.Role,
                pr.Name AS RoleName,
                pi.Token, 
                pi.Status, 
                pi.InvitedByUserId AS InvitedBy, 
                COALESCE(pi.InvitedByName, CONCAT('User #', pi.InvitedByUserId)) AS InvitedByName,
                pi.InvitedAt AS CreatedAt, 
                pi.ExpiresAt, 
                pi.AcceptedAt, 
                pi.RejectedAt
            FROM ProjectInvitation pi
            JOIN Project p ON pi.ProjectId = p.IdProject
            LEFT JOIN ProjectRole pr ON pi.Role = pr.IdProjectRole
            WHERE pi.Token = @Token;";

        internal const string GetPendingInvitationsByProjectId = @"
            SELECT 
                pi.IdProjectInvitation, 
                pi.ProjectId, 
                p.Name AS ProjectName,
                pi.Email, 
                pi.Role,
                pr.Name AS RoleName,
                pi.Token, 
                pi.Status, 
                pi.InvitedByUserId AS InvitedBy, 
                COALESCE(pi.InvitedByName, CONCAT('User #', pi.InvitedByUserId)) AS InvitedByName,
                pi.InvitedAt AS CreatedAt, 
                pi.ExpiresAt, 
                pi.AcceptedAt, 
                pi.RejectedAt
            FROM ProjectInvitation pi
            JOIN Project p ON pi.ProjectId = p.IdProject
            LEFT JOIN ProjectRole pr ON pi.Role = pr.IdProjectRole
            WHERE pi.ProjectId = @ProjectId AND pi.Status = 'Pending'
            ORDER BY pi.InvitedAt DESC;";

        internal const string GetInvitationsByEmail = @"
            SELECT 
                pi.IdProjectInvitation, 
                pi.ProjectId, 
                p.Name AS ProjectName,
                pi.Email, 
                pi.Role,
                pr.Name AS RoleName,
                pi.Token, 
                pi.Status, 
                pi.InvitedByUserId AS InvitedBy, 
                COALESCE(pi.InvitedByName, CONCAT('User #', pi.InvitedByUserId)) AS InvitedByName,
                pi.InvitedAt AS CreatedAt, 
                pi.ExpiresAt, 
                pi.AcceptedAt, 
                pi.RejectedAt
            FROM ProjectInvitation pi
            JOIN Project p ON pi.ProjectId = p.IdProject
            LEFT JOIN ProjectRole pr ON pi.Role = pr.IdProjectRole
            WHERE pi.Email = @Email
            ORDER BY pi.InvitedAt DESC;";

        internal const string AcceptInvitation = @"
            UPDATE ProjectInvitation
            SET Status = 'Accepted', AcceptedAt = @AcceptedAt
            WHERE IdProjectInvitation = @InvitationId;";

        internal const string RejectInvitation = @"
            UPDATE ProjectInvitation
            SET Status = 'Rejected', RejectedAt = @RejectedAt
            WHERE IdProjectInvitation = @InvitationId;";

        internal const string DeleteInvitation = @"
            DELETE FROM ProjectInvitation
            WHERE IdProjectInvitation = @InvitationId;";

        internal const string MarkExpiredInvitations = @"
            UPDATE ProjectInvitation
            SET Status = 'Expired'
            WHERE Status = 'Pending' AND ExpiresAt < @Now;";

        internal const string ExistsPendingByProjectAndEmail = @"
            SELECT COUNT(1)
            FROM ProjectInvitation
            WHERE ProjectId = @ProjectId AND Email = @Email AND Status = 'Pending';";
    }
}
