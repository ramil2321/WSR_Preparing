﻿using esoft.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace esoft.ModelView
{
    class DemandModelView : INotifyPropertyChanged
    {
        public ObservableCollection<Client> Clients { get; set; }
        public ObservableCollection<Agent> Agents { get; set; }
        private Client selectedClientDemand;
        private Agent selectedAgentDemand;

        public Client SelectedClientDemand { get => selectedClientDemand; set { selectedClientDemand = value; OnPropertyChanged("SelectedClientDemand"); } }
        public Agent SelectedAgentDemand { get => selectedAgentDemand; set { selectedAgentDemand = value; OnPropertyChanged("SelectedAgentDemand"); } }

        private Demand selectedDemand;
        public Demand SelectedDemand
        {
            get => selectedDemand;
            set { selectedDemand = value;
                if(selectedDemand != null)
                {
                    SelectedClientDemand = selectedDemand.Client;
                    SelectedAgentDemand = selectedDemand.Agent;
                }
                OnPropertyChanged("SelectedDemand");  }
        }
        public ObservableCollection<Demand> Demands { get; set; }

        public DemandModelView()
        {
            Demands = CreateCollection();
            using(Context db = new Context())
            {
                Agents = new ObservableCollection<Agent> { };
                Clients = new ObservableCollection<Client> { };
                Demands = new ObservableCollection<Demand> { };
                var resultClients = db.Clients.Where(c => !c.isDeleted);
                foreach (var c in resultClients)
                {
                    Clients.Add(c);
                }

                var resultAgents = db.Agents.Where(c => !c.isDeleted);
                foreach (var c in resultAgents)
                {
                    Agents.Add(c);
                }

                var result = db.Demands.Include("DemandFilter").Include("Agent").Include("Client").Where(e => !e.isDeleted && !e.isCompleted);
                foreach (var r in result)
                {
                    Demands.Add(r);
                }
            }
        }

        public RelayCommand RemoveCommand { get
            {
                return new RelayCommand(delegate (object parameter)
                {
                    Demand demand = parameter as Demand;
                    Model.Model.Remove(demand);
                    Demands.Remove(demand);
                }, (obj) => SelectedDemand != null);
            } }
        public RelayCommand AddCommand
        {
            get
            {
                return new RelayCommand(delegate (object parameter)
                {
                    Demand demand = GetDemand(parameter);
                    Model.Model.Create(demand);
                    Model.Model.Create(demand.DemandFilter);
                    Demands.Insert(0, demand);
                    SelectedDemand = Demands[0];
                }, (obj) => {
                    return true;
                });
            }
        }
        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(delegate (object parameter)
                {

                }, (obj) => false) ;
            }
        }

        private Demand GetDemand(object parameter)
        {
            var values = (object[])parameter;
            Client client = (Client)values[0];
            Agent agent = (Agent)values[1];
            int? minPrice = null;
            int? maxPrice = null;
            double? minArea = null;
            double? maxArea = null;


            if (Model.Checkers.IsUInt((string)values[2]))
            {
                minPrice = Convert.ToInt32(values[2]);
            }
            if (Model.Checkers.IsUInt((string)values[3]))
            {
                maxPrice = Convert.ToInt32(values[3]);
            }
            if (Model.Checkers.IsDouble((string)values[4]))
            {
                minArea = Convert.ToDouble(values[4]);
            }
            if (Model.Checkers.IsDouble((string)values[5]))
            {
                maxArea = Convert.ToDouble(values[5]);
            }

            int typeId = Convert.ToInt32(values[6]);

            DemandFilter demandFilter = new DemandFilter();

            switch (typeId)
            {
                case 0:
                    int? minRoomsApartment = null;
                    int? maxRoomsApartment = null;
                    int? minFloorApartment = null;
                    int? maxFloorApartment = null;

                    if (Model.Checkers.IsUInt((string)values[7]))
                    {
                        minRoomsApartment = Convert.ToInt32(values[7]);
                    }
                    if (Model.Checkers.IsUInt((string)values[8]))
                    {
                        maxRoomsApartment = Convert.ToInt32(values[8]);
                    }
                    if (Model.Checkers.IsUInt((string)values[9]))
                    {
                        minFloorApartment = Convert.ToInt32(values[9]);
                    }
                    if (Model.Checkers.IsUInt((string)values[10]))
                    {
                        maxFloorApartment = Convert.ToInt32(values[10]);
                    }
                    demandFilter = new DemandFilter
                    {
                        MinPrice = minPrice, MaxPrice = maxPrice,
                        MinRooms = minRoomsApartment, MaxRooms = maxRoomsApartment,
                        MinFloor = minFloorApartment, MaxFloor = maxFloorApartment,
                        MinArea = minArea, MaxArea = maxArea
                    };
                    break;
                case 1:
                    int? minRoomsHouse = null;
                    int? maxRoomsHouse = null;
                    int? minFloorsHouse = null;
                    int? maxFloorsHouse = null;

                    if (Model.Checkers.IsUInt((string)values[11]))
                    {
                        minRoomsHouse = Convert.ToInt32(values[11]);
                    }
                    if (Model.Checkers.IsUInt((string)values[12]))
                    {
                        maxRoomsHouse = Convert.ToInt32(values[12]);
                    }
                    if (Model.Checkers.IsUInt((string)values[13]))
                    {
                        minFloorsHouse = Convert.ToInt32(values[13]);
                    }
                    if (Model.Checkers.IsUInt((string)values[14]))
                    {
                        maxFloorsHouse = Convert.ToInt32(values[14]);
                    }
                    demandFilter = new DemandFilter
                    {
                        MinPrice = minPrice, MaxPrice = maxPrice,
                        MinRooms = minRoomsHouse, MaxRooms = maxRoomsHouse,
                        MinFloors = minFloorsHouse, MaxFloors = maxFloorsHouse,
                        MinArea = minArea, MaxArea = maxArea
                    };  
                    break;
                case 2:
                    demandFilter = new DemandFilter {
                        MinPrice = minPrice, MaxPrice = maxPrice,
                        MinArea = minArea, MaxArea = maxArea
                    };
                    break;
            }
            Demand demand = new Demand
            {
                Client = client,
                Agent = agent,
                EstateTypeID = typeId, DemandFilter = demandFilter
            };
            return demand;
        }
        public static ObservableCollection<Demand> CreateCollection()
        {
            ObservableCollection<Demand> demands = new ObservableCollection<Demand> { };
            using(Context db = new Context())
            {
                var result = db.Demands.Include("DemandFilter").Include("Agent").Include("Client").Where(e => !e.isDeleted && !e.isCompleted);
                foreach(var r in result)
                {
                    demands.Add(r);
                }
            }
            return demands;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
