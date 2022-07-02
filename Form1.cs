using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VoVRant
{
    public partial class Form1 : Form
    {
        private const int SizeX = 32;
        private const int SizeY = 16;

        private static readonly int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
        private static readonly int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;

        private readonly int _screenHeightHalf = ScreenHeight / 2;
        private readonly int _screenWidthHalf = ScreenWidth / 2;

        private readonly Bitmap _visual = new Bitmap(SizeX, SizeY);
        private double _avgPoint;
        private bool _breakOut;
        private bool _centerAligned;

        private bool _foundLeftSide;
        private bool _foundRightSide;

        private List<Point> _hitPoints;
        private int _idx;

        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        
        [DllImport("user32.dll",CharSet=CharSet.Auto, CallingConvention=CallingConvention.StdCall)]
        private static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private void Form1_Load(object sender, EventArgs e)
        {
            new Thread(Action) { IsBackground = true }.Start();
        }

        private void Action()
        {
            while (true)
            {
                if ((GetAsyncKeyState(Keys.XButton2) & 0x8000) == 0)
                    goto WaitCallback;

                _hitPoints = new List<Point>();
                _foundLeftSide = false;
                _centerAligned = false;
                _foundRightSide = false;
                _breakOut = false;
                _avgPoint = 0;
                _idx = 0;

                using (var graphics = Graphics.FromImage(_visual))
                {
                    //Take a screenshot
                    graphics.CopyFromScreen(_screenWidthHalf - SizeX / 2, _screenHeightHalf - SizeY / 2, 0, 0,
                        _visual.Size);

                    //Get all pixels
                    for (var i = 0; i < _visual.Width; ++i)
                    {
                        for (var j = 0; j < _visual.Height; ++j)
                        {
                            var centerColor = _visual.GetPixel(i, j);
                            //If good color, shoot
                            if (centerColor.R <= 230 || centerColor.B <= 230 || centerColor.G >= 200)
                                continue;

                            //Check both sides
                            if (i < _visual.Width / 2)
                                _foundLeftSide = true;
                            else if (i > _visual.Width / 2) _foundRightSide = true;

                            //Add it to list
                            _hitPoints.Add(new Point(i, j));

                            //Check if list avg is head like
                            foreach (var p in _hitPoints)
                            {
                                ++_idx;
                                _avgPoint += Math.Sqrt(Math.Pow(p.X - _visual.Width / 2, 2) +
                                                       Math.Pow(p.Y - _visual.Height / 2, 2));
                                if (!(_avgPoint / _idx < 12) || _idx <= 4)
                                    continue;
                                _centerAligned = true;
                                break;
                            }

                            //Shoot it boii
                            if (!_foundRightSide || !_foundLeftSide || !_centerAligned)
                                continue;

                            mouse_event(0x02 | 0x04, 0, 0, 0, 0);
                            
                            _breakOut = true;
                            break;
                        }

                        if (!_breakOut)
                            continue;

                        break;
                    }
                }

                WaitCallback:
                Thread.Sleep(10);
            }
        }
    }
}
