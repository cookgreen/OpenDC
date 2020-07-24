using System;
using System.IO;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.DarkColony.SpriteLoaders
{
	public class SPRLoader : ISpriteLoader
	{
		class SPRFrame : ISpriteFrame
		{
			public Size Size { get; set; }

			public Size FrameSize { get; set; }

			public float2 Offset { get; set; }

			public byte[] Data { get; set; }

			public bool DisableExportPadding { get; set; }

			public SpriteFrameType Type => SpriteFrameType.Indexed;
		}

		private static ISpriteFrame[] ParseFrames(BinaryReader reader, int numFrames, bool isCompressed)
		{
			var frames = new SPRFrame[numFrames];

			for (var i = 0; i < numFrames; i++)
			{
				var width = reader.ReadUInt16();
				var height = reader.ReadUInt16();
				var offsetX = reader.ReadUInt16();
				var offsetY = reader.ReadUInt16();

				frames[i] = new SPRFrame
				{
					FrameSize = new Size(width, height),
					Size = new Size(width, height),
					Offset = new float2(offsetX, offsetY),
					Data = new byte[width * height]
				};
			}

			for (var i = 0; i < numFrames; i++)
			{
				if (!isCompressed)
					frames[i].Data = reader.ReadBytes(frames[i].Size.Width * frames[i].Size.Height);
				else
				{
					var compressedSize = reader.ReadUInt32();
					var writeOffset = 0;

					for (var readOffset = 0; readOffset < compressedSize; readOffset++)
					{
						var compression = reader.ReadByte();
						var isEmpty = (compression & 0b10000000) != 0;
						var numPixels = compression & 0b01111111;

						if (isEmpty)
							writeOffset += 128 - numPixels;
						else
						{
							numPixels += 1;
							readOffset += numPixels;

							for (var k = 0; k < numPixels; k++)
								frames[i].Data[writeOffset++] = reader.ReadByte();
						}
					}
				}
			}

			return frames;
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			metadata = new TypeDictionary();

			if (Path.GetExtension(((FileStream)s).Name) != ".SPR")
			{
				frames = null;
				return false;
			}

			using (var reader = new BinaryReader(s))
			{
				var flags = reader.ReadUInt16();
				var numFrames = reader.ReadUInt16();
				var unkSize = reader.ReadUInt32(); // TODO could be filesize related

				reader.BaseStream.Position += 256 * 3; // We skip the palette here...

				var isCompressed = (flags & 0b10000000) != 0;
				var unknownFlag = (flags & 0b00000001) != 0; // TODO could be transparency related

				if ((flags & 0b01111110) != 0)
					throw new Exception("Unknown flags!");

				frames = ParseFrames(reader, numFrames, isCompressed);
			}

			return true;
		}
	}
}
