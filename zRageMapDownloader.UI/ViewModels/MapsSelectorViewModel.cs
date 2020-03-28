using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using zRageMapDownloader.Commands;
using zRageMapDownloader.Core;

namespace zRageMapDownloader.ViewModels
{
    public class MapsSelectorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MapModel> Maps { get; set; }
        public string SearchString { get; set; }

        public SelectAllMapsCommand SelectAllMapsCommand { get; set; }
        public UnselectAllMapsCommand UnselectAllMapsCommand { get; set; }
        public SearchStringChangedCommand SearchStringChangedCommand { get; set; }

        public MapsSelectorViewModel()
        {
            SelectAllMapsCommand = new SelectAllMapsCommand(this);
            UnselectAllMapsCommand = new UnselectAllMapsCommand(this);
            SearchStringChangedCommand = new SearchStringChangedCommand(this);

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var sv = new ServerModel { FastdlUrl = "www.teste.com/fastdl/", MapListUrl = "www.teste.com/file.csv", MapsDirectory = "csgo/files/maps/", Name = "ZRAGE MAPS", SteamApplicationID = 710 };
                Maps = new ObservableCollection<MapModel>
                {
                    new MapModel("$ze_skyrim", sv),
                    new MapModel("ze_boatescape", sv),
                    new MapModel("ze_run", sv)
                };
            }
        }

        public void BindMapsObject(ObservableCollection<MapModel> maps)
        {
            Maps = maps;
        }

        public void SelectAllMaps()
        {
            foreach (var map in Maps)
            {
                map.SkipOnDownload = false;
            }
        }

        public void UnselectAllMaps()
        {
            foreach (var map in Maps)
            {
                map.SkipOnDownload = true;
            }
        }

        public void FilterMaps()
        {
            foreach (var map in Maps)
            {
                map.Visible = map.Name.ToLower().Contains(SearchString.ToLower());
            }
        }
    }
}
