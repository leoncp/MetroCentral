using System;
using System.Collections.Generic;
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
using LP.Shared;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using DevComponents.WPF.Metro;
using Chilkat;
using Kent.Boogaart.Windows.Controls.Automation.Peers;
using Kent.Boogaart.Windows.Controls;
using System.IO;

namespace MetroCentral
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
        ServiceReference1.CoreServiceClient proxy = new ServiceReference1.CoreServiceClient();
        public static ServiceReference1.User _currentUser = new ServiceReference1.User();

        //Collections
        //ObservableCollection<ServiceReference1.> userCollection;
       
        //Temporary Objects
        ServiceReference1.User _tmpAddEditUser = new ServiceReference1.User();
        ServiceReference1.CSScreen _tmpAddEditScreen = new ServiceReference1.CSScreen();
        ServiceReference1.CSEvent _tmpAddEditEvent = new ServiceReference1.CSEvent();
        ServiceReference1.CSDirection _tmpAddEditDirection = new ServiceReference1.CSDirection();
        ServiceReference1.CSMedia _tmpAddEditMedia = new ServiceReference1.CSMedia();

        ServiceReference1.CSTemplateCollection _templateCollection = new ServiceReference1.CSTemplateCollection();
        ServiceReference1.CSMediaCollection _mediaCollection = new ServiceReference1.CSMediaCollection();
        ServiceReference1.CSMediaCollection _selectedMedia = new ServiceReference1.CSMediaCollection();
        ServiceReference1.CSEventCollection _tmpeventCollection = new ServiceReference1.CSEventCollection();
        
        string _addEditUserMode = ""; //add, or edit
        string _addEditScreenMode = ""; //add, or edit
        string _addEditEventMode = ""; //add, or edit
        string _addEditMedia = ""; //add, or edit

        public static string userlevel = "none";
        string CORESERVICE_IP_ADDRESS = "127.0.0.1";
        string CORESERVICE_PORT = "8888";
        string FTP_IP_ADDRESS = "127.0.0.1";
        string FTP_USERNAME = "ftpuser";
        string FTP_PASSWORD = "bugaboo";
        Boolean ftpBusy = false;
        Ftp2 _ftp;

        Boolean FTP_FILEISNEW = false;

		public MainWindow()
		{
			this.InitializeComponent();
            si.EnableSmartInspect("Metro Manager", true);
			// Insert code required on object creation below this point.
            LoginWindow _loginWindow = new LoginWindow();
            _loginWindow.ShowDialog();
		}

        private void MetroStartControl_Docked(object sender, RoutedEventArgs e)
        {
            
        }

     

        private void MetroAppWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //This happens after login
            ConfigureSavedTheme();
            ConfigureUserAndLevels();
            initSchedulingPage();
        }

        private void ConfigureSavedTheme()
        {
            string SavedTheme = Properties.Settings.Default.Theme;
            foreach (MetroTheme item in MetroTheme.PreDefinedThemes)
            {
                if (item.DisplayName == SavedTheme)
                {
                    MetroUI.SetTheme(mainAppWindow, item);
                    return;
                }

            }
        }

        private void ConfigureUserAndLevels()
        {
            AdjustProxySettings();
            StartControl.FirstName = _currentUser.Groupid;
            StartControl.LastName = _currentUser.Fullname;

            if (_currentUser.Groupid == "Administrator")
            {
                btnDeleteEvent.IsEnabled = true; btnDeleteEvent.Opacity = 1;
                btnAddNewEvent.IsEnabled = true; btnAddNewEvent.Opacity = 1;
                btnEditEvent.IsEnabled = true; btnEditEvent.Opacity = 1;
                mtScheduling.IsEnabled = true; mtScheduling.Opacity = 1;
                mtMediaLoops.IsEnabled = true; mtMediaLoops.Opacity = 1;
                mtTemplates.IsEnabled = true; mtTemplates.Opacity = 1;
                mtDisplays.IsEnabled = true; mtDisplays.Opacity = 1;
                mtDirections.IsEnabled = true; mtDirections.Opacity = 1;
                mtUsers.IsEnabled = true; mtUsers.Opacity = 1;
            }
            else
            if (_currentUser.Groupid == "Scheduler")
            {
                btnDeleteEvent.IsEnabled = true; btnDeleteEvent.Opacity = 1;
                btnAddNewEvent.IsEnabled = true; btnAddNewEvent.Opacity = 1;
                btnEditEvent.IsEnabled = true; btnEditEvent.Opacity = 1;
                mtScheduling.IsEnabled = true; mtScheduling.Opacity = 1;
                mtMediaLoops.IsEnabled = true; mtMediaLoops.Opacity = 1;
                mtTemplates.IsEnabled = false; mtTemplates.Opacity = 0.5;
                mtDisplays.IsEnabled = false; mtDisplays.Opacity = 0.5;
                mtDirections.IsEnabled = false; mtDirections.Opacity = 0.5;
                mtUsers.IsEnabled = false; mtUsers.Opacity = 0.5;
            }
            else
            if (_currentUser.Groupid == "Viewer")
            {
                btnDeleteEvent.IsEnabled = false; btnDeleteEvent.Opacity = 0.5;
                btnAddNewEvent.IsEnabled = false; btnAddNewEvent.Opacity = 0.5;
                btnEditEvent.IsEnabled = false; btnEditEvent.Opacity = 0.5;
                mtScheduling.IsEnabled = true; mtScheduling.Opacity = 1;
                mtMediaLoops.IsEnabled = false; mtMediaLoops.Opacity = 0.5;
                mtTemplates.IsEnabled = false; mtTemplates.Opacity = 0.5;
                mtDisplays.IsEnabled = false; mtDisplays.Opacity = 0.5;
                mtDirections.IsEnabled = false; mtDirections.Opacity = 0.5;
                mtUsers.IsEnabled = false; mtUsers.Opacity = 0.5;
            }
        }

        private void AdjustProxySettings()
        {
            try
            {
                CORESERVICE_IP_ADDRESS = Properties.Settings.Default.ServiceAddress;
                CORESERVICE_PORT = Properties.Settings.Default.ServicePort;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            System.ServiceModel.BasicHttpBinding binding = (System.ServiceModel.BasicHttpBinding)proxy.Endpoint.Binding;
            int max = 5000000;  // around 5M  
            binding.MaxReceivedMessageSize = max;
            binding.MaxBufferSize = max;
            binding.ReaderQuotas.MaxArrayLength = max;

            binding.MaxReceivedMessageSize = max;
            binding.MaxBufferSize = max;
            binding.ReaderQuotas.MaxArrayLength = max;
            binding.ReaderQuotas.MaxBytesPerRead = max;
            //binding.Name = "binding";
            
            // Increase binding max sizes so that the image can be retrieved  
            proxy = new ServiceReference1.CoreServiceClient(new BasicHttpBinding(), new EndpointAddress("http://" + CORESERVICE_IP_ADDRESS + ":" + CORESERVICE_PORT + "/iTactixCoreService"));

            

            // Increase binding max sizes so that the image can be retrieved  
            if (proxy.Endpoint.Binding is System.ServiceModel.BasicHttpBinding)
            {
                //System.ServiceModel.BasicHttpBinding binding = (System.ServiceModel.BasicHttpBinding)proxy.Endpoint.Binding;
                //int max = 5000000;  // around 5M  
                binding.MaxReceivedMessageSize = max;
                binding.MaxBufferSize = max;
                binding.ReaderQuotas.MaxArrayLength = max;

                //binding.MaxReceivedMessageSize = max;
                //binding.MaxBufferSize = max;
                //binding.ReaderQuotas.MaxArrayLength = max;
                //binding.ReaderQuotas.MaxBytesPerRead = max;
            }
           // si.sil("AdjustProxySettings");
        }

        #region AddUpdateUsers
        private void OptionsFormUsers_Selected(object sender, RoutedEventArgs e)
        {
            UpdateUserListBox();
        }

        private void UpdateUserListBox()
        {
            ServiceReference1.UserCollection _userCollection = new ServiceReference1.UserCollection();
            _userCollection = proxy.CollectUsers();
            ListboxUsers.DataContext = null;
            ListboxUsers.DataContext = _userCollection;
            if (_userCollection.Count > 0) ListboxUsers.SelectedIndex = 0; 
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tmpAddEditUser = new ServiceReference1.User();
                ConfigureUserlevelComboBox();
                _addEditUserMode = "edit";
                _tmpAddEditUser = (ServiceReference1.User)ListboxUsers.SelectedItem;
                borderAddEditUser.DataContext = _tmpAddEditUser;
                animateUserEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollDownards;
                animateUserEdit.StartStateTransition();
                if (_tmpAddEditUser.Loginid != "") borderAddEditUser.Visibility = Visibility.Visible;
                ComboboxUserGroupid.SelectedValue = (string)_tmpAddEditUser.Groupid;
                passwordBox1.Password = _tmpAddEditUser.Password;
                passwordBox2.Password = _tmpAddEditUser.Password;
                animateUserEdit.AnimateStateTransition();
            }
            catch (Exception)
            {
            }
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            ConfigureUserlevelComboBox();
            _addEditUserMode = "add";
            _tmpAddEditUser = new ServiceReference1.User();
            passwordBox1.Password = "";
            passwordBox2.Password = "";
            borderAddEditUser.DataContext = _tmpAddEditUser;
            animateUserEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.FadeIn;
            animateUserEdit.StartStateTransition();
            borderAddEditUser.Visibility = Visibility.Visible;
            animateUserEdit.AnimateStateTransition();
        }

        private void btnCancelAddEditUser_Click(object sender, RoutedEventArgs e)
        {
            animateUserEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.FadeIn;
            animateUserEdit.StartStateTransition();
            borderAddEditUser.Visibility = Visibility.Collapsed;
            animateUserEdit.AnimateStateTransition();
        }

        private void btnApplyAddEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (passwordBox1.Password != passwordBox2.Password)
            {
                MessageBox.Show("Passwords don't match...");
                return;
            }
            if (_addEditUserMode == "edit")
            {
                animateUserEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollUpwards;
                _tmpAddEditUser.Password = passwordBox1.Password;
                proxy.ChangeUser(_tmpAddEditUser);
                UpdateUserListBox();
            }
            else
            {
                animateUserEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollFromRightToLeft;
                _tmpAddEditUser.Password = passwordBox1.Password;
                proxy.InsertUser(_tmpAddEditUser);
                UpdateUserListBox();
            }
            animateUserEdit.StartStateTransition();
            borderAddEditUser.Visibility = Visibility.Collapsed;
            animateUserEdit.AnimateStateTransition();
        }

        private void ConfigureUserlevelComboBox()
        {
            ComboboxUserGroupid.Items.Clear();
            string s = "Administrator";
            ComboboxUserGroupid.Items.Add(s);
            s = "Scheduler";
            ComboboxUserGroupid.Items.Add(s);
            s = "Viewer";
            ComboboxUserGroupid.Items.Add(s);
        }

        private void btnRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tmpAddEditUser = new ServiceReference1.User();
                _tmpAddEditUser = (ServiceReference1.User)ListboxUsers.SelectedItem;
                if (_tmpAddEditUser.Loginid.ToLower() == "admin")
                {
                    MessageBox.Show("The Admin User cannot be removed...");
                    return;
                }
                else
                {
                    proxy.RemoveUser(_tmpAddEditUser);
                    UpdateUserListBox();
                }
                
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        #endregion
        #region AddUpdateDisplays

        private void btnEditDisplay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tmpAddEditScreen = new ServiceReference1.CSScreen();
                _addEditScreenMode = "edit";
                _tmpAddEditScreen = (ServiceReference1.CSScreen)ListboxScreens.SelectedItem;
                borderAddEditScreen.DataContext = _tmpAddEditScreen;
                animateScreenEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollDownards;
                animateScreenEdit.StartStateTransition();
                if (_tmpAddEditScreen.Computername != "") borderAddEditScreen.Visibility = Visibility.Visible;
                animateScreenEdit.AnimateStateTransition();
                tbComputerName.Focus();
            }
            catch (Exception)
            {
            }
        }

        private void btnAddDisplay_Click(object sender, RoutedEventArgs e)
        {
            //ConfigureUserlevelComboBox();
            _addEditScreenMode = "add";
            _tmpAddEditScreen = new ServiceReference1.CSScreen();
            _tmpAddEditScreen.Isdirectional = 0;
            borderAddEditScreen.DataContext = _tmpAddEditScreen;
            animateScreenEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.FadeIn;
            animateScreenEdit.StartStateTransition();
            borderAddEditScreen.Visibility = Visibility.Visible;
            tbComputerName.Focus();
            animateScreenEdit.AnimateStateTransition();
        }

        private void btnRemoveDisplay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tmpAddEditScreen = new ServiceReference1.CSScreen();
                _tmpAddEditScreen = (ServiceReference1.CSScreen)ListboxScreens.SelectedItem;
                proxy.RemoveCSScreen(_tmpAddEditScreen);
                UpdateScreenListBox();
            }
            catch (Exception)
            {  
            }
        }

        private void btnCancelAddEditDisplay_Click(object sender, RoutedEventArgs e)
        {
            animateScreenEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.FadeIn;
            animateScreenEdit.StartStateTransition();
            borderAddEditScreen.Visibility = Visibility.Collapsed;
            animateScreenEdit.AnimateStateTransition();
            UpdateScreenListBox();
        }

        private void btnApplyAddEditDisplay_Click(object sender, RoutedEventArgs e)
        {
            if (_addEditScreenMode == "edit")
            {
                animateScreenEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollUpwards;
                proxy.ChangeCSScreen(_tmpAddEditScreen);
                UpdateScreenListBox();
            }
            else
            {
                animateScreenEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollFromRightToLeft;
                //_tmpAddEditUser.Password = "password";
                proxy.InsertCSScreen(_tmpAddEditScreen);
                UpdateScreenListBox();
            }
            animateScreenEdit.StartStateTransition();
            borderAddEditScreen.Visibility = Visibility.Collapsed;
            animateScreenEdit.AnimateStateTransition();
        }

        private void OptionsFormDisplays_Selected(object sender, RoutedEventArgs e)
        {
            UpdateScreenListBox();
        }

        private void UpdateScreenListBox()
        {
            ServiceReference1.CSScreenCollection _screenCollection = new ServiceReference1.CSScreenCollection();
            _screenCollection = proxy.CollectCSScreens();
            
            ListboxScreens.DataContext = null;
            ListboxScreens.DataContext = _screenCollection;
            if (_screenCollection.Count > 0) ListboxScreens.SelectedIndex = 0;
        }

        #endregion

        private void OptionsFormDirections_Selected(object sender, RoutedEventArgs e)
        {
            ConfigureDirectionsDisplayPage();
        }
        private void ConfigureDirectionsDisplayPage()
        {
            comboDirectionalDisplay.Items.Clear();
            ServiceReference1.CSScreenCollection _screenCollection = new ServiceReference1.CSScreenCollection();
            _screenCollection = proxy.CollectCSScreens();

            string s = "";
            foreach (ServiceReference1.CSScreen _screen in _screenCollection)
            {
                if (_screen.Isdirectional == 1)
                {
                    s = _screen.Screenlocation;
                    comboDirectionalDisplay.Items.Add(s);
                }
            }

            if (comboDirectionalDisplay.Items.Count > 0)
            {
                comboDirectionalDisplay.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("There are currently no Directional Displays to Configure. Please allocate Directional Displays in your System through the Display Configuration Tab...");
            }
        }

        private void UpdateDirectionalDisplayListBox()
        {
            //ServiceReference1.CSDirectionCollection _directions = new ServiceReference1.CSDirectionCollection();
            ServiceReference1.CSDirectionCollection _selectedDirections = new ServiceReference1.CSDirectionCollection();

            string DirectionalString = "";

            DirectionalString = (string)comboDirectionalDisplay.SelectedItem;
            //MessageBox.Show("Directional = " + DirectionalString);
            
            //try
            //{
            //    _directions = proxy.CollectCSDirectionsForThisDirectionalDisplay(string displayname);
            //}
            //catch (Exception)
            //{
            //}
            

            //try
            //{
                _selectedDirections = proxy.CollectCSDirectionsForThisDirectionalDisplay(DirectionalString);
                //foreach (ServiceReference1.CSDirection item in _directions)
                //{
                //    if (item.Directionalscreenname.ToLower() == DirectionalString.ToLower())
                //    {
                //        _selectedDirections.Add(item);
                //    }
                //}
            //}
            //catch (Exception ex)
            //{
            //}

            ServiceReference1.CSScreenCollection _screens = new ServiceReference1.CSScreenCollection();
            _screens = proxy.CollectCSScreens();

            //This Section Creates Default Directions if they aren't already there...
            #region Insert Missing Directions
            Boolean found = false;
            foreach (ServiceReference1.CSScreen item in _screens)
            {
                found = false;
                foreach (ServiceReference1.CSDirection _sel in _selectedDirections)
                {
                    if (item.Screenlocation.ToLower() == _sel.Meetingroomname.ToLower())
                    {
                        found = true;
                    }
                }
                if (!found && item.Isdirectional != 1)
                {
                    ServiceReference1.CSDirection _newD = new ServiceReference1.CSDirection();
                    _newD.Meetingroomname = item.Screenlocation;
                    _newD.Directionalscreenname = DirectionalString;
                    _newD.Directionimageblob = null;
                    _newD.Directionimagefile = "";
                    if (proxy.InsertUpdateCSDirection(_newD))
                    {
                        _selectedDirections.Add(_newD);
                    }
                }
            }
            #endregion  //This Section Creates Default Directions if they aren't already there...

            ListboxDirections.DataContext = null;
            ListboxDirections.DataContext = _selectedDirections;

            if (ListboxDirections.Items.Count > 0)
            {
                ListboxDirections.SelectedIndex = 0;
            }
        }

        private void ListboxDirections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _tmpAddEditDirection = new ServiceReference1.CSDirection();
                _tmpAddEditDirection = (ServiceReference1.CSDirection)ListboxDirections.SelectedItem;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void comboDirectionalDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            UpdateDirectionalDisplayListBox();
        }

        private void testTheme_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ThemeSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MetroTheme selectedTheme = new MetroTheme();
            selectedTheme = (MetroTheme)ThemeSelectionBox.SelectedItem;
            MetroUI.SetTheme(mainAppWindow, selectedTheme);
            Properties.Settings.Default.Theme = selectedTheme.DisplayName;
            Properties.Settings.Default.Save();
        }

        private void showWait()
        {
        }

        private void SendCSMediaFileViaFTP(string fileToFTP, string destinationFileName, string fileType)
        {
            si.sii("SENDING FILE VIA FTP");
            ftpBusy = true;
            //System.Threading.Thread.Sleep(250);
            showWait();
            try
            {
                Boolean success;
                //_ftp = new Ftp2();
                Chilkat.Ftp2 _ftp = new Ftp2();
                //_ftp.UnlockComponent("FTP212345678_29E8FB35jA2U");
                success = _ftp.UnlockComponent("FTP212345678_29E8FB35jA2U");
                if (success != true)
                {
                    //MessageBox.Show(_ftp.LastErrorText);
                    return;
                }
                _ftp.Hostname = CORESERVICE_IP_ADDRESS;
                _ftp.Username = FTP_USERNAME;
                _ftp.Password = FTP_PASSWORD;
                _ftp.Port = 21;
                _ftp.ConnectTimeout = 2000;
                _ftp.IdleTimeoutMs = 2000;
                _ftp.ReadTimeout = 2000;
                _ftp.Passive = true;

                si.sii("FTP INIT OK: " + CORESERVICE_IP_ADDRESS);

                //MessageBox.Show("OK UNLOCKED");

                // Connect and login to the FTP server.
                try
                {
                    success = _ftp.Connect();
                    if (success != true)
                    {
                        //Wait and then try again
                        //System.Threading.Thread.Sleep(250);
                        si.sii("FTP ERR: " + _ftp.LastErrorText);
                        _ftp = new Ftp2();
                        _ftp.Hostname = CORESERVICE_IP_ADDRESS;
                        _ftp.Username = FTP_USERNAME;
                        _ftp.Password = FTP_PASSWORD;
                        _ftp.Port = 21;
                        _ftp.ConnectTimeout = 2000;
                        _ftp.IdleTimeoutMs = 2000;
                        _ftp.ReadTimeout = 2000;
                        success = _ftp.Connect();
                        if (success != true)
                        {
                            si.sii("FTP ERR: " + _ftp.LastErrorText);
                            //MessageBox.Show(_ftp.LastErrorText);
                            //Andagain
                            //System.Threading.Thread.Sleep(250);
                            si.sii("FTP ERR: " + _ftp.LastErrorText);
                            _ftp = new Ftp2();
                            _ftp.Hostname = CORESERVICE_IP_ADDRESS;
                            _ftp.Username = FTP_USERNAME;
                            _ftp.Password = FTP_PASSWORD;
                            _ftp.Port = 21;
                            _ftp.ConnectTimeout = 2000;
                            _ftp.IdleTimeoutMs = 2000;
                            _ftp.ReadTimeout = 2000;
                            success = _ftp.Connect();
                            if (success != true)
                            {

                                si.sii("FTP ERR: " + _ftp.LastErrorText);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    si.six(ex);
                }

                si.sii("FTP LOGIN OK: " + FTP_IP_ADDRESS);

                string subFolder = "";
                if (fileType.ToLower() == "video")
                {
                    subFolder = "Conference/Video";
                }
                else
                    if (fileType.ToLower().ToLower() == "image")
                    {
                        subFolder = "Conference/Images";
                    }
                    else
                        if (fileType.ToLower().ToLower() == "flash")
                        {
                            subFolder = "Conference/Flash";
                        }
                        else
                            if (fileType.ToLower().ToLower() == "audio")
                            {
                                subFolder = "Conference/Audio";
                            }
                            else
                                if (fileType.ToLower().ToLower() == "arrow")
                                {
                                    subFolder = "Conference/Directional";
                                } else
                                    if (fileType.ToLower().ToLower() == "logo")
                                    {
                                        subFolder = "Conference/Logos";
                                    };

                // Change to the remote directory where the file will be uploaded.
                success = _ftp.ChangeRemoteDir(subFolder);
                if (success != true)
                {
                    si.sii("FTP ERR: " + _ftp.LastErrorText);
                    success = _ftp.ChangeRemoteDir(subFolder);
                    if (success != true)
                    {
                        //MessageBox.Show("ERROR CD TO " + subFolder);
                        return;
                    }
                    //MessageBox.Show(_ftp.LastErrorText);
                    //return;
                }
                else
                {
                    //MessageBox.Show("OK CHDIR TO "+subFolder);
                }

                si.sii("FTP CHDIR OK: " + subFolder);

                string localFilename;
                localFilename = fileToFTP;
                string remoteFilename;
                remoteFilename = System.IO.Path.GetFileName(fileToFTP);

                si.sii("FTP FILENAMES: " + @localFilename + "-->" + @remoteFilename);

                //success = _ftp.AsyncPutFileStart(localFilename, remoteFilename);
                success = _ftp.PutFile(localFilename, remoteFilename);

                if (success != true)
                {
                    //Try again
                    //success = _ftp.AsyncPutFileStart(localFilename, remoteFilename);
                    si.sii("FTP ERR: " + _ftp.LastErrorText);
                    success = _ftp.PutFile(localFilename, remoteFilename);
                    if (success != true)
                    {
                        //MessageBox.Show(_ftp.LastErrorText);
                        si.sii("FTP ERR: " + _ftp.LastErrorText);
                        si.sii("FTP ERROR SENDING ASYNC FILE");
                        return;
                    }
                }


                _ftp.Disconnect();
            }
            catch (Exception ex)
            {
                si.six(ex);
            }
            try
            {
                _ftp.Disconnect();
            }
            catch (Exception)
            {
            }
            //showConfirmation();
            ftpBusy = false;
            FTP_FILEISNEW = false;
            //btnImportMedia.IsEnabled = true;
        }

        private void metroShell_BackstageOpened(object sender, RoutedEventArgs e)
        {
        }

        private void initSchedulingPage()
        {
            ServiceReference1.CSScreenCollection tmpscreenCollection = new ServiceReference1.CSScreenCollection();
            tmpscreenCollection = proxy.CollectCSScreens();
            ServiceReference1.CSScreenCollection screenCollection = new ServiceReference1.CSScreenCollection();
            _tmpeventCollection = proxy.CollectCSEvents();
            foreach (ServiceReference1.CSScreen item in tmpscreenCollection)
            {
                if (item.Isdirectional == 0) screenCollection.Add(item);
                //screenCollection.Add(item);
            }
            listboxVenues.DataContext = null;
            listboxVenues.DataContext = screenCollection;
            if (listboxVenues.Items.Count > 0) listboxVenues.SelectedIndex = 0;

            radCalendarEvent.SelectedDate = DateTime.Now;
        }

        private void btnAddNewEvent_Click(object sender, RoutedEventArgs e)
        {
            FTP_FILEISNEW = false;
            _addEditEventMode = "add";
            _tmpAddEditEvent = new ServiceReference1.CSEvent();
            _tmpAddEditEvent.Datetimecreated = DateTime.Now;
            _tmpAddEditEvent.Description = "";
            _tmpAddEditEvent.Title = "";
            ServiceReference1.CSScreen sScreen = new ServiceReference1.CSScreen();
            sScreen = (ServiceReference1.CSScreen)listboxVenues.SelectedItem;
            _tmpAddEditEvent.Screenlocation = sScreen.Screenlocation;
            _tmpAddEditEvent.Template = "default";
            borderAddEditEvent.DataContext = null;
            borderAddEditEvent.DataContext = _tmpAddEditEvent;
           
            DateTime datestarttimefrom = DateTime.Now;
            dtpickerFrom.SelectedTime = new TimeSpan(datestarttimefrom.Hour, datestarttimefrom.Minute, 0);
            DateTime datestarttimeto;
            datestarttimeto = DateTime.Now.AddHours(1);
            dtpickerTo.SelectedTime = new TimeSpan(datestarttimeto.Hour, datestarttimeto.Minute, 0);

            animateEventEdit.AnimationDuration = 250;
            textblockAddEditEventTitle.Text = "Schedule New Event";
            animateEventEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollDownards;
            animateEventEdit.StartStateTransition();
            borderAddEditEvent.Visibility = Visibility.Visible;
            borderAddEditEventInstruction.Visibility = Visibility.Hidden;
            animateEventEdit.AnimateStateTransition();
        }

        private void btnEditEvent_Click(object sender, RoutedEventArgs e)
        {
            FTP_FILEISNEW = false;
            if (listboxEvents.SelectedIndex < 0)
            {
                e.Handled = true;
                return;
            }

            animateEventEdit.AnimationDuration = 250;
            textblockAddEditEventTitle.Text = "Edit Selected Event";

            _addEditEventMode = "edit";
            _tmpAddEditEvent = (ServiceReference1.CSEvent)listboxEvents.SelectedItem;

            DateTime datestarttimefrom = (DateTime)_tmpAddEditEvent.Eventstart;
            dtpickerFrom.SelectedTime = new TimeSpan(datestarttimefrom.Hour, datestarttimefrom.Minute, 0);
            DateTime datestarttimeto;
            datestarttimeto = (DateTime)_tmpAddEditEvent.Eventend;
            dtpickerTo.SelectedTime = new TimeSpan(datestarttimeto.Hour, datestarttimeto.Minute, 0);

            //dtpickerFrom.SelectedDate = _tmpAddEditEvent.Eventend;
            //dtpickerTo.SelectedDate = _tmpAddEditEvent.Eventend;

            borderAddEditEvent.DataContext = null;
            borderAddEditEvent.DataContext = _tmpAddEditEvent;

            animateEventEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.SlideFromLeftToRight;
            animateEventEdit.StartStateTransition();
            borderAddEditEvent.Visibility = Visibility.Visible;
            borderAddEditEventInstruction.Visibility = Visibility.Hidden;
            animateEventEdit.AnimateStateTransition();
        }

        private void btnCancelAddEditEvent_Click(object sender, RoutedEventArgs e)
        {
            animateEventEdit.AnimationDuration = 150;
            animateEventEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollDownards;
            animateEventEdit.StartStateTransition();
            borderAddEditEvent.Visibility = Visibility.Hidden;
            borderAddEditEventInstruction.Visibility = Visibility.Visible;
            animateEventEdit.AnimateStateTransition();
        }

        private void btnApplyAddEditEvent_Click(object sender, RoutedEventArgs e)
        {
            Boolean res = false;

            if (FTP_FILEISNEW == true)
            {
                string fnonly = System.IO.Path.GetFileName(_tmpAddEditEvent.Eventlogofile);
                SendCSMediaFileViaFTP(_tmpAddEditEvent.Eventlogofile, fnonly, "logo");
            }

            DateTime tpST = new DateTime();
            DateTime tpET = new DateTime();
            try
            {
                DateTime rcSD = (DateTime)radCalendarEvent.SelectedDate;
               
                TimeSpan t = (TimeSpan)dtpickerFrom.SelectedTime;
                tpST = new DateTime(rcSD.Year, rcSD.Month, rcSD.Day, t.Hours, t.Minutes, t.Seconds);

                t = (TimeSpan)dtpickerTo.SelectedTime;
                tpET = new DateTime(rcSD.Year, rcSD.Month, rcSD.Day, t.Hours, t.Minutes, t.Seconds);

                _tmpAddEditEvent.Eventstart = (DateTime)tpST;
                _tmpAddEditEvent.Eventend = (DateTime)tpET;
            }
            catch (Exception ex)
            {
            }

            _tmpAddEditEvent.Createdby = _currentUser.Fullname;

            long eventID = 0;
            eventID = (long)proxy.InsertUpdateCSEvent(_tmpAddEditEvent);
            if (_addEditEventMode=="add") _tmpeventCollection.Add(_tmpAddEditEvent);

            animateEventEdit.AnimationDuration = 250;
            animateEventEdit.AnimationType = DevComponents.WpfEditors.eStateAnimationType.ScrollFromRightToLeft;
            animateEventEdit.StartStateTransition();
            borderAddEditEvent.Visibility = Visibility.Hidden;
            borderAddEditEventInstruction.Visibility = Visibility.Visible;
            animateEventEdit.AnimateStateTransition();

            listBoxVenuesOrDate_ChangedSelection();
        }

        private void listboxVenues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listBoxVenuesOrDate_ChangedSelection();
        }

        private void listBoxVenuesOrDate_ChangedSelection()
        {
            try
            {
                ServiceReference1.CSScreen _selectedVenue = new ServiceReference1.CSScreen();
                _selectedVenue = (ServiceReference1.CSScreen)listboxVenues.SelectedItem;
                textblockSelectedVenue.Text = _selectedVenue.Screenlocation;
                ServiceReference1.CSEventCollection eventCollection = new ServiceReference1.CSEventCollection();
                
                DateTime rc = (DateTime)radCalendarEvent.SelectedDate;

                ServiceReference1.CSEvent currentEvent = new ServiceReference1.CSEvent(); currentEvent.Title = "";
                ServiceReference1.CSEvent nextEvent = new ServiceReference1.CSEvent(); nextEvent.Title = "";
                long dtLowNext = 0;

                foreach (ServiceReference1.CSEvent item in _tmpeventCollection)
                {
                    if (item.Screenlocation.ToLower() == _selectedVenue.Screenlocation.ToLower())
                    {
                        //Check for current event, then next event
                        //CurrentEvent
                        long dtStartT = 0;
                        long dtEndT = 0;
                        DateTime evntStart;
                        DateTime evntEnd;
                        long dtCurrentT = DateTime.Now.Ticks;
                         
                        evntStart = (DateTime)item.Eventstart;
                        evntEnd = (DateTime)item.Eventend;
                        dtStartT = evntStart.Ticks;
                        dtEndT = evntEnd.Ticks;
                        if (dtCurrentT >= dtStartT && dtCurrentT < dtEndT)
                        {
                            currentEvent = item;
                        }
                        else
                        if (dtStartT >= dtCurrentT)
                        {
                            if (dtLowNext < dtStartT || dtLowNext == 0)
                            {
                                dtLowNext = dtStartT;
                                nextEvent = item;
                            }
                        }
                        //NextEvent
                        


                        DateTime st = new DateTime();
                        try
                        {
                            st = (DateTime)item.Eventstart;
                        }
                        catch (Exception ex)
                        {
                        }
                         
                         //MessageBox.Show(st.ToString());
                        if (rc.Year == st.Year && rc.Month == st.Month && rc.Day == st.Day)
                        {
                            eventCollection.Add(item);
                        }
                    }
 
                }
                listboxEvents.DataContext = null;
                listboxEvents.DataContext = eventCollection;

                if (currentEvent.Title.Length > 1)
                {
                    DateTime dtEventStart = (DateTime)currentEvent.Eventstart;
                    textblockCurrentEvent.Text = currentEvent.Title + " @ " + dtEventStart.ToShortTimeString();
                }
                else
                {
                    textblockCurrentEvent.Text = "none";
                }

                if (nextEvent.Title.Length > 1)
                {
                    DateTime dtEventStart = (DateTime)nextEvent.Eventstart;
                    textblockNextEvent.Text = nextEvent.Title + " @ " + dtEventStart.ToShortTimeString();
                }
                else
                {
                    textblockNextEvent.Text = "none";
                }

                
            }
            catch (Exception)
            {
            }
        }
        private void btnClearLogo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnChangeLogo_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog { /* Set filter options and filter index.*/Filter = "Image Files|*.jpg;*.png;", FilterIndex = 1, Multiselect = false };
                if (openFileDialog1.ShowDialog() == true)
                {
                    string FilenameAndPath = openFileDialog1.FileName;
                    _tmpAddEditEvent.Eventlogoblob = LPImageLib.GetPhotoThumbnail(@FilenameAndPath);
                    _tmpAddEditEvent.Eventlogofile = FilenameAndPath;
                    FTP_FILEISNEW = true;
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }
        }

        private void radCalendarEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listBoxVenuesOrDate_ChangedSelection();
        }

        private void OptionsFormTemplates_Selected(object sender, RoutedEventArgs e)
        {
            _templateCollection = new ServiceReference1.CSTemplateCollection();
            _templateCollection = proxy.CollectCSTemplates();
            CheckDefaultsAreRegisteredAndPopulateTextBoxes();
        }

        private void CheckDefaultsAreRegisteredAndPopulateTextBoxes()
        {
            //cbFontTest.Text = (string)"Gungsuh";

            tbBackgroundCol.Text = CheckFor("GeneralBackgroundColour", "Black");
            tbDirectionalAlignment.Text = CheckFor("DirectionalAlignment", "c");
            tbSDVenueOpacity.Text = CheckFor("SDVenueOpacity", "1");
            tbSDInfoOpacity.Text = CheckFor("SDInfoOpacity", "1");
            tbSDTitleOpacity.Text = CheckFor("SDTitleOpacity", "1");
            tbSDDateOpacity.Text = CheckFor("SDDateOpacity", "1");
            tbLDVenueOpacity.Text = CheckFor("LDVenueOpacity", "1");
            tbLDInfoOpacity.Text = CheckFor("LDInfoOpacity", "1");
            tbLDTitleOpacity.Text = CheckFor("LDTitleOpacity", "1");
            tbLDDateOpacity.Text = CheckFor("LDDateOpacity", "1");
            tbWDVenueOpacity.Text = CheckFor("WDVenueOpacity", "1");
            tbWDInfoOpacity.Text = CheckFor("WDInfoOpacity", "1");
            tbWDTitleOpacity.Text = CheckFor("WDTitleOpacity", "1");
            tbWDDateOpacity.Text = CheckFor("WDDateOpacity", "1");

            tbTitleFont.Text = CheckFor("TitleFont", "Verdana");
            tbTitleFontSize.Text = CheckFor("TitleFontSize", "66");
            tbTitleFontCol.SelectedColor = (Color)ColorConverter.ConvertFromString(CheckFor("TitleFontCol", "White"));
            tbInfoFont.Text = CheckFor("InfoFont", "Verdana");
            tbInfoFontSize.Text = CheckFor("InfoFontSize", "48");
            tbInfoFontCol.SelectedColor = (Color)ColorConverter.ConvertFromString(CheckFor("InfoFontCol", "White"));
            tbVenueFont.Text = CheckFor("VenueFont", "Verdana");
            tbVenueFontSize.Text = CheckFor("VenueFontSize", "48");
            tbVenueFontCol.SelectedColor = (Color)ColorConverter.ConvertFromString(CheckFor("VenueFontCol", "White"));
            tbDateFont.Text = CheckFor("DateFont", "Verdana");
            tbDateFontSize.Text = CheckFor("DateFontSize", "24");
            tbDateFontCol.SelectedColor = (Color)ColorConverter.ConvertFromString(CheckFor("DateFontCol", "White"));
            tbVenuePxFromTop.Text = CheckFor("VenuePx", "320");
            tbTitlePxFromTop.Text = CheckFor("TitlePx", "425");
            tbInfoPxFromTop.Text = CheckFor("InfoPx", "530");
            tbDatePxFromTop.Text = CheckFor("DatePx", "640");
            tbLogoX.Text = CheckFor("LogoX", "0");
            tbLogoY.Text = CheckFor("LogoY", "0");
            tbLogoW.Text = CheckFor("LogoW", "100");
            tbLogoH.Text = CheckFor("LogoH", "100");
            tbVenueBandCol.SelectedColor = (Color)ColorConverter.ConvertFromString(CheckFor("VenueBandCol", "White"));
            tbVenueBandOpacity.Text = CheckFor("VenueBandOpacity", "0.3");
            tbTitleBandCol.SelectedColor = (Color)ColorConverter.ConvertFromString(CheckFor("TitleBandCol", "White"));
            tbTitleBandOpacity.Text = CheckFor("TitleBandOpacity", "0.3");
            tbInfoBandCol.SelectedColor = (Color)ColorConverter.ConvertFromString(CheckFor("InfoBandCol", "White"));
            tbInfoBandOpacity.Text = CheckFor("InfoBandOpacity", "0.3");
            tbDateBandCol.SelectedColor = (Color)ColorConverter.ConvertFromString(CheckFor("DateBandCol", "White"));
            tbDateBandOpacity.Text = CheckFor("DateBandOpacity", "0.3");
            tbDateTxtOpacity.Text = CheckFor("DateTxtOpacity", "1");
            tbLogoOpacity.Text = CheckFor("LogoOpacity", "1");

            tbSDTitleFont.Text = CheckFor("SDTitleFont", "Verdana");
            tbSDTitleFontSize.Text = CheckFor("SDTitleFontSize", "20");
            tbSDTitleFontCol.Text = CheckFor("SDTitleFontCol", "Black");
            tbSDInfoFont.Text = CheckFor("SDInfoFont", "Verdana");
            tbSDInfoFontSize.Text = CheckFor("SDInfoFontSize", "16");
            tbSDInfoFontCol.Text = CheckFor("SDInfoFontCol", "Black");
            tbSDVenueFont.Text = CheckFor("SDVenueFont", "Verdana");
            tbSDVenueFontSize.Text = CheckFor("SDVenueFontSize", "12");
            tbSDVenueFontCol.Text = CheckFor("SDVenueFontCol", "Black");
            tbSDDateFont.Text = CheckFor("SDDateFont", "Verdana");
            tbSDDateFontSize.Text = CheckFor("SDDateFontSize", "12");
            tbSDDateFontCol.Text = CheckFor("SDDateFontCol", "Black");
            tbSDLogoOpacity.Text = CheckFor("SDLogoOpacity", "1");
            tbSDArrowOpacity.Text = CheckFor("SDArrowOpacity", "1");
            tbSDBackgroundOpacity.Text = CheckFor("SDBGOpacity", "0.7");
            tbSDBackgroundCol.Text = CheckFor("SDBGCol", "White");
            tbSDBorderCol.Text = CheckFor("SDBorderCol", "White");
            tbSDBorderThickness.Text = CheckFor("SDBorderThickness", "1");

            tbLDTitleFont.Text = CheckFor("LDTitleFont", "Verdana");
            tbLDTitleFontSize.Text = CheckFor("LDTitleFontSize", "24");
            tbLDTitleFontCol.Text = CheckFor("LDTitleFontCol", "Black");
            tbLDInfoFont.Text = CheckFor("LDInfoFont", "Verdana");
            tbLDInfoFontSize.Text = CheckFor("LDInfoFontSize", "20");
            tbLDInfoFontCol.Text = CheckFor("LDInfoFontCol", "Black");
            tbLDVenueFont.Text = CheckFor("LDVenueFont", "Verdana");
            tbLDVenueFontSize.Text = CheckFor("LDVenueFontSize", "16");
            tbLDVenueFontCol.Text = CheckFor("LDVenueFontCol", "Black");
            tbLDDateFont.Text = CheckFor("LDDateFont", "Verdana");
            tbLDDateFontSize.Text = CheckFor("LDDateFontSize", "14");
            tbLDDateFontCol.Text = CheckFor("LDDateFontCol", "Black");
            tbLDLogoOpacity.Text = CheckFor("LDLogoOpacity", "1");
            tbLDArrowOpacity.Text = CheckFor("LDArrowOpacity", "1");
            tbLDBackgroundOpacity.Text = CheckFor("LDBGOpacity", "0.7");
            tbLDBackgroundCol.Text = CheckFor("LDBGCol", "White");
            tbLDBorderCol.Text = CheckFor("LDBorderCol", "White");
            tbLDBorderThickness.Text = CheckFor("LDBorderThickness", "1");

            tbWDTitleFont.Text = CheckFor("WDTitleFont", "Verdana");
            tbWDTitleFontSize.Text = CheckFor("WDTitleFontSize", "48");
            tbWDTitleFontCol.Text = CheckFor("WDTitleFontCol", "Black");
            tbWDInfoFont.Text = CheckFor("WDInfoFont", "Verdana");
            tbWDInfoFontSize.Text = CheckFor("WDInfoFontSize", "20");
            tbWDInfoFontCol.Text = CheckFor("WDInfoFontCol", "Black");
            tbWDVenueFont.Text = CheckFor("WDVenueFont", "Verdana");
            tbWDVenueFontSize.Text = CheckFor("WDVenueFontSize", "24");
            tbWDVenueFontCol.Text = CheckFor("WDVenueFontCol", "Black");
            tbWDDateFont.Text = CheckFor("WDDateFont", "Verdana");
            tbWDDateFontSize.Text = CheckFor("WDDateFontSize", "16");
            tbWDDateFontCol.Text = CheckFor("WDDateFontCol", "Black");
            tbWDLogoOpacity.Text = CheckFor("WDLogoOpacity", "1");
            tbWDArrowOpacity.Text = CheckFor("WDArrowOpacity", "1");
            tbWDBackgroundOpacity.Text = CheckFor("WDBGOpacity", "0.7");
            tbWDBackgroundCol.Text = CheckFor("WDBGCol", "White");
            tbWDBorderCol.Text = CheckFor("WDBorderCol", "White");
            tbWDBorderThickness.Text = CheckFor("WDBorderThickness", "1");

            tbmediaVenueX.Text = CheckFor("mediaVenueX", "0");
            tbmediaVenueY.Text = CheckFor("mediaVenueY", "0");
            tbmediaVenueW.Text = CheckFor("mediaVenueW", "1366");
            tbmediaVenueH.Text = CheckFor("mediaVenueH", "768");
            tbmediaVenueS.Text = CheckFor("mediaVenueS", "10");
            tbmediaVenueIdle.Text = CheckFor("mediaVenueIdle", "0");
            tbmediaVenueEnabled.Text = CheckFor("mediaVenueEnabled", "1");

            tbmediaDirX.Text = CheckFor("mediaDirX", "0");
            tbmediaDirY.Text = CheckFor("mediaDirY", "0");
            tbmediaDirW.Text = CheckFor("mediaDirW", "1366");
            tbmediaDirH.Text = CheckFor("mediaDirH", "768");
            tbmediaDirS.Text = CheckFor("mediaDirS", "10");
            tbmediaDirIdle.Text = CheckFor("mediaDirIdle", "0");
            tbmediaDirEnabled.Text = CheckFor("mediaDirEnabled", "1");

            tbFlippingEnabled.Text = CheckFor("flippingEnabled", "1");
            tbFlippingDuration.Text = CheckFor("flippingDuration", "10");
        }

        private void btnApplyTemplateChangesNow()
        {
            UpdateTemplate("GeneralBackgroundColour", tbBackgroundCol.Text);
            UpdateTemplate("DirectionalAlignment", tbDirectionalAlignment.Text);
            UpdateTemplate("SDVenueOpacity", tbSDVenueOpacity.Text);
            UpdateTemplate("SDTitleOpacity", tbSDTitleOpacity.Text);
            UpdateTemplate("SDDateOpacity", tbSDDateOpacity.Text);
            UpdateTemplate("SDInfoOpacity", tbSDInfoOpacity.Text);
            UpdateTemplate("LDVenueOpacity", tbLDVenueOpacity.Text);
            UpdateTemplate("LDTitleOpacity", tbLDTitleOpacity.Text);
            UpdateTemplate("LDDateOpacity", tbLDDateOpacity.Text);
            UpdateTemplate("LDInfoOpacity", tbLDInfoOpacity.Text);
            UpdateTemplate("WDVenueOpacity", tbWDVenueOpacity.Text);
            UpdateTemplate("WDTitleOpacity", tbWDTitleOpacity.Text);
            UpdateTemplate("WDDateOpacity", tbWDDateOpacity.Text);
            UpdateTemplate("WDInfoOpacity", tbWDInfoOpacity.Text);

            UpdateTemplate("TitleFont", tbTitleFont.Text);
            UpdateTemplate("TitleFontSize", tbTitleFontSize.Text);
            UpdateTemplate("TitleFontCol", tbTitleFontCol.SelectedColor.ToString());
            UpdateTemplate("VenueFont", tbVenueFont.Text);
            UpdateTemplate("VenueFontSize", tbVenueFontSize.Text);
            UpdateTemplate("VenueFontCol", tbVenueFontCol.SelectedColor.ToString());
            UpdateTemplate("InfoFont", tbInfoFont.Text);
            UpdateTemplate("InfoFontSize", tbInfoFontSize.Text);
            UpdateTemplate("InfoFontCol", tbInfoFontCol.SelectedColor.ToString());
            UpdateTemplate("DateFont", tbDateFont.Text);
            UpdateTemplate("DateFontSize", tbDateFontSize.Text);
            UpdateTemplate("DateFontCol", tbDateFontCol.SelectedColor.ToString());
            UpdateTemplate("VenuePx", tbVenuePxFromTop.Text);
            UpdateTemplate("InfoPx", tbInfoPxFromTop.Text);
            UpdateTemplate("TitlePx", tbTitlePxFromTop.Text);
            UpdateTemplate("DatePx", tbDatePxFromTop.Text);
            UpdateTemplate("LogoX", tbLogoX.Text);
            UpdateTemplate("LogoY", tbLogoY.Text);
            UpdateTemplate("LogoW", tbLogoW.Text);
            UpdateTemplate("LogoH", tbLogoH.Text);
            UpdateTemplate("VenueBandCol", tbVenueBandCol.SelectedColor.ToString());
            UpdateTemplate("VenueBandOpacity", tbVenueBandOpacity.Text);
            UpdateTemplate("TitleBandCol", tbTitleBandCol.SelectedColor.ToString());
            UpdateTemplate("TitleBandOpacity", tbTitleBandOpacity.Text);
            UpdateTemplate("InfoBandCol", tbInfoBandCol.SelectedColor.ToString());
            UpdateTemplate("InfoBandOpacity", tbInfoBandOpacity.Text);
            UpdateTemplate("DateBandCol", tbDateBandCol.SelectedColor.ToString());
            UpdateTemplate("DateBandOpacity", tbDateBandOpacity.Text);
            UpdateTemplate("DateTxtOpacity", tbDateTxtOpacity.Text);
            UpdateTemplate("LogoOpacity", tbLogoOpacity.Text);

            UpdateTemplate("SDTitleFont", tbSDTitleFont.Text);
            UpdateTemplate("SDTitleFontSize", tbSDTitleFontSize.Text);
            UpdateTemplate("SDTitleFontCol", tbSDTitleFontCol.Text);
            UpdateTemplate("SDVenueFont", tbSDVenueFont.Text);
            UpdateTemplate("SDVenueFontSize", tbSDVenueFontSize.Text);
            UpdateTemplate("SDVenueFontCol", tbSDVenueFontCol.Text);
            UpdateTemplate("SDInfoFont", tbSDInfoFont.Text);
            UpdateTemplate("SDInfoFontSize", tbSDInfoFontSize.Text);
            UpdateTemplate("SDInfoFontCol", tbSDInfoFontCol.Text);
            UpdateTemplate("SDDateFont", tbSDDateFont.Text);
            UpdateTemplate("SDDateFontCol", tbSDDateFontCol.Text);
            UpdateTemplate("SDDateFontSize", tbSDDateFontSize.Text);
            UpdateTemplate("SDLogoOpacity", tbSDLogoOpacity.Text);
            UpdateTemplate("SDArrowOpacity", tbSDArrowOpacity.Text);
            UpdateTemplate("SDBGOpacity", tbSDBackgroundOpacity.Text);
            UpdateTemplate("SDBGCol", tbSDBackgroundCol.Text);
            UpdateTemplate("SDBorderCol", tbSDBorderCol.Text);
            UpdateTemplate("SDBorderThickness", tbSDBorderThickness.Text);

            UpdateTemplate("LDTitleFont", tbLDTitleFont.Text);
            UpdateTemplate("LDTitleFontSize", tbLDTitleFontSize.Text);
            UpdateTemplate("LDTitleFontCol", tbLDTitleFontCol.Text);
            UpdateTemplate("LDVenueFont", tbLDVenueFont.Text);
            UpdateTemplate("LDVenueFontSize", tbLDVenueFontSize.Text);
            UpdateTemplate("LDVenueFontCol", tbLDVenueFontCol.Text);
            UpdateTemplate("LDInfoFont", tbLDInfoFont.Text);
            UpdateTemplate("LDInfoFontSize", tbLDInfoFontSize.Text);
            UpdateTemplate("LDInfoFontCol", tbLDInfoFontCol.Text);
            UpdateTemplate("LDDateFont", tbLDDateFont.Text);
            UpdateTemplate("LDDateFontCol", tbLDDateFontCol.Text);
            UpdateTemplate("LDDateFontSize", tbLDDateFontSize.Text);
            UpdateTemplate("LDLogoOpacity", tbLDLogoOpacity.Text);
            UpdateTemplate("LDArrowOpacity", tbLDArrowOpacity.Text);
            UpdateTemplate("LDBGOpacity", tbLDBackgroundOpacity.Text);
            UpdateTemplate("LDBGCol", tbLDBackgroundCol.Text);
            UpdateTemplate("LDBorderCol", tbLDBorderCol.Text);
            UpdateTemplate("LDBorderThickness", tbLDBorderThickness.Text);

            UpdateTemplate("WDTitleFont", tbWDTitleFont.Text);
            UpdateTemplate("WDTitleFontSize", tbWDTitleFontSize.Text);
            UpdateTemplate("WDTitleFontCol", tbWDTitleFontCol.Text);
            UpdateTemplate("WDVenueFont", tbWDVenueFont.Text);
            UpdateTemplate("WDVenueFontSize", tbWDVenueFontSize.Text);
            UpdateTemplate("WDVenueFontCol", tbWDVenueFontCol.Text);
            UpdateTemplate("WDInfoFont", tbWDInfoFont.Text);
            UpdateTemplate("WDInfoFontSize", tbWDInfoFontSize.Text);
            UpdateTemplate("WDInfoFontCol", tbWDInfoFontCol.Text);
            UpdateTemplate("WDDateFont", tbWDDateFont.Text);
            UpdateTemplate("WDDateFontCol", tbWDDateFontCol.Text);
            UpdateTemplate("WDDateFontSize", tbWDDateFontSize.Text);
            UpdateTemplate("WDLogoOpacity", tbWDLogoOpacity.Text);
            UpdateTemplate("WDArrowOpacity", tbWDArrowOpacity.Text);
            UpdateTemplate("WDBGOpacity", tbWDBackgroundOpacity.Text);
            UpdateTemplate("WDBGCol", tbWDBackgroundCol.Text);
            UpdateTemplate("WDBorderCol", tbWDBorderCol.Text);
            UpdateTemplate("WDBorderThickness", tbWDBorderThickness.Text);

            UpdateTemplate("mediaVenueX", tbmediaVenueX.Text);
            UpdateTemplate("mediaVenueY", tbmediaVenueY.Text);
            UpdateTemplate("mediaVenueW", tbmediaVenueW.Text);
            UpdateTemplate("mediaVenueH", tbmediaVenueH.Text);
            UpdateTemplate("mediaVenueS", tbmediaVenueS.Text);
            UpdateTemplate("mediaVenueIdle", tbmediaVenueIdle.Text);
            UpdateTemplate("mediaVenueEnabled", tbmediaVenueEnabled.Text);

            UpdateTemplate("mediaDirX", tbmediaDirX.Text);
            UpdateTemplate("mediaDirY", tbmediaDirY.Text);
            UpdateTemplate("mediaDirW", tbmediaDirW.Text);
            UpdateTemplate("mediaDirH", tbmediaDirH.Text);
            UpdateTemplate("mediaDirS", tbmediaDirS.Text);
            UpdateTemplate("mediaDirIdle", tbmediaDirIdle.Text);
            UpdateTemplate("mediaDirEnabled", tbmediaDirEnabled.Text);

            UpdateTemplate("flippingEnabled", tbFlippingEnabled.Text);
            UpdateTemplate("flippingDuration", tbFlippingDuration.Text);

        }

        private string CheckFor(string p, string d)
        {
            Boolean found = false;
            string v = "";
            foreach (ServiceReference1.CSTemplate attr in _templateCollection)
            {
                if (attr.Name == p && attr.Template == "default")
                {
                    found = true;
                    v = attr.Attribvaluetext;
                }
            }

            if (found == false)
            {
                //MessageBox.Show("NOT FOUND");
                ServiceReference1.CSTemplate newTA = new ServiceReference1.CSTemplate();
                newTA.Name = p;
                newTA.Attribvaluetext = d;
                newTA.Template = "default";
                newTA.Description = p;
                newTA.Lastmodified = DateTime.Now.ToString();
                newTA.Active = 1;
                newTA.Attribvaluenumber = 0;
                newTA.Modifiedby = "";
                newTA.Thumb = null;
                newTA.Createdby = "";
                newTA.Created = 1;
                v = d;
                proxy.InsertUpdateCSTemplate(newTA);
            }

            return v;
        }

        private void btnApplyTemplateChanges_Click(object sender, RoutedEventArgs e)
        {
            btnApplyTemplateChangesNow();
        }

        private void UpdateTemplate(string param1, string text)
        {
            foreach (ServiceReference1.CSTemplate attr in _templateCollection)
            {
                if (attr.Name == param1 && attr.Template == "default")
                {
                    attr.Attribvaluetext = text;
                    proxy.InsertUpdateCSTemplate(attr);
                }
            }
        }

        private void btnDeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listboxEvents.SelectedIndex > -1)
                {
                    _tmpAddEditEvent = (ServiceReference1.CSEvent)listboxEvents.SelectedItem;
                    proxy.RemoveCSEvent(_tmpAddEditEvent);
                    _tmpeventCollection.Remove(_tmpAddEditEvent);
                }
                listBoxVenuesOrDate_ChangedSelection();
                if (listboxEvents.Items.Count > 0) listboxEvents.SelectedIndex = 0;
            }
            catch (Exception)
            {
            }
        }

        private void btnChangeDirection_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog { /* Set filter options and filter index.*/Filter = "Image Files|*.jpg;*.png;", FilterIndex = 1, Multiselect = false };
                if (openFileDialog1.ShowDialog() == true)
                {
                    string FilenameAndPath = openFileDialog1.FileName;
                    _tmpAddEditDirection.Directionimageblob = LPImageLib.GetPhotoThumbnail(@FilenameAndPath);
                    _tmpAddEditDirection.Directionimagefile = FilenameAndPath;
                    FTP_FILEISNEW = true;
                }
                else
                {
                    FTP_FILEISNEW = false;
                }
            }
            catch (Exception)
            {
                FTP_FILEISNEW = false;
            }

            if (FTP_FILEISNEW == true)
            {
                string fnonly = System.IO.Path.GetFileName(_tmpAddEditDirection.Directionimagefile);
                SendCSMediaFileViaFTP(_tmpAddEditDirection.Directionimagefile, fnonly, "arrow");
            }
            proxy.InsertUpdateCSDirection(_tmpAddEditDirection);
        }

        private void btnPurgeDirection_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    _tmpAddEditDirection = new ServiceReference1.CSDirection();
            //    _tmpAddEditDirection = (ServiceReference1.CSDirection)ListboxDirections.SelectedItem;
            //    if (proxy.RemoveCSDirection(_tmpAddEditDirection)) ListboxDirections.Items.Remove(_tmpAddEditDirection);
            //    UpdateDirectionalDisplayListBox();
            //}
            //catch (Exception ex)
            //{
            //}
        }

        private void btnImportNewMedia_Click(object sender, RoutedEventArgs e)
        {
            if (ImportMediaFile())
            {
                RefreshMediaListBox();
            }
        }

        private Boolean ImportMediaFile()
        {
            FTP_FILEISNEW = false;
            //mediaElementPreview.Stop();
            //mediaElementPreview.Source = null;
            //mediaElementPreview.Visibility = Visibility.Collapsed;
            //imagePreview.Visibility = Visibility.Collapsed;
            _tmpAddEditMedia = new ServiceReference1.CSMedia();
            //string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //if (File.Exists(@appPath + @"\reduced.jpg")) File.Delete(@appPath + @"\reduced.jpg");
            //if (File.Exists(@appPath + @"\Snapshot1.jpg")) File.Delete(@appPath + @"\Snapshot1.jpg");
            //if (File.Exists(@appPath + @"\Snapshot2.jpg")) File.Delete(@appPath + @"\Snapshot2.jpg");
            //if (File.Exists(@appPath + @"\Snapshot3.jpg")) File.Delete(@appPath + @"\Snapshot3.jpg");
            string FilenameAndPath = "";
            string FilenameOnly = "";
            string FileExtensionOnly = "";
            Boolean imageLoaded = false;
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog { /* Set filter options and filter index.*/Filter = "Media Files|*.jpg;*.png;*.wmv;*.avi;*.mpg;*.swf", FilterIndex = 1, Multiselect = false };
                if (openFileDialog1.ShowDialog() == true)
                {
                    FTP_FILEISNEW = true;
                    FilenameOnly = System.IO.Path.GetFileName(openFileDialog1.FileName);
                    FileExtensionOnly = System.IO.Path.GetExtension(openFileDialog1.FileName);
                    FilenameAndPath = openFileDialog1.FileName;
                    _tmpAddEditMedia.Mediafilename = FilenameOnly;
                    //tbTemplateName4.Text = System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    if (FileExtensionOnly.ToLower() == ".jpg" || FileExtensionOnly.ToLower() == ".png" || FileExtensionOnly.ToLower() == ".bmp")
                    {
                        _tmpAddEditMedia.Mediatype = "image";
                        //FileInfo finfo = new FileInfo(FilenameAndPath);
                        if (FTP_FILEISNEW == true)
                        {
                            //MessageBox.Show("CREATING THUMB");
                            //_tmpAddEditMedia.Mediablob = LPImageLib.GetPhotoThumbnail(@FilenameAndPath);
                            //MessageBox.Show("CREATING THUMB DONE");
                            SendCSMediaFileViaFTP(FilenameAndPath, FilenameOnly, "image");
                            _tmpAddEditMedia.Mediafilename = FilenameOnly;
                        }
                        proxy.InsertUpdateCSMedia(_tmpAddEditMedia);
                    }
                    else
                        if (FileExtensionOnly.ToLower() == ".mpg" || FileExtensionOnly.ToLower() == ".avi" || FileExtensionOnly.ToLower() == ".wmv")
                        {

                            _tmpAddEditMedia.Mediatype = "video";
                            if (FTP_FILEISNEW == true)
                            {
                                SendCSMediaFileViaFTP(FilenameAndPath, FilenameOnly, "video");
                            }
                            proxy.InsertUpdateCSMedia(_tmpAddEditMedia);
                            //isMediaOpened = false;
                            //spVolume.Visibility = Visibility.Visible;
                            //tbContenttype.Text = "Video";
                            //mediaFile.Contenttype = "Video";
                            //FileInfo finfo = new FileInfo(FilenameAndPath);
                            //tbFilesize.Text = finfo.Length.ToString();
                            //Uri _mediafile = new Uri(@FilenameAndPath);
                            //btnApplyMediaImport.IsEnabled = false;
                            //mediaElementPreview.Source = _mediafile;
                            //mediaElementPreview.Visibility = Visibility.Visible;
                            //appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                            //mediaFile.Filelocation = FilenameAndPath;
                            //mediaFile.Metadata1 = mediaElementPreview.NaturalDuration.ToString();
                            //mediaFile.Name = tbTemplateName4.Text;
                            //tbDuration.IsEnabled = false;
                        }
                        //else
                        //    if (FileExtensionOnly.ToLower() == ".swf")
                        //    {
                        //        spVolume.Visibility = Visibility.Collapsed;
                        //        tbContenttype.Text = "Flash";
                        //        mediaFile.Contenttype = "Flash";
                        //        FileInfo finfo = new FileInfo(FilenameAndPath);
                        //        tbFilesize.Text = finfo.Length.ToString();
                        //        Uri _mediafile = new Uri(@FilenameAndPath);
                        //        mediaElementPreview.Source = _mediafile;
                        //        mediaElementPreview.Visibility = Visibility.Visible;
                        //        appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        //        mediaFile.Filelocation = FilenameAndPath;
                        //        mediaFile.Metadata1 = "n/a";
                        //        mediaFile.Name = tbTemplateName4.Text;
                        //        tbDuration.IsEnabled = false;
                        //    }
                }
                imageLoaded = true;
            }
            catch (Exception ex)
            {
                imageLoaded = false;
                MessageBox.Show(ex.Message);
            }
            return imageLoaded;
        }

        private void RefreshMediaListBox()
        {
            try
            {
                _mediaCollection = new ServiceReference1.CSMediaCollection();
                _mediaCollection = proxy.CollectCSMedia();
                ListboxMedia.DataContext = null;
                ListboxMedia.DataContext = _mediaCollection;
            }
            catch (Exception ex)
            {
            }
        }

        private void OptionsFormMedia_Selected(object sender, RoutedEventArgs e)
        {
            RefreshMediaListBox();
            RefreshMediaLoop();
        }

        private void RefreshMediaLoop()
        {
            try
            {
                ListboxLoop.Items.Clear();
                ServiceReference1.CSMedialoopCollection _mloopCollection = new ServiceReference1.CSMedialoopCollection();
                _mloopCollection = proxy.CollectCSMediaLoops();
                foreach (ServiceReference1.CSMedialoop item in _mloopCollection)
                {
                    foreach (ServiceReference1.CSMedia _mediaItem in _mediaCollection)
                    {
                        if (item.Mediafilename.ToLower() == _mediaItem.Mediafilename.ToLower())
                        {
                            ListboxLoop.Items.Add(_mediaItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnApplyLoop_Click(object sender, RoutedEventArgs e)
        {
            btnApplyMediaLoop();
        }

        private void btnApplyMediaLoop()
        {
            int x = 1;
            try
            {
                proxy.PurgeCSMediaLoops();
                foreach (ServiceReference1.CSMedia item in ListboxLoop.Items)
                {
                    ServiceReference1.CSMedialoop ml = new ServiceReference1.CSMedialoop();
                    ml.Loopname = "default";
                    ml.Mediafilename = item.Mediafilename;
                    ml.Mediatype = item.Mediatype;
                    ml.Order = x;
                    proxy.InsertUpdateCSMediaLoop(ml);
                    x++;
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void btnDeleteMedia_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedMediaItem();
        }

        private void DeleteSelectedMediaItem()
        {
            try
            {
                if (ListboxMedia.SelectedIndex < 0) return;
                ServiceReference1.CSMedia delMedia = new ServiceReference1.CSMedia();
                delMedia = (ServiceReference1.CSMedia)ListboxMedia.SelectedItem;
                proxy.RemoveCSMedia(delMedia);
                try
                {
                    ListboxLoop.Items.Remove(delMedia);
                    btnApplyMediaLoop();
                }
                catch (Exception)
                {
                }

                RefreshMediaListBox();
                
                //if (ListboxMedia.Items.Count > 0) ListboxMedia.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
            }
        }
	}
}