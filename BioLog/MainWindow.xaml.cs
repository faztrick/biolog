using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Net;

namespace BioLog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int MAX_DEVICE = 128;

        private int m_Handle = -1;

        private int m_NumOfDevice = 0;
        private uint[] m_DeviceID;
        private int[] m_DeviceType;
        private uint[] m_DeviceAddr;

        private uint m_DeviceID1;
        private uint m_DeviceAddr1;
        private int m_DeviceType1;

        private int m_NumOfConnectedDevice = 0;
        private int[] m_ConnectedDeviceHandle;
        private uint[] m_ConnectedDeviceID;
        private int[] m_ConnectedDeviceType;
        private uint[] m_ConnectedDeviceAddr;

        BSSDK.BESysInfoData m_SysInfoBEPlus;
        BSSDK.BEConfigData m_ConfigBEPlus;

        BSSDK.BESysInfoDataBLN m_SysInfoBLN;
        BSSDK.BEConfigDataBLN m_ConfigBLN;

        BSSDK.BSSysInfoConfig m_SysInfoBST;
        BSSDK.BSIPConfig m_ConfigBST;

        public MainWindow()
        {
            InitializeComponent();
            m_DeviceID = new uint[MAX_DEVICE];
            m_DeviceType = new int[MAX_DEVICE];
            m_DeviceAddr = new uint[MAX_DEVICE];



            m_ConnectedDeviceHandle = new int[MAX_DEVICE];
            m_ConnectedDeviceID = new uint[MAX_DEVICE];
            m_ConnectedDeviceType = new int[MAX_DEVICE];
            m_ConnectedDeviceAddr = new uint[MAX_DEVICE];
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            deviceList.Items.Clear();

            //Cursor.Current = Cursors.WaitCursor;

            int result;

            //try
            //{
                result = BSSDK.BS_SearchDeviceInLAN(m_Handle, ref m_NumOfDevice, m_DeviceID, m_DeviceType, m_DeviceAddr);
            //}
            //finally
            //{
            //   // Cursor.Current = Cursors.Default;
            //}

            if (result != BSSDK.BS_SUCCESS)
            {
                MessageBox.Show("Cannot find any device", "Error");
                return;
            }

            for (int i = 0; i < m_NumOfDevice; i++)
            {
                string device = "";

                if (m_DeviceType[i] == BSSDK.BS_DEVICE_BIOSTATION)
                {
                    device += "BioStation ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_DSTATION)
                {
                    device += "D-Station ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_XSTATION)
                {
                    device += "X-Station ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_BIOSTATION2)
                {
                    device += "BioStation T2 ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_FSTATION)
                {
                    device += "FaceStation ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_BIOENTRY_PLUS)
                {
                    device += "BioEntry Plus ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_BIOENTRY_W)
                {
                    device += "BioEntry W ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_BIOLITE)
                {
                    device += "BioLite Net ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_XPASS)
                {
                    device += "Xpass ";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_XPASS_SLIM)
                {
                    device += "Xpass Slim";
                }
                else if (m_DeviceType[i] == BSSDK.BS_DEVICE_XPASS_SLIM2)
                {
                    device += "Xpass S2";
                }
                else
                {
                    device += "Unknown ";
                }

                device += (m_DeviceAddr[i] & 0xff) + ".";
                device += ((m_DeviceAddr[i] >> 8) & 0xff) + ".";
                device += ((m_DeviceAddr[i] >> 16) & 0xff) + ".";
                device += ((m_DeviceAddr[i] >> 24) & 0xff);

                device += "(" + m_DeviceID[i] + ")";

                deviceList.Items.Add(device);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int result = BSSDK.BS_InitSDK();

                if (result != BSSDK.BS_SUCCESS)
                {
                    MessageBox.Show("Cannot initialize the SDK", "Error");
                    return;
                }

                result = BSSDK.BS_OpenInternalUDP(ref m_Handle);

                if (result != BSSDK.BS_SUCCESS)
                {
                    MessageBox.Show("Cannot open internal UDP socket", "Error");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetDevice(int handle, uint deviceID2, uint deviceAddr2, int deviceType2)
        {
            m_Handle = handle;
            m_DeviceID1 = deviceID2;
            m_DeviceAddr1 = deviceAddr2;
            m_DeviceType1 = deviceType2;

           
        }

      
        private bool ReadSysInfo()
        {
            int configSize = 0;

           // Cursor.Current = Cursors.WaitCursor;

            if (m_DeviceType1 == BSSDK.BS_DEVICE_BIOENTRY_PLUS ||
                m_DeviceType1 == BSSDK.BS_DEVICE_BIOENTRY_W ||
                m_DeviceType1 == BSSDK.BS_DEVICE_XPASS ||
                m_DeviceType1 == BSSDK.BS_DEVICE_XPASS_SLIM ||
                m_DeviceType1 == BSSDK.BS_DEVICE_XPASS_SLIM2)
            {
                IntPtr sysInfo = Marshal.AllocHGlobal(Marshal.SizeOf(m_SysInfoBEPlus));

                int result = BSSDK.BS_ReadConfigUDP(m_Handle, m_DeviceAddr1, m_DeviceID1, BSSDK.BEPLUS_CONFIG_SYS_INFO, ref configSize, sysInfo);

                //Cursor.Current = Cursors.Default;

                if (result != BSSDK.BS_SUCCESS)
                {
                    MessageBox.Show("Cannot read the sys info", "Error");

                    Marshal.FreeHGlobal(sysInfo);
                    return false;
                }

                m_SysInfoBEPlus = (BSSDK.BESysInfoData)Marshal.PtrToStructure(sysInfo, typeof(BSSDK.BESysInfoData));

                Marshal.FreeHGlobal(sysInfo);

                deviceID.Text = String.Format("{0}", m_SysInfoBEPlus.ID);

                MAC.Text = "";

                int i = 0;
                for (i = 0; i < 5; i++)
                {
                    MAC.Text += m_SysInfoBEPlus.macAddr[i].ToString("X2") + ":";
                }
                MAC.Text += m_SysInfoBEPlus.macAddr[i].ToString("X2");

                FWVersion.Text = Encoding.ASCII.GetString(m_SysInfoBEPlus.firmwareVer);
            }
            else if (m_DeviceType1 == BSSDK.BS_DEVICE_BIOLITE)
            {
                IntPtr sysInfo = Marshal.AllocHGlobal(Marshal.SizeOf(m_SysInfoBLN));

                configSize = Marshal.SizeOf(m_SysInfoBLN);

                int result = BSSDK.BS_ReadConfigUDP(m_Handle, m_DeviceAddr1, m_DeviceID1, BSSDK.BIOLITE_CONFIG_SYS_INFO, ref configSize, sysInfo);

              //  Cursor.Current = Cursors.Default;

                if (result != BSSDK.BS_SUCCESS)
                {
                    MessageBox.Show("Cannot read the sys info", "Error");

                    Marshal.FreeHGlobal(sysInfo);
                    return false;
                }

                m_SysInfoBLN = (BSSDK.BESysInfoDataBLN)Marshal.PtrToStructure(sysInfo, typeof(BSSDK.BESysInfoDataBLN));

                Marshal.FreeHGlobal(sysInfo);

                deviceID.Text = String.Format("{0}", m_SysInfoBLN.ID);

                MAC.Text = "";

                int i = 0;
                for (i = 0; i < 5; i++)
                {
                    MAC.Text += m_SysInfoBLN.macAddr[i].ToString("X2") + ":";
                }
                MAC.Text += m_SysInfoBLN.macAddr[i].ToString("X2");

                FWVersion.Text = Encoding.ASCII.GetString(m_SysInfoBLN.firmwareVer);
            }
            else if (m_DeviceType1 == BSSDK.BS_DEVICE_BIOSTATION ||
                     m_DeviceType1 == BSSDK.BS_DEVICE_DSTATION ||
                     m_DeviceType1 == BSSDK.BS_DEVICE_XSTATION ||
                     m_DeviceType1 == BSSDK.BS_DEVICE_BIOSTATION2 ||
                     m_DeviceType1 == BSSDK.BS_DEVICE_FSTATION)
            {
                IntPtr sysInfo = Marshal.AllocHGlobal(Marshal.SizeOf(m_SysInfoBST));

                int result = BSSDK.BS_ReadConfigUDP(m_Handle, m_DeviceAddr1, m_DeviceID1, BSSDK.BS_CONFIG_SYS_INFO, ref configSize, sysInfo);

             //   Cursor.Current = Cursors.Default;

                if (result != BSSDK.BS_SUCCESS)
                {
                    MessageBox.Show("Cannot read the sys info", "Error");

                    Marshal.FreeHGlobal(sysInfo);
                    return false;
                }

                m_SysInfoBST = (BSSDK.BSSysInfoConfig)Marshal.PtrToStructure(sysInfo, typeof(BSSDK.BSSysInfoConfig));

                Marshal.FreeHGlobal(sysInfo);

                deviceID.Text = String.Format("{0}", m_SysInfoBST.ID);

                MAC.Text = Encoding.ASCII.GetString(m_SysInfoBST.macAddr);
                FWVersion.Text = Encoding.ASCII.GetString(m_SysInfoBST.firmwareVer);
            }

            return true;
        }
        private bool ReadConfig()
        {
            int configSize = 0;

           // Cursor.Current = Cursors.WaitCursor;

            if (m_DeviceType1 == BSSDK.BS_DEVICE_BIOENTRY_PLUS ||
                m_DeviceType1 == BSSDK.BS_DEVICE_BIOENTRY_W ||
                m_DeviceType1 == BSSDK.BS_DEVICE_XPASS ||
                m_DeviceType1 == BSSDK.BS_DEVICE_XPASS_SLIM ||
                m_DeviceType1 == BSSDK.BS_DEVICE_XPASS_SLIM2)
            {
                IntPtr config = Marshal.AllocHGlobal(Marshal.SizeOf(m_ConfigBEPlus));

                int result = BSSDK.BS_ReadConfigUDP(m_Handle, m_DeviceAddr1, m_DeviceID1, BSSDK.BEPLUS_CONFIG, ref configSize, config);

              //  Cursor.Current = Cursors.Default;

                if (result != BSSDK.BS_SUCCESS)
                {
                    MessageBox.Show("Cannot read the configuration", "Error");

                    Marshal.FreeHGlobal(config);
                    return false;
                }

                m_ConfigBEPlus = (BSSDK.BEConfigData)Marshal.PtrToStructure(config, typeof(BSSDK.BEConfigData));

                Marshal.FreeHGlobal(config);

                DHCP.IsChecked = m_ConfigBEPlus.useDHCP;
                useServer.IsChecked = m_ConfigBEPlus.useServer;
                synchTime.IsChecked = m_ConfigBEPlus.synchTime;

                IPAddress addr = new IPAddress(m_ConfigBEPlus.ipAddr);
                if (m_ConfigBST.useDHCP != false)
                {
                    ipAddr.Text = (m_DeviceAddr1 & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 8) & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 16) & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 24) & 0xff);
                }
                else
                {
                    ipAddr.Text = addr.ToString();
                }

                addr.Address = m_ConfigBEPlus.gateway;
                gateway.Text = addr.ToString();

                addr.Address = m_ConfigBEPlus.subnetMask;
                subnetMask.Text = addr.ToString();

                addr.Address = m_ConfigBEPlus.serverIpAddr;
                serverIP.Text = addr.ToString();

                port.Text = m_ConfigBEPlus.port.ToString();

                if (m_ConfigBEPlus.useServer)
                {
                    serverIP.IsEnabled = true;
                }
                else
                {
                    serverIP.IsEnabled = false;
                }

                if (DHCP.IsChecked==true)
                {
                    ipAddr.IsEnabled = false;
                    gateway.IsEnabled = false;
                    subnetMask.IsEnabled = false;
                }
                else
                {
                    ipAddr.IsEnabled = true;
                    gateway.IsEnabled = true;
                    subnetMask.IsEnabled = true;
                }
            }
            else if (m_DeviceType1 == BSSDK.BS_DEVICE_BIOLITE)
            {
                IntPtr config = Marshal.AllocHGlobal(Marshal.SizeOf(m_ConfigBLN));

                configSize = Marshal.SizeOf(m_ConfigBLN);

                int result = BSSDK.BS_ReadConfigUDP(m_Handle, m_DeviceAddr1, m_DeviceID1, BSSDK.BIOLITE_CONFIG, ref configSize, config);

                //Cursor.Current = Cursors.Default;

                if (result != BSSDK.BS_SUCCESS)
                {
                    MessageBox.Show("Cannot read the configuration", "Error");

                    Marshal.FreeHGlobal(config);
                    return false;
                }

                m_ConfigBLN = (BSSDK.BEConfigDataBLN)Marshal.PtrToStructure(config, typeof(BSSDK.BEConfigDataBLN));

                Marshal.FreeHGlobal(config);

                DHCP.IsChecked = m_ConfigBLN.useDHCP;
                useServer.IsChecked = m_ConfigBLN.useServer;
                synchTime.IsChecked = m_ConfigBLN.synchTime;

                DHCP.IsChecked = m_ConfigBST.useDHCP;
                useServer.IsChecked = m_ConfigBST.useServer;
                synchTime.IsChecked = m_ConfigBST.syncTimeWithServer;

                IPAddress addr = new IPAddress(m_ConfigBLN.ipAddr);
                if (m_ConfigBST.useDHCP != false)
                {
                    ipAddr.Text = (m_DeviceAddr1 & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 8) & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 16) & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 24) & 0xff);
                }
                else
                {
                    ipAddr.Text = addr.ToString();
                }

                addr.Address = m_ConfigBLN.gateway;
                gateway.Text = addr.ToString();

                addr.Address = m_ConfigBLN.subnetMask;
                subnetMask.Text = addr.ToString();

                addr.Address = m_ConfigBLN.serverIpAddr;
                serverIP.Text = addr.ToString();

                port.Text = m_ConfigBLN.port.ToString();


                if (m_ConfigBLN.useServer)
                {
                    serverIP.IsEnabled = true;
                }
                else
                {
                    serverIP.IsEnabled = false;
                }

                if (DHCP.IsChecked==true)
                {
                    ipAddr.IsEnabled = false;
                    gateway.IsEnabled = false;
                    subnetMask.IsEnabled = false;
                }
                else
                {
                    ipAddr.IsEnabled = true;
                    gateway.IsEnabled = true;
                    subnetMask.IsEnabled = true;
                }
            }
            else if (m_DeviceType1 == BSSDK.BS_DEVICE_BIOSTATION ||
                     m_DeviceType1 == BSSDK.BS_DEVICE_DSTATION ||
                     m_DeviceType1 == BSSDK.BS_DEVICE_XSTATION ||
                     m_DeviceType1 == BSSDK.BS_DEVICE_BIOSTATION2 ||
                     m_DeviceType1 == BSSDK.BS_DEVICE_FSTATION)
            {
                IntPtr config = Marshal.AllocHGlobal(Marshal.SizeOf(m_ConfigBST));

                int result = BSSDK.BS_ReadConfigUDP(m_Handle, m_DeviceAddr1, m_DeviceID1, BSSDK.BS_CONFIG_TCPIP, ref configSize, config);

               // Cursor.Current = Cursors.Default;

                if (result != BSSDK.BS_SUCCESS)
                {
                    MessageBox.Show("Cannot read the configuration", "Error");

                    Marshal.FreeHGlobal(config);
                    return false;
                }

                m_ConfigBST = (BSSDK.BSIPConfig)Marshal.PtrToStructure(config, typeof(BSSDK.BSIPConfig));

                Marshal.FreeHGlobal(config);

                DHCP.IsChecked = m_ConfigBST.useDHCP;
                useServer.IsChecked = m_ConfigBST.useServer;
                synchTime.IsChecked = m_ConfigBST.syncTimeWithServer;

                if (m_ConfigBST.useDHCP != false)
                {
                    ipAddr.Text = (m_DeviceAddr1 & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 8) & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 16) & 0xff) + ".";
                    ipAddr.Text += ((m_DeviceAddr1 >> 24) & 0xff);
                }
                else
                    ipAddr.Text = Encoding.ASCII.GetString(m_ConfigBST.ipAddr);

                gateway.Text = Encoding.ASCII.GetString(m_ConfigBST.gateway);
                subnetMask.Text = Encoding.ASCII.GetString(m_ConfigBST.subnetMask);
                serverIP.Text = Encoding.ASCII.GetString(m_ConfigBST.serverIP);

                if (m_ConfigBST.useServer)
                {
                    serverIP.IsEnabled = true;
                    port.Text = m_ConfigBST.serverPort.ToString();
                }
                else
                {
                    serverIP.IsEnabled = false;
                    port.Text = m_ConfigBST.port.ToString();
                }

                if (DHCP.IsChecked==true)
                {
                    ipAddr.IsEnabled = false;
                    gateway.IsEnabled = false;
                    subnetMask.IsEnabled = false;
                }
                else
                {
                    ipAddr.IsEnabled = true;
                    gateway.IsEnabled = true;
                    subnetMask.IsEnabled = true;
                }
            }

            return true;
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (deviceList.SelectedIndex < 0)
            {
                MessageBox.Show("Select a device first", "Error");
                return;
            }
            
            SetDevice(m_Handle, m_DeviceID[deviceList.SelectedIndex], m_DeviceAddr[deviceList.SelectedIndex], m_DeviceType[deviceList.SelectedIndex]);
            
            deviceInfo.Text = (m_DeviceAddr1 & 0xff) + ".";
            deviceInfo.Text += ((m_DeviceAddr1 >> 8) & 0xff) + ".";
            deviceInfo.Text += ((m_DeviceAddr1 >> 16) & 0xff) + ".";
            deviceInfo.Text += ((m_DeviceAddr1 >> 24) & 0xff);
            deviceInfo.Text += " (" + m_DeviceID + ")";

            if (ReadSysInfo())
            {
                ReadConfig();
            }
        }
    }
}
