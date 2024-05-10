using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Repository;

namespace WebApplication1.Controllers; // Convention is to use "Controllers"

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _bookRepository;

    public BooksController(IBooksRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        if (!await _bookRepository.DoesBookExist(id))
            return NotFound($"Book with given ID - {id} doesn't exist");
        
        var bookDto = await _bookRepository.GetBook(id);
        return Ok(bookDto);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(NewBookPublish newBook)
    {
        foreach (var house in newBook.houses)
        {
            if (!await _bookRepository.DoesPublishingExist(house.Id))
                return NotFound($"Publishing house with ID - {house.Id} doesn't exist");
        }

        await _bookRepository.AddNewBook(newBook);
        return Created(Request.Path.Value ?? "api/books", newBook);
    }
}