﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using FlagMaker.Divisions;
using FlagMaker.Overlays;
using FlagMaker.Overlays.OverlayTypes;
using FlagMaker.Overlays.OverlayTypes.PathTypes;
using FlagMaker.Overlays.OverlayTypes.ShapeTypes;

namespace FlagMaker.RandomFlag
{
	public static class RandomFlagFactory
	{
		#region Color definitions

		private static readonly Color Yellow = Color.FromRgb(253, 200, 47);
		private static readonly Color Silver = Color.FromRgb(141, 129, 123);
		private static readonly Color White = Color.FromRgb(255, 255, 255);
		private static readonly Color Black = Color.FromRgb(0, 0, 0);
		private static readonly Color Red = Color.FromRgb(198, 12, 48);
		private static readonly Color Orange = Color.FromRgb(255, 99, 25);
		private static readonly Color Green = Color.FromRgb(20, 77, 41);
		private static readonly Color LightBlue = Color.FromRgb(0, 101, 189);
		private static readonly Color DarkBlue = Color.FromRgb(0, 57, 166);
		private static readonly Color Purple = Color.FromRgb(102, 0, 153);

		#endregion

		private static Color _color1;
		private static Color _color2;
		private static Color _metal;
		private static Ratio _ratio;
		private static Ratio _gridSize;
		private static DivisionTypes _divisionType;
		private static Division _division;

		private static double _emblemX;
		private static double _emblemY;
		private static Color _emblemColor;

		public static Flag GenerateFlag()
		{
			GetColorScheme();
			GetRatio();
			_division = GetDivision();

			return new Flag("Random", _ratio, _gridSize, _division, GetOverlays());
		}

		private static void GetColorScheme()
		{
			// Simple Markov chain. Numbers are purely made up but should
			// bear some vague reflection of real-life flags.
			var colors = new[] { Black, Red, Green, LightBlue, DarkBlue, Silver, Orange, Purple };
			var color1Index = Randomizer.RandomWeighted(new List<int> { 33, 67, 50, 30, 40, 10, 8, 7 });

			var firstOrderBase = new List<List<int>>
			                     {                // B  R  G  L  D  S  O  P
				                     new List<int> { 0, 8, 8, 4, 4, 8, 4, 4 }, // Black
				                     new List<int> { 8, 0, 8, 6, 9, 2, 3, 1 }, // Red
				                     new List<int> { 8, 8, 0, 7, 8, 8, 2, 1 }, // Green
				                     new List<int> { 4, 6, 7, 0, 2, 3, 1, 2 }, // Light blue
				                     new List<int> { 4, 9, 8, 2, 0, 6, 2, 1 }, // Dark blue
				                     new List<int> { 8, 2, 8, 3, 6, 0, 2, 2 }, // Silver
				                     new List<int> { 4, 3, 2, 1, 2, 2, 0, 1 }, // Orange
				                     new List<int> { 4, 1, 1, 2, 1, 2, 1, 0 }, // Purple
			                     };

			var color2Index = Randomizer.RandomWeighted(firstOrderBase[color1Index]);

			var probabilityOfYellowValues = new[] { 0.3, 0.5, 0.5, 0.3, 0.4, 0.2, 0.4, 0.1 };
			var probabilityOfYellow = Math.Min(probabilityOfYellowValues[color1Index], probabilityOfYellowValues[color2Index]);
			_metal = Randomizer.ProbabilityOfTrue(probabilityOfYellow) ? Yellow : White;

			_color1 = colors[color1Index];
			_color2 = colors[color2Index];
		}

		private static void GetRatio()
		{
			_ratio = new List<Ratio>
			         {
				         new Ratio(3, 2),
				         new Ratio(5, 3),
				         new Ratio(2, 1)
			         }[Randomizer.RandomWeighted(new List<int> { 6, 1, 3 })];
			_gridSize = new Ratio(_ratio.Width * 8, _ratio.Height * 8);
		}

