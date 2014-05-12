using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using LifxLib.Messages;

namespace LifxLib
{
    public class LifxCommunicator : IDisposable
    {
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

        private List<LifxPanController> foundPanControllers = new List<LifxPanController>();
        private static LifxCommunicator mInstance = new LifxCommunicator();
        private int timeoutMilliseconds = 1000;
        private static DatagramSocket lifxCommunicatorClient = new DatagramSocket();
        //private static DataWriter writer;

        public List<LifxPanController> ConnectedPanControllers { get { return foundPanControllers; } }
        public bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

        public static LifxCommunicator Instance { get { return mInstance; } private set { mInstance = value; } }
        public event EventHandler<LifxMessage> MessageRecieved;
        public event EventHandler<LifxPanController> PanControllerFound;

        public int TimeoutMilliseconds
        {
            get { return timeoutMilliseconds; }
            set { timeoutMilliseconds = value; }
        }

        private LifxCommunicator()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the listner for bulb messages
        /// </summary>
        public async void Initialize()
        {
            lifxCommunicatorClient = new DatagramSocket();
            lifxCommunicatorClient.MessageReceived += lifxCommunicatorClient_MessageReceived;
            CoreApplication.Properties.Add("listener", lifxCommunicatorClient);
            //await lifxCommunicatorClient.BindServiceNameAsync(LifxHelper.LIFX_PORT.ToString());
            await lifxCommunicatorClient.BindEndpointAsync(null, LifxHelper.LIFX_PORT.ToString());

            IsInitialized = true;
        }

        private void lifxCommunicatorClient_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            if (IsDisposed)
                return;

            uint bufferArraySize = args.GetDataReader().UnconsumedBufferLength;
            Byte[] receiveBytes = new Byte[bufferArraySize];
            args.GetDataReader().ReadBytes(receiveBytes);

            string receiveString = LifxHelper.ByteArrayToString(receiveBytes);
            LifxDataPacket packet = new LifxDataPacket(receiveBytes);

            LifxMessage receivedMessage = LifxHelper.PacketToMessage(packet);

            if (receivedMessage != null)
            {
                if (receivedMessage.PacketType == MessagePacketType.PanGateway)
                    AddDiscoveredPanHandler(new LifxPanController()
                    {
                        MACAddress = LifxHelper.ByteArrayToString(receivedMessage.ReceivedData.PanControllerMac),
                        IPAddress = args.RemoteAddress.DisplayName
                    });
                else
                    MessageRecieved.Invoke(this, receivedMessage);
            }
        }

        /// <summary>
        /// Discovers the PanControllers (including their bulbs)
        /// </summary>
        /// <returns>List of bulbs</returns>
        public async Task Discover()
        {
            LifxGetPanGatewayCommand getPanGatewayCommand = new LifxGetPanGatewayCommand();
            foundPanControllers.Clear();
            timeoutMilliseconds = 1500;

            int savedTimeout = timeoutMilliseconds;

            try
            {
                await SendCommand(getPanGatewayCommand, LifxPanController.UninitializedPanController);
            }
            catch (Exception e)
            {
                //In case of any other exception, re-throw
                throw e;
            }
            finally
            {
                timeoutMilliseconds = savedTimeout;
            }
        }

        public async Task SendCommand(LifxCommand command, LifxBulb bulb)
        {
            await SendCommand(command, bulb.MACAddress, bulb.PanController.MACAddress, bulb.IPAddress);
        }

        public async Task SendCommand(LifxCommand command, LifxPanController panController)
        {
            await SendCommand(command, "", panController.MACAddress, panController.IPAddress);
        }

        /// <summary>
        /// Sends command to a bulb
        /// </summary>
        /// <param name="command"></param>
        /// <param name="bulb">The bulb to send the command to.</param>
        /// <returns>Returns the response message. If the command does not trigger a response it will reurn null. </returns>
        public async Task SendCommand(LifxCommand command, string bulbMacAddress, string panControllerMacAddress, string remoteIPAddress)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The communicator needs to be initialized before sending a command.");

            LifxDataPacket packet = new LifxDataPacket(command);
            packet.TargetMac = LifxHelper.StringToByteArray(bulbMacAddress);
            packet.PanControllerMac = LifxHelper.StringToByteArray(panControllerMacAddress);

            using (var stream = await lifxCommunicatorClient.GetOutputStreamAsync(new HostName(remoteIPAddress), LifxHelper.LIFX_PORT.ToString()))
            {
                using (var writer = new DataWriter(stream))
                {
                    writer.WriteBytes(packet.PacketData);
                    await writer.StoreAsync();
                }
            }
        }


        private void AddDiscoveredPanHandler(LifxPanController foundPanHandler)
        {
            foreach (LifxPanController handler in foundPanControllers)
            {
                if (handler.MACAddress == foundPanHandler.MACAddress)
                    return;//already added
            }

            foundPanHandler.Bulbs.Add(new LifxBulb(foundPanHandler, foundPanHandler.IPAddress, foundPanHandler.MACAddress));
            foundPanControllers.Add(foundPanHandler);
            PanControllerFound.Invoke(this, foundPanHandler);
        }

        //private void AddDiscoveredBulb(string macAddress, string panController)
        //{
        //    foreach (LifxPanController controller in foundPanControllers)
        //    {
        //        if (controller.MACAddress == panController)
        //        {
        //            foreach (LifxBulb bulb in controller.Bulbs)
        //            {
        //                if (bulb.MACAddress == macAddress)
        //                    return;
        //            }

        //            controller.Bulbs.Add(new LifxBulb(controller, macAddress));
        //            return;
        //        }
        //    }

        //    throw new InvalidOperationException("Should not end up here basically.");
        //}

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

        #region IDisposable Members

        public void CloseConnections()
        {
            lifxCommunicatorClient.Dispose();
        }

        public void Dispose()
        {
            IsDisposed = true;
            CloseConnections();
        }

        #endregion
    }
}