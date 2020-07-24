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
		private uint compressedSize;
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
					return SpriteFrameType.Indexed;
				}
			}
		}

		ISpriteFrame[] ParseFrames(BinaryReader reader, int numFrames)
		{
			var frames = new List<SPRFrame>();

			for (int i = 0; i < numFrames; i++)
			{
				ushort width = reader.ReadUInt16();
				ushort height = reader.ReadUInt16();
				ushort offsetX = reader.ReadUInt16();
				ushort offsetY = reader.ReadUInt16();

				frames.Add(new SPRFrame
				{
					FrameSize = new Size(width, height),
					Size = new Size(width, height),
					Offset = new float2(offsetX, offsetY),
					Data = new byte[width * height]
				});
			}

			for (int i = 0; i < numFrames; i++)
			{
				compressedSize = reader.ReadUInt32();
				var writeOffset = 0;

				for (int j = 0; j < compressedSize;)
				{
					var compression = reader.ReadByte();
					var isEmpty = (compression & 0b10000000) != 0;
					var numPixels = compression & 0b01111111;

					if (isEmpty)
						writeOffset += 0;
					else
					{
						for (int k = 0; k < numPixels; k++)
							frames[i].Data[writeOffset++] = reader.ReadByte();
					}
				}
			}

			return frames.ToArray();
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			var name = (s as FileStream).Name;

			if (Path.GetExtension(name) != ".SPR")
			{
				frames = null;
				metadata = new TypeDictionary();
				return false;
			}

			var palette = new uint[256];

			using (BinaryReader reader = new BinaryReader(s))
			{
				var flags = reader.ReadUInt16();
				var numFrames = reader.ReadUInt16();
				var unkSize = reader.ReadUInt32(); // somewhat related to the data size

				for (int i = 0; i < palette.Length; i++)
				{
					var r = reader.ReadByte();
					var g = reader.ReadByte();
					var b = reader.ReadByte();
					palette[i] = (uint)((r << 24) | (g << 16) | (b << 8) | (i == 0 ? 0x00 : 0xff));
				}

				frames = ParseFrames(reader, numFrames);
			}

			metadata = new TypeDictionary { new EmbeddedSpritePalette(palette) };

			return true;
		}
	}
}
