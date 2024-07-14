
using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;
    private readonly IPropertyCheckerService _propertyCheckerService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper, IPropertyCheckerService propertyCheckerService, ProblemDetailsFactory problemDetailsFactory)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));

    }

    [HttpGet(Name = "GetAuthors")]
    [HttpHead]
    public async Task<ActionResult<IActionResult>> GetAuthors([FromQuery] AuthorResourceParameter authorResourceParameter)
    {

        if (!_propertyCheckerService.CheckPropertyExist<AuthorDto>(authorResourceParameter.Fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
                statusCode : 400,
                detail : $"Property not exist"
                ));
        }
       
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
        };



        Response.Headers.Add("X-Pagination",JsonSerializer.Serialize(paginationData));

        var links = CreateLinksForAuthor(authorResourceParameter,authorsFromRepo.HasNext,authorsFromRepo.HasPrevious);

        var shapedData = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo).ShapeData(authorResourceParameter.Fields);

        var authorWithLinks = shapedData.Select(x =>
        {
            var temp = x as IDictionary<string, object?>;
            var authorLinks = CreateLinkForAuthor((Guid)temp["Id"], null);
            temp.Add("Links", authorLinks);
            return temp;
        }
        );

        var collectionResource = new
        {
            value = authorWithLinks,
            links = links
        };

        return Ok(collectionResource);
        //return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo).ShapeData(authorResourceParameter.Fields));
    }


    [Produces("application/json","application/vnd.marvin.hateoas+json")]
    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(Guid authorId,string? fields, [FromHeader(Name = "Accept")]string? mediaType)
    {

        if(!MediaTypeHeaderValue.TryParse(mediaType, out var contentType))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
                statusCode: 400,
                detail: $"Accept header value is invalid"
                ));
        }

        if (!_propertyCheckerService.CheckPropertyExist<AuthorDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
                statusCode: 400,
                detail: $"Property not exist"
                ));
        }

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        if(contentType.MediaType == "application/vnd.marvin.hateoas+json")
        {
            var links = CreateLinkForAuthor(authorId, fields);

            var linkResult = _mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields) as IDictionary<string, object?>;
            linkResult.Add("links", links);
            return Ok(linkResult);
        }

        return Ok(_mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields));

        // return author
        //return Ok(_mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields));
    }

    [HttpPost(Name = "CreateAuthor")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        var links= CreateLinkForAuthor(authorToReturn.Id,null);

        var linkToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object?>;
        linkToReturn.Add("links", links);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            linkToReturn);
    }

    private IEnumerable<LinkDto> CreateLinkForAuthor(Guid authorId,string? fields)
    {
        var links = new List<LinkDto>();
        if (string.IsNullOrWhiteSpace(fields))
        {
            links.Add(
                new(Url.Link("GetAuthor", new { authorId }),
                "self",
                "Get"));
        }
        else
        {
            links.Add(
                new(Url.Link("GetAuthor",new {authorId,fields}),
                "self",
                "Get"));
        }

        links.Add(
                new(Url.Link("CreateCourseForAuthor", new { authorId }),
                "create_course_for_author",
                "POST"));

        return links;
    }

    private string? CreateAuthorResourceUri(AuthorResourceParameter author, ResourceUriType type)
    {
        switch (type)
        {
            case ResourceUriType.PreviousPage:
                return Url.Link("GetAuthors", new
                {
                    fields = author.Fields,
                    orderBy = author.OrderBy,
                    pageNumber = author.PageNumber - 1,
                    pageSize = author.PageSize,
                    mainCategory = author.mainCategory,
                    searchQuery = author.searchQuery
                });

            case ResourceUriType.NextPage:
                return Url.Link("GetAuthors", new
                {
                    fields = author.Fields,
                    orderBy = author.OrderBy,
                    pageNumber = author.PageNumber + 1,
                    pageSize = author.PageSize,
                    mainCategory = author.mainCategory,
                    searchQuery = author.searchQuery
                });      
            case ResourceUriType.Current:               
            default:
                return Url.Link("GetAuthors", new
                {
                    fields = author.Fields,
                    orderBy = author.OrderBy,
                    pageNumber = author.PageNumber,
                    pageSize = author.PageSize,
                    mainCategory = author.mainCategory,
                    searchQuery = author.searchQuery
                });
        }
    }

    private IEnumerable<LinkDto> CreateLinksForAuthor(AuthorResourceParameter author,bool hasNext,bool hasPrevious)
    {
        var links = new List<LinkDto>();
       

        links.Add(
                new(CreateAuthorResourceUri(author,ResourceUriType.Current),
                "SELF",
                "GET"));

        if (hasNext)
        {
            links.Add(
                new(CreateAuthorResourceUri(author, ResourceUriType.NextPage),
                "Next Page",
                "GET"));
        }
        if (hasPrevious)
        {
            links.Add(
                new(CreateAuthorResourceUri(author, ResourceUriType.PreviousPage),
                "Previous Page",
                "GET"));
        }

        return links;
    }
}