		private static Division GetDivision()
		{
			// Roughly based on what's used in the presets
			_divisionType = (DivisionTypes)Randomizer.RandomWeighted(new List<int> { 3, 4, 7, 1, 2, 1, 4, 1, 1, 8 });

			switch (_divisionType)
			{
				case DivisionTypes.Stripes:
					return new DivisionGrid(_color1, _metal, 1, Randomizer.Clamp(Randomizer.NextNormalized(8, 3), 3, 15, true));
				case DivisionTypes.Pales:
				case DivisionTypes.Fesses:
					return SetUpFessesAndPales();
				case DivisionTypes.DiagonalForward:
					return new DivisionBendsForward(_color1, _color2);
				case DivisionTypes.DiagonalBackward:
					return new DivisionBendsBackward(_color1, _color2);
				case DivisionTypes.X:
					return new DivisionX(_color1, _color2);
				case DivisionTypes.Horizontal:
					return Randomizer.ProbabilityOfTrue(0.6)
						? new DivisionGrid(_color1, _metal, 1, 2)
						: new DivisionGrid(_metal, _color1, 1, 2);
				case DivisionTypes.Vertical:
					return new DivisionGrid(_color1, _color2, 2, 1);
				case DivisionTypes.Quartered:
					return new DivisionGrid(_color1, _color2, 2, 2);
				case DivisionTypes.Blank:
					return new DivisionGrid(_color1, _color1, 1, 1);
				default:
					throw new Exception("No valid type selection");
			}
		}

		private static Division SetUpFessesAndPales()
		{
			var colors = new[] { _color1, _color2, _metal }.OrderBy(c => Randomizer.NextDouble()).ToList();

			var color1 = colors[0];
			var color2 = colors[1];
			var color3 = Randomizer.ProbabilityOfTrue(0.4)
				? colors[2]
				: colors[0];

			var isBalanced = Randomizer.ProbabilityOfTrue(0.4); // Middle is larger than outsides
			var isOffset = !isBalanced && Randomizer.ProbabilityOfTrue(0.2); // One large section, two small
			_emblemColor = isOffset ? color2 : Randomizer.ProbabilityOfTrue(0.5) ? color1 : color3;
			var emblemOffset = isOffset && Randomizer.ProbabilityOfTrue(0.5) ? 3.0 : 2.0;

			if (_divisionType == DivisionTypes.Fesses)
			{
				_emblemX = _gridSize.Width / emblemOffset;
				_emblemY = isOffset
					? _gridSize.Height / 4.0
					: _gridSize.Height / 2.0;

				return new DivisionFesses(color1, color2, color3,
					isOffset ? 2 : 1,
					isBalanced ? 2 : 1,
					1);
			}

			_emblemY = _gridSize.Height / emblemOffset;
			_emblemX = isOffset
				? _gridSize.Width / 4.0
				: _gridSize.Width / 2.0;

			return new DivisionPales(color1, color2, color3,
					isOffset ? 2 : 1,
					isBalanced ? 2 : 1,
					1);
		}

		private static IEnumerable<Overlay> GetOverlays()
		{
			var list = new List<Overlay>();

			switch (_divisionType)
			{
				case DivisionTypes.Stripes:
					AddHoist(list, true);
					break;
				case DivisionTypes.Pales:
					AddEmblem(0.8, list);
					break;
				case DivisionTypes.Fesses:
					AddEmblem(0.6, list);
					break;
				case DivisionTypes.DiagonalForward:
					if (Randomizer.ProbabilityOfTrue(0.5))
					{
						AddFimbriationForward(list);
					}
					else
					{
						_emblemColor = _metal;
						AddEmblem(1.0, list, new Rect
											 {
												 Top = 0,
												 Bottom = _gridSize.Height * 2 / 3.0,
												 Left = 0,
												 Right = _gridSize.Width / 2.0
											 }, false);
						if (Randomizer.ProbabilityOfTrue(0.3))
						{
							AddFimbriationForward(list);
						}
					}
					break;
				case DivisionTypes.DiagonalBackward:
					if (Randomizer.ProbabilityOfTrue(0.5))
					{
						AddFimbriationBackward(list);
					}
					else
					{
						_emblemColor = _metal;
						AddEmblem(1.0, list, new Rect
											 {
												 Top = 0,
												 Bottom = _gridSize.Height * 2 / 3.0,
												 Left = _gridSize.Width / 2.0,
												 Right = _gridSize.Width
											 }, false);
						if (Randomizer.ProbabilityOfTrue(0.3))
						{
							AddFimbriationBackward(list);
						}
					}
					break;
				case DivisionTypes.X:
					AddOverlaysX(list, false);
					break;
				case DivisionTypes.Horizontal:
					if (Randomizer.ProbabilityOfTrue(0.2))
					{
						_emblemColor = _metal;
						var totalWidth = AddPall(list, true);
						AddTriangle(list, totalWidth);
					}
					else
					{
						AddHoist(list, false);
					}
					break;
				case DivisionTypes.Vertical:
					AddVerticalOverlays(list);
					break;
				case DivisionTypes.Quartered:
					AddCross(list, _gridSize.Width / 2.0);
					break;
				case DivisionTypes.Blank:
					AddAnyOverlays(list);
					break;
			}
			return list;
		}

