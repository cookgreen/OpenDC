﻿using System;
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
		private ushort compressedSize;
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
				ushort unknown3 = reader.ReadUInt16();
				ushort unknown4 = reader.ReadUInt16();
				frame.FrameSize = new Size(width, height);
				frame.Size = new Size(width, height);
				frame.Data = new byte[width * height * 4];
				frames.Add(frame);
			}

			for (int i = 0; i < FrameNum; i++)
			{
				compressedSize = reader.ReadUInt16();
				for (int k = 0; k < compressedSize;)
				{
					byte compression = reader.ReadByte();
					if (compression < 128)
					{
						try
						{
							// copy k bytes
							for (int j = 0; j < compression; j++)
							{
								frames[i].Data[j] = reader.ReadByte();
							}
						}
						catch
						{
							continue;
						}
					}
					else
					{
						k += 256 - compression;
					}
				}
			}

			return frames.ToArray();
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			string name = (s as FileStream).Name;
			if (Path.GetExtension(name) != ".SPR")
			{
				frames = null;
				metadata = new TypeDictionary();
				return false;
			}

			uint[] palette = new uint[256 * 3];

			using (BinaryReader reader = new BinaryReader(s))
			{
				compressedSize = reader.ReadUInt16();
				FrameNum = reader.ReadUInt16();
				ushort unknown2 = reader.ReadUInt16();

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
