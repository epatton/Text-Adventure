using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace TextAdventure.Core.Helpers
{
    public class SaveHandler
    {
        public string CompanionsFile { get; set; }
        public string ItemsFile { get; set; }
        public string ScreensFile { get; set; }
        public string ShopsFile { get; set; }

        public SaveHandler()
        {
            CompanionsFile = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\Companions.xml";
            ItemsFile = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\Items.xml";
            ScreensFile = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\Screens.xml";
            ShopsFile = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\Shops.xml";
        }

        public void Save(object data, Type type, string filePath)
        {
            if (!File.Exists(filePath))
                File.Create(filePath);

            var convertedData = Convert.ChangeType(data, type);
            var xs = new XmlSerializer(type);
            TextWriter txtWriter = new StreamWriter(filePath);
            xs.Serialize(txtWriter, convertedData);
            txtWriter.Close();
        }

        public object Load(Type type, string filePath)
        {
            try
            {
                object data = null;
                XmlSerializer serializer = new XmlSerializer(type);
                StreamReader reader = new StreamReader(filePath);
                data = Convert.ChangeType(serializer.Deserialize(reader), type);
                reader.Close();

                return data;
            }
            catch (Exception e)
            {
                //some error happened
            }

            return null;
        }
    }
}
