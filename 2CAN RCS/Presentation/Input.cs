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
namespace CtrElectronics.CrossLinkControlSystem.Presentation
{
    public interface Input
    {
        int GetValue(ref double value);
        string ToString();
        string GetUniqueString();
        string GetLastError();
        Control MakePresentationControl();
        void UpdatePresentationControl(Control obj);
    }

    public interface InputFactory
    {
        Input[] GetInputs();
    }
    
    public class GamepadInputFactory 
    {
        static Gamepad.SearchResult[] searchResults = null;
        public static void Update()
        {
            searchResults = Gamepad.GamepadFinder.FindInputs();
        }

        public static Input[] GetInputs()
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

    class GamepadAxisInput : Input
    {
        Guid m_gamepad_guid;
        uint m_axis_idx;
        string m_name;

        bool disconnected;

        public GamepadAxisInput(Guid gamepad_guid, uint axis_idx, string name)
        {
            m_gamepad_guid = gamepad_guid;
            m_axis_idx = axis_idx;
            m_name = name;

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
        public Control MakePresentationControl()
        {
            PercentProgressBar cntrl = new PercentProgressBar();
            cntrl.Percent = 50;
            cntrl.Style = ProgressBarStyle.Blocks;
            return cntrl;
        }
        public void UpdatePresentationControl(Control obj)
        {
            PercentProgressBar cntrl = (PercentProgressBar)obj;
            double value = 0;
            GetValue(ref value);
            cntrl.Percent = (int)(value * 50) + 50;
        }
    }
    class GamepadButtonInput : Input
    {
        Guid m_gamepad_guid;
        uint m_btn_idx;
        string m_name;

        bool disconnected;

        public GamepadButtonInput(Guid gamepad_guid, uint btn_idx, string name)
        {
            m_gamepad_guid = gamepad_guid;
            m_btn_idx = btn_idx;
            m_name = name;

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
        public Control MakePresentationControl()
        {
            Panel cntrl = new Panel();
            return cntrl;
        }
        public void UpdatePresentationControl(Control obj)
        {
            Panel cntrl = (Panel)obj;
            double val = 0;
            GetValue(ref val);
            if (val > 0)
                cntrl.BackColor = System.Drawing.Color.Blue;
            else
                cntrl.BackColor = System.Drawing.Color.Transparent;
        }
    }

    class _2CANInputFactory : InputFactory
    {
        private static _2CANInputFactory me = null;
        private CtrElectronics.CrossLinkControlSystem.RobotController _rc;

        public _2CANInputFactory()
        {
            _rc = null;
        }

        public void SetRC(CtrElectronics.CrossLinkControlSystem.RobotController rc)
        {
            _rc = rc;
        }

        public CtrElectronics.CrossLinkControlSystem.RobotController GetRC()
        {
            return _rc;
        }

        public static _2CANInputFactory GetInstance()
        {
            if (me == null)
                me = new _2CANInputFactory();
            return me;
        }

