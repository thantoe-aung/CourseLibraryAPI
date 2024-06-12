using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authorcollections",Name = "GetAuthorCollection")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet("({authorIds})")]
        public async Task<ActionResult<IEnumerable<AuthorForCreationDto>>> GetAuthorCollection(
            [ModelBinder(BinderType= typeof(ArrayModelBinder))]
            [FromRoute]IEnumerable<Guid> authorIds)
        {
            var authorEntities = await _courseLibraryRepository.GetAuthorsAsync(authorIds);
            if(authorEntities.Count() != authorIds.Count())
            {
                return NotFound();
            }

            var result = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollection(IEnumerable<AuthorForCreationDto> authorCollection)
        {
            var mappedCollection = _mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach(var author in mappedCollection)
            {
                _courseLibraryRepository.AddAuthor(author);
            }
            await _courseLibraryRepository.SaveAsync();

            var result = _mapper.Map<IEnumerable<AuthorDto>>(mappedCollection);

            var resultString = string.Join(",", result.Select(x => x.Id));

            return CreatedAtRoute("GetAuthorCollection",new {authorIds = resultString},result);

        }
    }
}
