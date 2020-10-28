using crud.api.core;
using crud.api.core.enums;
using crud.api.core.interfaces;
using crud.api.dto.Person;
using estock.Application;
using Jwt.Simplify.Core.Enums;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace e.stock.api.Controllers
{
    [EnableCors(Program.AllowSpecificOrigins)]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PersonController: Controller
    {
        private readonly PersonApplication _personApplication;

        public PersonController(PersonApplication personApplication)
        {
            this._personApplication = personApplication;
        }

        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public ActionResult<List<IHandleMessage>> Save(PersonModel personModel)
        {
            try
            {
                var uri = new Uri(Program.AthenticateApi, "api/Authorized");
                var auth = Request.Authenticated(uri.ToString());

                this._personApplication.HasAccess = auth.HasAccess(RulerType.EditorUser, "Person");
                this._personApplication.AccountId = auth.GetAccountId(Request.GetSystemName());
                this._personApplication.AthenticateApi = Program.AthenticateApi;
                this._personApplication.Token = Request.Headers["Authorization"].ToString().Replace("Bearer", string.Empty).Trim();
                this._personApplication.SystemSource = Request.GetSystemName();

                var result = this._personApplication.Save(personModel);

                return StatusCode(result.Any(x => x.Code != HandlesCode.Accepted) ? (int)HttpStatusCode.BadRequest : (int)HttpStatusCode.Accepted, result);
            }
            catch (UnauthorizedAccessException e)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new List<IHandleMessage>() { new HandleMessage(e) });
            }
            catch(Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new List<IHandleMessage>() { new HandleMessage(e) });
            }
        }
    }
}
