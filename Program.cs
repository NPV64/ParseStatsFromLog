using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using lab2;


namespace lab2
{
    class Document
    {

        private string _date = null;

        private DateTime _time = DateTime.Now;

        private string _filename { get; set; }

        public string logname = "log.xml";

        public Document()
        {
        }

        public Document(string _filename)
        {
            this._filename = _filename;
        }

        Dictionary<string, Urlinfo> UrlCountD = new Dictionary<string, Urlinfo>();
        Dictionary<string, int> CountryCountD = new Dictionary<string, int>();
        List<Urlinfo> AllZapL = new List<Urlinfo>();
        List<Urlinfo> ZapL = new List<Urlinfo>();

        public void Load()
        {
            CountrySearch cr = new CountrySearch();
            cr.LoadBase();
            XDocument doc = XDocument.Load(_filename);
            foreach (XElement el in doc.Root.Elements())
            {
                Urlinfo fild = new Urlinfo();
                fild.date = el.Attribute("access-date").Value;
                string proctime = el.Attribute("process-time").Value;
                fild.proc_time = Convert.ToDouble(proctime.Replace("ms", "").Replace(".", ","));
                fild.status = Convert.ToInt32(el.Attribute("status").Value);
                fild.url = el.Attribute("url").Value;
                cr.ip = el.Attribute("remote-addr").Value;
                fild.country = cr.SearchCountry();
                AllZapL.Add(fild);
            }
        }

        public void UrlCount()

        {
            for (int i = 0; i < ZapL.Count; i++)
            {

                if (UrlCountD.ContainsKey(ZapL[i].url))
                {
                    if ((ZapL[i].status >= 200) && (ZapL[i].status <= 400))
                    {
                        UrlCountD[ZapL[i].url].avgtime = (UrlCountD[ZapL[i].url].avgtime + ZapL[i].proc_time)/2;
                        if (ZapL[i].proc_time > UrlCountD[ZapL[i].url].peak)
                            UrlCountD[ZapL[i].url].peak = ZapL[i].proc_time;
                    }
                    else if (ZapL[i].status == 500)
                    {
                        UrlCountD[ZapL[i].url].status500++;
                    }
                }               
                else
                {
                    Urlinfo url = new Urlinfo();
                    if ((ZapL[i].status >= 200) && (ZapL[i].status <= 400))
                    {
                        url.url = ZapL[i].url;
                        url.avgtime = ZapL[i].proc_time;
                        url.peak = ZapL[i].proc_time;
                    }
                    else if (ZapL[i].status == 500)
                    {
                        url.url = ZapL[i].url;
                        url.status500++;
                    }
                    else
                    {
                        continue;
                    }
                    UrlCountD.Add(ZapL[i].url, url);
                }
            }
            Load:
            XDocument doc = XDocument.Load(logname);
            XElement log = doc.Element("results");
            try
            {
            XElement TimesByUrl = log.Element("times-by-url");         
            XAttribute date = new XAttribute("date", _date);
            TimesByUrl.Add(date);
            foreach (string key in UrlCountD.Keys)
            {
                string status = "BAD";
                if (UrlCountD[key].avgtime != 0)
                {
                    if ((UrlCountD[key].peak/UrlCountD[key].avgtime) < 15) status = "AVERAGE";
                    if ((UrlCountD[key].peak/UrlCountD[key].avgtime) < 5) status = "GOOD";
                    XElement Time = new XElement("time", status);
                    XAttribute url = new XAttribute("url", UrlCountD[key].url);
                    XAttribute avg = new XAttribute("avg", UrlCountD[key].avgtime);
                    XAttribute peak = new XAttribute("peak", UrlCountD[key].peak);
                    Time.Add(url);
                    Time.Add(avg);
                    Time.Add(peak);
                    TimesByUrl.Add(Time);
                }
            }
                XElement ProblemsByurl = log.Element("problems-by-url");
                ProblemsByurl.Add(date);
                foreach (string key500 in UrlCountD.Keys)
                {
                    if (UrlCountD[key500].status500 > 0)
                    {
                        XElement Problem = new XElement("problem");
                        XAttribute count = new XAttribute("count", UrlCountD[key500].status500);
                        XAttribute url500 = new XAttribute("url", UrlCountD[key500].url);
                        XAttribute status500 = new XAttribute("status", 500);
                        Problem.Add(url500);                    
                        Problem.Add(status500);
                        Problem.Add(count);
                        ProblemsByurl.Add(Problem);
                    }
                }
            }
            catch (Exception)
            {
                XElement TimesByUrl = new XElement("times-by-url");
                XElement ProblemsByurl = new XElement("problems-by-url");
                log.Add(TimesByUrl);
                log.Add(ProblemsByurl);
                doc.Save(logname);
                goto Load;
            }                                   
            doc.Save(logname);
        }

        public void CountryCount()
        {
            for (int i = 0; i < ZapL.Count; i++)
            {
                if (CountryCountD.ContainsKey(ZapL[i].country))
                {
                    CountryCountD[ZapL[i].country]++;
                }
                else
                {
                    CountryCountD.Add(ZapL[i].country,1);
                }
            }
            XDocument doc = XDocument.Load(logname);
            XElement log = doc.Element("results");
            XAttribute date = new XAttribute("date", _date);
            XElement RequestByCountryDate = new XElement("requests-by-country");       
            RequestByCountryDate.Add(date);
                
            foreach (string Country in CountryCountD.Keys)
            {
                XAttribute CountryA = new XAttribute("Country",Country);
                XElement CountryReq = new XElement("request-by-country", CountryCountD[Country]);
                CountryReq.Add(CountryA);
                RequestByCountryDate.Add(CountryReq);                
            }
            log.Add(RequestByCountryDate);
            doc.Save(logname);
        }

