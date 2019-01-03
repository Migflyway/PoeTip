using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ninja_Price.API.PoeNinja.Classes;
using tools.Dev;

namespace tools.Dev
{
    public class dowloadninja
    {
        private const string CurrencyURL = "https://poe.ninja/api/data/currencyoverview?type=Currency&league=";
        private const string DivinationCards_URL = "https://poe.ninja/api/data/itemoverview?type=DivinationCard&league=";
        private const string Essences_URL = "https://poe.ninja/api/data/itemoverview?type=Essence&league=";
        private const string Fragments_URL = "https://poe.ninja/api/data/currencyoverview?type=Fragment&league=";
        private const string Prophecies_URL = "https://poe.ninja/api/data/itemoverview?type=Prophecy&league=";
        private const string UniqueAccessories_URL = "https://poe.ninja/api/data/itemoverview?type=UniqueAccessory&league=";
        private const string UniqueArmours_URL = "https://poe.ninja/api/data/itemoverview?type=UniqueArmour&league=";
        private const string UniqueFlasks_URL = "https://poe.ninja/api/data/itemoverview?type=UniqueFlask&league=";
        private const string UniqueJewels_URL = "https://poe.ninja/api/data/itemoverview?type=UniqueJewel&league=";
        private const string UniqueMaps_URL = "https://poe.ninja/api/data/itemoverview?type=UniqueMap&league=";
        private const string UniqueWeapons_URL = "https://poe.ninja/api/data/itemoverview?type=UniqueWeapon&league=";
        private const string WhiteMaps_URL = "https://poe.ninja/api/data/itemoverview?type=Map&league=";
        private const string Resonators_URL = "https://poe.ninja/api/data/itemoverview?type=Resonator&league=";
        private const string Fossils_URL = "https://poe.ninja/api/data/itemoverview?type=Fossil&league=";
        private const string Scarabs_URL = "https://poe.ninja/api/data/itemoverview?type=Scarab&league=";
        public static bool UpdatingFromJson { get; set; } = false;
        public static bool UpdatingFromAPI { get; set; } = false;

        public static bool DownloadDone = false;


        public static NinjaData Ninja;
        private static string NinjaDirectory= ".//NinjaData//";

