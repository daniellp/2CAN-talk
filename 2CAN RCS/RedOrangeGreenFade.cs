using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using RgbFade = CtrElectronics.CrossLinkControlSystem.RgbFade;

namespace CtrElectronics.CrossLinkControlSystem
{
    class RedOrangeGreenFade
    {
        enum Fade
        {
            RedOrange,
            OrangeGreen,
        };

        uint ColorRed = ((uint)Color.Red.ToArgb()) & 0xFFFFFF;
        uint ColorOrange = ((uint)Color.Orange.ToArgb()) & 0xFFFFFF;
        uint ColorGreen = ((uint)0xC000) & 0xFFFFFF;

        uint StepsRedToOrange = 500 / 50;
        uint StepsOrangeToGreen = 1500 / 50;

        Fade _fade;
        RgbFade _rgbFade;

        public RedOrangeGreenFade()
        {
            SetToRed();
        }

        public void StepTowardsOrange()
        {
            _rgbFade.Approach1();

            switch (_fade)
            {
                case Fade.RedOrange:
                    break;

                case Fade.OrangeGreen:

                    _rgbFade = new RgbFade(ColorOrange,
                                ColorRed,
                                StepsRedToOrange,
                                ColorOrange);

                    _fade = Fade.RedOrange;
                    break;
            }
        }
        public void StepTowardsGreen()
        {
            _rgbFade.Approach1();

            switch(_fade)
            {
                case Fade.RedOrange:

                    if(_rgbFade.ToInt() == ColorOrange)
                    {
                        SetToOrange();
                    }
                    break;

                case Fade.OrangeGreen:
                    break;
            }
        }
        public void SetToOrange()
        {
            _rgbFade = new RgbFade(ColorGreen,
                                    ColorOrange,
                                    StepsOrangeToGreen,
                                    ColorOrange);
            _fade = Fade.OrangeGreen;
        }
        public void SetToRed()
        {
            _rgbFade = new RgbFade(ColorOrange,
                        ColorRed,
                        StepsRedToOrange,
                        ColorRed);

            _fade = Fade.RedOrange;
        }
        public uint ToInt()
        {
            return _rgbFade.ToInt();
        }
    }
}
