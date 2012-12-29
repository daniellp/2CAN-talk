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
using System.Text;
using System.Windows.Forms;

using Control = System.Windows.Forms.Control;
namespace CSharpRobotController.Presentation
{
    public interface PresentationInput
    {
        int GetValue(ref double value);
        string ToString();
        string GetUniqueString();
        string GetLastError();
        System.Windows.Forms.Control GetPresentationControl();
    }


    public interface InputFactory
    {
        PresentationInput[] GetInputs();
    }
    
    public class GamepadInputFactory 
    {
        static Gamepad.SearchResult[] searchResults = null;
        public static void Update()
        {
            searchResults = Gamepad.GamepadFinder.FindInputs();
        }

        public static PresentationInput[] GetInputs()
        {
            List<Input> ret = new List<Input>();

            foreach (Gamepad.SearchResult s in searchResults)
            {
                //truncate GUID and append to the text
                string sGUID = s.gamepad_guid.ToString();
                sGUID = sGUID.Substring(0, 5);  // hopefully 5 characters of the GUID is enough
                                                // for users to distinguish between multiple 
                                                //gamepads

                uint iNumAxes = s.axis_count;
                for (uint i = 0; i < iNumAxes; ++i)
                {
                    string nm = s.gamepad_name + 
                                " (" + sGUID + ")" +
                                " - " + 
                                s.axis_name[i];
                    ret.Add(new GamepadAxisInput(s.gamepad_guid, i, nm));
                }

                uint iNumBtns = s.btn_count;
                for (uint i = 0; i < iNumBtns; ++i)
                {
                    string nm = s.gamepad_name +
                                " (" + sGUID + ")" +
                                " - " + 
                                "Button " + 
                                (i + 1);
                    ret.Add(new GamepadButtonInput(s.gamepad_guid, i, nm));
                }
            }

            return ret.ToArray();
        }
    }

    public abstract class InputDisplaysProgressBar : PresentationInput
    {
        ProgressBar _progressBar;
        double _min;
        double _max;

        public InputDisplaysProgressBar(double min, double max)
        {
            _progressBar = new ProgressBar();
            _progressBar.Minimum = 0;
            _progressBar.Value = 50;
            _progressBar.Maximum = 100;
            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.MarqueeAnimationSpeed = 1;

            _min = min;
            _max = max;
        }
        void UpdateGui(double value)
        {
            try
            {
                double scaled = (value - min) / (max - min);
                _progressBar.Value = (int)(scaled * 50) + 50;
            }
            catch (Exception ee)
            {
                _progressBar.Value = 50;
            }
        }
        public Control GetPresentationControl()
        {
            double d = 0;
            GetValue(ref d);
            UpdateGui(d);
            return _progressBar;
        }

        public abstract int GetValue(ref double value);
        public abstract string GetUniqueString();
        public abstract string GetLastError();
    }

    class GamepadAxisInput : PresentationInput
    {
        Guid m_gamepad_guid;
        uint m_axis_idx;
        string m_name;

        ProgressBar cntrl;
        bool disconnected;

        public GamepadAxisInput(Guid gamepad_guid, uint axis_idx, string name)
        {
            m_gamepad_guid = gamepad_guid;
            m_axis_idx = axis_idx;
            m_name = name;

            cntrl = new ProgressBar();
            cntrl.Minimum = 0;
            cntrl.Value = 50;
            cntrl.Maximum = 100;
            cntrl.Style = ProgressBarStyle.Blocks;
            cntrl.MarqueeAnimationSpeed = 1;

            disconnected = false;
        }
        public int GetValue(ref double value)
        {
            value = 0;
            if (Gamepad.GamepadManager.GetAxis(m_gamepad_guid, m_axis_idx, ref value) < 0)
            {
                //something has gone wrong with gamepad
                disconnected = true;
                return -1;
            }
            else
                disconnected = false;
            return 0;
        }
        public string GetLastError()
        {
            if(disconnected)
                return "Gamepad disconnected";
            return "";
        }
        public override string ToString()
        {
            return m_name;
        }
        public string GetUniqueString()
        {
            return m_name + " " + m_gamepad_guid.ToString();
        }
        public Control GetPresentationControl()
        {
            double value = 0;
            GetValue(ref value);
            cntrl.Value = (int)(value * 50) + 50;

            return cntrl;
        }
    }
    class GamepadButtonInput : PresentationInput
    {
        Guid m_gamepad_guid;
        uint m_btn_idx;
        string m_name;

        Panel cntrl;
        bool disconnected;

        public GamepadButtonInput(Guid gamepad_guid, uint btn_idx, string name)
        {
            m_gamepad_guid = gamepad_guid;
            m_btn_idx = btn_idx;
            m_name = name;

            cntrl = new Panel();

            disconnected = false;
        }
        public int GetValue(ref double value)
        {
            value = 0;
            if (Gamepad.GamepadManager.GetButton(m_gamepad_guid, m_btn_idx, ref value) < 0)
            {
                //something has gone wrong with gamepad
                disconnected = true;
                return -1;
            }
            else
                disconnected = false;
            return 0;
        }
        public string GetLastError()
        {
            if (disconnected)
                return "Gamepad disconnected";
            return "";
        }
        public override string ToString()
        {
            return m_name;
        }
        public string GetUniqueString()
        {
            return m_name + " " + m_gamepad_guid.ToString();
        }
        public Control GetPresentationControl()
        {
            double val=0;
            GetValue(ref val);
            if (val > 0)
                cntrl.BackColor = System.Drawing.Color.Blue;
            else
                cntrl.BackColor = System.Drawing.Color.Transparent;
            return cntrl;
        }
    }

