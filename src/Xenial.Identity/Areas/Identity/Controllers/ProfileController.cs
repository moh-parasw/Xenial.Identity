using System.ComponentModel.DataAnnotations;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Mvc;

using SkiaSharp;

using Xenial.Identity.Components;
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
                u.PictureMimeType,
                u.Picture,
                u.Initials,
                BackColor = u.Color
            }).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            if (user.Picture == default || user.Picture.Length <= 0)
            {
                if (!string.IsNullOrEmpty(user.Initials) && !string.IsNullOrEmpty(user.BackColor))
                {
                    using var bitmap = new SKBitmap(128, 128, false);
                    using var skCanvas = new SKCanvas(bitmap);
                    using var skPaintBack = new SKPaint();
                    using var skPaintFore = new SKPaint();

                    skPaintBack.Style = SKPaintStyle.Fill;
                    skPaintBack.IsAntialias = true;
                    skPaintBack.Color = SKColor.Parse(user.BackColor);
                    skPaintBack.StrokeWidth = 10;

                    skPaintFore.TextSize = 64.0f;
                    skPaintFore.IsAntialias = true;
                    skPaintFore.Color = SKColor.Parse(MaterialColorPicker.ColorIsDark(user.BackColor) ? "#FFFFFF" : "#000000");
                    skPaintFore.StrokeWidth = 3;
                    skPaintFore.TextAlign = SKTextAlign.Center;

                    skCanvas.DrawCircle(64, 64, 64, skPaintBack);
                    var offset = 85.5f;
                    skCanvas.DrawText(user.Initials, bitmap.Info.Width / 2f, offset, skPaintFore);

                    using var skImage = SKImage.FromBitmap(bitmap);
                    using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
                    return File(skData.AsStream(), "image/png");
                }

                return NotFound();
            }

            var image = new MemoryStream(user.Picture);
            return File(image, user.PictureMimeType);
        }
    }
}
