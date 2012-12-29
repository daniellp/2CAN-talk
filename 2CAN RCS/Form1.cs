/*********************************************************************
 * Software License Agreement
 *
 * Copyright (C) 2010 Cross The Road Electronics.  All rights
 * reserved.
 *
 * Cross The Road Electronics (CTRE) licenses to you the right to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software ONLY when in use with CTRE's 2CAN 
 * Ethernet CAN Gateway.
 *
 * THE SOFTWARE AND DOCUMENTATION ARE PROVIDED "AS IS" WITHOUT
 * WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT
 * LIMITATION, ANY WARRANTY OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT SHALL
 * CROSS THE ROAD ELECTRONICS BE LIABLE FOR ANY INCIDENTAL, SPECIAL, 
 * INDIRECT OR CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, COST OF
 * PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY OR SERVICES, ANY CLAIMS
 * BY THIRD PARTIES (INCLUDING BUT NOT LIMITED TO ANY DEFENSE
 * THEREOF), ANY CLAIMS FOR INDEMNITY OR CONTRIBUTION, OR OTHER
 * SIMILAR COSTS, WHETHER ASSERTED ON THE BASIS OF CONTRACT, TORT
 * (INCLUDING NEGLIGENCE), BREACH OF WARRANTY, OR OTHERWISE.
 *
********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
//using RgbFade = CtrElectronics.CrossLinkControlSystem.RgbFade;
using Inputs = System.Collections.Generic.List<CtrElectronics.CrossLinkControlSystem.Presentation.Input>;
using MecanumInput = CtrElectronics.CrossLinkControlSystem.Presentation.MecanumInput;

namespace CtrElectronics.CrossLinkControlSystem
{
    public partial class Form1 : Form
    {
        private CtrElectronics.CrossLinkControlSystem.RobotController m_RobotController;

        UserApp oUserApp;

        List<Presentation.Input> _Inputs = new List<Presentation.Input>();
        /* flag array so we can random access in constant time.*/
        Presentation.Input[] _InputsArray = null;
        /**
         * For every input device store the unique descriptions for all inputs
         */
        private string[] unique_ids;
        List<Presentation.Output> _Outputs = new List<Presentation.Output>();
        Dictionary<Presentation.Output,Inputs> _OutToIns = new Dictionary<Presentation.Output,Inputs>();

        MecanumInput inputSignalMecFL;
        MecanumInput inputSignalMecFR;
        MecanumInput inputSignalMecRL;
        MecanumInput inputSignalMecRR;

        WireSegment[] _wireSegments = new WireSegment[9];
        Control[] _wireSegmentsControls = new Control[9];

        WireSegment[] _mecanumWireSegments = new WireSegment[3];
        Control[] _mecanumWireSegmentsControls = new Control[3];

        Button _btnRefreshInput = null;

        CtrElectronics.CrossLinkControlSystem.Settings _settings;
        string _settingsPath;
        bool _modified;

        RedOrangeGreenFade _batteryFade = new RedOrangeGreenFade();
        RedOrangeGreenFade _txFade = new RedOrangeGreenFade();
        RedOrangeGreenFade _rxFade = new RedOrangeGreenFade();

        DLL.CommStats _commStats;

        public enum FormState
        {
	        Disconnected,
            ConnnectedDisable,
            ConnnectedEnabledTeleop,
            ConnnectedEnabledAuton,
        };

        FormState _formState;
		/**
 		 * Trigger for the "first" or "left" camera view.
		 */
        Form2 _crossHair1 = new Form2();
		/**
 		 * Trigger for the "second" or "right" camera view.
		 */
        Form2 _crossHair2 = new Form2();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /* restore colors, set background to blend into Form. */
            pnlRightWebCam.BackColor = Color.Transparent;
            pnlLeftWebCam.BackColor = Color.Transparent;
            pnlWebCams.BackColor = Color.Transparent;
            tabPage3.BackColor = Color.Transparent;
            panel2.BackColor = Color.Transparent;
            int rgb = tabControl1.Parent.BackColor.B;
            rgb <<= 8;
            rgb |= tabControl1.Parent.BackColor.G;
            rgb <<= 8;
            rgb |= tabControl1.Parent.BackColor.R;
            try
            {
                axAmcWebCam1.BackgroundColor = rgb;
                axAmcWebCam2.BackgroundColor = rgb;
            }
            catch (Exception)
            {
            }
            /* set defaults for check boxes states. */
            chkCrossHairs1.Checked = false;
            chkCrossHairs2.Checked = false;
            chkWebCam1.Checked = false;
            chkWebCam2.Checked = false;

            oUserApp = new UserApp(this);
            String dllVersion = GetDllVersion();
            if (dllVersion.Length > 0)
                dllVersion = ", " + dllVersion;
            toolStripVersion.Text = "Version " + GetVersion() + dllVersion;
            //Add button to refresh lists...
            _btnRefreshInput = new Button();
            _btnRefreshInput.Text = "Refresh Input List";
            _btnRefreshInput.Size = new Size(flowLayoutPanel1.Size.Width - 25, 30);
            _btnRefreshInput.TextAlign = ContentAlignment.MiddleCenter;
            _btnRefreshInput.Click += new EventHandler(_btnRefreshInput_Click);
            flowLayoutPanel1.Controls.Add(_btnRefreshInput);
            //add the wire segments
            for (int x = 0; x < _wireSegments.Length; ++x)
            {
                _wireSegments[x] = new WireSegment(null,false);
                _wireSegments[x].OnModify += new WireSegment.Event(OnWireSegmentModified);
                flowLayoutPanel1.Controls.Add(_wireSegments[x]);
            }
            /* fill mecanum wire segments */
            string h1 = "Forward/reverse throttle.  Positive input should drive robot forward.";
            string h2 = "Strafe (side-to-side).    Positive input should drive robot to the right.";
            string h3 = "Turn Rotate (Clockwise, Counter-Clockwise).    Positive input should turn robot to the right.";
            flowLayoutMech.Controls.Add(_mecanumWireSegments[0] = new WireSegment(h1,true));
            flowLayoutMech.Controls.Add(_mecanumWireSegments[1] = new WireSegment(h2, true));
            flowLayoutMech.Controls.Add(_mecanumWireSegments[2] = new WireSegment(h3, true));
            _mecanumWireSegments[0].OnModify += new WireSegment.Event(OnWireSegmentModified);
            _mecanumWireSegments[1].OnModify += new WireSegment.Event(OnWireSegmentModified);
            _mecanumWireSegments[2].OnModify += new WireSegment.Event(OnWireSegmentModified);


            cboRcmId.BeginUpdate();
            cboRcmId.Items.Clear();
            cboRcmId.Items.Add("1 (Default)");
            for (int i = 2; i < 16; ++i)
                cboRcmId.Items.Add(i.ToString());
            cboRcmId.SelectedItem = cboRcmId.Items[0];
            cboRcmId.EndUpdate();

            m_RobotController = new CtrElectronics.CrossLinkControlSystem.RobotController();

            Presentation._2CANInputFactory.GetInstance().SetRC(m_RobotController);
            Presentation._2CANOutputFactory.GetInstance().SetRC(m_RobotController);

            _formState = FormState.Disconnected;
            SetFormState(FormState.Disconnected);

            btn2CANUtility.Visible = Is2CanFirmwareUtilityAvailable();

            _settingsPath = GetLastPath();

            if (LoadSettings(_settingsPath) == false)
                CreateNewFile();
            ApplySettingsToGui();

            _modified = false;
            UpdateTitleBar();
            timerIo.Enabled = false;
            timerPres.Enabled = true;

            RefreshWebCams(sender, e);
        }
        public void CreateNewFile()
        {
            _settings = new CtrElectronics.CrossLinkControlSystem.Settings();
            _settingsPath = null;
            ApplySettingsToGui();
            //_modified = true;
            _modified = false; // if file is brand new don't bother user about saving
            UpdateTitleBar();
        }
        /* given an input string description,return the index into our input list,
         * or zero if not found so selection defauls to "not selected".
         */ 
        int FindInputDevice(string inputDev )
        {
            for (int j = 0; j < _Inputs.Count; ++j){
                if (_Inputs[j].ToString().Equals(inputDev)){
                    return j;
                }
            }
            return 0;
        }
        /* TODO - finish and use everywhere and not just for mec stuff */
        void UpdateWsFromXml(WireSegment ws, WireSegmentEntry wse)
        {
            if (wse == null)
                return; /* xml didn't have an entry for this segment so do nothing */
            ws.InputIndex = FindInputDevice(wse.inputDevice);
            ws.Offset = wse.offset;
            ws.DeadbandPercent = wse.deadBandPerc;
            ws.Scalar = wse.scalar;
        }
        /* TODO - finish and use everywhere and not just for mec stuff */
        void SaveWsToXml(WireSegmentEntry wse,WireSegment ws)
        {
            wse.inputDevice = _Inputs[ws.InputIndex].ToString();
            wse.offset = ws.Offset;
            wse.deadBandPerc = ws.DeadbandPercent;
            wse.scalar = ws.Scalar;
        }
        void ApplySettingsToGui()
        {
            ResetWireSegments();
            /* comm settings */
            txt2CANIp.Text = _settings.connection._ip2can;
            cboRcmId.SelectedIndex = Convert.ToInt32(_settings.connection._rcmId) - 1;
            /* load the wire segments and their selected inputs and outputs, update indexes */
            for(int i=0;i<_settings.wireSegments.Length;++i)
            {
                string inputDev = _settings.wireSegments[i].inputDevice;
                string outputDev = _settings.wireSegments[i].outputDevice;

                for(int j=0;j<_Inputs.Count;++j)
                {
                    if(_Inputs[j].ToString().Equals( inputDev ) )
                    {
                        _wireSegments[i].InputIndex = j;
                        break;
                    }
                }
                for(int j=0;j<_Outputs.Count;++j)
                {
                    if(_Outputs[j].ToString().Equals( outputDev ) )
                    {
                        _wireSegments[i].OutputIndex = j;
                        break;
                    }
                }

                _wireSegments[i].Offset = _settings.wireSegments[i].offset;
                _wireSegments[i].DeadbandPercent = _settings.wireSegments[i].deadBandPerc;
                _wireSegments[i].Scalar = _settings.wireSegments[i].scalar;
            }
            /* load mecanum input signals */
            UpdateWsFromXml(_mecanumWireSegments[0], _settings.mecanumSettings.forward);
            UpdateWsFromXml(_mecanumWireSegments[1], _settings.mecanumSettings.strafe);
            UpdateWsFromXml(_mecanumWireSegments[2], _settings.mecanumSettings.twist);
        }
        void ApplyGuiToSettings()
        {
            /* comm settings */
            _settings.connection._ip2can = txt2CANIp.Text;
            _settings.connection._rcmId = Convert.ToString(cboRcmId.SelectedIndex + 1);
            /* clear out settings structure */
            _settings.wireSegments = new CtrElectronics.CrossLinkControlSystem.WireSegmentEntry[_wireSegments.Length];
            _settings.ClearSegments();
            /* fill segments */
            for (int i = 0; i < _wireSegments.Length; ++i)
            {
                string inputDev = _Inputs[ _wireSegments[i].InputIndex ].ToString();
                string outputDev = _Outputs[_wireSegments[i].OutputIndex].ToString();

                CtrElectronics.CrossLinkControlSystem.WireSegmentEntry ws = new CtrElectronics.CrossLinkControlSystem.WireSegmentEntry();
                ws.inputDevice = inputDev;
                ws.outputDevice = outputDev;
                ws.offset = _wireSegments[i].Offset;
                ws.deadBandPerc = _wireSegments[i].DeadbandPercent;
                ws.scalar = _wireSegments[i].Scalar;
                _settings.AddSegment(ws);
            }
            /* clear out mecanumsettings */
            _settings.mecanumSettings.forward = new WireSegmentEntry();
            _settings.mecanumSettings.strafe = new WireSegmentEntry();
            _settings.mecanumSettings.twist = new WireSegmentEntry();
            /* save mec wire segments */
            SaveWsToXml(_settings.mecanumSettings.forward, _mecanumWireSegments[0] );
            SaveWsToXml(_settings.mecanumSettings.strafe, _mecanumWireSegments[1] );
            SaveWsToXml(_settings.mecanumSettings.twist, _mecanumWireSegments[2] );

            _modified = true;
        }
        public bool LoadSettings(string settingsPath)
        {
            if (settingsPath == null)
                return false;
            _settings = CtrElectronics.CrossLinkControlSystem.Settings.LoadSettings(settingsPath);
            return _settings != null;
        }
        public void Save()
        {
            if (_settingsPath == null)
            {
                PromptSave();
            }

            if (_settingsPath != null)
            {
                ApplyGuiToSettings();
                CtrElectronics.CrossLinkControlSystem.Settings.SaveSettings(ref _settings, _settingsPath);
                SaveLastPath(_settingsPath);
                _modified = false;
            }
            UpdateTitleBar();
        }
        public void SaveAs()
        {
            if (PromptSave() == false)
                return;
            Save();
        }
        public void SaveLastPath(string path)
        {
            TextWriter w = new StreamWriter(GetAppPath() + "\\last.txt");
            w.WriteLine(path);
            w.Close();
        }
        public string GetLastPath()
        {
            try
            {
                TextReader r = new StreamReader(GetAppPath() + "\\last.txt");
                string lastPath = r.ReadLine();
                r.Close();

                return lastPath;
            }
            catch (Exception)
            {
            }
            return null;
        }
        bool PromptSave()
        {
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog.DefaultExt = "xml";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _settingsPath = (String)saveFileDialog.FileName;
                return true;
            }
            return false;
        }
        string PromptOpen()
        {
            string path = null;

            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            openFileDialog.DefaultExt = "xml";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }
            return path;
        }
        bool CheckModifedAndSave()
        {
            if (_modified)
            {
                DialogResult result = MessageBox.Show("The file has unsaved changes.\n\nDo you want to save the changes?", 
                                "Cross-Link RCS",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Exclamation);

                if (result == DialogResult.Yes)
                {
                    Save();
                }
                return result == DialogResult.Cancel ? false : true;
            }
            return true;
        }

        public string GetVersion()
        {
            // get the version object for this assembly
            Version v = System.Reflection.Assembly.GetExecutingAssembly().
             GetName().Version;
            
            return v.ToString();
        }

        public string GetDllVersion()
        {
            UInt32 dllVers = 0;
            if (DLL.CTR_GetDllVersion(ref dllVers) != DLL.ctr_error_t.ctr_ok)
                return "";

            string s = "";

            s += Convert.ToString((byte)(dllVers >> 0x18));
            s += ".";
            s += Convert.ToString((byte)(dllVers >> 0x10));
            s += ".";
            s += Convert.ToString((byte)(dllVers >> 0x08));
            s += ".";
            s += Convert.ToString((byte)(dllVers >> 0x00));

            return "DLL(" + s + ")";
        }

        void _btnRefreshInput_Click(object sender, EventArgs e)
        {
            RefreshInputs();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disable();
            if(CheckModifedAndSave() == false)
            {
                e.Cancel = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = "http://" + txt2CANIp.Text;

            System.Diagnostics.Process.Start(url);
        }

        void UpdateTitleBar()
        {
            string title = "";
            //title = "Cross-Link Robot Control System - ";

            if (_settingsPath != null)
            {
                //title += _settingsPath;
                title += Path.GetFileNameWithoutExtension(_settingsPath); ;
            }
            else
            {
                title += "New File";
            }
            if (_modified)
                title += "*";

            title += " (";

            switch (_formState)
            {
                case FormState.Disconnected:
                    title += "Disconnected";
                    break;
                case FormState.ConnnectedDisable:
                    title += "Disabled";
                    break;
                case FormState.ConnnectedEnabledTeleop:
                    title += "Enabled Teleop";
                    break;
                case FormState.ConnnectedEnabledAuton:
                    title += "Enabled Auton";
                    break;
            }

            title += ")";
            


            title += " - Cross-Link Robot Control System";

            this.Text = title;
        }

        void UpdateVBat()
        {
            double dVal = m_RobotController.GetADC(7);
            dVal *= 0.02745703125; // * 3.3/1024 * ( 70 + 5.2 + 10) /  ( 10) 
            lblBattery.Text = "Battery : " + Util.DoubleToStrFixed(dVal) + " V";
            lblBattery.Visible = true;

            if (dVal < 10)
                _batteryFade.SetToRed();
            else if (dVal < 11)
                _batteryFade.StepTowardsOrange();
            else
                _batteryFade.StepTowardsGreen();
            lblBattery.ForeColor = Color.FromArgb((int)_batteryFade.ToInt());

            DLL.CommStats cs = m_RobotController.GetCommStats();
            lblRxCount.Visible = true;
            lblRxCount.Text = "Rx : " + Convert.ToString(cs.rxCount);
            lblTxCount.Visible = true;
            lblTxCount.Text = "Tx : " + Convert.ToString(cs.txCount);

            if(_commStats.txCount == cs.txCount)
                _txFade.SetToRed();
            else
                _txFade.StepTowardsGreen();
            
            if(_commStats.rxCount == cs.rxCount)
                _rxFade.SetToRed();
            else
                _rxFade.StepTowardsGreen();

            lblTxCount.ForeColor = Color.FromArgb((int)_txFade.ToInt());
            lblRxCount.ForeColor = Color.FromArgb((int)_rxFade.ToInt());
            _commStats = cs;
        }
        UInt64 GetOutputEnables()
        {
            UInt64 outputEnables = 0;
            for(int i=0;i<_wireSegments.Length;++i)
            {
                outputEnables |= _Outputs[ _wireSegments[i].OutputIndex ].GetOutputEnable();
            }
            return outputEnables;
        }
        void SetFormState(FormState formState)
        {
            _formState = formState;
            switch (_formState)
            {
                case FormState.Disconnected:
                    oUserApp.SignalStop();

                    txt2CANIp.Enabled = true;
                    cboRcmId.Enabled = true;

                    lblEnabled.Text = "Disconnected";
                    foreach (WireSegment ws in _wireSegments)
                    {
                        ws.Unlock();
                    }
                    foreach (WireSegment ws in _mecanumWireSegments)
                        ws.Unlock();
                    
                    Gamepad.GamepadManager.StopStream(); // this will release all handles to gamepads

                    btnConnect.Enabled = true;
                    btnDisconnect.Enabled = false;
                    lblBattery.Visible = false;
                    lblTxCount.Visible = false;
                    lblRxCount.Visible = false;
                    _btnRefreshInput.Enabled = true;
                    btnEnableAuton.Enabled = false;
                    btnEnableTeleop.Enabled = false;
                    btnDisable.Enabled = false;
                    break;
                case FormState.ConnnectedDisable:
                    oUserApp.SignalStop();

                    txt2CANIp.Enabled = false;
                    cboRcmId.Enabled = false;

                    lblEnabled.Text = "Disabled";

                    /* allow input selection and stop reading */
                    foreach (WireSegment ws in _wireSegments)
                        ws.Unlock();
                    foreach (WireSegment ws in _mecanumWireSegments)
                        ws.Unlock();
                    Gamepad.GamepadManager.StopStream(); // this will release all handles to gamepads


                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    _btnRefreshInput.Enabled = true;
                    btnEnableAuton.Enabled = true;
                    btnEnableTeleop.Enabled = true;
                    btnDisable.Enabled = false;
                    break;

                case FormState.ConnnectedEnabledAuton:
                case FormState.ConnnectedEnabledTeleop:

                    txt2CANIp.Enabled = false;
                    cboRcmId.Enabled = false;

                    if(_formState == FormState.ConnnectedEnabledAuton)
                        lblEnabled.Text = "Enabled Auton";
                    else
                        lblEnabled.Text = "Enabled Teleop";

                    oUserApp.SignalStart();

                    PrepIOEngine();

                    /* start reading inputs and stop input selection */
                    Gamepad.GamepadManager.StartDataStream();
                    foreach (WireSegment ws in _wireSegments)
                        ws.Lock();
                    foreach (WireSegment ws in _mecanumWireSegments)
                        ws.Lock();

                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    _btnRefreshInput.Enabled = false;
                    btnEnableAuton.Enabled = false;
                    btnEnableTeleop.Enabled = false;
                    btnDisable.Enabled = true;
                    break;
            }

            UpdateVBat();
            UpdateTitleBar();
        }
        private void Connect()
        {
            m_RobotController.SetRcmNodeId(cboRcmId.SelectedIndex + 1); 
            m_RobotController.Connect(txt2CANIp.Text);

            if (m_RobotController.IsConnected())
            {
                SetFormState(FormState.ConnnectedDisable);
            }
        }
        private void Disconnect()
        {
            m_RobotController.Disconnect();
            SetFormState(FormState.Disconnected);
        }

        private void Enable(DLL.EnableState enableState)
        {
            MecanumInput [] inputMecSignals = {inputSignalMecFL,inputSignalMecFR,inputSignalMecRL,inputSignalMecRR};
            /* latch the raw inputs for mecanum 
             into the mecanum input signals */
            foreach(MecanumInput me in inputMecSignals){
				/* hook up the inputs to grab the user input. */
                me.SetInputs(_Inputs[_mecanumWireSegments[0].InputIndex],
                            _Inputs[_mecanumWireSegments[1].InputIndex],
                            _Inputs[_mecanumWireSegments[2].InputIndex]);
                /* Hook up the wire segments to get the scaler/offsets. */
                me.SetWireSegs(_mecanumWireSegments[0],
                                _mecanumWireSegments[1],
                                _mecanumWireSegments[2]);
            }
            if(m_RobotController.Enable(enableState,GetOutputEnables()) == true)
            {
                if (enableState == DLL.EnableState.Auton)
                    SetFormState(FormState.ConnnectedEnabledAuton);
                else// if (enableState == DLL.EnableState.Teleop)
                    SetFormState(FormState.ConnnectedEnabledTeleop);
            }
        }
        private void Disable()
        {
            m_RobotController.Enable(DLL.EnableState.Disabled,0);
            SetFormState(FormState.ConnnectedDisable);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Connect();
            webBrowser.Navigate(new Uri("http://" + txt2CANIp.Text));
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            Disable();
        }
        private void btnEnableTeleop_Click(object sender, EventArgs e)
        {
            Enable(DLL.EnableState.Teleop);
        }

        private void btnEnableAuton_Click(object sender, EventArgs e)
        {
            Enable(DLL.EnableState.Auton);
        }
        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            string url = "http://www.crosstheroadelectronics.com";

            System.Diagnostics.Process.Start(url);
        }

        private void timerIo_Tick(object sender, EventArgs e)
        {
            switch (_formState)
            {
                case FormState.Disconnected:
                case FormState.ConnnectedDisable:
                    break;
                case FormState.ConnnectedEnabledAuton:
                case FormState.ConnnectedEnabledTeleop:
                    //RunIOEngine();
                    break;
            }
        }
        private void timerPres_Tick(object sender, EventArgs e)
        {
            switch (_formState)
            {
                case FormState.Disconnected:
                    break;
                case FormState.ConnnectedDisable:
                    UpdateVBat();
                    break;
                case FormState.ConnnectedEnabledAuton:
                case FormState.ConnnectedEnabledTeleop:
                    RunPresentationLoop();
                    UpdateVBat();
                    /* if app is not in disconnected state, check spacebar and 
                     * if pressed move to disconnect.
					 * @see http://msdn.microsoft.com/en-us/library/windows/desktop/dd375731(v=vs.85).aspx
					 */
                    short ret = DLL.GetKeyState(0x20); /* VK_SPACE */
                    if ((ret & 0x80) > 0)
                    {
                        Disable();
                    }
                    break;
            }
        }

        void RefreshInputs()
        {
            _Inputs.Clear();

            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.NoInput());

            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.SliderInput(1));
            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.SliderInput(2));
            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.SliderInput(3));
            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.SliderInput(4));
            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.SliderInput(5));

            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.ButtonInput(1));
            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.ButtonInput(2));
            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.ButtonInput(3));
            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.ButtonInput(4));
            _Inputs.Add(new CtrElectronics.CrossLinkControlSystem.Presentation.ButtonInput(5));

            _Inputs.Add(inputSignalMecFL = new MecanumInput(MecanumInput.MecanumInputType.eFrontLeft));
            _Inputs.Add(inputSignalMecFR = new MecanumInput(MecanumInput.MecanumInputType.eFrontRight));
            _Inputs.Add(inputSignalMecRL = new MecanumInput(MecanumInput.MecanumInputType.eRearLeft));
            _Inputs.Add(inputSignalMecRR = new MecanumInput(MecanumInput.MecanumInputType.eRearRight));

            Presentation.GamepadInputFactory.Update();
            foreach (Presentation.Input inpt in Presentation.GamepadInputFactory.GetInputs())
                _Inputs.Add(inpt);
            foreach (Presentation.Input inpt in Presentation._2CANInputFactory.GetInstance().GetInputs())
                _Inputs.Add(inpt);
            /* create a primitive array to pass - TODO remove this and pass container ref*/
            _InputsArray = _Inputs.ToArray();
            /* fill the wire segments with latest input list */
            foreach (WireSegment ws in _wireSegments)            {
                ws.FillInputDropDown(_InputsArray, unique_ids);
            }
            /* fill the mecanum tab with latest input list */
            _mecanumWireSegments[0].FillInputDropDown(_InputsArray, unique_ids);
            _mecanumWireSegments[1].FillInputDropDown(_InputsArray, unique_ids);
            _mecanumWireSegments[2].FillInputDropDown(_InputsArray, unique_ids);

            //update unique key table
            unique_ids = new string[_InputsArray.Length];
            uint i = 0;
            foreach (Presentation.Input input in _InputsArray)
            {
                unique_ids[i++] = input.GetUniqueString();
            }
        }
        void RefreshOutputs()
        {
            _Outputs.Clear();

            _Outputs.Add(new Presentation.NoOutput());
            
            foreach (Presentation.Output outpt in Presentation._2CANOutputFactory.GetInstance().GetOutputs())
                _Outputs.Add(outpt);

            foreach (WireSegment ws in _wireSegments)
            {
                ws.FillOutputDropDown(_Outputs.ToArray());
            }
        }
        void ResetWireSegments()
        {
            RefreshInputs();
            RefreshOutputs();

            foreach (WireSegment ws in _wireSegments)
            {
                ws.InputIndex = 0;
                ws.OutputIndex = 0;
            }
            _wireSegments[0].InputIndex = 1; // first input = Slider 1
            _wireSegments[0].OutputIndex = 1; // first output = Jaguar 1
            /* reset mecanum inputs to not selected */
            _mecanumWireSegments[0].InputIndex = 0;
            _mecanumWireSegments[1].InputIndex = 0;
            _mecanumWireSegments[2].InputIndex = 0;
        }

        double abs(double d)
        {
            if (d < 0)
                return -d;
            return d;
        }
        void PrepIOEngine()
        {
            int i = 0;
            _OutToIns.Clear();
            foreach (WireSegment ws in _wireSegments)
            {
                Presentation.Input input = null;
                Presentation.Output output = null;

                if (ws.InputIndex >= 0)
                    input = _Inputs[ws.InputIndex];
                if (ws.OutputIndex >= 0)
                    output = _Outputs[ws.OutputIndex];
                if ((input != null) && (output != null))
                {
                    Inputs ins = null;
                    if(_OutToIns.TryGetValue(output, out ins) == false)
                    {
                        ins = new Inputs();
                        ins.Add(input);
                        _OutToIns.Add(output,ins);
                    }
                    else
                    {
                        ins.Add(input);
                    }
                }

                //fill input presentation objects
                _wireSegmentsControls[i++] = input.MakePresentationControl();
            }
            /* make presentation objects for mec wire segs */
            _mecanumWireSegmentsControls[0] = _Inputs[_mecanumWireSegments[0].InputIndex].MakePresentationControl();
            _mecanumWireSegmentsControls[1] = _Inputs[_mecanumWireSegments[1].InputIndex].MakePresentationControl();
            _mecanumWireSegmentsControls[2] = _Inputs[_mecanumWireSegments[2].InputIndex].MakePresentationControl();
        }