    class _2CANInputFactory : InputFactory
    {
        private static _2CANInputFactory me = null;
        private CSharpRobotController.RobotController _rc;

        public _2CANInputFactory()
        {
            _rc = null;
        }

        public void SetRC(CSharpRobotController.RobotController rc)
        {
            _rc = rc;
        }

        public CSharpRobotController.RobotController GetRC()
        {
            return _rc;
        }

        public static _2CANInputFactory GetInstance()
        {
            if (me == null)
                me = new _2CANInputFactory();
            return me;
        }

        public PresentationInput[] GetInputs()
        {
            List<PresentationInput> ret = new List<PresentationInput>();

            for (int i = 1; i <= 8; ++i)
                ret.Add(new AnalogInput(i));

            for (int i = 1; i <= 4; ++i)
                ret.Add(new DigitalInput(i));

            for (int i = 1; i <= 4; ++i)
                ret.Add(new QuadraturePositionInput(i));

            for (int i = 1; i <= 4; ++i)
                ret.Add(new QuadratureVelocityInput(i));

            return ret.ToArray();
        }
    }

    public class NoInput : PresentationInput
    {
        public NoInput()
        {
        }
        public override string ToString()
        {
            return "Not Selected";
        }
        public int GetValue(ref double value)
        {
            value = 0;
            return 0;
        }
        public Control GetPresentationControl()
        {
            return null;
        }
        public string GetLastError()
        {
            return "";
        }
        public string GetUniqueString()
        {
            return ToString();
        }

    }

    //Inputs based on Visual Componenets
    public class SliderInput : PresentationInput
    {
        TrackBar _tb = new TrackBar();
        protected int _idx;

        public SliderInput(int idx)
        {
            _idx = idx;
            _tb.Maximum = 200;
            _tb.Minimum = 0;
            _tb.Value = (_tb.Maximum + _tb.Minimum) / 2;
        }
        public int GetValue(ref double value)
        {
            double d = _tb.Value;    //  [0,200]
            d -= 100;                // [-100,100]
            d /= 100.0;              //  [-1,1]
            value = d;
            return 0;
        }
        override public string ToString()
        {
            return "Slider " + _idx;
        }
        public string GetUniqueString()
        {
            return ToString();
        }
        public string GetLastError()
        {
            return "";
        }
        public System.Windows.Forms.Control GetPresentationControl()
        {
            return _tb;
        }
    }

    public class ButtonInput : PresentationInput
    {
        Button _btn = new Button();
        protected int _idx;
        protected double _value;
        public ButtonInput(int idx)
        {
            _idx = idx;
            _value = 0;
            _btn.MouseDown += new MouseEventHandler(_btn_MouseDown);
            _btn.MouseUp += new MouseEventHandler(_btn_MouseUp);
            _btn.BackColor = System.Drawing.Color.WhiteSmoke;
            _btn.Text = "Button " + _idx;
        }
        public double GetValue()
        {
            return _value;
        }
        public int GetValue(ref double value)
        {
            value = _value;
            return 0;
        }

        void _btn_MouseUp(object sender, MouseEventArgs e)
        {
            _value = 0;
        }

        void _btn_MouseDown(object sender, MouseEventArgs e)
        {
            _value = 1;
        }

        override public string ToString()
        {
            return "Button " + _idx;
        }
        public string GetUniqueString()
        {
            return ToString();
        }
        public string GetLastError()
        {
            return "";
        }
        public System.Windows.Forms.Control GetPresentationControl()
        {
            return _btn;
        }
    }

    public abstract class AbstractInput : PresentationInput
    {
        private CTR.Library.Input _input;

        protected string _prefix;
        protected int _idx;

        public AbstractInput(CTR.Library.Input input)
        {
            _input = input;
        }
        public override string ToString()
        {
            return _input.ToString();
        }
        public int GetValue(ref double value)
        {
            value = _input.Get();
            return 0;
        }
        public virtual Control GetPresentationControl()
        {
            return null;
        }
        public virtual string GetLastError()
        {
            return "";
        }
        public string GetUniqueString()
        {
            return ToString();
        }
    }
    public class AnalogInput : InputDisplaysProgressBar
    {
        public AnalogInput(int idx)
            : base(idx)
        {
            _progressBar = new ProgressBar();
            _progressBar.Minimum = 0;
            _progressBar.Value = 50;
            _progressBar.Maximum = 100;
            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.MarqueeAnimationSpeed = 1;
        }
        public override int GetValue(ref double value)
        {
            //value = _2CANInputFactory.GetInstance().GetRC().GetADC(_idx - 1);
            return 0;
        }
    }

    public class DigitalInput : AbstractInput
    {
        public DigitalInput(int idx)
            : base(idx)
        {
            _prefix = "RCM : Digital";
        }
        public override int GetValue(ref double value)
        {
            uint i = _2CANInputFactory.GetInstance().GetRC().GetGPIO() >> (_idx - 1);
            i &= 1;
            value = (double)i;
            return 0;
        }
    }

    public class QuadraturePositionInput : AbstractInput
    {
        public QuadraturePositionInput(int idx)
            : base(idx)
        {
            _prefix = "RCM : Encoder Position";
        }
        public override int GetValue(ref double value)
        {
            value = _2CANInputFactory.GetInstance().GetRC().GetPosition(_idx - 1);
            return 0;
        }
    }

    public class QuadratureVelocityInput : AbstractInput
    {
        public QuadratureVelocityInput(int idx)
            : base(idx)
        {
            _prefix = "RCM : Encoder Velocity";
        }
        public override int GetValue(ref double value)
        {
            value = (double)_2CANInputFactory.GetInstance().GetRC().GetVelocity(_idx - 1);
            return 0;
        }
    }
}
