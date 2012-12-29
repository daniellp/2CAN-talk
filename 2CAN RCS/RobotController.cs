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
using System.Collections;
using System.Text;

using System.Net;
using System.Net.Sockets;

using System.Runtime.InteropServices;
using System.Threading;

namespace CtrElectronics.CrossLinkControlSystem
{

    class RobotController
    {
        UInt32 _handle = 0;
        int _rcmNodeId = 1;

        DLL.ctr_error_t _lastError = DLL.ctr_error_t.ctr_ok;
        bool _isConnected;
        DLL.CommStats _comStats;

        public RobotController()
        {
            _isConnected = false;
            _comStats =  new DLL.CommStats();
        }

        public void SetRcmNodeId(int nodeId)
        {
            if (nodeId > 0) // must be nonzero
                _rcmNodeId = nodeId;
        }
        public int GetRcmNodeId()
        {
            return _rcmNodeId;
        }
        DLL.ctr_error_t HandleReturn(DLL.ctr_error_t er)
        {
            _lastError = er;
            if (_lastError != DLL.ctr_error_t.ctr_ok)
            {
                //TODO present error codes in status bar 
            }
            return er;
        }
        public bool Connect(string sIp)
        {
            if (_isConnected)
                Disconnect();

            if (HandleReturn(DLL.CTR_Init(sIp, ref _handle)) == DLL.ctr_error_t.ctr_ok)
            {
                _isConnected = true;
            }
            else
                _isConnected = false;

            if(_isConnected)
                DLL.CTR_SetActiveRcmNodeId(_handle, _rcmNodeId);

            return _isConnected;
        }
        public bool Disconnect()
        {
            Disable();
            HandleReturn(DLL.CTR_Close(_handle));
            _handle = 0;
            _isConnected = false;
            return true;
        }
        public bool IsConnected()
        {
            return _isConnected;
        }

        public bool Enable(DLL.EnableState enableState, UInt64 outputEnableBits)
        {
            if (_isConnected == false)
                return false;
            return HandleReturn(DLL.CTR_Enable(_handle, enableState, outputEnableBits)) == DLL.ctr_error_t.ctr_ok;
        }

        public bool Disable()
        {
            if (_isConnected == false)
                return false;
            return (HandleReturn(DLL.CTR_Enable(_handle, DLL.EnableState.Disabled, 0)) == DLL.ctr_error_t.ctr_ok);          
        }

        public void SetJaguarPwm(double dThrottle, int jagId)
        {
            short pulseWidth;
            dThrottle = (dThrottle > 1) ? 1 : dThrottle;
            dThrottle = (dThrottle < -1) ? -1 : dThrottle;

            pulseWidth = (short)(dThrottle * 0x7FFF);

            HandleReturn(DLL.CTR_SetPWM(_handle,
                                        DLL.Node.Jaguar,
                                        0,
                                        jagId - 1,
                                        pulseWidth));
        }
        public void SetRcmPwm(double dThrottle, int channelIndex)
        {
            double forward = 2.1;
            double center = 1.50;
            double reverse = 1.00;
            double ms;
            
            dThrottle = (dThrottle > 1) ? 1 : dThrottle;
            dThrottle = (dThrottle < -1) ? -1 : dThrottle;

            if (dThrottle > 0)
            {
                ms = center + dThrottle * (forward - center);
            }
            else
            {
                ms = center + dThrottle * (center - reverse);
            }

            short pulseWidth = (short)(ms * 1e6 / 200);

            HandleReturn(DLL.CTR_SetPWM(_handle,
                                        DLL.Node.RCM,
                                        _rcmNodeId,
                                        channelIndex,
                                        pulseWidth));
        }
        public void SetRCM_Solenoid(bool enable, int id)
        {
            DLL.CTR_SetSolenoid(_handle, DLL.Node.RCM, _rcmNodeId, id, enable);
        }
        public void SetRCM_Relay(byte state, int id)
        {
            DLL.CTR_SetRelay(_handle, DLL.Node.RCM, _rcmNodeId, id, state);
        }

        public int GetADC(int channel)
        {
            UInt32 result=0;
            DLL.CTR_GetADC(_handle, DLL.Node.RCM, _rcmNodeId, channel, ref result);
            return (int)result;
        }
        public Int32 GetPosition(int channel)
        {
            UInt32 result=0;
            DLL.CTR_GetPosition(_handle, DLL.Node.RCM, _rcmNodeId, channel, ref result);
            return (int)result;
        }
        public Int32 GetVelocity(int channel)
        {
            UInt32 result=0;
            DLL.CTR_GetVelocity(_handle,DLL.Node.RCM,_rcmNodeId, channel, ref result);
            return (int)result;
        }
        public UInt32 GetGPIO()
        {
            UInt32 result=0;
            DLL.CTR_GetGPIO(_handle, DLL.Node.RCM, _rcmNodeId, ref result);
            return result;
        }
        public DLL.CommStats GetCommStats()
        {
            DLL.ctr_error_t err = HandleReturn(DLL.CTR_GetCommStats(_handle, ref _comStats));
            //return (err == DLL.ctr_error_t.ctr_ok) ? _comStats : null;
            return _comStats; 
        }
    }
}