		private static void AddVerticalOverlays(ICollection<Overlay> list)
		{
			var type = Randomizer.RandomWeighted(new List<int> { 1, 1, 1, 1, 1, 1 });

			_emblemX = _gridSize.Width / 2.0;
			_emblemY = _gridSize.Height / 2.0;

			switch (type)
			{
				case 0:
					AddFimbriationBackward(list);
					break;
				case 1:
					AddFimbriationForward(list);
					break;
				case 2: // Centered cross
					AddCross(list, _gridSize.Width / 2.0);
					break;
				case 3:
					AddDiamond(list, true);
					break;
				case 4:
					var forceMetal = GetEmblemPositionForVertical(false);
					AddCircle(list, _emblemX, _emblemY, 0.9, forceMetal);
					break;
				case 5:
					GetEmblemPositionForVertical(true);
					_emblemColor = _metal;
					AddEmblem(1.0, list);
					break;
			}
		}

		private static bool GetEmblemPositionForVertical(bool allowRaise)
		{
			var forceMetal = true;

			if (Randomizer.ProbabilityOfTrue(0.33))
			{
				_emblemX = _gridSize.Width * 0.75;
			}
			else if (Randomizer.ProbabilityOfTrue(0.5))
			{
				_emblemX = _gridSize.Width * 0.25;
				forceMetal = false;

				if (allowRaise && Randomizer.ProbabilityOfTrue(0.5))
				{
					_emblemY = _gridSize.Height / 4.0;
				}
			}

			return forceMetal;
		}

		private static void AddAnyOverlays(ICollection<Overlay> list)
		{
			var type = Randomizer.RandomWeighted(new List<int> { 2, 4, 3, 2, 2, 2, 1, 1, 3, 2 });
			var left = _gridSize.Width / (Randomizer.ProbabilityOfTrue(0.3) ? 3.0 : 2.0);
			switch (type)
			{
				case 0:
					AddOverlaysX(list, true);
					break;
				case 1:
					AddFimbriationBackward(list);
					break;
				case 2:
					AddFimbriationForward(list);
					break;
				case 3: // Nordic cross
					AddCross(list, _gridSize.Width / 3.0);
					break;
				case 4: // Centered cross
					AddCross(list, _gridSize.Width / 2.0);
					break;
				case 5:
					AddDiamond(list, false);
					break;
				case 6:
					AddPall(list, false);
					break;
				case 7:
					list.Add(new OverlayRays(_metal, left, _gridSize.Height / 2.0,
						Randomizer.Clamp(Randomizer.NextNormalized(_gridSize.Width * 3 / 4.0, _gridSize.Width / 10.0), 4, 20), 0, 0));
					AddCircle(list, left, _gridSize.Height / 2.0, 0.7, false);
					break;
				case 8:
					AddCircle(list, left, _gridSize.Height / 2.0, 0.9, false);
					break;
				case 9: // Horizontal stripes
					list.Add(new OverlayLineHorizontal(_metal, _gridSize.Height / 8.0, _gridSize.Height * 3 / 16.0, 0, 0));
					list.Add(new OverlayLineHorizontal(_metal, _gridSize.Height / 8.0, _gridSize.Height * 13 / 16.0, 0, 0));
					_emblemColor = _metal;
					_emblemX = Randomizer.ProbabilityOfTrue(0.2) ? _gridSize.Width / 3.0 : _gridSize.Width / 2.0;
					_emblemY = _gridSize.Height / 2.0;
					AddEmblem(1.0, list);
					break;
			}
		}

