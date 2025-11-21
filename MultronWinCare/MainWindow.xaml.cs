using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Management;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
 
namespace MultronWinCare
{
    
    public partial class MainWindow : Window
    {
        List<string> windefendlogs = new List<string>();
        List<string> winfirewalllogs = new List<string>();
        List<string> winupdatelogs = new List<string>();
        List<string> privacylogs = new List<string>();
        string appearance = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "appearance.txt");
        int processing1 = 0;
        int processing2 = 0;
        int processing3 = 0;
        int processing4 = 0;
        CancellationTokenSource cts1 = new CancellationTokenSource();
        CancellationToken token1;
        CancellationTokenSource cts2 = new CancellationTokenSource();
        CancellationToken token2;
        CancellationTokenSource cts3 = new CancellationTokenSource();
        CancellationToken token3;
        CancellationTokenSource cts4 = new CancellationTokenSource();
        CancellationToken token4;
        public MainWindow()
        {
           
            InitializeComponent();

            
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            token1 = cts1.Token;
            token2 = cts2.Token;
            token3 = cts3.Token;
            token4 = cts4.Token;
            if (File.Exists(appearance))
            {
                string status = File.ReadAllText(appearance);
                if(status.Equals("0"))
                {
                    light();
                } else
                {
                    dark();
                }
            }
            bool isSystem = WindowsIdentity.GetCurrent().User == new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            string psexec = System.IO.Path.Combine(Environment.CurrentDirectory, "PsExec64.exe");

            if (!isSystem && File.Exists(psexec))
            {
                int sessionId = Process.GetCurrentProcess().SessionId;
                string args = $"-accepteula -s -i {sessionId} -n 3 \"{exePath}\"";
                var psi = new ProcessStartInfo
                {
                    FileName = psexec,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,        
                                                    
                };

                try
                {
                    Process.Start(psi);
                    Environment.Exit(0); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (!File.Exists(psexec) && !isSystem)
            {
                 MessageBox.Show("Multron WinCare is not running under the NT AUTHORITY\\SYSTEM account and therefore cannot perform certain system-level actions. Since PseExec64.exe is not available, the program will be launched with Administrator privileges as a fallback.", "Multron WinCare", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            WinDefendMonitor windefmon = new WinDefendMonitor(this);
             
            Task.Run(() => windefmon.run());

            WinFirewallMonitor winfirewallmon = new WinFirewallMonitor(this);

            Task.Run(() => winfirewallmon.run());
            WinUpdateMonitor winupdatemon = new WinUpdateMonitor(this);

            Task.Run(() => winupdatemon.run());
            await GetInfoOfWindowsUpdateDir();
            PrivacyTelemetryAreaMonitor privacymon = new PrivacyTelemetryAreaMonitor(this);

            Task.Run(() => privacymon.run());

        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                MaximizeButton_Click(sender, e);
            else
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private Rect _normalWindowBounds;
        private bool _isManualMaximized = false;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isManualMaximized || this.WindowState == WindowState.Maximized)
            {
              
                this.WindowState = WindowState.Normal;

                if (_normalWindowBounds != Rect.Empty)
                {
                    this.Left = _normalWindowBounds.Left;
                    this.Top = _normalWindowBounds.Top;
                    this.Width = _normalWindowBounds.Width;
                    this.Height = _normalWindowBounds.Height;
                }

                _isManualMaximized = false;
            }
            else
            { 
                _normalWindowBounds = new Rect(this.Left, this.Top, this.Width, this.Height);

                var workingArea = SystemParameters.WorkArea;
                this.WindowState = WindowState.Normal;
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;

                _isManualMaximized = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
          
           
        }
        public void light()
        {
            Resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Resources["TitleBarBackground"] = new SolidColorBrush(Color.FromRgb(243, 243, 243));
            Resources["CardBackground"] = new SolidColorBrush(Color.FromRgb(249, 249, 249));
            Resources["SideMenuBackground"] = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            Resources["PrimaryText"] = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            Resources["SecondaryText"] = new SolidColorBrush(Color.FromRgb(96, 96, 96));
            Resources["HoverBackground"] = new SolidColorBrush(Color.FromRgb(229, 229, 229));

            Resources["MenuItemSelected"] = new SolidColorBrush(Color.FromRgb(225, 243, 255));
            Resources["MenuBorder"] = new SolidColorBrush(Color.FromRgb(225, 225, 225));
            Resources["ToggleTrackBackground"] = new SolidColorBrush(Color.FromRgb(230, 230, 230));
            Resources["ToggleThumbBackground"] = new SolidColorBrush(Colors.White);
            Resources["ToggleActiveBackground"] = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            System.IO.File.Create(appearance).Close();
            System.IO.File.WriteAllText(appearance, "0");
        }
        public void dark()
        {
            Resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            Resources["TitleBarBackground"] = new SolidColorBrush(Color.FromRgb(37, 37, 38));
            Resources["CardBackground"] = new SolidColorBrush(Color.FromRgb(45, 45, 48));
            Resources["SideMenuBackground"] = new SolidColorBrush(Color.FromRgb(37, 37, 38));
            Resources["PrimaryText"] = new SolidColorBrush(Colors.White);
            Resources["SecondaryText"] = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            Resources["HoverBackground"] = new SolidColorBrush(Color.FromRgb(58, 58, 60));

            Resources["MenuItemSelected"] = new SolidColorBrush(Color.FromRgb(9, 71, 113));
            Resources["MenuBorder"] = new SolidColorBrush(Color.FromRgb(58, 58, 60));

            Resources["ToggleTrackBackground"] = new SolidColorBrush(Color.FromRgb(58, 58, 60));
            Resources["ToggleThumbBackground"] = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            Resources["ToggleActiveBackground"] = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            System.IO.File.Create(appearance).Close();
            System.IO.File.WriteAllText(appearance, "1");
        }
        private void LightThemeButton_Click(object sender, RoutedEventArgs e)
        {
            light();
        }

        private void DarkThemeButton_Click(object sender, RoutedEventArgs e)
        {
            dark();
        }
        public void manageservice(string servicename, string type, bool work)
        {
            using var def = new ManagementObject($"Win32_Service.Name='{servicename}'");
            var parameters = new object[] { type };
            var result = def.InvokeMethod("ChangeStartMode", parameters);
            using var service = new ServiceController(servicename);
            if (work)
            {

                service.Start();


            }
            else
            {
                if (service.CanStop)
                {
                    service.Stop();

                }
            }

        }
        class UserHelper
        {
            [DllImport("Wtsapi32.dll")]
            static extern bool WTSQuerySessionInformation(
                IntPtr hServer,
                int sessionId,
                WTS_INFO_CLASS wtsInfoClass,
                out IntPtr ppBuffer,
                out int pBytesReturned);

            [DllImport("Wtsapi32.dll")]
            static extern void WTSFreeMemory(IntPtr pointer);

            [DllImport("kernel32.dll")]
            static extern int WTSGetActiveConsoleSessionId();

            enum WTS_INFO_CLASS
            {
                WTSUserName = 5,
                WTSDomainName = 7
            }

            public static string GetActiveUserSID()
            {
                int sessionId = WTSGetActiveConsoleSessionId();

                IntPtr buffer;
                int bytesReturned;
                string userName = null;
                string domainName = null;

                if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WTS_INFO_CLASS.WTSUserName, out buffer, out bytesReturned) && bytesReturned > 1)
                {
                    userName = Marshal.PtrToStringAnsi(buffer);
                    WTSFreeMemory(buffer);
                }

                if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WTS_INFO_CLASS.WTSDomainName, out buffer, out bytesReturned) && bytesReturned > 1)
                {
                    domainName = Marshal.PtrToStringAnsi(buffer);
                    WTSFreeMemory(buffer);
                }

                if (string.IsNullOrEmpty(userName))
                    return null;
                 
                string userSid = null;
                using (var profileList = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList"))
                {
                    foreach (var sid in profileList.GetSubKeyNames())
                    {
                        using (var key = profileList.OpenSubKey(sid))
                        {
                            string profilePath = key.GetValue("ProfileImagePath") as string;
                            if (!string.IsNullOrEmpty(profilePath) && profilePath.EndsWith(userName, StringComparison.OrdinalIgnoreCase))
                            {
                                userSid = sid;
                                break;
                            }
                        }
                    }
                }

                return userSid;
            }
        }
        public class PrivacyTelemetryAreaMonitor
        {
            MainWindow main;
            public PrivacyTelemetryAreaMonitor(MainWindow main)
            {
                this.main = main;
            }
          
            public async Task run()
            {
                try
                {
                    
                    RegistryKey localmachinesystemNCS = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator");
                    RegistryKey localmachinesystemInternet = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\NlaSvc\Parameters\Internet");
                    RegistryKey localmachinewindowsRA = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Remote Assistance");
                    RegistryKey localmachineterminalServer = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server");
                    RegistryKey localmachineterminalServices = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services");
                    RegistryKey localmachinesoftwareprotection = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\CurrentVersion\Software Protection Platform");
                    RegistryKey localmachineFeedback = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Feedback");
                    RegistryKey WindowsDefender = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender");
                    RegistryKey spynet = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet");
                    RegistryKey Driver = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching");
                    RegistryKey cloudContent = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent");
                    RegistryKey passwordReveal = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CredUI");
                    RegistryKey localmachineapprivacy = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy");
                    RegistryKey localmachineaccountinfo = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation");
                    RegistryKey locationandsensors = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors");

                    RegistryKey ConsentStorelocation = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location");

                    RegistryKey Autororation = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AutoRotation");

                    RegistryKey SensorService = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SensorService");

              

                    RegistryKey localmachinehandwritingsharing = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\TabletPC");
                    
                    RegistryKey localmachineAppCompat = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppCompat");

                    RegistryKey localmachinehandwritingerrorreports = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports");

                    RegistryKey localmachineDisableAdvertisingID = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo");

                    RegistryKey localmachinedisablecameraonlogon = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Personalization");

                    RegistryKey localmachinesystemdisablecameraonlogon = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");

                    RegistryKey localmachinedisablemessagebackup = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Messaging");

                    RegistryKey localmachinedisablemessagebackuptomicrosoftaccount = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\SettingSync");

                    RegistryKey localmachinewindowserrorreportingservice = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WerSvc");

                   

                
                

                   

                

                    RegistryKey localmachineceip = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\SQMClient\Windows");

                    RegistryKey localmachineApplicationExperienceProgram = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\AppV\CEIP");

                    RegistryKey localmachineApplicationCustomerExperienceProgram = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\SQMClient\Windows");

                    RegistryKey localmachineCEIPforinternetexplorer = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Internet Explorer\SQM");

                    RegistryKey localmachineMicrosoftCEIPscheduled = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection");

                    RegistryKey localmachineEdge = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Edge");
                    RegistryKey localmachineCDPUserService = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CDPUserSvc");


                    RegistryKey localmachineAppAccessUserAccountInfo = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation");

                    RegistryKey localmachineAppAccessLocation = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location");

                    RegistryKey localmachinePasswordReveal = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CredUI");

                    RegistryKey localmachineStepsRecorder = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppCompat");

                    RegistryKey localmachineDataCollection = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection");

                    RegistryKey localmachineDeliveryOptimization = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization");

                    RegistryKey localmachineSpeech = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Speech");

                    RegistryKey localmachineDefender = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender");

                    RegistryKey localMachineDefenderSpynet = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet");

                    RegistryKey localmachineDefenderReporting = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender");

                    RegistryKey localmachineMobileDevices = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Portable Devices");

                    RegistryKey localmachineWMDC = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows CE Services");

                    RegistryKey localmachineExplorer = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer");

                    RegistryKey localmachineOneDrive = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\OneDrive");

                    RegistryKey localmachineWindowsUpdateUX = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\UX");

                    RegistryKey localmachineWindowsUpdate = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate");

                    RegistryKey localmachineDataCollectionPersonalization = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection\Personalization");

                    RegistryKey localmachineDataCollectionDiagnosticLogs = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection\DiagnosticLogs");

                    RegistryKey localmachineSystem = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System");

                    RegistryKey localmachinePhoneLink = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\PhoneLink");

                    RegistryKey localmachinewindowsMaps = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Maps");

                    RegistryKey localmachineFeeds = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds");

                    RegistryKey localmachineCopilot = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot");

                    RegistryKey localmachineCopilotRecall = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsAI");

                    RegistryKey localmachinePaint = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint");

                    RegistryKey bthLEEnumParams = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\BthLEEnum\Parameters");
                    RegistryKey bthA2dpParams = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\BthA2dp\Parameters");
                    RegistryKey bthHFEnumParams = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\BthHFEnum\Parameters");
                    RegistryKey bthEnumParams = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\BthEnum\Parameters");
                    RegistryKey bthAvrcpTgParams = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\BthAvrcpTg\Parameters");
                    RegistryKey bthModemParams = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\BTHMODEM\Parameters");
                
                    RegistryKey deviceMetadata = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata");

                    RegistryKey bluetoothToastNotifications = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.BluetoothDevice");
                    RegistryKey currentuseraccountinfo = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation");

                    RegistryKey currentuserExplorerAdvanced = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");

                    RegistryKey currentuserADC = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo");

                    RegistryKey currentuserContentDelivery = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager");

                    RegistryKey currentuserFeeds = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\Feeds");

                    RegistryKey currentuserExplorer = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");

                    RegistryKey currentuserMobility = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\Mobility");

                    RegistryKey currentuserAppAccessUserAccountInfo = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation");

                    RegistryKey currentuserAppAccessDiagnostics = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics");

                    RegistryKey currentuserAppAccessLocation = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location");

                    RegistryKey currentUserAppAccessUserAccountInfo = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation");

                    RegistryKey currentuserClipboard = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Clipboard");

                    RegistryKey currentuserDisableOfficeCeip = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Policies\Microsoft\Office\Common\QMEnable");
                

                 

                    RegistryKey currentuserwindowserrorreporting = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\Windows Error Reporting");

                    RegistryKey currentuserdisablesmsmmscloudsync = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Messaging");

                    RegistryKey currentuserDisableAdvertisingID = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo");

                    RegistryKey disablegeneralsyncsettingsformessages = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Messaging");

                    RegistryKey currentuserDisableMicrosoftCeip = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\SQMClient");

                 
                    while(true)
                    {
                        if (main.processing4 != 1)
                        {
                            try { await Task.Delay(1250, main.token4); } catch (TaskCanceledException) { continue; }
                            main.privacylogs.Clear();
                            int handwritingsharing = 0;
                            int inventorycollector = 0;
                            int handwritingerrorreports = 0;
                            int AdvertisingID = 0;
                            int disablecameraonlogon = 0;
                            int messagebackup = 0;
                            int errorreporting = 0;

                            int bluetoothadvertisements = 0;
                            int ceip = 0;
                            int edgetracing = 0;
                            int edgepayment = 0;
                            int edgeasn = 0;
                            int edgeacowa = 0;
                            int edgeuserfeedback = 0;

                            int edgeautofill = 0;
                            int edgeautofills = 0;
                            int edgelpe = 0;
                            int edgesaws = 0;
                            int edgeshopassistant = 0;
                            int edgehubssidebar = 0;
                            int edgespellcheck = 0;
                            int edgewsfne = 0;
                            int edgenpo = 0;
                            int edgepm = 0;
                            int edgesb = 0;
                            int edgeie = 0;
                            int edgesmartscreen = 0;
                            int edgetc = 0;
                            int edgesimilarsitesuggestions = 0;

                            int locationservices = 0;

                            int locationsensors = 0;

                            int scriptinglocation = 0;

                            int recodinguseractivity = 0;

                            int storinguseractivity = 0;


                            int uploadactivity = 0;

                            int crossdeviceclipboard = 0;

                            int clipboardhistory = 0;

                            int accountinfo = 0;

                            int diagnosticaccess = 0;

                            int letappsaccesslocation = 0;
                            int passreveal = 0;
                            int usersteps = 0;
                            int allowtelemetry = 0;
                            int dte = 0;
                            int limitlog = 0;
                            int dosd = 0;
                            int dod = 0;
                            int speechmodelupdate = 0;
                            int featureupdates = 0;

                            int driverm = 0;
                            int spynetmembership = 0;
                            int autosamplesubmitting = 0;
                            int malinfosubmit = 0;
                            int mmx = 0;
                            int meetnow = 0;
                            int news = 0;
                            int feedbackrem = 0;
                            int kms = 0;
                            int mapsupdate = 0;
                            int mapsoffline = 0;
                            int rmtaccess1 = 0;
                            int rmtaccess2 = 0;
                            int ncs = 0;
                            int copilot = 0;
                            int recall = 0;
                            int imc = 0;
                            int imcreator = 0;
                            int imfill = 0;
                            int ads1 = 0;
                            int ads2 = 0;
                            int sr = 0;
                            if (checkifexists(bthLEEnumParams))
                            {
                                bool isdefault = await checkvalueisdefault(0, bthLEEnumParams, "DisableAdvertising", 0);
                                if (isdefault == true)
                                {
                                    bluetoothadvertisements++;
                                }
                            }

                            if (checkifexists(bthA2dpParams))
                            {
                                bool isdefault = await checkvalueisdefault(0, bthA2dpParams, "DisableAdvertising", 0);
                                if (isdefault == true)
                                {
                                    bluetoothadvertisements++;
                                }
                            }

                            if (checkifexists(bthHFEnumParams))
                            {
                                bool isdefault = await checkvalueisdefault(0, bthHFEnumParams, "DisableAdvertising", 0);
                                if (isdefault == true)
                                {
                                    bluetoothadvertisements++;
                                }
                            }

                            if (checkifexists(bthEnumParams))
                            {
                                bool isdefault = await checkvalueisdefault(0, bthEnumParams, "DisableAdvertising", 0);
                                if (isdefault == true)
                                {
                                    bluetoothadvertisements++;
                                }
                            }

                            if (checkifexists(bthAvrcpTgParams))
                            {
                                bool isdefault = await checkvalueisdefault(0, bthAvrcpTgParams, "DisableAdvertising", 0);
                                if (isdefault == true)
                                {
                                    bluetoothadvertisements++;
                                }
                            }

                            if (checkifexists(bthModemParams))
                            {
                                bool isdefault = await checkvalueisdefault(0, bthModemParams, "DisableAdvertising", 0);
                                if (isdefault == true)
                                {
                                    bluetoothadvertisements++;

                                }
                            }

                            if (checkifexists(bluetoothToastNotifications))
                            {
                                bool isdefault = await checkvalueisdefault(1, bluetoothToastNotifications, "Enabled", 1);
                                if (isdefault == true)
                                {
                                    bluetoothadvertisements++;
                                }
                            }

                            if (checkifexists(deviceMetadata))
                            {
                                bool isdefault = await checkvalueisdefault(0, deviceMetadata, "PreventDeviceMetadataFromNetwork", 0);
                                if (isdefault == true)
                                {
                                    bluetoothadvertisements++;
                                }
                            }
                            if (checkifexists(localmachineDeliveryOptimization))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineDeliveryOptimization, "DODownloadMode", 1);
                                if (isdefault == true)
                                {
                                    dod++;
                                }
                            }
                            if (checkifexists(localmachineExplorer))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineExplorer, "ShowRecommendations", 0);
                                if (isdefault == true)
                                {
                                    sr++;
                                }
                            }
                            if (checkifexists(currentuserExplorer))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserExplorerAdvanced, "ShowRecommendations", 0);
                                if (isdefault == true)
                                {
                                    sr++;
                                }
                            }
                            if (checkifexists(currentuserExplorerAdvanced))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserExplorerAdvanced, "Start_ShowRecommendations", 1);
                                if (isdefault == true)
                                {
                                    ads1++;
                                }
                            }
                            if (checkifexists(currentuserExplorerAdvanced))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserExplorerAdvanced, "ShowSyncProviderNotifications", 1);
                                if (isdefault == true)
                                {
                                    ads2++;
                                }
                            }
                            if (checkifexists(localmachinePaint))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinePaint, "DisableGenerativeFill", 0);
                                if (isdefault == true)
                                {
                                    imfill++;
                                }
                            }
                            if (checkifexists(localmachinePaint))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinePaint, "DisableImageCreator", 0);
                                if (isdefault == true)
                                {
                                    imcreator++;
                                }
                            }
                            if (checkifexists(localmachinePaint))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinePaint, "DisableCocreator", 0);
                                if (isdefault == true)
                                {
                                    imc++;
                                }
                            }

                            if (checkifexists(localmachineCopilotRecall))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineCopilotRecall, "AllowRecallEnablement", 0);
                                if (isdefault == true)
                                {
                                    recall++;
                                }
                            }
                            if (checkifexists(localmachineCopilot))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineCopilot, "TurnOffWindowsCopilot", 0);
                                if (isdefault == true)
                                {
                                    copilot++;
                                }
                            }
                            if (checkifexists(localmachinesystemNCS))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinesystemNCS, "DisablePassivePolling", 0);
                                if (isdefault == true)
                                {
                                    ncs++;
                                }
                            }
                            if (checkifexists(localmachinesystemInternet))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachinesystemInternet, "EnableActiveProbing", 1);
                                if (isdefault == true)
                                {
                                    ncs++;
                                }
                            }
                            if (checkifexists(localmachinewindowsRA))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachinewindowsRA, "fAllowToGetHelp", 0);
                                if (isdefault == true)
                                {
                                    rmtaccess2++;
                                }
                            }
                            if (checkifexists(localmachineterminalServer))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineterminalServices, "fDenyTSConnections", 1);
                                if (isdefault == true)
                                {
                                    rmtaccess2++;
                                }
                            }
                            if (checkifexists(localmachineterminalServices))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineterminalServices, "fAllowToGetHelp", 0);
                                if (isdefault == true)
                                {
                                    rmtaccess1++;
                                }
                            }
                            if (checkifexists(localmachinewindowsMaps))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachinewindowsMaps, "AllowUntriggeredNetworkTrafficOnSettingsPage", 0);
                                if (isdefault == true)
                                {
                                    mapsoffline++;
                                }
                            }
                            if (checkifexists(localmachinewindowsMaps))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachinewindowsMaps, "AutoDownloadAndUpdateMapData", 0);
                                if (isdefault == true)
                                {
                                    mapsupdate++;
                                }
                            }
                            if (checkifexists(localmachinesoftwareprotection))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinesoftwareprotection, "NoAcquireGT", 0);
                                if (isdefault == true)
                                {
                                    kms++;
                                }
                            }
                            if (checkifexists(localmachinesoftwareprotection))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinesoftwareprotection, "NoGenTicket", 0);
                                if (isdefault == true)
                                {
                                    kms++;
                                }
                            }
                            if (checkifexists(localmachineFeedback))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineFeedback, "DoNotShowFeedbackNotifications", 0);
                                if (isdefault == true)
                                {
                                    feedbackrem++;
                                }
                            }
                            if (checkifexists(localmachineDataCollection))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineDataCollection, "DoNotShowFeedbackNotifications", 0);
                                if (isdefault == true)
                                {
                                    feedbackrem++;
                                }
                            }

                            if (checkifexists(currentuserExplorerAdvanced))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserExplorerAdvanced, "TaskbarDa", 1);
                                if (isdefault == true)
                                {
                                    news++;
                                }
                            }
                            if (checkifexists(currentuserExplorerAdvanced))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserExplorerAdvanced, "TaskbarDa", 1);
                                if (isdefault == true)
                                {
                                    news++;
                                }
                            }

                            if (checkifexists(localmachineExplorer))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineExplorer, "HideSCAMeetNow", 0);
                                if (isdefault == true)
                                {
                                    meetnow++;
                                }
                            }
                            if (checkifexists(localmachineCDPUserService))
                            {
                                bool isdefault = await checkvalueisdefault(2, localmachineCDPUserService, "Start", 1);
                                if (isdefault == true)
                                {
                                    mmx++;
                                }
                            }
                            if (checkifexists(localmachineSystem))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineSystem, "EnableMmx", 0);
                                if (isdefault == true)
                                {
                                    mmx++;
                                }
                            }
                            if (checkifexists(WindowsDefender))
                            {
                                bool isdefault = await checkvalueisdefault(2, WindowsDefender, "SubmitSamplesConsent", 0);
                                if (isdefault == true)
                                {
                                    malinfosubmit++;
                                }
                            }
                            if (checkifexists(spynet))
                            {
                                bool isdefault = await checkvalueisdefault(2, spynet, "SubmitSamplesConsent", 0);
                                if (isdefault == true)
                                {
                                    autosamplesubmitting++;
                                }
                            }
                            if (checkifexists(spynet))
                            {
                                bool isdefault = await checkvalueisdefault(1, spynet, "SpyNetReporting", 0);
                                if (isdefault == true)
                                {
                                    spynetmembership++;
                                }
                            }
                            if (checkifexists(Driver))
                            {
                                bool isdefault = await checkvalueisdefault(1, Driver, "DriverUpdateWizardWuSearchEnabled", 0);
                                if (isdefault == true)
                                {
                                    driverm++;
                                }
                            }
                            if (checkifexists(Driver))
                            {
                                bool isdefault = await checkvalueisdefault(1, Driver, "SearchOrderConfig", 1);
                                if (isdefault == true)
                                {
                                    driverm++;
                                }
                            }
                            if (checkifexists(localmachineWindowsUpdate))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineWindowsUpdate, "DeferQualityUpdates", 1);
                                if (isdefault == true)
                                {
                                    featureupdates++;
                                }
                            }
                            if (checkifexists(localmachineWindowsUpdate))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineWindowsUpdate, "DeferFeatureUpdates", 1);
                                if (isdefault == true)
                                {
                                    featureupdates++;
                                }
                            }
                            if (checkifexists(localmachineSpeech))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineSpeech, "AllowSpeechModelUpdate", 0);
                                if (isdefault == true)
                                {
                                    speechmodelupdate++;
                                }
                            }
                            if (checkifexists(localmachineDataCollection))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineDataCollection, "DisableOneSettingsDownloads", 0);
                                if (isdefault == true)
                                {
                                    dosd++;
                                }
                            }
                            if (checkifexists(localmachineDataCollection))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineDataCollection, "LimitDiagnosticLogCollection", 0);
                                if (isdefault == true)
                                {
                                    limitlog++;
                                }
                            }
                            if (checkifexists(cloudContent))
                            {
                                bool isdefault = await checkvalueisdefault(0, cloudContent, "DisableTailoredExperiencesWithDiagnosticData", 0);
                                if (isdefault == true)
                                {
                                    dte++;
                                }
                            }
                            if (checkifexists(localmachineDataCollection))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineDataCollection, "AllowTelemetry", 0);
                                if (isdefault == true)
                                {
                                    allowtelemetry++;
                                }
                            }
                            if (checkifexists(localmachineAppCompat))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineAppCompat, "DisableUAR", 0);
                                if (isdefault == true)
                                {
                                    usersteps++;
                                }
                            }
                            if (checkifexists(passwordReveal))
                            {
                                bool isdefault = await checkvalueisdefault(0, passwordReveal, "DisablePasswordReveal", 0);
                                if (isdefault == true)
                                {
                                    passreveal++;
                                }
                            }
                            if (checkifexists(localmachineapprivacy))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineapprivacy, "LetAppsAccessLocation", 0);
                                if (isdefault == true)
                                {
                                    letappsaccesslocation++;
                                }
                            }
                            if (checkifexists(localmachineapprivacy))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineapprivacy, "LetAppsGetDiagnosticInfo", 0);
                                if (isdefault == true)
                                {
                                    diagnosticaccess++;
                                }
                            }
                            if (checkifexists(currentuseraccountinfo))
                            {
                                bool isdefault = await checkvalueisdefault("Allow", currentuseraccountinfo, "Value", 0);
                                if (isdefault == true)
                                {
                                    accountinfo++;
                                }
                            }
                            if (checkifexists(localmachineaccountinfo))
                            {
                                bool isdefault = await checkvalueisdefault("Allow", ConsentStorelocation, "Value", 0);
                                if (isdefault == true)
                                {
                                    accountinfo++;
                                }
                            }
                            if (checkifexists(localmachineSystem))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineSystem, "AllowCrossDeviceClipboard", 0);
                                if (isdefault == true)
                                {
                                    crossdeviceclipboard++;
                                }
                            }
                            if (checkifexists(localmachineSystem))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineSystem, "UploadUserActivities", 0);
                                if (isdefault == true)
                                {
                                    uploadactivity++;
                                }
                            }
                            if (checkifexists(localmachineSystem))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineSystem, "AllowClipboardHistory", 0);
                                if (isdefault == true)
                                {
                                    clipboardhistory++;
                                }
                            }
                            if (checkifexists(localmachineSystem))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineSystem, "EnableActivityFeed", 0);
                                if (isdefault == true)
                                {
                                    recodinguseractivity++;
                                }
                            }
                            if (checkifexists(localmachineSystem))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineSystem, "PublishUserActivities", 0);
                                if (isdefault == true)
                                {
                                    storinguseractivity++;
                                }
                            }
                            if (checkifexists(locationandsensors))
                            {
                                bool isdefault = await checkvalueisdefault(0, locationandsensors, "DisableWindowsLocationProvider", 0);
                                if (isdefault == true)
                                {
                                    scriptinglocation++;
                                }
                            }
                            if (checkifexists(locationandsensors))
                            {
                                bool isdefault = await checkvalueisdefault(0, locationandsensors, "DisableLocationScripting", 0);
                                if (isdefault == true)
                                {
                                    scriptinglocation++;
                                }
                            }
                            if (checkifexists(SensorService))
                            {
                                bool isdefault = await checkvalueisdefault(3, SensorService, "Start", 1);
                                if (isdefault == true)
                                {
                                    locationsensors++;
                                }
                            }
                            if (checkifexists(Autororation))
                            {
                                bool isdefault = await checkvalueisdefault(1, Autororation, "Enable", 1);
                                if (isdefault == true)
                                {
                                    locationsensors++;
                                }
                            }
                            if (checkifexists(ConsentStorelocation))
                            {
                                bool isdefault = await checkvalueisdefault("Allow", ConsentStorelocation, "Value", 0);
                                if (isdefault == true)
                                {
                                    locationservices++;
                                }
                            }
                            if (checkifexists(locationandsensors))
                            {
                                bool isdefault = await checkvalueisdefault(0, locationandsensors, "DisableLocation", 0);
                                if (isdefault == true)
                                {
                                    locationservices++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "TyposquattingCheckerEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgetc++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "TyposquattingCheckerEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgetc++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "SmartScreenPuaEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgesmartscreen++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "SmartScreenEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgesmartscreen++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "RedirectSitesFromInternetExplorerRedirectMode", 0);
                                if (isdefault == true)
                                {
                                    edgeie++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "SafeBrowsingEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgesb++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "PasswordManagerEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgepm++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineEdge, "NetworkPredictionOptions", 0);
                                if (isdefault == true)
                                {
                                    edgenpo++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "ResolveNavigationErrorsUseWebService", 0);
                                if (isdefault == true)
                                {
                                    edgewsfne++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "AlternateErrorPagesEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgesimilarsitesuggestions++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "SpellcheckEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgespellcheck++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "HubsSidebarEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgehubssidebar++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "addressBarMicrosoftSearchInBingProviderEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgesaws++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "EdgeShoppingAssistantEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgeshopassistant++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "SearchSuggestEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgesaws++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "LocalProvidersEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgelpe++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "AutofillEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgeautofills++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "AutofillCreditCardEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgeacowa++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "UserFeedbackAllowed", 0);
                                if (isdefault == true)
                                {
                                    edgeuserfeedback++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "AutofillAddressEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgeautofill++;
                                }
                            }

                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "PaymentMethodQueryEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgepayment++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "SearchSuggestEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgeasn++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "PersonalizationReportingEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgeasn++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "EdgeCollectionsEnabled", 0);
                                if (isdefault == true)
                                {
                                    edgetracing++;
                                }
                            }
                            if (checkifexists(localmachineEdge))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineEdge, "DiagnosticData", 0);
                                if (isdefault == true)
                                {
                                    edgetracing++;
                                }
                            }
                            if (checkifexists(localmachineMicrosoftCEIPscheduled))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineMicrosoftCEIPscheduled, "AllowTelemetry", 1);
                                if (isdefault == true)
                                {
                                    ceip++;
                                }
                            }
                            if (checkifexists(currentuserDisableOfficeCeip))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserDisableOfficeCeip, "CEIPEnable", 0);
                                if (isdefault == true)
                                {
                                    ceip++;
                                }
                            }
                            if (checkifexists(currentuserDisableMicrosoftCeip))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserDisableMicrosoftCeip, "CEIPEnable", 0);
                                if (isdefault == true)
                                {
                                    ceip++;
                                }
                            }
                            if (checkifexists(localmachineApplicationExperienceProgram))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineApplicationExperienceProgram, "DisableCustomerImprovementProgram", 0);
                                if (isdefault == true)
                                {
                                    ceip++;
                                }
                            }
                            if (checkifexists(localmachineceip))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineceip, "CEIPEnable", 0);
                                if (isdefault == true)
                                {
                                    ceip++;
                                }
                            }
                            if (checkifexists(localmachineceip))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachineceip, "CEIPEnable", 0);
                                if (isdefault == true)
                                {
                                    ceip++;
                                }
                            }



                            if (checkifexists(localmachinewindowserrorreportingservice))
                            {
                                bool isdefault = await checkvalueisdefault(3, localmachinewindowserrorreportingservice, "Start", 1);
                                if (isdefault == true)
                                {
                                    errorreporting++;
                                }
                            }
                            if (checkifexists(currentuserwindowserrorreporting))
                            {
                                bool isdefault = await checkvalueisdefault(0, currentuserwindowserrorreporting, "Disabled", 0);
                                if (isdefault == true)
                                {
                                    errorreporting++;
                                }
                            }
                            if (checkifexists(localmachinedisablemessagebackup))
                            {
                                bool isdefault = await checkvalueisdefault(1, localmachinedisablemessagebackup, "AllowMessageSync", 0);
                                if (isdefault == true)
                                {
                                    messagebackup++;
                                }
                            }
                            if (checkifexists(localmachinedisablemessagebackuptomicrosoftaccount))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinedisablemessagebackuptomicrosoftaccount, "DisableMessagingSync", 0);
                                if (isdefault == true)
                                {
                                    messagebackup++;
                                }
                            }
                            if (checkifexists(currentuserdisablesmsmmscloudsync))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserdisablesmsmmscloudsync, "CloudServiceSyncEnabled", 0);
                                if (isdefault == true)
                                {
                                    messagebackup++;
                                }
                            }
                            if (checkifexists(disablegeneralsyncsettingsformessages))
                            {
                                bool isdefault = await checkvalueisdefault(1, disablegeneralsyncsettingsformessages, "Enabled", 0);
                                if (isdefault == true)
                                {
                                    messagebackup++;
                                }
                            }

                            if (checkifexists(currentuserDisableAdvertisingID))
                            {
                                bool isdefault = await checkvalueisdefault(1, currentuserDisableAdvertisingID, "Enabled", 1);
                                if (isdefault == true)
                                {
                                    AdvertisingID++;
                                }
                            }
                            if (checkifexists(localmachineDisableAdvertisingID))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineDisableAdvertisingID, "DisabledByGroupPolicy", 0);
                                if (isdefault == true)
                                {
                                    AdvertisingID++;
                                }
                            }

                            if (checkifexists(localmachinehandwritingerrorreports))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinehandwritingerrorreports, "PreventHandwritingErrorReports", 0);
                                if (isdefault == true)
                                {
                                    handwritingerrorreports++;
                                }
                            }
                            if (checkifexists(localmachineAppCompat))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachineAppCompat, "DisableInventory", 0);
                                if (isdefault == true)
                                {
                                    inventorycollector++;
                                }
                            }

                            if (checkifexists(localmachinehandwritingsharing))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinehandwritingsharing, "RestrictImplicitTextCollection", 0);
                                if (isdefault == true)
                                {
                                    handwritingsharing++;
                                }
                            }
                            if (checkifexists(localmachinehandwritingsharing))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinehandwritingsharing, "RestrictImplicitInkCollection", 0);
                                if (isdefault == true)
                                {
                                    handwritingsharing++;
                                }
                            }
                            if (checkifexists(localmachinehandwritingsharing))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinehandwritingsharing, "PreventHandwritingDataSharing", 0);
                                if (isdefault == true)
                                {
                                    handwritingsharing++;
                                }
                            }
                            if (checkifexists(localmachinedisablecameraonlogon))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinedisablecameraonlogon, "NoLockScreenCamera", 0);

                                if (isdefault == true)
                                {
                                    disablecameraonlogon++;
                                }

                            }

                            if (checkifexists(localmachinesystemdisablecameraonlogon))
                            {
                                bool isdefault = await checkvalueisdefault(0, localmachinesystemdisablecameraonlogon, "NoLockScreenCamera", 0);

                                if (isdefault == true)
                                {

                                    disablecameraonlogon++;
                                }

                            }
                            await main.Dispatcher.InvokeAsync(async () =>
                            {
                                await setstatusof(main.DisableExplorerSuggestionsStatus, sr, 2);
                                await setstatusof(main.DisableADCAdsStatus, ads2, 1);
                                await setstatusof(main.DisableExplorerAdsStatus, ads1, 1);
                                await setstatusof(main.DisablePaintAIFillStatus, imfill, 1);
                                await setstatusof(main.DisablePaintImageCreatorStatus, imcreator, 1);
                                await setstatusof(main.DisablePaintCoCreatorStatus, imc, 1);
                                await setstatusof(main.DisableWindowsCopilotRecallStatus, recall, 1);
                                await setstatusof(main.DisableWindowsCopilotStatus, copilot, 1);
                                await setstatusof(main.DisableNCSIStatus, ncs, 2);
                                await setstatusof(main.DisableRemoteConnectionsStatus, rmtaccess2, 2);
                                await setstatusof(main.DisableRemoteAssistanceStatus, rmtaccess1, 1);
                                await setstatusof(main.DisableOfflineMapsNetworkTrafficStatus, mapsoffline, 1);
                                await setstatusof(main.DisableMapUpdatesStatus, mapsupdate, 1);
                                await setstatusof(main.DisableKMSOnlineActivationStatus, kms, 2);
                                await setstatusof(main.DisableFeedbackRemindersStatus, feedbackrem, 2);
                                await setstatusof(main.DisableNewsAndInterestsTaskbarStatus, news, 1);
                                await setstatusof(main.DisableMeetNowTaskbarStatus, meetnow, 1);
                                await setstatusof(main.DisablePCToMobileConnectionStatus, mmx, 2);
                                await setstatusof(main.DisableMalwareReportingStatus, malinfosubmit, 1);
                                await setstatusof(main.DisableSampleSubmissionStatus, autosamplesubmitting, 1);
                                await setstatusof(main.DisableSpynetMembershipStatus, spynetmembership, 1);
                                await setstatusof(main.DisableSpeechModuleUpdatesStatus, speechmodelupdate, 1);
                                await setstatusof(main.DisableWU_P2PStatus, dod, 1);
                                await setstatusof(main.DisableOneSettingsDownloadStatus, dosd, 1);
                                await setstatusof(main.DisableDiagLogsStatus, limitlog, 1);
                                await setstatusof(main.DisableDiagCustomizationStatus, dte, 1);
                                await setstatusof(main.DisableAppTelemetryStatus, allowtelemetry, 1);
                                await setstatusof(main.DisableStepsRecorderStatus, usersteps, 1);
                                await setstatusof(main.DisablePasswordRevealStatus, passreveal, 1);
                                await setstatusof(main.DisableAppAccessLocationStatus, letappsaccesslocation, 1);
                                await setstatusof(main.DisableAppAccessDiagnosticsStatus, diagnosticaccess, 1);
                                await setstatusof(main.DisableAppAccessUserInfoStatus, accountinfo, 2);
                                await setstatusof(main.DisableClipboardHistoryStatus, clipboardhistory, 1);
                                await setstatusof(main.DisableClipboardCloudSyncStatus, crossdeviceclipboard, 1);
                                await setstatusof(main.DisableActivitySubmissionStatus, uploadactivity, 1);
                                await setstatusof(main.DisableUserActivityRecordingStatus, storinguseractivity, 1);
                                await setstatusof(main.DisableActivityHistoryStorageStatus, recodinguseractivity, 1);
                                await setstatusof(main.DisableLocationScriptingStatus, scriptinglocation, 2);
                                await setstatusof(main.DisableLocationSensorsStatus, locationsensors, 2);
                                await setstatusof(main.DisableSystemLocationFunctionalityStatus, locationservices, 2);
                                await setstatusof(main.DisableTypesquattingCheckerStatus, edgetc, 1);
                                await setstatusof(main.DisableSmartScreenFilterStatus, edgesmartscreen, 2);
                                await setstatusof(main.DisableIEtoEdgeRedirectStatus, edgeie, 1);
                                await setstatusof(main.DisableSiteSafetyServicesStatus, edgesb, 1);
                                await setstatusof(main.DisableSavingPasswordsStatus, edgepm, 1);
                                await setstatusof(main.DisablePagePreloadStatus, edgenpo, 1);
                                await setstatusof(main.DisableWebServiceResolveErrorsStatus, edgewsfne, 1);
                                await setstatusof(main.DisableEnhancedSpellCheckStatus, edgespellcheck, 1);
                                await setstatusof(main.DisableEdgeSidebarStatus, edgehubssidebar, 1);
                                await setstatusof(main.DisableShoppingAssistantStatus, edgeshopassistant, 1);
                                await setstatusof(main.DisableSearchWebsiteSuggestionsStatus, edgesaws, 1);
                                await setstatusof(main.DisableLocalProviderSuggestionsStatus, edgelpe, 1);
                                await setstatusof(main.DisableFormSuggestionsStatus, edgeautofills, 1);
                                await setstatusof(main.DisableAutoCompleteAddressesStatus, edgeautofill, 1);
                                await setstatusof(main.DisableSimilarSiteSuggestionsStatus, edgesimilarsitesuggestions, 1);
                                await setstatusof(main.DisableToolbarFeedbackStatus, edgeuserfeedback, 1);
                                await setstatusof(main.DisablePersonalizedAdsSearchStatus, edgeasn, 2);
                                await setstatusof(main.DisableCreditCardAutofillStatus, edgeacowa, 1);
                                await setstatusof(main.DisableSavedPaymentCheckStatus, edgepayment, 1);
                                await setstatusof(main.DisableTracingWebStatus, edgetracing, 2);
                                await setstatusof(main.DisableCEIPStatus, ceip, 7);
                                await setstatusof(main.DisableBluetoothAdvertisingStatus, bluetoothadvertisements, 8);

                                await setstatusof(main.DisableWindowsErrorReportingStatus, errorreporting, 2);
                                await setstatusof(main.DisableBackupTextMessagesStatus, messagebackup, 4);
                                await setstatusof(main.DisableAdvertisingIDStatus, AdvertisingID, 2);
                                await setstatusof(main.DisableHandwritingErrorReportsStatus, handwritingerrorreports, 1);
                                await setstatusof(main.DisableInventoryCollectorStatus, inventorycollector, 1);
                                await setstatusof(main.DisableHandwritingDataSharingStatus, handwritingsharing, 3);
                                await setstatusof(main.DisableCameraLogonScreenStatus, disablecameraonlogon, 2);
                            });

                        }

                    }




                }
                catch (Exception ex)
                {
                    main.Dispatcher.InvokeAsync(() =>
                    {
                        main.PrivacyStatusLabel.Text = ex.Message + " " + ex.StackTrace;
                    });
                }
            }
            public Task setstatusof(TextBlock textblock, int enabledCount, int totalCount)
            {
                if (enabledCount == 0)
                    textblock.Text = "Status: Disabled";
                else if (enabledCount >= totalCount)
                    textblock.Text = "Status: Enabled";
                else
                    textblock.Text = $"Status: Partially enabled ({enabledCount}/{totalCount})";

                return Task.CompletedTask;
            }
            public async Task<bool> checkvalueisdefault(object defaultVal, RegistryKey key, string name, int defaultExists)
            {
                try
                {
                    if (key == null && defaultExists == 0)
                    {
                        return true; 
                    }

                    if (key == null && defaultExists == 1)
                    {
                        return false; 
                    }

                    object startValue = key.GetValue(name, null);
                    bool valueExists = (startValue != null);

                    if (defaultExists == 0 && !valueExists)
                    {
                        return true;  
                    }

                    if (defaultExists == 1 && !valueExists)
                    {
                        return false;  
                    }

                
                    if (startValue != null && startValue.Equals(defaultVal))
                    {
                        return true; 
                    }
                    else
                    {
                        return false; 
                    }
                }
                catch (Exception ex)
                {
                    return true;  
                }
            }


            public bool checkifexists(RegistryKey key)
            {
             
                if(key == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
               
            }
           
        }
        public class WinUpdateMonitor
        {
            MainWindow main;
            public WinUpdateMonitor(MainWindow main)
            {
                this.main = main;
            }
            public async Task run()
            {
                try
                {
                    ServiceController winupdate = new ServiceController("wuauserv");
                    RegistryKey wuauserv = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\wuauserv");
                    RegistryKey usosvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\UsoSvc");
                    RegistryKey WaaSMedicSvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WaaSMedicSvc");
                    RegistryKey DeliveryOptimization = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization"); 
                    RegistryKey WindowsUpdate = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"); 
                    RegistryKey DriverSearching = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DriverSearching");  
                    RegistryKey CVDriverSearching = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching");  
                    RegistryKey DeviceMetadata = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Device Metadata");  
                    RegistryKey DeviceInstaller = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Installer");  
           
                    RegistryKey WindowsUpdateAU = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU");
                    RegistryKey WindowsUpdateUX = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX");
                    RegistryKey WindowsUpdateUXS = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings");
                    RegistryKey WindowsUpdateExplorer = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer");
                    
                    RegistryKey Gwx = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Gwx");  
                    while (true)
                    {
                        if (main.processing3 != 1)
                        {
                            try { await Task.Delay(5000, main.token3); } catch (TaskCanceledException) { continue; }
                            main.winupdatelogs.Clear();
                            string servicestatus = "";
                            string registrystatus = "";
                            int registrychanges = 0;
                            bool exists = ServiceController.GetServices().Any(s => s.ServiceName.Equals("wuauserv", StringComparison.OrdinalIgnoreCase));
                            if (exists)
                            {
                                if (winupdate.Status == ServiceControllerStatus.Running)
                                {

                                    servicestatus = "Windows Update service running (wuauserv)";
                                }
                                else
                                {
                                    servicestatus = "Windows Update service not running (wuauserv)";
                                }
                            }
                            else
                            {
                                servicestatus = "wuauserv service does not exists.";
                            }
                            if (WindowsUpdateExplorer != null)
                            {
                                string[] subkeys5 = WindowsUpdateExplorer.GetValueNames();

                                if (subkeys5.Length > 3)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winupdatelogs.Add("Windows Update: " + name + " in " + WindowsUpdateExplorer.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (wuauserv != null)
                            {
                                object startValue = wuauserv?.GetValue("Start");

                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 2)
                                    {
                                        main.winupdatelogs.Add("Windows Update: wuauserv Changed to = " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\wuauserv default value (2)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winupdatelogs.Add("Windows Update: Start value not found in " + wuauserv.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SYSTEM\\CurrentControlSet\\Services\\wuauserv" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (usosvc != null)
                            {
                                object startValue = usosvc?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 2)
                                    {
                                        main.winupdatelogs.Add("Windows Update: usosvc Changed to = " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\UsoSvc default value (2)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winupdatelogs.Add("Windows Update: Start value not found in " + usosvc.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SYSTEM\\CurrentControlSet\\Services\\UsoSvc" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }


                            if (WaaSMedicSvc != null)
                            {
                                object startValue = WaaSMedicSvc?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 3)
                                    {
                                        main.winupdatelogs.Add("Windows Update: WaaSMedicSvc Changed to = " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc default value (3)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winupdatelogs.Add("Windows Update: Start value not found in " + WaaSMedicSvc.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (DeliveryOptimization != null)
                            {
                                string[] valueNames = DeliveryOptimization.GetValueNames();
                                if (valueNames.Length > 0)
                                {
                                    main.winupdatelogs.Add("Windows Update: DeliveryOptimization policy found with " + valueNames.Length + " values - This may affect Windows Update " + " in " + DeliveryOptimization.Name);
                                    foreach (string valueName in valueNames)
                                    {
                                        object value = DeliveryOptimization.GetValue(valueName);
                                        main.winupdatelogs.Add("  - " + valueName + " = " + value);
                                    }
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Policies\\Microsoft\\Windows\\DeliveryOptimization" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (WindowsUpdate != null)
                            {
                                string[] valueNames = WindowsUpdate.GetValueNames();
                                if (valueNames.Length > 0)
                                {
                                    main.winupdatelogs.Add("Windows Update: WindowsUpdate policy found with " + valueNames.Length + " values - This may block updates " + " in " + WindowsUpdate.Name);
                                    foreach (string valueName in valueNames)
                                    {
                                        object value = WindowsUpdate.GetValue(valueName);
                                        main.winupdatelogs.Add("  - " + valueName + " = " + value);
                                    }
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (DriverSearching != null)
                            {
                                object dontSearchWU = DriverSearching.GetValue("DontSearchWindowsUpdate");
                                object searchOrderConfig = DriverSearching.GetValue("SearchOrderConfig");
                                if (dontSearchWU != null && (int)dontSearchWU == 1)
                                {
                                    main.winupdatelogs.Add("Windows Update: Driver searching from Windows Update is DISABLED " + " in " + DriverSearching.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                                if (searchOrderConfig != null && (int)searchOrderConfig != 0)
                                {
                                    main.winupdatelogs.Add("Windows Update: Driver search order changed to " + searchOrderConfig + " in " + DriverSearching.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Policies\\Microsoft\\Windows\\DriverSearching" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (CVDriverSearching != null)
                            {
                                object searchOrderConfig = CVDriverSearching.GetValue("SearchOrderConfig");
                                if (searchOrderConfig != null && (int)searchOrderConfig != 1)
                                {
                                    main.winupdatelogs.Add("Windows Update: CurrentVersion DriverSearching changed from default (1) to " + searchOrderConfig + " in " + CVDriverSearching.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\DriverSearching" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (DeviceMetadata != null)
                            {
                                object preventDeviceMetadata = DeviceMetadata.GetValue("PreventDeviceMetadataFromNetwork");
                                if (preventDeviceMetadata != null && (int)preventDeviceMetadata == 1)
                                {
                                    main.winupdatelogs.Add("Windows Update: Device Metadata retrieval from network is DISABLED in " + DeviceMetadata.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Policies\\Microsoft\\Windows\\Device Metadata" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (WindowsUpdateUX != null)
                            {
                                object IsConvergedUpdateStackEnabled = WindowsUpdateUX.GetValue("IsConvergedUpdateStackEnabled");
                                if (IsConvergedUpdateStackEnabled != null && (int)IsConvergedUpdateStackEnabled == 0)
                                {
                                    main.winupdatelogs.Add("Windows Update: DIsConvergedUpdateStackEnabled are DISABLED in " + WindowsUpdateUX.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Microsoft\\WindowsUpdate\\UX" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (DeviceInstaller != null)
                            {
                                object disableCoInstallers = DeviceInstaller.GetValue("DisableCoInstallers");
                                if (disableCoInstallers != null && (int)disableCoInstallers == 1)
                                {
                                    main.winupdatelogs.Add("Windows Update: DeviceCoInstallers are DISABLED in " + DeviceInstaller.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Device Installer" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (Gwx != null)
                            {
                                string[] valueNames = Gwx.GetValueNames();
                                if (valueNames.Length > 0)
                                {
                                    main.winupdatelogs.Add("Windows Update: DeliveryOptimization policy found with " + valueNames.Length + " values - This may affect Windows Update " + " in " + Gwx.Name);
                                    foreach (string valueName in valueNames)
                                    {
                                        object value = DeliveryOptimization.GetValue(valueName);
                                        main.winupdatelogs.Add("  - " + valueName + " = " + value);
                                    }
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Policies\\Microsoft\\Windows\\Gwx" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (WindowsUpdateAU != null)
                            {
                                string[] subkeys5 = WindowsUpdateAU.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winupdatelogs.Add("Windows Update: " + name + " in " + WindowsUpdateAU.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (WindowsUpdateUX != null)
                            {
                                string[] subkeys5 = WindowsUpdateUX.GetValueNames();

                                if (subkeys5.Length > 1)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winupdatelogs.Add("Windows Update: " + name + " in " + WindowsUpdateUX.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winupdatelogs.Add("Windows Update: " + "SOFTWARE\\Microsoft\\WindowsUpdate\\UX" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (WindowsUpdateUXS != null)
                            {
                                string[] subkeys5 = WindowsUpdateUXS.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winupdatelogs.Add("Windows Update: " + name + " in " + WindowsUpdateUXS.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }

                            main.Dispatcher.InvokeAsync(async () =>
                            {
                                if (registrystatus == "")
                                {
                                    main.WindowsUpdateStatusLabel.Text = servicestatus + " and not found registry changes.";
                                }
                                else
                                {
                                    main.WindowsUpdateStatusLabel.Text = servicestatus + " " + registrystatus + " (for more details click > icon and export log file)";
                                }


                            });
                        }
                      
                       
                    }
                }   
                catch (Exception ex)
                {
                    main.Dispatcher.InvokeAsync(() =>
                    {
                        main.WindowsUpdateStatusLabel.Text = ex.Message + " " + ex.StackTrace;
                    });
                }
            }
        }
        public class WinFirewallMonitor
        {
            MainWindow main;
            public WinFirewallMonitor(MainWindow main)
            {
                this.main = main;
            }
            public async Task run()
            {
                try
                {
                    ServiceController winfirewall = new ServiceController("mpssvc");

                    RegistryKey mpssvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\mpssvc");
                    RegistryKey bfe = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\BFE");

                    RegistryKey domainprofile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile");
                    RegistryKey domainprofilelogging = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile\Logging");

                    RegistryKey standardprofile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile");
                    RegistryKey standardprofilelogging = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile\Logging");

                    RegistryKey publicprofile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile");
                    RegistryKey publicprofilelogging = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile\Logging");

                    RegistryKey policiesdomainprofile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\DomainProfile");
                    RegistryKey policiesstandardprofile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\StandardProfile");
                    RegistryKey policiespublicprofile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\PublicProfile");

                    RegistryKey securitycenter = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Security Center");

                    while (true)
                    {

                        if (main.processing2 != 1)
                        {
                            try { await Task.Delay(5000, main.token2); } catch (TaskCanceledException) { continue; }
                            main.winfirewalllogs.Clear();
                            string servicestatus = "";
                            string registrystatus = "";
                            int registrychanges = 0;
                            bool exists = ServiceController.GetServices().Any(s => s.ServiceName.Equals("mpssvc", StringComparison.OrdinalIgnoreCase));
                            if (exists)
                            {
                                if (winfirewall.Status == ServiceControllerStatus.Running)
                                {

                                    servicestatus = "Windows Firewall service running (mpssvc)";
                                }
                                else
                                {
                                    servicestatus = "Windows Firewall service not running (mpssvc)";
                                }
                            }
                            else
                            {
                                servicestatus = "mpssvc service does not exists.";
                            }
                            if (bfe != null)
                            {
                                object startValue = bfe?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 2)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: bfe Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\bfe default value (2)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: Start value not found in " + bfe.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SYSTEM\\CurrentControlSet\\Services\\bfe" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (mpssvc != null)
                            {
                                object startValue = mpssvc?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 2)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: mpssvc Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\mpssvc default value (2)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: Start value not found in " + mpssvc.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SYSTEM\\CurrentControlSet\\Services\\mpssvc" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (securitycenter != null)
                            {
                                object startValue = securitycenter?.GetValue("cval");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 1)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: cva Changed to  =  " + startInt + " location: " + "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Security Center\\cva default value (1)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: cval value not found in " + securitycenter.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }

                            if (securitycenter != null)
                            {
                                string[] subkeys5 = securitycenter.GetValueNames();

                                if (subkeys5.Length > 1)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + securitycenter.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SOFTWARE\\Microsoft\\Security Center" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (policiespublicprofile != null)
                            {
                                string[] subkeys5 = policiespublicprofile.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + policiespublicprofile.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PublicProfile" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (policiesstandardprofile != null)
                            {
                                string[] subkeys5 = policiesstandardprofile.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + policiesstandardprofile.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\StandardProfile" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (policiesdomainprofile != null)
                            {
                                string[] subkeys5 = policiesdomainprofile.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + policiesdomainprofile.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\DomainProfile" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (policiesdomainprofile != null)
                            {
                                string[] subkeys5 = policiesdomainprofile.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + policiesdomainprofile.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\DomainProfile" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (publicprofilelogging != null)
                            {
                                object startValue = publicprofilelogging?.GetValue("LogDroppedPackets");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: LogDroppedPackets Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\PublicProfile\\Logging\\LogDroppedPackets default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: LogDroppedPackets value not found in " + publicprofilelogging.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (publicprofilelogging != null)
                            {
                                object startValue = publicprofilelogging?.GetValue("LogSuccessfulConnections");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: LogSuccessfulConnections Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\PublicProfile\\Logging\\LogSuccessfulConnections default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: LogSuccessfulConnections value not found in " + publicprofilelogging.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (publicprofilelogging != null)
                            {
                                string[] subkeys5 = publicprofilelogging.GetValueNames();

                                if (subkeys5.Length > 4)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + publicprofilelogging.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\PublicProfile\\Logging" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (publicprofile != null)
                            {
                                object startValue = publicprofile?.GetValue("DisableNotifications");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: DisableNotifications Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\PublicProfile\\DisableNotifications default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: DisableNotifications value not found in " + publicprofile.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (publicprofile != null)
                            {
                                object startValue = publicprofile?.GetValue("EnableFirewall");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 1)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: EnableFirewall Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\PublicProfile\\EnableFirewall default value (1)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: EnableFirewall value not found in " + publicprofile.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (publicprofile != null)
                            {
                                string[] subkeys5 = publicprofile.GetValueNames();

                                if (subkeys5.Length > 2)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + publicprofile.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\PublicProfile" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (standardprofilelogging != null)
                            {
                                object startValue = standardprofilelogging?.GetValue("LogDroppedPackets");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: LogDroppedPackets Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile\\Logging\\LogDroppedPackets default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: LogDroppedPackets value not found in " + standardprofilelogging.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (standardprofilelogging != null)
                            {
                                object startValue = standardprofilelogging?.GetValue("LogSuccessfulConnections");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: LogSuccessfulConnections Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile\\Logging\\LogSuccessfulConnections default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: LogSuccessfulConnections value not found in " + standardprofilelogging.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (standardprofilelogging != null)
                            {
                                string[] subkeys5 = standardprofilelogging.GetValueNames();

                                if (subkeys5.Length > 4)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + standardprofilelogging.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile\\Logging" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (standardprofile != null)
                            {
                                object startValue = standardprofile?.GetValue("DisableNotifications");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: DisableNotifications Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile\\DisableNotifications default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: DisableNotifications value not found in " + standardprofile.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (standardprofile != null)
                            {
                                object startValue = standardprofile?.GetValue("EnableFirewall");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 1)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: EnableFirewall Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile\\EnableFirewall default value (1)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: EnableFirewall value not found in " + standardprofile.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (standardprofile != null)
                            {
                                string[] subkeys5 = standardprofile.GetValueNames();

                                if (subkeys5.Length > 2)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + standardprofile.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (domainprofilelogging != null)
                            {

                                object startValue = domainprofilelogging?.GetValue("LogDroppedPackets");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: LogDroppedPackets Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\DomainProfile\\Logging\\LogDroppedPackets default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: LogDroppedPackets value not found in " + domainprofilelogging.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }


                            }
                            if (domainprofilelogging != null)
                            {
                                object startValue = domainprofilelogging?.GetValue("LogSuccessfulConnections");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: LogSuccessfulConnections Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\DomainProfile\\Logging\\LogSuccessfulConnections default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: LogSuccessfulConnections value not found in " + domainprofilelogging.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }

                            if (domainprofilelogging != null)
                            {
                                string[] subkeys5 = domainprofilelogging.GetValueNames();

                                if (subkeys5.Length > 4)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + domainprofilelogging.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\DomainProfile\\Logging" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (domainprofile != null)
                            {
                                object startValue = domainprofile?.GetValue("DisableNotifications");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: DisableNotifications Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\DomainProfile\\DisableNotifications default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: DisableNotifications value not found in " + domainprofile.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (domainprofile != null)
                            {
                                object startValue = domainprofile?.GetValue("EnableFirewall");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 1)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: EnableFirewall Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\DomainProfile\\EnableFirewall default value (1)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.winfirewalllogs.Add("Microsoft Firewall: EnableFirewall value not found in " + domainprofile.Name);
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (domainprofile != null)
                            {
                                string[] subkeys5 = domainprofile.GetValueNames();

                                if (subkeys5.Length > 2)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.winfirewalllogs.Add("Microsoft Firewall: " + name + " in " + domainprofile.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.winfirewalllogs.Add("Microsoft Firewall: " + "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\DomainProfile" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            main.Dispatcher.InvokeAsync(async () =>
                            {
                                if (registrystatus == "")
                                {
                                    main.FirewallStatusLabel.Text = servicestatus + " and not found registry changes.";
                                }
                                else
                                {
                                    main.FirewallStatusLabel.Text = servicestatus + " " + registrystatus + " (for more details click > icon and export log file)";
                                }


                            });
                        }
                      
                     
                    }



                }
                catch (Exception ex)
                {
                    main.Dispatcher.InvokeAsync(() =>
                    {
                        main.FirewallStatusLabel.Text = ex.Message + " " + ex.StackTrace;
                    });
                }
            }

        }
        public class WinDefendMonitor
        {
            MainWindow main;
            public WinDefendMonitor(MainWindow main)
            {
                this.main = main;
            }
            public async Task run()
            {
                try
                {
                 
                    ServiceController windefend = new ServiceController("WinDefend");
                  
                    RegistryKey vtp = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Virus and threat protection"); 
                    RegistryKey sense_service = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Sense");
                    RegistryKey wd_boot = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WdBoot");  
                    RegistryKey wd_filter = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WdFilter"); 
                    RegistryKey wd_nis_drv = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WdNisDrv");  
                    RegistryKey wd_nis_svc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WdNisSvc");  
                    RegistryKey windowsdefender = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender");
                    RegistryKey windowsdefendersystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender");
                    RegistryKey wdrealtimeprotectionSystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection");
                    RegistryKey SignatureUpdateSystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Signature Update");
                   
                    RegistryKey spynet = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet");
                    RegistryKey securityhealthservice = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SecurityHealthService");

                  
                    RegistryKey windowsdefenderui = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Policy Manager");
                    RegistryKey wdrealtimeprotection = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection");
                    RegistryKey micdef = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender");  
                    RegistryKey softdef = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender"); 
                    RegistryKey wscsvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\wscsvc");
                    RegistryKey defendertelemetry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Reporting");
                    RegistryKey defenderfeatures = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Features");

                    RegistryKey accountprotection = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Account protection");
                    RegistryKey appandbrowserprotection = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\App and browser protection");
                    RegistryKey deviceperformanceandhealth = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Device performance and health");
                    RegistryKey devicesecurity = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Device security");
                    RegistryKey familyoptions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Family options");
                    RegistryKey firewallandnetworkprotection = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Firewall and network protection");
                    RegistryKey wdscnotify = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications");
                    RegistryKey mwdscnotify = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications");
                    RegistryKey uxnotify = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\UX Configuration");
                    RegistryKey securityandmaintenanceToastNotify = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance");
                    RegistryKey WindowsExplorer = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Explorer");
                    RegistryKey securitycenter = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Security Center");
                    RegistryKey securitycenterchangesinacitoncenter = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\Action Center\Checks");
                    while (true)
                        {

                        if(main.processing1 != 1)
                        {

                            try { await Task.Delay(5000, main.token1); } catch (TaskCanceledException) { continue; }
                            main.windefendlogs.Clear();
                            string servicestatus = "";
                            string registrystatus = "";
                            int registrychanges = 0;

                            bool exists = ServiceController.GetServices().Any(s => s.ServiceName.Equals("WinDefend", StringComparison.OrdinalIgnoreCase));
                            if (exists)
                            {
                                if (windefend.Status == ServiceControllerStatus.Running)
                                {

                                    servicestatus = "WinDefend service running";
                                }
                                else
                                {
                                    servicestatus = "WinDefend service not running";
                                }
                            }
                            else
                            {
                                servicestatus = "WinDefend service does not exists.";
                            }
                            if (securitycenterchangesinacitoncenter != null)
                            {
                                string[] subkeys5 = securitycenterchangesinacitoncenter.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Action Center: " + name + " in " + securitycenterchangesinacitoncenter.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                          
                            if (securityandmaintenanceToastNotify != null)
                            {
                                object startValue = securityandmaintenanceToastNotify?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.windefendlogs.Add("Security And Maintenance: Enabled value found and is changed to  =  " + startInt + " location: " + securitycenterchangesinacitoncenter.Name + " default value (1)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                              

                            }
                            else
                            {
                                main.windefendlogs.Add("Security And Maintenance: " + "Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\\Windows.SystemToast.SecurityAndMaintenance" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (securitycenter != null)
                            {
                                string[] subkeys5 = securitycenter.GetValueNames();

                                if (subkeys5.Length > 1)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Security And Maintenance Notifications: " + name + " in " + securitycenter.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Security And Maintenance Notifications: " + "SOFTWARE\\Microsoft\\Security Center" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (WindowsExplorer != null)
                            {
                                string[] subkeys5 = WindowsExplorer.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Security And Maintenance Notifications: " + name + " in " + WindowsExplorer.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Security And Maintenance Notifications: " + "SOFTWARE\\Policies\\Microsoft\\Windows\\Explorer" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (uxnotify != null)
                            {
                                string[] subkeys5 = uxnotify.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Windows Security Notifications: " + name + " in " + uxnotify.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            if (mwdscnotify != null)
                            {
                                string[] subkeys5 = mwdscnotify.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Windows Security Notifications: " + name + " in " + mwdscnotify.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }

                            if (wdscnotify != null)
                            {
                                string[] subkeys5 = wdscnotify.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Windows Security Notifications: " + name + " in " + wdscnotify.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Windows Security Notifications: " + "SOFTWARE\\Microsoft\\Windows Defender Security Center\\Notifications" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (defendertelemetry != null)
                            {
                                string[] subkeys5 = defendertelemetry.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + defendertelemetry.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Reporting" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (defenderfeatures != null)
                            {
                                var expected = new (string Name, RegistryValueKind Kind, object Value)[]
                  {
                ("MpPlatformKillbitsFromEngine", RegistryValueKind.Binary, new byte[8] {0,0,0,0,0,0,0,0}),
                ("TamperProtectionSource", RegistryValueKind.DWord, 0),
                ("MpCapability", RegistryValueKind.Binary, new byte[8] {0,0,0,0,0,0,0,0}),
                ("TamperProtection", RegistryValueKind.DWord, 0)
                  };

                                foreach (var (Name, Kind, Value) in expected)
                                {
                                    object actual = defenderfeatures?.GetValue(Name);
                                    if (actual == null)
                                    {
                                        continue;
                                    }

                                    bool ok = false;
                                    if (Kind == RegistryValueKind.DWord && actual is int dword)
                                    {
                                        ok = dword.Equals(Value);
                                    }
                                    else if (Kind == RegistryValueKind.Binary && actual is byte[] bytes && Value is byte[] expectedBytes)
                                    {
                                        if (bytes.Length == expectedBytes.Length)
                                        {
                                            ok = true;
                                            for (int i = 0; i < bytes.Length; i++)
                                                if (bytes[i] != expectedBytes[i])
                                                    registrychanges++;
                                        }
                                    }
                                }
                                main.windefendlogs.Add("Microsoft Defender: Found some changes inside " + "SOFTWARE\\Microsoft\\Windows Defender\\Features");
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender\\Features" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (firewallandnetworkprotection != null)
                            {
                                string[] subkeys5 = firewallandnetworkprotection.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + firewallandnetworkprotection.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender Security Center\\Firewall and network protection" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (familyoptions != null)
                            {
                                string[] subkeys5 = familyoptions.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + familyoptions.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender Security Center\\Family options" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (devicesecurity != null)
                            {
                                string[] subkeys5 = devicesecurity.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + devicesecurity.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender Security Center\\Device security" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (deviceperformanceandhealth != null)
                            {
                                string[] subkeys5 = deviceperformanceandhealth.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + deviceperformanceandhealth.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender Security Center\\Device performance and health" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (appandbrowserprotection != null)
                            {
                                string[] subkeys5 = appandbrowserprotection.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + appandbrowserprotection.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender Security Center\\App and browser protection" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            
                            if (accountprotection != null)
                            {
                                string[] subkeys5 = accountprotection.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + accountprotection.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender Security Center\\Account protection" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (vtp != null)
                            {
                                string[] subkeys5 = vtp.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + vtp.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender Security Center\\Virus and threat protection" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (softdef != null)
                            {
                                string[] subkeys5 = softdef.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + softdef.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Policies\\Microsoft\\Windows Defender" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (micdef != null)
                            {
                                string[] subkeys5 = micdef.GetValueNames();
                                main.windefendlogs.Add("Microsoft Defender: Found some changes inside " + micdef.Name);
                                if (subkeys5.Length > 19 || subkeys5.Length < 19)
                                {
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Microsoft\\Windows Defender " + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (wscsvc != null)
                            {
                                object startValue = wscsvc?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 2)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: wscsvc Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\wscsvc default value (2)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.windefendlogs.Add("Microsoft Defender: wscsvc Start Value not found in  " + "SYSTEM\\CurrentControlSet\\Services\\wscsvc");
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " changes.";
                                }

                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\CurrentControlSet\\Services\\wscsvc" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (wd_nis_svc != null)
                            {
                                object startValue = wd_nis_svc?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 3)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: WdNisSvc Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\WdNisSvc default value (3)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.windefendlogs.Add("Microsoft Defender: WdNisSvc Start Value not found in " + "SYSTEM\\CurrentControlSet\\Services\\WdNisSvc");
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " changes.";
                                }

                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\CurrentControlSet\\Services\\WdNisSvc" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (wd_nis_drv != null)
                            {
                                object startValue = wd_nis_drv?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 3)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: WdNisDrv Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\WdNisDrv default value (3)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.windefendlogs.Add("Microsoft Defender: WdNisDrv Start Value not found in " + "SYSTEM\\CurrentControlSet\\Services\\WdNisDrv");
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " changes.";
                                }

                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\CurrentControlSet\\Services\\WdNisDrv" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (wd_filter != null)
                            {
                                object startValue = wd_filter?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: WdFilter Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\WdFilter default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.windefendlogs.Add("Microsoft Defender: WdFilter Start Value not found in " + "SYSTEM\\CurrentControlSet\\Services\\WdFilter");
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }

                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\CurrentControlSet\\Services\\WdFilter" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (wd_boot != null)
                            {
                                object startValue = wd_boot?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 0)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: WdBoot Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\WdBoot default value (0)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.windefendlogs.Add("Microsoft Defender: WdBoot Start Value not found in " + "SYSTEM\\CurrentControlSet\\Services\\WdBoot");
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }

                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\CurrentControlSet\\Services\\WdBoot" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (sense_service != null)
                            {
                                object startValue = sense_service?.GetValue("Start");
                                if (sense_service != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 3)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: SecurityHealthService Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\Sense default value (3)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.windefendlogs.Add("Microsoft Defender: Sense Start Value not found in " + "SYSTEM\\CurrentControlSet\\Services\\Sense");
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }

                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\CurrentControlSet\\Services\\Sense" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (securityhealthservice != null)
                            {
                                object startValue = securityhealthservice?.GetValue("Start");
                                if (startValue != null)
                                {
                                    int startInt = (int)startValue;
                                    if (startInt != 3)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: SecurityHealthService Changed to  =  " + startInt + " location: " + "SYSTEM\\CurrentControlSet\\Services\\SecurityHealthService default value (3)");
                                        registrychanges += 1;
                                        registrystatus = "and found " + registrychanges + " changes.";
                                    }
                                }
                                else
                                {
                                    main.windefendlogs.Add("Microsoft Defender: SecurityHealthService Start Value not found in " + "SYSTEM\\CurrentControlSet\\Services\\SecurityHealthService");
                                    registrychanges += 1;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }

                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\CurrentControlSet\\Services\\SecurityHealthService" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }
                            if (wdrealtimeprotectionSystem != null)
                            {
                                string[] subkeys5 = wdrealtimeprotectionSystem.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + wdrealtimeprotectionSystem.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }




                            if (windowsdefenderui != null)
                            {
                                string[] subkeys5 = windowsdefenderui.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + windowsdefenderui.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "Software\\Policies\\Microsoft\\Windows Defender\\Policy Manager" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }





                            if (spynet != null)
                            {
                                string[] subkeys5 = spynet.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + spynet.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }




                            if (SignatureUpdateSystem != null)
                            {
                                string[] subkeys4 = SignatureUpdateSystem.GetValueNames();
                                foreach (var name in subkeys4)
                                {
                                    main.windefendlogs.Add("Microsoft Defender: " + name + " in " + SignatureUpdateSystem.Name);
                                }
                                if (subkeys4.Length > 0)
                                {
                                    registrychanges += subkeys4.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Signature Update" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }



                            if (wdrealtimeprotection != null)
                            {
                                string[] subkeys5 = wdrealtimeprotection.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + wdrealtimeprotection.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + " registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }




                            if (windowsdefender != null)
                            {
                                string[] subkeys5 = windowsdefender.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + windowsdefender.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + "  registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SOFTWARE\\Policies\\Microsoft\\Windows Defender" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }

                            if (windowsdefendersystem != null)
                            {
                                string[] subkeys5 = windowsdefendersystem.GetValueNames();

                                if (subkeys5.Length > 0)
                                {
                                    foreach (var name in subkeys5)
                                    {
                                        main.windefendlogs.Add("Microsoft Defender: " + name + " in " + windowsdefendersystem.Name);
                                    }
                                    registrychanges += subkeys5.Length;
                                    registrystatus = "and found " + registrychanges + "  registry changes.";
                                }
                            }
                            else
                            {
                                main.windefendlogs.Add("Microsoft Defender: " + "SYSTEM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender" + " not found.");
                                registrychanges += 1;
                                registrystatus = "and found " + registrychanges + " registry changes.";
                            }


                            if (registrystatus == "")
                            {
                                registrystatus = "and not found registry changes.";
                            }
                            else
                            {
                                main.Dispatcher.InvokeAsync(async () =>
                                {
                                    main.DefenderStatusLabel.Text = servicestatus + " " + registrystatus + " (for more details click > icon and export log file)";

                                });
                            }


                        }
                    }
                    
                   
                } catch (Exception ex)
                {
                    main.Dispatcher.InvokeAsync(() =>
                    {
                        main.DefenderStatusLabel.Text = ex.Message + " " + ex.StackTrace;
                    });
                } 
            
            }
        }
  
        private void WinDefendIconButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderdialog = new OpenFolderDialog();
            var folderDialog = new OpenFolderDialog();
            bool? result = folderDialog.ShowDialog();

            if (result == true) 
            {
                string selectedFolder = folderDialog.FolderName;
                string filepath = selectedFolder + "\\windefendchanges.txt";
                System.IO.File.Create(filepath).Close();
               
                using (StreamWriter writer = new StreamWriter(filepath))
                {
                    foreach (string line in windefendlogs)
                    {
                        writer.WriteLine(line);
                    }
                }
                MessageBox.Show("windefendchanges.txt created in " + filepath, "Multron WinCare", MessageBoxButton.OK, MessageBoxImage.Information);
            }
         
        }
        private void DoNothingAllButton_Click(object sender, RoutedEventArgs e)
        {
            
            var doNothingRadioButtons = new List<RadioButton>
    {
        DoNothingRadio,
        DoNothingVirusThreatRadio,
        DoNothingAccountProtectionRadio,
        DoNothingFirewallRadio,
        DoNothingAppBrowserRadio,
        DoNothingDeviceSecurityRadio,
        DoNothingDeviceHealthRadio,
        DoNothingFamilyRadio,
        DoNothingHistoryRadio,
        DoNothingUIRadio,
        DoNothingMNORadio,
        DoNothingSecurityNotificationsRadio
    }; 
            foreach (var radioButton in doNothingRadioButtons)
            {
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }
        private void EnableAllButton_Click(object sender, RoutedEventArgs e)
        {
        
            var enableRadioButtons = new List<RadioButton>
    {
        EnableDefenderRadio,
        EnableVirusThreatRadio,
        EnableAccountProtectionRadio,
        EnableFirewallRadio,
        EnableAppBrowserRadio,
        EnableDeviceSecurityRadio,
        EnableDeviceHealthRadio,
        EnableFamilyRadio,
        EnableHistoryRadio,
        EnableDefenderUIRadio,
        EnableMNONotificationsRadio,
        EnableSecurityNotificationsRadio
    };
 
            foreach (var radioButton in enableRadioButtons)
            {
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }

        private void DisableAllButton_Click(object sender, RoutedEventArgs e)
        {
            
            var disableRadioButtons = new List<RadioButton>
    {
        DisableDefenderRadio,
        DisableVirusThreatRadio,
        DisableAccountProtectionRadio,
        DisableFirewallRadio,
        DisableAppBrowserRadio,
        DisableDeviceSecurityRadio,
        DisableDeviceHealthRadio,
        DisableFamilyRadio,
        DisableHistoryRadio,
        DisableDefenderUIRadio,
        DisableMNORadio,
        DisableSecurityNotificationsRadio
    };

           
            foreach (var radioButton in disableRadioButtons)
            {
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }
        public void enablesecuritycentertasks()
        {
            string[] tasks = {
        @"\Microsoft\Windows\Windows Defender\Windows Defender Scheduled Scan",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Cache Maintenance",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Cleanup",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Verification"
    };

            foreach (var task in tasks)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = $"/Change /TN \"{task}\" /ENABLE",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
                catch { }
            }
        }
        public void disablesecuritycentertasks()
        {
            string[] tasks = {
        @"\Microsoft\Windows\Windows Defender\Windows Defender Scheduled Scan",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Cache Maintenance",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Cleanup",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Verification"
    };

            foreach (var task in tasks)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = $"/Change /TN \"{task}\" /DISABLE",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
                catch { }
            }
        }
        public async Task enablesecurityandmaintenancenotifications()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Security And Maintenance Notifications enablement process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey smn = null;

                   
                    try { smn = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", true); } catch { }
                    try { smn?.DeleteValue("Enabled", false); } catch { }
                    try { smn?.DeleteValue("ShowInActionCenter", false); } catch { }
                    try { smn?.Close(); } catch { }
                     
                    try { smn = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true); } catch { }
                    try { smn?.DeleteValue("HideSCAHealth", false); } catch { }
                    try { smn?.Close(); } catch { }
                     
                    try { smn = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\Action Center\Checks", true); } catch { }
                    try { smn?.DeleteValue("{C8E6F269-B90A-4053-A3BE-499AFCEC98C4}.check.0", false); } catch { }
                    try { smn?.DeleteValue("{E8433B72-5842-4d43-8645-BC2C35960837}.check.0", false); } catch { }
                    try { smn?.DeleteValue("{E8433B72-5842-4d43-8645-BC2C35960837}.check.100", false); } catch { }
                    try { smn?.DeleteValue("{01979c6a-42fa-414c-b8aa-eee2c8202018}.check.0", false); } catch { }
                    try { smn?.Close(); } catch { }
                     
                    try { smn = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Security Center", true); } catch { }
                    try { smn?.DeleteValue("AntiVirusDisableNotify", false); } catch { }
                    try { smn?.DeleteValue("FirewallDisableNotify", false); } catch { }
                    try { smn?.DeleteValue("UpdatesDisableNotify", false); } catch { }
                    try { smn?.DeleteValue("UacDisableNotify", false); } catch { }
                    try { smn?.DeleteValue("AutoUpdateDisableNotify", false); } catch { }
                    try { smn?.DeleteValue("AntiSpywareDisableNotify", false); } catch { }
                    try { smn?.DeleteValue("InternetSettingsDisableNotify", false); } catch { }
                    try { smn?.Close(); } catch { }

                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablesecurityandmaintenancenotifications()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Security And Maintenance Notifications disablement process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey smn = null;
                     
                    try { smn = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", true); } catch { }
                    try { smn?.SetValue("Enabled", 0, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("ShowInActionCenter", 0, RegistryValueKind.DWord); } catch { }
                    try { smn?.Close(); } catch { }
                     
                    try { smn = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true); } catch { }
                    try { smn?.SetValue("HideSCAHealth", 1, RegistryValueKind.DWord); } catch { }
                    try { smn?.Close(); } catch { }
                     
                    try { smn = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\Action Center\Checks", true); } catch { }
                    try { smn?.SetValue("{C8E6F269-B90A-4053-A3BE-499AFCEC98C4}.check.0", 0, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("{E8433B72-5842-4d43-8645-BC2C35960837}.check.0", 0, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("{E8433B72-5842-4d43-8645-BC2C35960837}.check.100", 0, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("{01979c6a-42fa-414c-b8aa-eee2c8202018}.check.0", 0, RegistryValueKind.DWord); } catch { }
                    try { smn?.Close(); } catch { }
                     
                    try { smn = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Security Center", true); } catch { }
                    try { smn?.SetValue("AntiVirusDisableNotify", 1, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("FirewallDisableNotify", 1, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("UpdatesDisableNotify", 1, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("UacDisableNotify", 1, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("AutoUpdateDisableNotify", 1, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("AntiSpywareDisableNotify", 1, RegistryValueKind.DWord); } catch { }
                    try { smn?.SetValue("InternetSettingsDisableNotify", 1, RegistryValueKind.DWord); } catch { }
                    try { smn?.Close(); } catch { }

                    tried++;
                    await Task.Delay(50);
                }
            });
        }

        public async Task enablewindowssecuritynotifications()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Windows Security Notifications disablement UI process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey wdscnotify = null;

                    RegistryKey mwdscnotify = null;

                    RegistryKey notificationsuppress = null;

                    try { wdscnotify = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications"); } catch { }
                    try { wdscnotify?.DeleteValue("DisableNotifications" ); } catch { }
                    try { wdscnotify?.DeleteValue("DisableEnhancedNotifications" ); } catch { }
                    try { wdscnotify?.Close(); } catch { }

                    try { mwdscnotify = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications"); } catch { }
                    try { mwdscnotify?.DeleteValue("DisableNotifications" ); } catch { }
                    try { mwdscnotify?.DeleteValue("DisableEnhancedNotifications" ); } catch { }
                    try { mwdscnotify?.Close(); } catch { }


                    try { notificationsuppress = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\UX Configuration"); } catch { }
                    try { notificationsuppress?.DeleteValue("Notification_Suppress"); } catch { }
                    try { notificationsuppress?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }
            });





        }
        public async Task disablewindowssecuritynotifications()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Windows Security Notifications disablement UI process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey wdscnotify = null;

                    RegistryKey mwdscnotify = null;

                    RegistryKey notificationsuppress = null;

                    try { wdscnotify = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications", true); } catch { }
                    try { wdscnotify?.SetValue("DisableNotifications", 1, RegistryValueKind.DWord); } catch { }
                    try { wdscnotify?.SetValue("DisableEnhancedNotifications", 1, RegistryValueKind.DWord); } catch { }
                    try { wdscnotify?.Close(); } catch { }

                    try { mwdscnotify = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", true); } catch { }
                    try { mwdscnotify?.SetValue("DisableNotifications", 1, RegistryValueKind.DWord); } catch { }
                    try { mwdscnotify?.SetValue("DisableEnhancedNotifications", 1, RegistryValueKind.DWord); } catch { }
                    try { mwdscnotify?.Close(); } catch { }


                    try { notificationsuppress = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\UX Configuration", true); } catch { }
                    try { notificationsuppress?.SetValue("Notification_Suppress", 1, RegistryValueKind.DWord); } catch { }
                    try { notificationsuppress?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }
            });





        }
        public async Task disablewindowsdefenderui()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender disablement UI process is in progress...";
            await Task.Run(async() => {
                while (tried < remake)
                {
                    RegistryKey securityhealthservice = null;

                    RegistryKey windowsdefenderui = null;
                    try { securityhealthservice = Registry.LocalMachine.CreateSubKey(@"SYSTEM\\CurrentControlSet\\Services\\SecurityHealthService", true); } catch { }
                    try { securityhealthservice?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { securityhealthservice?.Close(); } catch { }



                    try { windowsdefenderui = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Policy Manager", true); } catch { }
                    try { windowsdefenderui?.SetValue("AllowUserUIAccess", 1, RegistryValueKind.DWord); } catch { }
                    try { windowsdefenderui?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }
            });

         

        

        }
        public async Task uilockdowndisable()
        {
           
            RegistryKey uilockdown = null;
            try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Virus and threat protection", true); } catch { }
            try { uilockdown?.SetValue("UILockdown", 1, RegistryValueKind.DWord); } catch(Exception ex) { MessageBox.Show(ex.Message); }
            try { uilockdown?.Close(); } catch { }

        }
        public async Task uilockdownenable()
        {
            RegistryKey uilockdown = null;
            try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Virus and threat protection", true); } catch { }
            try { uilockdown?.DeleteValue("UILockdown"); } catch { }
            try { uilockdown?.Close(); } catch { }

        }
        public async Task enablewindowsdefenderui()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender enablement UI process is in progress...";
            await Task.Run(async() => {
                while(tried < remake)
                {
                    RegistryKey securityhealthservice = null;

                    RegistryKey windowsdefenderui = null;

                    try { securityhealthservice = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\SecurityHealthService", true); } catch { }
                    try { securityhealthservice?.SetValue("Start", 3, RegistryValueKind.DWord); } catch { }
                    try { securityhealthservice?.Close(); } catch { }



                    try { windowsdefenderui = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Policy Manager", true); } catch { }
                    try { windowsdefenderui?.DeleteValue("AllowUserUIAccess"); } catch { }
                    try { windowsdefenderui?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task enablewindowsdefender()
        {
   
            DefenderStatusLabel.Text = "Microsoft Defender enablement process is in progress...";
            await Task.Run(async() =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey windefendservice = null;
                    RegistryKey windowsdefender = null;
                    RegistryKey windowsdefendersystem = null;
                    RegistryKey wdrealtimeprotectionSystem = null;
                    RegistryKey SignatureUpdateSystem = null;
                    RegistryKey spynet = null;
                    RegistryKey wdrealtimeprotection = null;
                    RegistryKey sense_service = null;
                    RegistryKey wd_boot = null;
                    RegistryKey wd_filter = null;
                    RegistryKey wd_nis_drv = null;
                    RegistryKey wd_nis_svc = null;
                    RegistryKey wscsvc = null;
                    RegistryKey softdef = null;
                    RegistryKey micdef = null;
                    RegistryKey shs = null;
                    RegistryKey passive_mod = null;
                    RegistryKey MDCoreSvc = null;
                    RegistryKey defendertelemetry = null;
                    RegistryKey wdFeatures = null;

                    try { wdFeatures = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Features", true); } catch { }
                    try { defendertelemetry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Reporting", true); } catch { }
                    try { MDCoreSvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\MDCoreSvc", true); } catch { }
                    try { micdef = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender", true); } catch { }
                    try { softdef = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }
                    try { wscsvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\wscsvc", true); } catch { }
                    try { windefendservice = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\WinDefend", true); } catch { }
                    try { sense_service = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Sense", true); } catch { }
                   
                    try { wd_boot = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WdBoot", true); } catch { }
                    try { wd_filter = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WdFilter", true); } catch { }
                    try { wd_nis_drv = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WdNisDrv", true); } catch { }
                    try { wd_nis_svc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WdNisSvc", true); } catch { }
                    try { windowsdefender = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }
                    try { windowsdefendersystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }
                    try { wdrealtimeprotectionSystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", true); } catch { }
                    try { SignatureUpdateSystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Signature Update", true); } catch { }
                    try { spynet = Registry.LocalMachine.OpenSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", true); } catch { }
                    try { wdrealtimeprotection = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", true); } catch { }
                    try { wdFeatures?.DeleteValue("MpPlatformKillbitsFromEngine", false); } catch { }
                    try { wdFeatures?.DeleteValue("TamperProtectionSource", false); } catch { }
                    try { wdFeatures?.DeleteValue("MpCapability", false); } catch { }
                    try { wdFeatures?.DeleteValue("TamperProtection", false); } catch { }

                    try { defendertelemetry?.DeleteValue("DisableEnhancedNotifications"); } catch { }
                    try { defendertelemetry?.DeleteValue("DisableGenericReports"); } catch { }
                    try { defendertelemetry?.DeleteValue("DisableGenericRemediation"); } catch { }
                   
                    try { MDCoreSvc?.SetValue("Start", 2, RegistryValueKind.DWord); } catch { }
                    try { MDCoreSvc?.Close(); } catch { }

                    try { wscsvc?.SetValue("Start", 2, RegistryValueKind.DWord); } catch { }
                    try { wscsvc?.Close(); } catch { }
                 
                    try { sense_service?.SetValue("Start", 3, RegistryValueKind.DWord); } catch { }
                    try { sense_service?.Close(); } catch { }

                    try { wd_boot?.SetValue("Start", 0, RegistryValueKind.DWord); } catch { }
                    try { wd_boot?.Close(); } catch { }

                    try { wd_filter?.SetValue("Start", 0, RegistryValueKind.DWord); } catch { }
                    try { wd_filter?.Close(); } catch { }

                    try { wd_nis_drv?.SetValue("Start", 3, RegistryValueKind.DWord); } catch { }
                    try { wd_nis_drv?.Close(); } catch { }

                    try { wd_nis_svc?.SetValue("Start", 3, RegistryValueKind.DWord); } catch { }
                    try { wd_nis_svc?.Close(); } catch { }

                    try { micdef?.SetValue("PassiveMode", 0, RegistryValueKind.DWord); } catch { }
                    try { micdef?.Close(); } catch { }
                    try { micdef?.DeleteValue("DisableAntiSpyware"); } catch { }
                    try { micdef?.DeleteValue("DisableAntiVirus"); } catch { }
                    try { micdef?.Close(); } catch { }
                    try { softdef?.DeleteValue("DisableAntiSpyware"); } catch { }
                    try { softdef?.DeleteValue("DisableAntivirus"); } catch { }
                    try { softdef?.DeleteValue("DisableSpecialRunningModes"); } catch { }
                    try { softdef?.DeleteValue("DisableRoutinelyTakingAction"); } catch { }
                    try { softdef?.DeleteValue("DisableAntiSpywareDefinitionUpdate"); } catch { }
                    try { softdef?.DeleteValue("AllowFastServiceStartup"); } catch { }
                    try { softdef?.DeleteValue("DisableLocalAdminMerge"); } catch { }
                    try { softdef?.DeleteValue("AllowCloudProtection"); } catch { }
                    try { softdef?.DeleteValue("ServiceKeepAlive"); } catch { }
                    try { softdef?.Close(); } catch { }

                    try { windefendservice?.SetValue("Start", 2, RegistryValueKind.DWord); } catch { }
                    try { windefendservice?.Close(); } catch { }

                    try { windowsdefender?.DeleteValue("DisableAntiSpyware"); } catch { }
                    try { windowsdefender?.DeleteValue("DisableAntivirus"); } catch { }
                    try { windowsdefender?.DeleteValue("DisableSpecialRunningModes"); } catch { }
                    try { windowsdefender?.DeleteValue("DisableRoutinelyTakingAction"); } catch { }
                    try { windowsdefender?.DeleteValue("DisableAntiSpywareDefinitionUpdate"); } catch { }
                    try { windowsdefender?.DeleteValue("AllowFastServiceStartup"); } catch { }
                    try { windowsdefender?.DeleteValue("DisableLocalAdminMerge"); } catch { }
                    try { windowsdefender?.DeleteValue("AllowCloudProtection"); } catch { }
                    try { windowsdefender?.DeleteValue("ServiceKeepAlive"); } catch { }
                    try { windowsdefender?.DeleteValue("DontReportInfectionInformation"); } catch { }
                    try { windowsdefender?.Close(); } catch { }

                    try { windowsdefendersystem?.DeleteValue("DisableAntiSpyware"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("DisableAntivirus"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("DisableSpecialRunningModes"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("DisableRoutinelyTakingAction"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("DisableAntiSpywareDefinitionUpdate"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("AllowFastServiceStartup"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("DisableLocalAdminMerge"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("AllowCloudProtection"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("ServiceKeepAlive"); } catch { }
                    try { windowsdefendersystem?.DeleteValue("SubmitSamplesConsent"); } catch { }
                    try { windowsdefendersystem?.Close(); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableRawWriteNotification"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableInformationProtectionControl", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableIOAVProtection", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("LocalSettingOverrideDisableOnAccessProtection", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("LocalSettingOverrideRealtimeScanDirection", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("LocalSettingOverrideDisableIOAVProtection", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("LocalSettingOverrideDisableBehaviorMonitoring", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("LocalSettingOverrideDisableIntrusionPreventionSystem", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("LocalSettingOverrideDisableRealtimeMonitoring", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("RealtimeScanDirection", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableIntrusionPreventionSystem", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("IOAVMaxSize", false); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableAntiSpyware"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableBehaviorMonitoring"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableOnAccessProtection"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableRealtimeMonitoring"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableScanOnRealtimeEnable"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DisableAntiSpywareDefinitionUpdate"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("AllowCloudProtection"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("ServiceKeepAlive"); } catch { }
                    try { wdrealtimeprotectionSystem?.DeleteValue("DpaDisabled"); } catch { }
                    try { wdrealtimeprotectionSystem?.Close(); } catch { }

                    try { wdrealtimeprotection?.DeleteValue("DisableRawWriteNotification"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DisableInformationProtectionControl", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DisableIOAVProtection", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("LocalSettingOverrideDisableOnAccessProtection", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("LocalSettingOverrideRealtimeScanDirection", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("LocalSettingOverrideDisableIOAVProtection", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("LocalSettingOverrideDisableBehaviorMonitoring", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("LocalSettingOverrideDisableIntrusionPreventionSystem", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("LocalSettingOverrideDisableRealtimeMonitoring", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("RealtimeScanDirection", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DisableIntrusionPreventionSystem", false); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("IOAVMaxSize", false); } catch { }

                    try { wdrealtimeprotection?.DeleteValue("DisableAntiSpyware"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DisableBehaviorMonitoring"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DisableOnAccessProtection"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DisableRealtimeMonitoring"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DisableScanOnRealtimeEnable"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DisableAntiSpywareDefinitionUpdate"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("AllowCloudProtection"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("ServiceKeepAlive"); } catch { }
                    try { wdrealtimeprotection?.DeleteValue("DpaDisabled"); } catch { }
                    try { wdrealtimeprotection?.Close(); } catch { }

                    try { SignatureUpdateSystem?.DeleteValue("ForceUpdateFromMU"); } catch { }
                    try { SignatureUpdateSystem?.Close(); } catch { }
                    try { spynet?.DeleteValue("SpyNetReporting"); } catch { }
                    try { spynet?.DeleteValue("SubmitSamplesConsent"); } catch { }
                    try { spynet?.DeleteValue("DisableBlockAtFirstSeen"); } catch { }
                    try { spynet?.Close(); } catch { }

                    try { manageservice("WinDefend", "Automatic", true); } catch { }
                    try
                    {
                        using (Microsoft.Win32.TaskScheduler.TaskService ts = new Microsoft.Win32.TaskScheduler.TaskService())
                        {

                            string defenderFolderPath = @"Microsoft\Windows\Windows Defender";


                            Microsoft.Win32.TaskScheduler.TaskFolder defenderFolder = ts.GetFolder(defenderFolderPath);

                            foreach (Microsoft.Win32.TaskScheduler.Task task in defenderFolder.Tasks)
                            {


                                Microsoft.Win32.TaskScheduler.TaskDefinition def = task.Definition;
                                def.Settings.Enabled = true;

                                defenderFolder.RegisterTaskDefinition(task.Name, def);

                            }


                        }
                    }
                    catch (Exception ex)
                    { }
                    tried++;
                    await Task.Delay(50);
                }
            });




         
        }
        public async Task enablefamilyoptionsarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Enablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Family options", true); } catch { }
                    try { uilockdown?.DeleteValue("UILockdown"); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task enablefirewallandnetworkprotectionarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Enablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Firewall and network protection", true); } catch { }
                    try { uilockdown?.DeleteValue("UILockdown"); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task enablewindowsecuritydevicesecurityarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Enablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Device security", true); } catch { }
                    try { uilockdown?.DeleteValue("UILockdown"); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task enablewindowsecuritydeviceperformanceandhealtharea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Enablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Device performance and health", true); } catch { }
                    try { uilockdown?.DeleteValue("UILockdown"); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task enablewindowsecurityappandbrowserprotectionarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Enablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\App and browser protection", true); } catch { }
                    try { uilockdown?.DeleteValue("UILockdown"); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task enablewindowsecurityaccountprotectionarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Enablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Account protection", true); } catch { }
                    try { uilockdown?.DeleteValue("UILockdown"); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
   
        public async Task enablewindowsecurityhistoryarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Enablement protection history area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Virus and threat protection", true); } catch { }
                    try { uilockdown?.DeleteValue("UILockdown_ProtectionHistory"); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablewindowsecurityhistoryarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Disablement protection history area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Virus and threat protection", true); } catch { }
                    try { uilockdown?.SetValue("UILockdown_ProtectionHistory", 1, RegistryValueKind.DWord); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablefamilyoptionsarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Disablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Family options", true); } catch { }
                    try { uilockdown?.SetValue("UILockdown", 1, RegistryValueKind.DWord); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablefirewallandnetworkprotectionarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Disablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Firewall and network protection", true); } catch { }
                    try { uilockdown?.SetValue("UILockdown", 1, RegistryValueKind.DWord); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablewindowsecuritydevicesecurityarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Disablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Device security", true); } catch { }
                    try { uilockdown?.SetValue("UILockdown", 1, RegistryValueKind.DWord); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablewindowsecuritydeviceperformanceandhealtharea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Disablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Device performance and health", true); } catch { }
                    try { uilockdown?.SetValue("UILockdown", 1, RegistryValueKind.DWord); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablewindowsecurityappandbrowserprotectionarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Disablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;



                    try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\App and browser protection", true); } catch { }
                    try { uilockdown?.SetValue("UILockdown", 1, RegistryValueKind.DWord); } catch { }
                    try { uilockdown?.Close(); } catch { }



                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablewindowsecurityaccountprotectionarea()
        {
            int remake = 12;
            int tried = 0;
            DefenderStatusLabel.Text = "Microsoft Defender Disablement account protection area process is in progress...";
            await Task.Run(async () => {
                while (tried < remake)
                {
                    RegistryKey uilockdown = null;

                

                    try { uilockdown = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender Security Center\Account protection", true); } catch { }
                    try { uilockdown?.SetValue("UILockdown", 1, RegistryValueKind.DWord); } catch { }
                    try { uilockdown?.Close(); } catch { }


 
                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablewindowsdefender()
        {
            DefenderStatusLabel.Text = "Microsoft Defender disablement process is in progress...";
            await Task.Run(async () => {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey windowsdefendersystem = null;
                    RegistryKey wdrealtimeprotection = null;
                    RegistryKey wdrealtimeprotectionSystem = null;
                    RegistryKey SignatureUpdateSystem = null;
                    RegistryKey spynet = null;
                    RegistryKey windowsdefender = null;
                    RegistryKey sense_service = null;
                    RegistryKey wd_boot = null;
                    RegistryKey wd_filter = null;
                    RegistryKey wd_nis_drv = null;
                    RegistryKey wd_nis_svc = null;
                    RegistryKey windefendservice = null;
                    RegistryKey wscsvc = null;
                    RegistryKey softdef = null;
                    RegistryKey micdef = null;
                    RegistryKey shs = null;
                    RegistryKey MDCoreSvc = null;
                    RegistryKey defendertelemetry = null;
                    RegistryKey wdFeatures = null; 
                   

                    try { wdFeatures = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender\Features", true); } catch { }
                    try { defendertelemetry = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Reporting", true); } catch { }
                    try { MDCoreSvc = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\MDCoreSvc", true); } catch { }
                    try { micdef = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows Defender", true); } catch { }
                    try { softdef = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }
                    try { wscsvc = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\wscsvc", true); } catch { }
                    try { windefendservice = Registry.LocalMachine.CreateSubKey(@"System\CurrentControlSet\Services\WinDefend", true); } catch { }
                    try { sense_service = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\Sense", true); } catch { }
                    try { wd_boot = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\WdBoot", true); } catch { }
                    try { wd_filter = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\WdFilter", true); } catch { }
                    try { wd_nis_drv = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\WdNisDrv", true); } catch { }
                    try { wd_nis_svc = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\WdNisSvc", true); } catch { }
                    try { windowsdefendersystem = Registry.LocalMachine.CreateSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }
                    try { wdrealtimeprotection = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", true); } catch { }
                    try { wdrealtimeprotectionSystem = Registry.LocalMachine.CreateSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", true); } catch { }
                    try { SignatureUpdateSystem = Registry.LocalMachine.CreateSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Signature Update", true); } catch { }
                    try { spynet = Registry.LocalMachine.CreateSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", true); } catch { }
                    try { windowsdefender = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }

                    try { wdFeatures?.SetValue("MpPlatformKillbitsFromEngine", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary); } catch { }
                    try { wdFeatures?.SetValue("TamperProtectionSource", 0, RegistryValueKind.DWord); } catch { }
                    try { wdFeatures?.SetValue("MpCapability", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary); } catch { }
                    try { wdFeatures?.SetValue("TamperProtection", 0, RegistryValueKind.DWord); } catch { }

                    try { defendertelemetry?.SetValue("DisableEnhancedNotifications", "1", RegistryValueKind.DWord); } catch { }
                    try { defendertelemetry?.SetValue("DisableGenericReports", "1", RegistryValueKind.DWord); } catch { }
                    try { defendertelemetry?.SetValue("DisableGenericRemediation", "1", RegistryValueKind.DWord); } catch { }

                    try { MDCoreSvc?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { MDCoreSvc?.Close(); } catch { }
                    try { wscsvc?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { wscsvc?.Close(); } catch { }
                    try { sense_service?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { sense_service?.Close(); } catch { }
                   
                    try { wd_boot?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { wd_boot?.Close(); } catch { }

                    try { wd_filter?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { wd_filter?.Close(); } catch { }

                    try { wd_nis_drv?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { wd_nis_drv?.Close(); } catch { }

                    try { wd_nis_svc?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { wd_nis_svc?.Close(); } catch { }
                    try { micdef?.SetValue("PassiveMode", 0, RegistryValueKind.DWord); } catch { }
                    try { micdef?.Close(); } catch { }
                    try { micdef?.SetValue("DisableAntiSpyware", 1, RegistryValueKind.DWord); } catch { }
                    try { micdef?.SetValue("DisableAntiVirus", 1, RegistryValueKind.DWord); } catch { }
                    try { micdef?.Close(); } catch { }
                    try { softdef?.SetValue("DisableAntiSpyware", "1", RegistryValueKind.DWord); } catch { }
                    try { softdef?.SetValue("DisableAntivirus", "1", RegistryValueKind.DWord); } catch { }
                    try { softdef?.SetValue("DisableSpecialRunningModes", "1", RegistryValueKind.DWord); } catch { }
                    try { softdef?.SetValue("DisableRoutinelyTakingAction", "1", RegistryValueKind.DWord); } catch { }
                    try { softdef?.SetValue("DisableAntiSpywareDefinitionUpdate", "1", RegistryValueKind.DWord); } catch { }
                    try { softdef?.SetValue("AllowFastServiceStartup", "0", RegistryValueKind.DWord); } catch { }
                    try { softdef?.SetValue("DisableLocalAdminMerge", "1", RegistryValueKind.DWord); } catch { }
                    try { softdef?.SetValue("AllowCloudProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { softdef?.SetValue("ServiceKeepAlive", "0", RegistryValueKind.DWord); } catch { }
                  
                    try { softdef?.Close(); } catch { }
                    try { windefendservice?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { windefendservice?.Close(); } catch { }


                    try { windowsdefender?.SetValue("DisableAntiSpyware", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("DisableAntivirus", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("DisableSpecialRunningModes", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("DisableRoutinelyTakingAction", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("DisableAntiSpywareDefinitionUpdate", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("AllowFastServiceStartup", "0", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("DisableLocalAdminMerge", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("AllowCloudProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("ServiceKeepAlive", "0", RegistryValueKind.DWord); } catch { }
                    try { windowsdefender?.SetValue("DontReportInfectionInformation", "0", RegistryValueKind.DWord); } catch { } 
             
                    try { windowsdefender?.Close(); } catch { }

                    try { windowsdefendersystem?.SetValue("DisableAntiSpyware", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("DisableAntivirus", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("DisableSpecialRunningModes", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("DisableRoutinelyTakingAction", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("DisableAntiSpywareDefinitionUpdate", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("AllowFastServiceStartup", "0", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("DisableLocalAdminMerge", "1", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("AllowCloudProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("ServiceKeepAlive", "0", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.SetValue("SubmitSamplesConsent", "0", RegistryValueKind.DWord); } catch { }
                    try { windowsdefendersystem?.Close(); } catch { }

                    try { wdrealtimeprotectionSystem?.SetValue("DisableAntiSpyware", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableBehaviorMonitoring", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableOnAccessProtection", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableRealtimeMonitoring", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableScanOnRealtimeEnable", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableAntiSpywareDefinitionUpdate", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableRawWriteNotification", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableInformationProtectionControl", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableIOAVProtection", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("LocalSettingOverrideDisableOnAccessProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("LocalSettingOverrideRealtimeScanDirection", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("LocalSettingOverrideDisableIOAVProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("LocalSettingOverrideDisableBehaviorMonitoring", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("LocalSettingOverrideDisableIntrusionPreventionSystem", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("LocalSettingOverrideDisableRealtimeMonitoring", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("RealtimeScanDirection", "2", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DisableIntrusionPreventionSystem", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("IOAVMaxSize", "00000512", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("AllowCloudProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("ServiceKeepAlive", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.SetValue("DpaDisabled", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotectionSystem?.Close(); } catch { }

                    try { wdrealtimeprotection?.SetValue("DisableAntiSpyware", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableBehaviorMonitoring", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableOnAccessProtection", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableRealtimeMonitoring", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableScanOnRealtimeEnable", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableAntiSpywareDefinitionUpdate", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("AllowCloudProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableRawWriteNotification", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableIOAVProtection", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("IOAVMaxSize", "00000512", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableInformationProtectionControl", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("LocalSettingOverrideDisableOnAccessProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("LocalSettingOverrideRealtimeScanDirection", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("LocalSettingOverrideDisableIOAVProtection", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("LocalSettingOverrideDisableBehaviorMonitoring", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("LocalSettingOverrideDisableIntrusionPreventionSystem", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("LocalSettingOverrideDisableRealtimeMonitoring", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("RealtimeScanDirection", "2", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DisableIntrusionPreventionSystem", "1", RegistryValueKind.DWord); } catch { }

                    try { wdrealtimeprotection?.SetValue("ServiceKeepAlive", "0", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.SetValue("DpaDisabled", "1", RegistryValueKind.DWord); } catch { }
                    try { wdrealtimeprotection?.Close(); } catch { }

                    try { SignatureUpdateSystem?.SetValue("ForceUpdateFromMU", "0", RegistryValueKind.DWord); } catch { }
                    try { SignatureUpdateSystem?.Close(); } catch { }
                    try { spynet?.SetValue("SpyNetReporting", "0", RegistryValueKind.DWord); } catch { }
                    try { spynet?.SetValue("SubmitSamplesConsent", "0", RegistryValueKind.DWord); } catch { }
                    try { spynet?.SetValue("DisableBlockAtFirstSeen", "1", RegistryValueKind.DWord); } catch { }
                    try { spynet?.Close(); } catch { }

                    try { manageservice("WinDefend", "Disabled", false); } catch { }
                    try
                    {
                        using (Microsoft.Win32.TaskScheduler.TaskService ts = new Microsoft.Win32.TaskScheduler.TaskService())
                        {
                           
                            string defenderFolderPath = @"Microsoft\Windows\Windows Defender";

                          
                            Microsoft.Win32.TaskScheduler.TaskFolder defenderFolder = ts.GetFolder(defenderFolderPath);
                             
                            foreach (Microsoft.Win32.TaskScheduler.Task task in defenderFolder.Tasks)
                            {
                            
                                 
                                Microsoft.Win32.TaskScheduler.TaskDefinition def = task.Definition;
                                def.Settings.Enabled = false;
                                 
                                defenderFolder.RegisterTaskDefinition(task.Name, def, Microsoft.Win32.TaskScheduler.TaskCreation.CreateOrUpdate, null, null, Microsoft.Win32.TaskScheduler.TaskLogonType.ServiceAccount);
                            }

              
                        }
                    }
                    catch (Exception ex)
                    {  }
                    tried++;
                    await Task.Delay(50);
                }
            
            });
           
           

         

        }
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }

   
        private async void SelectRecommendedButton_Click(object sender, RoutedEventArgs e)
        {
            var recommendedStatusButtons = FindVisualChildren<ToggleButton>(this)
           .Where(t => t.Tag != null &&
                       t.Tag.ToString().Equals("Recommended", StringComparison.OrdinalIgnoreCase) &&
                       t.Style == (Style)FindResource("StatusCircleStyle"))
           .ToList();

            if (!recommendedStatusButtons.Any())
                return;
 
            var togglePairs = new List<(ToggleButton statusBtn, ToggleButton settingToggle)>();

            foreach (var statusBtn in recommendedStatusButtons)
            {
                 
                string statusBtnName = statusBtn.Name;
                if (statusBtnName.EndsWith("StatusBtn"))
                { 
                    string baseName = statusBtnName.Replace("StatusBtn", "");
                     
                    var grid = FindParent<Grid>(statusBtn);
                    if (grid != null)
                    {
                        var settingToggle = FindVisualChildren<ToggleButton>(grid)
                            .FirstOrDefault(t => t.Style == (Style)FindResource("LocalToggleSwitchStyle"));

                        if (settingToggle != null)
                        {
                            togglePairs.Add((statusBtn, settingToggle));
                        }
                    }
                }
            }

            if (!togglePairs.Any())
                return;
             
            bool allChecked = togglePairs.All(pair => pair.settingToggle.IsChecked == true);
            bool newState = !allChecked;
             
            foreach (var (statusBtn, settingToggle) in togglePairs)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    settingToggle.IsChecked = newState;
                    settingToggle.ApplyTemplate();
                    VisualStateManager.GoToState(settingToggle, newState ? "Checked" : "Unchecked", true);
                }, System.Windows.Threading.DispatcherPriority.Background);
            }

            PrivacyStatusLabel.Text = newState
                ? "All recommended settings have been enabled."
                : "All recommended settings have been disabled.";
            
        }
        private async void PrivacyLogsIconButton_Click(object sender, RoutedEventArgs e)

        {
            OpenFolderDialog folderdialog = new OpenFolderDialog();
            var folderDialog = new OpenFolderDialog();
            bool? result = folderDialog.ShowDialog();

            if (result == true)
            {
                string selectedFolder = folderDialog.FolderName;
                string filepath = selectedFolder + "\\privacy&telemetrychanges.txt";
                System.IO.File.Create(filepath).Close();

                using (StreamWriter writer = new StreamWriter(filepath))
                {
                    foreach (string line in privacylogs)
                    {
                        writer.WriteLine(line);
                    }
                }
                MessageBox.Show("privacy&telemetrychanges.txt created in " + filepath, "Multron WinCare", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void FirewallIconButton_Click(object sender, RoutedEventArgs e)

        {
            OpenFolderDialog folderdialog = new OpenFolderDialog();
            var folderDialog = new OpenFolderDialog();
            bool? result = folderDialog.ShowDialog();

            if (result == true)
            {
                string selectedFolder = folderDialog.FolderName;
                string filepath = selectedFolder + "\\winfirewallchanges.txt";
                System.IO.File.Create(filepath).Close();

                using (StreamWriter writer = new StreamWriter(filepath))
                {
                    foreach (string line in winfirewalllogs)
                    {
                        writer.WriteLine(line);
                    }
                }
                MessageBox.Show("winfirewall.txt created in " + filepath, "Multron WinCare", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void ResetClickFirewall(object sender, RoutedEventArgs e)
       {
            FirewallStatusLabel.Text = "Setting Microsoft Firewall Default settings...";
            ApplyFirewallButton.IsEnabled = false;
            ResetFirewallButton.IsEnabled = false;
            processing2 = 1;
            await Task.Yield();
            await enablefirewallnotifications();
            await enablemfirewall();
            ApplyFirewallButton.IsEnabled = true;
            ResetFirewallButton.IsEnabled = true;
            processing2 = 0;
            FirewallStatusLabel.Text = "Setting Microsoft Firewall To Default settings is done! You need to restart your system for the changes to take effect completely.";
     
        }
        public async Task enablemfirewall()
        {
                    FirewallStatusLabel.Text = "Microsoft Firewall enablement process is in progress...";
                    await Task.Run(async () =>
                    {
                        int remake = 16;
                        int tried = 0;
                        while (tried < remake)
                        {
                            RegistryKey mpssvc = null;
                            RegistryKey BFE = null;
                            RegistryKey DomainProfile = null;
                            RegistryKey DomainProfileLogging = null;
                            RegistryKey StandardProfile = null;
                            RegistryKey StandardProfileLogging = null;
                            RegistryKey PublicProfile = null;
                            RegistryKey PublicProfileLogging = null;
                            RegistryKey PoliciesDomainProfile = null;
                            RegistryKey PoliciesStandardProfile = null;

                            RegistryKey PoliciesPublicProfile = null;
                            RegistryKey SecurityCenter = null;

                            try { mpssvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\mpssvc", true); } catch { }
                            try { BFE = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\BFE", true); } catch { }
                            try { DomainProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", true); } catch { }
                            try { DomainProfileLogging = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile\Logging", true); } catch { }
                            try { StandardProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", true); } catch { }
                            try { StandardProfileLogging = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile\Logging", true); } catch { }
                            try { PublicProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile", true); } catch { }
                            try { PublicProfileLogging = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile\Logging", true); } catch { }

                            try { PoliciesDomainProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\DomainProfile", true); } catch { }
                            try { PoliciesStandardProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\StandardProfile", true); } catch { }
                            try { PoliciesPublicProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\PublicProfile", true); } catch { }
                            try { SecurityCenter = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Security Center"); } catch { }

                            try { SecurityCenter?.DeleteValue("FirewallOverride"); } catch { }

                            try { PoliciesDomainProfile?.DeleteValue("EnableFirewall"); } catch { }

                            try { PoliciesDomainProfile?.Close(); } catch { }

                            try { PoliciesStandardProfile?.DeleteValue("EnableFirewall"); } catch { }

                            try { PoliciesStandardProfile?.Close(); } catch { }

                            try { PoliciesPublicProfile?.DeleteValue("EnableFirewall"); } catch { }

                            try { PoliciesPublicProfile?.Close(); } catch { }

                            try { DomainProfile?.SetValue("EnableFirewall", "1", RegistryValueKind.DWord); } catch { }
                            try { DomainProfile?.DeleteValue("DoNotAllowExceptions"); } catch { }
                            try { DomainProfile?.Close(); } catch { }

                            try { DomainProfileLogging?.SetValue("LogDroppedPackets", "0", RegistryValueKind.DWord); } catch { }
                            try { DomainProfileLogging?.SetValue("LogSuccessfulConnections", "0", RegistryValueKind.DWord); } catch { }
                            try { DomainProfileLogging?.Close(); } catch { }

                            try { StandardProfile?.SetValue("EnableFirewall", "1", RegistryValueKind.DWord); } catch { }
                            try { StandardProfile?.DeleteValue("DoNotAllowExceptions"); } catch { }
                            try { StandardProfile?.Close(); } catch { }

                            try { StandardProfileLogging?.SetValue("LogDroppedPackets", "0", RegistryValueKind.DWord); } catch { }
                            try { StandardProfileLogging?.SetValue("LogSuccessfulConnections", "0", RegistryValueKind.DWord); } catch { }
                            try { StandardProfileLogging?.Close(); } catch { }

                            try { PublicProfile?.SetValue("EnableFirewall", "1", RegistryValueKind.DWord); } catch { }
                            try { PublicProfile?.DeleteValue("DoNotAllowExceptions"); } catch { }
                            try { PublicProfile?.Close(); } catch { }

                            try { PublicProfileLogging?.SetValue("LogDroppedPackets", "0", RegistryValueKind.DWord); } catch { }
                            try { PublicProfileLogging?.SetValue("LogSuccessfulConnections", "0", RegistryValueKind.DWord); } catch { }
                            try { PublicProfileLogging?.Close(); } catch { }

                            try { mpssvc?.SetValue("Start", 2, RegistryValueKind.DWord); } catch { }
                            try { mpssvc?.Close(); } catch { }
                            try { BFE?.SetValue("Start", 2, RegistryValueKind.DWord); } catch { }
                            try { BFE?.Close(); } catch { }
                            tried++;
                            await Task.Delay(50);
                        }


                    });
        }
        public async Task disablemfirewall()
        {
                    FirewallStatusLabel.Text = "Microsoft Firewall disablement process is in progress...";
                    await Task.Run(async () =>
                    {
                        int remake = 16;
                        int tried = 0;
                        while (tried < remake)
                        {
                            RegistryKey mpssvc = null;
                            RegistryKey BFE = null;
                            RegistryKey DomainProfile = null;
                            RegistryKey DomainProfileLogging = null;
                            RegistryKey StandardProfile = null;
                            RegistryKey StandardProfileLogging = null;
                            RegistryKey PublicProfile = null;
                            RegistryKey PublicProfileLogging = null;

                            RegistryKey PoliciesDomainProfile = null;
                            RegistryKey PoliciesStandardProfile = null;
                       
                            RegistryKey PoliciesPublicProfile = null;

                            RegistryKey SecurityCenter = null;

                            try { mpssvc = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\mpssvc", true); } catch { }
                            try { BFE = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\BFE", true); } catch { }
                            try { DomainProfile = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", true); } catch { }
                            try { DomainProfileLogging = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile\Logging", true); } catch { }
                            try { StandardProfile = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", true); } catch { }
                            try { StandardProfileLogging = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile\Logging", true); } catch { }
                            try { PublicProfile = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile", true); } catch { }
                            try { PublicProfileLogging = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile\Logging", true); } catch { }

                            try { PoliciesDomainProfile = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\DomainProfile", true); } catch { }
                            try { PoliciesStandardProfile = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\StandardProfile", true); } catch { }
                            try { PoliciesPublicProfile = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\PublicProfile", true); } catch { }
                            try { SecurityCenter = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Security Center"); } catch { }

                            try { SecurityCenter?.SetValue("FirewallOverride", "1", RegistryValueKind.DWord); } catch { }

                            try { PoliciesDomainProfile?.SetValue("EnableFirewall", "0", RegistryValueKind.DWord); } catch { }
                      
                            try { PoliciesDomainProfile?.Close(); } catch { }
                            
                            try { PoliciesStandardProfile?.SetValue("EnableFirewall", "0", RegistryValueKind.DWord); } catch { }
                       
                            try { PoliciesStandardProfile?.Close(); } catch { }

                            try { PoliciesPublicProfile?.SetValue("EnableFirewall", "0", RegistryValueKind.DWord); } catch { }

                            try { PoliciesPublicProfile?.Close(); } catch { }

                            try { DomainProfile?.SetValue("EnableFirewall", "0", RegistryValueKind.DWord); } catch { }
                            try { DomainProfile?.SetValue("DoNotAllowExceptions", "0", RegistryValueKind.DWord); } catch { }
                            try { DomainProfile?.Close(); } catch { }

                            try { DomainProfileLogging?.SetValue("LogDroppedPackets", "0", RegistryValueKind.DWord); } catch { }
                            try { DomainProfileLogging?.SetValue("LogSuccessfulConnections", "0", RegistryValueKind.DWord); } catch { }
                            try { DomainProfileLogging?.Close(); } catch { }

                            try { StandardProfile?.SetValue("EnableFirewall", "0", RegistryValueKind.DWord); } catch { }
                            try { StandardProfile?.SetValue("DoNotAllowExceptions", "0", RegistryValueKind.DWord); } catch { }
                            try { StandardProfile?.Close(); } catch { }

                            try { StandardProfileLogging?.SetValue("LogDroppedPackets", "0", RegistryValueKind.DWord); } catch { }
                            try { StandardProfileLogging?.SetValue("LogSuccessfulConnections", "0", RegistryValueKind.DWord); } catch { }
                            try { StandardProfileLogging?.Close(); } catch { }

                            try { PublicProfile?.SetValue("EnableFirewall", "0", RegistryValueKind.DWord); } catch { }
                            try { PublicProfile?.SetValue("DoNotAllowExceptions", "0", RegistryValueKind.DWord); } catch { }
                            try { PublicProfile?.Close(); } catch { }

                            try { PublicProfileLogging?.SetValue("LogDroppedPackets", "0", RegistryValueKind.DWord); } catch { }
                            try { PublicProfileLogging?.SetValue("LogSuccessfulConnections", "0", RegistryValueKind.DWord); } catch { }
                            try { PublicProfileLogging?.Close(); } catch { }

                            try { mpssvc?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                            try { mpssvc?.Close(); } catch { }
                            try { BFE?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                            try { BFE?.Close(); } catch { }
                            tried++;
                            await Task.Delay(50);
                        }
                        
                 
                });

        }
        public async Task enablewinupdatenotifications()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update Notifications enablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 3;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey NotificationsUpdate = null;
                    RegistryKey UXSettings = null;
                    try { NotificationsUpdate = Registry.LocalMachine.OpenSubKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.WindowsUpdate.MoNotification2", true); } catch { }
                    try { UXSettings = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", true); } catch { }
                    try { NotificationsUpdate?.DeleteValue("Enabled"); } catch { }
                    try { NotificationsUpdate?.Close(); } catch { }
                    try { UXSettings?.DeleteValue("TrayIconVisibility"); } catch { }
                    try { UXSettings?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task disablewinupdatenotifications()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update Notifications disablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 3;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey NotificationsUpdate = null;
                    RegistryKey UXSettings = null;
                    try { NotificationsUpdate = Registry.LocalMachine.CreateSubKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.WindowsUpdate.MoNotification2", true); } catch { }
                    try { UXSettings = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", true); } catch { }
                    try { NotificationsUpdate?.SetValue("Enabled", 0, RegistryValueKind.DWord); } catch { }
                    try { NotificationsUpdate?.Close(); } catch { }
                    try { UXSettings?.SetValue("TrayIconVisibility", 0, RegistryValueKind.DWord); } catch { }
                    try { UXSettings?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task enableupdatenotifications()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update UI Access enablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 3;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey Notifications2 = null;
                    RegistryKey UX = null;
                    try { Notifications2 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.WindowsUpdate.MoNotification2", true); } catch { }
                    try { UX = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\UX", true); } catch { }
                    try { Notifications2?.DeleteValue("Enabled"); } catch { }
                    try { UX?.DeleteValue("TrayIconVisibility"); } catch { }
                    try { UX?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task disableupdatenotifications()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update UI Access disablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 3;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey Notifications2 = null;
                    RegistryKey UX = null;
                    RegistryKey WUExplorer = null;
                    try { WUExplorer = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true); } catch { }
                    try { Notifications2 = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.WindowsUpdate.MoNotification2", true); } catch { }
                    try { UX = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\UX", true); } catch { }
                    try { Notifications2?.SetValue("Enabled", 0, RegistryValueKind.DWord); } catch { }
                    try { UX?.SetValue("TrayIconVisibility", 0, RegistryValueKind.DWord); } catch { }
                    try { UX?.Close(); } catch { }
                    try { WUExplorer?.DeleteValue("SettingsPageVisibility"); } catch { }
                    try { WUExplorer?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task disablewinupdateuiaccess()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update UI Access disablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 3;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey winupdate = null;
                    RegistryKey UX = null;
                    RegistryKey WUExplorer = null;
                    try { winupdate = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true); } catch { }
                    try { UX = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\UX", true); } catch { }
                    try { WUExplorer = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true); } catch { }
                    try { winupdate?.SetValue("SetDisableUXWUAccess", 1, RegistryValueKind.DWord); ; } catch { }
                    try { winupdate?.SetValue("DisableWindowsUpdateAccess", 1, RegistryValueKind.DWord); } catch { }
                    try { winupdate?.Close(); } catch { }
                    try { UX?.SetValue("HideSettingsPage", 1, RegistryValueKind.DWord); } catch { }
                    try { UX?.Close(); } catch { }
                    try { WUExplorer?.SetValue("SettingsPageVisibility", "hide:windowsupdate;windowsupdate-activehours;windowsupdate-advancedoptions;windowsupdate-optionalupdates;windowsupdate-restartoptions;windowsupdate-seekerupdates", RegistryValueKind.String); } catch { }
                    try { WUExplorer?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task enablewinupdateuiaccess()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update UI Access enablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 3;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey winupdate = null;
                    RegistryKey UX = null;
                    RegistryKey WUExplorer = null;
                    try { winupdate = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true); } catch { }
                    try { UX = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\UX", true); } catch { }
                    try { WUExplorer = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true); }  catch { }
                    try { winupdate?.DeleteValue("SetDisableUXWUAccess");  ; } catch { }
                    try { winupdate?.DeleteValue("DisableWindowsUpdateAccess"); } catch { }
                    try { winupdate?.Close(); } catch { }
                    try { UX?.DeleteValue("HideSettingsPage"); } catch { }
                    try { UX?.Close(); } catch { }
                    try { WUExplorer?.DeleteValue("SettingsPageVisibility"); } catch { }
                    try { WUExplorer?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task disableautomaticupdates()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update Automatic Updates disablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 3;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey automaticupdates = null;
                    try { automaticupdates = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true); } catch { }
                    try { automaticupdates?.SetValue("NoAutoUpdate", 1, RegistryValueKind.DWord); } catch { }
                    try { automaticupdates?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task enableautomaticupdates()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update Automatic Updates enablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 3;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey automaticupdates = null;
                    try { automaticupdates = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true); } catch { }
                    try { automaticupdates?.DeleteValue("NoAutoUpdate"); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task enablefeatureupdates()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update Feature Updates enablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey winupdate = null;
                    RegistryKey uxsettings = null;
                    RegistryKey gwx = null;

                    try
                    {
                        try { winupdate = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true); } catch { }
                        try { uxsettings = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", true); } catch { }
                        try { gwx = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Gwx", true); } catch { }

           
                        try { winupdate?.DeleteValue("DeferFeatureUpdates"); } catch { }
                        try { winupdate?.DeleteValue("DeferFeatureUpdatesPeriodInDays"); } catch { }
                        try { winupdate?.DeleteValue("PauseFeatureUpdatesStartTime"); } catch { }
                        try { winupdate?.DeleteValue("TargetReleaseVersion"); } catch { }
                        try { winupdate?.DeleteValue("TargetReleaseVersionInfo"); } catch { }
                        try { winupdate?.DeleteValue("ProductVersion"); } catch { }
                        try { winupdate?.DeleteValue("BranchReadinessLevel"); } catch { }
                        try { winupdate?.DeleteValue("ExcludeWUDriversInQualityUpdate"); } catch { }

                         

                        try { winupdate?.Close(); } catch { }

                        try { uxsettings?.DeleteValue("DeferFeatureUpdates"); } catch { }
                
                        try { uxsettings?.Close(); } catch { }

                        try { gwx?.DeleteValue("DisableGwx"); } catch { }
                        try { gwx?.Close(); } catch { }
                    }
                    catch { }
                    finally
                    {
                        try { winupdate?.Close(); } catch { }
                        try { uxsettings?.Close(); } catch { }
                        try { gwx?.Close(); } catch { }
                    }

                    tried++;
                    await Task.Delay(50);
                }
            });
        }
        public async Task disablefeatureupdates()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update Feature Updates disablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey winupdate = null;
                    RegistryKey uxsettings = null;
                    RegistryKey gwx = null;

                    try
                    {
                        try { winupdate = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true); } catch { }
                        try { uxsettings = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", true); } catch { }
                        try { gwx = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Gwx", true); } catch { }
                        string productVersion = GetWindowsProductName();
                        string targetVersion = GetWindowsReleaseVersion();

                        
                        try { winupdate?.SetValue("DeferFeatureUpdates", 1, RegistryValueKind.DWord); } catch { }
                        try { winupdate?.SetValue("DeferFeatureUpdatesPeriodInDays", 365, RegistryValueKind.DWord); } catch { }
                    
                        try { winupdate?.SetValue("TargetReleaseVersion", 1, RegistryValueKind.DWord); } catch { }
                        try { winupdate?.SetValue("TargetReleaseVersionInfo", targetVersion, RegistryValueKind.String); } catch { }
                        try { winupdate?.SetValue("ProductVersion", productVersion, RegistryValueKind.String); } catch { }
                        try { winupdate?.SetValue("BranchReadinessLevel", 32, RegistryValueKind.DWord); } catch { }

                        
                        try { winupdate?.SetValue("DeferQualityUpdates", 0, RegistryValueKind.DWord); } catch { }
                     
                        try { winupdate?.SetValue("DeferQualityUpdatesPeriodInDays", 0, RegistryValueKind.DWord); } catch { }

                   
                        try { winupdate?.SetValue("ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord); } catch { }
                        try { winupdate?.Close(); } catch { }

                        try { uxsettings?.SetValue("DeferFeatureUpdates", 1, RegistryValueKind.DWord); } catch { }
                        try { uxsettings?.SetValue("DeferQualityUpdates", 0, RegistryValueKind.DWord); } catch { }
                        try { uxsettings?.Close(); } catch { }

                        try { gwx?.SetValue("DisableGwx", 1, RegistryValueKind.DWord); } catch { }
                        try { gwx?.Close(); } catch { }
                    }
                    catch { }
                    finally
                    {
                        try { winupdate?.Close(); } catch { }
                        try { uxsettings?.Close(); } catch { }
                        try { gwx?.Close(); } catch { }
                    }
                    tried++;
                    await Task.Delay(50);
                }
            });
        }


        private string GetWindowsProductName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string caption = obj["Caption"]?.ToString() ?? "";
                        if (caption.Contains("Windows 11"))
                            return "Windows 11";
                        else
                            return "Windows 10";
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetWindowsReleaseVersion()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", true))
                {
                    if (key != null)
                    {
                        object displayVersion = key.GetValue("DisplayVersion");
                        if (displayVersion != null)
                            return displayVersion.ToString();
                    }
                }
            }
            catch { }
            return "22H2";
        }
        public async Task enabledriverupdates()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update Driver Updates enablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey winupdate = null;
                    RegistryKey driversearching = null;
                    RegistryKey cvdriversearching = null;
                    RegistryKey devicemetadata = null;
                    RegistryKey deviceinstaller = null;
                    try { winupdate = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true); } catch { }
                    try { driversearching = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DriverSearching", true); } catch { }
                    try { cvdriversearching = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", true); } catch { }
                    try { devicemetadata = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Device Metadata", true); } catch { }
                    try { deviceinstaller = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Installer", true); } catch { }

                    try { winupdate?.DeleteValue("ExcludeWUDriversInQualityUpdate"); } catch { }
                    try { winupdate?.Close(); } catch { }

                    try { driversearching?.DeleteValue("DontSearchWindowsUpdate"); } catch { }
                    try { driversearching?.DeleteValue("SearchOrderConfig"); } catch { }
                    try { driversearching?.Close(); } catch { }

                    try { cvdriversearching?.DeleteValue("SearchOrderConfig"); } catch { }
                    try { cvdriversearching?.Close(); } catch { }

                    try { devicemetadata?.DeleteValue("PreventDeviceMetadataFromNetwork"); } catch { }
                    try { devicemetadata?.Close(); } catch { }

                    try { deviceinstaller?.DeleteValue("DisableCoInstallers"); } catch { }
                    try { deviceinstaller?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task disabledriverupdates()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update Driver Updates disablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey winupdate = null;
                    RegistryKey driversearching = null;
                    RegistryKey cvdriversearching = null;
                    RegistryKey devicemetadata = null;
                    RegistryKey deviceinstaller = null;
                    try { winupdate = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true); } catch { }
                    try { driversearching = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DriverSearching", true); } catch { }
                    try { cvdriversearching = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", true); } catch { }
                    try { devicemetadata = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Device Metadata", true); } catch { }
                    try { deviceinstaller = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Installer", true); } catch { }

                    try { winupdate?.SetValue("ExcludeWUDriversInQualityUpdate", "1", RegistryValueKind.DWord); } catch { }
                    try { winupdate?.Close(); } catch { }

                    try { driversearching?.SetValue("DontSearchWindowsUpdate", "1", RegistryValueKind.DWord); } catch { }
                    try { driversearching?.SetValue("SearchOrderConfig", "0", RegistryValueKind.DWord); } catch { }
                    try { driversearching?.Close(); } catch { }

                    try { cvdriversearching?.SetValue("SearchOrderConfig", "0", RegistryValueKind.DWord); } catch { }
                    try { cvdriversearching?.Close(); } catch { }

                    try { devicemetadata?.SetValue("PreventDeviceMetadataFromNetwork", "1", RegistryValueKind.DWord); } catch { }
                    try { devicemetadata?.Close(); } catch { }

                    try { deviceinstaller?.SetValue("DisableCoInstallers", "1", RegistryValueKind.DWord); } catch { }
                    try { deviceinstaller?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task enablewindowsupdate()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update enablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey wuauserv = null;
                    RegistryKey UsoSvc = null;
                    RegistryKey WaaSMedicSvc = null;
                    RegistryKey DeliveryOptimization = null;
                    try { wuauserv = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\wuauserv", true); } catch { }
                    try { UsoSvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\UsoSvc", true); } catch { }
                    try { WaaSMedicSvc = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WaaSMedicSvc", true); } catch { }
                    try { DeliveryOptimization = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", true); } catch { }

                    try { wuauserv?.SetValue("Start", 2, RegistryValueKind.DWord); } catch { }
                    try { wuauserv?.Close(); } catch { }

                    try { UsoSvc?.SetValue("Start", 2, RegistryValueKind.DWord); } catch { }

                    try { UsoSvc?.Close(); } catch { }

                    try { WaaSMedicSvc?.SetValue("Start", 3, RegistryValueKind.DWord); } catch { }

                    try { WaaSMedicSvc?.Close(); } catch { }

                    try { DeliveryOptimization?.DeleteValue("DODownloadMode"); } catch { }
                    try { DeliveryOptimization?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task disablewindowsupdate()
        {
            WindowsUpdateStatusLabel.Text = "Windows Update disablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
                    RegistryKey wuauserv = null;
                    RegistryKey UsoSvc = null;
                    RegistryKey WaaSMedicSvc = null;
                    RegistryKey DeliveryOptimization = null;
                
                    try { wuauserv = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\wuauserv", true); } catch { }
                    try { UsoSvc = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\UsoSvc", true); } catch { }
                    try { WaaSMedicSvc  = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\WaaSMedicSvc", true); } catch { }
                    try { DeliveryOptimization = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", true); } catch { }

                    try { wuauserv?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }
                    try { wuauserv?.Close(); } catch { }

                    try { UsoSvc?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }

                    try { UsoSvc?.Close(); } catch { }

                    try { WaaSMedicSvc?.SetValue("Start", 4, RegistryValueKind.DWord); } catch { }

                    try { WaaSMedicSvc?.Close(); } catch { }

                    try { DeliveryOptimization?.SetValue("DODownloadMode", "0", RegistryValueKind.DWord); } catch { } 
                    try { DeliveryOptimization?.Close(); } catch { }
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        private async void ApplyUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string status = "";
            ApplyUpdateButton.IsEnabled = false;
            ResetUpdateButton.IsEnabled = false;
            processing3 = 1;
            cts3.Cancel();
            await Task.Yield();
            if (EnableWindowsUpdateRadio.IsChecked == true)
            {
                await enablewindowsupdate();
                status += "Windows Update Enable Actions Applied.";
            }
            else if (DisableWindowsUpdateRadio.IsChecked == true)
            {
                await disablewindowsupdate();
                status += "Windows Update Disable Actions Applied.";
            }
            if (EnableWindowsUpdateNotificationsRadio.IsChecked == true)
            {
                await enableupdatenotifications();
                if (status != "")
                {
                    status += " And Windows Update Notifications Enabled.";
                }
                else
                {
                    status += "Windows Update Notifications Enable Actions Applied.";
                }
            }
            else if (DisableWindowsUpdateNotificationsRadio.IsChecked == true)
            {
                await disableupdatenotifications();
                if (status != "")
                {
                    status += " And Windows Update Notifications Disabled.";
                }
                else
                {
                    status += "Windows Update Notifications Disable Actions Applied.";
                }
            }
            if (EnableAutoUpdateRadio.IsChecked == true)
            {
                await enableautomaticupdates();
                if (status != "")
                {
                    status += " And Windows Update Notifications Enabled.";
                }
                else
                {
                    status += "Windows Update Notifications Enable Actions Applied.";
                }
            }
            else if (DisableAutoUpdateRadio.IsChecked == true)
            {
                await disableautomaticupdates();
                if (status != "")
                {
                    status += " And Windows Update Automatic Update Disabled.";
                }
                else
                {
                    status += "Windows Windows Update Automatic Update Disable Actions Applied.";
                }
            }
            if (EnableWindowsUpdateFeatureUpdatesRadio.IsChecked == true)
            {
                await enablefeatureupdates();
                if (status != "")
                {
                    status += " And Windows Update Notifications Enabled.";
                }
                else
                {
                    status += "Windows Update Notifications Enable Actions Applied.";
                }
            }
            else if (DisableWindowsUpdateFeatureUpdatesRadio.IsChecked == true)
            {
                await disablefeatureupdates();
                if (status != "")
                {
                    status += " And Windows Update Feature Update Disabled.";
                }
                else
                {
                    status += "Windows Windows Update Feature Update Disable Actions Applied.";
                }
            }
            if (EnableDriverUpdateRadio.IsChecked == true)
            {
                await enabledriverupdates();
                if (status != "")
                {
                    status += " And Windows Update Driver Updates Enabled.";
                }
                else
                {
                    status += "Windows Update Driver Updates Enable Actions Applied.";
                }
            }
            else if (DisableDriverUpdateRadio.IsChecked == true)
            {
                await disabledriverupdates();
                if (status != "")
                {
                    status += " And Windows Update Driver Updates Update Disabled.";
                }
                else
                {
                    status += "Windows Windows Update Driver Updates Disable Actions Applied.";
                }
            }
            if (EnableWindowsUpdateAreaRadio.IsChecked == true)
            {
                await enablewinupdateuiaccess();
                if (status != "")
                {
                    status += " And Windows Update Area Updates Enabled.";
                }
                else
                {
                    status += "Windows Update Area Enable Actions Applied.";
                }
            }
            else if (DisableWindowsUpdateAreaRadio.IsChecked == true)
            {
                await disablewinupdateuiaccess();
                if (status != "")
                {
                    status += " And Windows Update Area Update Disabled.";
                }
                else
                {
                    status += "Windows Windows Update Area Disable Actions Applied.";
                }
            }
            status += " You need to restart your system for the changes to take effect completely.";
            WindowsUpdateStatusLabel.Text = status;
            processing3 = 0;
            tokenreset(ref cts3, ref token3);
            ApplyUpdateButton.IsEnabled = true;
            ResetUpdateButton.IsEnabled = true;
        }
        public async Task<long> GetInfoOfWindowsUpdateDir()
        {
            UpdateCacheSizeLabel.Text = "Calculating...";
        

            long initialSize = await Task.Run(() => GetDirectorySize(@"C:\Windows\SoftwareDistribution\Download"));
            string initialSizeText = FormatBytes(initialSize);
            UpdateCacheSizeLabel.Text = "Size " + initialSizeText;

            return initialSize;
        }
        private async void RefreshCacheSizeButton_Click(object sender, RoutedEventArgs e)
        {
            await GetInfoOfWindowsUpdateDir();
        }
        private async void ClearUpdateCacheButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
           
                ClearUpdateCacheButton.IsEnabled = false;
                long initialSize = await GetInfoOfWindowsUpdateDir();
                string initialSizeText = FormatBytes(initialSize);
                var result = MessageBox.Show(
                    $"This will clear {initialSizeText} of Windows Update cache files.\n\nWindows Update service will be stopped before cleaning.\n\nDo you want to continue?",
                    "Clear Windows Update Cache",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    UpdateCacheSizeLabel.Text = "Cleaning...";
              
                  
                    await Task.Run(() =>
                    {
                        
                        StopService("wuauserv");
                        StopService("bits");

                       
                        ClearDirectory(@"C:\Windows\SoftwareDistribution\Download");

                      
                        StartService("bits");
                        StartService("wuauserv");
                    });

                    long freedSpace = initialSize;
               

                    UpdateCacheSizeLabel.Text = "Cache cleared: " + FormatBytes(freedSpace);
                }
                else
                {
                    UpdateCacheSizeLabel.Text = $"Cache size: {initialSizeText}";
                }
            }
            catch (Exception ex)
            {
                UpdateCacheSizeLabel.Text = "Failed to clear Windows Update cache: {ex.Message}";
 
            }
            finally
            {
                ClearUpdateCacheButton.IsEnabled = true;
            }
        }

        private long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
                return 0;

            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                return di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
            catch
            {
                return 0;
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "bytes", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        private void ClearDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;

            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                try { file.Delete(); } catch { }
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try { dir.Delete(true); } catch { }
            }
        }

        private void StopService(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch { }
        }

        private void StartService(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch { }
        }

        private async void ResetClickPrivacy(object sender, RoutedEventArgs e)
        {
           
        }

        private async void ResetClickWindowsUpdate(object sender, RoutedEventArgs e)
        {
            WindowsUpdateStatusLabel.Text = "Setting Windows Update To Default settings...";
            ApplyUpdateButton.IsEnabled = false;
            ResetUpdateButton.IsEnabled = false;
            processing3 = 1;
            await Task.Yield();
            await enablewindowsupdate();
            await enableupdatenotifications();
            await enableautomaticupdates();
            await enablefeatureupdates();
            await enabledriverupdates();
            await enablewinupdateuiaccess();
            ApplyUpdateButton.IsEnabled = true;
            ResetUpdateButton.IsEnabled = true;
            processing3 = 0;
            WindowsUpdateStatusLabel.Text = "Setting Windows Update To Default settings is done! You need to restart your system for the changes to take effect completely.";
        }
        private void MenuHome_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Home");
            UpdateMenuSelection("HomeSection");
        }
        private void MenuDefender_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Defender");
            UpdateMenuSelection("Defender");
        }

        private void MenuFirewall_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Firewall");
            UpdateMenuSelection("Firewall");
        }

        private void MenuWindowsUpdate_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("WindowsUpdate");
            UpdateMenuSelection("WindowsUpdate");
        }

        private void MenuPrivacy_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Privacy");
            UpdateMenuSelection("Privacy");
        }

        private void MenuPerformance_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Performance");
            UpdateMenuSelection("Performance");
        }
        private void UpdateMenuSelection(string selectedMenu)
        {
           
            MenuDefender.Style = (Style)FindResource("MenuItemStyle");
            MenuFirewall.Style = (Style)FindResource("MenuItemStyle");
            MenuWindowsUpdate.Style = (Style)FindResource("MenuItemStyle");
            MenuPrivacy.Style = (Style)FindResource("MenuItemStyle");
            MenuPerformance.Style = (Style)FindResource("MenuItemStyle");
            MenuHome.Style = (Style)FindResource("MenuItemStyle");
            switch (selectedMenu)
            {
                case "Defender":
                    MenuDefender.Style = (Style)FindResource("MenuItemSelectedStyle");
                    break;
                case "Firewall":
                    MenuFirewall.Style = (Style)FindResource("MenuItemSelectedStyle");
                    break;
                case "WindowsUpdate":
                    MenuWindowsUpdate.Style = (Style)FindResource("MenuItemSelectedStyle");
                    break;
                case "Privacy":
                    MenuPrivacy.Style = (Style)FindResource("MenuItemSelectedStyle");
                    break;
                case "Performance":
                    MenuPerformance.Style = (Style)FindResource("MenuItemSelectedStyle");
                    break;
                case "HomeSection":
                    MenuHome.Style = (Style)FindResource("MenuItemSelectedStyle");
                    break;
            }
        }
        private void ShowSection(string sectionName)
        {
        
            DefenderSection.Visibility = Visibility.Collapsed;
            FirewallSection.Visibility = Visibility.Collapsed;
            WindowsUpdateSection.Visibility = Visibility.Collapsed;
            PrivacySection.Visibility = Visibility.Collapsed;
            PerformanceSection.Visibility = Visibility.Collapsed;
            HomeSection.Visibility = Visibility.Collapsed;
            switch (sectionName)
            {
                case "Defender":
                    DefenderSection.Visibility = Visibility.Visible;
                    break;
                case "Firewall":
                    FirewallSection.Visibility = Visibility.Visible;
                    break;
                case "WindowsUpdate":
                    WindowsUpdateSection.Visibility = Visibility.Visible;
                    break;
                case "Privacy":
                    PrivacySection.Visibility = Visibility.Visible;
                    break;
                case "Performance":
                    PerformanceSection.Visibility = Visibility.Visible;
                    break;
                case "Home":
                    HomeSection.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox searchBox) return;

            string searchText = searchBox.Text.ToLower().Trim();

            UpdatePlaceholderVisibility(searchBox);

             
            var elementsToProcess = new List<DependencyObject>();

           
            foreach (var child in PrivacySection.Children)
            {
            
                if (child is Border || child is TextBlock)
                {
                    elementsToProcess.Add(child as DependencyObject);
                } 
                else if (child is Panel panel)
                {
                    
                    foreach (var panelChild in panel.Children)
                    {
                        if (panelChild is Border)
                        {
                            elementsToProcess.Add(panelChild as DependencyObject);
                        }
                    }
                }
            }

         
            foreach (var element in elementsToProcess)
            {
               
                if (element is Border border && border.Child is Grid grid)
                {
                    if (border.Name == "ApplySettingsAndSystemRestoreTextBlock")
                    {
                        continue;
                    }
                    if (border.Name == "ApplySettingsAndSystemRestore")
                    {
                        continue;
                    }

                    if (border.Name == "SearchBoxContainer")
                    {
                        continue;
                    }

                    var textBlocks = GetAllTextBlocks(grid).ToList();
 
                    string allText = string.Join(" ",
                        textBlocks.Select(tb => tb.Text).Where(t => t != null).Select(t => t.ToLower())
                    );

                 
                    bool isMatch;

                    if (string.IsNullOrEmpty(searchText))
                    {
                        isMatch = true; 
                    }
                    else
                    {
                        isMatch = allText.Contains(searchText);  
                    }

                    border.Visibility = isMatch ? Visibility.Visible : Visibility.Collapsed;
                }

              
                else if (element is TextBlock titleBlock)
                {
                   
                    titleBlock.Visibility = Visibility.Visible;
                }
            }
        }
        public static IEnumerable<TextBlock> GetAllTextBlocks(DependencyObject parent)
        {
            if (parent == null) yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child == null) continue;

                if (child is TextBlock textBlock)
                {
                    yield return textBlock;
                }

                foreach (var subChild in GetAllTextBlocks(child))
                {
                    yield return subChild;
                }
            }
        }

        private IEnumerable<Border> GetAllBorders(DependencyObject parent)
            {
                if (parent == null) yield break;

                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    if (child is Border border)
                        yield return border;

                    foreach (var subBorder in GetAllBorders(child))
                        yield return subBorder;
                }
            }
            private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
            {
                if (parent == null) yield break;

                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    if (child is T t)
                        yield return t;

                    foreach (var descendant in FindVisualChildren<T>(child))
                        yield return descendant;
                }
            }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
     
            var allToggles = FindVisualChildren<ToggleButton>(PrivacySection).Where(t => t.Name != "TelemetryToggle" && t.Name != "CopilotPersonalizationToggle")  .ToList();

           
            bool shouldBeChecked = allToggles.Any(t => t.IsChecked == false);

            foreach (var toggle in allToggles)
            {
                toggle.IsChecked = shouldBeChecked;
            }

            PrivacyStatusLabel.Text = shouldBeChecked ? "All settings enabled." : "All settings disabled.";
        }
        private void setstatuscolor(string color)
        {
            if (!color.StartsWith("#")) color = "#" + color;
            if (color.Length == 7) color = "#FF" + color.Substring(1); 

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => setstatuscolor(color));
                return;
            }

            StatusEllipse.Fill = (Brush)new BrushConverter().ConvertFromString(color);
        }
        private void CreateRestorePointButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrivacyStatusLabel.Text = "Opening Windows Restore Point dialog...";
                setstatuscolor("#FF1F6AA5");

                Process.Start(new ProcessStartInfo
                {
                    FileName = "SystemPropertiesProtection.exe",
                    UseShellExecute = true,
                    Verb = "runas"  
                });

                PrivacyStatusLabel.Text = "Windows Restore Point window opened.";
                setstatuscolor("#FF1FA55F");
            }
            catch (Exception ex)
            {
                PrivacyStatusLabel.Text = $"Error: {ex.Message}";
                setstatuscolor("#FFB22222");
            }
        }



        private bool AnyPrivacyToggleSelected()
        {
            if (PrivacySection == null) return false;

            
            var toggles = FindVisualChildren<ToggleButton>(PrivacySection);
             
            var filteredToggles = toggles.Where(t => t.Name != "ApplyOnlySelectedToggle");
             
            return filteredToggles.Any(t => t.IsChecked == true);
        }
        public async Task settingsapply()
        {
       
            if (!AnyPrivacyToggleSelected() && ApplyOnlySelectedToggle.IsChecked == true)
            {
                PrivacyStatusLabel.Text = "Nothing selected.";
                setstatuscolor("#FF1F6AA5");
                return;
            }
            RegistryKey localmachinesystemNCS = null;
            try { localmachinesystemNCS = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator", true); } catch { }

            RegistryKey localmachinesystemInternet = null;
            try { localmachinesystemInternet = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\NlaSvc\Parameters\Internet", true); } catch { }

            RegistryKey localmachinewindowsRA = null;
            try { localmachinewindowsRA = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Remote Assistance", true); } catch { }

            RegistryKey localmachineterminalServer = null;
            try { localmachineterminalServer = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server", true); } catch { }

            RegistryKey localmachineterminalServices = null;
            try { localmachineterminalServices = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", true); } catch { }

            RegistryKey localmachinesoftwareprotection = null;
            try { localmachinesoftwareprotection = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\CurrentVersion\Software Protection Platform", true); } catch { }

            RegistryKey localmachineFeedback = null;
            try { localmachineFeedback = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Feedback", true); } catch { }

            RegistryKey WindowsDefender = null;
            try { WindowsDefender = Registry.LocalMachine.CreateSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }

            RegistryKey spynet = null;
            try { spynet = Registry.LocalMachine.CreateSubKey(@"SYSTEM\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", true); } catch { }

            RegistryKey Driver = null;
            try { Driver = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", true); } catch { }

            RegistryKey cloudContent = null;
            try { cloudContent = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent", true); } catch { }

            RegistryKey passwordReveal = null;
            try { passwordReveal = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CredUI", true); } catch { }

            RegistryKey localmachineapprivacy = null;
            try { localmachineapprivacy = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", true); } catch { }

            RegistryKey localmachineaccountinfo = null;
            try { localmachineaccountinfo = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", true); } catch { }

            RegistryKey locationandsensors = null;
            try { locationandsensors = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", true); } catch { }

            RegistryKey ConsentStorelocation = null;
            try { ConsentStorelocation = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", true); } catch { }

            RegistryKey Autororation = null;
            try { Autororation = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AutoRotation", true); } catch { }

            RegistryKey SensorService = null;
            try { SensorService = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\SensorService", true); } catch { }

            RegistryKey localmachineSystem = null;
            try { localmachineSystem = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System", true); } catch { }

            RegistryKey localmachinehandwritingsharing = null;
            try { localmachinehandwritingsharing = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\TabletPC", true); } catch { }

            RegistryKey localmachineCDPUserService = null;
            try { localmachineCDPUserService = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CDPUserSvc", true); } catch { }

            RegistryKey localmachineAppCompat = null;
            try { localmachineAppCompat = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppCompat", true); } catch { }

            RegistryKey localmachinehandwritingerrorreports = null;
            try { localmachinehandwritingerrorreports = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", true); } catch { }

            RegistryKey localmachineDisableAdvertisingID = null;
            try { localmachineDisableAdvertisingID = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", true); } catch { }

            RegistryKey localmachinedisablecameraonlogon = null;
            try { localmachinedisablecameraonlogon = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Personalization", true); } catch { }

            RegistryKey localmachinesystemdisablecameraonlogon = null;
            try { localmachinesystemdisablecameraonlogon = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true); } catch { }

            RegistryKey localmachinedisablemessagebackup = null;
            try { localmachinedisablemessagebackup = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Messaging", true); } catch { }

            RegistryKey localmachinedisablemessagebackuptomicrosoftaccount = null;
            try { localmachinedisablemessagebackuptomicrosoftaccount = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\SettingSync", true); } catch { }

            RegistryKey currentuserdisablesmsmmscloudsync = null;
            try { currentuserdisablesmsmmscloudsync = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"SOFTWARE\Policies\Microsoft\Windows\Messaging", true); } catch { }

            RegistryKey disablegeneralsyncsettingsformessages = null;
            try { disablegeneralsyncsettingsformessages = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"SOFTWARE\Policies\Microsoft\Windows\Messaging", true); } catch { }

            RegistryKey localmachinewindowserrorreportingservice = null;
            try { localmachinewindowserrorreportingservice = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\WerSvc", true); } catch { }

            RegistryKey currentuserwindowserrorreporting = null;
            try { currentuserwindowserrorreporting = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", true); } catch { }

            RegistryKey localmachinebiometricslogon = null;
            try { localmachinebiometricslogon = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true); } catch { }

            RegistryKey localmachinebiometricsforremotedesktop = null;
            try { localmachinebiometricsforremotedesktop = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Biometrics", true); } catch { }

            RegistryKey localmachinedisablebiometricservice = null;
            try { localmachinedisablebiometricservice = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\WbioSrvc\Start", true); } catch { }

            RegistryKey localmachinedisablefingerprintrecognation = null;
            try { localmachinedisablefingerprintrecognation = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider", true); } catch { }

            RegistryKey localmachinedisablefacialrecognition = null;
            try { localmachinedisablefacialrecognition = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Biometrics\FacialFeatures", true); } catch { }

            RegistryKey localmachinedisablewindowshello = null;
            try { localmachinedisablewindowshello = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\PassportForWork", true); } catch { }

            RegistryKey localmachinedisablebiometricfeatures = null;
            try { localmachinedisablebiometricfeatures = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Biometrics", true); } catch { }

        

            RegistryKey localmachineMicrosoftCEIPscheduled = null;
            try { localmachineMicrosoftCEIPscheduled = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", true); } catch { }

            RegistryKey currentuserDisableOfficeCeip = null;
            try { currentuserDisableOfficeCeip = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Policies\Microsoft\Office\Common\QMEnable", true); } catch { }

            RegistryKey currentuserDisableMicrosoftCeip = null;
            try { currentuserDisableMicrosoftCeip = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\SQMClient", true); } catch { }

            RegistryKey localmachineApplicationExperienceProgram = null;
            try { localmachineApplicationExperienceProgram = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\AppV\CEIP", true); } catch { }

            RegistryKey localmachineceip = null;
            try { localmachineceip = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\SQMClient\Windows", true); } catch { }

            RegistryKey localmachineEdge = null;
            try { localmachineEdge = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Edge", true); } catch { }

            RegistryKey localmachineAppAccessUserAccountInfo = null;
            try { localmachineAppAccessUserAccountInfo = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", true); } catch { }

            RegistryKey localmachineAppAccessLocation = null;
            try { localmachineAppAccessLocation = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", true); } catch { }

            RegistryKey localmachinePasswordReveal = null;
            try { localmachinePasswordReveal = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CredUI", true); } catch { }

            RegistryKey localmachineStepsRecorder = null;
            try { localmachineStepsRecorder = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppCompat", true); } catch { }

            RegistryKey localmachineDataCollection = null;
            try { localmachineDataCollection = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", true); } catch { }

            RegistryKey localmachineDeliveryOptimization = null;
            try { localmachineDeliveryOptimization = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", true); } catch { }

            RegistryKey localmachineSpeech = null;
            try { localmachineSpeech = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Speech", true); } catch { }

            RegistryKey localmachineDefender = null;
            try { localmachineDefender = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }

            RegistryKey localMachineDefenderSpynet = null;
            try { localMachineDefenderSpynet = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", true); } catch { }

            RegistryKey localmachineDefenderReporting = null;
            try { localmachineDefenderReporting = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender", true); } catch { }

            RegistryKey localmachineMobileDevices = null;
            try { localmachineMobileDevices = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Portable Devices", true); } catch { }

            RegistryKey localmachineWMDC = null;
            try { localmachineWMDC = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows CE Services", true); } catch { }

            RegistryKey localmachineExplorer = null;
            try { localmachineExplorer = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true); } catch { }

            RegistryKey localmachineOneDrive = null;
            try { localmachineOneDrive = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\OneDrive", true); } catch { }

            RegistryKey localmachineWindowsUpdateUX = null;
            try { localmachineWindowsUpdateUX = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\UX", true); } catch { }

            RegistryKey localmachineWindowsUpdate = null;
            try { localmachineWindowsUpdate = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true); } catch { }

            RegistryKey localmachineDataCollectionPersonalization = null;
            try { localmachineDataCollectionPersonalization = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection\Personalization", true); } catch { }

            RegistryKey localmachineDataCollectionDiagnosticLogs = null;
            try { localmachineDataCollectionDiagnosticLogs = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection\DiagnosticLogs", true); } catch { }
            RegistryKey bthLEEnumParams = null;
            try { bthLEEnumParams = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\BthLEEnum\Parameters", true); } catch { }
            RegistryKey bthA2dpParams = null;
            try { bthA2dpParams = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\BthA2dp\Parameters", true); } catch { }
            RegistryKey bthHFEnumParams = null;
            try { bthHFEnumParams = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\BthHFEnum\Parameters", true); } catch { }
            RegistryKey bthEnumParams = null;
            try { bthEnumParams = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\BthEnum\Parameters", true); } catch { }
            RegistryKey bthAvrcpTgParams = null;
            try { bthAvrcpTgParams = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\BthAvrcpTg\Parameters", true); } catch { }
            RegistryKey bthModemParams = null;
            try { bthModemParams = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\BTHMODEM\Parameters", true); } catch { }
            RegistryKey bluetoothToastNotifications = null;
            try { bluetoothToastNotifications = Registry.Users.OpenSubKey(UserHelper.GetActiveUserSID() + @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.BluetoothDevice", true); } catch { }
            RegistryKey deviceMetadata = null;
            try { deviceMetadata = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata", true); } catch { }


            RegistryKey localmachinePhoneLink = null;
            try { localmachinePhoneLink = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\PhoneLink", true); } catch { }

            RegistryKey localmachinewindowsMaps = null;
            try { localmachinewindowsMaps = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Maps", true); } catch { }

            RegistryKey localmachineFeeds = null;
            try { localmachineFeeds = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", true); } catch { }

            RegistryKey localmachineCopilot = null;
            try { localmachineCopilot = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", true); } catch { }

            RegistryKey localmachineCopilotRecall = null;
            try { localmachineCopilotRecall = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", true); } catch { }

            RegistryKey localmachinePaint = null;
            try { localmachinePaint = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", true); } catch { }
            RegistryKey currentuseraccountinfo = null;
            try { currentuseraccountinfo = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", true); } catch { }

            RegistryKey currentuserExplorerAdvanced = null;
            try { currentuserExplorerAdvanced = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true); } catch { }

            RegistryKey currentuserADC = null;
            try { currentuserADC = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", true); } catch { }

            RegistryKey currentuserContentDelivery = null;
            try { currentuserContentDelivery = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", true); } catch { }

            RegistryKey currentuserFeeds = null;
            try { currentuserFeeds = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\Feeds", true); } catch { }

            RegistryKey currentuserExplorer = null;
            try { currentuserExplorer = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true); } catch { }

            RegistryKey currentuserMobility = null;
            try { currentuserMobility = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\Mobility", true); } catch { }

            RegistryKey currentuserAppAccessUserAccountInfo = null;
            try { currentuserAppAccessUserAccountInfo = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", true); } catch { }

            RegistryKey currentuserAppAccessDiagnostics = null;
            try { currentuserAppAccessDiagnostics = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics", true); } catch { }

            RegistryKey currentuserAppAccessLocation = null;
            try { currentuserAppAccessLocation = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", true); } catch { }

            RegistryKey currentUserAppAccessUserAccountInfo = null;
            try { currentUserAppAccessUserAccountInfo = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", true); } catch { }

            RegistryKey currentuserClipboard = null;
            try { currentuserClipboard = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Clipboard", true); } catch { }

            RegistryKey currentuserDisableAdvertisingID = null;
            try { currentuserDisableAdvertisingID = Registry.Users.CreateSubKey(UserHelper.GetActiveUserSID() + @"\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", true); } catch { }

            RegistryKey localmachineApplicationCustomerExperienceProgram = null;
            try { localmachineApplicationCustomerExperienceProgram = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\SQMClient\Windows", true); } catch { }

            RegistryKey localmachineCEIPforinternetexplorer = null;
            try { localmachineCEIPforinternetexplorer = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Internet Explorer\SQM", true); } catch { }

            int tried = 0;
            processing4 = 1;
            cts4.Cancel();
            while (tried < 5)
                {
               
                await Task.Delay(500);
                Dispatcher.Invoke(() => setstatuscolor("#FFB0B0B0"));
            
         

                if (DisableHandwritingDataSharing.IsChecked == true)
                {
                    await ApplyValue(1, localmachinehandwritingsharing, "RestrictImplicitInkCollection", RegistryValueKind.DWord);
                    await ApplyValue(1, localmachinehandwritingsharing, "RestrictImplicitTextCollection", RegistryValueKind.DWord);
                    await ApplyValue(1, localmachinehandwritingsharing, "PreventHandwritingDataSharing", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachinehandwritingsharing, "RestrictImplicitInkCollection", RegistryValueKind.DWord);
                        await ApplyValue(0, localmachinehandwritingsharing, "RestrictImplicitTextCollection", RegistryValueKind.DWord);
                        await ApplyValue(0, localmachinehandwritingsharing, "PreventHandwritingDataSharing", RegistryValueKind.DWord);
                    }
                }

                if (DisableHandwritingErrorReports.IsChecked == true)
                {
                    await ApplyValue(1, localmachinehandwritingerrorreports, "PreventHandwritingErrorReports", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachinehandwritingerrorreports, "PreventHandwritingErrorReports", RegistryValueKind.DWord);
                    }
                }

                if (DisableInventoryCollector.IsChecked == true)
                {
                    await ApplyValue(1, localmachineAppCompat, "DisableInventory", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineAppCompat, "DisableInventory", RegistryValueKind.DWord);
                    }
                }

                if (DisableAdvertisingID.IsChecked == true)
                {
                    await ApplyValue(1, localmachineDisableAdvertisingID, "DisabledByGroupPolicy", RegistryValueKind.DWord);
                    await ApplyValue(0, localmachineDisableAdvertisingID, "Enabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineDisableAdvertisingID, "DisabledByGroupPolicy", RegistryValueKind.DWord);
                        await ApplyValue(1, localmachineDisableAdvertisingID, "Enabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableCameraLogon.IsChecked == true)
                {
                    await ApplyValue(1, localmachinedisablecameraonlogon, "NoLockScreenCamera", RegistryValueKind.DWord);
                    await ApplyValue(1, localmachinesystemdisablecameraonlogon, "NoLockScreenCamera", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachinedisablecameraonlogon, "NoLockScreenCamera", RegistryValueKind.DWord);
                        await ApplyValue(0, localmachinesystemdisablecameraonlogon, "NoLockScreenCamera", RegistryValueKind.DWord);
                    }
                }

                if (DisableBackupTextMessage.IsChecked == true)
                {
                    await ApplyValue(0, localmachinedisablemessagebackup, "AllowMessageSync", RegistryValueKind.DWord);
                    await ApplyValue(1, localmachinedisablemessagebackuptomicrosoftaccount, "DisableMessagingSync", RegistryValueKind.DWord);
                    await ApplyValue(0, currentuserdisablesmsmmscloudsync, "CloudServiceSyncEnabled", RegistryValueKind.DWord);
                    await ApplyValue(0, disablegeneralsyncsettingsformessages, "Enabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachinedisablemessagebackup, "AllowMessageSync", RegistryValueKind.DWord);
                        await ApplyValue(0, localmachinedisablemessagebackuptomicrosoftaccount, "DisableMessagingSync", RegistryValueKind.DWord);
                        await ApplyValue(1, currentuserdisablesmsmmscloudsync, "CloudServiceSyncEnabled", RegistryValueKind.DWord);
                        await ApplyValue(1, disablegeneralsyncsettingsformessages, "Enabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableWindowsErrorReporting.IsChecked == true)
                {
                    await ApplyValue(4, localmachinewindowserrorreportingservice, "Start", RegistryValueKind.DWord);
                    await ApplyValue(1, currentuserwindowserrorreporting, "Disabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(3, localmachinewindowserrorreportingservice, "Start", RegistryValueKind.DWord);
                        await ApplyValue(0, currentuserwindowserrorreporting, "Disabled", RegistryValueKind.DWord);
                    }
                }

            

                if (DisableBluetooth.IsChecked == true)
                {
                    await ApplyValue(1, bthLEEnumParams, "DisableAdvertising", RegistryValueKind.DWord);
                    await ApplyValue(1, bthA2dpParams, "DisableAdvertising", RegistryValueKind.DWord);
                    await ApplyValue(1, bthHFEnumParams, "DisableAdvertising", RegistryValueKind.DWord);
                    await ApplyValue(1, bthEnumParams, "DisableAdvertising", RegistryValueKind.DWord);
                    await ApplyValue(1, bthAvrcpTgParams, "DisableAdvertising", RegistryValueKind.DWord);
                    await ApplyValue(1, bthModemParams, "DisableAdvertising", RegistryValueKind.DWord);
                    await ApplyValue(0, bluetoothToastNotifications, "Enabled", RegistryValueKind.DWord);
                    await ApplyValue(1, deviceMetadata, "PreventDeviceMetadataFromNetwork", RegistryValueKind.DWord);

                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, bthLEEnumParams, "DisableAdvertising", RegistryValueKind.DWord);
                        await ApplyValue(0, bthA2dpParams, "DisableAdvertising", RegistryValueKind.DWord);
                        await ApplyValue(0, bthHFEnumParams, "DisableAdvertising", RegistryValueKind.DWord);
                        await ApplyValue(0, bthEnumParams, "DisableAdvertising", RegistryValueKind.DWord);
                        await ApplyValue(0, bthAvrcpTgParams, "DisableAdvertising", RegistryValueKind.DWord);
                        await ApplyValue(0, bthModemParams, "DisableAdvertising", RegistryValueKind.DWord);
                        await ApplyValue(1, bluetoothToastNotifications, "Enabled", RegistryValueKind.DWord);
                        await ApplyValue(0, deviceMetadata, "PreventDeviceMetadataFromNetwork", RegistryValueKind.DWord);
                    }
                }

                if (DisableCEIP.IsChecked == true)
                {
                    await ApplyValue(0, localmachineMicrosoftCEIPscheduled, "AllowTelemetry", RegistryValueKind.DWord);
                    await ApplyValue(0, currentuserDisableOfficeCeip, "CEIPEnable", RegistryValueKind.DWord);
                    await ApplyValue(0, currentuserDisableMicrosoftCeip, "CEIPEnable", RegistryValueKind.DWord);
                    await ApplyValue(1, localmachineApplicationExperienceProgram, "DisableCustomerImprovementProgram", RegistryValueKind.DWord);
                    await ApplyValue(0, localmachineceip, "CEIPEnable", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineMicrosoftCEIPscheduled, "AllowTelemetry", RegistryValueKind.DWord);
                        await ApplyValue(1, currentuserDisableOfficeCeip, "CEIPEnable", RegistryValueKind.DWord);
                        await ApplyValue(1, currentuserDisableMicrosoftCeip, "CEIPEnable", RegistryValueKind.DWord);
                        await ApplyValue(0, localmachineApplicationExperienceProgram, "DisableCustomerImprovementProgram", RegistryValueKind.DWord);
                        await ApplyValue(1, localmachineceip, "CEIPEnable", RegistryValueKind.DWord);
                    }
                }

                if (DisableSmartScreenFilter.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "SmartScreenPuaEnabled", RegistryValueKind.DWord);
                    await ApplyValue(0, localmachineEdge, "SmartScreenEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "SmartScreenPuaEnabled", RegistryValueKind.DWord);
                        await ApplyValue(1, localmachineEdge, "SmartScreenEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableTypesquattingChecker.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "TyposquattingCheckerEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "TyposquattingCheckerEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableIEtoEdgeRedirect.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "RedirectSitesFromInternetExplorerRedirectMode", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "RedirectSitesFromInternetExplorerRedirectMode", RegistryValueKind.DWord);
                    }
                }

                if (DisableSiteSafetyServices.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "SafeBrowsingEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "SafeBrowsingEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableSavingPasswords.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "PasswordManagerEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "PasswordManagerEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisablePagePreload.IsChecked == true)
                {
                    await ApplyValue(2, localmachineEdge, "NetworkPredictionOptions", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineEdge, "NetworkPredictionOptions", RegistryValueKind.DWord);
                    }
                }

                if (DisableWebServiceResolveErrors.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "ResolveNavigationErrorsUseWebService", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "ResolveNavigationErrorsUseWebService", RegistryValueKind.DWord);
                    }
                }

                if (DisableEnhancedSpellCheck.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "SpellcheckEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "SpellcheckEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableEdgeSidebar.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "HubsSidebarEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "HubsSidebarEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableShoppingAssistant.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "EdgeShoppingAssistantEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "EdgeShoppingAssistantEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableSearchWebsiteSuggestions.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "SearchSuggestEnabled", RegistryValueKind.DWord);
                    await ApplyValue(0, localmachineEdge, "addressBarMicrosoftSearchInBingProviderEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "SearchSuggestEnabled", RegistryValueKind.DWord);
                        await ApplyValue(1, localmachineEdge, "addressBarMicrosoftSearchInBingProviderEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableLocalProviderSuggestions.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "LocalProvidersEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "LocalProvidersEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableFormSuggestions.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "AutofillEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "AutofillEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableAutoCompleteAddresses.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "AutofillAddressEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "AutofillAddressEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableSimilarSiteSuggestions.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "AlternateErrorPagesEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "AlternateErrorPagesEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableToolbarFeedback.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "UserFeedbackAllowed", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "UserFeedbackAllowed", RegistryValueKind.DWord);
                    }
                }

                if (DisablePersonalizedAdsSearch.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "PersonalizationReportingEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "PersonalizationReportingEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableCreditCardAutofill.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "AutofillCreditCardEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "AutofillCreditCardEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableSavedPaymentCheck.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "PaymentMethodQueryEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "PaymentMethodQueryEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableTracingWeb.IsChecked == true)
                {
                    await ApplyValue(0, localmachineEdge, "DiagnosticData", RegistryValueKind.DWord);
                    await ApplyValue(0, localmachineEdge, "EdgeCollectionsEnabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineEdge, "DiagnosticData", RegistryValueKind.DWord);
                        await ApplyValue(1, localmachineEdge, "EdgeCollectionsEnabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableSystemLocationFunctionality.IsChecked == true)
                {
                    await ApplyValue("Deny", ConsentStorelocation, "Value", RegistryValueKind.String);
                    await ApplyValue(1, locationandsensors, "DisableLocation", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue("Allow", ConsentStorelocation, "Value", RegistryValueKind.String);
                        await ApplyValue(0, locationandsensors, "DisableLocation", RegistryValueKind.DWord);
                    }
                }

                if (DisableLocationScripting.IsChecked == true)
                {
                    await ApplyValue(1, locationandsensors, "DisableWindowsLocationProvider", RegistryValueKind.DWord);
                    await ApplyValue(1, locationandsensors, "DisableLocationScripting", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, locationandsensors, "DisableWindowsLocationProvider", RegistryValueKind.DWord);
                        await ApplyValue(0, locationandsensors, "DisableLocationScripting", RegistryValueKind.DWord);
                    }
                }

                if (DisableLocationSensors.IsChecked == true)
                {
                    await ApplyValue(4, SensorService, "Start", RegistryValueKind.DWord);
                    await ApplyValue(0, Autororation, "Enable", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(3, SensorService, "Start", RegistryValueKind.DWord);
                        await ApplyValue(1, Autororation, "Enable", RegistryValueKind.DWord);
                    }
                }

                if (DisableUserActivityRecording.IsChecked == true)
                {
                    await ApplyValue(1, localmachineSystem, "EnableActivityFeed", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineSystem, "EnableActivityFeed", RegistryValueKind.DWord);
                    }
                }

                if (DisableActivityHistoryStorage.IsChecked == true)
                {
                    await ApplyValue(1, localmachineSystem, "PublishUserActivities", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineSystem, "PublishUserActivities", RegistryValueKind.DWord);
                    }
                }

                if (DisableActivitySubmission.IsChecked == true)
                {
                    await ApplyValue(1, localmachineSystem, "UploadUserActivities", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineSystem, "UploadUserActivities", RegistryValueKind.DWord);
                    }
                }

                if (DisableClipboardHistory.IsChecked == true)
                {
                    await ApplyValue(1, localmachineSystem, "AllowClipboardHistory", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineSystem, "AllowClipboardHistory", RegistryValueKind.DWord);
                    }
                }

                if (DisableClipboardCloudSync.IsChecked == true)
                {
                    await ApplyValue(1, localmachineSystem, "AllowCrossDeviceClipboard", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineSystem, "AllowCrossDeviceClipboard", RegistryValueKind.DWord);
                    }
                }

                if (DisableAppAccessUserInfo.IsChecked == true)
                {
                    await ApplyValue("Deny", currentuseraccountinfo, "Value", RegistryValueKind.String);
                    await ApplyValue("Deny", localmachineaccountinfo, "Value", RegistryValueKind.String);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue("Allow", currentuseraccountinfo, "Value", RegistryValueKind.String);
                        await ApplyValue("Allow", localmachineaccountinfo, "Value", RegistryValueKind.String);
                    }
                }

                if (DisableAppAccessDiagnostics.IsChecked == true)
                {
                    await ApplyValue(0, localmachineapprivacy, "LetAppsGetDiagnosticInfo", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineapprivacy, "LetAppsGetDiagnosticInfo", RegistryValueKind.DWord);
                    }
                }

                if (DisableAppAccessLocation.IsChecked == true)
                {
                    await ApplyValue(0, localmachineapprivacy, "LetAppsAccessLocation", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineapprivacy, "LetAppsAccessLocation", RegistryValueKind.DWord);
                    }
                }

                if (DisablePasswordReveal.IsChecked == true)
                {
                    await ApplyValue(1, passwordReveal, "DisablePasswordReveal", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, passwordReveal, "DisablePasswordReveal", RegistryValueKind.DWord);
                    }
                }

                if (DisableStepsRecorder.IsChecked == true)
                {
                    await ApplyValue(1, localmachineAppCompat, "DisableUAR", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineAppCompat, "DisableUAR", RegistryValueKind.DWord);
                    }
                }

                if (DisableAppTelemetry.IsChecked == true)
                {
                    await ApplyValue(0, localmachineDataCollection, "AllowTelemetry", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineDataCollection, "AllowTelemetry", RegistryValueKind.DWord);
                    }
                }

                if (DisableDiagCustomization.IsChecked == true)
                {
                    await ApplyValue(1, cloudContent, "DisableTailoredExperiencesWithDiagnosticData", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, cloudContent, "DisableTailoredExperiencesWithDiagnosticData", RegistryValueKind.DWord);
                    }
                }

                if (DisableDiagLogs.IsChecked == true)
                {
                    await ApplyValue(1, localmachineDataCollection, "LimitDiagnosticLogCollection", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineDataCollection, "LimitDiagnosticLogCollection", RegistryValueKind.DWord);
                    }
                }

                if (DisableOneSettingsDownload.IsChecked == true)
                {
                    await ApplyValue(1, localmachineDataCollection, "DisableOneSettingsDownloads", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineDataCollection, "DisableOneSettingsDownloads", RegistryValueKind.DWord);
                    }
                }
                if (DisableWU_P2P.IsChecked == true)
                {
                    await ApplyValue(0, localmachineDeliveryOptimization, "DODownloadMode", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineDeliveryOptimization, "DODownloadMode", RegistryValueKind.DWord);
                    }
                }

                if (DisableSpeechModuleUpdates.IsChecked == true)
                {
                    await ApplyValue(0, localmachineSpeech, "AllowSpeechModelUpdate", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineSpeech, "AllowSpeechModelUpdate", RegistryValueKind.DWord);
                    }
                }

                if (DisableSpynetMembership.IsChecked == true)
                {
                    await ApplyValue(0, spynet, "SpyNetReporting", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, spynet, "SpyNetReporting", RegistryValueKind.DWord);
                    }
                }

                if (DisableSampleSubmission.IsChecked == true)
                {
                    await ApplyValue(0, spynet, "SubmitSamplesConsent", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(2, spynet, "SubmitSamplesConsent", RegistryValueKind.DWord);
                    }
                }

                if (DisableMalwareReporting.IsChecked == true)
                {
                    await ApplyValue(0, WindowsDefender, "SubmitSamplesConsent", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(2, WindowsDefender, "SubmitSamplesConsent", RegistryValueKind.DWord);
                    }
                }
                if (DisablePCToMobileConnection.IsChecked == true)
                {
                    await ApplyValue(0, localmachineSystem, "EnableMmx", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineSystem, "EnableMmx", RegistryValueKind.DWord);
                    }
                }
                if (DisablePCToMobileConnection.IsChecked == true)
                {
                    await ApplyValue(4, localmachineCDPUserService, "Start", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(2, localmachineCDPUserService, "Start", RegistryValueKind.DWord);
                    }
                }

                if (DisableMeetNowTaskbar.IsChecked == true)
                {
                    await ApplyValue(1, localmachineExplorer, "HideSCAMeetNow", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineExplorer, "HideSCAMeetNow", RegistryValueKind.DWord);
                    }
                }

                if (DisableNewsAndInterestsTaskbar.IsChecked == true)
                {
                    await ApplyValue(0, currentuserExplorerAdvanced, "TaskbarDa", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, currentuserExplorerAdvanced, "TaskbarDa", RegistryValueKind.DWord);
                    }
                }

                if (DisableFeedbackReminders.IsChecked == true)
                {
                    await ApplyValue(1, localmachineFeedback, "DoNotShowFeedbackNotifications", RegistryValueKind.DWord);
                    await ApplyValue(1, localmachineDataCollection, "DoNotShowFeedbackNotifications", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineFeedback, "DoNotShowFeedbackNotifications", RegistryValueKind.DWord);
                        await ApplyValue(0, localmachineDataCollection, "DoNotShowFeedbackNotifications", RegistryValueKind.DWord);
                    }
                }

                if (DisableKMSOnlineActivation.IsChecked == true)
                {
                    await ApplyValue(1, localmachinesoftwareprotection, "NoAcquireGT", RegistryValueKind.DWord);
                    await ApplyValue(1, localmachinesoftwareprotection, "NoGenTicket", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachinesoftwareprotection, "NoAcquireGT", RegistryValueKind.DWord);
                        await ApplyValue(0, localmachinesoftwareprotection, "NoGenTicket", RegistryValueKind.DWord);
                    }
                }

                if (DisableMapUpdates.IsChecked == true)
                {
                    await ApplyValue(0, localmachinewindowsMaps, "AutoDownloadAndUpdateMapData", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachinewindowsMaps, "AutoDownloadAndUpdateMapData", RegistryValueKind.DWord);
                    }
                }

                if (DisableOfflineMapsNetworkTraffic.IsChecked == true)
                {
                    await ApplyValue(0, localmachinewindowsMaps, "AllowUntriggeredNetworkTrafficOnSettingsPage", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachinewindowsMaps, "AllowUntriggeredNetworkTrafficOnSettingsPage", RegistryValueKind.DWord);
                    }
                }

                if (DisableRemoteAssistance.IsChecked == true)
                {
                    await ApplyValue(0, localmachinewindowsRA, "fAllowToGetHelp", RegistryValueKind.DWord);
                    await ApplyValue(0, localmachineterminalServices, "fAllowToGetHelp", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachinewindowsRA, "fAllowToGetHelp", RegistryValueKind.DWord);
                        await ApplyValue(1, localmachineterminalServices, "fAllowToGetHelp", RegistryValueKind.DWord);
                    }
                }

                if (DisableRemoteConnections.IsChecked == true)
                {
                    await ApplyValue(1, localmachineterminalServer, "fDenyTSConnections", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineterminalServer, "fDenyTSConnections", RegistryValueKind.DWord);
                    }
                }

                if (DisableNCSI.IsChecked == true)
                {
                    await ApplyValue(1, localmachinesystemNCS, "DisablePassivePolling", RegistryValueKind.DWord);
                    await ApplyValue(0, localmachinesystemInternet, "EnableActiveProbing", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachinesystemNCS, "DisablePassivePolling", RegistryValueKind.DWord);
                        await ApplyValue(1, localmachinesystemInternet, "EnableActiveProbing", RegistryValueKind.DWord);
                    }
                }

                if (DisableWindowsCopilot.IsChecked == true)
                {
                    await ApplyValue(1, localmachineCopilot, "TurnOffWindowsCopilot", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineCopilot, "TurnOffWindowsCopilot", RegistryValueKind.DWord);
                    }
                }

                if (DisableWindowsCopilotRecall.IsChecked == true)
                {
                    await ApplyValue(0, localmachineCopilotRecall, "AllowRecallEnablement", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineCopilotRecall, "AllowRecallEnablement", RegistryValueKind.DWord);
                    }
                }

                if (DisablePaintImageCreator.IsChecked == true)
                {
                    await ApplyValue(1, localmachinePaint, "DisableImageCreator", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachinePaint, "DisableImageCreator", RegistryValueKind.DWord);
                    }
                }

                if (DisablePaintCoCreator.IsChecked == true)
                {
                    await ApplyValue(1, localmachinePaint, "DisableCocreator", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachinePaint, "DisableCocreator", RegistryValueKind.DWord);
                    }
                }

                if (DisablePaintAIFill.IsChecked == true)
                {
                    await ApplyValue(1, localmachinePaint, "DisableGenerativeFill", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachinePaint, "DisableGenerativeFill", RegistryValueKind.DWord);
                    }
                }

                if (DisableExplorerAds.IsChecked == true)
                {
                    await ApplyValue(0, localmachineExplorer, "ShowRecommendations", RegistryValueKind.DWord);
                    await ApplyValue(0, currentuserExplorer, "ShowRecommendations", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, localmachineExplorer, "ShowRecommendations", RegistryValueKind.DWord);
                        await ApplyValue(1, currentuserExplorer, "ShowRecommendations", RegistryValueKind.DWord);
                    }
                }

                if (DisableADCAds.IsChecked == true)
                {
                    await ApplyValue(1, localmachineDisableAdvertisingID, "DisabledByGroupPolicy", RegistryValueKind.DWord);
                    await ApplyValue(0, localmachineDisableAdvertisingID, "Enabled", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(0, localmachineDisableAdvertisingID, "DisabledByGroupPolicy", RegistryValueKind.DWord);
                        await ApplyValue(1, localmachineDisableAdvertisingID, "Enabled", RegistryValueKind.DWord);
                    }
                }

                if (DisableExplorerSuggestions.IsChecked == true)
                {
                    await ApplyValue(0, currentuserExplorerAdvanced, "Start_ShowRecommendations", RegistryValueKind.DWord);
                    await ApplyValue(0, currentuserExplorerAdvanced, "ShowSyncProviderNotifications", RegistryValueKind.DWord);
                }
                else
                {
                    if (ApplyOnlySelectedToggle.IsChecked == false)
                    {
                        await ApplyValue(1, currentuserExplorerAdvanced, "Start_ShowRecommendations", RegistryValueKind.DWord);
                        await ApplyValue(1, currentuserExplorerAdvanced, "ShowSyncProviderNotifications", RegistryValueKind.DWord);
                    }
                }

                await Task.Delay(500);

                Dispatcher.Invoke(() => setstatuscolor("#FF1F6AA5"));
                tried++;
            }
            processing4 = 0;
            tokenreset(ref cts4, ref token4);


            await Dispatcher.InvokeAsync(() => PrivacyStatusLabel.Text = "Setting up is complete! You need to reboot your system");
            setstatuscolor("#FFFFD700");





        }
        public async Task ApplyValue(object value, RegistryKey key, string valueName, RegistryValueKind kind)
        {
            await Task.Run(() =>
            {
                try
                {
                    key.SetValue(valueName, value, kind);
                }
                catch (Exception ex)
                {
                    privacylogs.Add("PrivacyError: " + ex.Message + " " + ex.StackTrace);

                }
            });
        }
        private async void ApplySelectedButton_Click(object sender, RoutedEventArgs e)
        {
            PrivacyStatusLabel.Text = "Applying selected settings...";
            await settingsapply();
        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox searchBox)
                UpdatePlaceholderVisibility(searchBox);
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox searchBox)
                UpdatePlaceholderVisibility(searchBox);
        }

        private void UpdatePlaceholderVisibility(TextBox searchBox)
        {
            if (searchBox == null) return;

            TextBlock placeholder = null;

            if (searchBox.Name == "PrivacySearchBox")
            {
                placeholder = PrivacySearchPlaceholder;
            }

            if (placeholder != null)
            {
              
                placeholder.Visibility = string.IsNullOrEmpty(searchBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
        private void WindowsUpdateIconButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderdialog = new OpenFolderDialog();
            var folderDialog = new OpenFolderDialog();
            bool? result = folderDialog.ShowDialog();

            if (result == true)
            {
                string selectedFolder = folderDialog.FolderName;
                string filepath = selectedFolder + "\\winupdatechanges.txt";
                System.IO.File.Create(filepath).Close();

                using (StreamWriter writer = new StreamWriter(filepath))
                {
                    foreach (string line in winupdatelogs)
                    {
                        writer.WriteLine(line);
                    }
                }
                MessageBox.Show("winupdatechanges.txt created in " + filepath, "Multron WinCare", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public async Task enablefirewallnotifications()
        {
            FirewallStatusLabel.Text = "Microsoft Firewall Notifications enablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {

                    RegistryKey DomainProfile = null;

                    RegistryKey StandardProfile = null;

                    RegistryKey PublicProfile = null;

                    RegistryKey PoliciesDomainProfile = null;
                    RegistryKey PoliciesStandardProfile = null;
                    RegistryKey PoliciesPublicProfile = null;

                    RegistryKey SecurityCenter = null;

                    try { DomainProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", true); } catch { }

                    try { StandardProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", true); } catch { }

                    try { PublicProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile", true); } catch { }

                    try { PoliciesDomainProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\DomainProfile", true); } catch { }
                    try { PoliciesStandardProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\StandardProfile", true); } catch { }
                    try { PoliciesPublicProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\PublicProfile", true); } catch { }
                    try { SecurityCenter = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Security Center"); } catch { }

                    try { SecurityCenter?.DeleteValue("FirewallDisableNotify"); } catch { }

                    try { PoliciesDomainProfile?.DeleteValue("DisableNotifications"); } catch { }

                    try { PoliciesDomainProfile?.Close(); } catch { }



                    try { PoliciesStandardProfile?.DeleteValue("DisableNotifications"); } catch { }

                    try { PoliciesStandardProfile?.Close(); } catch { }



                    try { PoliciesPublicProfile?.DeleteValue("DisableNotifications"); } catch { }

                    try { PoliciesPublicProfile?.Close(); } catch { }

                    try { DomainProfile?.SetValue("DisableNotifications", "0", RegistryValueKind.DWord); } catch { }

                    try { DomainProfile?.Close(); } catch { }



                    try { StandardProfile?.SetValue("DisableNotifications", "0", RegistryValueKind.DWord); } catch { }

                    try { StandardProfile?.Close(); } catch { }



                    try { PublicProfile?.SetValue("DisableNotifications", "0", RegistryValueKind.DWord); } catch { }

                    try { PublicProfile?.Close(); } catch { }




                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        public async Task disablefirewallnotifications()
        {
            FirewallStatusLabel.Text = "Microsoft Firewall Notifications disablement process is in progress...";
            await Task.Run(async () =>
            {
                int remake = 16;
                int tried = 0;
                while (tried < remake)
                {
               
                    RegistryKey DomainProfile = null;
               
                    RegistryKey StandardProfile = null;
            
                    RegistryKey PublicProfile = null;
               
                    RegistryKey PoliciesDomainProfile = null;
                    RegistryKey PoliciesStandardProfile = null;
                    RegistryKey PoliciesPublicProfile = null;

                    RegistryKey SecurityCenter = null;

                    try { DomainProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", true); } catch { }
              
                    try { StandardProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", true); } catch { }
              
                    try { PublicProfile = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile", true); } catch { }

                    try { PoliciesDomainProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\DomainProfile", true); } catch { }
                    try { PoliciesStandardProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\StandardProfile", true); } catch { }
                    try { PoliciesPublicProfile = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsFirewall\PublicProfile", true); } catch { }
                    try { SecurityCenter = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Security Center"); } catch { }
                   
                    try { SecurityCenter?.SetValue("FirewallDisableNotify", "1", RegistryValueKind.DWord); } catch { }

                    try { PoliciesDomainProfile?.SetValue("DisableNotifications", "1", RegistryValueKind.DWord); } catch { }

                    try { PoliciesDomainProfile?.Close(); } catch { }



                    try { PoliciesStandardProfile?.SetValue("DisableNotifications", "1", RegistryValueKind.DWord); } catch { }

                    try { PoliciesStandardProfile?.Close(); } catch { }



                    try { PoliciesPublicProfile?.SetValue("DisableNotifications", "1", RegistryValueKind.DWord); } catch { }

                    try { PoliciesPublicProfile?.Close(); } catch { }

                    try {  DomainProfile?.SetValue("DisableNotifications", "1", RegistryValueKind.DWord); } catch { }
                   
                    try {  DomainProfile?.Close(); } catch { }

                

                    try { StandardProfile?.SetValue("DisableNotifications", "1", RegistryValueKind.DWord); } catch { }
               
                    try { StandardProfile?.Close(); } catch { }

                  

                    try { PublicProfile?.SetValue("DisableNotifications", "1", RegistryValueKind.DWord); } catch { }
                    
                    try { PublicProfile?.Close(); } catch { }

                   

                  
                    tried++;
                    await Task.Delay(50);
                }


            });
        }
        private void OpenGitHubButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
         
                string githubUrl = "https://github.com/winball501";

                Process.Start(new ProcessStartInfo
                {
                    FileName = githubUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open GitHub repository: {ex.Message}", "Error",   MessageBoxButton.OK,  MessageBoxImage.Error);
            }
        }
       
        private void OpenDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
          
                string discordUrl = "https://discord.gg/eBsJgbYEj4";

                Process.Start(new ProcessStartInfo
                {
                    FileName = discordUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Discord invite: {ex.Message}", "Error",   MessageBoxButton.OK,  MessageBoxImage.Error);
            }
        }
        private async void ApplyFirewallButton_Click(object sender, RoutedEventArgs e)
        {
            string status = "";
            ApplyFirewallButton.IsEnabled = false;
            ResetFirewallButton.IsEnabled = false;
            processing2 = 1;
            cts2.Cancel();
            await Task.Yield();
            if(EnableMSFirewallRadio.IsChecked == true)
            {
                await enablemfirewall();
                status += "Microsoft Firewall Enable Actions Applied.";
            } else if(DisableMSFirewallRadio.IsChecked == true)
            {
                await disablemfirewall();
                status += "Microsoft Firewall Disable Actions Applied.";
            }
            if(EnableNotificationsRadio.IsChecked == true)
            {
                await enablefirewallnotifications();
                if (status != "")
                {
                    status += " And Firewall Notifications Enabled.";
                }
                else
                {
                    status += "Windows Firewall Notifications Enable Actions Applied.";
                }
            } else if(DisableNotificationsRadio.IsChecked == true)
            {
                await disablefirewallnotifications();
                if (status != "")
                {
                    status += " And Firewall Notifications Disabled.";
                }
                else
                {
                    status += "Windows Firewall Notifications Disable Actions Applied.";
                }
            }
            status += " You need to restart your system for the changes to take effect completely.";
            FirewallStatusLabel.Text = status;
            processing2 = 0;
            tokenreset(ref cts2, ref token2);
            ApplyFirewallButton.IsEnabled = true;
            ResetFirewallButton.IsEnabled = true;

        }
        private async void ResetClickDefender(object sender, RoutedEventArgs e)

        {
            DefenderStatusLabel.Text = "Setting Microsoft Defender And Windows Security To Default settings...";
            ApplyButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            processing1 = 1;
         
            await Task.Yield();
            await enablewindowsdefender();
            await enablewindowsdefenderui();
            await uilockdownenable();
            await enablewindowsecurityappandbrowserprotectionarea();
            await enablewindowsecurityaccountprotectionarea();
            await enablewindowsecuritydevicesecurityarea();
            await enablefirewallandnetworkprotectionarea();
            await enablefamilyoptionsarea();
            await enablewindowsecuritydeviceperformanceandhealtharea();
            await enablewindowsecurityhistoryarea();
            await enablesecurityandmaintenancenotifications();
            await enablewindowssecuritynotifications();
            enablesecuritycentertasks();
            ApplyButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
            processing1 = 0;
            DefenderStatusLabel.Text = "Setting Microsoft Defender And Windows Security To Default settings is done! You need to restart your system for the changes to take effect completely.";
        }
        public void tokenreset(ref CancellationTokenSource cts, ref CancellationToken token)
        {
            cts.Dispose();
            cts = new CancellationTokenSource();
            token = cts.Token;
        }
        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {

            string status = "";
            ApplyButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            processing1 = 1;
            cts1.Cancel();
        
            await Task.Yield();
            if (DisableDefenderRadio.IsChecked == true)
            {
                try
                {
                    using (var wdFeatures = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Features"))
                    {
                        if (wdFeatures == null)
                        {
                            return;
                        }

                        object tamperValue = wdFeatures?.GetValue("TamperProtection");

                        if (tamperValue is int tp)
                        {
                            if (tp == 1)
                            {
                                MessageBox.Show("Tamper Protection is currently enabled. It must be disabled before attempting to disable Microsoft Defender, as it prevents modifications to core antivirus settings and registry keys", "Multron WinCare", MessageBoxButton.OK, MessageBoxImage.Warning);

                                ApplyButton.IsEnabled = true;
                                processing1 = 0;
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex) { }

                disablesecuritycentertasks();
                await disablewindowsdefender();
                status += "Microsoft Defender Disable Actions Applied.";
            }
            else if (EnableDefenderRadio.IsChecked == true)
            {
                enablesecuritycentertasks();
                await enablewindowsdefender();
                status += "Microsoft Defender Enable Actions Applied.";
            }

            switch (true)
            {
                case bool _ when EnableDefenderUIRadio.IsChecked == true:
                    await enablewindowsdefenderui();
                    status += string.IsNullOrEmpty(status) ? "Windows Security Center UI enablement process is done." : " And UI Enabled.";
                    break;

                case bool _ when DisableDefenderUIRadio.IsChecked == true:
                    await disablewindowsdefenderui();
                    status += string.IsNullOrEmpty(status) ? "Windows Security Center UI Disablement process is done." : " And UI Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when EnableVirusThreatRadio.IsChecked == true:
                    await uilockdownenable();
                    status += string.IsNullOrEmpty(status) ? "Virus And Threat Protection Area Enablement process is done." : " And Virus And Threat Protection Area Enabled.";
                    break;

                case bool _ when DisableVirusThreatRadio.IsChecked == true:
                    await uilockdowndisable();
                    status += string.IsNullOrEmpty(status) ? "Virus And Threat Protection Area Disablement process is done." : " And Virus And Threat Protection Area Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when EnableAccountProtectionRadio.IsChecked == true:
                    await enablewindowsecurityaccountprotectionarea();
                    status += string.IsNullOrEmpty(status) ? "Account Protection Area Enablement process is done." : " And Account Protection Area Enabled.";
                    break;

                case bool _ when DisableAccountProtectionRadio.IsChecked == true:
                    await disablewindowsecurityaccountprotectionarea();
                    status += string.IsNullOrEmpty(status) ? "Account Protection Area Disablement process is done." : " And Account Protection Area Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when EnableAppBrowserRadio.IsChecked == true:
                    await enablewindowsecurityappandbrowserprotectionarea();
                    status += string.IsNullOrEmpty(status) ? "App And Browser Control Area Enablement process is done." : " And App And Browser Control Area Enabled.";
                    break;

                case bool _ when DisableAppBrowserRadio.IsChecked == true:
                    await disablewindowsecurityappandbrowserprotectionarea();
                    status += string.IsNullOrEmpty(status) ? "App And Browser Control Area Disablement process is done." : " And App And Browser Control Area Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when EnableDeviceSecurityRadio.IsChecked == true:
                    await enablewindowsecuritydevicesecurityarea();
                    status += string.IsNullOrEmpty(status) ? "Device Security Area Enablement process is done." : " And Device Security Area Enabled.";
                    break;

                case bool _ when DisableDeviceSecurityRadio.IsChecked == true:
                    await disablewindowsecuritydevicesecurityarea();
                    status += string.IsNullOrEmpty(status) ? "Device Security Area Disablement process is done." : " And Device Security Area Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when EnableFirewallRadio.IsChecked == true:
                    await enablefirewallandnetworkprotectionarea();
                    status += string.IsNullOrEmpty(status) ? "Firewall And Network Protection Area Enablement process is done." : " And Firewall And Network Protection Area Enabled.";
                    break;

                case bool _ when DisableFirewallRadio.IsChecked == true:
                    await disablefirewallandnetworkprotectionarea();
                    status += string.IsNullOrEmpty(status) ? "Firewall And Network Protection Area Disablement process is done." : " And Firewall And Network Protection Area Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when EnableFamilyRadio.IsChecked == true:
                    await enablefamilyoptionsarea();
                    status += string.IsNullOrEmpty(status) ? "Family Options Area Enablement process is done." : " And Family Options Area Enabled.";
                    break;

                case bool _ when DisableFamilyRadio.IsChecked == true:
                    await disablefamilyoptionsarea();
                    status += string.IsNullOrEmpty(status) ? "Family Options Area Disablement process is done." : " And Family Options Area Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when EnableDeviceHealthRadio.IsChecked == true:
                    await enablewindowsecuritydeviceperformanceandhealtharea();
                    status += string.IsNullOrEmpty(status) ? "Device Performance And Health Area Enablement process is done." : " And Device Performance And Health Area Enabled.";
                    break;

                case bool _ when DisableDeviceHealthRadio.IsChecked == true:
                    await disablewindowsecuritydeviceperformanceandhealtharea();
                    status += string.IsNullOrEmpty(status) ? "Device Performance And Health Area Disablement process is done." : " And Device Performance And Health Area Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when EnableHistoryRadio.IsChecked == true:
                    await disablewindowsecurityhistoryarea();
                    status += string.IsNullOrEmpty(status) ? "Protection History Area Enablement process is done." : " And Protection History Area Enabled.";
                    break;

                case bool _ when DisableHistoryRadio.IsChecked == true:
                    await disablewindowsecurityhistoryarea();
                    status += string.IsNullOrEmpty(status) ? "Protection History Area Disablement process is done." : " And Protection History Area Disabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when DisableSecurityNotificationsRadio.IsChecked == true:
                    await disablewindowssecuritynotifications();
                    status += string.IsNullOrEmpty(status) ? "Windows Security Notifications Disablement process is done." : " And Windows Security Notifications Disabled.";
                    break;

                case bool _ when EnableSecurityNotificationsRadio.IsChecked == true:
                    await enablewindowssecuritynotifications();
                    status += string.IsNullOrEmpty(status) ? "Windows Security Notifications Enablement process is done." : " And Windows Security Notifications Enabled.";
                    break;
            }

            switch (true)
            {
                case bool _ when DisableMNORadio.IsChecked == true:
                    await disablesecurityandmaintenancenotifications();
                    status += string.IsNullOrEmpty(status) ? "Security And Maintenance Notifications Disablement process is done." : " And Security and Maintenance Notifications Disabled.";
                    break;

                case bool _ when EnableMNONotificationsRadio.IsChecked == true:
                    await enablesecurityandmaintenancenotifications();
                    status += string.IsNullOrEmpty(status) ? "Security And Maintenance Notifications Enablement process is done." : " And Security And Maintenance Notifications Enabled.";
                    break;
            }

            status += " You need to restart your system for the changes to take effect completely.";
            ApplyButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
            processing1 = 0;
            tokenreset(ref cts1, ref token1);
            DefenderStatusLabel.Text = status;
        }

    }
}
