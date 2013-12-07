﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FlagMaker.Divisions;
using FlagMaker.Overlays;
using FlagMaker.Overlays.OverlayTypes.RepeaterTypes;
using FlagMaker.Overlays.OverlayTypes.ShapeTypes;
using Microsoft.Win32;

namespace FlagMaker
{
	// An immutable flag object
	public class Flag
	{
		public readonly string Name;
		public readonly Ratio Ratio;
		public readonly Ratio GridSize;
		public readonly Division Division;
		public readonly List<Overlay> Overlays;

		public Flag(string name, Ratio ratio, Ratio gridSize, Division division, IEnumerable<Overlay> overlays)
		{
			Name = name;
			Ratio = ratio;
			GridSize = gridSize;
			Division = division;
			Overlays = overlays.ToList();
		}

		public static Flag LoadFromFile(string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				return new Flag("none", new Ratio(2, 3), new Ratio(2, 3), new DivisionGrid(Colors.White, Colors.Black, 2, 2), new List<Overlay>());
			}

			var name = string.Empty;
			var ratio = new Ratio(3, 2);
			var gridRatio = new Ratio(3, 2);

			string divisionType = "grid";
			Color divisionColor1 = Colors.White;
			Color divisionColor2 = Colors.White;
			Color divisionColor3 = Colors.White;
			int divisionVal1 = 1;
			int divisionVal2 = 1;
			int divisionVal3 = 1;

			var overlays = new List<TempOverlay>();
			try
			{

				using (var sr = new StreamReader(filename))
				{
					string line;

					bool isDivision = false;
					int overlayIndex = -1;

					while ((line = sr.ReadLine()) != null)
					{
						switch (line.Split('=')[0].ToLower())
						{
							case "name":
								name = line.Split('=')[1];
								break;
							case "ratio":
								var ratioStrings = line.Split('=')[1].Split(':');
								ratio = new Ratio(int.Parse(ratioStrings[1]), int.Parse(ratioStrings[0]));
								break;
							case "gridsize":
								var data = line.Split('=')[1].Split(':');
								gridRatio = new Ratio(int.Parse(data[1]), int.Parse(data[0]));
								break;
							case "division":
								isDivision = true;
								break;
							case "overlay":
								isDivision = false;
								overlayIndex++;
								overlays.Add(new TempOverlay());
								break;
							case "type":
								if (isDivision)
								{
									divisionType = line.Split('=')[1];
								}
								else
								{
									overlays[overlayIndex].Type = line.Split('=')[1];
								}
								break;
							case "color1":
								divisionColor1 = ParseColor(line.Split('=')[1]);
								break;
							case "color2":
								divisionColor2 = ParseColor(line.Split('=')[1]);
								break;
							case "color3":
								divisionColor3 = ParseColor(line.Split('=')[1]);
								break;
							case "color":
								overlays[overlayIndex].Color = ParseColor(line.Split('=')[1]);
								break;
							case "size1":
								if (isDivision)
								{
									divisionVal1 = int.Parse(line.Split('=')[1]);
								}
								else
								{
									overlays[overlayIndex].Values[0] = double.Parse(line.Split('=')[1]);
								}
								break;
							case "size2":
								if (isDivision)
								{
									divisionVal2 = int.Parse(line.Split('=')[1]);
								}
								else
								{
									overlays[overlayIndex].Values[1] = double.Parse(line.Split('=')[1]);
								}
								break;
							case "size3":
								if (isDivision)
								{
									divisionVal3 = int.Parse(line.Split('=')[1]);
								}
								else
								{
									overlays[overlayIndex].Values[2] = double.Parse(line.Split('=')[1]);
								}
								break;
							case "size4":
								overlays[overlayIndex].Values[3] = double.Parse(line.Split('=')[1]);
								break;
							case "size5":
								overlays[overlayIndex].Values[4] = double.Parse(line.Split('=')[1]);
								break;
							case "size6":
								overlays[overlayIndex].Values[5] = double.Parse(line.Split('=')[1]);
								break;
							case "path":
								overlays[overlayIndex].FlagPath = line.Split('=')[1];
								break;
						}
					}
				}

				Division division;
				switch (divisionType)
				{
					case "fesses":
						division = new DivisionFesses(divisionColor1, divisionColor2, divisionColor3, divisionVal1, divisionVal2,
							divisionVal3);
						break;
					case "pales":
						division = new DivisionPales(divisionColor1, divisionColor2, divisionColor3, divisionVal1, divisionVal2,
							divisionVal3);
						break;
					case "bends forward":
						division = new DivisionBendsForward(divisionColor1, divisionColor2);
						break;
					case "bends backward":
						division = new DivisionBendsBackward(divisionColor1, divisionColor2);
						break;
					case "bends both":
						division = new DivisionX(divisionColor1, divisionColor2);
						break;
					default:
						division = new DivisionGrid(divisionColor1, divisionColor2, divisionVal1, divisionVal2);
						break;
				}

				return new Flag(name, ratio, gridRatio, division,
					overlays.Select(o => o.ToOverlay(gridRatio.Width, gridRatio.Height)));
			}
			catch (Exception)
			{
				MessageBox.Show("Couldn't open the file. Check your syntax and try again.");
				return new Flag("none", new Ratio(2, 3), new Ratio(2, 3), new DivisionGrid(Colors.White, Colors.Black, 2, 2), new List<Overlay>());
			}
		}

