using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using LifxLib.Messages;

namespace LifxLib
{
    public class LifxCommunicator : IDisposable
    {
        private const Int32 LIFX_PORT = 56700;
        private const String BROADCAST_IP_ADDRESS = "255.255.255.255";

        private struct ConnectionState
        {
            public DatagramSocket UDPClient { get; set; }
            public HostName EndPoint { get; set; }
        }
        private struct IncomingMessage
        {
            public LifxDataPacket Data { get; set; }
            public HostName BulbAddress { get; set; }
        }

        private Queue<IncomingMessage> mIncomingQueue = new Queue<IncomingMessage>(10);

        private List<LifxPanController> mFoundPanHandlers = new List<LifxPanController>();

        private int mTimeoutMilliseconds = 1000;
        private DatagramSocket mSendCommandClient;
        private DatagramSocket mListnerClient;
        private bool mIsInitialized = false;
        private bool mIsDisposed = false;
        private static LifxCommunicator mInstance  = new LifxCommunicator();

        public static LifxCommunicator Instance
        {
            get
            {
                return mInstance;
            }
        }
        public int TimeoutMilliseconds
        {
            get { return mTimeoutMilliseconds; }
            set { mTimeoutMilliseconds = value; }
        }

        private LifxCommunicator()
        {

        }

        /// <summary>
        /// Initializes the listner for bulb messages
        /// </summary>
        public async void Initialize()
        {
            HostName endPoint = new HostName(BROADCAST_IP_ADDRESS);
            mListnerClient = new DatagramSocket();
            mListnerClient.MessageReceived += mListnerClient_MessageReceived;
            await mListnerClient.BindEndpointAsync(endPoint, LIFX_PORT.ToString());

            mIsInitialized = true;
        }

        void mListnerClient_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            if (mIsDisposed)
                return;

            Byte[] receiveBytes = { };
            args.GetDataReader().ReadBytes(receiveBytes);
            string receiveString = LifxHelper.ByteArrayToString(receiveBytes);

            LifxDataPacket packet = new LifxDataPacket(receiveBytes);
            mIncomingQueue.Enqueue(new IncomingMessage() { Data = packet, BulbAddress = args.RemoteAddress });            
        }

        /// <summary>
        /// Discovers the PanControllers (including their bulbs)
        /// </summary>
        /// <returns>List of bulbs</returns>
        public List<LifxPanController> Discover()
        {
           LifxGetPANGatewayCommand getPANCommand = new LifxGetPANGatewayCommand();

           mFoundPanHandlers.Clear();
           mTimeoutMilliseconds = 1500;
           int savedTimeout = mTimeoutMilliseconds;

           try
           {
               SendCommand(getPANCommand, LifxPanController.UninitializedPanController);

               foreach (LifxPanController controller in mFoundPanHandlers)
               {
                   LifxGetLightStatusCommand getBulbs = new LifxGetLightStatusCommand();
                   getBulbs.IsDiscoveryCommand = true;

                   SendCommand(getBulbs, controller);
               }

               return mFoundPanHandlers;
           }
           catch (Exception e)
           {
               //In case of any other exception, re-throw
               throw e;
           }
           finally 
           {
               mTimeoutMilliseconds = savedTimeout;
           }
        }

        public LifxReceivedMessage SendCommand(LifxCommand command, LifxBulb bulb)
        {
            return SendCommand(command, bulb.MacAddress, bulb.PanHandler, bulb.HostName);
        }

        public LifxReceivedMessage SendCommand(LifxCommand command, LifxPanController panController)
        {
            return SendCommand(command, "", panController.MacAddress, panController.HostName);
        }

        /// <summary>
        /// Sends command to a bulb
        /// </summary>
        /// <param name="command"></param>
        /// <param name="bulb">The bulb to send the command to.</param>
        /// <returns>Returns the response message. If the command does not trigger a response it will reurn null. </returns>
        public LifxReceivedMessage SendCommand(LifxCommand command, string macAddress, string panController, HostName endPoint)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The communicator needs to be initialized before sending a command.");
           
            LifxDataPacket packet = new LifxDataPacket(command);
            packet.TargetMac = LifxHelper.StringToByteArray(macAddress);
            packet.PanControllerMac = LifxHelper.StringToByteArray(panController);

            DataWriter writer = new DataWriter(mSendCommandClient.OutputStream);
            writer.WriteBytes(packet.PacketData);

            DateTime commandSentTime = DateTime.Now;

            if (command.ReturnMessage == null)
                return null;

