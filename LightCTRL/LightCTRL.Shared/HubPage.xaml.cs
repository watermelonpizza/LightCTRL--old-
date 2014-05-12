using LightCTRL.Common;
using LifxLib;
using LifxLib.Messages;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Networking.Sockets;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace LightCTRL
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {        
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private bool firstLightStatusFlag = true;
        private LifxBulb bulb;

        public HubPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            Initialise();
        }

        private void Initialise()
        {
            LifxCommunicator.Instance.MessageRecieved += Instance_MessageRecieved;
            LifxCommunicator.Instance.PanControllerFound += Instance_PanControllerFound;
        }

        private void SetControlState(bool enabled = true)
        {
            //PowerToggleSwitch.IsEnabled = enabled;
            FadeTimeTextBox.IsEnabled = enabled;
            HueSlider.IsEnabled = enabled;
            SaturationSlider.IsEnabled = enabled;
            LuminositySlider.IsEnabled = enabled;
            KelvinSlider.IsEnabled = enabled;
        }

        private void BindValueChangedEventHandlers()
        {
            HueSlider.ValueChanged += HueSlider_ValueChanged;
            SaturationSlider.ValueChanged += SaturationSlider_ValueChanged;
            LuminositySlider.ValueChanged += LuminositySlider_ValueChanged;
            KelvinSlider.ValueChanged += KelvinSlider_ValueChanged;
        }

        private void UnBindValueChangedEventHandlers()
        {
            HueSlider.ValueChanged -= HueSlider_ValueChanged;
            SaturationSlider.ValueChanged -= SaturationSlider_ValueChanged;
            LuminositySlider.ValueChanged -= LuminositySlider_ValueChanged;
            KelvinSlider.ValueChanged -= KelvinSlider_ValueChanged;
        }

        private async void Instance_PanControllerFound(object sender, LifxPanController e)
        {
            bulb = e.Bulbs[0];
            bulb.SendGetPowerStateCommand();

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
#if WINDOWS_PHONE_APP
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
#endif
                PowerToggleSwitch.IsEnabled = true;
                ConnectButton.IsEnabled = false;
            });
        }

        private async void Instance_MessageRecieved(object sender, LifxMessage e)
        {
            if (e.PacketType == MessagePacketType.PowerState)
            {
                LifxPowerStateMessage message = (LifxPowerStateMessage)e;

                if (message.PowerState == LifxPowerState.On)
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        PowerToggleSwitch.IsOn = true;
                        SetControlState(true);
                    });
                    bulb.SendGetLightStatusCommand();
                }
                else if (message.PowerState == LifxPowerState.Off)
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        PowerToggleSwitch.IsOn = false;
                        SetControlState(false);
                    });
                }
            }
            else if (e.PacketType == MessagePacketType.LightStatus)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    LifxLightStatusMessage message = (LifxLightStatusMessage)e;
                    PacketInfoTextBox.Text = "Hue: " + message.Hue.ToString() + "\n" +
                                             "Saturation: " + message.Saturation.ToString() + "\n" +
                                             "Luminosity: " + message.Lumnosity.ToString() + "\n" +
                                             "Kelvin: " + message.Kelvin.ToString();
                    FadeTimeTextBox.Text = message.Dim.ToString();

                    if (firstLightStatusFlag)
                    {
                        UnBindValueChangedEventHandlers();
                        HueSlider.Value = message.Hue;
                        SaturationSlider.Value = message.Saturation;
                        LuminositySlider.Value = message.Lumnosity;
                        KelvinSlider.Value = KelvinSlider.Value;
                        BindValueChangedEventHandlers();
                        firstLightStatusFlag = false;
                    }
                });
            }
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Shows the details of a clicked group in the <see cref="SectionPage"/>.
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Details about the click event.</param>
        private void GroupSection_ItemClick(object sender, ItemClickEventArgs e)
        {
            
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Defaults about the click event.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void PowerToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
                bulb.SendSetPowerStateCommand(LifxPowerState.On);
            else
            {
                SetControlState(false);
                bulb.SendSetPowerStateCommand(LifxPowerState.Off);
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
#endif
            await LifxCommunicator.Instance.Discover();
        }

        private void HueSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SetColour();
        }

        private void SaturationSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SetColour();
        }

        private void LuminositySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SetColour();
        }

        private void KelvinSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UnBindValueChangedEventHandlers();
                HueSlider.Value = 0;
                SaturationSlider.Value = 0;
            BindValueChangedEventHandlers();
            SetColour();
        }

        private void SetColour()
        {
            bulb.SendSetColorCommand(new LifxColor()
            {
                Hue = (UInt16)HueSlider.Value,
                Saturation = (UInt16)SaturationSlider.Value,
                Luminosity = (UInt16)LuminositySlider.Value,
                Kelvin = (UInt16)KelvinSlider.Value
            },
            Convert.ToUInt16(FadeTimeTextBox.Text));

            bulb.SendGetLightStatusCommand();
        }
    }
}