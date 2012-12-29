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
    public interface PresentationOutput : CTR.Library.InputOutput
    {
        Control GetPresentationControl();
    }

    public class OutputNoGuiWrapper : PresentationOutput
    {
        CTR.Library.InputOutput _inputOutput;

        public OutputNoGuiWrapper(CTR.Library.InputOutput inputOutput)
        {
            _inputOutput = inputOutput;
        }
        public Control GetPresentationControl()
        {
            return null;
        }
        public void Set(double value)
        {
            _inputOutput.Set(value);
        }
        public double Get()
        {
            return _inputOutput.Get();
        }
        public override string ToString()
        {
            return _inputOutput.ToString();
        }
    }

    public interface OutputFactory
    {
        PresentationOutput[] GetOutputs();
    }

    class _2CANOutputFactory : OutputFactory
    {
        private static _2CANOutputFactory me = null;
        private CSharpRobotController.RobotController _rc;

        public _2CANOutputFactory()
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

        public static _2CANOutputFactory GetInstance()
        {
            if (me == null)
                me = new _2CANOutputFactory();
            return me;
        }

        public PresentationOutput[] GetOutputs()
        {
            List<PresentationOutput> ret = new List<PresentationOutput>();

            for(int i=1;i<=16;++i)
                ret.Add(new OutputNoGuiWrapper(new CTR.Library.Jaguar(i)));

            for (int i = 1; i <= 8; ++i)
                ret.Add( new OutputNoGuiWrapper(new CTR.Library.Victor(1,i)));

            for (int i = 1; i <= 8; ++i)
                ret.Add( new OutputNoGuiWrapper(new CTR.Library.Solenoid(i)));

            for (int i = 1; i <= 4; ++i)
                ret.Add( new OutputNoGuiWrapper(new CTR.Library.Relay(i)));

            return ret.ToArray();
        }
    }


    class NoOutput : PresentationOutput
    {
        public void Set(double value)
        {
        }
        public double Get()
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
    }
}
