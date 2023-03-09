using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Collections.Generic;
using CefSharp;
using CefSharp.DevTools.DOM;
using CefSharp.DevTools.IndexedDB;
using CefSharp.OffScreen;
using System.Xml.Linq;
using System.Threading;
using System.IO;
using System.Diagnostics;
using CefSharp.Web;
using System.Net;
using System.Collections;
using System.Globalization;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using CefSharp.DevTools.Accessibility;
using CefSharp.DevTools.IO;

namespace myProject
{
    internal class Program
    {
        public class PriceValue
        {            
            public double Value { get; set; }
        }
        public class specificData
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        static void Main(string[] args)
        {
            Cef.Initialize(new CefSettings());
            //Model S
            //Gather all data for all cars on the first 2 pages.
            HtlmtoParse("https://www.cars.com/shopping/results/?page=1&page_size=20&list_price_max=100000&makes[]=tesla&maximum_distance=all&models[]=tesla-model_s&stock_type=used&zip=94596", "Model_S_1_Page.json");
            HtlmtoParse("https://www.cars.com/shopping/results/?page=2&page_size=20&list_price_max=100000&makes[]=tesla&maximum_distance=all&models[]=tesla-model_s&stock_type=used&zip=94596", "Model_S_2_Page.json");

            //Choose a specific car and gather specific car data..
            HtlmtoParse2("https://www.cars.com/vehicledetail/72be69da-11d8-46e0-9e16-5aa078da8cf7/", "Model_S_specificcar.json");
            //Home delivery and gather all data.
            HtlmtoParse2("https://www.cars.com/vehicledetail/9f38cefd-8ef9-4d75-8dda-9fc2fea9fefa/", "Model_S_homedelivery.json");

            //Model X
            HtlmtoParse("https://www.cars.com/shopping/results/?page=1&page_size=20&dealer_id=&home_delivery=true&keyword=&list_price_max=100000&list_price_min=&makes[]=tesla&maximum_distance=all&mileage_max=&models[]=tesla-model_s&sort=best_match_desc&stock_type=used&year_max=&year_min=&zip=94596", "ModelX_1_Page.json");
            HtlmtoParse("https://www.cars.com/shopping/results/?page=2&page_size=20&dealer_id=&home_delivery=true&keyword=&list_price_max=100000&list_price_min=&makes[]=tesla&maximum_distance=all&mileage_max=&models[]=tesla-model_s&sort=best_match_desc&stock_type=used&year_max=&year_min=&zip=94596", "ModelX_2_Page.json");
            //Choose a specific car and gather specific car data..
            HtlmtoParse2("https://www.cars.com/vehicledetail/72be69da-11d8-46e0-9e16-5aa078da8cf7/", "Model_X_specificcar.json");
            //Home delivery and gather all data.
            HtlmtoParse2("https://www.cars.com/vehicledetail/9f38cefd-8ef9-4d75-8dda-9fc2fea9fefa/", "Model_X_homedelivery.json");

        }
        public static string InitializeChromium(string link, string fileName)
        { 
            var browser = new ChromiumWebBrowser(link);
            browser.Load(link);
            browser.WaitForInitialLoadAsync();
            Thread.Sleep(3000);
            string html = browser.GetSourceAsync().Result;
            return html;
        }
        public static void HtlmtoParse(string link,string fileName)
        { 
            using (WebClient client = new WebClient()) 
            { 
                
                string html = InitializeChromium(link,fileName);

                var uri = new Uri(link); 
                var host = uri.Host; 
                var scheme = uri.Scheme; 
                client.Encoding = Encoding.UTF8;

                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(html);

                HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//span[@class='primary-price']");//"//span[@class='primary-price']"

                List<PriceValue> list = new List<PriceValue>();
                decimal priceSum = 0;    
                if (htmlNodes != null)
                {                    
                    foreach (HtmlNode node in htmlNodes)
                    {                       
                        HtmlAgilityPack.HtmlDocument _subDocument = new HtmlAgilityPack.HtmlDocument();
                        _subDocument.LoadHtml(node.InnerHtml);
                        PriceValue price = new PriceValue();
                        price.Value = Convert.ToDouble(node.InnerHtml.Replace("$","").ToString(), new CultureInfo("en-US"));
                        list.Add(price);
                        priceSum = + Convert.ToDecimal(node.InnerHtml.Replace("$", "").ToString());
                    }
                }
               
                string json = JsonConvert.SerializeObject(list); 
                Console.WriteLine(json);
                Console.WriteLine("Toplam:" + priceSum);

                Save(fileName, json);
                //Console.WriteLine("Please press any key to continue..."); Console.ReadKey();
            }
        }
        public static void HtlmtoParse2(string link,string fileName)
        {
            using (WebClient client = new WebClient()) // Html'i indirmek için bir İstemci Oluşturuyoruz.
            {                
                string html = InitializeChromium(link, fileName);

                var uri = new Uri(link); 
                var host = uri.Host; 
                var scheme = uri.Scheme; 
                client.Encoding = Encoding.UTF8; 
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//dl[@class='fancy-description-list']");
                List<specificData> list = new List<specificData>();
               
                if (htmlNodes != null)
                {
                    foreach (HtmlNode node in htmlNodes)
                    {
                        HtmlAgilityPack.HtmlDocument _subDocument = new HtmlAgilityPack.HtmlDocument();
                        _subDocument.LoadHtml(node.InnerHtml);
                        
                        HtmlNodeCollection childNodes = node.ChildNodes;
                        string keyTemp = null; string valueTemp = null;
                        foreach (var cnode in childNodes)
                        {
                            if (cnode.NodeType == HtmlNodeType.Element)
                            {
                                specificData data = new specificData();                                                               
                                if (cnode.EndNode.Name == "dt") { keyTemp   = cnode.InnerText; }
                                if (cnode.EndNode.Name == "dd") { valueTemp = cnode.InnerText; }
                                if(keyTemp != null && valueTemp != null)
                                {
                                    data.Key = keyTemp; 
                                    data.Value = valueTemp;
                                    list.Add(data);
                                    keyTemp = null; valueTemp = null;
                                } 
                            }
                        }
                    }
                }

                string json = JsonConvert.SerializeObject(list);
                Console.WriteLine(json);               
                Save(fileName, json);
                //Console.WriteLine("Please press any key to continue..."); Console.ReadKey();
            }            
        }
        public static void Save(string fileName, string json)
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(
    Environment.SpecialFolder.MyDoc‌​uments), "MyApp");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                if (!System.IO.File.Exists(path))
                {
                    System.IO.File.WriteAllText(path + fileName, json);
                }
                else
                {
                    Console.WriteLine("File \"{0}\" already exists.", fileName);
                    return;
                }
            }
            else
            {
                if (!System.IO.File.Exists(path))
                {
                    System.IO.File.WriteAllText(path + "\\" +fileName , json);
                }
                else
                {
                    Console.WriteLine("File \"{0}\" already exists.", fileName);
                    return;
                }
            }
        }       
    }
}
