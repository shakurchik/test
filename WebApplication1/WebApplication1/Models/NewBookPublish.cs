using System.Collections;

namespace WebApplication1.Models;

public class NewBookPublish
{
    

   
        public int Id { get; set; }
        public String Title { get; set; } = null!;
        public IEnumerable<AuthorDTO> authors { get; set; } = null!;
        public IEnumerable<GenreDTO> genres { get; set; } = null!;
        public IEnumerable<PublishingHouseDTO> houses { get; set; } = null!;
    
}