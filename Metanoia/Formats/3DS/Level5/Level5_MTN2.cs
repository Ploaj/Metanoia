using System;
using System.Collections.Generic;
using Metanoia.Modeling;
using Metanoia.Tools;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_MTN2 : IAnimationFormat
    {
        public string Name => "Level5 Motion";
        public string Extension => ".mtn2";
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

        public void Open(FileItem file)
        {
            anim.Name = file.FilePath;

            using(DataReader r = new DataReader(file))
            {
                r.BigEndian = false;

                r.Seek(0x08);
                var decomSize = r.ReadInt32();
                var nameOffset = r.ReadUInt32();
                var compDataOffset = r.ReadUInt32();
                var positionCount = r.ReadInt32();
                var rotationCount = r.ReadInt32();
                var scaleCount = r.ReadInt32();
                var unknownCount = r.ReadInt32();
                var boneCount = r.ReadInt32();

                r.Seek(0x54);
                anim.FrameCount = r.ReadInt32();

                r.Seek(nameOffset);
                var hash = r.ReadUInt32();
                anim.Name = r.ReadString(r.Position, -1);
                
                var data = Decompress.Level5Decom(r.GetSection(compDataOffset, (int)(r.Length - compDataOffset)));

                using (DataReader d = new DataReader(data))
                {
                    // Header
                    var boneHashTableOffset = d.ReadUInt32();
                    var trackInfoOffset = d.ReadUInt32();
                    var dataOffset = d.ReadUInt32();

                    // Bone Hashes
                    List<uint> boneNameHashes = new List<uint>();
                    d.Seek(boneHashTableOffset);
                    while (d.Position < trackInfoOffset)
                        boneNameHashes.Add(d.ReadUInt32());
                    
                    // Track Information
                    List<AnimTrack> Tracks = new List<AnimTrack>();
                    for (int i = 0; i < 4; i++)
                    {
                        d.Seek((uint)(trackInfoOffset + 2 * i));
                        d.Seek(d.ReadUInt16());
                        
                        Tracks.Add(new AnimTrack()
                        {
                            Type = d.ReadByte(),
                            DataType = d.ReadByte(),
                            unk = d.ReadByte(),
                            DataCount = d.ReadByte(),
                            Start = d.ReadUInt16(),
                            End = d.ReadUInt16()
                        });
                    }

                    foreach (var v in Tracks)
                        Console.WriteLine(v.Type + " " 
                            + v.DataType + " " 
                            + v.DataCount 
                            + " " + v.Start.ToString("X")
                             + " " + v.End.ToString("X"));

                    // Data

                    foreach(var v in boneNameHashes)
                    {
                        var node = new GenericAnimationTransform();
                        node.Hash = v;
                        node.HashType = AnimNodeHashType.CRC32C;
                        anim.TransformNodes.Add(node);
                    }
                    
                    var offset = 0;
                    ReadFrameData(d, offset, positionCount, dataOffset, boneCount, Tracks[0]);
                    offset += positionCount;
                    ReadFrameData(d, offset, rotationCount, dataOffset, boneCount, Tracks[1]);
                    offset += rotationCount;
                    ReadFrameData(d, offset, scaleCount, dataOffset, boneCount, Tracks[2]);
                    offset += scaleCount;
                    //ReadFrameData(d, unknownCount, dataOffset, boneCount, Tracks[3]);
                }
            }
        }

        private void ReadFrameData(DataReader d, int offset, int count, uint dataOffset, int boneCount, AnimTrack Track)
        {
            for (int i = offset; i < offset + count; i++)
            {
                d.Seek((uint)(dataOffset + 4 * 4 * i));
                var flagOffset = d.ReadUInt32();
                var keyFrameOffset = d.ReadUInt32();
                var keyDataOffset = d.ReadUInt32();

                d.Seek(flagOffset);
                var boneIndex = d.ReadInt16();
                var keyFrameCount = d.ReadByte();
                var flag = d.ReadByte();
                
                var node = anim.TransformNodes[boneIndex + (flag == 0 ? boneCount : 0)];

                d.Seek(keyDataOffset);
                for (int k = 0; k < keyFrameCount; k++)
                {
                    var temp = d.Position;
                    d.Seek((uint)(keyFrameOffset + k * 2));
                    var frame = d.ReadInt16();
                    d.Seek(temp);

                    float[] animdata = new float[Track.DataCount];
                    for (int j = 0; j < Track.DataCount; j++)
                        switch (Track.DataType)
                        {
                            case 1:
                                animdata[j] = d.ReadInt16() / (float)short.MaxValue;
                                break;
                            case 2:
                                animdata[j] = d.ReadSingle();
                                break;
                            case 4:
                                animdata[j] = d.ReadInt16();
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
