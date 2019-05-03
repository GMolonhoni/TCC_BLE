using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCC
{
    public partial class PrincipalForm : Form
    {
        BLE ble = BLE.getInstance();
        bool newDevice;
        string selectedDevice;

        public PrincipalForm()
        {
            InitializeComponent();
        }

        private void PrincipalForm_Load(object sender, EventArgs e)
        {
            ble.NewDevice += NewDeviceHandler;
            ble.Start(); //Starts looking for BLE Devices
        }

        private void NewDeviceHandler(object sender, EventArgs e)
        {
            newDevice = true;
        }

        private void updateDevicesListComboBox()
        {
            var DevicesList = ble.getDevicesList();
            foreach (var device in DevicesList)
            {
                devicesListComboBox.Items.Add(device.Name);
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if(newDevice)
            {
                updateDevicesListComboBox();
                newDevice = false;
            }
        }

        private void devicesListComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedDevice = devicesListComboBox.SelectedItem.ToString();
            Console.WriteLine("Selected: " + selectedDevice);
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            ble.Connect(selectedDevice); //Connects to the Selected BLE device
        }
    }
}
