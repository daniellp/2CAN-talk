using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace CtrElectronics.CrossLinkControlSystem
{
    class RgbFade
    {    
        uint _numSteps;

        double _r1;
        double _g1;
        double _b1;

        double _r2;
        double _g2;
        double _b2;

        double _rMin;
        double _gMin;
        double _bMin;

        double _rMax;
        double _gMax;
        double _bMax;

        double _slopeR;
        double _slopeG;
        double _slopeB;

        double _r;
        double _g;
        double _b;

        public RgbFade(uint rgb1, uint rgb2, uint numSteps, uint initRgb)
        {

            rgb1 &= 0x00FFFFFF;
            rgb2 &= 0x00FFFFFF;

            _numSteps = numSteps;

            Set(initRgb);

            _r1 = GetR(rgb1);
            _g1 = GetG(rgb1);
            _b1 = GetB(rgb1);

            _r2 = GetR(rgb2);
            _g2 = GetG(rgb2);
            _b2 = GetB(rgb2);

            _slopeR = ((double)_r2 - (double)_r1) / (double)numSteps;
            _slopeG = ((double)_g2 - (double)_g1) / (double)numSteps;
            _slopeB = ((double)_b2 - (double)_b1) / (double)numSteps;

            _rMin = (_r1 < _r2) ? _r1 : _r2;
            _gMin = (_g1 < _g2) ? _g1 : _g2;
            _bMin = (_b1 < _b2) ? _b1 : _b2;

            _rMax = (_r1 > _r2) ? _r1 : _r2;
            _gMax = (_g1 > _g2) ? _g1 : _g2;
            _bMax = (_b1 > _b2) ? _b1 : _b2;
        }
        public void Set(uint rgb)
        {
            _r = GetR(rgb);
            _g = GetG(rgb);
            _b = GetB(rgb);
        }
        uint GetR(uint rgb)
        {
            return (rgb >> 0x10) & 0xFF;
        }
        uint GetG(uint rgb)
        {
            return (rgb >> 0x08) & 0xFF;
        }
        uint GetB(uint rgb)
        {
            return (rgb >> 0x00) & 0xFF;
        }
        public uint ToInt()
        {
            uint r = ((uint)_r) & 0xff;
            uint g = ((uint)_g) & 0xff;
            uint b = ((uint)_b) & 0xff;
            return (r << 0x10) | (g << 0x08) | (b << 0x00);
        }
        double ApplySlope(double component, double slope, double min, double max)
        {
            component += slope;
            return Math.Max(
                Math.Min(component, max)
                            , min);

        }
        public uint Approach1()
        {
            _r = ApplySlope(_r, -_slopeR, _rMin, _rMax);
            _g = ApplySlope(_g, -_slopeG, _gMin, _gMax);
            _b = ApplySlope(_b, -_slopeB, _bMin, _bMax);

            return ToInt();
        }
        public uint Approach2()
        {
            _r = ApplySlope(_r, _slopeR, _rMin, _rMax);
            _g = ApplySlope(_g, _slopeG, _gMin, _gMax);
            _b = ApplySlope(_b, _slopeB, _bMin, _bMax);

            return ToInt();
        }
    }
}
