using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReconNess.Core.Services;
using ReconNess.Entities;
using ReconNess.Web.Dtos;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReconNess.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RootDomainsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly ITargetService targetService;
        private readonly IRootDomainService rootDomainService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetsController" /> class
        /// </summary>
        /// <param name="mapper"><see cref="IMapper"/></param>
        /// <param name="targetService"><see cref="ITargetService"/></param>
        /// <param name="rootDomainService"><see cref="IRootDomainService"/></param>
        public RootDomainsController(
            IMapper mapper,
            ITargetService targetService,
            IRootDomainService rootDomainService)
        {
            this.mapper = mapper;
            this.targetService = targetService;
            this.rootDomainService = rootDomainService;
        }

        // GET api/rootdomains/{targetName}/{rootDomainName}
        [HttpGet("{targetName}/{rootDomainName}")]
        public async Task<IActionResult> Get(string targetName, string rootDomainName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(rootDomainName))
            {
                return BadRequest();
            }

            var target = await this.targetService.GetTargetNotTrackingAsync(t => t.Name == targetName, cancellationToken);
            if (target == null)
            {
                return NotFound();
            }

            var rootDomain = await rootDomainService.GetRootDomainNoTrackingAsync(r => r.Target == target && r.Name == rootDomainName, cancellationToken);

            return Ok(this.mapper.Map<RootDomain, RootDomainDto>(rootDomain));
        }

        // DELETE api/rootdomains/deleteSubdomians/{targetName}/{rootDomainName}
        [HttpDelete("deleteSubdomians/{targetName}/{rootDomainName}")]
        public async Task<IActionResult> DeleteSubdomains(string targetName, string rootDomainName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(rootDomainName))
            {
                return BadRequest();
            }

            var target = await this.targetService.GetTargetNotTrackingAsync(t => t.Name == targetName, cancellationToken);
            if (target == null)
            {
                return NotFound();
            }

            var rootDomain = await this.rootDomainService.GetRootDomainWithSubdomainsAsync(r => r.Target == target && r.Name == rootDomainName, cancellationToken);
            if (rootDomain == null)
            {
                return NotFound();
            }

            await this.rootDomainService.DeleteSubdomainsAsync(rootDomain, cancellationToken);

            return NoContent();
        }

        // GET api/rootdomains/export/{targetName}/{rootDomainName}
        [HttpPost("export/{targetName}/{rootDomainName}")]
        public async Task<IActionResult> Export(string targetName, string rootDomainName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(rootDomainName))
            {
                return BadRequest();
            }

            var target = await this.targetService.GetTargetNotTrackingAsync(t => t.Name == targetName, cancellationToken);
            if (target == null)
            {
                return NotFound();
            }

            var rootDomain = await this.rootDomainService.ExportRootDomainNoTrackingAsync(r => r.Target == target && r.Name == rootDomainName, cancellationToken);
            if (rootDomain == null)
            {
                return NotFound();
            }

            var rootdomainDto = this.mapper.Map<RootDomain, RootDomainDto>(rootDomain);

            var download = Helpers.Helpers.ZipSerializedObject<RootDomainDto>(rootdomainDto);

            return File(download, "application/json", $"rootdomain-{rootDomain.Name}.json");
        }

        // POST api/rootdomains/uploadSubdomains/{targetName}/{rootDomainName}
        [HttpPost("uploadSubdomains/{targetName}/{rootDomainName}")]
        public async Task<IActionResult> UploadSubdomains(string targetName, string rootDomainName, IFormFile file, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(rootDomainName))
            {
                return BadRequest();
            }

            if (file.Length == 0)
            {
                return BadRequest();
            }

            var target = await this.targetService.GetTargetNotTrackingAsync(t => t.Name == targetName, cancellationToken);
            if (target == null)
            {
                return NotFound();
            }

            var rootDomain = await this.rootDomainService.GetRootDomainWithSubdomainsAsync(r => r.Target == target && r.Name == rootDomainName, cancellationToken);
            if (rootDomain == null)
            {
                return NotFound();
            }

            var path = Path.GetTempFileName();
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var subdomains = System.IO.File.ReadAllLines(path).ToList();
            await this.rootDomainService.UploadSubdomainsAsync(rootDomain, subdomains);

            return NoContent();
        }

        // GET api/rootdomains/exportSubdomains/{targetName}/{rootDomainName}
        [HttpPost("exportSubdomains/{targetName}/{rootDomainName}")]
        public async Task<IActionResult> DonwloadSubdomains(string targetName, string rootDomainName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(rootDomainName))
            {
                return BadRequest();
            }

            var target = await this.targetService.GetTargetNotTrackingAsync(t => t.Name == targetName, cancellationToken);
            if (target == null)
            {
                return NotFound();
            }

            var rootDomain = await this.rootDomainService.GetRootDomainWithSubdomainsAsync(r => r.Target == target && r.Name == rootDomainName, cancellationToken);
            if (rootDomain == null)
            {
                return NotFound();
            }

            var data = string.Join(",", rootDomain.Subdomains.Select(s => s.Name));

            var download = Encoding.UTF8.GetBytes(data);

            return File(download, "text/csv", "subdomains.csv");
        }
    }
}
