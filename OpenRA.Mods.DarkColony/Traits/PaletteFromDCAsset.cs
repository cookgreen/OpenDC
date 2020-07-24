using System.Collections.Generic;
using System.IO;
using OpenRA.FileSystem;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.DarkColony.Traits
{
	class PaletteFromDCAssetInfo : ITraitInfo, IProvidesCursorPaletteInfo
	{
		[FieldLoader.Require]
		[PaletteDefinition]
		[Desc("Internal palette name")]
		public readonly string Name = null;

		[FieldLoader.Require]
		[Desc("Filename to load")]
		public readonly string Filename = null;

		public readonly bool AllowModifiers = true;

		public object Create(ActorInitializer init) { return new PaletteFromDCAsset(init.World, this); }

		string IProvidesCursorPaletteInfo.Palette => Name;

		ImmutablePalette IProvidesCursorPaletteInfo.ReadPalette(IReadOnlyFileSystem fileSystem)
		{
			var colors = new uint[Palette.Size];

			using (var reader = new BinaryReader(fileSystem.Open(Filename)))
			{
				reader.BaseStream.Position = 8;

				for (var i = 0; i < colors.Length; i++)
				{
					var r = reader.ReadByte() << 2;
					var g = reader.ReadByte() << 2;
					var b = reader.ReadByte() << 2;
					colors[i] = (uint)((r << 16) | (g << 8) | (b << 0) | (0xff << 24));
				}
			}

			if (Filename.EndsWith(".spr"))
				colors[0] = 0;

			return new ImmutablePalette(colors);
		}
	}

	class PaletteFromDCAsset : ILoadsPalettes, IProvidesAssetBrowserPalettes
	{
		private readonly World world;
		private readonly PaletteFromDCAssetInfo info;

		public PaletteFromDCAsset(World world, PaletteFromDCAssetInfo info)
		{
			this.world = world;
			this.info = info;
		}

		public void LoadPalettes(WorldRenderer wr)
		{
			wr.AddPalette(info.Name, ((IProvidesCursorPaletteInfo)info).ReadPalette(world.Map), info.AllowModifiers);
		}

		public IEnumerable<string> PaletteNames { get { yield return info.Name; } }
	}
}
