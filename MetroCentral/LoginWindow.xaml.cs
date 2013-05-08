using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SmartInspectHelper;
using System.ServiceModel;

namespace MetroCentral
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        ServiceReference1.CoreServiceClient proxy = new ServiceReference1.CoreServiceClient();
        string CORESERVICE_IP_ADDRESS = "127.0.0.1";
        string CORESERVICE_PORT = "8888";
        public LoginWindow()
        {
            InitializeComponent();
            tbPort.Text = Properties.Settings.Default.ServicePort;
            tbServiceAddress.Text = Properties.Settings.Default.ServiceAddress;
            borderConfigure.Visibility = Visibility.Hidden;
            borderLogin.Visibility = Visibility.Collapsed;
            AdjustProxySettings();
            TestService();

            //To be commented out
            //tbUsername.Text = "admin";
            //tbPassword.Password = "password";
        
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) PerformLogin();
        }

        private void btnLogin_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            tbLoginFailed.Visibility = Visibility.Hidden;
            tbUsername.Focus();
        }

        private void PerformLogin()
        {
            si.sie("PerformLogin");
            try
            {
                tbLoginFailed.Visibility = Visibility.Collapsed;
                ServiceReference1.User _currentUser = new ServiceReference1.User();
                _currentUser = proxy.VerifyLogin(tbUsername.Text, tbPassword.Password);
                if (_currentUser.Fullname == "failed")
                {
                    MainWindow.userlevel = "none";
                    tbLoginFailed.Visibility = Visibility.Visible;
                    tbUsername.Focus();
                }
                else
                {
                    tbLoginFailed.Visibility = Visibility.Hidden;
                    MainWindow.userlevel = _currentUser.Groupid;
                    MainWindow._currentUser = _currentUser;
                    //SetupUserLevel();
                    this.Close();
                    //gridLogin.Visibility = Visibility.Collapsed;
                    //tbUserLoggedIn.Text = _currentUser.Fullname;
                    //tbLogout.Visibility = Visibility.Visible;
                    //spNavigation.IsEnabled = true;
                    //btnConfiguration_Click(this, e);
                    //animationControl.AnimateStateTransition();
                    //FetchMediaCollection();
                    //InitializeFTPTimer();
                }
            }
            catch (Exception ex)
            {
            }
            si.sil("PerformLogin");
        }

        private void TestService()
        {
            si.sie("TestService");
            Boolean allok = true;
            try
            {
                ServiceReference1.UserCollection _users = new ServiceReference1.UserCollection();
                _users = proxy.CollectUsers();
                if (_users.Count <= 0)
                {
                    MessageBox.Show(_users.Count.ToString());
                    allok = false;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                allok = false;
            }
            if (allok == false)
            {
                si.sii("allok = false");
                si.sii("collapsed");
                MessageBox.Show("Core Service is currently not available - please check your configuration.");
                borderConfigure.Visibility = Visibility.Visible;
                borderLogin.Visibility = Visibility.Hidden;
                tbServiceAddress.Focus();
                si.sii("done");
            }
            else
            {
                //Initialize Login Opportunity
                borderConfigure.Visibility = Visibility.Hidden;
                borderLogin.Visibility = Visibility.Visible;
            }
            si.sil("TestService");
        }

        private void AdjustProxySettings()
        {
            si.sie("AdjustProxySettings");
            CORESERVICE_IP_ADDRESS = Properties.Settings.Default.ServiceAddress;
            CORESERVICE_PORT = Properties.Settings.Default.ServicePort;
            // Increase binding max sizes so that the image can be retrieved  
            try
            {
                proxy = new ServiceReference1.CoreServiceClient(new BasicHttpBinding(), new EndpointAddress("http://" + CORESERVICE_IP_ADDRESS + ":" + CORESERVICE_PORT + "/iTactixCoreService"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("http://" + CORESERVICE_IP_ADDRESS + ":" + CORESERVICE_PORT + "/iTactixCoreService");
       
            }
            

            // Increase binding max sizes so that the image can be retrieved  
            if (proxy.Endpoint.Binding is System.ServiceModel.BasicHttpBinding)
            {
                System.ServiceModel.BasicHttpBinding binding = (System.ServiceModel.BasicHttpBinding)proxy.Endpoint.Binding;
                int max = 5000000;  // around 5M  
                binding.MaxReceivedMessageSize = max;
                binding.MaxBufferSize = max;
                binding.ReaderQuotas.MaxArrayLength = max;
            }
            si.sil("AdjustProxySettings");
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
        }

        private void btnSaveConfiguration_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                Properties.Settings.Default.ServiceAddress = tbServiceAddress.Text;
                Properties.Settings.Default.ServicePort = tbPort.Text;
                Properties.Settings.Default.Save();
                AdjustProxySettings();
                TestService();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Core Service is currently not available - please check the client configuration.");
            }
        }
    }
}
