using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Data;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients
{
    public class DeleteClientModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteClientModel> logger;
        public DeleteClientModel(UnitOfWork unitOfWork, ILogger<DeleteClientModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ClientOutputModel
        {
            public string ClientId { get; set; }
            public string ClientName { get; set; }
        }

        public ClientOutputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int id)
        {
            var client = await unitOfWork.GetObjectByKeyAsync<XpoClient>(id);
            if (client == null)
            {
                StatusMessage = "Cannot find client";
                return Page();
            }
            if (client != null)
            {
                Input = new ClientOutputModel
                {
                    ClientId = client.ClientId,
                    ClientName = client.ClientName
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id)
        {
            if (ModelState.IsValid)
            {
                var client = await unitOfWork.GetObjectByKeyAsync<XpoClient>(id);
                if (client == null)
                {
                    StatusMessage = "Error: Cannot find client";
                    return Page();
                }

                try
                {
                    await unitOfWork.DeleteAsync(client);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/Clients");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error deleting Client with {ClientId} and {ClientName}", Input?.ClientId, Input?.ClientName);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.ClientName)}", "Client name must be unique");
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.ClientId)}", "Client id must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error deleting Client with {ClientId} and {ClientName}", Input?.ClientId, Input?.ClientName);
                    StatusMessage = $"Error deleting client: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
