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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace CtrElectronics.CrossLinkControlSystem
{
    public partial class WireSegment : UserControl
    {
        private int _index;
        private static int _total = 0;

        public delegate void Event();

        public event Event OnModify;

        private int _outputIndex;
        private int _inputIndex;

        double _lastInValue;
        double _lastOutValue;
        const double invalid = -49224.792;
		/**
		 * @param header Text description at top of WireSegment.
		 * 					Pass null for default text.
		 * @param disableOutput Pass true to NOT display the 
		 *					output dropdown.  This is useful for say
		 *					Mecanum whose outputs are selected elsewhere.
		 */
        public WireSegment(string header, bool disableOutput)
        {
            _lastInValue = invalid;
            _lastOutValue = invalid;

            _outputIndex = -1;
            _inputIndex = -1;
            _index = _total++;
            InitializeComponent();

            if ((_index & 1) == 1)
            {
                panel1.BackColor = System.Drawing.Color.DarkSeaGreen;
            }
            if (disableOutput)
            {
                label2.Visible = false;
                cboOutput1.Visible = false;
                lblOutput1.Visible = false;
                pnlOutput1.Visible = false;
                Width = 560;
            }
            if (header != null)
            {
                pnlHeader.Height = 20;
                this.Height += 20;
                lblHeader.Text = header;
            }
            OnModify = null;
        }
        public void FillInputDropDown(Presentation.Input[] _Inputs, string[] unique_ids)
        {
            Util.FillDropdown(_Inputs, cboInput1, unique_ids);
        }
        public void FillOutputDropDown(Presentation.Output[] _Outputs)
        {
            cboOutput1.BeginUpdate();
            cboOutput1.Items.Clear();
            foreach (Presentation.Output output in _Outputs)
            {
                cboOutput1.Items.Add(output.ToString());
            }
            cboOutput1.EndUpdate();
        }

        double abs(double d)
        {
            if (d < 0)
                return -d;
            return d;
        }

        public void Lock()
        {
            cboInput1.Enabled = false;
            cboOutput1.Enabled = false;
            numOffset.Enabled = false;
            numScalar.Enabled = false;
            numDeadband.Enabled = false;
        }
        public void Unlock()
        {
            cboInput1.Enabled = true;
            cboOutput1.Enabled = true;
            numOffset.Enabled = true;
            numScalar.Enabled = true;
            numDeadband.Enabled = true;
        }

        public int InputIndex
        {
            get { return _inputIndex; }
            set { 
                   cboInput1.SelectedIndex = value;
                   _inputIndex = cboInput1.SelectedIndex;
            }
        }
        public int OutputIndex
        {
            //get { return cboOutput1.SelectedIndex; }
            get { return _outputIndex; }
            set { 
                cboOutput1.SelectedIndex = value;
                _outputIndex = cboOutput1.SelectedIndex;
            }
        }
        public double Offset
        {
            get {
                try
                {
                    return (double)numOffset.Value; 
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set { numOffset.Value = (decimal)value; }
        }
        public double Scalar
        {
            get {
                try
                {
                    return (double)numScalar.Value;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set { numScalar.Value = (decimal)value; }
        }
        public double DeadbandPercent
        {
            get {
                try
                {
                    return (double)((int)numDeadband.Value) / 100.0;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set { numDeadband.Value = (decimal)(100 * value); }
        }

        public void Update( Control input, 
                            Control output, 
                            double inValue, 
                            double outValue,
                            string message)
        {
            //input
            if (_inputIndex >= 0)
            {
                if (input != null)
                {
                    input.Parent = pnlInput1;
                    input.Dock = DockStyle.Fill;

                    if (pnlInput1.Controls.Count > 1)
                        pnlInput1.Controls.Clear();
                }
                else
                    pnlInput1.Controls.Clear();


                if (message != null)
                {
                    lblInput1.Text = Util.DoubleToStr(inValue);
                    lblInput1.Text += ", " + message;
                    _lastInValue = invalid;
                }
                else
                {
                    if (_lastInValue != inValue)
                    {
                        lblInput1.Text = Util.DoubleToStr(inValue);
                        _lastInValue = inValue;
                    }
                }
                lblInput1.BringToFront();
                //lblInput1.Refresh();
            }
            else
            {
                lblInput1.Text = "Not Selected";
                _lastInValue = invalid;
            }

            //output
            if (_outputIndex >= 0)
            {
                if (output != null)
                {
                    output.Parent = pnlOutput1;
                    output.Dock = DockStyle.Fill;

                    if (pnlOutput1.Controls.Count > 1)
                        pnlOutput1.Controls.Clear();
                }
                else
                    pnlOutput1.Controls.Clear();


                if (_lastOutValue != outValue)
                {
                    lblOutput1.Text = Util.DoubleToStr(outValue);
                    _lastOutValue = outValue;
                }
            }
            else
            {
                lblOutput1.Text = "Not Selected, " + Util.DoubleToStr(outValue);
                _lastOutValue = invalid;
            }
        }


        public void UpdateInputPres(Presentation.Input [] Inputs)
        {
            if ((_inputIndex < 0) || (_inputIndex > Inputs.Length) )
            {   /* nothing selected in dropdown */
                lblInput1.Text = "Not Selected";
                _lastInValue = invalid;
                return;
            }
            Presentation.Input input = Inputs[_inputIndex];
            if (input == null)
            {   /* no presentation? */
                pnlInput1.Controls.Clear();
                return;
            }

            double inValue = 0;
            Util.CalcInput(this, input, ref inValue);
         
            if (_lastInValue != inValue)
            {   /* value has changed so update */
                lblInput1.Text = Util.DoubleToStr(inValue);
                _lastInValue = inValue;
            }
            lblInput1.BringToFront();
        }

        private void cboInput1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnModify();
            _inputIndex = cboInput1.SelectedIndex;
            _lastInValue = invalid;
        }

        private void numDeadband_ValueChanged(object sender, EventArgs e)
        {
            OnModify();
        }

        private void numOffset_ValueChanged(object sender, EventArgs e)
        {
            OnModify();
        }

        private void numScalar_ValueChanged(object sender, EventArgs e)
        {
            OnModify();
        }

        private void cboOutput1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnModify();
            _outputIndex = cboOutput1.SelectedIndex;
            _lastOutValue = invalid;
        }
    }
}

