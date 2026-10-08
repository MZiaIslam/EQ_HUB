using EHUB.Models.Administration;
using EHUB.Models.HRManagement;

namespace EHUB.Models.Tickets
{

	public class Ticket
	{
		public int ticketid { get; set; }
		public string subject { get; set; }
		public int assignto { get; set; }
		public string details { get; set; }
		public string stat { get; set; }
		public int CreatedBy { get; set; }
		public DateTime Created { get; set; }
        public string cls { get; set; }
    }
	public class TicketsDetail
	{
		public int ticketid { get; set; }
		public string details { get; set; }
		public int CreatedBy { get; set; }
		public DateTime Created { get; set; }
		public int id { get; set; }
	}
	public class TicketsInfo
	{
		public int ticketid { get; set; }
		public string details { get; set; }
		public int empcode { get; set; }
		public int empid { get; set; }
		public DateTime Created { get; set; }
		public string empname { get; set; }
		public int assignto { get; set; }
        public string stat { get; set; }
        public string cls { get; set; }
    }
	public class TInfo
	{
		
		public string? details { get; set; }
		public string? subject { get; set; }
        public int? depart { get; set; }
        public int? ticketid { get; set; }
		public string? message { get; set; } = "";
        public int? empid { get; set; }
        public string? stat { get; set; }

    }
	public class TModel
	{

		public List<Ticket>? Ticket { get; set; }
		public List<StaffData>? emps { get; set; }
	public List<Libs>? libs { get; set; }
	}
}
