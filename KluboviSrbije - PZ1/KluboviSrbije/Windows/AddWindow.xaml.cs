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

using static System.Net.Mime.MediaTypeNames;

namespace KluboviSrbije.Windows
{
    public partial class AddWindow : Window
    {
        public User savedUser = new User();
        public string savedImageName = "";
        private string inputClubName = "Unesite ime kluba...";
        private bool isPlaceholderActive = true;
        private string inputCapacity = "Unesite kapacitet stadiona...";

        public ObservableCollection<KluboviKlasa> Klubovi { get; set; }
        public string savedPath = string.Empty;
        public AddWindow(User user)
        {
            InitializeComponent();
            savedUser = user;

            ColorComboBox.ItemsSource = typeof(Colors).GetProperties()
                                            .Where(p => p.PropertyType == typeof(Color))
                                            .OrderBy(p => p.Name)
                                            .Select(p => (Color)p.GetValue(null))
                                            .ToList();
            FontFamilyComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontSizeComboBox.ItemsSource = Enumerable.Range(1, 72).Select(i => (double)i).ToList();
            NameTextBox.Text = inputClubName;
            NameTextBox.Foreground = Brushes.Gray;
            capacityTxb.Text = inputCapacity;
            capacityTxb.Foreground = Brushes.Gray;

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void FontFamilyComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if(FontFamilyComboBox.SelectedItem != null && !EditorRichTextBox.Selection.IsEmpty)
            {
                if (FontFamilyComboBox.SelectedItem is FontFamily selectedFontFamily)
                {
                    EditorRichTextBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, FontFamilyComboBox.SelectedItem);
                }
            }
        }

        public void FontSizeComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if(FontSizeComboBox.SelectedItem != null && !EditorRichTextBox.Selection.IsEmpty)
            {
                EditorRichTextBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, FontSizeComboBox.SelectedItem);
            }
        }

        private void AddImgBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (dlg.ShowDialog() != true)
            {
                return;
            }

            ImagePreview.Source = new BitmapImage(
                new Uri(dlg.FileName, UriKind.Absolute));

            // ime fajla
            string selectedImageName = Path.GetFileName(dlg.FileName);
            SelectedImageNameLabel.Content = selectedImageName;
            savedPath = dlg.FileName;

            // penje se 3 nivoa gore od bin\Debug\net… do foldera projekta
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..")); // sad je u folderu projekta

            // spaja sa Images folderom i nazivom slike
            string fullImagePath = Path.Combine(projectRoot, "Images", selectedImageName);

            savedImageName = fullImagePath;
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
            if(fontSize != DependencyProperty.UnsetValue)
            {
                FontSizeComboBox.SelectedItem = (double)fontSize;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(ValidateFormData())
            {
                string name = NameTextBox.Text.Trim();

                string xmlFilePath = "../../../DataBase/klubovi.xml";

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
                        serializer.Serialize(writer, new List<KluboviKlasa> { klub});
                    }
                }

                string folderName = "../../../RTF";
                string folderPAth = Path.Combine(Environment.CurrentDirectory, folderName);

                if(!Directory.Exists(folderPAth))
                {
                    Directory.CreateDirectory(folderPAth);
                }

                string rtfFilePath = Path.Combine(folderPAth, name + ".rtf");

                TextRange range = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd);
                using (FileStream fileStream = new FileStream(rtfFilePath, FileMode.Create))
                {
                    range.Save(fileStream, DataFormats.Rtf);
                }

                capacityTxb.Text = string.Empty;
                NameTextBox.Text = string.Empty;
                EditorRichTextBox.Document.Blocks.Clear();
                ImagePreview.Source = null;
                MessageBox.Show("Klub je uspešno dodat.", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                SelectedImageNameLabel.Content = "";

            }
        }

        private bool ValidateFormData()
        {
            bool isValid=true;

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

            foreach (KluboviKlasa engine in Klubovi)
            {
                if (engine.RtfFilePath == NameTextBox.Text.Trim())
                {
                    NameTextBox.BorderBrush = Brushes.Red;
                    NameErrorLabel.Text = "Klub koji ste uneli je vec dodat!";
                    isValid = false;
                    break;
                }
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
            else if(capacityTxb.Text.Trim().Equals(string.Empty) || capacityTxb.Text.Trim().Equals(inputCapacity))
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

            if (SelectedImageNameLabel.Content.ToString().Trim()== string.Empty || SelectedImageNameLabel.Content.ToString()=="Slika mora biti dodana!")
            {
                isValid = false;
                SelectedImageNameLabel.Content = "Slika mora biti dodana!";
                SelectedImageNameLabel.Foreground = Brushes.Red;
                BorderForImage.BorderBrush = Brushes.Red;
            }

            string description = new TextRange(EditorRichTextBox.Document.ContentStart,EditorRichTextBox.Document.ContentEnd).Text.Trim();

            if (string.IsNullOrWhiteSpace(description)|| description == "Unesite opis kluba...")
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
            if(ColorComboBox.SelectedItem != null && !EditorRichTextBox.Selection.IsEmpty)
            {
                if(ColorComboBox.SelectedItem is Color selectedColor)
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

            EditorRichTextBox.Document.Blocks.Clear();
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
            if(StatusBarText == null)
            {
                return;
            }

            string text = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd).Text;
            int wordCount = text.Split(new char[] { ' ','\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
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