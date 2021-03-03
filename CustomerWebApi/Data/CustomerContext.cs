using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CustomerWebApi.Models;

namespace CustomerWebApi.Data
{
	public class CustomerContext: DbContext
	{
		public CustomerContext(DbContextOptions<CustomerContext> options): base(options) { }
		
		// Tables
		public DbSet<Customer> Customers { get; set; }
		public DbSet<Address> Addresses { get; set; }
	}
}
