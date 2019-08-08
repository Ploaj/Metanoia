using System;
using System.IO;
using OpenTK;
using System.Globalization;
using System.Collections.Generic;
using Metanoia.Modeling;
using System.Linq;

public class FFST
{
    public static void Process(string[] args)
    {
        FileStream fileStream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        StreamWriter streamWriter = null;
        StreamWriter streamWriter2 = null;
        Vector3[] array = null;
        Quaternion[] array2 = null;
        int num = binaryReader.ReadInt32();
        if (args[0].EndsWith("bin.gz"))
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(args[0]);
            int num2 = num;
            int[] array3 = new int[num2];
            int[] array4 = new int[num2];
            for (int i = 0; i < num2; i++)
            {
                array3[i] = binaryReader.ReadInt32();
                array4[i] = binaryReader.ReadInt32();
            }
            if (!Directory.Exists(fileNameWithoutExtension))
            {
                Directory.CreateDirectory(fileNameWithoutExtension);
            }
            for (int i = 0; i < num2; i++)
            {
                fileStream.Seek(array3[i], SeekOrigin.Begin);
                int num3 = binaryReader.ReadInt32();
                string str = ".dat";
                switch (num3)
                {
                    case 1194415175:
                        str = ".g1t";
                        break;
                    case 1194413407:
                        str = ".g1m";
                        break;
                }
                fileStream.Seek(array3[i], SeekOrigin.Begin);
                byte[] array5 = new byte[array4[i]];
                fileStream.Read(array5, 0, array4[i]);
                File.WriteAllBytes(fileNameWithoutExtension + "\\" + i.ToString("d2") + str, array5);
            }
            return;
        }
        switch (num)
        {
            case 1263553863:
                {
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    int num110 = binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    int num111 = binaryReader.ReadInt32();
                    fileStream.Seek(num111, SeekOrigin.Begin);
                    int num112 = binaryReader.ReadInt32();
                    int num113 = binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    int num114 = binaryReader.ReadInt32();
                    fileStream.Seek(num113, SeekOrigin.Begin);
                    binaryReader.ReadInt64();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    int num115 = binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    for (int i = 0; i < num115; i++)
                    {
                        binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                    }
                    long position3 = fileStream.Position;
                    int[] array60 = new int[num115];
                    int[] array61 = new int[num115];
                    for (int i = 0; i < num115; i++)
                    {
                        array60[i] = binaryReader.ReadInt16();
                        array61[i] = binaryReader.ReadInt16();
                    }
                    string[] array62 = new string[num115];
                    string[] array63 = new string[num115];
                    for (int i = 0; i < num115; i++)
                    {
                        fileStream.Seek(position3 + array60[i], SeekOrigin.Begin);
                        int num116 = binaryReader.ReadByte();
                        for (int j = 0; j < num116; j++)
                        {
                            string[] array64;
                            string[] array65 = array64 = array62;
                            int num117 = i;
                            IntPtr intPtr = (IntPtr)num117;
                            array65[num117] = array64[(long)intPtr] + (char)binaryReader.ReadByte();
                        }
                        fileStream.Seek(position3 + array61[i], SeekOrigin.Begin);
                        num116 = binaryReader.ReadByte();
                        for (int j = 0; j < num116; j++)
                        {
                            string[] array64;
                            string[] array66 = array64 = array63;
                            int num118 = i;
                            IntPtr intPtr = (IntPtr)num118;
                            array66[num118] = array64[(long)intPtr] + (char)binaryReader.ReadByte();
                        }
                        Console.WriteLine(array63[i] + array62[i]);
                    }
                    fileStream.Seek(num110, SeekOrigin.Begin);
                    int[] array67 = new int[num114];
                    int[] array68 = new int[num114];
                    for (int i = 0; i < num114; i++)
                    {
                        array67[i] = binaryReader.ReadInt32();
                        array68[i] = binaryReader.ReadInt32();
                    }
                    fileStream.Seek(num112, SeekOrigin.Begin);
                    binaryReader.ReadInt64();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    int num119 = binaryReader.ReadInt32();
                    int num120 = binaryReader.ReadInt32() / 2;
                    binaryReader.ReadInt32();
                    fileStream.Seek(num112 + binaryReader.ReadInt32(), SeekOrigin.Begin);
                    int[,] array69 = new int[num120, num119];
                    int[,] array70 = new int[num120, num119];
                    for (int i = 0; i < num119; i++)
                    {
                        for (int j = 0; j < num120; j++)
                        {
                            array69[j, i] = binaryReader.ReadInt32();
                            array70[j, i] = binaryReader.ReadInt32();
                        }
                    }
                    if (num119 != num115)
                    {
                        Console.WriteLine("Error. Names!=Entries");
                    }
                    for (int i = 0; i < num119; i++)
                    {
                        if (array69[0, i] > 0)
                        {
                            int num121 = array68[array70[0, i]];
                            fileStream.Seek(num110 + array67[array70[0, i]], SeekOrigin.Begin);
                            byte[] array71 = new byte[num121];
                            fileStream.Read(array71, 0, num121);
                            File.WriteAllBytes(array63[i] + array62[i] + ".g1m", array71);
                        }
                        if (array69[1, i] > 0)
                        {
                            int num122 = array68[array70[1, i]];
                            fileStream.Seek(num110 + array67[array70[1, i]], SeekOrigin.Begin);
                            byte[] array72 = new byte[num122];
                            fileStream.Read(array72, 0, num122);
                            File.WriteAllBytes(array63[i] + array62[i] + ".g1t", array72);
                        }
                    }
                    break;
                }
            case 1380729933:
                {
                    string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(args[0]);
                    fileStream.Seek(32L, SeekOrigin.Begin);
                    int num123 = binaryReader.ReadInt32();
                    fileStream.Seek(num123, SeekOrigin.Begin);
                    int num124 = binaryReader.ReadInt32();
                    fileStream.Seek(num124 + 20, SeekOrigin.Begin);
                    int num125 = binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    int num126 = binaryReader.ReadInt32();
                    int num127 = binaryReader.ReadInt32();
                    int[] array73 = new int[num125];
                    int[] array74 = new int[num125];
                    fileStream.Seek(num124 + num126, SeekOrigin.Begin);
                    for (int i = 0; i < num125; i++)
                    {
                        array73[i] = binaryReader.ReadInt32();
                    }
                    fileStream.Seek(num124 + num127, SeekOrigin.Begin);
                    for (int i = 0; i < num125; i++)
                    {
                        array74[i] = binaryReader.ReadInt32();
                    }
                    if (!Directory.Exists(fileNameWithoutExtension2))
                    {
                        Directory.CreateDirectory(fileNameWithoutExtension2);
                    }
                    for (int i = 0; i < num125; i++)
                    {
                        fileStream.Seek(num124 + array73[i], SeekOrigin.Begin);
                        byte[] array75 = new byte[array74[i]];
                        fileStream.Read(array75, 0, array74[i]);
                        File.WriteAllBytes(fileNameWithoutExtension2 + "\\" + i.ToString("d2") + ".g1m", array75);
                    }
                    break;
                }
            case 1194413407:
                {
                    float num4 = 0f;
                    float num5 = 0f;
                    float zz = 0f;
                    NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
                    numberFormatInfo.NumberDecimalSeparator = ".";
                    MemoryStream memoryStream = null;
                    fileStream.Seek(16L, SeekOrigin.Current);
                    int num6 = binaryReader.ReadInt32();
                    long num7 = -1L;
                    long num8 = -1L;
                    long num9 = -1L;
                    for (int i = 0; i < num6; i++)
                    {
                        long position = fileStream.Position;
                        int num10 = binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                        int num11 = binaryReader.ReadInt32();
                        switch (num10)
                        {
                            case 1194413395:
                                {
                                    byte[] buffer = new byte[num11 - 12];
                                    fileStream.Read(buffer, 0, num11 - 12);
                                    memoryStream = new MemoryStream(buffer);
                                    break;
                                }
                            case 1194413383:
                                num7 = position;
                                break;
                            case 1314213455:
                                num8 = position;
                                break;
                            case 1314213462:
                                num9 = position;
                                break;
                        }
                        fileStream.Seek(position + num11, SeekOrigin.Begin);
                    }
                    if (args.Length > 1)
                    {
                        FileStream fileStream2 = new FileStream(args[1], FileMode.Open, FileAccess.Read);
                        BinaryReader binaryReader2 = new BinaryReader(fileStream2);
                        num = binaryReader2.ReadInt32();
                        if (num == 1194413407)
                        {
                            fileStream2.Seek(16L, SeekOrigin.Current);
                            num6 = binaryReader2.ReadInt32();
                            for (int i = 0; i < num6; i++)
                            {
                                long position = fileStream2.Position;
                                int num12 = binaryReader2.ReadInt32();
                                binaryReader2.ReadInt32();
                                int num13 = binaryReader2.ReadInt32();
                                if (num12 == 1194413395)
                                {
                                    byte[] buffer2 = new byte[num13 - 12];
                                    fileStream2.Read(buffer2, 0, num13 - 12);
                                    memoryStream = new MemoryStream(buffer2);
                                }
                                fileStream2.Seek(position + num13, SeekOrigin.Begin);
                            }
                        }
                    }
                    Vector3[][] array6 = null;
                    Vector3[][] array7 = null;
                    int[] array8 = null;
                    int[] array9 = null;
                    int[] array10 = null;
                    int[] array11 = null;
                    if (memoryStream != null)
                    {
                        BinaryReader binaryReader3 = new BinaryReader(memoryStream);
                        int num14 = binaryReader3.ReadInt32();
                        binaryReader3.ReadInt32();
                        int num15 = binaryReader3.ReadInt16();
                        int num16 = binaryReader3.ReadInt16();
                        binaryReader3.ReadInt32();
                        array10 = new int[num16];
                        for (int i = 0; i < num16; i++)
                        {
                            array10[i] = binaryReader3.ReadInt16();
                        }
                        memoryStream.Seek(num14 - 12, SeekOrigin.Begin);
                        Vector3[] array12 = new Vector3[num15];
                        Quaternion[] array13 = new Quaternion[num15];
                        array = new Vector3[num15];
                        array2 = new Quaternion[num15];
                        array11 = new int[num15];
                        for (int j = 0; j < num15; j++)
                        {
                            binaryReader3.ReadSingle();
                            binaryReader3.ReadSingle();
                            binaryReader3.ReadSingle();
                            array11[j] = binaryReader3.ReadInt32();
                            num4 = binaryReader3.ReadSingle();
                            num5 = binaryReader3.ReadSingle();
                            zz = binaryReader3.ReadSingle();
                            float real = binaryReader3.ReadSingle();
                            array13[j] = new Quaternion(real, num4, num5, zz);
                            num4 = binaryReader3.ReadSingle();
                            num5 = binaryReader3.ReadSingle();
                            zz = binaryReader3.ReadSingle();
                            real = binaryReader3.ReadSingle();
                            array12[j] = new Vector3(num4, num5, zz);
                        }
                        for (int i = 0; i < num15; i++)
                        {
                            if (array11[i] < 0)
                            {
                                array[i] = array12[i];
                                array2[i] = array13[i];
                                continue;
                            }
                            int num17 = array11[i];
                            array2[i] = array2[num17] * array13[i];
                            Quaternion right = new Quaternion(array12[i], 0f);
                            Quaternion left = array2[num17] * right;
                            Quaternion quaternion3D = left * new Quaternion(array2[num17].W, 0f - array2[num17].X, 0f - array2[num17].Y, 0f - array2[num17].Z);
                            array[i] = quaternion3D.Xyz;
                            Vector3[] array14;
                            Vector3[] array15 = array14 = array;
                            int num18 = i;
                            IntPtr intPtr = (IntPtr)num18;
                            array15[num18] = array14[(long)intPtr] + array[num17];
                        }
                        streamWriter2 = new StreamWriter(args[0] + ".smd");
                        streamWriter = new StreamWriter(Path.GetDirectoryName(fileStream.Name) + "\\" + Path.GetFileNameWithoutExtension(fileStream.Name) + ".ascii");
                        streamWriter2.WriteLine("version 1");
                        streamWriter2.WriteLine("nodes");
                        for (int j = 0; j < num15; j++)
                        {
                            streamWriter2.WriteLine(j + " \"b_" + j.ToString("X4") + "\" " + array11[j]);
                        }
                        streamWriter2.WriteLine("end");
                        streamWriter2.WriteLine("skeleton");
                        streamWriter2.WriteLine("time 0");
                        streamWriter.WriteLine(num15);
                        for (int j = 0; j < num15; j++)
                        {
                            streamWriter.WriteLine("b_" + j.ToString("X4"));
                            streamWriter.WriteLine(array11[j]);
                            streamWriter.Write(array[j].X.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array[j].Y.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array[j].Z.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array2[j].X.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array2[j].Y.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array2[j].Z.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array2[j].W.ToString("0.######", numberFormatInfo));
                            streamWriter.WriteLine();
                            streamWriter2.Write(j + "  ");
                            var temp = new GenericBone();
                            temp.Transform = Matrix4.Identity;
                            temp.QuaternionRotation = array13[j];
                            Vector3 vector3D = temp.Rotation;
                            streamWriter2.Write(array12[j].X.ToString("0.000000", numberFormatInfo));
                            streamWriter2.Write(" " + array12[j].Y.ToString("0.000000", numberFormatInfo));
                            streamWriter2.Write(" " + array12[j].Z.ToString("0.000000", numberFormatInfo));
                            streamWriter2.Write("  " + vector3D.X.ToString("0.000000", numberFormatInfo));
                            streamWriter2.Write(" " + vector3D.Y.ToString("0.000000", numberFormatInfo));
                            streamWriter2.WriteLine(" " + vector3D.Z.ToString("0.000000", numberFormatInfo));
                        }
                        streamWriter2.WriteLine("end");
                    }
                    if (num8 > 0)
                    {
                        fileStream.Seek(num8 + 4, SeekOrigin.Begin);
                        int num19 = binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                        int num20 = binaryReader.ReadInt32();
                        for (int k = 0; k < num20; k++)
                        {
                            long position = fileStream.Position;
                            int num21 = binaryReader.ReadInt32();
                            int num22 = binaryReader.ReadInt32();
                            if (num21 == 196609)
                            {
                                int num23 = binaryReader.ReadInt32();
                                array6 = new Vector3[num23][];
                                array8 = new int[num23];
                                for (int i = 0; i < num23; i++)
                                {
                                    array8[i] = (binaryReader.ReadInt32() & int.MaxValue);
                                    int num24 = binaryReader.ReadInt32();
                                    int num25 = binaryReader.ReadInt32();
                                    int num26 = binaryReader.ReadInt32();
                                    int num27 = binaryReader.ReadInt32();
                                    int num28 = binaryReader.ReadInt32();
                                    fileStream.Seek(76L, SeekOrigin.Current);
                                    if (num19 >= 808464949)
                                    {
                                        fileStream.Seek(16L, SeekOrigin.Current);
                                    }
                                    array6[i] = new Vector3[num24];
                                    for (int j = 0; j < num24; j++)
                                    {
                                        array6[i][j] = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                                        binaryReader.ReadSingle();
                                    }
                                    Vector3[] array16 = new Vector3[num24];
                                    int[] array17 = new int[num24];
                                    int[] array18 = new int[num24];
                                    int[] array19 = new int[num24];
                                    int[] array20 = new int[num24];
                                    int num29 = 0;
                                    for (int j = 0; j < num24; j++)
                                    {
                                        array19[j] = binaryReader.ReadInt32();
                                        array20[j] = binaryReader.ReadInt32();
                                        array17[j] = binaryReader.ReadInt32();
                                        array18[j] = binaryReader.ReadInt32();
                                        binaryReader.ReadSingle();
                                        binaryReader.ReadSingle();
                                        if (array17[j] >= 0 && array19[j] >= 0)
                                        {
                                            num29++;
                                        }
                                        if (array18[j] >= 0 && array20[j] >= 0)
                                        {
                                            num29++;
                                        }
                                        if (array8[i] >= 0 && array8[i] < array2.Length)
                                        {
                                            Quaternion quaternion3D2 = array2[array8[i]];
                                            Quaternion right = new Quaternion(array6[i][j], 0f);
                                            Quaternion left = quaternion3D2 * right;
                                            Quaternion quaternion3D = left * new Quaternion(quaternion3D2.W, 0f - quaternion3D2.X, 0f - quaternion3D2.Y, 0f - quaternion3D2.Z);
                                            array16[j] = quaternion3D.Xyz + array[array8[i]];
                                        }
                                        else
                                        {
                                            array16[j] = array6[i][j];
                                        }
                                    }
                                    StreamWriter streamWriter3 = new StreamWriter(args[0] + "_driver" + i + ".ascii");
                                    streamWriter3.WriteLine(num24);
                                    for (int j = 0; j < num24; j++)
                                    {
                                        streamWriter3.WriteLine("drv_" + i + "_bone_" + j);
                                        streamWriter3.WriteLine(array17[j]);
                                        streamWriter3.Write(array16[j].X.ToString("0.######", numberFormatInfo));
                                        streamWriter3.Write(" " + array16[j].Y.ToString("0.######", numberFormatInfo));
                                        streamWriter3.WriteLine(" " + array16[j].Z.ToString("0.######", numberFormatInfo));
                                    }
                                    streamWriter3.WriteLine(1);
                                    streamWriter3.WriteLine("driver_" + i);
                                    streamWriter3.WriteLine(0);
                                    streamWriter3.WriteLine(0);
                                    streamWriter3.WriteLine(num24);
                                    for (int j = 0; j < num24; j++)
                                    {
                                        streamWriter3.Write(array16[j].X.ToString("0.######", numberFormatInfo));
                                        streamWriter3.Write(" " + array16[j].Y.ToString("0.######", numberFormatInfo));
                                        streamWriter3.WriteLine(" " + array16[j].Z.ToString("0.######", numberFormatInfo));
                                        streamWriter3.WriteLine("0 0 0");
                                        streamWriter3.WriteLine("0 0 0 0");
                                        streamWriter3.WriteLine(j + " 0 0 0");
                                        streamWriter3.WriteLine("1 0 0 0");
                                    }
                                    streamWriter3.WriteLine(num29);
                                    for (int j = 0; j < num24; j++)
                                    {
                                        if (array17[j] >= 0 && array19[j] >= 0)
                                        {
                                            streamWriter3.Write(j);
                                            streamWriter3.Write(" " + array17[j]);
                                            streamWriter3.WriteLine(" " + array19[j]);
                                        }
                                        if (array18[j] >= 0 && array20[j] >= 0)
                                        {
                                            streamWriter3.Write(j);
                                            streamWriter3.Write(" " + array18[j]);
                                            streamWriter3.WriteLine(" " + array20[j]);
                                        }
                                    }
                                    streamWriter3.Close();
                                    fileStream.Seek(48 * num25 + 4 * (num26 + num27 + num28), SeekOrigin.Current);
                                }
                            }
                            fileStream.Seek(position + num22, SeekOrigin.Begin);
                        }
                    }
                    if (num9 > 0)
                    {
                        fileStream.Seek(num9 + 4, SeekOrigin.Begin);
                        int num30 = binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                        int num31 = binaryReader.ReadInt32();
                        if (num31 == 327681)
                        {
                            binaryReader.ReadInt32();
                            int num32 = binaryReader.ReadInt32();
                            array7 = new Vector3[num32][];
                            array9 = new int[num32];
                            for (int i = 0; i < num32; i++)
                            {
                                array9[i] = (binaryReader.ReadInt32() & int.MaxValue);
                                int num33 = binaryReader.ReadInt32();
                                int num34 = binaryReader.ReadInt32();
                                int num35 = binaryReader.ReadInt32();
                                fileStream.Seek(84L, SeekOrigin.Current);
                                if (num30 >= 808464689)
                                {
                                    fileStream.Seek(16L, SeekOrigin.Current);
                                }
                                array7[i] = new Vector3[num33];
                                for (int j = 0; j < num33; j++)
                                {
                                    array7[i][j] = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                                    binaryReader.ReadSingle();
                                }
                                Vector3[] array21 = new Vector3[num33];
                                int[] array22 = new int[num33];
                                int[] array23 = new int[num33];
                                int[] array24 = new int[num33];
                                int[] array25 = new int[num33];
                                int num36 = 0;
                                for (int j = 0; j < num33; j++)
                                {
                                    array24[j] = binaryReader.ReadInt32();
                                    array25[j] = binaryReader.ReadInt32();
                                    array22[j] = binaryReader.ReadInt32();
                                    array23[j] = binaryReader.ReadInt32();
                                    binaryReader.ReadSingle();
                                    binaryReader.ReadSingle();
                                    if (array22[j] >= 0 && array24[j] >= 0)
                                    {
                                        num36++;
                                    }
                                    if (array23[j] >= 0 && array25[j] >= 0)
                                    {
                                        num36++;
                                    }
                                    if (array9[i] >= 0 && array9[i] < array2.Length)
                                    {
                                        Quaternion quaternion3D2 = array2[array9[i]];
                                        Quaternion right = new Quaternion(array7[i][j], 0f);
                                        Quaternion left = quaternion3D2 * right;
                                        Quaternion quaternion3D = left * new Quaternion(quaternion3D2.W, 0f - quaternion3D2.X, 0f - quaternion3D2.Y, 0f - quaternion3D2.Z);
                                        array21[j] = quaternion3D.Xyz + array[array9[i]];
                                    }
                                    else
                                    {
                                        array21[j] = array7[i][j];
                                    }
                                }
                                StreamWriter streamWriter4 = new StreamWriter(args[0] + "_vdriver" + i + ".ascii");
                                streamWriter4.WriteLine(num33);
                                for (int j = 0; j < num33; j++)
                                {
                                    streamWriter4.WriteLine("drvv_" + i + "_bone_" + j);
                                    streamWriter4.WriteLine(array22[j]);
                                    streamWriter4.Write(array21[j].X.ToString("0.######", numberFormatInfo));
                                    streamWriter4.Write(" " + array21[j].Y.ToString("0.######", numberFormatInfo));
                                    streamWriter4.WriteLine(" " + array21[j].Z.ToString("0.######", numberFormatInfo));
                                }
                                streamWriter4.WriteLine(1);
                                streamWriter4.WriteLine("driver_" + i);
                                streamWriter4.WriteLine(0);
                                streamWriter4.WriteLine(0);
                                streamWriter4.WriteLine(num33);
                                for (int j = 0; j < num33; j++)
                                {
                                    streamWriter4.Write(array21[j].X.ToString("0.######", numberFormatInfo));
                                    streamWriter4.Write(" " + array21[j].Y.ToString("0.######", numberFormatInfo));
                                    streamWriter4.WriteLine(" " + array21[j].Z.ToString("0.######", numberFormatInfo));
                                    streamWriter4.WriteLine("0 0 0");
                                    streamWriter4.WriteLine("0 0 0 0");
                                    streamWriter4.WriteLine(j + " 0 0 0");
                                    streamWriter4.WriteLine("1 0 0 0");
                                }
                                streamWriter4.WriteLine(num36);
                                for (int j = 0; j < num33; j++)
                                {
                                    if (array22[j] >= 0 && array24[j] >= 0)
                                    {
                                        streamWriter4.Write(j);
                                        streamWriter4.Write(" " + array22[j]);
                                        streamWriter4.WriteLine(" " + array24[j]);
                                    }
                                    if (array23[j] >= 0 && array25[j] >= 0)
                                    {
                                        streamWriter4.Write(j);
                                        streamWriter4.Write(" " + array23[j]);
                                        streamWriter4.WriteLine(" " + array25[j]);
                                    }
                                }
                                streamWriter4.Close();
                                fileStream.Seek(48 * num34 + 4 * num35, SeekOrigin.Current);
                            }
                        }
                    }
                    fileStream.Seek(num7 + 44, SeekOrigin.Begin);
                    num6 = binaryReader.ReadInt32();
                    long[] array26 = new long[num6];
                    for (int i = 0; i < num6; i++)
                    {
                        array26[i] = fileStream.Position;
                        binaryReader.ReadInt32();
                        int num37 = binaryReader.ReadInt32();
                        fileStream.Seek(array26[i] + num37, SeekOrigin.Begin);
                    }
                    fileStream.Seek(array26[5] + 8, SeekOrigin.Begin);
                    int num38 = binaryReader.ReadInt32();
                    int[][] array27 = new int[num38][];
                    int[][] array28 = new int[num38][];
                    int num39 = 0;
                    int num40 = 0;
                    for (int i = 0; i < num38; i++)
                    {
                        int num41 = binaryReader.ReadInt32();
                        array27[i] = new int[num41];
                        array28[i] = new int[num41];
                        for (int j = 0; j < num41; j++)
                        {
                            binaryReader.ReadInt32();
                            num40 = (binaryReader.ReadInt32() & int.MaxValue);
                            num39 = (binaryReader.ReadInt32() & int.MaxValue);
                            array27[i][j] = num39;
                            array28[i][j] = num40;
                        }
                    }
                    fileStream.Seek(array26[7] + 8, SeekOrigin.Begin);
                    int num42 = binaryReader.ReadInt32();
                    int[] array29 = new int[num42];
                    int[] array30 = new int[num42];
                    int[] array31 = new int[num42];
                    int[] array32 = new int[num42];
                    int[] array33 = new int[num42];
                    int[] array34 = new int[num42];
                    int[] array35 = new int[num42];
                    int[] array36 = new int[num42];
                    int[] array37 = new int[num42];
                    for (int i = 0; i < num42; i++)
                    {
                        array29[i] = binaryReader.ReadInt32();
                        array30[i] = binaryReader.ReadInt32();
                        array31[i] = binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                        array32[i] = binaryReader.ReadInt32();
                        fileStream.Seek(16L, SeekOrigin.Current);
                        array33[i] = binaryReader.ReadInt32();
                        array36[i] = binaryReader.ReadInt32();
                        array37[i] = binaryReader.ReadInt32();
                        array34[i] = binaryReader.ReadInt32();
                        array35[i] = binaryReader.ReadInt32();
                    }
                    fileStream.Seek(array26[8] + 8, SeekOrigin.Begin);
                    int num43 = binaryReader.ReadInt32();
                    Dictionary<int, int> dictionary = new Dictionary<int, int>();
                    new Dictionary<int, int>();
                    if (num43 > 1)
                    {
                        fileStream.Seek(12L, SeekOrigin.Current);
                        int num44 = binaryReader.ReadInt32();
                        num44 += binaryReader.ReadInt32();
                        fileStream.Seek(16L, SeekOrigin.Current);
                        for (int i = 0; i < num44; i++)
                        {
                            fileStream.Seek(16L, SeekOrigin.Current);
                            int num45 = binaryReader.ReadInt32();
                            int value = binaryReader.ReadInt32();
                            if (num45 == 1)
                            {
                                int num46 = binaryReader.ReadInt32();
                                for (int j = 0; j < num46; j++)
                                {
                                    int key = binaryReader.ReadInt32();
                                    if (!dictionary.ContainsKey(key))
                                    {
                                        dictionary.Add(key, value);
                                    }
                                }
                            }
                            else
                            {
                                fileStream.Seek(binaryReader.ReadInt32() * 4, SeekOrigin.Current);
                            }
                        }
                    }
                    fileStream.Seek(array26[4] + 8, SeekOrigin.Begin);
                    int num47 = binaryReader.ReadInt32();
                    long[] array38 = new long[num47];
                    for (int i = 0; i < num47; i++)
                    {
                        array38[i] = fileStream.Position;
                        fileStream.Seek(binaryReader.ReadInt32() * 4, SeekOrigin.Current);
                        int num48 = binaryReader.ReadInt32();
                        fileStream.Seek(num48 * 8, SeekOrigin.Current);
                    }
                    fileStream.Seek(array26[3] + 8, SeekOrigin.Begin);
                    int num49 = binaryReader.ReadInt32();
                    int[] array39 = new int[num49];
                    long[] array40 = new long[num49];
                    for (int i = 0; i < num49; i++)
                    {
                        binaryReader.ReadInt32();
                        array39[i] = binaryReader.ReadInt32();
                        int num50 = binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                        array40[i] = fileStream.Position;
                        fileStream.Seek(num50 * array39[i], SeekOrigin.Current);
                    }
                    fileStream.Seek(array26[6] + 8, SeekOrigin.Begin);
                    int num51 = binaryReader.ReadInt32();
                    int[] array41 = new int[num51];
                    long[] array42 = new long[num51];
                    for (int i = 0; i < num51; i++)
                    {
                        int num52 = binaryReader.ReadInt32();
                        array41[i] = binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                        array42[i] = fileStream.Position;
                        fileStream.Seek(num52 * array41[i] / 8, SeekOrigin.Current);
                        fileStream.Seek((fileStream.Position + 3) & 4294967292u, SeekOrigin.Begin);
                    }
                    streamWriter.WriteLine(num42);
                    int num53 = 0;
                    float[] weights = new float[17];
                    int[] indices = new int[16];
                    for (int i = 0; i < num42; i++)
                    {
                        Console.Write(".");
                        int num54 = array37[i];
                        Vector3[] array45 = new Vector3[num54];
                        Vector3[] array46 = new Vector3[num54];
                        float[,] array47 = new float[num54, 4];
                        float[,] array48 = new float[num54, 4];
                        int[,] array49 = new int[num54, 8];
                        float[,] array50 = new float[num54, 8];
                        int num55 = array31[i];
                        int num56 = 0;
                        int num57 = 0;
                        Vector3 vector3D2 = new Vector3();
                        int num58 = 0;
                        int num59 = 0;
                        fileStream.Seek(array38[array30[i]], SeekOrigin.Begin);
                        int num60 = binaryReader.ReadInt32();
                        int[] array51 = new int[num60];
                        for (int l = 0; l < num60; l++)
                        {
                            array51[l] = binaryReader.ReadInt32();
                        }
                        int num61 = binaryReader.ReadInt32();
                        long position2 = fileStream.Position;
                        for (int m = 0; m < num60; m++)
                        {
                            int num62 = -1;
                            int num63 = -1;
                            int[] array52 = new int[4];
                            int[] array53 = new int[4];
                            int num64 = -1;
                            int num65 = -1;
                            int num66 = -1;
                            int num67 = -1;
                            int num68 = -1;
                            int num69 = -1;
                            int num70 = -1;
                            int num71 = -1;
                            int num72 = -1;
                            int num73 = 0;
                            int num74 = 0;
                            int num75 = 0;
                            int num76 = 0;
                            int num77 = array39[array51[m]];
                            long num78 = array40[array51[m]] + array36[i] * num77;
                            fileStream.Seek(position2, SeekOrigin.Begin);
                            for (int j = 0; j < num61; j++)
                            {
                                int num79 = binaryReader.ReadInt16();
                                int num80 = binaryReader.ReadInt16();
                                int num81 = binaryReader.ReadInt16();
                                int num82 = binaryReader.ReadInt16();
                                if (num79 == m)
                                {
                                    switch (num82)
                                    {
                                        case 0:
                                            num62 = num80;
                                            break;
                                        case 3:
                                            num63 = num80;
                                            num76 = num81;
                                            break;
                                        case 2:
                                            num66 = num80;
                                            num73 = num81;
                                            num59 = 4;
                                            break;
                                        case 258:
                                            num67 = num80;
                                            num73 = num81;
                                            num59 = 8;
                                            break;
                                        case 1:
                                            num64 = num80;
                                            num74 = num81;
                                            break;
                                        case 257:
                                            num65 = num80;
                                            num75 = num81;
                                            break;
                                        case 5:
                                            array52[0] = num80;
                                            array53[0] = num81;
                                            num58++;
                                            break;
                                        case 261:
                                            array52[1] = num80;
                                            array53[1] = num81;
                                            num58++;
                                            break;
                                        case 517:
                                            array52[2] = num80;
                                            array53[2] = num81;
                                            num58++;
                                            break;
                                        case 7:
                                            num68 = num80;
                                            break;
                                        case 266:
                                            num69 = num80;
                                            break;
                                        case 4:
                                            num70 = num80;
                                            break;
                                        case 11:
                                            num71 = num80;
                                            break;
                                        case 1285:
                                            num72 = num80;
                                            break;
                                    }
                                }
                            }
                            if (dictionary.ContainsKey(i))
                            {
                                num57 = dictionary[i];
                            }
                            for (int j = 0; j < num54; j++)
                            {
                                if (dictionary.ContainsKey(i))
                                {
                                    fileStream.Seek(num78 + j * num77, SeekOrigin.Begin);
                                    for (int n = 0; n < 4; n++)
                                    {
                                        weights[n] = binaryReader.ReadSingle();
                                    }
                                    fileStream.Seek(num78 + num64 + j * num77, SeekOrigin.Begin);
                                    for (int num83 = 0; num83 < 4; num83++)
                                    {
                                        weights[num83 + 4] = binaryReader.ReadSingle();
                                    }
                                    fileStream.Seek(num78 + num68 + j * num77, SeekOrigin.Begin);
                                    for (int num84 = 0; num84 < 4; num84++)
                                    {
                                        weights[num84 + 8] = binaryReader.ReadSingle();
                                    }
                                    fileStream.Seek(num78 + num69 + j * num77, SeekOrigin.Begin);
                                    for (int num85 = 0; num85 < 4; num85++)
                                    {
                                        weights[num85 + 12] = binaryReader.ReadSingle();
                                    }
                                    fileStream.Seek(num78 + num66 + j * num77, SeekOrigin.Begin);
                                    for (int num86 = 0; num86 < 4; num86++)
                                    {
                                        indices[num86] = binaryReader.ReadByte();
                                    }
                                    fileStream.Seek(num78 + num70 + j * num77, SeekOrigin.Begin);
                                    for (int num87 = 0; num87 < 4; num87++)
                                    {
                                        indices[num87 + 4] = binaryReader.ReadByte();
                                    }
                                    fileStream.Seek(num78 + num71 + j * num77, SeekOrigin.Begin);
                                    for (int num88 = 0; num88 < 4; num88++)
                                    {
                                        indices[num88 + 8] = binaryReader.ReadByte();
                                    }
                                    fileStream.Seek(num78 + num72 + j * num77, SeekOrigin.Begin);
                                    for (int num89 = 0; num89 < 4; num89++)
                                    {
                                        indices[num89 + 12] = binaryReader.ReadByte();
                                    }
                                    fileStream.Seek(num78 + num63 + j * num77, SeekOrigin.Begin);
                                    array46[j] = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                                    weights[16] = binaryReader.ReadSingle();
                                    for (int num90 = 0; num90 < num58; num90++) // uvs
                                    {
                                        fileStream.Seek(num78 + array52[num90] + j * num77, SeekOrigin.Begin);
                                        if (array53[num90] == 1)
                                        {
                                            num4 = binaryReader.ReadSingle();
                                            num5 = binaryReader.ReadSingle();
                                        }
                                        else if (array53[num90] == 10)
                                        {
                                            num4 = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            num5 = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Unknown UV format " + array53[num90]);
                                        }
                                        array47[j, num90] = num4;
                                        array48[j, num90] = num5;
                                    }
                                    Vector3 vector3D3 = Vector3.Zero;
                                    if (indices[4] + indices[5] + indices[6] + indices[7] == 0)
                                    {
                                        vector3D3 = new Vector3(weights[0], weights[1], weights[2]);
                                        array49[j, 0] = indices[0];
                                        array49[j, 1] = indices[1];
                                        array49[j, 2] = indices[2];
                                        array49[j, 3] = indices[3];
                                        array50[j, 0] = weights[4];
                                        array50[j, 1] = weights[5];
                                        array50[j, 2] = weights[6];
                                        array50[j, 3] = weights[7];
                                    }
                                    else
                                    {
                                        array49[j, 0] = 0; // bone id
                                        array50[j, 0] = 1f; // weight

                                        Vector3[] array54 = new Vector3[4];
                                        for (int num91 = 0; num91 < 4; num91++)
                                        {
                                            array54[num91] = new Vector3();
                                            for (int num92 = 0; num92 < 4; num92++)
                                            {
                                                {
                                                    Vector3[] array14;
                                                    Vector3[] array56 = array14 = array54;
                                                    int num94 = num91;
                                                    IntPtr intPtr = (IntPtr)num94;
                                                    array56[num94] = array14[(long)intPtr] + array7[num57 - 10000][indices[num91 * 4 + num92]] * weights[num92];
                                                }
                                            }
                                        }

                                        Vector3[] array57 = new Vector3[4];
                                        for (int num91 = 0; num91 < 4; num91++)
                                        {
                                            array57[num91] = new Vector3();
                                            for (int num92 = 0; num92 < 4; num92++)
                                            {
                                                {
                                                    Vector3[] array14;
                                                    Vector3[] array59 = array14 = array57;
                                                    int num96 = num91;
                                                    IntPtr intPtr = (IntPtr)num96;
                                                    array59[num96] = array14[(long)intPtr] + array7[num57 - 10000][indices[num91 * 4 + num92]] * weights[8 + num92];
                                                }
                                            }
                                        }

                                        Vector3 vector3D4 = new Vector3();
                                        for (int num92 = 0; num92 < 4; num92++)
                                        {
                                            vector3D4 += array54[num92] * weights[4 + num92];
                                        }

                                        Vector3 vector3D5 = new Vector3();
                                        for (int num92 = 0; num92 < 4; num92++)
                                        {
                                            vector3D5 += array54[num92] * weights[12 + num92];
                                        }

                                        Vector3 vector3D6 = new Vector3();
                                        for (int num92 = 0; num92 < 4; num92++)
                                        {
                                            vector3D6 += array57[num92] * weights[4 + num92];
                                        }

                                        vector3D3 = Vector3.Cross(vector3D5, vector3D6) * weights[16] + vector3D4;

                                        Console.WriteLine(string.Join(", ", array7[0]));
                                    }
                                    array45[j] = vector3D3;
                                    continue;
                                }
                                if (num62 >= 0)
                                {
                                    fileStream.Seek(num78 + j * num77, SeekOrigin.Begin);
                                    array45[j] = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                                    array46[j] = new Vector3(0f, 0f, 0f);
                                }
                                if (num63 >= 0)
                                {
                                    fileStream.Seek(num78 + num63 + j * num77, SeekOrigin.Begin);
                                    switch (num76)
                                    {
                                        case 2:
                                            num4 = binaryReader.ReadSingle();
                                            num5 = binaryReader.ReadSingle();
                                            zz = binaryReader.ReadSingle();
                                            break;
                                        case 11:
                                            num4 = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            num5 = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            zz = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            break;
                                        default:
                                            Console.WriteLine("Unknown normals format " + num76);
                                            break;
                                        case 3:
                                            break;
                                    }
                                    array46[j] = new Vector3(num4, num5, zz);
                                }
                                if (num64 >= 0)
                                {
                                    fileStream.Seek(num78 + num64 + j * num77, SeekOrigin.Begin);
                                    switch (num74)
                                    {
                                        case 0:
                                            array50[j, 0] = binaryReader.ReadSingle();
                                            array50[j, 1] = 1f - array50[j, 0];
                                            break;
                                        case 1:
                                            array50[j, 0] = binaryReader.ReadSingle();
                                            array50[j, 1] = binaryReader.ReadSingle();
                                            array50[j, 2] = 1f - array50[j, 0] - array50[j, 1];
                                            break;
                                        case 2:
                                            array50[j, 0] = binaryReader.ReadSingle();
                                            array50[j, 1] = binaryReader.ReadSingle();
                                            array50[j, 2] = binaryReader.ReadSingle();
                                            array50[j, 3] = 1f - array50[j, 0] - array50[j, 1] - array50[j, 2];
                                            break;
                                        case 3:
                                            array50[j, 0] = binaryReader.ReadSingle();
                                            array50[j, 1] = binaryReader.ReadSingle();
                                            array50[j, 2] = binaryReader.ReadSingle();
                                            array50[j, 3] = binaryReader.ReadSingle();
                                            array50[j, 4] = 1f - array50[j, 0] - array50[j, 1] - array50[j, 2] - array50[j, 3];
                                            break;
                                        case 10:
                                            array50[j, 0] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 1] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 2] = 1f - array50[j, 0] - array50[j, 1];
                                            break;
                                        case 11:
                                            array50[j, 0] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 1] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 2] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 3] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 4] = 1f - array50[j, 0] - array50[j, 1] - array50[j, 2] - array50[j, 3];
                                            break;
                                        default:
                                            Console.WriteLine("Unknown weights format " + num74);
                                            break;
                                    }
                                }
                                if (num65 >= 0)
                                {
                                    fileStream.Seek(num78 + num65 + j * num77, SeekOrigin.Begin);
                                    switch (num75)
                                    {
                                        case 10:
                                            array50[j, 4] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 5] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 6] = 1f - array50[j, 0] - array50[j, 1] - array50[j, 2] - array50[j, 3] - array50[j, 4] - array50[j, 5];
                                            break;
                                        case 11:
                                            array50[j, 4] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 5] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 6] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            array50[j, 7] = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                            break;
                                        default:
                                            Console.WriteLine("Unknown weights format " + num75);
                                            break;
                                    }
                                }
                                if (num66 >= 0)
                                {
                                    fileStream.Seek(num78 + num66 + j * num77, SeekOrigin.Begin);
                                    switch (num73)
                                    {
                                        case 7:
                                            array49[j, 0] = binaryReader.ReadInt16();
                                            array49[j, 1] = binaryReader.ReadInt16();
                                            array49[j, 2] = binaryReader.ReadInt16();
                                            array49[j, 3] = binaryReader.ReadInt16();
                                            break;
                                        case 5:
                                            array49[j, 0] = binaryReader.ReadByte();
                                            array49[j, 1] = binaryReader.ReadByte();
                                            array49[j, 2] = binaryReader.ReadByte();
                                            array49[j, 3] = binaryReader.ReadByte();
                                            break;
                                        default:
                                            Console.WriteLine("Unknown bone id format " + num73);
                                            break;
                                    }
                                    if (num64 < 0)
                                    {
                                        array50[j, 0] = 1f;
                                    }
                                }
                                if (num67 >= 0)
                                {
                                    fileStream.Seek(num78 + num67 + j * num77, SeekOrigin.Begin);
                                    switch (num73)
                                    {
                                        case 7:
                                            array49[j, 4] = binaryReader.ReadInt16();
                                            array49[j, 5] = binaryReader.ReadInt16();
                                            array49[j, 6] = binaryReader.ReadInt16();
                                            array49[j, 7] = binaryReader.ReadInt16();
                                            break;
                                        case 5:
                                            array49[j, 4] = binaryReader.ReadByte();
                                            array49[j, 5] = binaryReader.ReadByte();
                                            array49[j, 6] = binaryReader.ReadByte();
                                            array49[j, 7] = binaryReader.ReadByte();
                                            break;
                                        default:
                                            Console.WriteLine("Unknown bone id format " + num73);
                                            break;
                                    }
                                }
                                for (int num97 = 0; num97 < num58; num97++)
                                {
                                    fileStream.Seek(num78 + array52[num97] + j * num77, SeekOrigin.Begin);
                                    if (array53[num97] == 1)
                                    {
                                        num4 = binaryReader.ReadSingle();
                                        num5 = binaryReader.ReadSingle();
                                    }
                                    else if (array53[num97] == 10)
                                    {
                                        num4 = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                        num5 = Half.FromBytes(binaryReader.ReadBytes(2), 0);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Unknown UV format " + array53[num97]);
                                    }
                                    array47[j, num97] = num4;
                                    array48[j, num97] = num5;
                                }
                            }
                        }
                        streamWriter.WriteLine("sm_" + num53++);
                        streamWriter.WriteLine(num58);
                        streamWriter.WriteLine(0);
                        streamWriter.WriteLine(num54);
                        for (int j = 0; j < num54; j++)
                        {
                            num56 = array28[num55][array49[j, 0] / 3];
                            if (num56 != 0 && num56 < array2.Length)
                            {
                                Quaternion quaternion3D2 = array2[num56]; // bone at some position
                                vector3D2 = array[num56]; // ??
                                Quaternion right = new Quaternion(array45[j], 0f); // position
                                Quaternion left = quaternion3D2 * right;
                                Quaternion quaternion3D = left * new Quaternion(quaternion3D2.W, 0f - quaternion3D2.X, 0f - quaternion3D2.Y, 0f - quaternion3D2.Z);
                                array45[j] = quaternion3D.Xyz + vector3D2;
                            }
                            streamWriter.Write(array45[j].X.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array45[j].Y.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array45[j].Z.ToString("0.######", numberFormatInfo));
                            streamWriter.WriteLine();
                            streamWriter.Write(array46[j].X.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array46[j].Y.ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array46[j].Z.ToString("0.######", numberFormatInfo));
                            streamWriter.WriteLine();
                            streamWriter.WriteLine("0 0 0 0");
                            for (int num99 = 0; num99 < num58; num99++)
                            {
                                streamWriter.WriteLine(array47[j, num99].ToString("0.######", numberFormatInfo) + " " + array48[j, num99].ToString("0.######", numberFormatInfo));
                            }
                            if (num59 == 0)
                            {
                                streamWriter.WriteLine("0 0 0 0");
                                streamWriter.WriteLine("1 0 0 0");
                                continue;
                            }
                            streamWriter.Write(array27[num55][array49[j, 0] / 3]);
                            streamWriter.Write(" " + array27[num55][array49[j, 1] / 3]);
                            streamWriter.Write(" " + array27[num55][array49[j, 2] / 3]);
                            streamWriter.Write(" " + array27[num55][array49[j, 3] / 3]);
                            if (num59 > 4)
                            {
                                streamWriter.Write(" " + array27[num55][array49[j, 4] / 3]);
                                streamWriter.Write(" " + array27[num55][array49[j, 5] / 3]);
                                streamWriter.Write(" " + array27[num55][array49[j, 6] / 3]);
                                streamWriter.WriteLine(" " + array27[num55][array49[j, 7] / 3]);
                            }
                            else
                            {
                                streamWriter.WriteLine();
                            }
                            float num100 = array50[j, 3] + array50[j, 0] + array50[j, 1] + array50[j, 2] + array50[j, 4] + array50[j, 5] + array50[j, 6] + array50[j, 7];
                            if ((double)num100 < 0.99 || (double)num100 > 1.01)
                            {
                                Console.WriteLine("Lost weight " + num100);
                            }
                            streamWriter.Write(array50[j, 0].ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array50[j, 1].ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array50[j, 2].ToString("0.######", numberFormatInfo));
                            streamWriter.Write(" " + array50[j, 3].ToString("0.######", numberFormatInfo));
                            if (num59 > 4)
                            {
                                streamWriter.Write(" " + array50[j, 4].ToString("0.######", numberFormatInfo));
                                streamWriter.Write(" " + array50[j, 5].ToString("0.######", numberFormatInfo));
                                streamWriter.Write(" " + array50[j, 6].ToString("0.######", numberFormatInfo));
                                streamWriter.WriteLine(" " + array50[j, 7].ToString("0.######", numberFormatInfo));
                            }
                            else
                            {
                                streamWriter.WriteLine();
                            }
                        }
                        int num101 = array35[i];
                        int num102 = array36[i];
                        fileStream.Seek(array42[array30[i]] + array34[i] * array41[array30[i]] / 8, SeekOrigin.Begin);
                        if (array33[i] == 4)
                        {
                            bool flag = true;
                            int num103 = 0;
                            int num104 = 0;
                            int num105 = 0;
                            for (int j = 0; j < num101; j++)
                            {
                                int num106 = num103;
                                num103 = num104;
                                num104 = binaryReader.ReadInt16();
                                if (j > 1 && num106 != num103 && num103 != num104 && num104 != num106)
                                {
                                    num105++;
                                }
                            }
                            streamWriter.WriteLine(num105);
                            fileStream.Seek(array42[array30[i]] + array34[i] * array41[array30[i]] / 8, SeekOrigin.Begin);
                            for (int j = 0; j < num101; j++)
                            {
                                flag = !flag;
                                int num106 = num103;
                                num103 = num104;
                                num104 = binaryReader.ReadInt16();
                                if (j > 1 && num106 != num103 && num103 != num104 && num104 != num106)
                                {
                                    if (flag)
                                    {
                                        streamWriter.WriteLine(num106 - num102 + " " + (num103 - num102) + " " + (num104 - num102));
                                    }
                                    else
                                    {
                                        streamWriter.WriteLine(num104 - num102 + " " + (num103 - num102) + " " + (num106 - num102));
                                    }
                                }
                            }
                            continue;
                        }
                        streamWriter.WriteLine(num101 / 3);
                        for (int j = 0; j < num101 / 3; j++)
                        {
                            int num107;
                            int num108;
                            int num109;
                            if (array41[array30[i]] == 32)
                            {
                                num107 = binaryReader.ReadInt32() - num102;
                                num108 = binaryReader.ReadInt32() - num102;
                                num109 = binaryReader.ReadInt32() - num102;
                            }
                            else
                            {
                                num107 = binaryReader.ReadUInt16() - num102;
                                num108 = binaryReader.ReadUInt16() - num102;
                                num109 = binaryReader.ReadUInt16() - num102;
                            }
                            streamWriter.WriteLine(num109 + " " + num108 + " " + num107);
                        }
                    }
                    streamWriter2.Close();
                    streamWriter.Close();
                    break;
                }
        }
    }
}