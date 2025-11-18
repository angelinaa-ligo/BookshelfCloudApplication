using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Windows;

namespace _301428777_GuertaLigo__Lab_2
{
    public partial class ReaderWindow : Window
    {
        private readonly BookItem book1;
        private readonly IAmazonS3 s3Client;
        private readonly Action<BookItem> onClose1;

        public ReaderWindow(BookItem book, Action<BookItem> onClose)
        {
            InitializeComponent();
            book1 = book;
            s3Client = AWSHelper.GetS3Client();
            onClose1 = onClose;

            LoadPdfFromS3(); 
        }

        private async void LoadPdfFromS3()
        {
            try
            {
                // request to get PDF directly from S3 bucket
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = "mybookshelf-bucket",
                    Key = book1.S3Key
                };

                //GetObject request asynchronously
                using GetObjectResponse response = await s3Client.GetObjectAsync(request); 

                //get the pdf to local storage
                MemoryStream documentStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(documentStream);

                //reset stream position to beggining
                documentStream.Position = 0; 

                // show in Syncfusion PDF viewer
                PdfViewer.Load(documentStream);

                // save the bookmark
                if (book1.BookmarkPage > 0)
                    PdfViewer.GotoPage(book1.BookmarkPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading PDF: " + ex.Message);
            }
        }

        private void BtnBookmark_Click(object sender, RoutedEventArgs e)
        {
            SaveBookmark();
            MessageBox.Show($"Bookmarked page {book1.BookmarkPage}");
            this.Hide();
        }
        private void SaveBookmark()
        {
            book1.BookmarkPage = PdfViewer.CurrentPage;   // save current page
            book1.BookmarkTime = DateTime.UtcNow.ToString("s"); // save time
            onClose1?.Invoke(book1); // pass updated book back to MainWindow
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveBookmark();
            base.OnClosed(e);
        }
    }
}