		public static string GetFlagPath()
		{
			var dlg = new OpenFileDialog
			{
				DefaultExt = ".flag",
				Filter = "Flag (*.flag)|*.flag|All files (*.*)|*.*",
				Multiselect = false
			};

			bool? result = dlg.ShowDialog();
			if (!((bool)result)) return string.Empty;
			return dlg.FileName;
		}

		public void Draw(Canvas canvas)
		{
			canvas.Children.Clear();
			Division.Draw(canvas);

			SetRepeaterOverlays();

			for (int i = 0; i < Overlays.Count; i++)
			{
				// Skip overlays used in repeaters
				if (i > 0 && Overlays[i - 1] is OverlayRepeater) continue;
				
				Overlays[i].Draw(canvas);
			}
		}

		public void ExportToSvg(Uri path)
		{
			const int width = 600;
			var height = (int)(((double)Ratio.Height / Ratio.Width) * width);

			using (var sw = new StreamWriter(path.AbsolutePath))
			{
				sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
				sw.WriteLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
				sw.WriteLine("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"{0}\" height=\"{1}\">", width, height);

				sw.WriteLine(Division.ExportSvg(width, height));
				
				SetRepeaterOverlays();
				
				for (int i = 0; i < Overlays.Count; i++)
				{
					if (i > 0 && Overlays[i - 1] is OverlayRepeater) continue;

					var overlay = Overlays[i];

					try
					{
						sw.WriteLine(overlay.ExportSvg(width, height));
					}
					catch (NotImplementedException)
					{
						// Ignore overlays without SVG implementation
					}
				}

				sw.WriteLine("</svg>");
			}
		}

		public List<Color> ColorsUsed()
		{
			var colors = new List<Color>();

			if (Division is DivisionGrid && Division.Values[0] == 1 && Division.Values[1] == 1)
			{
				colors.Add(Division.Colors[0]);
			}
			else
			{
				colors.AddRange(Division.Colors);
			}

			foreach (var overlay in Overlays)
			{
				if (overlay is OverlayFlag)
				{
					colors.AddRange(((OverlayFlag) overlay).Flag.ColorsUsed());
				}
				else if (!(overlay is OverlayRepeater))
				{
					colors.Add(overlay.Color);
				}
			}

			// Strip duplicates (Distinct() doesn't work)
			//var distinctColors = new List<Color>();
			//foreach (var color in colors)
			//{
			//	if (!distinctColors.Any(c => c.R == color.R && c.G == color.G && c.B == color.B))
			//	{
			//		distinctColors.Add(color);
			//	}
			//}

			return colors.Distinct().OrderBy(c => c.Hue()).ToList();
		} 

		private void SetRepeaterOverlays()
		{
			for (int i = Overlays.Count - 1; i > 0; i--)
			{
				if (Overlays[i - 1] is OverlayRepeater)
				{
					((OverlayRepeater)Overlays[i - 1]).SetOverlay(Overlays[i]);
				}
			}
		}

		private static Color ParseColor(string str)
		{
			Byte a = 0xff, r, b, g;

			if (str.Length == 8)
			{
				a = byte.Parse(str.Substring(0, 2), NumberStyles.HexNumber);
				r = byte.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
				g = byte.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
				b = byte.Parse(str.Substring(6, 2), NumberStyles.HexNumber);
			}
			else
			{
				r = byte.Parse(str.Substring(0, 2), NumberStyles.HexNumber);
				g = byte.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
				b = byte.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
			}

			return Color.FromArgb(a, r, g, b);
		}

		private class TempOverlay
		{
			public string Type;
			public readonly List<double> Values;
			public Color Color;
			public string FlagPath;

			public TempOverlay()
			{
				Values = new List<double>
				{
					1,
					1,
					1,
					1,
					1,
					1
				};
			}

			public Overlay ToOverlay(int maxX, int maxY)
			{
				Overlay overlay = string.IsNullOrWhiteSpace(FlagPath)
					? OverlayFactory.GetInstance(Type, maxX, maxY)
					: OverlayFactory.GetInstance(Type, FlagPath, maxX, maxY);

				if (overlay != null)
				{
					overlay.SetColors(new List<Color> { Color });
					overlay.SetValues(Values);
				}

				return overlay;
			}
		}
	}
}
