using Microsoft.EntityFrameworkCore;
using System;

namespace t_project.Database
{
    public class Config
    {
        public static readonly string connection = "server=localhost;port=3306;uid=root;pwd=;database=bd02;";
        public static readonly MySqlServerVersion version = new MySqlServerVersion(new Version(8, 0, 11));
    }
}
