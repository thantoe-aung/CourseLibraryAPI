
using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet(Name = "GetAuthors")]
    [HttpHead]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors([FromQuery] AuthorResourceParameter authorResourceParameter)
    {
        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourceParameter);

        var previousPageLink = authorsFromRepo.HasPrevious ? CreateAuthorResourceUri(authorResourceParameter, ResourceUriType.PreviousPage) : null;
        var nextPageLink = authorsFromRepo.HasPrevious ? CreateAuthorResourceUri(authorResourceParameter, ResourceUriType.NextPage) : null;

        var paginationData = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages,
            previousPageLink = previousPageLink,
            nextPageLink = nextPageLink
        };

        Response.Headers.Add("X-Pagination",JsonSerializer.Serialize(paginationData));

        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }

    private string? CreateAuthorResourceUri(AuthorResourceParameter author, ResourceUriType type)
    {
        switch (type)
        {
            case ResourceUriType.PreviousPage:
                return Url.Link("GetAuthors", new
                {
                    pageNumber = author.PageNumber -1,
                    pageSize = author.PageSize,
                    mainCategory = author.mainCategory,
                    searchQuery = author.searchQuery
                });
               
            case ResourceUriType.NextPage:
                return Url.Link("GetAuthors", new
                {
                    pageNumber = author.PageNumber + 1,
                    pageSize = author.PageSize,
                    mainCategory = author.mainCategory,
                    searchQuery = author.searchQuery
                });               
            default:
                return Url.Link("GetAuthors", new
                {
                    pageNumber = author.PageNumber,
                    pageSize = author.PageSize,
                    mainCategory = author.mainCategory,
                    searchQuery = author.searchQuery
                });
        }
    }

    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
    {
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }
}
