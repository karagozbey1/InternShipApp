namespace Core.Entity
{
    public class University
    {
        public int Id { get; set; }               
        public string Name { get; set; } = string.Empty;
        public byte Status { get; set; } = 1;         

       
        public int ProvinceId { get; set; }
        public Province? Province { get; set; }

        public ICollection<User>? Users { get; set; }
    }
}
