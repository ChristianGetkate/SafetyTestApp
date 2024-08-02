using System;
using System.ComponentModel;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Text.Unicode;

namespace LedSingleLibrary
{
    public partial class LedSingleControl : UserControl
    {
        #region Declare variables & constants

        private bool _Value = true;

        private Color _OnColor = Color.Lime;

        private Color _OffColor = Color.DarkGray;

        private string _Text = "";

        #endregion

        #region Constructors
        public LedSingleControl()
        {
            InitializeComponent();

        }

        #endregion

        #region Methods & Events
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Graphics g = e.Graphics;
            PointF pointF = new PointF((float)this.Width / 2f, (float)this.Height / 2f);
            float num1 = Math.Min(pointF.X, pointF.Y);
            float num2 = (float)((double)num1 * 65.0 / 100.0);
            float num3 = (float)((double)num1 * 55.0 / 100.0);
            float num4 = (float)((double)num1 * 45.0 / 100.0);
            Brush _Brush = (Brush)new LinearGradientBrush(new Point((int)((double)pointF.X - (double)num2), (int)((double)pointF.Y - (double)num2)),
                new Point((int)((double)pointF.X + (double)num2), (int)((double)pointF.Y + (double)num2)), Color.WhiteSmoke, SystemColors.ControlDarkDark);
            g.FillEllipse(_Brush, pointF.X - num2, pointF.Y - num2, 2f * num2, 2f * num2);
            _Brush.Dispose();
            if (this.Value)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(pointF.X - num1, pointF.Y - num1, num1 * 2f, num1 * 2f);
                PathGradientBrush pathGradientBrush = new PathGradientBrush(path);
                pathGradientBrush.CenterColor = Color.FromArgb(150, (int)this.OnColor.R, (int)this.OnColor.G, (int)this.OnColor.B);
                Color[] colorArray = new Color[1] { Color.FromArgb(1, (int)this.OnColor.R, (int)this.OnColor.G, (int)this.OnColor.B) };
                pathGradientBrush.SurroundColors = colorArray;
                g.FillEllipse((Brush)pathGradientBrush, pointF.X - num1, pointF.Y - num1, num1 * 2f, num1 * 2f);
                path.Dispose();
                pathGradientBrush.Dispose();
            }
            Brush brush2 = (Brush)new LinearGradientBrush(new Point((int)((double)pointF.X - (double)num3), (int)((double)pointF.Y - (double)num3)),
                new Point((int)((double)pointF.X + (double)num3), (int)((double)pointF.Y + (double)num2)), SystemColors.ControlDarkDark, Color.WhiteSmoke);
            g.FillEllipse(brush2, pointF.X - num4, pointF.Y - num4, 2f * num4, 2f * num4);
            _Brush.Dispose();
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(pointF.X - num4, pointF.Y - num4, 2f * num4, 2f * num4);
            if (this.Value) // Value = true(Led On)
            {
                PathGradientBrush pathGradientBrush = new PathGradientBrush(gp);
                pathGradientBrush.CenterColor = Color.WhiteSmoke;
                Color[] colorArray = new Color[1] { this.OnColor };
                pathGradientBrush.SurroundColors = colorArray;
                pathGradientBrush.CenterPoint = new PointF(pointF.X - num4 / 2f, pointF.Y - num4 / 2f);
                g.FillEllipse((Brush)pathGradientBrush, pointF.X - num4, pointF.Y - num4, 2f * num4, 2f * num4);
                pathGradientBrush.Dispose();
            }
            else // Value = false(Led Off)
            {
                PathGradientBrush pathGradientBrush = new PathGradientBrush(gp);
                pathGradientBrush.CenterColor = Color.WhiteSmoke;
                Color[] colorArray = new Color[1] { this.OffColor };
                pathGradientBrush.SurroundColors = colorArray;
                pathGradientBrush.CenterPoint = new PointF(pointF.X - num4 / 2f, pointF.Y - num4 / 2f);
                g.FillEllipse((Brush)pathGradientBrush, pointF.X - num4, pointF.Y - num4, 2f * num4, 2f * num4);
                pathGradientBrush.Dispose();
            }
            gp.Dispose();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            float num = (float)Math.Min(this.Width, this.Height);
            if ((double)num < 20.0)
                num = 20f;
            this.Width = (int)num;
            this.Height = (int)num;
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, this.Width, this.Height);
            this.Region = new Region(path);
        }
        #endregion

        #region Properties
        [Category("HMI Properties")]
        public Color OffColor
        {
            get { return _OffColor; }
            set
            {
                _OffColor = value;
                this.Refresh();
            }
        }
        [Category("HMI Properties")]
        public Color OnColor
        {
            get { return _OnColor; }
            set
            {
                _OnColor = value;
                this.Refresh();
            }
        }
        [Category("HMI Properties")]
        public bool Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                this.Refresh();
            }
        }
        [Category("HMI Properties")]
        public string Text { get => _Text; set => _Text = value; }

        #endregion
    }
}
