using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerWebApi.Models
{
	public class Address
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string ID { get; set; }
		public string StreetName { get; set; }
		public int HouseNumber { get; set; }
		public int ZipCode { get; set; }
		public string City { get; set; }

		// Foreign key
		public string GetCustomer { get; set; }
	}
}
