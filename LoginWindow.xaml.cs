using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _301428777_GuertaLigo__Lab_2
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AmazonDynamoDBClient dynamoClient;

        public LoginWindow()
        {
            InitializeComponent();

            dynamoClient = AWSHelper.GetDynamoDBClient();
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both Email and Password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //try to login in matching the credentials in the table 
            bool loginSuccess = await CheckLogin(email, password);

            if (loginSuccess)
            {
                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Hide();
                MainWindow mainWindow = new MainWindow(email);
                mainWindow.Show();

                mainWindow.Closed += (s, args) =>
                {
                    this.Show(); // show the login window again
                };
            }
            else
            {
                MessageBox.Show("Invalid Email or Password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //check email and password in the table
        private async Task<bool> CheckLogin(string email, string password)
        {
            try
            {
                GetItemRequest request = new GetItemRequest
                {
                    TableName = "Bookshelf",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "email", new AttributeValue { S = email } }  //partition key: email
                    }
                };

                var response = await dynamoClient.GetItemAsync(request);

                //login fails if no items found
                if (response.Item == null || response.Item.Count == 0)
                    return false; 

                //get the password
                string storedPassword = response.Item.ContainsKey("password") ? response.Item["password"].S : "";

                return storedPassword == password;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accessing DynamoDB: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown(); //end application when login page closes
        }
    }
}
