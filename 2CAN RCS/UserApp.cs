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
using System.Threading;

namespace CtrElectronics.CrossLinkControlSystem
{
    class UserApp
    {
        Object lk;
        bool enabled;
        Thread m_UserThread = null;
        Form1 _frm = null;
        public UserApp( Form1 frm)
        {
            _frm = frm;
            lk = new Object();
            enabled = false;
        }

        public void SignalStart()
        {
            lock (lk)
            {
                enabled = true;
            }

            m_UserThread = new Thread(new ThreadStart(Loop));
            m_UserThread.Start();
        }
        public void SignalStop()
        {
            lock (lk)
            {
                enabled = false;
            }
            Thread.Sleep(100);

            if (m_UserThread != null)
                if(m_UserThread.IsAlive)
                {
                    m_UserThread.Abort();
                }
        }

        public bool IsEnabled()
        {
            bool b = false;

            lock (lk)
            {
                b = enabled;
            }

            return b;
        }

        void Loop()
        {
            while (IsEnabled())
            {
                _frm.RunIOEngine();
                Thread.Sleep(10);
            }
        }
    }
}
