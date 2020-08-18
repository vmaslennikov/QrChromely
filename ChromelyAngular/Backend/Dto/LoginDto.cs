using System;
using System.Collections.Generic;
using System.Text;

namespace ChromelyAngular.Backend.Dto
{
    public class LoginDto
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class PersonDto
    {
        public Guid? id { get; set; }
        public string fullname { get; set; }
        public string company { get; set; }
        public string position { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public bool hasdeclaration { get; set; }
        public bool haspcr { get; set; }
        public string status { get; set; }
        public string blockreason { get; set; }
        public string zone { get; set; }
        public bool deleted { get; set; }
    }

    public class PersonDeleteDto
    {
        public Guid[] ids { get; set; }
    }
}
