using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ConsoleApplication5
{

    class Program
    {
        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {

            Console.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);

            //TODO: falta validar o schmea.. ou o caralho...

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(Encoding.UTF8.GetString(e.Message));

            XmlNode node = xml.SelectSingleNode(".");

            adicionarAoLog(node, e.Topic);

        }

        private static void adicionarAoLog(XmlNode node, string topic)
        {

            string fileName = topic + ".xml";

            if (!File.Exists(fileName)) // Se o ficheiro nao existe e necessario criar um novo..
            {
                criarXmlBase(fileName, topic);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

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

        static void Main(string[] args)
        {

            //test for git

            //TODO: FALTA CRIAR A CONFIGUARACAO
            // ADICIONAR LA O IP
            // ADICIONAR OS TOPICOS A SUBSCREVER...

            // TODO: IMPRIMIR UM INTRODUCAO SOBRE O QUE ESTA A FAZER, EM QUE IP E QUAIS OS CANAIS SUBSCRITOS


            MqttClient m_cClient = new MqttClient(IPAddress.Parse("192.168.237.193"));
            string[] m_strTopicsInfo = { "dataSensors", "alarms" };


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
