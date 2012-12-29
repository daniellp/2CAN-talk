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
using System.Runtime.InteropServices;

namespace CtrElectronics.CrossLinkControlSystem
{
    public class DLL
    {
        public enum ctr_error_t
        {
	        ctr_ok,
	        ctr_fail,
	        ctr_not_implemented,
	        ctr_bad_handle,
        };
        public enum Node
        {
            Jaguar,
            RCM,
            Gyro,
        };
        public enum EnableState
        {
	        Disabled,
	        Teleop,
	        Auton,
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CommStats
        {
            public UInt32 rxCount;
            public UInt32 txCount;
            public UInt32 rxTimeFromLastMs;
            public UInt32 statusFlags;
            UInt32 reserved_0;
            UInt32 reserved_1;
            UInt32 reserved_2;
            UInt32 reserved_3;
            UInt32 reserved_4;
            UInt32 reserved_5;
            UInt32 reserved_6;
            UInt32 reserved_7;
            UInt32 reserved_8;
        };
		/**
		 * Pull out GetKeyState  from Windows API.  
		 * There doesn't seem to be a good .NET replacement.
		 * @see http://msdn.microsoft.com/en-us/library/windows/desktop/ms646301(v=vs.85).aspx
		 */
        [DllImport("user32.dll")]
        public static extern short GetKeyState(UInt32 keyState);

        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_GetDllVersion(ref UInt32 version);

        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_Init([MarshalAs(UnmanagedType.LPStr)]  string ip, ref UInt32 handle);
        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_Close(UInt32 handle);
        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_Enable(UInt32 handle, EnableState enable, UInt64 outputEnableBits);
        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_SetPWM(UInt32 handle,
                                                        Node target,
                                                        int param, 
                                                        int pwmChannel, 
                                                        short pwmWidth);
        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_GetADC(    UInt32 handle,
                                                        Node target,
                                                        int param,
                                                        int channel,
                                                        ref UInt32 result);
        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_GetPosition(   UInt32 handle,
                                                            Node target,
                                                            int param,
                                                            int channel,
                                                            ref UInt32 result);
        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_GetVelocity(   UInt32 handle,
                                                            Node target,
                                                            int param,
                                                            int channel,
                                                            ref UInt32 result);

        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_SetSolenoid(UInt32 handle,
                                                            Node target,
                                                            int param,
                                                            int channel, 
                                                            bool enable);
        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_SetRelay(UInt32 handle,
                                                            Node target,
                                                            int param,
                                                            int channel, 
                                                            byte state);
        
        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_GetGPIO(   UInt32 handle,
                                                        Node target,
                                                        int param,
                                                        ref UInt32 result);

        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_SetActiveRcmNodeId(UInt32 handle,
                                                                int nodeId);

        [DllImport("2CAN_Comm.dll")]
        public static extern ctr_error_t CTR_GetCommStats(  UInt32 handle,
                                                            ref CommStats commStats);


        public enum OutputEnable
        {
	        OUTPUT_ENABLE_JAG1,
	        OUTPUT_ENABLE_JAG2,
	        OUTPUT_ENABLE_JAG3,
	        OUTPUT_ENABLE_JAG4,
	        OUTPUT_ENABLE_JAG5,
	        OUTPUT_ENABLE_JAG6,
	        OUTPUT_ENABLE_JAG7,
	        OUTPUT_ENABLE_JAG8,
	        OUTPUT_ENABLE_JAG9,
	        OUTPUT_ENABLE_JAG10,
	        OUTPUT_ENABLE_JAG11,
	        OUTPUT_ENABLE_JAG12,
	        OUTPUT_ENABLE_JAG13,
	        OUTPUT_ENABLE_JAG14,
	        OUTPUT_ENABLE_JAG15,
	        OUTPUT_ENABLE_PWM1,
	        OUTPUT_ENABLE_PWM2, // 16
	        OUTPUT_ENABLE_PWM3,
	        OUTPUT_ENABLE_PWM4,
	        OUTPUT_ENABLE_PWM5,
	        OUTPUT_ENABLE_PWM6,
	        OUTPUT_ENABLE_PWM7,
	        OUTPUT_ENABLE_PWM8,
	        OUTPUT_ENABLE_SOLENOID1,
	        OUTPUT_ENABLE_SOLENOID2,
	        OUTPUT_ENABLE_SOLENOID3,
	        OUTPUT_ENABLE_SOLENOID4,
	        OUTPUT_ENABLE_SOLENOID5,
	        OUTPUT_ENABLE_SOLENOID6,
	        OUTPUT_ENABLE_SOLENOID7,
	        OUTPUT_ENABLE_SOLENOID8,
	        OUTPUT_ENABLE_RELAY1,
	        OUTPUT_ENABLE_RELAY2, // 32
	        OUTPUT_ENABLE_RELAY3,
	        OUTPUT_ENABLE_RELAY4
        };
    }
}