        public void CreateLog()
        {
            string fulldate;
            string date = null;
            string load_date = null;
            int hour = 0;
            int[] count_by_hour = new int[23];
            double[] proc_time = new double[23];
            int count_list = 0;
            foreach (var zap in ZapL)
            {              
                lastlist:
                count_list++;          
                if (date == null)
                {
                    fulldate=zap.date;
                    hour= Int32.Parse(fulldate.Substring(11, 2));
                    date = fulldate.Substring(0, 10);
                    load_date = fulldate.Substring(0, 10);
                }
                else
                {
                    fulldate = zap.date;
                    hour = Int32.Parse(fulldate.Substring(11, 2));
                    load_date = fulldate.Substring(0, 10);
                }
                if (date == load_date)
                {
                    count_by_hour[hour]++; 
                    proc_time[hour] += zap.proc_time;
                }
                else
                {
                    SaveLog( count_by_hour, proc_time);
                    for (int i = 0; i < 23; i++)
                    {
                        count_by_hour[i] = 0;
                    }
                    date = null;   
                    if (count_list == ZapL.Count) goto lastlist;
                }
            }      
            SaveLog(count_by_hour, proc_time);
        }                         

        public void SaveLog(int[] count_by_hour, double[] proc_time)
        {            
            try
            {
                XDocument doc = XDocument.Load(logname);
                XElement log = doc.Element("results");
                XElement AveragesByHourDate = log.Element("averages-by-hour");
                XElement AveragesByHour = log.Element("averages-by-hour_");
                XElement RequestCountsDate = log.Element("request-counts");
                XAttribute date = new XAttribute("date",_date);
                double count_by_date = 0;
                double sumprictime = 0;
                
                for (int i = 0; i < 23; i++)
                {        
                               
                    count_by_date += count_by_hour[i];
                    sumprictime += proc_time[i];  //  Такой способ?
                }


                //    double avg_by_date =Math.Round((sumprictime / count_by_date), 1) ;
                double avg_by_date = Math.Round(( count_by_date/(3600*24)), 3);




                for (int i = 0; i < 23; i++)
                {
                  //  proc_time[i] /= count_by_hour[i];
                    proc_time[i] = count_by_hour[i]/3600;
                }


                AveragesByHourDate.Add(date);
                for (int i = 0; i < 23; i++)
                   if (proc_time[i] > 0)
                    {
                        XElement AverageHour = new XElement("average",Math.Round(proc_time[i],3)); 
                        XAttribute hour = new XAttribute("hour", i);
                        AverageHour.Add(hour);                       
                        AveragesByHourDate.Add(AverageHour);
                    }

               // if ( )
                {


                    XElement AverageDate = new XElement("average", avg_by_date);
                    AverageDate.Add(date);
                    AveragesByHour.Add(AverageDate);
                }


                RequestCountsDate.Add(date);
                for (int i = 0; i < 23; i++)
                    if (count_by_hour[i] > 0)
                    {
                        XElement RequestCountHour = new XElement("request-count", count_by_hour[i]);
                        XAttribute hour = new XAttribute("hour",i);
                        RequestCountHour.Add(hour);                        
                        RequestCountsDate.Add(RequestCountHour);
                    }



                doc.Save(logname);
            }
            catch (Exception ex)
            {
                XDocument doc = new XDocument();
                XElement log = new XElement("results");
                XElement AveragesByHourDate = new XElement("averages-by-hour");
                XElement AveragesByHour = new XElement("averages-by-hour_");
                XElement RequestCountsDate = new XElement("request-counts");
                XElement AverageDate = new XElement("average");
                log.Add(AveragesByHourDate);
                log.Add(AveragesByHour);
                log.Add(RequestCountsDate);
                doc.Add(log);
                doc.Save(logname);
            //    Console.WriteLine(ex);
                SaveLog( count_by_hour, proc_time);
            }                                      
        }

        public void NewDate()
        {
            foreach (var zap in AllZapL)
            {

                if (_date == null) _date = zap.date.Substring(0,10);
                if(_date==zap.date.Substring(0, 10)) ZapL.Add(zap);
                else
                {
                    logname = _date + ".xml";
                    CreateLog();
                    CountryCount();
                    UrlCount();
                    ZapL.Clear();
                    CountryCountD.Clear();
                    UrlCountD.Clear();
                    _date = zap.date.Substring(0, 10); 
                    ZapL.Add(zap);
                }
            }
            logname = _date+".xml";
            CreateLog();
            CountryCount();
            UrlCount();
        }

       



        class Program
        {
            static void Main(string[] args)
            {
                Document dc = new Document("log-example.xml");
                dc.Load();               
                dc.NewDate();            
                Console.WriteLine("ProcessTime = {0}", Convert.ToString(dc._time-DateTime.Now).Substring(8, 4)+"ms");
                Console.ReadKey();
            }
        }
    }
}