        public Input[] GetInputs()
        {
            List<Input> ret = new List<Input>();

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

    public abstract class AbstractInput : Input
    {
        protected string _prefix;
        protected int _idx;

        public AbstractInput(int idx)
        {
            _idx = idx;
        }
        public override string ToString()
        {
            return _prefix + " " + _idx;
        }
        public abstract int GetValue(ref double value);

        public virtual Control MakePresentationControl()
        {
            return null;
        }
        public virtual void UpdatePresentationControl(Control obj)
        {
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


    public class NoInput : Input
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
        public Control MakePresentationControl()
        {
            return null;
        }
        public void UpdatePresentationControl(Control obj)
        {
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

    public class AnalogInput : AbstractInput
    {
        public AnalogInput(int idx) : base(idx)
        {
            _prefix = "Analog";
        }
        public override int GetValue(ref double value)
        {
            value = _2CANInputFactory.GetInstance().GetRC().GetADC(_idx - 1);
            return 0;
        }

        public override Control MakePresentationControl()
        {
            PercentProgressBar cntrl = new PercentProgressBar();
            cntrl.Percent = 50;
            cntrl.Style = ProgressBarStyle.Blocks;
            return cntrl;
        }
        public override void UpdatePresentationControl(Control obj)
        {
            PercentProgressBar cntrl = (PercentProgressBar)obj;
            double d = 0;
            GetValue(ref d);
            d -= 512;
            d /= 512.0;

            try
            {
                cntrl.Percent = (int)(d * 50) + 50;
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }
    }

    public class DigitalInput : AbstractInput
    {
        public DigitalInput(int idx)
            : base(idx)
        {
            _prefix = "Digital";
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
            _prefix = "Encoder Position";
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
            _prefix = "Encoder Velocity";
        }
        public override int GetValue(ref double value)
        {
            value = (double)_2CANInputFactory.GetInstance().GetRC().GetVelocity(_idx - 1);
            return 0;
        }
    }

    public class SliderInput : Input
    {
        TrackBar _tb = new TrackBar();
        protected int _idx;

        Object _lk = new Object();
        double _value;

        public SliderInput(int idx)
        {
            _idx = idx;
            _tb.Maximum = 200;
            _value = 100;
            _tb.Minimum = 0;
            _tb.Value = (_tb.Maximum + _tb.Minimum) / 2;
            _tb.ValueChanged += new EventHandler(ValueChanged);
        }
        public int GetValue(ref double value)
        {
            double d;

            lock (_lk)
            {
                d = _value;
            }
                                    //  [0,200]
            d -= 100;               // [-100,100]
            d /= 100.0;             //  [-1,1]
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
        public Control MakePresentationControl()
        {
            return _tb;
        }
        public void UpdatePresentationControl(Control obj)
        {
        }
        void ValueChanged(object sender, EventArgs e)
        {
            lock (_lk)
            {
                _value = _tb.Value;
            }
        }
    }

    public class ButtonInput : Input
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
        public Control MakePresentationControl()
        {
            return _btn;
        }
        public void UpdatePresentationControl(Control obj)
        {
            Button btn = (Button)obj;
        }
    }


    public class MecanumInput : Input
    {
        Label _label = new Label();
        Input _forwardInput = null;
        Input _strafeInput = null;
        Input _turnInput = null;

        WireSegment _forwardWs;
        WireSegment _strafeWs;
        WireSegment _turnWs;

        public enum MecanumInputType{
            eFrontLeft,
            eFrontRight,
            eRearLeft,
            eRearRight,
        };

        protected MecanumInputType _type;

        public MecanumInput(MecanumInputType type)
        {
            _type = type;
        }
        public void SetInputs(  Input forwardInput,
                                Input strafeInput, 
                                Input turnInput)
        {
            _forwardInput = forwardInput;
            _strafeInput = strafeInput;
            _turnInput = turnInput;
        }
        public void SetWireSegs(WireSegment forwardWs,
                                WireSegment strafeWs,
                                WireSegment turnWs)
        {
            _forwardWs = forwardWs;
            _strafeWs = strafeWs;
            _turnWs = turnWs;
        }
        public int GetValue(ref double value)
        {
            /* pull the gamepad values from the user selected list */
            double mecTurnValue = 0;
            double mecStrafe = 0;
            double mecForward = 0;

            if (!Util.CalcInput(_forwardWs, _forwardInput, ref mecForward))
            {
                value = 0;
                return -1; /* signal caller we have bad input */
            }
            if (!Util.CalcInput(_strafeWs, _strafeInput, ref mecStrafe))
            {
                value = 0;
                return -1; /* signal caller we have bad input */
            }
            if (!Util.CalcInput(_turnWs, _turnInput, ref mecTurnValue))
            {
                value = 0;
                return -1; /* signal caller we have bad input */
            }
            /* compute using the appropriate math */
            switch(_type){
                case MecanumInputType.eFrontLeft:   value = mecForward + mecStrafe + mecTurnValue;  break;
                case MecanumInputType.eFrontRight:  value = mecForward - mecStrafe - mecTurnValue;  break;
                case MecanumInputType.eRearLeft:    value = mecForward - mecStrafe + mecTurnValue;  break;
                case MecanumInputType.eRearRight:   value = mecForward + mecStrafe - mecTurnValue;  break;
                default: value = 0; break;
            }
            return 0;
        }
        
        override public string ToString()
        {
            switch(_type){
                case MecanumInputType.eFrontLeft: return "Mecanum Front Left Input";
                case MecanumInputType.eFrontRight: return "Mecanum Front Right Input";
                case MecanumInputType.eRearLeft: return "Mecanum Rear Left Input";
                case MecanumInputType.eRearRight: return "Mecanum Rear Right Input";
            }
            return "Invalid Type";
        }

        public string GetUniqueString()
        {
            return ToString();
        }
        public string GetLastError()
        {
            return "";
        }
        public Control MakePresentationControl()
        {
            return _label;
        }
        public void UpdatePresentationControl(Control obj)
        {
        }
    }
}
