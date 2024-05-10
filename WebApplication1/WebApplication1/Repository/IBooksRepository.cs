using WebApplication1.Models;

namespace WebApplication1.Repository;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
    Task<BooksDTO> GetBook(int id);
    Task AddNewBook(NewBookPublish newBookPublish);
    Task<bool> DoesPublishingExist(int id);
}