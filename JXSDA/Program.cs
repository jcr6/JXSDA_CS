using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickyUnits {
    static class Program {

        const string PackFile = "E:/Projects/Applications/VisualStudio/JXSDA/JXSDA/JXSDA.cs";
        static readonly byte[] startbuf;

        static Program() {
            QCol.Doing("Reading", PackFile);
            startbuf = QuickStream.GetFile(PackFile);
        }

        static void Main(string[] args) {
            JXSDA.Verbose = true;
            var packed = JXSDA.Pack(startbuf);
            Console.WriteLine($"Packed attempt done. {startbuf.Length} => {packed.Length} => ({(int)Math.Floor(((double)packed.Length/startbuf.Length)*100)}%)");
            TrickyDebug.AttachWait();
        }
    }
}
