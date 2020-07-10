using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Resources;

namespace BoincETL
{
    class Program
    {
        public struct BoincData
        {
            public BoincData(string src, string fname, DateTime last)
            {
                projectSource = src;
                filename = fname;
                lastDay = last;
            }
            public string projectSource { get; set; }
            public string filename { get; set; }
            public DateTime lastDay { get; set; }
        }

        private static List<BoincData> data = new List<BoincData>();
        private static List<daily_statistic> daily_Statistics = new List<daily_statistic>();

        static void LoadMaxDates(MySqlCommand sqlCommand)
        {
            using (MySqlDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    data.Add(new BoincData(reader.GetString("projectSource"), reader.GetString("filename"), reader.GetDateTime("LastDates")));
                }
            }
        }
        static void Main(string[] args)
        {
            ResourceManager resourceManager = new ResourceManager("BoincETL.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            try
            {
                using (MySqlConnection conn = new MySqlConnection(resourceManager.GetString("ConnectionString")))
                {
                    using (MySqlCommand mySqlCommand = new MySqlCommand("getBoincParams", conn))
                    {
                        mySqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        mySqlCommand.Connection.Open();
                        LoadMaxDates(mySqlCommand);
                        foreach (BoincData item in data)
                        {
                            LoadData(item.filename, item.lastDay, item.projectSource);
                        }
                        mySqlCommand.CommandText = BuildInsert();
                        _ = mySqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine(ex.Message);
                //_ = Console.ReadKey();
            }
        }


        private static string BuildInsert()
        {
            StringBuilder stringBuilder = new StringBuilder("INSERT INTO boinc_data (`ID`,`day`,`user_total_credit`,`user_expavg_credit`,`host_total_credit`,`host_expavg_credit`,`projectSource`) VALUES ");
            foreach (daily_statistic item in daily_Statistics)
            {
                _ = stringBuilder.Append(item + ",");
            }
            _ = stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.Append(";").ToString();
        }

        private static DateTime epoch2dt(double inDate) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(inDate);

        private static void LoadData(string path, DateTime last, string proj)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlNodeList xmlNodes = xmlDocument.GetElementsByTagName("daily_statistics");
            foreach (XmlNode statistics in xmlNodes)
            {
                DateTime dateIn = epoch2dt(double.Parse(statistics["day"].InnerText));
                if (dateIn > last)
                {
                    daily_Statistics.Add(new daily_statistic(dateIn, double.Parse(statistics["user_total_credit"].InnerText), double.Parse(statistics["user_expavg_credit"].InnerText), double.Parse(statistics["host_total_credit"].InnerText), double.Parse(statistics["host_expavg_credit"].InnerText), proj));
                }
            }
        }

        class daily_statistic
        {
            public daily_statistic(DateTime day, double user_total_credit, double user_expavg_credit, double host_total_credit, double host_expavg_credit, string projectSource)
            {
                this.day = day;
                this.user_total_credit = user_total_credit;
                this.user_expavg_credit = user_expavg_credit;
                this.host_total_credit = host_total_credit;
                this.host_expavg_credit = host_expavg_credit;
                this.projectSource = projectSource;
            }

            public string projectSource { get; set; }
            public DateTime day { get; set; }
            public double user_total_credit { get; set; }
            public double user_expavg_credit { get; set; }
            public double host_total_credit { get; set; }
            public double host_expavg_credit { get; set; }
            public override string ToString() => string.Format("(ordered_uuid(UUID()), '{0}',{1},{2},{3},{4},'{5}')", day.ToString("yyyy-MM-dd"), user_total_credit, user_expavg_credit, host_total_credit, host_expavg_credit, projectSource);



        }
    }
}
