using System;

namespace Metanoia.Formats.Misc
{
    /// <summary>
    /// 
    /// </summary>
    public class G1M
    {
        public G1MS Skeleton { get; set; }

        public G1MM G1MM { get; set; } // matrices

        public G1MG G1MG { get; set; } // mesh groups?

        public NUNO NUNO { get; set; }

        public NUNV NUNV { get; set; }

        public G1M(DataReader r)
        {
            var DataStart = r.ReadUInt32();
            r.ReadInt32(); //0
            var someCount = r.ReadInt32();

            r.Seek(DataStart);
            while (r.Position < r.Length)
            {
                var start = r.Position;
                string flag = new string(r.ReadChars(8));
                if (r.Position + 4 >= r.Length)
                    break;
                var size = r.ReadUInt32();
                var sectionEnd = r.Position + size - 12;

                //File.WriteAllBytes("Warriors\\Claude\\" + flag, r.GetSection(start, (int)sectionEnd));

                Console.WriteLine(flag + " " + sectionEnd.ToString("X8"));

                switch (flag.Substring(0, 4))
                {
                    case "SM1G":
                        Skeleton = new G1MS(r);
                        break;
                    case "G1MM":
                        G1MM = new G1MM(r);
                        break;
                    case "GM1G":
                        G1MG = new G1MG(r);
                        break;
                    case "ONUN":
                        NUNO = new NUNO(r);
                        break;
                    case "VNUN":
                        NUNV = new NUNV(r);
                        break;
                }

                r.Seek(sectionEnd);
            }
        }
    }

}
