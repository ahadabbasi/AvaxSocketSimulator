using AvaxSocketSimulator.WindowsApp.Models.Persistence;
using AvaxSocketSimulator.WindowsApp.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AvaxSocketSimulator.WindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowsVm _dataVm;
        public MainWindow()
        {
            InitializeComponent();

            _dataVm = new MainWindowsVm()
            {
                DataType = "LocationData",
                DelayTime = "60000",
                WebServerAddress = "https://localhost:44392/api/history"
            };

            DataContext = _dataVm;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dataVm.IsValid())
            {
                Task.Factory.StartNew(async () =>
                {
                    _dataVm.IsProcessing = true;

                    using (ApplicationContext context = new ApplicationContext())
                    {
                        IEnumerable<Guid> data =
                            await context.Packets.Where(item => item.Type == _dataVm.DataType)
                                .OrderBy(item => item.Inserted)
                                .Select(item => item.Id)
                                .ToListAsync();

                        if (data.Any())
                        {
                            foreach (Guid item in data)
                            {
                                FetchVm sendItem =
                                    await context.Packets.Where(packet => packet.Id == item)
                                        .Select(packet => new FetchVm() { Imei = packet.Imei, Data = packet.Data })
                                        .FirstOrDefaultAsync();

                                if (sendItem != null)
                                {
                                    try
                                    {
                                        string contentSerialize =
                                            JsonConvert.SerializeObject(
                                                sendItem,
                                                Formatting.Indented
                                            );

                                        using (HttpClient client = new HttpClient())
                                        {
                                            using (StringContent content =
                                                new StringContent(
                                                    contentSerialize,
                                                    Encoding.UTF8,
                                                    "application/json"
                                                )
                                            )
                                            {
                                                using (HttpRequestMessage request =
                                                    new HttpRequestMessage(
                                                        HttpMethod.Post,
                                                        _dataVm.WebServerAddress
                                                    )
                                                )
                                                {
                                                    request.Content = content;

                                                    using (HttpResponseMessage response = await client.SendAsync(request))
                                                    {
                                                        contentSerialize = await response.Content.ReadAsStringAsync();

                                                        if(response.IsSuccessStatusCode)
                                                        {
                                                            Console.WriteLine("");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        //
                                    }
                                }

                                await Task.Delay(int.Parse(_dataVm.DelayTime));
                            }
                        }


                    }

                    _dataVm.IsProcessing = false;
                });
            }
        }
    }
}