		private static void AddFimbriationBackward(ICollection<Overlay> list)
		{
			var width = Randomizer.Clamp(Randomizer.NextNormalized(_gridSize.Width / 3.0, _gridSize.Width / 10.0), 1, _gridSize.Width);
			if (Randomizer.ProbabilityOfTrue(0.4))
			{
				list.Add(new OverlayFimbriationBackward(_metal, width + 1, 0, 0));
				list.Add(new OverlayFimbriationBackward(_color2, width - 1, 0, 0));
			}
			else
			{
				list.Add(new OverlayFimbriationBackward(_metal, width, 0, 0));
			}
		}

		private static void AddFimbriationForward(ICollection<Overlay> list)
		{
			var width = Randomizer.Clamp(Randomizer.NextNormalized(_gridSize.Width / 3.0, _gridSize.Width / 10.0), 1, _gridSize.Width);
			if (Randomizer.ProbabilityOfTrue(0.4))
			{
				list.Add(new OverlayFimbriationForward(_metal, width + 1, 0, 0));
				list.Add(new OverlayFimbriationForward(_color2, width - 1, 0, 0));
			}
			else
			{
				list.Add(new OverlayFimbriationForward(_metal, width, 0, 0));
			}
		}

		private static void AddCross(ICollection<Overlay> list, double left)
		{
			var width = Randomizer.Clamp(Randomizer.NextNormalized(_gridSize.Width / 10.0, _gridSize.Width / 20.0), 1, _gridSize.Width / 3);
			if (Randomizer.ProbabilityOfTrue(0.4))
			{
				list.Add(new OverlayCross(_metal, width + 1, left, _gridSize.Height / 2.0, 0, 0));
				list.Add(new OverlayCross(_color2, width > 1 ? width - 1 : 1, left, _gridSize.Height / 2.0, 0, 0));
			}
			else
			{
				list.Add(new OverlayCross(_metal, width, left, _gridSize.Height / 2.0, 0, 0));
			}
		}

		private static int HoistElementWidth()
		{
			return (int)(_gridSize.Width * Randomizer.NextNormalized(0.35, 0.05));
		}

		private static void AddHoist(ICollection<Overlay> list, bool allowCanton)
		{
			// Made-up values
			var type = Randomizer.RandomWeighted(new List<int> { allowCanton ? 4 : 0, 3, 6 });
			var width = HoistElementWidth();

			_emblemColor = _metal;
			switch (type)
			{
				case 0: // Canton box
					var height = _gridSize.Height / 2.0;
					if (_divisionType == DivisionTypes.Stripes && _division.Values[1] > 1)
					{
						var stripe = (int)(_division.Values[1] / 2) + 1;
						height = _gridSize.Height * (stripe / _division.Values[1]);
						if (width < height) width = (int)height;
					}
					list.Add(new OverlayBox(_color2, 0, 0, width, height, 0, 0));
					AddEmblem(1.0, list, new Rect { Top = 0, Left = 0, Bottom = height, Right = width }, false);
					break;
				case 1: // Full hoist box
					list.Add(new OverlayBox(_color2, 0, 0, width, _gridSize.Height, 0, 0));
					AddEmblem(0.6, list, new Rect { Top = 0, Left = 0, Bottom = _gridSize.Height, Right = width }, true);
					break;
				case 2: // Triangle
					AddTriangle(list, width);
					break;
			}
		}

		private static void AddTriangle(ICollection<Overlay> list, double width)
		{
			list.Add(new OverlayTriangle(_color2, 0, 0, width, _gridSize.Height / 2.0, 0, _gridSize.Height, 0, 0));
			AddEmblem(0.5, list, new Rect { Top = 0, Left = 0, Bottom = _gridSize.Height, Right = width * 3 / 4.0 }, true);
		}

