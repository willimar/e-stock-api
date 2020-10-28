using crud.api.core;
using crud.api.core.enums;
using crud.api.core.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace estock.Application
{
    public class HandleMessageAbs: IHandleMessage
    {
        public HandleMessageAbs()
        {

        }

        public string MessageType { get; set; }

        public string Message { get; set; }

        public HandlesCode Code { get; set; }

        public List<string> StackTrace { get; set; }
    }
}