            while ((DateTime.Now - commandSentTime).TotalMilliseconds < mTimeoutMilliseconds)
            {
                if (mIncomingQueue.Count != 0)
                {
                    IncomingMessage mess = mIncomingQueue.Dequeue();
                    LifxDataPacket receivedPacket = mess.Data;

                    if (receivedPacket.PacketType == LifxPANGatewayStateMessage.PACKET_TYPE) 
                    { 
                        //Panhandler identified
                        LifxPANGatewayStateMessage panGateway = new LifxPANGatewayStateMessage();
                        panGateway.ReceivedData = receivedPacket;

                        AddDiscoveredPanHandler(new LifxPanController(
                               LifxHelper.ByteArrayToString(receivedPacket.TargetMac),
                               mess.BulbAddress));

                    }
                    else if (receivedPacket.PacketType == LifxLightStatusMessage.PACKET_TYPE && command.IsDiscoveryCommand)
                    {
                        //Panhandler identified
                        LifxLightStatusMessage panGateway = new LifxLightStatusMessage();
                        panGateway.ReceivedData = receivedPacket;

                        AddDiscoveredBulb(
                            LifxHelper.ByteArrayToString(receivedPacket.TargetMac),   
                            LifxHelper.ByteArrayToString(receivedPacket.PanControllerMac));
                    }
                    else if (receivedPacket.PacketType == command.ReturnMessage.PacketType)
                    {
                       
                        command.ReturnMessage.ReceivedData = receivedPacket;
                        mIncomingQueue.Clear();
                        return command.ReturnMessage;
                    }
                }
            }

            if (command.IsDiscoveryCommand)
                return null;

            if (command.RetryCount > 0)
            {
                command.RetryCount -= 1;

                //Recurssion
                return SendCommand(command, macAddress, panController, endPoint);
            }
            else
                throw new TimeoutException("Did not get a reply from bulb in a timely fashion");

        }


        private void AddDiscoveredPanHandler(LifxPanController foundPanHandler)
        {
            foreach (LifxPanController handler in mFoundPanHandlers)
            {
                if (handler.MacAddress == foundPanHandler.MacAddress)
                    return;//already added
            }

            mFoundPanHandlers.Add(foundPanHandler);
        }

        private void AddDiscoveredBulb(string macAddress, string panController)
        {
            foreach (LifxPanController controller in mFoundPanHandlers)
            {
                if (controller.MacAddress == panController)
                {
                    foreach (LifxBulb bulb in controller.Bulbs)
                    {
                        if (bulb.MacAddress == macAddress)
                            return;
                    }

                    controller.Bulbs.Add(new LifxBulb(controller, macAddress));
                    return;
                }
            }

            throw new InvalidOperationException("Should not end up here basically.");
        }

        //private Task<DatagramSocket> GetConnectedClient(LifxCommand command, HostName endPoint)
        //{
        //    if (mSendCommandClient == null)
        //    {
        //        return CreateClient(command, endPoint);
        //    }
        //    else
        //    { 
        //        if (command.IsBroadcastCommand)
        //        {
        //            if (mSendCommandClient.Information.RemoteAddress.DisplayName == BROADCAST_IP_ADDRESS) //TODO: MAY NOT BE DISPLAY NAME
        //            {
        //                return new Task<DatagramSocket>(() => { return mSendCommandClient; });
        //            }
        //            else
        //            {
        //                mSendCommandClient.Dispose();
        //                return CreateClient(command, endPoint);
        //            }
        //        }
        //        else
        //        {
        //            if (mSendCommandClient.Information.RemoteAddress.DisplayName == BROADCAST_IP_ADDRESS)
        //            {
        //                mSendCommandClient.Dispose();
        //                return CreateClient(command, endPoint); 
                        
        //            }
        //            else
        //            {
        //                return new Task<DatagramSocket>(() => { return mSendCommandClient; });
        //            }
        //        }
        //    }
        //}
        //private async Task<DatagramSocket> CreateClient(LifxCommand command, HostName endPoint)
        //{
        //    if (command.IsBroadcastCommand)
        //    {
        //        mSendCommandClient = new DatagramSocket();

        //        await mSendCommandClient.ConnectAsync(new HostName(BROADCAST_IP_ADDRESS), LIFX_PORT.ToString());
        //        return mSendCommandClient;
        //    }
        //    else
        //    {
        //        mSendCommandClient = new DatagramSocket();

        //        await mSendCommandClient.ConnectAsync(endPoint, LIFX_PORT.ToString());
        //        return mSendCommandClient;
        //    }
        //}

        public bool IsInitialized
        {
            get { return mIsInitialized; }
            set { mIsInitialized = value; }
        }

        #region IDisposable Members

        public void CloseConnections()
        {
            mListnerClient.Dispose();
            mSendCommandClient.Dispose();
        }

        public void Dispose()
        {
            mIsDisposed = true;
            CloseConnections();
        }

        #endregion
    }
}
