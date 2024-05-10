using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Repository;

public class BooksRepository : IBooksRepository
{
    private readonly string _connectionString;

    public BooksRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Database connection string 'Default' is not initialized.");
        }
    }

    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT COUNT(1) FROM Books WHERE PK = @ID";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@ID", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return (int)result > 0;
    }

    public async Task<BooksDTO> GetBook(int id)
    {
        var query = @"
            SELECT 
                Books.pk AS BookID,
                Books.Title AS BookName,
                Piblishing_houses.Ipk AS PubID,
                Piblishing_houses.Name AS PubName,
                Piblishing_houses.OwnerFirstName AS OwnerFirst,
                Piblishing_houses.OwnerLastName AS OwnerLast
            FROM Books
            JOIN BooksEditions ON BooksEditions.BookID = Books.PK
            JOIN Piblishing_houses ON Piblishing_houses.PK = BooksEditions.Piblishing_houses_PK
            WHERE Books.ID = @ID";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        BooksDTO booksDto = null;
        if (await reader.ReadAsync())
        {
            booksDto = new BooksDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("BookID")),
                Title = reader.GetString(reader.GetOrdinal("BookName")),
                houses  = new List<PublishingHouseDTO>
                {
                    new PublishingHouseDTO
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("PubID")),
                        name = reader.GetString(reader.GetOrdinal("PubName")),
                        owner_last =  reader.GetString(reader.GetOrdinal("OwnerFirst")),
                        owner_first =  reader.GetString(reader.GetOrdinal("OwnerLast"))
                    }
                }
            };
        }

        return booksDto ?? throw new KeyNotFoundException($"No book found with ID {id}");
    }

   

    public async Task<bool> DoesPublishingExist(int Id)
    {
        var query = "SELECT COUNT(1) FROM Piblishing_houses WHERE PK = @ID";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ID", Id);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return (int)result > 0;
    }
    
        public async Task AddNewBook(NewBookPublish newBookPublish)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                   
                    SqlCommand command = new SqlCommand("INSERT INTO Books (Title) VALUES (@Title); SELECT SCOPE_IDENTITY();", connection, transaction);
                    command.Parameters.AddWithValue("@Title", newBookPublish.Title);

                    var bookId = (decimal)await command.ExecuteScalarAsync();

                 
                    if (newBookPublish.authors != null)
                    {
                        foreach (var author in newBookPublish.authors)
                        {
                            command = new SqlCommand("INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @AuthorId)", connection, transaction);
                            command.Parameters.AddWithValue("@BookId", (int)bookId);
                            command.Parameters.AddWithValue("@AuthorId", author.Id);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    if (newBookPublish.genres != null)
                    {
                        foreach (var genre in newBookPublish.genres)
                        {
                            command = new SqlCommand("INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @GenreId)", connection, transaction);
                            command.Parameters.AddWithValue("@BookId", (int)bookId);
                            command.Parameters.AddWithValue("@GenreId", genre.Id);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    if (newBookPublish.houses != null)
                    {
                        foreach (var house in newBookPublish.houses)
                        {
                            command = new SqlCommand("INSERT INTO BookPublishingHouses (BookId, PublishingHouseId) VALUES (@BookId, @PublishingHouseId)", connection, transaction);
                            command.Parameters.AddWithValue("@BookId", (int)bookId);
                            command.Parameters.AddWithValue("@PublishingHouseId", house.Id);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch
                    {
                        throw new Exception("An error occurred during the transaction rollback.", ex);
                    }
                    throw; 
                }
            }
        }
    }

   
