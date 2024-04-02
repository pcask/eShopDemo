using CatalogService.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PicturesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly CatalogContext _catalogContext;

        public PicturesController(IWebHostEnvironment env, CatalogContext catalogContext)
        {
            _env = env;
            _catalogContext = catalogContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("App Ok and Running");
        }

        [HttpGet]
        [Route("api/v1/catalog/items/{catalogItemId:int}/pic")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        // GET: /<controller>/
        public async Task<ActionResult> GetImageAsync(int catalogItemId)
        {
            if (catalogItemId <= 0)
            {
                return BadRequest();
            }

            var item = await _catalogContext.CatalogItems
                .SingleOrDefaultAsync(ci => ci.Id == catalogItemId);

            if (item != null)
            {
                var webRoot = _env.WebRootPath;
                var path = Path.Combine(webRoot, item.PictureFileName);

                string imageFileExtension = Path.GetExtension(item.PictureFileName);
                string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

                var buffer = await System.IO.File.ReadAllBytesAsync(path);

                return File(buffer, mimetype);
            }

            return NotFound();
        }

        private string GetImageMimeTypeFromImageFileExtension(string extension)
        {
            string mimetype = extension switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                ".wmf" => "image/wmf",
                ".jp2" => "image/jp2",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream",
            };
            return mimetype;
        }
    }
}
