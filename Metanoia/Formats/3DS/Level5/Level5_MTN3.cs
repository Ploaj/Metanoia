using System;
using System.Linq;
using System.Collections.Generic;
using Metanoia.Modeling;
using Metanoia.Tools;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_MTN3 : IAnimationFormat
    {
        public string Name => "Level5 Motion";
        public string Extension => ".mtn3";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

        private GenericAnimation anim = new GenericAnimation();

        public class AnimTrack
        {
            public int Type;
            public int DataType;
            public int unk;
            public int DataCount;
            public int Start;
            public int End;
        }

        public class AnimTableOffset
        {
            public int FlagOffset;
            public int KeyFrameOffset;
            public int KeyDataOffset;
        }

        public void Open(string name, byte[] fileContent)
        {
            anim.Name = name;

            using (DataReader reader = new DataReader(fileContent))
            {
                reader.BigEndian = false;

                reader.Seek(0x04);
                int hashOffset = reader.ReadInt16() - 4;
                int nameOffset = reader.ReadInt16() - 4;
                int unkOffset = reader.ReadInt16();
                reader.Skip(0x06);
                int compDataLength = reader.ReadInt32();
                reader.Skip(0x04);
                int positionCount = reader.ReadInt32();
                int rotationCount = reader.ReadInt32();
                int scaleCount = reader.ReadInt32();
                int unknownCount = reader.ReadInt32();
                int boneCount = reader.ReadInt32();

                reader.Seek((uint)hashOffset);
                var hash = reader.ReadUInt32();
                anim.Name = reader.ReadString(reader.Position, -1);

                reader.Seek((uint)0x58);
                Console.WriteLine("hashOffset = " + hashOffset);
                anim.FrameCount = reader.ReadInt32();
                short positionTrackOffset = reader.ReadInt16();
                short rotationTrackOffset = reader.ReadInt16();
                short scaleTrackOffset = reader.ReadInt16();
                short unknownTrackOffset = reader.ReadInt16();

                List<AnimTableOffset> animTableOffset = new List<AnimTableOffset>();
                for (int i = 0; i < positionCount + rotationCount + scaleCount + unknownCount; i++)
                {
                    animTableOffset.Add(new AnimTableOffset()
                    {
                        FlagOffset = reader.ReadInt32(),
                        KeyFrameOffset = reader.ReadInt32(),
                        KeyDataOffset = reader.ReadInt32(),
                    });
                    reader.Skip(0x04);
                }

                using (DataReader dataReader = new DataReader(Decompress.Level5Decom(reader.GetSection(reader.Position, (int)(reader.Length - reader.Position)))))
                {
                    // Bone Hashes
                    List<uint> boneNameHashes = new List<uint>();
                    for (int i = 0; i < boneCount; i++)
                    {
                        boneNameHashes.Add(dataReader.ReadUInt32());
                    }

                    // Track Information
                    List<AnimTrack> Tracks = new List<AnimTrack>();
                    dataReader.Seek((uint)positionTrackOffset);
                    for (int i = 0; i < 4; i++)
                    {
                        Tracks.Add(new AnimTrack()
                        {
                            Type = dataReader.ReadByte(),
                            DataType = dataReader.ReadByte(),
                            unk = dataReader.ReadByte(),
                            DataCount = dataReader.ReadByte(),
                            Start = dataReader.ReadUInt16(),
                            End = dataReader.ReadUInt16()
                        });
                    }

                    foreach (var v in Tracks)
                        Console.WriteLine(v.Type + " "
                            + v.DataType + " "
                            + v.DataCount
                            + " " + v.Start.ToString("X")
                             + " " + v.End.ToString("X"));

                    // Data

                    foreach (var v in boneNameHashes)
                    {
                        var node = new GenericAnimationTransform();
                        node.Hash = v;
                        node.HashType = AnimNodeHashType.CRC32C;
                        anim.TransformNodes.Add(node);
                    }

                    using (DataReader animDataReader = new DataReader(dataReader.GetSection(dataReader.Position, (int)(dataReader.Length - dataReader.Position))))
                    {
                        ReadFrameData(animDataReader, animTableOffset.Take(positionCount).ToList(), boneCount, Tracks[0]);
                        ReadFrameData(animDataReader, animTableOffset.Skip(positionCount).Take(rotationCount).ToList(), boneCount, Tracks[1]);
                        ReadFrameData(animDataReader, animTableOffset.Skip(positionCount + rotationCount).Take(scaleCount).ToList(), boneCount, Tracks[2]);
                    }
                }
            }
        }

        public void Open(FileItem file)
        {
            Open(file.FilePath, file.GetFileBinary());
        }

        private void ReadFrameData(DataReader dataReader, List<AnimTableOffset> animTableOffset, int boneCount, AnimTrack Track)
        {
            for (int i = 0; i < animTableOffset.Count; i++)
            {
                dataReader.Seek((uint)animTableOffset[i].FlagOffset);
                var boneIndex = dataReader.ReadInt16();
                var keyFrameCount = dataReader.ReadByte();
                var flag = dataReader.ReadByte();

                var node = anim.TransformNodes[boneIndex + (flag == 0 ? boneCount : 0)];

                dataReader.Seek((uint)animTableOffset[i].KeyDataOffset);
                for (int k = 0; k < keyFrameCount; k++)
                {
                    var temp = dataReader.Position;
                    dataReader.Seek((uint)(animTableOffset[i].KeyFrameOffset + k * 2));
                    var frame = dataReader.ReadInt16();
                    dataReader.Seek(temp);

                    float[] animdata = new float[Track.DataCount];
                    for (int j = 0; j < Track.DataCount; j++)
                        switch (Track.DataType)
                        {
                            case 1:
                                animdata[j] = dataReader.ReadInt16() / (float)short.MaxValue;
                                break;
                            case 2:
                                animdata[j] = dataReader.ReadSingle();
                                break;
                            case 4:
                                animdata[j] = dataReader.ReadInt16();
                                break;
                            default:
                                throw new NotImplementedException("Data Type " + Track.DataType + " not implemented");
                        }

                    switch (Track.Type)
                    {
                        case 1:
                            node.AddKey(frame, animdata[0], AnimationTrackFormat.TranslateX);
                            node.AddKey(frame, animdata[1], AnimationTrackFormat.TranslateY);
                            node.AddKey(frame, animdata[2], AnimationTrackFormat.TranslateZ);
                            break;
                        case 2:
                            var e = GenericBone.ToEulerAngles(new OpenTK.Quaternion(animdata[0], animdata[1], animdata[2], animdata[3]).Inverted());
                            node.AddKey(frame, e.X, AnimationTrackFormat.RotateX);
                            node.AddKey(frame, e.Y, AnimationTrackFormat.RotateY);
                            node.AddKey(frame, e.Z, AnimationTrackFormat.RotateZ);
                            break;
                        case 3:
                            node.AddKey(frame, animdata[0], AnimationTrackFormat.ScaleX);
                            node.AddKey(frame, animdata[1], AnimationTrackFormat.ScaleY);
                            node.AddKey(frame, animdata[2], AnimationTrackFormat.ScaleZ);
                            break;
                    }
                }
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericAnimation ToGenericAnimation()
        {
            return anim;
        }

        public bool Verify(FileItem file)
        {
            return (file.MagicString == "XMTN");
        }
    }
}
