using System.ComponentModel.DataAnnotations;

namespace t_project.Models
{
    public class Equipment
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] ImagePath { get; set; }
        public string EquipmentNumber { get; set; }
        public string RoomNumber { get; set; }
        public string ResponsibleUser { get; set; }
        public string TemporaryUser { get; set; }
        public decimal Price { get; set; }
        public string Direction { get; set; }
        public string Status { get; set; }
        public string Model { get; set; }
        public string Comment { get; set; }
    }
}
