using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Element = Bloomberglp.Blpapi.Element;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using CorrelationID = Bloomberglp.Blpapi.CorrelationID;
using Subscription = Bloomberglp.Blpapi.Subscription;

namespace Transposer
{
    class BloombergRealTimeData
    {
        private static readonly Name EXCEPTIONS = new Name("exceptions");
        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name REASON = new Name("reason");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name DESCRIPTION = new Name("description");
        private static readonly Name ERROR_CODE = new Name("errorCode");
        private static readonly Name SOURCE = new Name("source");
        private static readonly Name SECURITY_ERROR = new Name("securityError");
        private static readonly Name MESSAGE = new Name("message");
        private static readonly Name RESPONSE_ERROR = new Name("responseError");
        private static readonly Name SECURITY_DATA = new Name("securityData");
        private static readonly Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
        private static readonly Name ERROR_INFO = new Name("errorInfo");
        private static readonly Name FORCE_DELAY = new Name(" [FD]");

        // BB real-time subscription params
        private SessionOptions _sessionOptions;
        private Session _session;
        private List<Subscription> _subscriptions;
        private Boolean isSubscribed = false;
        readonly Dictionary<string, BloombergSecurity> _securities = new Dictionary<string, BloombergSecurity>();
        private List<string> _fields = new List<string>();



        public BloombergRealTimeData(string Name, string ticker)
        {
            UseDfltFields();
        }

        public BloombergRealTimeData()
        {
            UseDfltFields();
        }

        public void AddSecurity(BloombergSecurity security)
        {
            _securities.Add(security.Ticker, security);
        }

        private void UseDfltFields()
        {
            _fields = new List<string> { "Security", "ASK", "LAST_PRICE", "BID"};
        }

        public void SetFields(List<string> fields)
        {
            if ((fields == null) || (fields.Count <= 0)) return;
            _fields = fields;
        }

        public bool CreateSession()
        {
            const string serverHost = "localhost";
            const int serverPort = 8194;

            // set sesson options
            _sessionOptions = new SessionOptions();
            _sessionOptions.ServerHost = serverHost;
            _sessionOptions.ServerPort = serverPort;

            return OpenSession();
        }

        private bool OpenSession()
        {
            if (_session == null)
            {
                //toolStripStatusLabel1.Text = "Connecting...";
                // create new session
                _session = new Session(_sessionOptions, new EventHandler(ProcessEvent));
            }
            return _session.Start();
        }

        public void SendRequest()
        {
            // create session
            if (!CreateSession())
            {
                //toolStripStatusLabel1.Text = "Failed to start session.";
                return;
            }
            // open market data service
            if (!_session.OpenService("//blp/mktdata"))
            {
                //toolStripStatusLabel1.Text = "Failed to open //blp/mktdata";
                return;
            }
            //toolStripStatusLabel1.Text = "Connected sucessfully";
            Service refDataService = _session.GetService("//blp/mktdata");
            List<string> options = new List<string>();
            _subscriptions = new List<Subscription>();


            // create subscription and add to list
            foreach (var security in _securities)
            {
                options.Clear();
                _subscriptions.Add(new Subscription(security.Key, _fields, options, new CorrelationID(security.Value)));
            }

            // subscribe to securities
            _session.Subscribe(_subscriptions);
            isSubscribed = true;
            //toolStripStatusLabel1.Text = "Subscribed to securities.";

        }

        /// <summary>
        /// Data Event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void ProcessEvent(Event eventObj, Session session)
        {
            //if (InvokeRequired)
            //{
            //    try
            //    {
            //        Invoke(new EventHandler(processEvent), new object[] { eventObj, session });
            //    }
            //    catch (Exception)
            //    {
            //        Console.WriteLine("Invoke failed");
            //    }
            //}
            //else
            //{
            //try
            //{
                switch (eventObj.Type)
                {
                    case Event.EventType.SUBSCRIPTION_DATA:
                        // process subscription data
                        ProcessRequestDataEvent(eventObj, session);
                        break;
                    case Event.EventType.SUBSCRIPTION_STATUS:
                        // process subscription status
                        ProcessRequestStatusEvent(eventObj, session);
                        break;
                    default:
                        ProcessMiscEvents(eventObj, session);
                        break;
                }
            //}
            //catch (System.Exception e)
            //{
            //    //toolStripStatusLabel1.Text = e.Message.ToString();
            //}
            //}
        }

