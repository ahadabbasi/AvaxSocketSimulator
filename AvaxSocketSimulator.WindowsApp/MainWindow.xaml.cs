using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AvaxSocketSimulator.WindowsApp.Models.Persistence;
using AvaxSocketSimulator.WindowsApp.Models.ViewModels;
using Newtonsoft.Json;

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

            _dataVm = new MainWindowsVm
            {
                DataType = "LocationData",
                DelayTime = "1000",
                WebServerAddress = "https://localhost:44392/api/concox/history"
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
                                        .Select(packet => new FetchVm { Imei = packet.Imei, Data = packet.Data })
                                        .FirstOrDefaultAsync();

                                if (sendItem != null)
                                {
                                    try
                                    {
                                        string contentSerialize =
                                            JsonConvert.SerializeObject(
                                                new RequestVm(sendItem),
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
                                                        string responseContent = await response.Content.ReadAsStringAsync();
                                                        if (response.IsSuccessStatusCode)
                                                        {
                                                            Console.WriteLine("");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch
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


        private async Task<IEnumerable<IEnumerable<Guid>>> GenerateBatches()
        {
            IList<IEnumerable<Guid>> result = new List<IEnumerable<Guid>>();

            using (ApplicationContext context = new ApplicationContext())
            {
                IEnumerable<Guid> data =
                    await context.Packets.Where(item => item.Type == _dataVm.DataType)
                        .OrderBy(item => item.Inserted)
                        .Select(item => item.Id)
                        .ToListAsync();
                /*
                IEnumerable<IEnumerable<Guid>> batches = data
                    .Select((item, index) => new { item, index })
                    .GroupBy(x => x.index / 10)
                    .Select(g => g.Select(x => x.item));
                */

                const int sizeOfBatch = 20;

                int totalBatches = (int)Math.Ceiling((double)data.Count() / sizeOfBatch);

                for (int page = 1; page <= totalBatches; page++)
                {
                    result.Add(
                        data.Skip((page - 1) * sizeOfBatch).Take(sizeOfBatch)
                    );
                }
            }

            return result;
        }
    }
}