		private static void AddOverlaysX(ICollection<Overlay> list, bool allowExtra)
		{
			list.Add(new OverlaySaltire(_metal, _gridSize.Width / 3.0, 0, 0));
			if (allowExtra && Randomizer.ProbabilityOfTrue(0.1))
			{
				list.Add(new OverlayHalfSaltire(_color1, _gridSize.Width / 2.0, 0, 0));
				list.Add(new OverlayCross(_metal, _gridSize.Width / 10.0, _gridSize.Width / 2.0, _gridSize.Height / 2.0, 0, 0));
			}
		}

		private static void AddCircle(ICollection<Overlay> list, double x, double y, double emblemProbability, bool forceMetal)
		{
			if (forceMetal || Randomizer.ProbabilityOfTrue(0.5))
			{
				list.Add(new OverlayEllipse(_metal, x, y, _gridSize.Width * 0.35, 0, 0, 0));
				list.Add(new OverlayEllipse(_color2, x, y, _gridSize.Width * 0.3, 0, 0, 0));
			}
			else
			{
				list.Add(new OverlayEllipse(_color2, x, y, _gridSize.Width * 0.35, 0, 0, 0));
			}

			_emblemColor = _metal;
			_emblemX = x;
			_emblemY = y;
			AddEmblem(emblemProbability, list);
		}

		private static void AddDiamond(ICollection<Overlay> list, bool forceMetal)
		{
			if (forceMetal || Randomizer.ProbabilityOfTrue(0.5))
			{
				list.Add(new OverlayDiamond(_metal, _gridSize.Width / 2.0, _gridSize.Height / 2.0,
					_gridSize.Width * 3 / 4.0, _gridSize.Height * 3 / 4.0, 0, 0));
				list.Add(new OverlayDiamond(_color2, _gridSize.Width / 2.0, _gridSize.Height / 2.0,
					_gridSize.Width * 5 / 8.0, _gridSize.Height * 5 / 8.0, 0, 0));
			}
			else
			{
				list.Add(new OverlayDiamond(_color2, _gridSize.Width / 2.0, _gridSize.Height / 2.0,
					_gridSize.Width * 3 / 4.0, _gridSize.Height * 3 / 4.0, 0, 0));
			}

			_emblemColor = _metal;
			_emblemX = _gridSize.Width / 2.0;
			_emblemY = _gridSize.Height / 2.0;
			AddEmblem(0.9, list);
		}

		private static double AddPall(ICollection<Overlay> list, bool useBlack)
		{
			var totalWidth = _gridSize.Width / (Randomizer.ProbabilityOfTrue(0.6) ? 3.0 : 2.0);
			list.Add(new OverlayPall(useBlack ? Black : _metal, totalWidth, _gridSize.Width / 10.0, 0, 0));
			return totalWidth;
		}

		private static void AddEmblem(double probability, ICollection<Overlay> list)
		{
			const double size = 0.25;
			AddEmblem(probability, list, new Rect
										 {
											 Bottom = _emblemY + _gridSize.Height * size,
											 Top = _emblemY - _gridSize.Height * size,
											 Left = _emblemX - _gridSize.Width * size,
											 Right = _emblemX + _gridSize.Width * size,
										 }, false);
		}

		private static void AddEmblem(double probability, ICollection<Overlay> list, Rect rect, bool isSmall)
		{
			if (!Randomizer.ProbabilityOfTrue(probability)) return;

			var types = OverlayFactory.GetOverlaysByType(typeof(OverlayPath)).ToList();
			var type = types[Randomizer.Next(types.Count)];
			var emblem = (Overlay)Activator.CreateInstance(type, 0, 0);
			emblem.SetColors(new List<Color> { _emblemColor });
			emblem.SetValues(new List<double> { rect.Left + (rect.Right - rect.Left) / 2, rect.Top + (rect.Bottom - rect.Top) / 2, (rect.Bottom - rect.Top) / (isSmall ? 3.0 : 1.5), 0 });
			list.Add(emblem);
		}

		private enum DivisionTypes
		{
			Stripes,
			Pales,
			Fesses,
			DiagonalBackward,
			DiagonalForward,
			X,
			Horizontal,
			Vertical,
			Quartered,
			Blank
		}

		private struct Rect
		{
			public double Top;
			public double Bottom;
			public double Left;
			public double Right;
		}
	}
}