        private static void ProcessRequestDataEvent(Event eventObj, Session session)
        {
            // process message
            foreach (Message msg in eventObj)
            {
                // get correlation id
                var securityObj = (BloombergSecurity)msg.CorrelationID.Object;

                // process market data
                if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("MarketDataEvents")))
                {
                    // check for initial paint
                    if (msg.HasElement("SES_START"))
                    {
                        if (msg.GetElementAsBool("IS_DELAYED_STREAM"))
                        {
                            securityObj.SetDelayedStream();
                        }
                    }

                    // process tick data
                    for (int fieldIndex = 1; fieldIndex < securityObj.SecurityFields.Count; fieldIndex++)
                    {
                        string field = securityObj.SecurityFields[fieldIndex].ToUpper();
                        if (msg.HasElement(field))
                        {
                            //msg.GetElementAsDatetime(Element.)
                            // check element to see if it has null value
                            if (!msg.GetElement(field).IsNull)
                            {
                                string value = msg.GetElementAsString(field);
                                securityObj.Setfield(securityObj.SecurityFields[fieldIndex], value);
                            }
                        }
                    }
                    // allow application to update UI
                    Application.DoEvents();
                }
            }
        }

        /// <summary>
        /// Request status event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private static void ProcessRequestStatusEvent(Event eventObj, Session session)
        {
            List<string> dataList = new List<string>();
            // process status message
            foreach (Message msg in eventObj)
            {

                var securityObj = (BloombergSecurity)msg.CorrelationID.Object;
                DataGridViewRow dataRow = securityObj.DataGridRow;

                if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("SubscriptionStarted")))
                {
                    // set subscribed color
                    foreach (DataGridViewCell cell in dataRow.Cells)
                    {
                        cell.Style.BackColor = Color.LightGreen;
                    }
                    //try
                    //{
                        // check for error
                        if (msg.HasElement("exceptions"))
                        {
                            // subscription has error
                            Element error = msg.GetElement("exceptions");
                            int searchIndex = 0;
                            for (int errorIndex = 0; errorIndex < error.NumValues; errorIndex++)
                            {
                                Element errorException = error.GetValueAsElement(errorIndex);
                                string field = errorException.GetElementAsString(FIELD_ID);
                                Element reason = errorException.GetElement(REASON);
                                string message = reason.GetElementAsString(DESCRIPTION);
                                //for (; searchIndex < dataGridViewData.ColumnCount - 1; searchIndex++)
                                //{
                                //    if (field == dataGridViewData.Columns[searchIndex].Name)
                                //    {
                                //        dataRow.Cells[searchIndex].Value = message;
                                //        break;
                                //    }
                                //}
                            }
                        }
                    //}
                    //catch (Exception e)
                    //{
                    //    //toolStripStatusLabel1.Text = e.Message;
                    //}
                }
                else
                {
                    // check for subscription failure
                    if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("SubscriptionFailure")))
                    {
                        if (msg.HasElement(REASON))
                        {
                            Element reason = msg.GetElement(REASON);
                            string message = reason.GetElementAsString(DESCRIPTION);
                            dataRow.Cells[1].Value = message;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process miscellaneous events
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private static void ProcessMiscEvents(Event eventObj, Session session)
        {
            foreach (Message msg in eventObj)
            {
                switch (msg.MessageType.ToString())
                {
                    case "SessionStarted":
                        // "Session Started"
                        break;
                    case "SessionTerminated":
                    case "SessionStopped":
                        // "Session Terminated"
                        break;
                    case "ServiceOpened":
                        // "Reference Service Opened"
                        break;
                    case "RequestFailure":
                        Element reason = msg.GetElement(REASON);
                        string message = string.Concat("Error: Source-", reason.GetElementAsString(SOURCE),
                            ", Code-", reason.GetElementAsString(ERROR_CODE), ", category-", reason.GetElementAsString(CATEGORY),
                            ", desc-", reason.GetElementAsString(DESCRIPTION));
                        //toolStripStatusLabel1.Text = message;
                        break;
                    default:
                        //toolStripStatusLabel1.Text = msg.MessageType.ToString();
                        break;
                }
            }
        }



    }

}
