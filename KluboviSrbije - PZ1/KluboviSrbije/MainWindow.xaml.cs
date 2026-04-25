using KluboviSrbije.Modeli;
using KluboviSrbije.Windows;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace KluboviSrbije
{
    public partial class MainWindow : Window
    {
        private const string UsersFile = "users.xml";
        private List<User> _users;
        private NotificationManager notificationManager;
        private string nameFocus = "Unesite korisnicko ime";

        public MainWindow()
        {
            InitializeComponent();
            UcitajIliKreirajKorisnike();
            notificationManager = new NotificationManager();

            UsernameBox.Text = nameFocus;
            UsernameBox.Foreground = Brushes.Gray;
        }

        private void UcitajIliKreirajKorisnike()
        {
            if (!File.Exists(UsersFile))
            {
                _users = new List<User>
                {
                    new User { Username = "admin",   Password = "admin123",   Role = UserRole.Admin   },
                    new User { Username = "visitor", Password = "visitor123", Role = UserRole.Visitor }
                };
                SacuvajKorisnike();
            }
            else
            {
                var serializer = new XmlSerializer(typeof(List<User>));
                using var stream = File.OpenRead(UsersFile);
                _users = (List<User>)serializer.Deserialize(stream);
            }
        }

        public void ShowToastNotification(ToastNotification toastNotification)
        {
            notificationManager.Show(toastNotification.Title, toastNotification.Message, toastNotification.Type, "WindowNotificationArea");
        }

        private void SacuvajKorisnike()
        {
            var serializer = new XmlSerializer(typeof(List<User>));
            using var stream = File.Create(UsersFile);
            serializer.Serialize(stream, _users);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            UsernameError.Text = "";
            UsernameError.Visibility = Visibility.Collapsed;
            PasswordError.Text = "";
            PasswordError.Visibility = Visibility.Collapsed;
            UsernameBox.ClearValue(Border.BorderBrushProperty);
            PasswordBox.ClearValue(Border.BorderBrushProperty);

            if (!ValidateFormData())
                return;

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                && u.Password == password);

            if (user != null)
            {
                new TableWindow(user).Show();
                MessageBox.Show($"Dobrodošli, {user.Username}! Uspesna prijava!", "Uspesna prijava", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                var userByUsername = _users
                    .FirstOrDefault(u =>
                         u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (userByUsername == null)
                {
                    UsernameError.Text = "Nepostojeće korisničko ime.";
                    UsernameError.Visibility = Visibility.Visible;
                    UsernameBox.BorderBrush = Brushes.Red;
                    ShowToastNotification(new ToastNotification("Netacno ime!","Netacno ime!",NotificationType.Error));
                    return;
                }

                if (userByUsername.Password != password)
                {
                    PasswordError.Text = "Pogrešna lozinka.";
                    PasswordError.Visibility = Visibility.Visible;
                    PasswordBox.BorderBrush = Brushes.Red;
                    ShowToastNotification(new ToastNotification("Nepravilna lozinka!", "Nepravilna lozinka!", NotificationType.Error));
                    return;
                }
            }
        }

        private bool ValidateFormData()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(UsernameBox.Text) || UsernameBox.Text.Trim().Equals(nameFocus))
            {
                ShowToastNotification(
                    new ToastNotification(
                        "Nepravilno korisnicko ime",
                        "Polje za unos korisnickog imena ne sme biti prazno!",
                        NotificationType.Error));

                UsernameError.Text = "Polje ne sme biti prazno!";
                UsernameError.Visibility = Visibility.Visible;
                UsernameBox.BorderBrush = Brushes.Red;
                isValid = false;
            }

            if (PasswordBox.SecurePassword.Length == 0)
            {
                ShowToastNotification(
                    new ToastNotification(
                        "Netacna lozinka",
                        "Polje za unos lozinke ne sme biti prazno!",
                        NotificationType.Error));

                PasswordError.Text = "Polje ne sme biti prazno!";
                PasswordError.Visibility = Visibility.Visible;
                PasswordBox.BorderBrush = Brushes.Red;
                isValid = false;
            }

            return isValid;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite izaći?", "Izlaz", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SacuvajKorisnike();
                Close();
            }
        }

        private void NameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text.Trim().Equals(nameFocus))
            {
                UsernameBox.Text = string.Empty;
                UsernameBox.Foreground = Brushes.Black;
            }

        }

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text.Trim().Equals(string.Empty))
            {
                UsernameBox.Text = nameFocus;
                var bc = new BrushConverter();
                UsernameBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

    }
}
