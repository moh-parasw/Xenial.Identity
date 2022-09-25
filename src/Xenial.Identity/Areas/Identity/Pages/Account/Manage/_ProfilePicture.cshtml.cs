﻿using Xenial.Identity.Components;
using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Identity.Pages.Account.Manage
{
    public class ProfilePictureModel
    {
        public ProfilePictureModel(XenialIdentityUser user)
        {
            if (user == null)
            {
                return;
            }

            ImageUri = CreateImageUri(user);
            if (string.IsNullOrEmpty(ImageUri))
            {
                BackColor = user.Color;
                ForeColor = MaterialColorPicker.ColorIsLight(user.Color) ? "var(--xenial-darker-color)" : "var(--xenial-lighter-color)";
                ForeColorMudBlazor = MaterialColorPicker.ColorIsLight(user.Color) ? "var(--mud-palette-black)" : "var(--mud-palette-white)";
            }
            Inititals = user.Initials;
        }

        public string ImageUri { get; set; }
        public string BackColor { get; set; }
        public string ForeColor { get; set; }
        public string ForeColorMudBlazor { get; set; }
        public string Inititals { get; set; }

        private static string CreateImageUri(XenialIdentityUser user) => user.Picture != null && user.Picture.Length > 0 && !string.IsNullOrEmpty(user.PictureMimeType)
                ? $"data:{user.PictureMimeType};base64,{Convert.ToBase64String(user.Picture)}"
                : null;
    }
}
