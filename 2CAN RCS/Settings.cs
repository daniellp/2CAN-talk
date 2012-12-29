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
/**
 * @file    Settings.cs
 *
 * @brief   Implements the settings class to save/load application settings. 
 * 			Class leverages the C# XML library.
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xml.Serialization.GeneratedAssembly;
   
namespace CtrElectronics.CrossLinkControlSystem
{

    [XmlRoot("settings")]
    public class Settings
    {
        private ArrayList _segments;
        private ConnectionSettings _connectionSettings;
        private MecanumSettings _mecanumSettings;
        public Settings()
        {
            _segments = new ArrayList();
            _connectionSettings = new ConnectionSettings();
            _mecanumSettings = new MecanumSettings();
        }

        [XmlElement("connection")]
        public ConnectionSettings connection
        {
            get
            {
                return _connectionSettings;
            }
            set
            {
                if (value == null) return;
                _connectionSettings = (ConnectionSettings)value;
            }
        }

        [XmlElement("mecanumSettings")]
        public MecanumSettings mecanumSettings
        {
            get
            {
                return _mecanumSettings;
            }
            set
            {
                if (value == null) return;
                _mecanumSettings = (MecanumSettings)value;
            }
        }
        

        [XmlElement("wireSegment")]
        public WireSegmentEntry[] wireSegments
        {
            get
            {
                WireSegmentEntry[] items = new WireSegmentEntry[_segments.Count];
                _segments.CopyTo(items);
                return items;
            }
            set
            {
                if (value == null) return;
                WireSegmentEntry[] items = (WireSegmentEntry[])value;
                _segments.Clear();
                foreach (WireSegmentEntry item in items)
                    _segments.Add(item);
            }
        }

        public int AddSegment(WireSegmentEntry item)
        {
            return _segments.Add(item);
        }
        public ArrayList GetSegments()
        {
            return _segments;
        }
        public void ClearSegments()
        {
            _segments.Clear();
        }

        public static void SaveSettings(ref Settings settings, string appPath)
        {
			/* Build serializer during the runtime.
				This is great for developers when you are changing/adding to the xml settings.
				But on slower laptops/netbooks this could take a couple seconds. */
            //XmlSerializer s = new XmlSerializer(typeof(Settings));
			/* Use CtrElectronics.CrossLinkControlSystem.XmlSerializers.dll.
				If you make changes to the settings xml, then run XmlBatch.bat to recompile the dll.
				This will use the Windows tool sgen CtrElectronics.CrossLinkControlSystem.exe
				which will compile a dll that can replace XmlSerializer, and is WAY faster.
				@see http://msdn.microsoft.com/en-us/library/bk3w6240(v=vs.80).aspx */			
            SettingsSerializer s = new SettingsSerializer();

            // Serialization
            TextWriter w = new StreamWriter(appPath);
            s.Serialize(w, settings);
            w.Close();
        }
        public static Settings LoadSettings(string appPath)
        {
            try
            {
				/* Build serializer during the runtime.
					This is great for developers when you are changing/adding to the xml settings.
					But on slower laptops/netbooks this could take a couple seconds. */
                //XmlSerializer s = new XmlSerializer(typeof(Settings));
				/* Use CtrElectronics.CrossLinkControlSystem.XmlSerializers.dll.
					If you make changes to the settings xml, then run XmlBatch.bat to recompile the dll.
					This will use the Windows tool sgen CtrElectronics.CrossLinkControlSystem.exe
					which will compile a dll that can replace XmlSerializer, and is WAY faster.
					@see http://msdn.microsoft.com/en-us/library/bk3w6240(v=vs.80).aspx */	
                SettingsSerializer s = new SettingsSerializer();

                // Deserialization
                Settings settings;
                TextReader r = new StreamReader(appPath);
                settings = (Settings)s.Deserialize(r);
                r.Close();

                return settings;
            }
            catch (Exception)
            {
            }
            return null;
        }
    }

    public class ConnectionSettings
    {
        [XmlAttribute("ip2can")]
        public string _ip2can;
        [XmlAttribute("rcmId")]
        public string _rcmId;
        
        public ConnectionSettings()
        {
            _ip2can = "192.168.1.1";
            _rcmId = "1";
        }

        public ConnectionSettings(string ip2can, string rcmId)
        {
            _ip2can = ip2can;
            _rcmId = rcmId;
        }
    }

    public class WireSegmentEntry
    {
        [XmlAttribute("inputDevice")]
        public string inputDevice;
        [XmlAttribute("outputDevice")]
        public string outputDevice;
        [XmlAttribute("deadBandPerc")]
        public double deadBandPerc;
        [XmlAttribute("offset")]
        public double offset;
        [XmlAttribute("scalar")]
        public double scalar;

        public WireSegmentEntry()
        {
            inputDevice = "Button 1";
            outputDevice = "None";
            deadBandPerc = 0;
            offset = 0;
            scalar = 1;
        }
    }
    public class MecanumSettings
    {
        public WireSegmentEntry forward;
        public WireSegmentEntry strafe;
        public WireSegmentEntry twist;

        public MecanumSettings()
        {
        }
    }

}
