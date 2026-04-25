using KluboviSrbije.Modeli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace KluboviSrbije.Windows
{
    public partial class ViewWindow : Window
    {
        public User savedUser = new User();

        public ViewWindow(User user, KluboviKlasa klubovi)
        {
            InitializeComponent();
            savedUser = user;
            this.DataContext = savedUser;
            NameLabel.Content = klubovi.RtfFilePath;

            string imageFile = klubovi.ImagePath; //ime slike zvezda.png

            // trazi .exe i vraca do foldera projekta
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Directory.GetParent(baseDir)  // bin/debug/net
                                       .Parent                // /bin/debug
                                       .Parent                // /bin
                                       .FullName;             // KluboviSrbije folder

            string fullPath = Path.Combine(projectDir, "Images", imageFile); //Dolazi do images foldera

            if (File.Exists(fullPath))
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(fullPath, UriKind.Absolute); // postavlja apsolutnu putanju do fajla
                bmp.CacheOption = BitmapCacheOption.OnLoad; // učitava sliku odmah iz foldera
                bmp.EndInit();
                ImgSrc.Source = bmp;
            }
            else
            {
                ImgSrc.Source = null;
                ImageBorder.Visibility = Visibility.Collapsed; //da sakrije border ako nema slike
            }

            SelectedImageLabel.Content = Path.GetFileName(fullPath);

            try
            {
                string rtfFilePath = "../../../RTF/" + klubovi.RtfFilePath + ".rtf";
                if (File.Exists(rtfFilePath))
                {
                    var range = new TextRange(
                        RichTextBoxView.Document.ContentStart,
                        RichTextBoxView.Document.ContentEnd);
                    using var fileStream = new FileStream(rtfFilePath, FileMode.Open);
                    range.Load(fileStream, DataFormats.Rtf);
                }
                else
                {
                    MessageBox.Show("Nije pronađen RTF fajl.", "Greška",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška tokom učitavanja RTF-a: {ex.Message}",
                                "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string formattedDate = klubovi.DateAdded.ToString("dd-MM-yyyy");

            capacityTextBox.Text = klubovi.ActiveUsersField.ToString();
        }

        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            TableWindow tableWindow = new TableWindow(savedUser);
            tableWindow.Show();
            this.Close();
        }

        public void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da izađete iz aplikacije?", "Izlaz", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

    }
}
