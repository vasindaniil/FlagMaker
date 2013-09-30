using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlagMaker.Overlays.OverlayTypes
{
	internal class OverlayFimbriationForward : Overlay
	{
		public OverlayFimbriationForward(int maximum)
			: base(new List<Attribute>
			       {
				       new Attribute("Ratio", true, 1)
			       }, maximum)
		{
		}

		public OverlayFimbriationForward(Color color, double ratio, int maximum)
			: base(color, new List<Attribute>
			              {
				              new Attribute("Ratio", true, ratio)
			              }, maximum)
		{
		}

		public override string Name { get { return "fimbriation forward"; } }

		public override void Draw(Canvas canvas)
		{
			var width = (int)(canvas.Width / (Attributes.Get("Ratio").Value + 2));
			var path = new Path
				           {
					           Fill = new SolidColorBrush(Color),
					           Width = canvas.Width,
					           Height = canvas.Height,
					           Data = Geometry.Parse(string.Format("M {0},0 {1},0 {1},{2} {2},{3} 0,{3} 0,{4} {0},0", canvas.Width - width, canvas.Width, width, canvas.Height, canvas.Height - width)),
					           SnapsToDevicePixels = true
				           };
			canvas.Children.Add(path);
		}
		
		public override void SetValues(List<double> values)
		{
			Attributes.Get("Ratio").Value = values[0];
		}

		public override string ExportSvg(int width, int height)
		{
			var w = (int)(width / (Attributes.Get("Ratio").Value + 2));
			return string.Format("<polygon points=\"{0},0 {1},0 {1},{2} {2},{3} 0,{3} 0,{4} {0},0\" fill=\"#{5}\" />",
				 width - w, width, w, height, height - w,
				Color.ToHexString());
		}

		public override IEnumerable<Shape> Thumbnail
		{
			get
			{
				return new List<Shape>
				       {
						   new Line
						   {
							   StrokeThickness = 5,
							   X1 = 30,
							   X2 = 0,
							   Y1 = 0,
							   Y2 = 20
						   }
				       };
			}
		}
	}
}