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
    public interface Output
    {
        void SetValue(double value);
        string ToString();

        double GetValue();
        System.Windows.Forms.Control GetPresentationControl();
        UInt64 GetOutputEnable();
    }

    public interface OutputFactory
    {
        Output[] GetOutputs();
    }

    class _2CANOutputFactory : OutputFactory
    {
        private static _2CANOutputFactory me = null;
        private CtrElectronics.CrossLinkControlSystem.RobotController _rc;

        public _2CANOutputFactory()
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

        public static _2CANOutputFactory GetInstance()
        {
            if (me == null)
                me = new _2CANOutputFactory();
            return me;
        }

        public Output[] GetOutputs()
        {
            List<Output> ret = new List<Output>();

            for(int i=1;i<=16;++i)
                ret.Add(new JaguarOutput(i));

            for (int i = 1; i <= 8; ++i)
                ret.Add(new PwmOutput(i));

            for (int i = 1; i <= 8; ++i)
                ret.Add(new SolenoidOutput(i));

            for (int i = 1; i <= 4; ++i)
                ret.Add(new RelayOutput(i));

            return ret.ToArray();
        }
    }


    class NoOutput : Output
    {
        public void SetValue(double value)
        {
        }
        public double GetValue()
        {
            return 0;
        }

        public override string ToString()
        {
            return "Not Selected";
        }

        public Control GetPresentationControl()
        {
            return null;
        }
        public UInt64 GetOutputEnable()
        {
            return 0;
        }
    }
    abstract class AbstractOutput : Output
    {
        private double _value;

        protected string _prefix;
        protected int _idx;

        public void SetValue(double value)
        {
            _value = value;
            SetValue2(value);
        }
        public double GetValue()
        {
            return _value;
        }

        public override string ToString()
        {
            return _prefix + " " + _idx;
        }
        public abstract void SetValue2(double value);

        public Control GetPresentationControl()
        {
            return null;
        }
        public abstract UInt64 GetOutputEnable();
    }
    class JaguarOutput : AbstractOutput
    {
        public JaguarOutput(int idx) 
        {
            _idx = idx;
            _prefix = "Jaguar";
        }
        public override void SetValue2(double value)
        {
            _2CANOutputFactory.GetInstance().GetRC().SetJaguarPwm(value, _idx);
        }
        public override UInt64 GetOutputEnable()
        {
            return 1ul << ( (int)DLL.OutputEnable.OUTPUT_ENABLE_JAG1 + _idx - 1);
        }
    }

    class PwmOutput : AbstractOutput
    {
        public PwmOutput(int idx)
        {
            _idx = idx;
            _prefix = "PWM";
        }
        public override void SetValue2(double value)
        {
            _2CANOutputFactory.GetInstance().GetRC().SetRcmPwm(value, _idx - 1);
        }
        public override UInt64 GetOutputEnable()
        {
            return 1ul << ((int)DLL.OutputEnable.OUTPUT_ENABLE_PWM1 + _idx - 1);
        }
    }

    class SolenoidOutput : AbstractOutput
    {
        public SolenoidOutput(int idx)
        {
            _idx = idx;
            _prefix = "Solenoid";
        }
        public override void SetValue2(double value)
        {
            value = Util.Deadband(value, .1);
            _2CANOutputFactory.GetInstance().GetRC().SetRCM_Solenoid(value == 0 ? false : true, _idx - 1);
        }
        public override UInt64 GetOutputEnable()
        {
            return 1ul << ((int)DLL.OutputEnable.OUTPUT_ENABLE_SOLENOID1 + _idx - 1);
        }
    }
    class RelayOutput : AbstractOutput
    {
        public RelayOutput(int idx)
        {
            _idx = idx;
            _prefix = "Relay";
        }
        public override void SetValue2(double value)
        {
            byte s = 0;
            value = Util.Deadband(value, .1);
            if (value > 0)
                s = 1;
            if (value < 0)
                s = 2;
            _2CANOutputFactory.GetInstance().GetRC().SetRCM_Relay(s, _idx - 1);
        }
        public override UInt64 GetOutputEnable()
        {
            return 1ul << ((int)DLL.OutputEnable.OUTPUT_ENABLE_RELAY2 + _idx - 1);
        }
    }


    
}
