using KluboviSrbije.Modeli;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace KluboviSrbije.Windows
{
    public partial class EditWindow : Window
    {
        public User savedUser = new User();
        public string savedImageName = "";
        public string savedPath = "";
        public string startFileName = "";
        private string inputClubName = "Unesite ime kluba...";
        private bool isPlaceholderActive = true;
        private string inputCapacity = "Unesite kapacitet stadiona...";

        public ObservableCollection<KluboviKlasa> Klubovi { get; set; }
        public EditWindow(User user, KluboviKlasa klub)
        {
            InitializeComponent();
            savedUser = user;
            NameTextBox.Text = klub.RtfFilePath;
            startFileName = klub.RtfFilePath.Trim();

            var bc = new BrushConverter();
            NameTextBox.Foreground = Brushes.Black;

            capacityTxb.Text = klub.ActiveUsersField.ToString();
            capacityTxb.Foreground = Brushes.Black;

            string fileName = Path.GetFileName(klub.ImagePath);
            string relPath = $"../../../Images/{fileName}";

            ImagePreview.Source = new BitmapImage(new Uri($"../../../Images/{fileName}", UriKind.Relative));

            savedImageName = relPath;
            SelectedImageNameLabel.Content = fileName;

            try
            {
                string rtfFilePath = "../../../RTF/" + klub.RtfFilePath + ".rtf";

                if (File.Exists(rtfFilePath))
                    if (File.Exists(rtfFilePath))
                {

                    TextRange range = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd);
                    using (FileStream fileStream = new FileStream(rtfFilePath, FileMode.Open))
                    {
                        range.Load(fileStream, DataFormats.Rtf);
                    }
                }
                else
                {
                    MessageBox.Show("Nije pronadjen RTF fajl.", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greska priliko ucitavanja RTF fajla: {ex.Message}", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            FontFamilyComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            ColorComboBox.ItemsSource = typeof(Colors).GetProperties()
                                            .Where(p => p.PropertyType == typeof(Color))
                                            .OrderBy(p => p.Name)
                                            .Select(p => (Color)p.GetValue(null))
                                            .ToList();
            FontSizeComboBox.ItemsSource = Enumerable.Range(1, 30).Select(i => (double)i).ToList();

            //UpdateWordCount();
            this.DataContext = this;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
                DragMove();
        }

        private void FontFamilyComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (FontFamilyComboBox.SelectedItem != null && !EditorRichTextBox.Selection.IsEmpty)
            {
                if (FontFamilyComboBox.SelectedItem is FontFamily selectedFontFamily)
                {
                    EditorRichTextBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, FontFamilyComboBox.SelectedItem);
                }
            }
        }

        public void FontSizeComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem != null && !EditorRichTextBox.Selection.IsEmpty)
            {
                EditorRichTextBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, FontSizeComboBox.SelectedItem);
            }
        }

        private void AddImgBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                savedPath = filePath;
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName)); // uri je putanja do slike
                ImagePreview.Source = bitmap;
                string selectedImageName = System.IO.Path.GetFileName(filePath);
                SelectedImageNameLabel.Content = selectedImageName;
                savedImageName = "../../../Images/" + selectedImageName;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da zelite da izadjete?", "Izlaz", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                TableWindow tableWindow = new TableWindow(savedUser);
                tableWindow.Show();
                this.Close();
            }
        }
        private void EditorRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object fontBold = EditorRichTextBox.Selection.GetPropertyValue(Inline.FontWeightProperty);
            BoldToggleButton.IsChecked = (fontBold != DependencyProperty.UnsetValue) && (fontBold.Equals(FontWeights.Bold));

            object fontItalic = EditorRichTextBox.Selection.GetPropertyValue(Inline.FontStyleProperty);
            ItalicToggleButton.IsChecked = (fontItalic != DependencyProperty.UnsetValue) && (fontItalic.Equals(FontStyles.Italic));

            object textUnderline = EditorRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            UnderlineToggleButton.IsChecked = (textUnderline != DependencyProperty.UnsetValue) && (textUnderline.Equals(TextDecorations.Underline));

            object foregroundColor = EditorRichTextBox.Selection.GetPropertyValue(Inline.ForegroundProperty);
            if (foregroundColor is SolidColorBrush brush)
            {
                Color selectedColor = brush.Color;
                ColorComboBox.SelectedItem = selectedColor;
            }

            object fontFamily = EditorRichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            FontFamilyComboBox.SelectedItem = fontFamily;

            object fontSize = EditorRichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty);
            if (fontSize != DependencyProperty.UnsetValue)
            {
                FontSizeComboBox.SelectedItem = (double)fontSize;
            }
        }
        private void EditorRichTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFormData())
            {
                string name = NameTextBox.Text.Trim();

                string xmlFilePath = "../../../DataBase/klubovi.xml";

                List<KluboviKlasa> remainingClubs = new List<KluboviKlasa>();
                List<KluboviKlasa> ClubsCheck = new List<KluboviKlasa>();
                //brisanje iz xml fajla izmenjenog kluba
                if (File.Exists(xmlFilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<KluboviKlasa>));
                    using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
                    {
                        ClubsCheck = (List<KluboviKlasa>)serializer.Deserialize(fileStream);
                    }
                }

                foreach (KluboviKlasa klubs in ClubsCheck)
                {
                    if (klubs.RtfFilePath == startFileName)
                    {
                         string rtfFileForDelete = "../../../RTF/" + startFileName;
                            if(!rtfFileForDelete.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase))
                            {
                                rtfFileForDelete += ".rtf";
                            }
                            continue;
                    }
                    remainingClubs.Add(klubs);
                }

                if(File.Exists(xmlFilePath))
                {
                     XmlSerializer serializer = new XmlSerializer(typeof(List<KluboviKlasa>));
                    using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Create))
                    {
                        serializer.Serialize(fileStream, remainingClubs);
                    }

                }

                KluboviKlasa klub = new KluboviKlasa(Convert.ToInt32(capacityTxb.Text), new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd).Text, savedImageName, name) { DateAdded = DateTime.Now};

                if (File.Exists(xmlFilePath))
                {
                    List<KluboviKlasa> klubovi;
                    XmlSerializer serializer = new XmlSerializer(typeof(List<KluboviKlasa>));

                    using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
                    {
                        klubovi = (List<KluboviKlasa>)serializer.Deserialize(fileStream);
                    }

                    klubovi.Add(klub);

                    using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Create))
                    {
                        serializer.Serialize(fileStream, klubovi);
                    }
                }
                else
                {
                    using (TextWriter writer = new StreamWriter(xmlFilePath))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<KluboviKlasa>));
                        serializer.Serialize(writer, new List<KluboviKlasa> { klub });
                    }
                }

                string folderName = "../../../RTF";
                string folderPAth = Path.Combine(Environment.CurrentDirectory, folderName);

                if (!Directory.Exists(folderPAth))
                {
                    Directory.CreateDirectory(folderPAth);
                }

                string rtfFilePath = Path.Combine(folderPAth, name);

                if (!rtfFilePath.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase))
                {
                    rtfFilePath += ".rtf";
                }

                TextRange range = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd);
                using (FileStream fileStream = new FileStream(rtfFilePath, FileMode.Create))
                {
                    range.Save(fileStream, DataFormats.Rtf);
                }

                MessageBox.Show("Klub je uspešno izmenjen.", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);


            }
        }
        private bool ValidateFormData()
        {
            bool isValid = true;

            if (NameTextBox.Text.Trim().Equals(string.Empty) || NameTextBox.Text.Trim().Equals(inputClubName))
            {
                isValid = false;
                NameErrorLabel.Text = "Polje ne sme biti prazno!";
                NameTextBox.BorderBrush = Brushes.Red;

            }
            else
            {
                NameErrorLabel.Text = string.Empty;
                NameTextBox.BorderBrush = Brushes.Black;
            }

            string capacity = capacityTxb.Text;

            if (!string.IsNullOrEmpty(capacity))
            {
                try
                {
                    int result = int.Parse(capacity);
                }
                catch (FormatException)
                {
                    isValid = false;
                    capacityError.Text = "Polje mora biti broj!";
                    capacityTxb.BorderBrush = Brushes.Red;
                }
            }
            else if (capacityTxb.Text.Trim().Equals(string.Empty) || capacityTxb.Text.Trim().Equals(inputCapacity))
            {
                isValid = false;
                capacityError.Text = "Polje ne sme biti prazno!";
                capacityTxb.BorderBrush = Brushes.Red;
            }
            else
            {
                capacityError.Text = string.Empty;
                capacityTxb.BorderBrush = Brushes.Black;
            }

            if (SelectedImageNameLabel.Content.ToString().Trim() == string.Empty || SelectedImageNameLabel.Content.ToString() == "Slika mora biti dodana!")
            {
                isValid = false;
                SelectedImageNameLabel.Content = "Slika mora biti dodana!";
                SelectedImageNameLabel.Foreground = Brushes.Red;
                BorderForImage.BorderBrush = Brushes.Red;
            }

            string description = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd).Text.Trim();

            if (string.IsNullOrWhiteSpace(description))
            {
                isValid = false;
                RichTextBoxError.Text = "Opis kluba ne sme biti prazan!";
                EditorRichTextBox.BorderBrush = Brushes.Red;
            }
            else
            {
                RichTextBoxError.Text = "";
                EditorRichTextBox.BorderBrush = Brushes.DarkGray;
            }

            return isValid;
        }

        private void ColorComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (ColorComboBox.SelectedItem != null && !EditorRichTextBox.Selection.IsEmpty)
            {
                if (ColorComboBox.SelectedItem is Color selectedColor)
                {
                    SolidColorBrush brush = new SolidColorBrush(selectedColor);
                    EditorRichTextBox.Selection.ApplyPropertyValue(Inline.ForegroundProperty, brush);
                }
            }
        }

        private void txbName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text.Trim().Equals(inputClubName))
            {
                NameTextBox.Text = string.Empty;
                NameTextBox.Foreground = Brushes.Black;
            }

        }

        private void txbName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text.Trim().Equals(string.Empty))
            {
                NameTextBox.Text = inputClubName;
                var bc = new BrushConverter();
                NameTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void EditorRichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!isPlaceholderActive)
            {
                return;
            }

            EditorRichTextBox.Foreground = Brushes.Black;
            isPlaceholderActive = false;
        }

        private void EditorRichTextBox_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string text = new TextRange(
                EditorRichTextBox.Document.ContentStart,
                EditorRichTextBox.Document.ContentEnd).Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                EditorRichTextBox.Document.Blocks.Clear();
                EditorRichTextBox.Document.Blocks.Add(
                    new Paragraph(new Run("Unesite opis kluba...")));
                EditorRichTextBox.Foreground = Brushes.Gray;
                isPlaceholderActive = true;
            }
        }

        private void Capacity_GotFocus(object sender, RoutedEventArgs e)
        {
            if (capacityTxb.Text.Trim().Equals(inputCapacity))
            {
                capacityTxb.Text = string.Empty;
                capacityTxb.Foreground = Brushes.Black;
            }

        }

        private void Capacity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (capacityTxb.Text.Trim().Equals(string.Empty))
            {
                capacityTxb.Text = inputCapacity;
                var bc = new BrushConverter();
                capacityTxb.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void UpdateWordCount()
        {
            if (StatusBarText == null)
            {
                return;
            }

            string text = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd).Text;
            int wordCount = text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            StatusBarText.Text = $"Broj reci: {wordCount}";

        }

        private void EditorRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWordCount();
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