using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerWebApi.Models
{
	public class Customer
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string ID { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		[DataType(DataType.Date)]
		public DateTime Birthday { get; set; }
		public string Sex { get; set; }
		[DataType(DataType.PhoneNumber)]
		public string PhoneNumber { get; set; }
		[DataType(DataType.EmailAddress)]
		public string EmailAddress { get; set; }
	}

}
