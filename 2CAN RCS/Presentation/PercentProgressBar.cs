/**
 * @file    Presentation\PercentProgressBar.cs
 *
 * @brief   Implements progress bar that works around an 
 * 			animation issue with the stock ProgressBar class. 
 * 			In Windows 7/Vista an increasing progress bar animates slowly.  
 * 			In other words set the Value to the minimum, then set Value to 
 * 			maximum and the progress bars animates slowly from left to right.
 * 			This class also provides an interface for 
 * 			specify percent (0-100%).
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CtrElectronics.CrossLinkControlSystem.Presentation
{
    class PercentProgressBar : ProgressBar
    {
        public PercentProgressBar()
        {
            Minimum = 0;
            Value = 0;
            Maximum = 100;
        }

        /**
         * @brief   Sets percent full of progress bar(0-100). 
         *
         * @author  Ozrien
         * @date    11/25/2010
         *
         * @param   percent The percent. 
         */

        public void SetPercent(int percent)
        {
            // Workaround for slow animated progress bar.  
            // Set value to one plus percent then decrease to percent.  
            // The decrease will cause progress bar 
            // to not animate the bar slowely, 
            // but instead quickly move bar to desired position.
            if(percent < 100)
            {
                Value = percent+1;
                Value = percent;
            }
            else
            {
                //to set bar to 100%, use the same trick (100 => 99 forcing fast fill), then set to 100 for correctness.
                Value = 100;
                Value = 99;
                Value = 100;
            }
        }

        /**
         * @brief   Gets or sets the percent of the progress bar. 
         */

        public int Percent
        {
            get
            {
                return base.Value;
            }
            set
            {
                SetPercent(value);
            }
        }
    }
}
