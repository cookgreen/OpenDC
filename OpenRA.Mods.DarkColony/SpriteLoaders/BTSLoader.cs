using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.DarkColony.SpriteLoaders
{
	public class BTSLoader : ISpriteLoader
	{
		public int TilesNum { get; set; }
		class BTSFrame : ISpriteFrame
		{
			public Size Size { get; set; }

			public Size FrameSize { get; set; }

			public float2 Offset { get; set; }

			public byte[] Data { get; set; }

			public bool DisableExportPadding { get; set; }

			public SpriteFrameType Type
			{
				get
				{
					return SpriteFrameType.BGRA;
				}
			}
		}

		ISpriteFrame[] ParseFrames(BinaryReader reader)
		{
			List<BTSFrame> frames = new List<BTSFrame>();
			for (int i = 0; i < TilesNum; i++)
			{
				int id = reader.ReadInt32();
				BTSFrame frame = new BTSFrame();
				frame.Size = new Size(32, 32);
				frame.Data = new byte[32 * 32];
				for (int j = 0; j < frame.Data.Length; j++)
				{
					frame.Data[j] = reader.ReadByte();
				}

				frames.Add(frame);
			}

			return frames.ToArray();
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
        {
            uint[] palette = new uint[256 * 3];
            using (BinaryReader reader = new BinaryReader(s))
			{
				int unknown = reader.ReadInt32();
				TilesNum = reader.ReadInt32();

				for (int i = 0; i < palette.Length; i++)
				{
					palette[i] = reader.ReadByte();
				}

				frames = ParseFrames(reader);
            }

            metadata = new TypeDictionary { new EmbeddedSpritePalette(palette) };

            return true;
		}
	}
}
