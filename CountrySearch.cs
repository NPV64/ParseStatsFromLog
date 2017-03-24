using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace lab2
{
    class CountrySearch
    {
        public string ip { get; set; }
        List<IpInfo> linefileList = new List<IpInfo>();
        public string SearchCountry()
        {
            long ip = IpLong(this.ip);
            int begin = 0;
            int end = linefileList.Count ;
            int mid = (begin + end)/2;
            while (begin < end)
            {
                if ((ip < linefileList[mid].EndIp)&&(ip>linefileList[mid].BeginIp)) return linefileList[mid].Country;
                if (ip < linefileList[mid].EndIp)
                {
                    end = mid;
                    mid = (begin + end)/2;
                }
                else
                {
                    begin = mid;
                    mid = (begin + end)/2;
                }
                if (begin == mid) return linefileList[begin].Country;
                if (end == mid) return linefileList[end].Country;
            }               
            return "ERROR";
        }

        public void LoadBase()
        {
            string path = "GeoIPCountryWhois.csv";

            try
            {
                using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
                {
                    string line;
                    line = sr.ReadLine();
                    while ((line = sr.ReadLine()) != null)
                    {
                        var linefile = line.Split(',');
                        IpInfo ipfild = new IpInfo();
                        ipfild.BeginIp = IpLong(linefile[0]);
                        ipfild.EndIp = IpLong(linefile[1]);
                        ipfild.Country = linefile[4];
                        linefileList.Add(ipfild);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public long IpLong(string ip)
        {
            var ipActect = ip.Split('.');
            return (UInt32.Parse(ipActect[0]) * 255 * 255 * 255) + (UInt32.Parse(ipActect[1]) * 255 * 255) + (UInt32.Parse(ipActect[2]) * 255) + (UInt32.Parse(ipActect[3]));
        }
    }
}
