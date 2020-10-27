using city.core.entities;
using crud.api.core;
using crud.api.core.enums;
using crud.api.core.interfaces;
using crud.api.core.mappers;
using crud.api.core.repositories;
using crud.api.core.services;
using crud.api.dto.Person;
using crud.api.register.entities.registers;
using crud.api.register.validations.register;
using Easy.Navigator;
using Jwt.Simplify.Core.Entities;
using Jwt.Simplify.Core.Enums;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace e.stock.api.Controllers
{
    [EnableCors(Program.AllowSpecificOrigins)]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PersonController: Controller
    {
        private readonly IService<Person<User>> _personService;
        private readonly MapperProfile<PersonModel, Person<User>> _personModelPorfile;
        private readonly IRepository<City> _cityRepository;
        private readonly PersonValidator<User> _personValidator;
        private readonly EasyRequest _easyRequest;

        public PersonController(IService<Person<User>> personService, MapperProfile<PersonModel, Person<User>> personModelPorfile, 
            IRepository<City> cityRepository, PersonValidator<User> personValidator, EasyRequest easyRequest)
        {
            this._personService = personService;
            this._personModelPorfile = personModelPorfile;
            this._cityRepository = cityRepository;
            this._personValidator = personValidator;
            this._easyRequest = easyRequest;
        }

        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public ActionResult<List<IHandleMessage>> Save(PersonModel personModel)
        {
            var uri = new Uri(Program.AthenticateApi, "api/Authorized");
            var auth = Request.Authenticated(uri.ToString());

            // Check whether is a autheticated user
            if (auth.HasAccess(RulerType.EditorUser, "Person"))
            {
                var data = this._personService.GetData(x => x.Id.Equals(personModel.Id));
                Person<User> person = null;

                if (data.Any())
                {
                    if (data.Count() > 1)
                    {
                        var manyRecords = new List<IHandleMessage>() { new HandleMessage("ManyRecordsFound", $"{auth.GetUserName()}, we have a problem. Many records found.", HandlesCode.InternalException) };
                        return StatusCode((int)HttpStatusCode.InternalServerError, manyRecords); 
                    }

                    person = this._personModelPorfile.Map(personModel, data.First());
                }
                else
                {
                    person = this._personModelPorfile.Map(personModel);
                }

                person.AccountId = auth.GetAccountId(Request.GetSystemName());

                var validate = person.Validate(this._personValidator);

                if (validate.Any())
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, validate);
                }

                var city = this._cityRepository.GetData(c => c.Id.Equals(personModel.PersonInfo.BirthCity));

                if (!city.Any())
                {
                    var invalidCity = new List<IHandleMessage>() { new HandleMessage("CityNotFound", $"{auth.GetUserName()}, we have problem to found your birth city.", HandlesCode.InternalException) };
                    return StatusCode((int)HttpStatusCode.InternalServerError, invalidCity);
                }
                else
                {
                    person.BirthCity = city.First();
                }

                var result = this._personService.SaveData(person);

                if (!result.Any(x => x.Code != HandlesCode.Accepted) && !string.IsNullOrEmpty(personModel.UserInfo.UserPassword))
                {
                    var userUri = new Uri(Program.AthenticateApi, "api/User");
                    var token = Request.Headers["Authorization"].ToString().Replace("Bearer", string.Empty).Trim();

                    this._easyRequest.Token = token;
                    this._easyRequest.SystemSource = Request.GetSystemName();
                    var response = this._easyRequest.Send(person.User, userUri.ToString());
                    var content = JsonConvert.DeserializeObject<List<HandleMessageAbs>>(response.Content.ReadAsStringAsync().Result);

                    if (content.Any(x => x.Code != HandlesCode.Accepted))
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, content);
                    }

                    return StatusCode((int)HttpStatusCode.Accepted, content);
                }

                return StatusCode((int)HttpStatusCode.Accepted, result);
            }
            else
            {
                var error = new List<IHandleMessage>()
                {
                    new HandleMessage("Unauthorized", "Access danied.", HandlesCode.InvalidField)
                };
                return StatusCode((int)HttpStatusCode.Unauthorized, error);
            }
        }
    }
}