#if false
        void RunIOEngine()
        {
            Presentation.Output[] outputs = _Outputs.ToArray();
            foreach (WireSegment ws in _wireSegments)
            {
                Presentation.Output output = null;
                if(ws.OutputIndex >= 0)
                    output = _Outputs[ws.OutputIndex];
                
                Inputs ins = null;
                if(_OutToIns.TryGetValue(output, ref ins) == true)
                {
                    ins
                }


                double inValue = 0;
                double dThrottle = inValue;
                Presentation.Input input = null;
                Presentation.Output output = null;
                string errorMessage = null;
                    
                if(ws.InputIndex >= 0)
                    input = _Inputs[ws.InputIndex ];

                if( (input != null) && (output != null) )
                {
                    double dDeadband = ws.DeadbandPercent;
                    double scalar = ws.Scalar;
                    double offset = ws.Offset;

                    if(input.GetValue(ref inValue) < 0)
                        errorMessage = input.GetLastError();

                    dThrottle = inValue;
                    if (abs(dThrottle) < dDeadband)
                        dThrottle = 0;
                    dThrottle += offset;
                    dThrottle *= scalar;

                    output.SetValue(dThrottle);
                }
                ws.Update(  input.GetPresentationControl(), 
                            output.GetPresentationControl(), 
                            inValue, 
                            dThrottle, 
                            errorMessage);
            }
#endif        
        
        public void RunIOEngine()
        {
            /* roll thru the wiresegments computing them, then passing to the output */
            Presentation.Output[] outputs = _Outputs.ToArray();
            foreach (WireSegment ws in _wireSegments)
            {
                Presentation.Input input = null;
                Presentation.Output output = null;

                if (ws.InputIndex >= 0)
                    input = _Inputs[ws.InputIndex];
                if (ws.OutputIndex >= 0)
                    output = _Outputs[ws.OutputIndex];

                if ((input != null) && (output != null))
                {
                    double dThrottle = 0;
                    Util.CalcInput(ws, input, ref dThrottle); /* no need to check return */
                    output.SetValue(dThrottle);
                    /*
                    double dDeadband = ws.DeadbandPercent;
                    double scalar = ws.Scalar;
                    double offset = ws.Offset;
                    double inValue = 0;
                    input.GetValue(ref inValue);
                    double dThrottle = inValue;
                    if (abs(dThrottle) < dDeadband)
                        dThrottle = 0;
                    dThrottle += offset;
                    dThrottle *= scalar;

                    output.SetValue(dThrottle);*/
                }
            }
            
            /* outputs that are not wired to at least one wireSegment should be driven to zero. */
            foreach (Presentation.Output outp in _Outputs)
            {
                bool found = false;
                foreach (WireSegment ws in _wireSegments)
                {
                    if (ws.OutputIndex >= 0)
                        if (_Outputs[ws.OutputIndex] == outp)
                        {
                            found = true;
                            break;
                        }
                }
                if (!found)
                    outp.SetValue(0);
            }
        }
        void RunPresentationLoop()
        {
            int i = 0;
            Presentation.Output[] outputs = _Outputs.ToArray();
            foreach (WireSegment ws in _wireSegments)
            {
                double inValue = 0;
                double outValue = 0;
                Presentation.Input input = null;
                Presentation.Output output = null;
                string errorMessage = null;

                if (ws.InputIndex >= 0)
                    input = _Inputs[ws.InputIndex];
                if (ws.OutputIndex >= 0)
                    output = _Outputs[ws.OutputIndex];

                if ((input != null) && (output != null))
                {
                    if (input.GetValue(ref inValue) < 0)
                        errorMessage = input.GetLastError();

                    outValue = output.GetValue();
                }

                input.UpdatePresentationControl(_wireSegmentsControls[i]);
                ws.Update(_wireSegmentsControls[i],
                            output.GetPresentationControl(),
                            inValue,
                            outValue,
                            errorMessage);

                ++i;
            }
            /* refresh the presentation objects for mec view */
            _Inputs[_mecanumWireSegments[0].InputIndex].UpdatePresentationControl( _mecanumWireSegmentsControls[0] );
            _Inputs[_mecanumWireSegments[1].InputIndex].UpdatePresentationControl( _mecanumWireSegmentsControls[1] );
            _Inputs[_mecanumWireSegments[2].InputIndex].UpdatePresentationControl( _mecanumWireSegmentsControls[2] );
            /* more gross */
            _mecanumWireSegments[0].UpdateInputPres(_InputsArray);
            _mecanumWireSegments[1].UpdateInputPres(_InputsArray);
            _mecanumWireSegments[2].UpdateInputPres(_InputsArray);
        }

        string GetAppPath()
        {
            return Path.GetDirectoryName(Application.ExecutablePath); ;
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckModifedAndSave();
            CreateNewFile();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = PromptOpen();
            if(path != null)
            {
                if (LoadSettings(path) == true)
                {
                    _settingsPath = path;
                    ApplySettingsToGui();
                    _modified = false;
                }
                else
                {
                    DialogResult result = MessageBox.Show("Could not open file : " + path,
                                    "Cross-Link RCS",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
                }
            }
            UpdateTitleBar();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void OnWireSegmentModified()
        {
            _modified = true;
            UpdateTitleBar();
        }

        private void txt2CANIp_TextChanged(object sender, EventArgs e)
        {
            _modified = true;
            UpdateTitleBar();
            
        }

        private void cboRcmId_SelectedIndexChanged(object sender, EventArgs e)
        {

            _modified = true;
            UpdateTitleBar();
        }
        
        private bool Is2CanFirmwareUtilityAvailable()
        {
            string exePath = GetAppPath() + "\\2CANFirmwareUtility.exe";
            return File.Exists(exePath);
        }
        private void btn2CANUtility_Click(object sender, EventArgs e)
        {
            if (!Is2CanFirmwareUtilityAvailable())
                return;
            string exePath = GetAppPath() + "\\2CANFirmwareUtility.exe";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = exePath;
            p.Start();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        // Navigates to the given URL if it is valid.
        //private void Navigate(String address,  WebBrowser wb)
        //{
        //    return;
        //    if (String.IsNullOrEmpty(address)) return;
        //    if (address.Equals("about:blank")) return;
        //    if (!address.StartsWith("http://") &&
        //        !address.StartsWith("https://"))
        //    {
        //        address = "http://" + address;
        //    }
        //    try
        //    {
        //        wb.Navigate(new Uri(address));
        //    }
        //    catch (System.UriFormatException)
        //    {
        //        return;
        //    }
        //}
        /*
        void DoWebCam(String address, WebBrowser wb, CheckBox chk)
        {
            if (chk.Checked)
            {
                wb.Visible = true;
                Navigate(address, wb);
            }
            else
            {
                wb.Visible = false;
                //wb.Dispose();
            }
        }
         */
        /**
         * @param aspect = 640/480 = width over height
         */
        void KeepAspectRatio(ref int width, ref int height, double aspect)
        {
            if ((height * aspect) < width)
                width = (int)(height * aspect);

            if ((width / aspect) < height)
                height = (int)(width / aspect);
        }
        void KeepAspectRatio(System.Windows.Forms.Control control)
        {
            int w = control.Width;
            int h = control.Height;
            KeepAspectRatio(ref w, ref h, 640.0 / 480.0);
            control.Width = w;
            control.Height = h;
        }

        void UpdateCrossHairs()
        {
            try
            {
                bool enable1 = chkCrossHairs1.Checked && chkWebCam1.Checked;
                bool enable2 = chkCrossHairs2.Checked && chkWebCam2.Checked;
                /* just use the axis component's cross hair */
                axAmcWebCam1.ClientOverlay = enable1 ? 1 : 0;
                axAmcWebCam1.EnableOverlays = enable1;
                axAmcWebCam1.UIMode = enable1 ? "ptz-relative" : "none";

                axAmcWebCam2.ClientOverlay = enable2 ? 1 : 0;
                axAmcWebCam2.EnableOverlays = enable2;
                axAmcWebCam2.UIMode = enable2 ? "ptz-relative" : "none";
            }
            catch (Exception)
            {
            }
            return; 

            //Point p;
            //if (tabControl1.SelectedIndex != 2)
            //    return;
            //p = FindLocation(pnlLeftWebCam);
            //_crossHair1.Location = new Point(p.X - _crossHair1.Width / 2 + pnlLeftWebCam.Width / 2,
            //                                 p.Y - _crossHair1.Height / 2 + pnlLeftWebCam.Height / 2);
            //_crossHair1.Visible = chkCrossHairs1.Checked &&
            //                    chkWebCam1.Checked;
            //p = FindLocation(pnlRightWebCam);
            //_crossHair2.Location = new Point(p.X - _crossHair2.Width / 2 + pnlRightWebCam.Width / 2,
            //                                 p.Y - _crossHair2.Height / 2 + pnlRightWebCam.Height / 2);
            //_crossHair2.Visible = chkCrossHairs2.Checked &&
            //                    chkWebCam2.Checked;
        }
        private void RestretchPanels()
        {
            if (tabControl1.SelectedIndex != 2)
                return;

            /* update browser sizes */
            if (chkWebCam1.Checked && chkWebCam2.Checked)
            {
                int innerMargin = 1;
                pnlLeftWebCam.Size = new Size(pnlWebCams.Width / 2 - innerMargin, pnlWebCams.Height);
                pnlRightWebCam.Size = new Size(pnlWebCams.Width / 2 - innerMargin, pnlWebCams.Height);
                pnlLeftWebCam.Dock = DockStyle.Left;
                pnlRightWebCam.Dock = DockStyle.Right;
                pnlLeftWebCam.Visible = true;
                pnlRightWebCam.Visible = true;

            }
            else if (chkWebCam1.Checked)
            {
                pnlRightWebCam.Visible = false;
                pnlLeftWebCam.Size = new Size(pnlWebCams.Width, pnlWebCams.Height);
                pnlLeftWebCam.Dock = DockStyle.Fill;
                pnlLeftWebCam.Visible = true;
            }
            else if (chkWebCam2.Checked)
            {
                pnlLeftWebCam.Visible = false;
                pnlRightWebCam.Size = new Size(pnlWebCams.Width, pnlWebCams.Height);
                pnlRightWebCam.Dock = DockStyle.Fill;
                pnlRightWebCam.Visible = true;
            }
            pnlRightWebCam.Refresh();
            pnlLeftWebCam.Refresh();
            pnlWebCams.Refresh();
            axAmcWebCam1.Refresh();
            axAmcWebCam2.Refresh();

            //KeepAspectRatio(pnlLeftWebCam);
            //KeepAspectRatio(pnlRightWebCam);
            /*
            _crossHair1.TopLevel = false;
            _crossHair2.TopLevel = false;
            _crossHair1.Parent = this;
            _crossHair2.Parent = this;
            _crossHair1.Visible = true;
            _crossHair2.Visible = true;
            _crossHair1.Show();
            _crossHair2.Show();
            _crossHair1.BringToFront();
            _crossHair2.BringToFront();
            _crossHair1.Location = new Point(100, 100);
            _crossHair2.Location = new Point(100, 200);
            */
        }

        private static Point FindLocation(Control ctrl)
        {
            Point p;
            for (p = ctrl.Location; ctrl.Parent != null; ctrl = ctrl.Parent)
                p.Offset(ctrl.Parent.Location);
            return p;
        }
        /*private Point FindLocation(Control ctrl)
        {
            if (ctrl.Parent is Form)
                return ctrl.Location;
            else
            {
                Point p = FindLocation(ctrl.Parent);
                p.X += ctrl.Location.X;
                p.Y += ctrl.Location.Y;
                return p;
            }
        }*/


        private void Form1_Leave(object sender, EventArgs e)
        {
            _crossHair1.Visible = false;
            _crossHair2.Visible = false;
        }
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            _crossHair1.Visible = false;
            _crossHair2.Visible = false;
        }

        private void Form1_Enter(object sender, EventArgs e)
        {
        }

        private void pnlWebCams_Paint(object sender, PaintEventArgs e)
        {
           // RestretchPanels();
        }


        private void RefreshWebCams(object sender, EventArgs e)
        {
            try
            {
                if (chkWebCam1.Checked)
                {
                    axAmcWebCam1.Stop();
                    axAmcWebCam1.Visible = true;
                    // Set properties, deciding what url completion to use by MediaType.
                    axAmcWebCam1.MediaUsername = "";
                    axAmcWebCam1.MediaPassword = "";
                    axAmcWebCam1.MediaType = "mjpeg";
                    axAmcWebCam1.MediaURL = "http://" + txtWebCam1Ip.Text + "//axis-cgi/mjpg/video.cgi";
                    // Start the streaming
                    axAmcWebCam1.Play();
                }
                else
                {
                    axAmcWebCam1.Stop();
                    axAmcWebCam1.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Unable to play stream: " + ex.Message);
            }

            try
            {
                if (chkWebCam2.Checked)
                {
                    axAmcWebCam2.Stop();
                    axAmcWebCam2.Visible = true;
                    // Set properties, deciding what url completion to use by MediaType.
                    axAmcWebCam2.MediaUsername = "";
                    axAmcWebCam2.MediaPassword = "";
                    axAmcWebCam2.MediaType = "mjpeg";
                    axAmcWebCam2.MediaURL = "http://" + txtWebCam2Ip.Text + "//axis-cgi/mjpg/video.cgi";
                    // Start the streaming
                    axAmcWebCam2.Play();
                }
                else
                {
                    axAmcWebCam2.Stop();
                    axAmcWebCam2.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Unable to play stream: " + ex.Message);
            }
            UpdateCrossHairs();
            RestretchPanels();
        }
        private void btnWebCam1_Click(object sender, EventArgs e)
        {
            string url = "http://" + txtWebCam1Ip.Text;
            System.Diagnostics.Process.Start(url);
        }

        private void btnWebCam2_Click(object sender, EventArgs e)
        {
            string url = "http://" + txtWebCam2Ip.Text;
            System.Diagnostics.Process.Start(url);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {	/* first attempt at spacebar => Disable.
				This doesn't work all the time, depending on where
				the application focus is. */
            if(e.KeyCode  == Keys.Space)
                Disable();
        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e)
        {
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            { /* we are on the webcam page, press the refresh button */
                RefreshWebCams(sender, e);
            }
            else
            {
                _crossHair1.Visible = false;
                _crossHair2.Visible = false;
            }
        }

        private void chkCrossHairs1_CheckedChanged(object sender, EventArgs e)
        {
            RefreshWebCams(sender, e);
        }

        private void chkCrossHairs2_CheckedChanged_1(object sender, EventArgs e)
        {
            RefreshWebCams(sender, e);
        }
        private void chkWebCam1_CheckedChanged_1(object sender, EventArgs e)
        {
            RefreshWebCams(sender, e);
        }
        private void chkWebCam2_CheckedChanged_1(object sender, EventArgs e)
        {
            RefreshWebCams(sender, e);
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            UpdateCrossHairs();
            RestretchPanels();
        }
        private void Form1_Move(object sender, EventArgs e)
        {
            UpdateCrossHairs();
            RestretchPanels();
        }
        private void pnlWebCams_SizeChanged(object sender, EventArgs e)
        {
			/* empty for now, maybe remove later */
        }
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
			/* empty for now, maybe remove later */
        }
        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
			/* empty for now, maybe remove later */
        }
        private void axAmcWebCam1_OnNewVideoSize(object sender, AxAXISMEDIACONTROLLib._IAxisMediaControlEvents_OnNewVideoSizeEvent e)
        {
			/* empty for now, maybe remove later */
        }
        private void axAmcWebCam2_OnNewVideoSize(object sender, AxAXISMEDIACONTROLLib._IAxisMediaControlEvents_OnNewVideoSizeEvent e)
        {
			/* empty for now, maybe remove later */
        }
        private void axAmcWebCam1_OnStatusChange(object sender, AxAXISMEDIACONTROLLib._IAxisMediaControlEvents_OnStatusChangeEvent e)
        {
			/* empty for now, maybe remove later */
        }
        private void axAmcWebCam2_OnStatusChange(object sender, AxAXISMEDIACONTROLLib._IAxisMediaControlEvents_OnStatusChangeEvent e)
        {
			/* empty for now, maybe remove later */
        }
    }
}