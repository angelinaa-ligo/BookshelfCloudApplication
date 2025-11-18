using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace _301428777_GuertaLigo__Lab_2
{
    public partial class MainWindow : Window
    {
        public string CurrentUserName { get; set; }
        public ObservableCollection<BookItem> Books { get; set; } = new();

        private readonly AmazonDynamoDBClient dynamoClient;
        private readonly IAmazonS3 s3Client;
        private const string TableName = "Bookshelf";

        public MainWindow(string email)
        {
            InitializeComponent();
            DataContext = this;
           
            CurrentUserName = email;
            s3Client = AWSHelper.GetS3Client();
            dynamoClient = AWSHelper.GetDynamoDBClient();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //get all books for the user
                List<BookItem> books = await GetBooksForUserAsync(CurrentUserName);
                Books.Clear();

                //add the book in the collection
                foreach (var b in books)
                    Books.Add(b);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading books: " + ex.Message);
            }
        }

        //get the books for the given user
        private async Task<List<BookItem>> GetBooksForUserAsync(string userEmail)
        {

            //request to get the user's books by email
            GetItemRequest request = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "email", new AttributeValue { S = userEmail } }
                }
            };

            GetItemResponse response = await dynamoClient.GetItemAsync(request);

            //if no books, get empty list
            if (!response.IsItemSet || !response.Item.ContainsKey("books"))
            {
                return new List<BookItem>();
            }

            //get book list (list of maps)
            List<AttributeValue> bookList = response.Item["books"].L;

            //convert to objects in BookItam
            return bookList
                .Select(b => new BookItem
                {
                    Email = userEmail,
                    Isbn = b.M.ContainsKey("isbn") ? b.M["isbn"].S : "",
                    Title = b.M.ContainsKey("title") ? b.M["title"].S : "",
                    Author = b.M.ContainsKey("author") ? b.M["author"].S : "",
                    BookmarkPage = b.M.ContainsKey("bookmarkPage") ? int.Parse(b.M["bookmarkPage"].N) : 0,
                    BookmarkTime = b.M.ContainsKey("bookmarkTime") ? b.M["bookmarkTime"].S : "",
                    S3Key = b.M.ContainsKey("s3Key") ? b.M["s3Key"].S : ""
                })
                .OrderByDescending(b => b.BookmarkTime) //latest bookmark time
                .ToList();
        }

        private void Book_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is BookItem book)
            {
                this.Hide();

                //open reader window
                ReaderWindow reader = new ReaderWindow(book, async updatedBook =>
                {
                    await SaveBookmarkAsync(updatedBook);
                    await RefreshBooksAsync();
                });
                reader.ShowDialog();

                this.Show();
            }
        }

        private async Task SaveBookmarkAsync(BookItem book)
        {
            GetItemRequest getRequest = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "email", new AttributeValue { S = book.Email } }
                }
            };

            GetItemResponse res = await dynamoClient.GetItemAsync(getRequest);
            List<AttributeValue> bookList = res.Item["books"].L;

            // find the matching book by ISBN
            var match = bookList.FirstOrDefault(b => b.M["isbn"].S == book.Isbn);
            if (match != null)
            {
                //update bookmark time and page
                match.M["bookmarkPage"] = new AttributeValue { N = book.BookmarkPage.ToString() };
                match.M["bookmarkTime"] = new AttributeValue { S = book.BookmarkTime };
            }

            //update 
            UpdateItemRequest updateReq = new UpdateItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "email", new AttributeValue { S = book.Email } }
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#B", "books" }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":booksVal", new AttributeValue { L = bookList } }
                },
                UpdateExpression = "SET #B = :booksVal" //update the list
            };

            await dynamoClient.UpdateItemAsync(updateReq);
        }

        //update the list and refresh the UI
        private async Task RefreshBooksAsync()
        {
            List<BookItem> books = await GetBooksForUserAsync(CurrentUserName);
            Books.Clear();
            foreach (var b in books)
                Books.Add(b);
        }
    }
}
