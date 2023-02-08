namespace MoviesApi.Dtos
{
    public class MovieDto
    {
        [MaxLength(250)]
        public string? Title { get; set; }

        public int Year { get; set; }

        public double Rate { get; set; }

        [MaxLength(2500), Required]
        public string? Storeline { get; set; }

        [Required]
        public IFormFile? Poster { get; set; }

        public byte GenreId { get; set; }
    }
}
