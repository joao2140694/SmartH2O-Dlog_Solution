using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ConsoleApplication5
{

    class Program
    {
        static String ipAddress = ConfigurationSettings.AppSettings["ipAddressMessagingChannel"];
        static String xsdAlarmPath = ConfigurationSettings.AppSettings["alarmsXSD"];
        static String xsdSignalPath = ConfigurationSettings.AppSettings["dataSensorsXSD"];
        static List<string> topicsList = new List<string>(ConfigurationSettings.AppSettings["topics"].Split(new char[] { ';' }));
        static private string strValidateMsg;
        static private bool isValid = true;

        private const string ALARMS_FILE_NAME = "alarms-data.xml";
        private const string PARAM_FILE_NAME = "param-data.xml";


        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {

            Console.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(Encoding.UTF8.GetString(e.Message));

            if (!validateXml(xml, e.Topic))
            {
                Console.WriteLine("Invalid XML Structure\n");
                return;
            }

            XmlNode node = xml.SelectSingleNode(".");

            adicionarAoLog(node, e.Topic);

        }

        #region VALIDATE XML with SCHEMA
        private static bool validateXml(XmlDocument xml, object topic)
        {
            if (topic.Equals("alarms"))
            {
                xml.Schemas.Add(null, @xsdAlarmPath);
                ValidationEventHandler handler = new ValidationEventHandler(MyValidationMethod);

                xml.Validate(handler);

                if (isValid)
                {
                    Console.WriteLine("Alarm structure: OK\n");
                }
                else
                {
                    Console.WriteLine("Alarm structure: INVALID\n" + strValidateMsg);
                }
            }

            if (topic.Equals("dataSensors"))
            {
                xml.Schemas.Add(null, @xsdSignalPath);
                ValidationEventHandler handler = new ValidationEventHandler(MyValidationMethod);

                xml.Validate(handler);

                if (isValid)
                {
                    Console.WriteLine("Signal structure: OK\n");
                }
                else
                {
                    Console.WriteLine("Signal structure: INVALID\n" + strValidateMsg);
                }
            }
            return isValid;
        }

        private static void MyValidationMethod(object sender, ValidationEventArgs args)
        {
            isValid = false;

            switch (args.Severity)
            {
                case XmlSeverityType.Error:
                    strValidateMsg = "Error: " + args.Message;
                    break;
                case XmlSeverityType.Warning:
                    strValidateMsg = "Warning: " + args.Message;
                    break;
                default:
                    break;
            }
        }
        #endregion

        private static bool validateLogWithSchema(XmlDocument xml, String path)
        {

            xml.Schemas.Add(null, @path);
            ValidationEventHandler handler = new ValidationEventHandler(MyValidationMethod);

            xml.Validate(handler);

            if (isValid)
            {
                Console.WriteLine("XML Structure: OK\n");
            }
            else
            {
                Console.WriteLine("XML Structure: INVALID\n" + strValidateMsg);
            }

            return isValid;
        }

        #region CREATE or ADD to existing LOG
        private static void adicionarAoLog(XmlNode node, string topic)
        {
            //hardCoded porque o professor especificou estes nomes
            string fileName;
            if (topic == "alarms")
            {
                fileName = ALARMS_FILE_NAME;
            }
            else if (topic == "dataSensors")
            {
                fileName = PARAM_FILE_NAME;
            }
            else
            {
                fileName = topic + "-data.xml";
            }

            string path = ConfigurationSettings.AppSettings[topic + "XSD"];
            Console.WriteLine(path);

            if (!File.Exists(fileName)) // Se o ficheiro nao existe e necessario criar um novo..
            {
                criarXmlBase(fileName, topic);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            if (!validateLogWithSchema(doc, path))
            {
                Console.WriteLine("Invalid XML Structure\n");
                return;
            }

            XmlDocumentFragment xfrag = doc.CreateDocumentFragment();
            xfrag.InnerXml = node.OuterXml;

            doc.DocumentElement.AppendChild(xfrag);
            doc.Save(fileName);

        }

        private static void criarXmlBase(string fileName, string rootElementName)
        {
            //novo xml
            XmlDocument novXml = new XmlDocument();

            //nova declaracao
            XmlDeclaration dec = novXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            novXml.AppendChild(dec);

            // Criacao do elemeto root
            XmlElement root = novXml.CreateElement(rootElementName);
            novXml.AppendChild(root);

            novXml.Save(fileName);
        }
        #endregion

        static void Main(string[] args)
        {
            //TODO criar log
            MqttClient m_cClient = new MqttClient(IPAddress.Parse(ipAddress));

            string[] m_strTopicsInfo = new string[topicsList.Count];
            for (int i = 0; i < topicsList.Count; i++)
            {
                m_strTopicsInfo[i] = topicsList[i];
            }

            //string[] m_strTopicsInfo = { "dataSensors", "alarms" };


            m_cClient.Connect(Guid.NewGuid().ToString());
            if (!m_cClient.IsConnected)
            {
                Console.WriteLine("Error connecting to message broker...");
                return;
            }
            //Specify events we are interest on
            //New Msg Arrived
            m_cClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            //Subscribe to topics
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };//QoS
            m_cClient.Subscribe(m_strTopicsInfo, qosLevels);


            Console.ReadKey();


            if (m_cClient.IsConnected)
            {
                m_cClient.Unsubscribe(m_strTopicsInfo); //Put this in a button to see notif!
                m_cClient.Disconnect(); //Free process and process's resources
            }
        }







        /*
            static void Main(string[] args)
            {
                MqttClient m_cClient = new MqttClient(IPAddress.Parse("192.168.237.214")); //este ip é do prof //127.0.0.1 local host
                string[] m_strTopicsInfo = { "news", "complaints" };

                m_cClient.Connect(Guid.NewGuid().ToString());
                if (!m_cClient.IsConnected)
                {
                    Console.WriteLine("Error connecting to message broker...");
                    return;
                }
                //Specify events we are interest on
                //New Msg Arrived
                m_cClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                //This client's subscription operation id done
                //m_cClient.MqttMsgSubscribed += client_MqttMsgSubscribed;
                //This client's unsubscription operation is done
                //m_cClient.MqttMsgUnsubscribed += client_MqttMsgUnsubscribed;
                //Subscribe to topics
                byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };//QoS
                m_cClient.Subscribe(m_strTopicsInfo, qosLevels);


            }

            void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)

            {
                Console.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " +
                e.Topic);
            }

            void client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
            {
                Console.WriteLine("UNSUBSCRIBED WITH SUCCESS");
            }
            void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
            {
                Console.WriteLine("SUBSCRIBED WITH SUCCESS");
            }
        }*/
    }
}
