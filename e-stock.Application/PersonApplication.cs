using city.core.entities;
using crud.api.core;
using crud.api.core.enums;
using crud.api.core.fieldType;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace estock.Application
{
    public class PersonApplication
    {
        private readonly IService<Person<User>> _personService;
        private readonly MapperProfile<PersonModel, Person<User>> _personModelPorfile;
        private readonly PersonValidator<User> _personValidator;
        private readonly IRepository<City> _cityRepository;
        private readonly EasyRequest _easyRequest;

        public bool HasAccess { get; set; }
        public Guid AccountId { get; set; }
        public Uri AthenticateApi { get; set; }
        public string Token { get; set; }
        public string SystemSource { get; set; }

        public PersonApplication(IService<Person<User>> personService, MapperProfile<PersonModel, Person<User>> personModelPorfile, 
            PersonValidator<User> personValidator, IRepository<City> cityRepository, EasyRequest easyRequest)
        {
            this._personService = personService;
            this._personModelPorfile = personModelPorfile;
            this._personValidator = personValidator;
            this._cityRepository = cityRepository;
            this._easyRequest = easyRequest;
        }

        public IEnumerable<IHandleMessage> Save(PersonModel personModel)
        {
            try
            {
                if (this.HasAccess)
                {
                    var data = this._personService.GetData(x => x.Id.Equals(personModel.Id));
                    Person<User> person = null;

                    if (data.Any())
                    {
                        if (data.Count() > 1)
                        {
                            var manyRecords = new List<IHandleMessage>() { new HandleMessage("ManyRecordsFound", $"Hi, we have a problem. Many records found.", HandlesCode.InternalException) };
                            return manyRecords;
                        }

                        var entity = data.First();
                        person = this._personModelPorfile.Map(personModel, entity);
                        person.RegisterDate = entity.RegisterDate;
                        person.User.RegisterDate = entity.RegisterDate;
                    }
                    else
                    {
                        person = this._personModelPorfile.Map(personModel);

                        person.User.RegisterDate = person.RegisterDate;
                        person.User.Status = RecordStatus.Active;
                    }

                    if (person.User.AuthorizedSystems == null || !person.User.AuthorizedSystems.Any())
                    {
                        person.User.AuthorizedSystems = new List<AuthorizedSystem>() {
                                new AuthorizedSystem() { AccountId = this.AccountId, SystemName = this.SystemSource }
                            };
                    }

                    if (person.User.Roles == null || !person.User.Roles.Any())
                    {
                        person.User.Roles = new List<UserRule>() {
                                new UserRule() { Id = Guid.NewGuid(), LastChangeDate = DateTime.UtcNow, RegisterDate = DateTime.UtcNow, Roler = RulerType.BasicUser, RolerName = RulerType.BasicUser.ToString(), Status = RecordStatus.Active }
                            };
                    }

                    person.AccountId = this.AccountId;

                    var validate = person.Validate(this._personValidator);

                    if (validate.Any())
                    {
                        return validate;
                    }

                    var city = this._cityRepository.GetData(c => c.Id.Equals(personModel.PersonInfo.BirthCity));

                    if (!city.Any())
                    {
                        var invalidCity = new List<IHandleMessage>() { new HandleMessage("CityNotFound", $"Hi, we have problem to found your birth city.", HandlesCode.InternalException) };
                        return invalidCity;
                    }
                    else
                    {
                        person.BirthCity = city.First();
                    }

                    List<IHandleMessage> result = this._personService.SaveData(person).ToList();

                    if (!result.Any(x => x.Code != HandlesCode.Accepted) && !string.IsNullOrEmpty(personModel.UserInfo.UserPassword))
                    {
                        var userUri = new Uri(this.AthenticateApi, $"api/Account/SaveUser?password={personModel.UserInfo.UserPassword}");
                        var token = this.Token;

                        this._easyRequest.Token = this.Token;
                        this._easyRequest.SystemSource = this.SystemSource;
                        var response = this._easyRequest.Send(person.User, userUri.ToString());
                        var content = JsonConvert.DeserializeObject<List<HandleMessageAbs>>(response.Content.ReadAsStringAsync().Result);

                        if (content.Any(x => x.Code != HandlesCode.Accepted))
                        {
                            result.Add(new HandleMessage("UserNoSaved", "User data don't saved.", HandlesCode.EmptyField));
                        }
                    }

                    return result;
                }
                else
                {
                    var error = new List<IHandleMessage>()
                    {
                        new HandleMessage("Unauthorized", "Access danied.", HandlesCode.InvalidField)
                    };
                    return error;
                }
            }
            catch (Exception e)
            {
                List<IHandleMessage> handleMessage = new List<IHandleMessage>();
                handleMessage.Add(new HandleMessage(e));
                return handleMessage;
            }
        }
    }
}
