﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace Military
{
    public class Target
    {
        protected int _healthPoints;     
         
        public int X { get; set; }  

        public int Y { get; set; } 

        public virtual int HealthPoints
        {
            get
            {
                return _healthPoints;
            } 
            set
            {
                Monitor.Enter(this);
                if (_healthPoints <= 0)
                {
                    _healthPoints = 0;
                }
                else
                { 
                    _healthPoints = value;
                }
                Monitor.Exit(this);
            }
        }

        public Target(int x , int y)
        {
            _healthPoints = 100;
            X = x;
            Y = y;
        }
    }
}