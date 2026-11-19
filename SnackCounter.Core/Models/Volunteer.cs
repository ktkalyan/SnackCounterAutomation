
namespace SnackCounter.Core.Models
{
    public class Volunteer
    {
        public int VolunteerId { get; set; }

        public string EmployeeId { get; set; }

        public string Name { get; set; }

        public string HomeOfficeCode { get; set; }

        public string CurrentOfficeCode { get; set; }

        public bool IsAvailable { get; set; }
    }
}
