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
using Microsoft.DirectX.DirectInput; // This 
using System.Timers;
using System.Threading;

namespace CtrElectronics.CrossLinkControlSystem.Gamepad
{
    public class SearchResult
    {
        public Guid gamepad_guid;
        public string gamepad_name;
        public uint axis_count;
        public uint btn_count;
        public string []axis_name;
    };

    public class GamepadFinder
    {
        static SearchResult[] _SearchResult = null;

        private static IntPtr m_hWnd = (IntPtr)0;

        public static SearchResult[] FindInputs()
        { 
            DeviceList deviceList = null;

            _SearchResult = new SearchResult[0];

            try
            {
                /*
                 * There is a known issue with debugging when using this version of Direct X 
                 * where the debugger halts on Loader Lock exceptions.  To bypass this go to the 
                 * Debug menu option, then Exceptions, then under "Managed Debugging Assistants" 
                 * uncheck the "on Loader Lock" option.
                 */
                deviceList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            }
            catch (Exception)
            {
            }
            if (deviceList.Count > 0)
            {
                uint i = 0;
                _SearchResult = new SearchResult[deviceList.Count];
                foreach (DeviceInstance deviceInstance in deviceList)
                {
                    _SearchResult[i] = new SearchResult();
                    try
                    {
                        _SearchResult[i].gamepad_guid = deviceInstance.InstanceGuid;
                        _SearchResult[i].gamepad_name = deviceInstance.InstanceName;
                        Device m_DXDevice = new Device(deviceInstance.InstanceGuid);
                        m_DXDevice.SetCooperativeLevel(m_hWnd, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                        m_DXDevice.SetDataFormat(DeviceDataFormat.Joystick);

                        _SearchResult[i].axis_count = (uint)m_DXDevice.Caps.NumberAxes;
                        _SearchResult[i].axis_name = new string[m_DXDevice.Caps.NumberAxes];
                        FillAxisNames(  ref _SearchResult[i].axis_name, 
                                        _SearchResult[i].axis_count);
                        _SearchResult[i].btn_count = (uint)m_DXDevice.Caps.NumberButtons;

                        m_DXDevice = null;
                    }
                    catch(Exception)
                    {
                        _SearchResult[i].gamepad_guid = Guid.Empty;
                        _SearchResult[i].axis_count = 0;
                        _SearchResult[i].btn_count = 0;
                        _SearchResult[i].axis_name = null;
                    }
                    ++i;
                }
            }

            return _SearchResult;
        }

        public static bool CheckConnection(Guid g)
        {
            DeviceList deviceList = null;

            try
            {
                deviceList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            }
            catch (Exception)
            {
                return false;
            }
            foreach (DeviceInstance deviceInstance in deviceList)
            {
                if (deviceInstance.InstanceGuid.Equals(g) == true)
                    return true;
            }
            return false;
        }

        private static void FillAxisNames(ref string [] names, uint iNumAxes)
        {
			/* Hardcoded list for now.  Maybe later we can ask 
				DirectX for a comprehensive list of descriptions */			
            if (iNumAxes > 0)
                names[0] = "X Axis";

            if (iNumAxes > 1)
                names[1] = "Y Axis";

            if (iNumAxes > 2)
                names[2] = "Z Axis";

            if (iNumAxes > 3)
                names[3] = "X Rotate";

            if (iNumAxes > 4)
                names[4] = "Y Rotate";

            for (int i = 5; i < iNumAxes; ++i)
            {
                if (iNumAxes > i)
                    names[i] = "Axis " + i;
            }
        }
    }
    class GamepadManager
    {
        static Dictionary<Guid, Guid> _guids_to_acquire = new Dictionary<Guid, Guid>();

        static Dictionary<Guid, Device> _gamepads = new Dictionary<Guid, Device>();

        static System.Timers.Timer _timer = new System.Timers.Timer();

        static private IntPtr m_hWnd = (IntPtr)0;

        static Thread _acqireThrd = null;
        static ManualResetEvent _stopAcquireThrd = new ManualResetEvent(false);
        static ManualResetEvent _hasStoppedAcquireThrd = new ManualResetEvent(false);

