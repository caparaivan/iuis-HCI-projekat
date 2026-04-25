using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : ClassINotifyPropertyChanged
    {
        private List<Entity> comboBoxItems;
        private Entity selectedEntity;
        private Entity selectedEntityToShow;

        public Dictionary<string, List<Measurement>> MeasurementDict { get; set; }
        public ObservableCollection<CircleMarker> CircleMarkers { get; set; }
        public BindingList<Entity> EntitiesInList { get; set; }

        public ClassICommand ShowCommand { get; set; }

        public List<Entity> ComboBoxItems
        {
            get => comboBoxItems;
            set { comboBoxItems = value; OnPropertyChanged(nameof(ComboBoxItems)); }
        }

        public Entity SelectedEntity
        {
            get => selectedEntity;
            set
            {
                selectedEntity = value;
                ShowCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(SelectedEntity));
            }
        }

        public Entity SelectedEntityToShow
        {
            get => selectedEntityToShow;
            set
            {
                selectedEntityToShow = value;
                OnPropertyChanged(nameof(SelectedEntityToShow));
            }
        }

        public MeasurementGraphViewModel()
        {
            EntitiesInList = new BindingList<Entity>();
            EntitiesInList.ListChanged += OnEntitiesInListChanged;

            MeasurementDict = new Dictionary<string, List<Measurement>>();
            CircleMarkers = new ObservableCollection<CircleMarker>();
            for (int i = 0; i < 5; i++)
            {
                CircleMarker marker = new CircleMarker();
                CircleMarkers.Add(marker);
            }

            UpdateComboBoxItems();

            ShowCommand = new ClassICommand(OnShow, CanShow);
        }

        public void OnShow()
        {
            SelectedEntityToShow = SelectedEntity;
            UpdateValue();
        }

        private bool CanShow()
        {
            return SelectedEntity != null;
        }

        public void UpdateValue()
        {
            if (SelectedEntityToShow != null)
            {
                string key = SelectedEntityToShow.Name;
                if (MeasurementDict.ContainsKey(key))
                {
                    var list = MeasurementDict[key];

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CircleMarkers.Clear();

                        foreach (var measurement in list.Take(5))
                        {
                            var marker = new CircleMarker
                            {
                                CmValue = measurement.Value,
                                CmDate = measurement.Date,
                                CmTime = measurement.Time,
                                CmColor = GetColorForValue(measurement.Value),
                                CmYPosition = MapValueToCanvasY(measurement.Value)
                            };
                            CircleMarkers.Add(marker);
                        }
                    });
                }
            }
        }

        private Brush GetColorForValue(double value)
        {
            return (value >= 0.34 && value <= 2.73) ? Brushes.Green : Brushes.Red;
        }

        private double MapValueToCanvasY(double value)
        {
            double maxHeight = 365;
            double minValue = 0.0;
            double maxValue = 3.0;
            double normalized = (Math.Max(value, minValue) - minValue) / (maxValue - minValue);
            double calculatedY = maxHeight - (normalized * maxHeight);
            double minY = 0;
            double maxY = maxHeight - 50 - 20;
            return Math.Min(Math.Max(calculatedY, minY), maxY);
        }

        public void AutoShow()
        {
            string filePath = "Log.txt";
            if (!File.Exists(filePath)) return;

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                (string date, string time, string entityName, int value) = ParseLine(line);
                string key = entityName;

                Measurement measurement = new Measurement(date, time, value);

                if (!MeasurementDict.ContainsKey(key))
                {
                    MeasurementDict[key] = new List<Measurement>();
                }

                if (!MeasurementDict[key].Any(m => m.Date == date && m.Time == time && m.Value == value))
                {
                    MeasurementDict[key].Add(measurement);
                    if (MeasurementDict[key].Count > 5)
                    {
                        MeasurementDict[key].RemoveAt(0);
                    }
                }

                if (SelectedEntityToShow != null && SelectedEntityToShow.Name == key)
                {
                    Application.Current.Dispatcher.Invoke(() => UpdateValue());
                }
            }

            if (SelectedEntityToShow != null)
            {
                UpdateValue();
            }
        }

        static (string date, string time, string entity, int value) ParseLine(string line)
        {
            // dijeli po ';' u dva dijela: datum i vrijeme i entitet, vrijednost
            string[] parts = line.Split(';');
            if (parts.Length < 2)
                throw new FormatException($"Neispravan red u logu: '{line}'");

            // obradi prvi dio da izvuces datum i vrijeme
            string dateTimePart = parts[0].Trim();              // 2025-09-23 10:15:42
            string[] dt = dateTimePart.Split(' ');
            string date = dt.Length > 0 ? dt[0] : string.Empty; // 2025-09-23
            string time = dt.Length > 1 ? dt[1] : string.Empty; // 10:15:42

            // obradi drugi dio da izvuce entitet i broj
            string leftover = parts[1].Trim();                  // Entity_1, 275"
            string[] lp = leftover.Split(',');
            if (lp.Length < 2)
                throw new FormatException($"Neispravan entitet ili vrijednost: '{leftover}'");

            string entity = lp[0].Trim();                       // Entity_1
            int value = int.Parse(lp[1].Trim());                // 275

            return (date, time, entity, value);
        }

        private void OnEntitiesInListChanged(object sender, ListChangedEventArgs e)
        {
            UpdateComboBoxItems();
            if (SelectedEntity != null && !EntitiesInList.Contains(SelectedEntity))
            {
                SelectedEntity = null;
            }
        }

        private void UpdateComboBoxItems()
        {
            ComboBoxItems = EntitiesInList.ToList();
            OnPropertyChanged(nameof(ComboBoxItems));
        }
    }
}