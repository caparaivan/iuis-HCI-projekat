using KluboviSrbije.Modeli;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace KluboviSrbije.Windows
{
    public partial class TableWindow : Window
    {
        public ObservableCollection<KluboviKlasa> Klubovi { get; set; }
        public User savedUser = new User();
        public TableWindow(User user)
        {
            InitializeComponent();
            savedUser = user;
            if (user.Role == UserRole.Admin)
            {
                AddBtn.Visibility = Visibility.Visible;
                DeleteBtn.Visibility = Visibility.Visible;
                LogOutBtn.Visibility = Visibility.Visible;
                SelectColon.Visibility = Visibility.Visible;
            }
            else
            {
                AddBtn.Visibility = Visibility.Hidden;
                DeleteBtn.Visibility = Visibility.Hidden;
                SelectColon.Visibility = Visibility.Hidden;
            }
            DataContext = this;
            LoadKluboviFromXml();
        }

        private void LoadKluboviFromXml()
        {
            string xmlFilePath = "../../../DataBase/klubovi.xml";

            if (File.Exists(xmlFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<KluboviKlasa>));
                using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
                {
                    Klubovi = (ObservableCollection<KluboviKlasa>)serializer.Deserialize(fileStream);
                }
            }
            else
            {
                Klubovi = new ObservableCollection<KluboviKlasa>();
            }

            UsersDataGrid.ItemsSource = Klubovi;

        }

        public void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
                DragMove();
        }

        public void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow(savedUser);
            addWindow.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UsersDataGrid.Items.SortDescriptions.Add(new SortDescription("DateAdded", ListSortDirection.Descending));
        }

        public void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int cnt = 0;
            foreach (KluboviKlasa kluboviSelected in UsersDataGrid.Items)
            {
                if (kluboviSelected.IsSelected)
                {
                    cnt++;
                    break;
                }
            }

            if(cnt != 0)
            {
                MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da obrišete izabrane klubove?", "Brisanje", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    List<KluboviKlasa> remainingKlubovi = new List<KluboviKlasa>();

                    foreach (KluboviKlasa kluboviSelected in UsersDataGrid.Items)
                    {
                        if (kluboviSelected.IsSelected)
                        {
                            string rtfFilePath = "../../../RTF/" + kluboviSelected.RtfFilePath;
                            if(!rtfFilePath.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase))
                            {
                                rtfFilePath += ".rtf";
                            }

                            if(File.Exists(rtfFilePath))
                            {
                                File.Delete(rtfFilePath);
                            }
                            continue;
                        }
                        remainingKlubovi.Add(kluboviSelected);
                    }
                    UsersDataGrid.ItemsSource = remainingKlubovi;

                    XmlSerializer serializer = new XmlSerializer(typeof(List<KluboviKlasa>));
                    using (TextWriter writer = new StreamWriter("../../../DataBase/klubovi.xml"))
                    {
                        serializer.Serialize(writer, remainingKlubovi);
                    }
                    MessageBox.Show("Klubovi su uspešno obrisani.", "Brisanje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Niste izabrali nijedan klub za brisanje.", "Brisanje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da se odjavite?", "Odjava", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        public void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var textBlock = (TextBlock)sender;
            var item = textBlock.DataContext as KluboviKlasa;

            if(item != null)
            {
                if(savedUser.Role == UserRole.Admin)
                {
                    EditWindow editWindow = new EditWindow(savedUser, item);
                    editWindow.Show();
                    this.Close();
                }
                else
                {
                    ViewWindow viewWindow = new ViewWindow(savedUser, item);
                    viewWindow.Show();
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Došlo je do greške prilikom otvaranja prozora.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da izađete iz aplikacije?", "Izlaz", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

    }
}