        public static void GetJsonData(string league)
        {
            Task.Run(() =>
            {
                while (UpdatingFromAPI || UpdatingFromJson)
                {
                    Thread.Sleep(250);
                }
                UpdatingFromAPI = true;
                DownloadDone = false;
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "Currency.json", JsonConvert.DeserializeObject<Currency.RootObject>(ninjaapi.DownloadFromUrl(CurrencyURL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "DivinationCards.json", JsonConvert.DeserializeObject<DivinationCards.RootObject>(ninjaapi.DownloadFromUrl(DivinationCards_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "Essences.json", JsonConvert.DeserializeObject<Essences.RootObject>(ninjaapi.DownloadFromUrl(Essences_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "Fragments.json", JsonConvert.DeserializeObject<Fragments.RootObject>(ninjaapi.DownloadFromUrl(Fragments_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "Prophecies.json", JsonConvert.DeserializeObject<Prophecies.RootObject>(ninjaapi.DownloadFromUrl(Prophecies_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "UniqueAccessories.json", JsonConvert.DeserializeObject<UniqueAccessories.RootObject>(ninjaapi.DownloadFromUrl(UniqueAccessories_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "UniqueArmours.json", JsonConvert.DeserializeObject<UniqueArmours.RootObject>(ninjaapi.DownloadFromUrl(UniqueArmours_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "UniqueFlasks.json", JsonConvert.DeserializeObject<UniqueFlasks.RootObject>(ninjaapi.DownloadFromUrl(UniqueFlasks_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "UniqueJewels.json", JsonConvert.DeserializeObject<UniqueJewels.RootObject>(ninjaapi.DownloadFromUrl(UniqueJewels_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "UniqueMaps.json", JsonConvert.DeserializeObject<UniqueMaps.RootObject>(ninjaapi.DownloadFromUrl(UniqueMaps_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "UniqueWeapons.json", JsonConvert.DeserializeObject<UniqueWeapons.RootObject>(ninjaapi.DownloadFromUrl(UniqueWeapons_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "WhiteMaps.json", JsonConvert.DeserializeObject<WhiteMaps.RootObject>(ninjaapi.DownloadFromUrl(WhiteMaps_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "Resonators.json", JsonConvert.DeserializeObject<Resonators.RootObject>(ninjaapi.DownloadFromUrl(Resonators_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "Fossils.json", JsonConvert.DeserializeObject<Fossils.RootObject>(ninjaapi.DownloadFromUrl(Fossils_URL + league)));
                ninjaapi.Json.SaveSettingFile(NinjaDirectory + "Scarabs.json", JsonConvert.DeserializeObject<Scarab.RootObject>(ninjaapi.DownloadFromUrl(Scarabs_URL + league)));
                UpdatingFromAPI = false;
                DownloadDone = true;
                UpdatePoeNinjaData();
            });
        }

        public static bool JsonExists(string fileName)
        {
            return File.Exists(NinjaDirectory + fileName);
        }
        private static T JsonToClass<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T jsonObject = (T)ser.ReadObject(ms);
            ms.Close();
            return jsonObject;
        }

        private static void UpdatePoeNinjaData()
        {
            Task.Run(() =>
            {
                while (UpdatingFromAPI || UpdatingFromJson)
                {
                    Thread.Sleep(250);
                }
                Ninja = new NinjaData();

                UpdatingFromJson = true;
                if (JsonExists("Currency.json"))
                    using (var r = new StreamReader(NinjaDirectory + "Currency.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.Currency = JsonConvert.DeserializeObject<Currency.RootObject>(json);
                    }

                if (JsonExists("DivinationCards.json"))
                    using (var r = new StreamReader(NinjaDirectory + "DivinationCards.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.DivinationCards = JsonConvert.DeserializeObject<DivinationCards.RootObject>(json);
                    }

                if (JsonExists("Essences.json"))
                    using (var r = new StreamReader(NinjaDirectory + "Essences.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.Essences = JsonConvert.DeserializeObject<Essences.RootObject>(json);
                    }

                if (JsonExists("Fragments.json"))
                    using (var r = new StreamReader(NinjaDirectory + "Fragments.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.Fragments = JsonConvert.DeserializeObject<Fragments.RootObject>(json);
                    }

                if (JsonExists("Prophecies.json"))
                    using (var r = new StreamReader(NinjaDirectory + "Prophecies.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.Prophecies = JsonConvert.DeserializeObject<Prophecies.RootObject>(json);
                    }

                if (JsonExists("UniqueAccessories.json"))
                    using (var r = new StreamReader(NinjaDirectory + "UniqueAccessories.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.UniqueAccessories = JsonConvert.DeserializeObject<UniqueAccessories.RootObject>(json);
                    }

                if (JsonExists("UniqueArmours.json"))
                    using (var r = new StreamReader(NinjaDirectory + "UniqueArmours.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.UniqueArmours = JsonConvert.DeserializeObject<UniqueArmours.RootObject>(json);
                    }

                if (JsonExists("UniqueFlasks.json"))
                    using (var r = new StreamReader(NinjaDirectory + "UniqueFlasks.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.UniqueFlasks = JsonConvert.DeserializeObject<UniqueFlasks.RootObject>(json);
                    }

                if (JsonExists("UniqueJewels.json"))
                    using (var r = new StreamReader(NinjaDirectory + "UniqueJewels.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.UniqueJewels = JsonConvert.DeserializeObject<UniqueJewels.RootObject>(json);
                    }

                if (JsonExists("UniqueMaps.json"))
                    using (var r = new StreamReader(NinjaDirectory + "UniqueMaps.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.UniqueMaps = JsonConvert.DeserializeObject<UniqueMaps.RootObject>(json);
                    }

                if (JsonExists("UniqueWeapons.json"))
                    using (var r = new StreamReader(NinjaDirectory + "UniqueWeapons.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.UniqueWeapons = JsonConvert.DeserializeObject<UniqueWeapons.RootObject>(json);
                    }

                if (JsonExists("WhiteMaps.json"))
                    using (var r = new StreamReader(NinjaDirectory + "WhiteMaps.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.WhiteMaps = JsonToClass<WhiteMaps.RootObject>(json);
                        //Ninja.WhiteMaps = JsonConvert.DeserializeObject<WhiteMaps.RootObject>(json);
                    }

                if (JsonExists("Resonators.json"))
                    using (var r = new StreamReader(NinjaDirectory + "Resonators.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.Resonators = JsonConvert.DeserializeObject<Resonators.RootObject>(json);
                    }

                if (JsonExists("Fossils.json"))
                    using (var r = new StreamReader(NinjaDirectory + "Fossils.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.Fossils = JsonConvert.DeserializeObject<Fossils.RootObject>(json);
                    }

                if (JsonExists("Scarabs.json"))
                    using (var r = new StreamReader(NinjaDirectory + "Scarabs.json"))
                    {
                        var json = r.ReadToEnd();
                        Ninja.Scarab = JsonConvert.DeserializeObject<Scarab.RootObject>(json);
                    }

                UpdatingFromJson = false;
            });
        }
    }
}