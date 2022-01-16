using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using reservation_de_vol.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace reservation_de_vol.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]

        public IActionResult Privacy()
        {
            return View();
        }



        public static string getAirportFullName(string code)
        {

            StreamReader r = new StreamReader("wwwroot/js/airports.json");
            string jsonString = r.ReadToEnd();
            List<Airport> airports = JsonConvert.DeserializeObject<List<Airport>>(jsonString);

            foreach (var i in airports)
            {
                if (i.code == code)
                {
                    return i.name;
                }
            }
            return "";
        }

        public IActionResult Result(Vol vol)
        {

            Vol v = new Vol();
            v.departure = vol.departure;
            v.arrival = vol.arrival;
            /*v.flight_date_d = Convert.ToDateTime("01/11/2022 00:00:00");*/
            /*v.flight_date_a = Convert.ToDateTime("18/11/2022 00:00:00");*/
            v.flight_date_d = vol.flight_date_d;
            v.flight_date_a = vol.flight_date_a;
            v.adults = vol.adults;
            v.children = vol.children;
            v.infants = vol.infants;

            System.Diagnostics.Debug.WriteLine("------------dep   " + vol.departure);
            System.Diagnostics.Debug.WriteLine("------------arv   " + vol.arrival);
            System.Diagnostics.Debug.WriteLine("------------deb   " + vol.flight_date_d);
            System.Diagnostics.Debug.WriteLine("------------ret   " + vol.flight_date_a);
            System.Diagnostics.Debug.WriteLine("------------adu   " + vol.adults);
            System.Diagnostics.Debug.WriteLine("------------child   " + vol.children);
            System.Diagnostics.Debug.WriteLine("------------enf   " + vol.infants);


            v.flight_type = vol.flight_type;
            v.travel_class = vol.travel_class;
            v.non_stop = vol.non_stop;
            System.Diagnostics.Debug.WriteLine("------------class   " + vol.travel_class);

            System.Diagnostics.Debug.WriteLine("------------non_stop   " + vol.non_stop);


            if (v.flight_type == "Roundtrip")
            {

                var date_dep = v.flight_date_d.ToString("yyyy-MM-dd");
                var date_arr = v.flight_date_a.ToString("yyyy-MM-dd");
                System.Diagnostics.Debug.WriteLine(date_arr);
                System.Diagnostics.Debug.WriteLine(date_dep);

                var resp = getFlightsRoundtrip(v.departure, v.arrival, date_dep, date_arr, v.adults, v.children, v.infants, v.travel_class, v.non_stop);
                System.Diagnostics.Debug.WriteLine("VOILA"+resp);
                ViewBag.Message = resp;
                var mydataa = ViewBag.Message.data;
                foreach (var obj in mydataa)
                {
                    foreach(var it in obj.itineraries)
                    {
                        foreach (var seg in it.segments)
                        {
                            var mycodeAller = getAirportFullName(seg.departure.iataCode.ToString());
                            seg.departure.iataCode = mycodeAller;
                            var mycodeArrivER = getAirportFullName(seg.arrival.iataCode.ToString());
                            seg.arrival.iataCode = mycodeArrivER;

                        }

                    }
                }
                
                ViewBag.Vol = v;
                return View("AllerRetour");
            }
            else
            {
                var date_dep = v.flight_date_d.ToString("yyyy-MM-dd");
                var resp = getFlightsOneWay(v.departure, v.arrival, date_dep, v.adults, v.children, v.infants, v.travel_class, v.non_stop);

                ViewBag.Message = resp;
                var mydataa = ViewBag.Message.data;

                foreach (var obj in mydataa)
                {
                    foreach (var it in obj.itineraries)
                    {
                        foreach (var seg in it.segments)
                        {
                            var mycodeAller = getAirportFullName(seg.departure.iataCode.ToString());
                            seg.departure.iataCode = mycodeAller;
                            var mycodeArrivER = getAirportFullName(seg.arrival.iataCode.ToString());
                            seg.arrival.iataCode = mycodeArrivER;

                        }

                    }
                }
                ViewBag.Vol = v;
                return View("Aller");
            }

        }


        public Object getFlightsOneWay(string depart, string arriver, string d, int adults, int children, int infants, string travel_class, string non_stop)
        {
            string accessToken = getAccessToken();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.BaseAddress = new Uri("https://test.api.amadeus.com/v2/shopping/");

            var obj = httpClient.GetAsync("flight-offers?originLocationCode=" + depart + "&destinationLocationCode=" + arriver + "&departureDate=" + d + "&adults=" + adults + "&children=" + children + "&infants=" + infants + "&travelClass=" + travel_class + "&nonStop=" + non_stop).Result;
            System.Diagnostics.Debug.WriteLine("VOILAA RRSULTAT");
            System.Diagnostics.Debug.WriteLine(obj);
            if (obj.IsSuccessStatusCode)
            {
                var response = obj.Content.ReadAsStringAsync().Result;

                Api list = JsonConvert.DeserializeObject<Api>(response);

                System.Diagnostics.Debug.WriteLine("resultat  : " + list);
                System.Diagnostics.Debug.WriteLine("resultat  : " + list.data);
                foreach (var item in list.data)
                {
                    System.Diagnostics.Debug.WriteLine("resultat  : " + item);
                }
                return list;
            }


            return "error ... ";
        }
        public Object getFlightsRoundtrip(string depart, string arriver, string date_dep, string date_arr, int adults, int children, int infants, string travel_class, string non_stop)
        {
            string accessToken = getAccessToken();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.BaseAddress = new Uri("https://test.api.amadeus.com/v2/shopping/");

            var obj = httpClient.GetAsync("flight-offers?originLocationCode=" + depart + "&destinationLocationCode=" + arriver + "&departureDate=" + date_dep + "&returnDate=" + date_arr + "&adults=" + adults + "&children=" + children + "&infants=" + infants + "&travelClass=" + travel_class + "&nonStop=" + non_stop).Result;

            if (obj.IsSuccessStatusCode)
            {
                var response = obj.Content.ReadAsStringAsync().Result;

                Api list = JsonConvert.DeserializeObject<Api>(response);

                System.Diagnostics.Debug.WriteLine("resultat  : " + list);
                System.Diagnostics.Debug.WriteLine("resultat  : " + list.data);
                foreach (var item in list.data)
                {
                    System.Diagnostics.Debug.WriteLine("resultat  : " + item);
                }
                return list;
            }


            return "error ... ";
        }
        public string getAccessToken()
        {
            string accessToken = "";
            HttpClient httpClient = new HttpClient();

            Confidentialite credentials = new Confidentialite();
            credentials.grant_type = "client_credentials";
            credentials.client_id = "Ty75lfgRjecDiCsJezAe8FsLmtGvRUfX";
            credentials.client_secret = "nwGaOjGJqs4TrAOF";

            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("client_id", "Ty75lfgRjecDiCsJezAe8FsLmtGvRUfX"));
            nvc.Add(new KeyValuePair<string, string>("client_secret", "nwGaOjGJqs4TrAOF"));
            nvc.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

            var req = new HttpRequestMessage(HttpMethod.Post, "https://test.api.amadeus.com/v1/security/oauth2/token")
            {
                Content = new FormUrlEncodedContent(nvc)
            };

            using (HttpResponseMessage result = httpClient.SendAsync(req).Result)
            {
                string resultJson = result.Content.ReadAsStringAsync().Result;
                Jeton list = JsonConvert.DeserializeObject<Jeton>(resultJson);
                accessToken = list.access_token;
                System.Diagnostics.Debug.WriteLine("resultat  TOKEN: {} = " + list.access_token);
            }



            return accessToken;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
