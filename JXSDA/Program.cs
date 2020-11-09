// Lic:
// ***********************************************************
// JXSDA/Program.cs
// This particular file has been released in the public domain
// and is therefore free of any restriction. You are allowed
// to credit me as the original author, but this is not
// required.
// This file was setup/modified in:
// 2020
// If the law of your country does not support the concept
// of a product being released in the public domain, while
// the original author is still alive, or if his death was
// not longer than 70 years ago, you can deem this file
// "(c) Jeroen Broks - licensed under the CC0 License",
// with basically comes down to the same lack of
// restriction the public domain offers. (YAY!)
// ***********************************************************
// Version 20.11.10
// EndLic

// This program was only written to test if the algorithm works!

using System;
using System.Text;

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
            var unpacked = JXSDA.Unpack(packed);
            Console.WriteLine($"Packed attempt done. {startbuf.Length} => {packed.Length} => ({(int)Math.Floor(((double)packed.Length/startbuf.Length)*100)}%)");
            if (unpacked != null) {
                Console.WriteLine($"Unpacking attempt done. {packed.Length}=>{unpacked.Length} (Size check succes: {unpacked.Length==startbuf.Length})");
                var s = Encoding.UTF8.GetString(unpacked, 0, unpacked.Length);
                Console.WriteLine($"<unpacked>\n{s}\n</unpacked>");
            } else {
                QCol.QuickError("Something went wrong in unpacking");
            }
            TrickyDebug.AttachWait();
        }
    }
}