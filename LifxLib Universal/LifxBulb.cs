using System;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using LifxLib.Messages;

namespace LifxLib
{
    public class LifxBulb
    {
        public string IPAddress { get; set; }
        public string MACAddress { get; set; }
        public LifxPanController PanController { get; set; }

        public LifxBulb(LifxPanController panController, string ipAddress, string macAddress)
        {
            PanController = panController;
            IPAddress = ipAddress;
            MACAddress = macAddress;
        }

        /// <summary>
        /// Get current power state
        /// </summary>
        /// <returns></returns>
        public async Task SendGetPowerStateCommand()
        {
            LifxGetPowerStateCommand command = new LifxGetPowerStateCommand();
            await LifxCommunicator.Instance.SendCommand(command, this);
        }

        /// <summary>
        /// Set current power state
        /// </summary>
        /// <param name="stateToSet"></param>
        /// <returns>Returns the set power state</returns>
        public async Task SendSetPowerStateCommand(LifxPowerState stateToSet)
        {
            LifxSetPowerStateCommand command = new LifxSetPowerStateCommand(stateToSet);
            await LifxCommunicator.Instance.SendCommand(command, this);
        }

        public async Task SendGetLabelCommand()
        {
            LifxGetLabelCommand command = new LifxGetLabelCommand();
            await LifxCommunicator.Instance.SendCommand(command, this);
        }


        public async Task SendSetLabelCommand(string newLabel)
        {
            LifxSetLabelCommand command = new LifxSetLabelCommand(newLabel);
            await LifxCommunicator.Instance.SendCommand(command, this);
        }

        public async Task SendGetLightStatusCommand()
        {

            LifxGetLightStateCommand command = new LifxGetLightStateCommand();
            await LifxCommunicator.Instance.SendCommand(command, this);

            //HSLColor hslColor = new HSLColor((double)(lsMessage.Hue * 240 / 65535), (double)(lsMessage.Saturation * 240 / 65535), (double)(lsMessage.Lumnosity * 240 / 65535));
        }


        public async Task SendSetColorCommand(LifxColor color, UInt32 fadeTime)
        {
            LifxSetLightStateCommand command = new LifxSetLightStateCommand(color.Hue, color.Saturation, color.Luminosity, color.Kelvin, fadeTime);
            await LifxCommunicator.Instance.SendCommand(command, this);
        }


        public async Task SendSetDimLevelCommand(UInt16 dimLevel, UInt32 fadeTime)
        {

            LifxSetDimAbsoluteCommand command = new LifxSetDimAbsoluteCommand(dimLevel, fadeTime);
            await LifxCommunicator.Instance.SendCommand(command, this);

        }
    }
}
