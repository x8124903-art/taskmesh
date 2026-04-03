using Microsoft.AspNetCore.Mvc;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.Project;

namespace MsProjects.Application.Controllers
{
    [ApiController]
    [Route("projects")]
    public sealed class ProjectsController : ControllerBase
    {
        private readonly IGetUserProjectsUseCase _getUserProjects;
        private readonly IGetProjectDetailsUseCase _getProjectDetails;
        private readonly ICreateProjectUseCase _createProject;
        private readonly IUpdateProjectUseCase _updateProject;
        private readonly IDeleteProjectUseCase _deleteProject;

        public ProjectsController(
            IGetUserProjectsUseCase getUserProjects,
            IGetProjectDetailsUseCase getProjectDetails,
            ICreateProjectUseCase createProject,
            IUpdateProjectUseCase updateProject,
            IDeleteProjectUseCase deleteProject)
        {
            _getUserProjects = getUserProjects;
            _getProjectDetails = getProjectDetails;
            _createProject = createProject;
            _updateProject = updateProject;
            _deleteProject = deleteProject;
        }

        private int GetCurrentUserId()
        {
            var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(userIdHeader))
                throw new UnauthorizedAccessException("User authentication required. X-User-Id header not found.");
            
            if (!int.TryParse(userIdHeader, out var userId))
                throw new ArgumentException($"Invalid X-User-Id header value: '{userIdHeader}' (cannot parse as integer)");
            
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            var projects = await _getUserProjects.ExecuteAsync(currentUserId, cancellationToken);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            var project = await _getProjectDetails.ExecuteAsync(id, currentUserId, cancellationToken);
            if (project == null)
                return NotFound();
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddProjectRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var currentUserId = GetCurrentUserId();
            var userName = Request.Headers["X-User-Name"].FirstOrDefault();
            var userEmail = Request.Headers["X-User-Email"].FirstOrDefault();
            var project = await _createProject.ExecuteAsync(request, currentUserId, userName, userEmail, cancellationToken);
            
            var locationUri = $"/api/v1.0/projects/{project.IdProject}";
            return Created(locationUri, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            await _updateProject.ExecuteAsync(id, request, currentUserId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            await _deleteProject.ExecuteAsync(id, currentUserId, cancellationToken);
            return NoContent();
        }
    }
}
