using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMoviesServices _moviesServices;
        private readonly IGenresService _genresService;

        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowdPosterSize = 1048576;

        public MoviesController(ApplicationDbContext context, IMoviesServices moviesServices, IGenresService genresService, IMapper mapper)
        {
            _moviesServices = moviesServices;
            _genresService = genresService;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _moviesServices.GetAll();
            
            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
            
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _moviesServices.GetById(id);

            if(movie == null)
                return NotFound();

            var dto = _mapper.Map<MovieDetailsDto>(movie);

            return Ok(dto);
        }
        
        [HttpGet("GetbyGenreId")]
        public async Task<IActionResult> getbygenreidasync(byte genraId)
        {
            var movies = await _moviesServices.GetAll(genraId);

            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);

            return Ok(data);
        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm]MovieDto dto)
        {
            if (dto.Poster == null)
                return BadRequest("Poster is Required!");

            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed");

            if (dto.Poster.Length > _maxAllowdPosterSize)
                return BadRequest("Max Allowed Size For Poster Is 1MB");

            var isValidGenre = await _genresService.IsvalidGenre(dto.GenreId);

            if(!isValidGenre)
            return BadRequest("InValid Gener ID!");

            using var dataStream = new MemoryStream();

            await dto.Poster.CopyToAsync(dataStream);
            
            var movie = _mapper.Map<Movie>(dto);
            movie.Poster = dataStream.ToArray();

           _moviesServices.Add(movie);

            return Ok(movie);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto dto)
        {
            var movie = await _moviesServices.GetById(id);

            if (movie == null)
                return NotFound($"No Movie Was Found With ID {id}");


            var isValidGenre = await _genresService.IsvalidGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("InValid Gener ID!");

            if (dto.Poster != null)
            {
                if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are allowed");

                if (dto.Poster.Length > _maxAllowdPosterSize)
                    return BadRequest("Max Allowed Size For Poster Is 1MB");

                using var dataStream = new MemoryStream();

                await dto.Poster.CopyToAsync(dataStream);

                movie.Poster = dataStream.ToArray();
            }

            movie.Title = dto.Title;
            movie.GenreId = dto.GenreId;
            movie.Year = dto.Year;
            movie.Storeline = dto.Storeline;
            movie.Rate = dto.Rate;

            _moviesServices.Update(movie);

            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _moviesServices.GetById(id);

            if (movie == null)
                return NotFound($"No Movie Was Found With ID {id}");

            _moviesServices.Delete(movie);

            return Ok(movie);
            
        }
    }
}
