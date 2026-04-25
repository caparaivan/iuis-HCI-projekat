using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : ClassINotifyPropertyChanged
    {
        public ClassICommand<string> NavCommand { get; private set; }

        private NetworkEntitiesViewModel networkEntitiesViewModel = new NetworkEntitiesViewModel();
        private NetworkDisplayViewModel networkDisplayViewModel = new NetworkDisplayViewModel();
        private MeasurementGraphViewModel measurementGraphViewModel = new MeasurementGraphViewModel();

        private ClassINotifyPropertyChanged currentViewModel;
        private ClassINotifyPropertyChanged alwaysOnViewModel;

        private int selectedTabIndex;
        public int SelectedTabIndex
        {
            get => selectedTabIndex;
            set
            {
                if (selectedTabIndex != value)
                {
                    selectedTabIndex = value;
                    OnPropertyChanged(nameof(SelectedTabIndex));

                    // kad se promijeni tab, pozove se NavCommand
                    switch (selectedTabIndex)
                    {
                        case 0:
                            NavCommand.Execute("1_Entities");
                            break;
                        case 1:
                            NavCommand.Execute("2_Graph");
                            break;
                    }
                }
            }
        }



        public ClassINotifyPropertyChanged CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }
        public ClassINotifyPropertyChanged AlwaysOnViewModel
        {
            get { return alwaysOnViewModel; }
            set
            {
                SetProperty(ref alwaysOnViewModel, value);
            }
        }
        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "1_Entities":
                    CurrentViewModel = networkEntitiesViewModel;
                    break;
                case "2_Graph":
                    CurrentViewModel = measurementGraphViewModel;
                    break;
            }
        }
        public MainWindowViewModel()
        {
            
            NavCommand = new ClassICommand<string>(OnNav);

            CurrentViewModel = networkEntitiesViewModel;
            AlwaysOnViewModel = networkDisplayViewModel;

            createListener(); // povezivanje sa serverskom aplikacijom

            networkEntitiesViewModel.Entities.CollectionChanged += this.OnCollectionChanged;

            networkEntitiesViewModel.Entities.CollectionChanged += this.OnCollectionChangedMeasurementGraphViewModel;
        }

        private void OnCollectionChangedMeasurementGraphViewModel(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Entity newEntity in e.NewItems)
                {
                    if (!measurementGraphViewModel.EntitiesInList.Contains(newEntity))
                    {
                        measurementGraphViewModel.EntitiesInList.Add(newEntity);
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (Entity oldEntity in e.OldItems)
                {
                    if (measurementGraphViewModel.EntitiesInList.Contains(oldEntity))
                    {
                        measurementGraphViewModel.EntitiesInList.Remove(oldEntity);
                    }
                }
            }
        }
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
           if (e.NewItems != null)
            {
                foreach (Entity newEntity in e.NewItems)
                {
                    if (!networkDisplayViewModel.EntitiesInList.Contains(newEntity))
                    {
                        networkDisplayViewModel.EntitiesInList.Add(newEntity);
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (Entity oldEntity in e.OldItems)
                {
                    if (networkDisplayViewModel.EntitiesInList.Contains(oldEntity))
                    {
                        networkDisplayViewModel.EntitiesInList.Remove(oldEntity);
                    }
                    else
                    {
                        int canvasIndex = networkDisplayViewModel.GetCanvasIndexForEntityId(oldEntity.Id);
                        networkDisplayViewModel.DeleteEntityFromCanvas(oldEntity);
                    }
                }
            }
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        // prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        // primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        if (incomming.Equals("Need object count"))
                        {

                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(networkEntitiesViewModel.Entities.Count.ToString());
                            stream.Write(data, 0, data.Length);

                            if (File.Exists("Log.txt"))
                            {
                                File.WriteAllText("Log.txt", String.Empty);
                            }
                            else
                            {
                                File.Create("Log.txt");
                            }                     
                        }
                        else
                        {
                            Console.WriteLine(incomming); //entitet_1:272

                            if (networkEntitiesViewModel.Entities.Count > 0)
                            {
                                var splited = incomming.Replace("Entitet", "Entity").Split(':');
                                DateTime dt = DateTime.Now;
                                using (StreamWriter sw = File.AppendText("Log.txt"))
                                    sw.WriteLine(dt + "; " + splited[0] + ", " + splited[1]);

                                int id = Int32.Parse(splited[0].Split('_')[1]);
                                networkEntitiesViewModel.Entities[id].Value = Double.Parse(splited[1]);

                                networkDisplayViewModel.UpdateEntityOnCanvas(networkEntitiesViewModel.Entities[id]);
                                measurementGraphViewModel.AutoShow();
                            }    
                        }
                    }, null);
                }
            });
            listeningThread.IsBackground = true;
            listeningThread.Start();
        }
    }
}