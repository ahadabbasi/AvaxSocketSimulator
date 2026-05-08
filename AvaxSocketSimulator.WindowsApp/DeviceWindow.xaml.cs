using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AvaxSocketSimulator.WindowsApp.Models.Persistence;
using AvaxSocketSimulator.WindowsApp.Models.Services;
using AvaxSocketSimulator.WindowsApp.Models.ViewModels;

namespace AvaxSocketSimulator.WindowsApp
{
    /// <summary>
    /// Interaction logic for DeviceWindow.xaml
    /// </summary>
    public partial class DeviceWindow : Window
    {
        private readonly DeviceWindowsVm _data;

        public DeviceWindow()
        {
            InitializeComponent();

            _data = new DeviceWindowsVm();

            DataContext = _data;
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_data.IsValid() && _data.IsProcessing == false)
            {
                Task.Factory.StartNew(async () =>
                {
                    _data.IsProcessing = !_data.IsProcessing;

                    using (ApplicationContext context = new ApplicationContext())
                    {
                        string type =
                            _data.Options.Where(item => item.Key == _data.Selected)
                                .Select(item => item.Value)
                                .First();

                        FetchCountVm device =
                            context.Packets.Where(item => item.Type.Equals(type))
                                .GroupBy(item => item.Imei)
                                .Select(item =>
                                    new FetchCountVm
                                    {
                                        Imei = item.Key,
                                        Count = item.Count()
                                    }
                                ).OrderByDescending(item => item.Count)
                                .First();

                        const string loginType = "LoginMessage";

                        string loginPacket = context.Packets
                            .Where(item => item.Imei.Equals(device.Imei) && item.Type.Equals(loginType))
                            .Select(item => item.Data)
                            .FirstOrDefault();

                        if (!string.IsNullOrEmpty(loginPacket))
                            try
                            {
                                using (SocketServerService server = new SocketServerService(_data.WebServerAddress, 5058))
                                {
                                    server.Send(loginPacket);

                                    const int size = 20;

                                    int pages = (int)Math.Ceiling((decimal)device.Count / size);

                                    int delayTime = int.Parse(_data.DelayTime);

                                    for (int page = 1; page <= pages; page++)
                                    {
                                        IEnumerable<FetchDataVm> packets =
                                            context.Packets.Where(item =>
                                                    item.Imei.Equals(device.Imei) && item.Type.Equals(type))
                                                .Select(item => new FetchDataVm { Data = item.Data, Inserted = item.Inserted })
                                                .OrderByDescending(item => item.Inserted)
                                                .Skip((page - 1) * size)
                                                .Take(size)
                                                .ToList();

                                        foreach (FetchDataVm packet in packets)
                                        {
                                            server.Send(packet.Data);
                                            await Task.Delay(delayTime);
                                        }
                                    }
                                }

                            }
                            catch
                            {
                                //
                            }
                    }

                    _data.IsProcessing = !_data.IsProcessing;
                });
            }
        }
    }
}
