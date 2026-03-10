using Microsoft.AspNetCore.Mvc;
using MsProjects.Application.Models;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.Controllers
{
    [ApiController]
    [Route("projects")]
    public sealed class ProjectsController : ControllerBase
    {
        private readonly MsProjects.Domain.Services.IProjectService _service;
        private readonly IProjectAuthorizationService _authService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            MsProjects.Domain.Services.IProjectService service,
            IProjectAuthorizationService authService,
            ILogger<ProjectsController> logger)
        {
            _service = service;
            _authService = authService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(userIdHeader))
                throw new UnauthorizedAccessException("User authentication required. X-User-Id header not found.");
            
            if (!int.TryParse(userIdHeader, out var userId))
            {
                _logger.LogError("Failed to parse X-User-Id header. Value: '{Value}'", userIdHeader);
                throw new ArgumentException($"Invalid X-User-Id header value: '{userIdHeader}' (cannot parse as integer)");
            }
            
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            
            var userProjectIds = await _authService.GetUserProjectIdsAsync(currentUserId, cancellationToken);
            
            var allProjects = await _service.GetAllAsync(cancellationToken);
            
            var accessibleProjects = allProjects.Where(p => userProjectIds.Contains(p.IdProject)).ToList();
            
            return Ok(accessibleProjects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            
            var project = await _service.GetAsync(id, cancellationToken);
            if (project == null)
            {
                return NotFound();
            }
            
            if (!await _authService.IsProjectMemberAsync(currentUserId, id, cancellationToken))
            {
                return StatusCode(403);
            }
            
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddProjectRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var currentUserId = GetCurrentUserId();
            var userName = Request.Headers["X-User-Name"].FirstOrDefault();
            var userEmail = Request.Headers["X-User-Email"].FirstOrDefault();
            var project = await _service.AddAsync(request, currentUserId, cancellationToken, userName, userEmail);
            
            var locationUri = $"/api/v1.0/projects/{project.IdProject}";
            return Created(locationUri, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            
            var project = await _service.GetAsync(id, cancellationToken);
            if (project == null)
            {
                return NotFound();
            }
            
            if (!await _authService.HasProjectRoleAsync(currentUserId, id, cancellationToken, 
                MsProjects.Domain.Services.ProjectRoles.Owner, 
                MsProjects.Domain.Services.ProjectRoles.Admin))
            {
                return StatusCode(403);
            }
            
            await _service.UpdateAsync(id, request, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            
            var project = await _service.GetAsync(id, cancellationToken);
            if (project == null)
            {
                return NotFound();
            }
            
            if (!await _authService.IsProjectOwnerAsync(currentUserId, id, cancellationToken))
            {
                return StatusCode(403);
            }
            
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
