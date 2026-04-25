using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : ClassINotifyPropertyChanged
    {
        private int _id = 0;
        private string errorMSg = "";
        public List<string> ComboBoxItems { get; set; } = new List<string>()
        {
            "IntervalMeter",
            "SmartMeter",
        };
        public ObservableCollection<Entity> EntitiesToShow { get; set; }
        public ObservableCollection<Entity> Entities { get; set; }
        public ObservableCollection<Entity> EntitiesSearched { get; set; }
        public ClassICommand AddEntityCommand { get; set; }
        public ClassICommand DeleteEntityCommand { get; set; }
        public ClassICommand SearchEntityCommand { get; set; }
        public ClassICommand RefreshEntityCommand { get; set; }
        public ClassICommand SaveSearchCommand { get; set; }

        private EntityType currentEntityType = new EntityType();
        private Entity selectedEntity;
        private string searchBox;
        private bool isTypeRBSelected;
        private bool isNameRBSelected = true;

        private string selectedFilterType;

        public string SelectedFilterType
        {
            get => selectedFilterType;
            set { selectedFilterType = value; OnPropertyChanged(nameof(SelectedFilterType)); }
        }

        private bool isLessThanSelected;
        public bool IsLessThanSelected
        {
            get => isLessThanSelected;
            set { isLessThanSelected = value; OnPropertyChanged(nameof(IsLessThanSelected));}
        }

        private bool isGreaterThanSelected;
        public bool IsGreaterThanSelected
        {
            get => isGreaterThanSelected;
            set { isGreaterThanSelected = value; OnPropertyChanged(nameof(IsGreaterThanSelected));}
        }

        private bool isEqualSelected;
        public bool IsEqualSelected
        {
            get => isEqualSelected;
            set { isEqualSelected = value; OnPropertyChanged(nameof(IsEqualSelected));}
        }

        private string idFilterText;
        public string IdFilterText
        {
            get => idFilterText;
            set { idFilterText = value; OnPropertyChanged(nameof(IdFilterText)); }
        }

        public ClassICommand FilterEntityCommand { get; set; }

        private string terminalOutput = "=== Terminal ready ===\n";
        public string TerminalOutput
        {
            get => terminalOutput;
            set { terminalOutput = value; OnPropertyChanged(nameof(TerminalOutput)); }
        }

        private string terminalInput;
        public string TerminalInput
        {
            get => terminalInput;
            set { terminalInput = value; OnPropertyChanged(nameof(TerminalInput)); }
        }

        public ClassICommand RunTerminalCommand { get; set; }


        public NetworkEntitiesViewModel()
        {
            Entities = new ObservableCollection<Entity>();
            EntitiesSearched = new ObservableCollection<Entity>();
            EntitiesToShow = Entities;

            AddEntityCommand = new ClassICommand(OnAdd);
            DeleteEntityCommand = new ClassICommand(OnDelete, CanDelete);
            SearchEntityCommand = new ClassICommand(onSearch);
            RefreshEntityCommand = new ClassICommand(onRefresh);
            RunTerminalCommand = new ClassICommand(ExecuteTerminalCommand);

            FilterEntityCommand = new ClassICommand(ApplyFilter);
        }

        private void ApplyFilter()
        {
            var query = Entities.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SelectedFilterType))
            {
                query = query.Where(e => e.Type.Type == SelectedFilterType);
            }

            if (int.TryParse(IdFilterText, out int idValue))
            {
                if (IsLessThanSelected)
                {
                    query = query.Where(e => e.Id < idValue);
                }
                else if (IsGreaterThanSelected)
                {
                    query = query.Where(e => e.Id > idValue);
                }
                else if (IsEqualSelected)
                {
                    query = query.Where(e => e.Id == idValue);
                }
            }

            EntitiesToShow = new ObservableCollection<Entity>(query);
            OnPropertyChanged(nameof(EntitiesToShow));
        }

        private void ExecuteTerminalCommand()
        {
            if (string.IsNullOrWhiteSpace(TerminalInput)) return;

            TerminalOutput += $"> {TerminalInput}\n";
            var parts = TerminalInput.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "search":
                    var query = parts.Length > 1 ? parts[1] : string.Empty;
                    var mode = parts.Length > 2 ? parts[2].ToLower() : null;

                    if (mode == "type") { IsTypeRBSelected = true; IsNameRBSelected = false; }
                    else if (mode == "name") { IsTypeRBSelected = false; IsNameRBSelected = true; }

                    SearchBox = query;
                    TerminalOutput += $"[SEARCH] query='{query}', mode={(IsTypeRBSelected ? "type" : "name")}\n";
                    SearchEntityCommand.Execute(null);
                    break;

                case "refresh":
                    TerminalOutput += "[REFRESH]\n";
                    RefreshEntityCommand.Execute(null);
                    break;

                case "add":
                    var typeArg = parts.Length > 1 ? parts[1] : null;
                    if (!string.IsNullOrWhiteSpace(typeArg))
                    {
                        CurrentEntityType = new EntityType { Type = typeArg };
                    }
                    TerminalOutput += $"[ADD] type='{CurrentEntityType?.Type}'\n";
                    OnAdd();
                    break;

                case "delete":
                    var targetName = parts.Length > 1 ? parts[1] : null;
                    if (!string.IsNullOrWhiteSpace(targetName))
                    {
                        var target = Entities.FirstOrDefault(e => e.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                        SelectedEntity = target;
                    }
                    if (SelectedEntity != null && CanDelete())
                    {
                        TerminalOutput += $"[DELETE] name='{SelectedEntity.Name}'\n";
                        OnDelete();
                    }
                    else
                    {
                        TerminalOutput += "[DELETE] No entity selected or not found.\n";
                    }
                    break;
                default:
                    TerminalOutput += $"Unknown command: {cmd}\n";
                    break;
            }

            TerminalInput = string.Empty;
        }

        private void onRefresh()
        {
            if(EntitiesToShow != Entities)
            {
                EntitiesToShow = Entities;
                OnPropertyChanged("EntitiesToShow");
            }      
        }

        public string SearchBox
        {
            get { return searchBox; }
            set
            {
                searchBox = value;
                OnPropertyChanged("SearchBox");
            }
        }

        public bool IsTypeRBSelected
        {
            get { return isTypeRBSelected; }
            set
            {
                isTypeRBSelected = value;
                OnPropertyChanged("IsTypeRBSelected");
            }
        }

        public bool IsNameRBSelected
        {
            get { return isNameRBSelected; }
            set
            {
                isNameRBSelected = value;
                OnPropertyChanged("IsNameRBSelected");
            }
        }

        private void onSearch()
        {
            EntitiesSearched.Clear();
            try
            {
                if (IsTypeRBSelected)
                {
                    if (string.IsNullOrWhiteSpace(SearchBox))
                    {
                        if (EntitiesToShow != Entities)
                        {
                            EntitiesToShow = Entities;
                            OnPropertyChanged("EntitiesToShow");
                        }
                    }
                    else
                    {
                        foreach (var entity in Entities)
                        {
                            if (entity.Type.Type.Contains(SearchBox))
                            {
                                EntitiesSearched.Add(entity);
                            }
                        }
                        EntitiesToShow = EntitiesSearched;
                        OnPropertyChanged("EntitiesToShow");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(SearchBox))
                    {
                        if (EntitiesToShow != Entities)
                        {
                            EntitiesToShow = Entities;
                            OnPropertyChanged("EntitiesToShow");
                        }
                    }
                    else
                    {
                        foreach (var entity in Entities)
                        {
                            if (entity.Name.Contains(SearchBox))
                            {
                                EntitiesSearched.Add(entity);
                            }
                        }
                        EntitiesToShow = EntitiesSearched;
                        OnPropertyChanged("EntitiesToShow");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public Entity SelectedEntity
        {
            get { return selectedEntity; }
            set
            {
                selectedEntity = value;
                DeleteEntityCommand.RaiseCanExecuteChanged();
            }
        }
        private void OnDelete()
        {
            Entities.Remove(SelectedEntity);
            if (EntitiesSearched.Contains(SelectedEntity))
            {
                EntitiesSearched.Remove(SelectedEntity);
            }
        }

        private bool CanDelete()
        {
            return SelectedEntity != null;
        }

        public string ErrorMSg
        {
            get { return errorMSg; }
            set
            {
                errorMSg = value;
                OnPropertyChanged("ErrorMSg");
            }
        }
        public EntityType CurrentEntityType
        {
            get { return currentEntityType; }
            set
            {
                currentEntityType = value;
                OnPropertyChanged("CurrentEntityType");
            }
        }

        public void OnAdd()
        {
            string imgPath = "";
            if(CurrentEntityType.Type == null)
            {
                ErrorMSg = "Need To Choose Type!";
                return;
            }
            else if(CurrentEntityType.Type == "SmartMeter")
            {
                imgPath = "pack://application:,,,/NetworkService;component/Images/intervalmeter.jpg";
            }
            else
            {
                imgPath = "pack://application:,,,/NetworkService;component/Images/smartmeter.jpg";
            }
            ErrorMSg = "";
            try
            {
                Entities.Add(new Entity() 
                            { Id = _id,
                              Name = $"Entity_{_id++}",
                              Value = 0,
                              Type = new EntityType() { Type = CurrentEntityType.Type, ImgSrc = imgPath }
                            });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} - {ex.Message}");
            }
        }
    }
}