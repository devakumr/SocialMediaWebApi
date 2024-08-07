using Domain.Auth.Account;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    public class DataContext:DbContext
    {

        public DataContext(DbContextOptions options) : base(options) 
        { 
        
        }

        public DbSet<UserModel> Users { get; set; }

    }
}
