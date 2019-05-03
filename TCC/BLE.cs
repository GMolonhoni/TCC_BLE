using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage.Streams;

/* Para Acesso a essas bibliotecas, inserir as seguintes referências no projeto:
   C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.17134.0\Windows.winmd
   C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll
*/



namespace TCC
{
    class BLE
    {
        private static BLE instance;

        BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
        DeviceWatcher deviceWatcher = DeviceInformation.CreateWatcher();
        GattDeviceService deviceService;
        private static GattCharacteristic  read;

        //List<BLE_Devices> DevicesList = new List<BLE_Devices>();
        List<BluetoothLEDevice> DevicesList = new List<BluetoothLEDevice>();
        List<string> DevicesNames = new List<string>();

        BluetoothLEDevice device;

        public event EventHandler NewDevice;

        public static BLE getInstance()
        {
            if (instance == null) instance = new BLE();
            return instance;
        }

        BLE()
        {
            Setup();
        }

        /// <summary>
        /// Initialize watcher
        /// </summary>
        private void Setup()
        {
            watcher.Received += OnAdvertisementReceived;
            watcher.ScanningMode = BluetoothLEScanningMode.Active;            
        }
        
        /// <summary>
        /// Starts watcher
        /// </summary>
        public void Start()
        {
            watcher.Start();
        }

        /// <summary>
        /// Event when new device detected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private  void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            try
            {
                var address = args.BluetoothAddress;

                getDeviceAsync(address);

                if (device == null) return;

                if (!DevicesList.Any(c => c.Name.Equals(device.Name)))
                {
                    DevicesList.Add(device);
                    //DevicesNames.Add(device.Name);
                    NewDevice(this, null);
                    Console.WriteLine("Got new device: " + device.Name);
                }
                else watcher.Stop();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Returns the list of Devices
        /// </summary>
        /// <returns></returns>
        public List<BluetoothLEDevice> getDevicesList()
        {
            return DevicesList;
        }

        /// <summary>
        /// Get the device detected
        /// </summary>
        /// <param name="address"></param>
        private async void getDeviceAsync(ulong address)
        {
            device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
        }

        /// <summary>
        /// Connects to the Device in paramter
        /// </summary>
        /// <param name="deviceName"></param>
        public void Connect(string deviceName)
        {
            var index = DevicesList.FindIndex(c => c.Name.Equals(deviceName));
            var selectedDevice = DevicesList[index];
            Console.WriteLine("Trying to connect to: " + selectedDevice.Name);


            Console.WriteLine("Pairing to device now....");

            try
            {
                pairDeviceAsync(selectedDevice);
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR");
            }
        }

        

        private async void pairDeviceAsync(BluetoothLEDevice device)
        {
            var pairingResult = await device.GetGattServicesAsync();

            foreach(var service in pairingResult.Services)
            {
                Console.WriteLine(service.Uuid.ToString());
                if(service.Uuid.ToString().Equals("6e400001-b5a3-f393-e0a9-e50e24dcca9e")) //Compare the UUID with the one in ESP32
                {
                    deviceService = service;
                }
            }

            EnableNotifications(); //Enable to receive the notifications from ESP32


        }

        private async void EnableNotifications()
        {
            Console.WriteLine("Enabling");
            GattCharacteristicsResult dataCharacteristics = await deviceService.GetCharacteristicsAsync(); //get the characteristics

            if (dataCharacteristics.Status == GattCommunicationStatus.Success)
            {
                var characteristicss = dataCharacteristics.Characteristics;
                foreach (GattCharacteristic c in characteristicss)
                {
                    Console.WriteLine(c.Uuid);
                    if (c.Uuid.ToString().Equals("6e400003-b5a3-f393-e0a9-e50e24dcca9e")) //compare with the UUID on ESP32
                    {
                        read = c; 
                        Console.WriteLine(read.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify));
                        Console.WriteLine("Found read/write characteristic");

                        SubscribeAsync();

                        read.ValueChanged += receiveData; //Event to get data Received
                       
                    }
                }
            }
        }

        /// <summary>
        /// Subscribe to Receive Notification
        /// </summary>
        private async void SubscribeAsync()
        {
            if(read.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                Console.WriteLine("Attempting subscription");
                GattCommunicationStatus status = await read.WriteClientCharacteristicConfigurationDescriptorAsync(
                                    GattClientCharacteristicConfigurationDescriptorValue.Notify);

                if (status == GattCommunicationStatus.Success) Console.WriteLine("Recieving notifcations from device."); ;
            }
        }

        private void receiveData(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            //var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            uint bufferLength = (uint)reader.UnconsumedBufferLength;
            byte[] dataArray = new byte[16];
            //string receivedString = "";
            try
            {
                Console.WriteLine("Received");
                reader.ReadBytes(dataArray);
                foreach(var b in dataArray)
                {
                    Console.Write("{0:x}", b);
                }
                Console.WriteLine();
            }
            catch (Exception e)
            {

            }
        }
    }
}
