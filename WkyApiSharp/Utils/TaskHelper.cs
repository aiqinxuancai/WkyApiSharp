﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WkyApiSharp.Utils
{
   
    public class TaskHelper
    {

        public static void Sleep(int millisecondsToWait )
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
                {
                    break;
                }
                Thread.Sleep(1);
            }
        }

        public static void Sleep(int millisecondsToWait, int millisecondsTocycle)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
                {
                    break;
                }
                Thread.Sleep(millisecondsTocycle); 
            }
        }

        
        public static void Sleep(int millisecondsToWait, int millisecondsTocycle, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
   
                if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
                {
                    break;
                }

                Thread.Sleep(millisecondsTocycle); 

            }
        }
    }
}
