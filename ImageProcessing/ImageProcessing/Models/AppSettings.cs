using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImageProcessing.Models
{
    public class AppSettings
    {
        protected XDocument _settings;
        protected string settingsFileName = "";

        public int BrightAreaThreshold { get; set; }
        public int DarkAreaThreshold { get; set; }
        public int Iterations { get; set; }
        public int Iterations2 { get; set; }
        public double Gamma { get; set; }
        public int Clusters { get; set; }
        public double LevelGamma { get; set; }
        public double LevelMax { get; set; }
        public double LevelMin { get; set; }
        public double ABFTheta { get; set; }
        public double ClipLimit { get; set; }

        public AppSettings()
        {
            string currentDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            settingsFileName = currentDirectory + @"\settings.config";
        }

        public bool Load()
        {
            bool result = false;
            try
            {
                _settings = XDocument.Load(settingsFileName);
                foreach (var prop in this.GetType().GetProperties())
                {
                    prop.SetValue(this, Convert.ChangeType(Get(prop.Name), prop.PropertyType,
                        System.Globalization.CultureInfo.InvariantCulture));

                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        protected object Get(string name)
        {
            object res = null;

            var field = _settings.Descendants("setting")
                                    .Where(x => (string)x.Attribute("name") == name)
                                    .FirstOrDefault();

            if (field != null)
            {
                res = field.Element("value").Value;
            }
            else
                throw new Exception("Property not found in Settings");

            return res;
        }

        protected void Set(string name, object value)
        {
            var field = _settings.Descendants("setting")
                                    .Where(x => (string)x.Attribute("name") == name)
                                    .FirstOrDefault();

            if (field != null)
            {
                field.Element("value").Value = value.ToString();
            }
            else
                throw new Exception("Property not found in Settings");
        }

        public void Save()
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                Set(prop.Name, prop.GetValue(this));
            }
            _settings.Save(settingsFileName);

        }
    }
}
