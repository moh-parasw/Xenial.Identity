﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources.Secrets
{
    public class EditApiResourceSecretModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditApiResourceSecretModel> logger;
        public EditApiResourceSecretModel(UnitOfWork unitOfWork, ILogger<EditApiResourceSecretModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ApiResourceSecretInputModel
        {
            [Required]
            public string Key { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class ApiResourceMappingConfiguration : Profile
        {
            public ApiResourceMappingConfiguration()
                => CreateMap<ApiResourceSecretInputModel, XpoApiResourceSecret>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ApiResourceSecretInputModel Input { get; set; } = new ApiResourceSecretInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int resourceId, [FromRoute] int id)
        {
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(resourceId);
            if (apiResource == null)
            {
                StatusMessage = "Error: cannot find api resource";
                return Page();
            }

            var apiResourceSecret = apiResource.Secrets.FirstOrDefault(secret => secret.Id == id);
            if (apiResourceSecret == null)
            {
                StatusMessage = "Error: cannot find api resource secret";
                return Page();
            }

            Input = Mapper.Map<ApiResourceSecretInputModel>(apiResourceSecret);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int resourceId, [FromRoute] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(resourceId);
                    if (apiResource == null)
                    {
                        StatusMessage = "Error: cannot find api resource";
                        return Page();
                    }

                    var apiResourceSecret = apiResource.Secrets.FirstOrDefault(secret => secret.Id == id);

                    if (apiResourceSecret == null)
                    {
                        StatusMessage = "Error: cannot find api resource secret";
                        return Page();
                    }

                    Mapper.Map(Input, apiResourceSecret);

                    await unitOfWork.SaveAsync(apiResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/ApiResources/Edit/{resourceId}?SelectedPage=Secrets");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving ApiResource with {resourceId}", resourceId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving ApiResource with {resourceId}", resourceId);
                    StatusMessage = $"Error saving api resource: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
