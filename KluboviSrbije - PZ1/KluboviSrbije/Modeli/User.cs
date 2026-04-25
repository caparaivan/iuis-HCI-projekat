using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KluboviSrbije.Modeli
{
    public enum UserRole
    {
        Admin,
        Visitor
    }
    [Serializable]
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }

    }
}
