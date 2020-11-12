using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Data.Filtering;
using DevExpress.Xpo;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Xenial.Identity.Models;

namespace Xenial.Identity.Areas.Identity.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;

        public ProfileController(UnitOfWork unitOfWork)
            => this.unitOfWork = unitOfWork;

        [HttpGet]
        [Route("picture/{pictureId}")]
        public async Task<IActionResult> Get([Required] string pictureId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await unitOfWork.Query<XpoXeniaIIdentityUser>().Where(u => u.PictureId == pictureId).Select(u => new
            {
                PictureMimeType = u.PictureMimeType,
                Picture = u.Picture
            }).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            if (user.Picture == default || user.Picture.Length <= 0)
            {
                return NotFound();
            }

            var image = new MemoryStream(user.Picture);
            return File(image, user.PictureMimeType);
        }
    }
}