        static GamepadManager()
        {
            _timer.Interval=30;
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.Enabled=false;
        }
        public static void AcquireTask()
        {
            while (true)
            {
                if (_stopAcquireThrd.WaitOne(100))
                    return;

                lock (_guids_to_acquire)
                {
                    foreach (KeyValuePair<Guid, Guid> pair in _guids_to_acquire)
                    {
                        BeginAcquireBlocking( pair.Value );
                    }
                }
            }
        }
        public static int BeginAcquireBlocking(Guid g)
        {
            Device d = null;
            _gamepads.TryGetValue(g, out d);

            if (d != null) // already start working on this object
                return 0;
          
            try
            {
                SearchResult[] searchResults = Gamepad.GamepadFinder.FindInputs();


                d = new Device(g);
            }
            catch (Exception)
            {
                return -1;
            }
            d.SetCooperativeLevel(m_hWnd, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            d.SetDataFormat(DeviceDataFormat.Joystick);
            d.Acquire();
            _gamepads.Add(g, d);
            return 0;
        }
        public static int BeginAcquire(Guid g)
        {
            try
            {
                if (Monitor.TryEnter(_guids_to_acquire))
                {
                    try
                    {
                        if(_guids_to_acquire.ContainsKey(g) == false)
                            _guids_to_acquire.Add(g, g);
                    }
                    finally { Monitor.Exit(_guids_to_acquire); }
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }

        public static void StopStream()
        {
            foreach (KeyValuePair<Guid, Device> pair in _gamepads)
            {
                pair.Value.Unacquire();
            }
            _gamepads.Clear();

            if (_acqireThrd != null)
            {
                _stopAcquireThrd.Set();
                _hasStoppedAcquireThrd.WaitOne(10);
                _acqireThrd = null;
                lock (_guids_to_acquire)
                {
                    _guids_to_acquire.Clear();
                }
            }
        }

        public static void StartDataStream()
        {
            _stopAcquireThrd.Reset();
            _hasStoppedAcquireThrd.Reset();
            _acqireThrd = new Thread(new ThreadStart(GamepadManager.AcquireTask));
            _acqireThrd.Start();
        }

        private static void RemoveGuid(Guid g)
        {
            Device d = null;
            _gamepads.TryGetValue(g, out d);

            if (d == null)
                return;

            d.Unacquire();
            _gamepads.Remove(g);
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            ((GamepadManager)source).OnTimedEvent();
        }
        private void OnTimedEvent()
        {
            foreach (KeyValuePair<Guid, Device> pair in _gamepads)
            {
                pair.Value.Poll();
            }
        }
        public static int GetAxis(Guid g, uint axis_idx, ref double value)
        {
            Device d = null;
            _gamepads.TryGetValue(g,out d);

            if (d == null)
            {
                int err = BeginAcquire(g);
                err = -1;
                value = 0;
                return err;
            }
            try
            {
                value = GetAxis(d.CurrentJoystickState, axis_idx);
                return 0;
            }
            catch (Exception)
            {
                value = 0;
                RemoveGuid(g);
            }
            return -1;
        }

        private static double Scale(int i)
        {
            i -= 0x8000;
            return (double)i / (double)32767;
        }
        private static double GetAxis(JoystickState state, uint axis_idx)
        {
            switch (axis_idx)
            {
                case 0:
                    return Scale(state.X);
                case 1:
                    return Scale(state.Y);
                case 2:
                    return Scale(state.Z);
                //case 3:
                //    return Scale(state.Rz);
                case 3:
                    return Scale(state.Rx);
                case 4:
                    return Scale(state.Ry);
            }
            return 0;
        }
        public static int GetButton(Guid g, uint btn_idx, ref double value)
        {
            Device d = null;
            _gamepads.TryGetValue(g, out d);

            if (d == null) // already start working on this object
            {
                int err = BeginAcquire(g);
                err = -1;
                value = 0;
                return err;
            }

            try
            {
                byte[] btns = d.CurrentJoystickState.GetButtons();

                value = (btns[btn_idx] > 0) ? 1.0 : 0.0;
                return 0;
            }
            catch (Exception)
            {
                value = 0;
                RemoveGuid(g);
            }
            return -1;
        }
    }
}
