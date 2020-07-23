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
	public class SPRLoader : ISpriteLoader
	{
		public ushort FrameNum { get; set; }

		class SPRFrame : ISpriteFrame
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
			List<SPRFrame> frames = new List<SPRFrame>();

			for (int i = 0; i < FrameNum; i++)
			{
				SPRFrame frame = new SPRFrame();
				ushort width = reader.ReadUInt16();
				ushort height = reader.ReadUInt16();
				var unknown1 = reader.ReadUInt16();
				var unknown2 = reader.ReadUInt16();
				frame.Size = new Size(width, height);
				frame.Data = new byte[width * height];
				frames.Add(frame);
			}

			for (int i = 0; i < FrameNum; i++)
			{
				for (int j = 0; j < frames[i].Data.Length; j++)
				{
					frames[i].Data[j] = reader.ReadByte();
				}
			}

			return frames.ToArray();
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
        {
            uint[] palette = new uint[256 * 3];
            using (BinaryReader reader = new BinaryReader(s))
			{
				ushort unknown = reader.ReadUInt16();
				FrameNum = reader.ReadUInt16();
				unknown = reader.ReadUInt16();

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
