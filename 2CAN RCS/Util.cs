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

namespace CtrElectronics.CrossLinkControlSystem
{
    class Util
    {
        public static double abs(double d)
        {
            if (d < 0)
                return -d;
            return d;
        }
        public static double Deadband(double d, double cap)
        {
            if (abs(d) < cap)
                return 0;
            return d;
        }
        public static double Limit(double num)
        {
            if (num > 1.0)
            {
                return 1.0;
            }
            if (num < -1.0)
            {
                return -1.0;
            }
            return num;
        }

        public static String DoubleToStr(double d)
        {
            return d.ToString("0.00");
            //d *= 100;
            //d = (double)((int)(d));
            //d /= 100;
            //return Convert.ToString(d);
        }

        public static String DoubleToStrFixed(double d)
        {
            return d.ToString("00.00");
            //uint iWhole = (uint)d;
            //uint iFrac = (uint)((d - (double)iWhole) * 100);

            //string whole = Convert.ToString(iWhole);
            //while (whole.Length < 2)
            //    whole = "0" + whole;

            //string frac = Convert.ToString(iFrac);
            //while (frac.Length < 2)
            //    frac = "0" + frac;

            //return Convert.ToString(whole) + "." + frac;
        }
        /**
         * Helpful function that will update a combobox's list of items
         * but will keep the currently selected item selected if it
         * still exists, otherwise will default to first (not selected)
         * item 
         * @param _Inputs new input list to apply	.			  
         * @param cbo Combobox to update.
         */
        public static void FillDropdown(Presentation.Input[] _Inputs, System.Windows.Forms.ComboBox cbo, string[] unique_ids)
        {
            string unique = "deadbeef";
            int new_selected = 0; // default is first input - The not selected input
            //save current selection
            if (cbo.SelectedIndex >= 0)
                unique = unique_ids[cbo.SelectedIndex];
            //now fill the segment combos
            cbo.BeginUpdate();
            cbo.Items.Clear();
            foreach (Presentation.Input input in _Inputs)
            {
                cbo.Items.Add(input.ToString());

                if (input.GetUniqueString().Equals(unique))
                    new_selected = cbo.Items.Count - 1;
            }
            //restore current selection
            cbo.SelectedIndex = new_selected;
            cbo.EndUpdate();
        }
		/**
		 * Computes the output (throttle) for a given input and wiresegment.
		 * @param ws 		WireSegment to process (a particular row).  This gives us the offset and scaler.
		 * @param input 	The input to read : gamepad, joystick, button, slider, etc...
		 * @dThrottle [out] The computed throttle to send to the output : motor, servo, solenoid, etc...
		 * @return 			true iff input was successfully accesible.  Example of a false return would be
		 *						pulling a USB gamepad while running.  GUI should signal that it was pulled
		 *						and throttle should go to zero.
		 */
        public static bool CalcInput(WireSegment ws, Presentation.Input input, ref double dThrottle)
        {
            double dDeadband = ws.DeadbandPercent;
            double scalar = ws.Scalar;
            double offset = ws.Offset;

            dThrottle = 0;
            if (input.GetValue(ref dThrottle) < 0)
                return false;

            if (abs(dThrottle) < dDeadband)
                dThrottle = 0;
            dThrottle += offset;
            dThrottle *= scalar;

            return true;
        }
    }
}
